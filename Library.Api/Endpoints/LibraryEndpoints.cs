using FluentValidation;
using FluentValidation.Results;
using Library.Api.Endpoints.Internal;
using Library.Api.Models;
using Library.Api.Services;

namespace Library.Api.Endpoints;

public class LibraryEndpoints : IEndpoints
{
    public static void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("books",
                async (Book book, IBookService bookService, IValidator<Book> validator) =>
                {
                    var validationResult = await validator.ValidateAsync(book);
                    if (!validationResult.IsValid)
                    {
                        return Results.BadRequest(validationResult.Errors);
                    }

                    var created = await bookService.CreateAsync(book);
                    if (!created)
                    {
                        return Results.BadRequest(new List<ValidationFailure>
                        {
                            new("Isbn", "A book with the same ISBN already exists")
                        });
                    }

                    return Results.CreatedAtRoute("GetBook", new { isbn = book.Isbn }, book);
                })
            .WithName("CreateBook")
            .Accepts<Book>("application/json")
            .Produces<Book>(201)
            .Produces<IEnumerable<ValidationFailure>>(400)
            .WithTags("Books");

        app.MapGet("books", async (IBookService bookService, string? searchTerm) =>
            {
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    var matchedBooks = await bookService.SearchByTitleAsync(searchTerm);
                    return Results.Ok(matchedBooks);
                }

                var books = await bookService.GetAllAsync();
                return Results.Ok(books);
            })
            .WithName("GetBooks")
            .Produces<IEnumerable<Book>>()
            .WithTags("Books");


        app.MapGet("books/{isbn}", async (string isbn, IBookService bookService) =>
            {
                var book = await bookService.GetByIsbnAsync(isbn);
                return book is not null ? Results.Ok(book) : Results.NotFound();
            })
            .WithName("GetBook")
            .Produces<Book>()
            .Produces(404)
            .WithTags("Books");

        app.MapPut("books/{isbn}",
                async (string isbn, Book book, IBookService bookService, IValidator<Book> validator) =>
                {
                    book.Isbn = isbn;
                    var validationResult = await validator.ValidateAsync(book);
                    if (!validationResult.IsValid)
                    {
                        return Results.BadRequest(validationResult.Errors);
                    }

                    var updated = await bookService.UpdateAsync(book);
                    return updated ? Results.Ok(book) : Results.NotFound();
                })
            .WithName("UpdateBook")
            .Accepts<Book>("application/json")
            .Produces<Book>(201)
            .Produces<IEnumerable<ValidationFailure>>(400)
            .WithTags("Books");

        app.MapDelete("books/{isbn}", async (string isbn, IBookService bookService) =>
            {
                var deleted = await bookService.DeleteAsync(isbn);
                return deleted ? Results.NoContent() : Results.NotFound();
            })
            .WithName("DeleteBook")
            .Produces(204)
            .Produces(404)
            .WithTags("Books");
    }

    public static void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IBookService, BookService>();
    }
}