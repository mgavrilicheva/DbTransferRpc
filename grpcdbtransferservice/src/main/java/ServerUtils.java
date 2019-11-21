import org.apache.commons.io.IOUtils;

import javax.crypto.BadPaddingException;
import javax.crypto.Cipher;
import javax.crypto.IllegalBlockSizeException;
import javax.crypto.NoSuchPaddingException;
import java.io.*;
import java.security.*;
import java.util.zip.GZIPInputStream;

final class ServerUtils {
    static byte[] decompressData(byte[] compressedData) throws IOException {
        try (ByteArrayInputStream is = new ByteArrayInputStream(compressedData))
        {
            try (GZIPInputStream gis = new GZIPInputStream(is, 1024))
            {
                return IOUtils.toByteArray(gis);
            }
        }
    }

    static byte[] decryptData(byte[] encryptedData, PrivateKey privKey)
            throws NoSuchPaddingException, NoSuchAlgorithmException, InvalidKeyException,
            BadPaddingException, IllegalBlockSizeException {
        Cipher cipher = Cipher.getInstance("RSA/ECB/PKCS1Padding");
        cipher.init(Cipher.DECRYPT_MODE, privKey);
        return cipher.doFinal(encryptedData);
    }

    static void fillSqliteDb(String filename, byte[] data) throws IOException {
        File dbFile = new File(filename);
        boolean success = dbFile.createNewFile();
        try (FileOutputStream dbFileStream = new FileOutputStream(dbFile, false)) {
            dbFileStream.getChannel().truncate(0);
            dbFileStream.write(data);
        }
    }

    static KeyPair generateKeyPair() throws NoSuchAlgorithmException {
        KeyPairGenerator keyGenerator = KeyPairGenerator.getInstance("RSA");
        keyGenerator.initialize(4096);
        return keyGenerator.generateKeyPair();
    }

    static String getQuery(String query) throws IOException {
        return IOUtils.toString(
                ServerUtils.class.getResourceAsStream(String.format("/db_queries/%s.sql", query)), "UTF-8");
    }
}
