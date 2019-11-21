import java.sql.ResultSet;

class SqliteDao {
    private String fullDbQuery;

    SqliteDao(String dbName){

    }

    ResultSet getData(byte[] data) {
        return null;
    }

    void setFullDbQuery(String fullDbQuery){
        this.fullDbQuery = fullDbQuery;
    }
}
