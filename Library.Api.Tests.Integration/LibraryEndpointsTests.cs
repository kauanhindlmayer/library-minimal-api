using System.Net;
using FluentAssertions;
using Library.Api.Models;
using Xunit;

namespace Library.Api.Tests.Integration;

public class LibraryEndpointsTests(LibraryApiFactory factory)
    : IClassFixture<LibraryApiFactory>, IAsyncLifetime
{
    private readonly List<string> _createdIsbns = [];

    [Fact]
    public async void CreateBook_CreatesBook_WhenDataIsCorrect()
    {
        // Arrange
        var httpClient = factory.CreateClient();
        var book = GenerateBook();

        // Act
        var result = await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book.Isbn);
        var createdBook = await result.Content.ReadFromJsonAsync<Book>();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Created);
        createdBook.Should().BeEquivalentTo(book);
        result.Headers.Location?.PathAndQuery.Should().Be($"/books/{book.Isbn}");
    }

    [Fact]
    public async void CreateBook_Fails_WhenIsbnIsInvalid()
    {
        // Arrange
        var httpClient = factory.CreateClient();
        var book = GenerateBook();
        book.Isbn = "Invalid ISBN";

        // Act
        var result = await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book.Isbn);
        var errors = await result.Content.ReadFromJsonAsync<IEnumerable<ValidationError>>();
        var error = errors!.Single();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.PropertyName.Should().Be("Isbn");
        error.ErrorMessage.Should().Be("Invalid ISBN format");
    }

    [Fact]
    public async void CreateBook_Fails_WhenBookExists()
    {
        // Arrange
        var httpClient = factory.CreateClient();
        var book = GenerateBook();

        // Act
        await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book.Isbn);
        var result = await httpClient.PostAsJsonAsync("/books", book);
        var errors = await result.Content.ReadFromJsonAsync<IEnumerable<ValidationError>>();
        var error = errors!.Single();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.PropertyName.Should().Be("Isbn");
        error.ErrorMessage.Should().Be("A book with the same ISBN already exists");
    }

    [Fact]
    public async Task GetBook_ReturnsBook_WhenBookExists()
    {
        // Arrange
        var httpClient = factory.CreateClient();
        var book = GenerateBook();
        await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book.Isbn);

        // Act
        var result = await httpClient.GetAsync($"/books/{book.Isbn}");
        var existingBook = await result.Content.ReadFromJsonAsync<Book>();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        existingBook.Should().BeEquivalentTo(book);
    }

    [Fact]
    public async Task GetBook_ReturnsNotFound_WhenBookDoesNotExist()
    {
        // Arrange
        var httpClient = factory.CreateClient();
        var isbn = GenerateIsbn();

        // Act
        var result = await httpClient.GetAsync($"/books/{isbn}");

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAllBooks_ReturnsAllBooks_WhenBooksExist()
    {
        // Arrange
        var httpClient = factory.CreateClient();
        var book = GenerateBook();
        await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book.Isbn);
        var books = new List<Book> { book };

        // Act
        var result = await httpClient.GetAsync("/books");
        var existingBooks = await result.Content.ReadFromJsonAsync<List<Book>>();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        existingBooks.Should().BeEquivalentTo(books);
    }

    [Fact]
    public async Task GetAllBooks_ReturnsEmptyList_WhenNoBooksExist()
    {
        // Arrange
        var httpClient = factory.CreateClient();

        // Act
        var result = await httpClient.GetAsync("/books");
        var returnedBooks = await result.Content.ReadFromJsonAsync<List<Book>>();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        returnedBooks.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchBooks_ReturnBooks_WhenTitleMatches()
    {
        // Arrange
        var httpClient = factory.CreateClient();
        var book = GenerateBook();
        await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book.Isbn);
        var books = new List<Book> { book };

        // Act
        var result = await httpClient.GetAsync($"/books?searchTerm=est");
        var existingBooks = await result.Content.ReadFromJsonAsync<List<Book>>();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        existingBooks.Should().BeEquivalentTo(books);
    }

    [Fact]
    public async Task SearchBooks_ReturnsEmptyList_WhenTitleDoesNotMatch()
    {
        // Arrange
        var httpClient = factory.CreateClient();
        var book = GenerateBook();
        await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book.Isbn);

        // Act
        var result = await httpClient.GetAsync($"/books?searchTerm=Invalid");
        var returnedBooks = await result.Content.ReadFromJsonAsync<List<Book>>();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        returnedBooks.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateBook_UpdatesBook_WhenDataIsCorrect()
    {
        // Arrange
        var httpClient = factory.CreateClient();
        var book = GenerateBook();
        await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book.Isbn);
        book.Title = "Updated Title";

        // Act
        var result = await httpClient.PutAsJsonAsync($"/books/{book.Isbn}", book);
        var updatedBook = await result.Content.ReadFromJsonAsync<Book>();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        updatedBook.Should().BeEquivalentTo(book);
    }

    [Fact]
    public async Task UpdateBook_Fails_WhenIsbnIsInvalid()
    {
        // Arrange
        var httpClient = factory.CreateClient();
        var book = GenerateBook();
        await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book.Isbn);
        book.Isbn = "Invalid ISBN";

        // Act
        var result = await httpClient.PutAsJsonAsync($"/books/{book.Isbn}", book);
        var errors = await result.Content.ReadFromJsonAsync<IEnumerable<ValidationError>>();
        var error = errors!.Single();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.PropertyName.Should().Be("Isbn");
        error.ErrorMessage.Should().Be("Invalid ISBN format");
    }

    [Fact]
    public async Task DeleteBook_DeletesBook_WhenBookExists()
    {
        // Arrange
        var httpClient = factory.CreateClient();
        var book = GenerateBook();
        await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book.Isbn);

        // Act
        var result = await httpClient.DeleteAsync($"/books/{book.Isbn}");

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteBook_ReturnsNotFound_WhenBookDoesNotExist()
    {
        // Arrange
        var httpClient = factory.CreateClient();
        var isbn = GenerateIsbn();

        // Act
        var result = await httpClient.DeleteAsync($"/books/{isbn}");

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private Book GenerateBook(string title = "Test Book")
    {
        return new Book
        {
            Isbn = GenerateIsbn(),
            Title = title,
            Author = "Test Author",
            ShortDescription = "Test Description",
            PageCount = 100,
            ReleaseDate = new DateTime(2024, 1, 1)
        };
    }

    private string GenerateIsbn()
    {
        return $"{Random.Shared.Next(100, 999)}-" +
               $"{Random.Shared.Next(1000000000, 2100999999)}";
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        var httpClient = factory.CreateClient();
        foreach (var createdIsbn in _createdIsbns)
        {
            await httpClient.DeleteAsync($"/books/{createdIsbn}");
        }
    }
}