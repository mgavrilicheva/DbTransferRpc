import com.zaxxer.hikari.HikariDataSource;

import java.sql.Connection;
import java.sql.ResultSet;
import java.sql.SQLException;

class SqliteDao {
    private Connection connection;
    private final HikariDataSource dataSource;
    private String fullDbQuery;

    SqliteDao(String fileName){
        dataSource = new HikariDataSource();
        dataSource.setJdbcUrl(String.format("jdbc:sqlite:%s", fileName));
    }

    void closeConnection() throws SQLException {
        if (connection == null)
            return;
        connection.close();
        connection = null;
    }

    ResultSet getData() throws SQLException {
        Connection connection = dataSource.getConnection();
        return connection.createStatement().executeQuery(fullDbQuery);
    }

    void setFullDbQuery(String fullDbQuery){
        this.fullDbQuery = fullDbQuery;
    }
}
