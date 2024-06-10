using Dapper;
using Library.Api.Data;
using Library.Api.Models;

namespace Library.Api.Services;

public class BookService(IDbConnectionFactory dbConnectionFactory) : IBookService
{
    public async Task<bool> CreateAsync(Book book)
    {
        // var existingBook = await GetByIsbnAsync(book.Isbn);
        // if (existingBook is not null)
        // {
        //     return false;
        // }

        using var connection = await dbConnectionFactory.CreateConnectionAsync();
        var result = await connection.ExecuteAsync("""
                                      INSERT INTO Books (Isbn, Title, Author, ShortDescription, PageCount, ReleaseDate)
                                      VALUES (@Isbn, @Title, @Author, @ShortDescription, @PageCount, @ReleaseDate);
                                      """, book);
        return result > 0;
    }

    public Task<Book?> GetByIsbnAsync(string isbn)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Book>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Book>> SearchByTitleAsync(string searchTerm)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateAsync(Book book)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAsync(string isbn)
    {
        throw new NotImplementedException();
    }
}