import com.google.protobuf.ByteString;
import com.google.protobuf.Empty;
import dbtransferservice.contract.DataParams;
import dbtransferservice.contract.DataResponse;
import dbtransferservice.contract.DbTransferServiceGrpc;
import dbtransferservice.contract.Token;
import io.grpc.Server;
import io.grpc.ServerBuilder;
import io.grpc.stub.StreamObserver;

import javax.crypto.BadPaddingException;
import javax.crypto.IllegalBlockSizeException;
import javax.crypto.NoSuchPaddingException;
import java.io.IOException;
import java.io.InputStream;
import java.security.InvalidKeyException;
import java.security.KeyPair;
import java.security.NoSuchAlgorithmException;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.util.*;
import java.util.logging.Logger;

public class DbTransferServer {
    private static final Logger logger = Logger.getLogger(DbTransferServer.class.getName());

    private Server server;

    private void start() throws IOException {
        Properties props = new Properties();
        try (InputStream inputStream = getClass().getResourceAsStream("/settings.properties")){
            props.load(inputStream);
        }
        catch (IOException e) {
            e.printStackTrace();
            return;
        }

        int port = Integer.parseInt(props.getProperty("port"));

        KeyPair keyPair;
        try {
            keyPair = ServerUtils.generateKeyPair();
        } catch (NoSuchAlgorithmException e) {
            e.printStackTrace();
            return;
        }

        DbDao dao = new DbDao(
                props.getProperty("pgHost"),
                Integer.parseInt(props.getProperty("pgPort")),
                props.getProperty("pgUser"),
                props.getProperty("pgPassword"),
                props.getProperty("database")
        );
        try {
            dao.initialise(ServerUtils.getQuery("initialise"));
            dao.clear();
        } catch (SQLException e) {
            e.printStackTrace();
            return;
        }

        SqliteDao sqlite = new SqliteDao("temp.db");
        sqlite.setFullDbQuery(ServerUtils.getQuery("getSqliteData"));

        server = ServerBuilder.forPort(port)
                .addService(new DbTransferServiceImpl(keyPair, dao, sqlite))
                .build()
                .start();

        logger.info("Server started, listening on " + port);
        Runtime.getRuntime().addShutdownHook(new Thread(() -> {
            System.err.println("*** shutting down gRPC server since JVM is shutting down");
            DbTransferServer.this.stop();
            System.err.println("*** server shut down");
        }));
    }

    private void stop() {
        if (server != null) {
            server.shutdown();
        }
    }

    public static void main(String[] args) throws IOException {
        final DbTransferServer server = new DbTransferServer();
        server.start();
        System.out.println("Press any key to shutdown server.");
        System.in.read();
    }

    private static class DbTransferServiceImpl extends DbTransferServiceGrpc.DbTransferServiceImplBase {
        private final KeyPair keyPair;
        private final DbDao dao;
        private final SqliteDao sqlite;

        DbTransferServiceImpl(KeyPair keyPair, DbDao dao, SqliteDao sqlite) {
            this.keyPair = keyPair;
            this.dao = dao;
            this.sqlite = sqlite;
        }

        @Override
        public void getToken(Empty request, StreamObserver<Token> responseObserver) {
            System.out.println("Someone requested a token");
            responseObserver.onNext(
                Token.newBuilder()
                .setPublicKey(ByteString.copyFrom(keyPair.getPublic().getEncoded()))
                .build());
        }

        @Override
        public void acceptData(DataParams request, StreamObserver<DataResponse> responseObserver) {
            boolean status = false;
            String message;
            System.out.println("Someone sent data");
            try {
                byte[] decryptedData = ServerUtils.decryptData(request.getData().toByteArray(), keyPair.getPrivate());
                byte[] decompressedData = ServerUtils.decompressData(decryptedData);
                ResultSet data;
                synchronized (sqlite){
                    ServerUtils.fillSqliteDb("temp.db", decompressedData);
                    data = sqlite.getData();
                    dao.acceptData(data);
                    sqlite.closeConnection();
                }
                status = true;
                message = "Success";
            }
            catch (NoSuchPaddingException | NoSuchAlgorithmException | InvalidKeyException |
                    BadPaddingException | IllegalBlockSizeException e){
                e.printStackTrace();
                message = "Decryption failure";
            }
            catch (IOException e) {
                e.printStackTrace();
                message = "Decompression failure";
            }
            catch (SQLException e) {
                e.printStackTrace();
                try {
                    sqlite.closeConnection();
                } catch (SQLException ex) {
                    ex.printStackTrace();
                }
                message = "Database error";
            }
            responseObserver.onNext(
                    DataResponse
                    .newBuilder()
                    .setStatus(status)
                    .setMessage(message)
                    .build());
        }
    }
}
