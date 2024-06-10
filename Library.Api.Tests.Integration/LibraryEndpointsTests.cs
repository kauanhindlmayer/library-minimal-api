using System.Net;
using FluentAssertions;
using Library.Api.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Library.Api.Tests.Integration;

public class LibraryEndpointsTests(WebApplicationFactory<IApiMarker> factory)
    : IClassFixture<WebApplicationFactory<IApiMarker>>, IAsyncLifetime
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
        book.Isbn = "Invalid ISBN";
        
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
        foreach (var isbn in _createdIsbns)
        {
            await httpClient.DeleteAsync($"/books/{isbn}");
        }
    }
}