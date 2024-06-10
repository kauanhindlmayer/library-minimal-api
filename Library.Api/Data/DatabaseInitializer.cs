using Dapper;

namespace Library.Api.Data;

public class DatabaseInitializer(IDbConnectionFactory connectionFactory)
{
    public async Task InitializeAsync()
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        await connection.ExecuteAsync("""
                                      CREATE TABLE IF NOT EXISTS Books (
                                          Isbn TEXT PRIMARY KEY,
                                          Title TEXT NOT NULL,
                                          Author TEXT NOT NULL,
                                          ShortDescription TEXT NOT NULL,
                                          PageCount INTEGER NOT NULL,
                                          ReleaseDate TEXT NOT NULL
                                      );
                                      """);
    }
}