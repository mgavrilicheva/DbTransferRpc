import com.zaxxer.hikari.HikariDataSource;

import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Collections;
import java.util.List;

class DbDao {
    private static List<String> idColumns = Arrays.asList(
            "macrofamily_id", "family_id", "language_id", "writing_type_id", "writing_id", "alphabet_id",
            "lang_info_rec_id", "country_id", "language_by_countries_rec_id"
    );

    private static List<String> tables = Arrays.asList(
            "macrofamilies", "families", "languages", "writing_types", "writings", "alphabets",
            "language_info","countries", "languages_by_countries_info"
    );

    private static List<List<String>> bindings = Arrays.asList(
            Arrays.asList("macrofamily_id", "macrofamily_name"),
            Arrays.asList("family_id", "family_name", "macrofamily_id"),
            Arrays.asList("language_id", "language_name", "family_id"),
            Arrays.asList("writing_type_id", "writing_type_name"),
            Arrays.asList("writing_id", "writing_name", "writing_symobls_count", "writing_type_id"),
            Arrays.asList("alphabet_id", "alphabet_name", "alphabet_count_of_letters"),
            Arrays.asList("lang_info_rec_id", "language_id", "writing_id", "alphabet_id"),
            Arrays.asList("country_id", "country_name", "country_capital"),
            Arrays.asList("language_by_countries_rec_id", "lang_info_rec_id", "country_id", "is_state_language")
    );

    private static final List<List<String>> fields = Arrays.asList(
            Arrays.asList("id", "name"),
            Arrays.asList("id", "name", "macrofamily_id"),
            Arrays.asList("id", "name", "family_id"),
            Arrays.asList("id", "name"),
            Arrays.asList("id", "name", "symbols_count", "type_id"),
            Arrays.asList("id", "name", "count_of_letters"),
            Arrays.asList("record_id", "language_id", "writing_id", "alphabet_id"),
            Arrays.asList("id", "name", "capital"),
            Arrays.asList("rec_id", "lang_info_rec_id", "country_id", "is_state_language")
    );

    private static final List<List<String>> insertParams = Arrays.asList(
            Arrays.asList("id", "name"),
            Arrays.asList("id", "name", "macrofamily_id"),
            Arrays.asList("id", "name", "family_id"),
            Arrays.asList("id", "name"),
            Arrays.asList("id", "name", "symbols_count", "type_id"),
            Arrays.asList("id", "name", "count_of_letters"),
            Arrays.asList("record_id", "language_id", "writing_id", "alphabet_id"),
            Arrays.asList("id", "name", "capital"),
            Arrays.asList("rec_id", "lang_info_rec_id", "country_id", "is_state_language")
    );

    private final HikariDataSource dataSource;

    DbDao(String host, int port, String username, String password, String database)
    {
        dataSource = new HikariDataSource();
        dataSource.setJdbcUrl(String.format("jdbc:postgresql://%s:%d/%s", host, port, database));
        dataSource.setUsername(username);
        dataSource.setPassword(password);
    }

    void acceptData(ResultSet data) throws SQLException {
        List<Long> ids = new ArrayList<>(Collections.nCopies(idColumns.size(), null));
        while (data.next())
            for (int i = 0; i < ids.size(); i++) {
                Long id = data.getLong(idColumns.get(i));
                if (data.wasNull())
                    ids.set(i, null);
                else
                {
                    if (id.equals(ids.get(i)))
                        continue;
                    ids.set(i, id);

                    String query = buildInsertQuery(tables.get(i), insertParams.get(i));

                    try (Connection connection = dataSource.getConnection()){
                        PreparedStatement statement = connection.prepareStatement(query);
                        for (int j = 0; j < insertParams.get(i).size(); j++)
                            statement.setObject(j + 1, data.getObject(bindings.get(i).get(j)));
                        statement.executeUpdate();
                    } catch (SQLException e) {
                        e.printStackTrace();
                    }
                }
            }
    }

    private String buildInsertQuery(String table, List<String> columns)
    {
        return String.format("INSERT INTO public.%s(%s) VALUES(%s) ON CONFLICT DO NOTHING;",
                table, String.join(",", columns), Collections.nCopies(columns.size(), "?"));
    }

    void clear() throws SQLException {
        try (Connection connection = dataSource.getConnection()){
            for (String table : tables)
                connection.createStatement().execute(String.format("TRUNCATE TABLE public.%s CASCADE;", table));
        }
    }

    void initialise(String initialisationQuery) throws SQLException {
        try (Connection connection = dataSource.getConnection()){
            connection.createStatement().execute(initialisationQuery);
        }
    }
}
