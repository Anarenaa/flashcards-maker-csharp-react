using System.Net;
using System.Net.Http.Json;
using Api.Dtos;
using Application.Common.Models;
using Domain.Flashcards;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tests.Common;
using Tests.Data.Flashcards;
using Xunit;

namespace Api.Tests.Integration.Flashcards;

public class FlashcardsControllerTests : BaseIntegrationTest, IAsyncLifetime
{
    private readonly Flashcard _firstTestFlashcard;
    private readonly Flashcard _secondTestFlashcard;
    private readonly Flashcard _thirdTestFlashcard;
    private readonly string _flashcardsRoute;

    public FlashcardsControllerTests(IntegrationTestWebFactory factory) : base(factory)
    {
        _firstTestFlashcard = FlashcardsTestData.FirstTestFlashcard();
        _secondTestFlashcard = FlashcardsTestData.SecondTestFlashcard();
        _thirdTestFlashcard = FlashcardsTestData.ThirdTestFlashcard();
        _flashcardsRoute = "api/flashcards";
    }

    public async Task InitializeAsync()
    {
        await Context.Flashcards.AddAsync(_secondTestFlashcard);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();
    }

    public async Task DisposeAsync()
    {
        Context.Flashcards.RemoveRange(Context.Flashcards);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task ShouldCreateFlashcard()
    {
        // Arrange
        var request = new CreateFlashcardDto(
            Term: _firstTestFlashcard.Term,
            Definition: _firstTestFlashcard.Definition);

        // Act
        var response = await Client.PostAsJsonAsync(_flashcardsRoute, request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var responseDto = await response.Content.ReadFromJsonAsync<FlashcardDto>();
        responseDto.Should().NotBeNull();
        responseDto!.Id.Should().BeGreaterThan(0);

        var dbFlashcard = await Context.Flashcards.FirstOrDefaultAsync(x => x.Id == new FlashcardId(responseDto.Id));
        dbFlashcard.Should().NotBeNull();
        dbFlashcard!.Term.Should().Be(_firstTestFlashcard.Term);
        dbFlashcard.Definition.Should().Be(_firstTestFlashcard.Definition);
    }

    [Fact]
    public async Task ShouldNotCreateFlashcardBecauseRequestIsInvalid()
    {
        // Arrange - empty term and definition fail validation rules
        var request = new CreateFlashcardDto(Term: "", Definition: "");

        // Act
        var response = await Client.PostAsJsonAsync(_flashcardsRoute, request);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var count = await Context.Flashcards.CountAsync(x => x.Term == "");
        count.Should().Be(0);
    }

    [Fact]
    public async Task ShouldUpdateFlashcard()
    {
        // Arrange
        var request = new UpdateFlashcardDto(
            Term: _thirdTestFlashcard.Term,
            Definition: _thirdTestFlashcard.Definition);

        // Act
        var response = await Client.PutAsJsonAsync($"{_flashcardsRoute}/{_secondTestFlashcard.Id.Value}", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var responseDto = await response.Content.ReadFromJsonAsync<FlashcardDto>();
        responseDto.Should().NotBeNull();
        responseDto!.Id.Should().Be(_secondTestFlashcard.Id.Value);

        var dbFlashcard = await Context.Flashcards.FirstOrDefaultAsync(x => x.Id == _secondTestFlashcard.Id);
        dbFlashcard.Should().NotBeNull();
        dbFlashcard!.Term.Should().Be(_thirdTestFlashcard.Term);
        dbFlashcard.Definition.Should().Be(_thirdTestFlashcard.Definition);
    }

    [Fact]
    public async Task ShouldNotUpdateFlashcardBecauseFlashcardNotFound()
    {
        // Arrange
        var request = new UpdateFlashcardDto(
            Term: _thirdTestFlashcard.Term,
            Definition: _thirdTestFlashcard.Definition);

        // Act
        var response = await Client.PutAsJsonAsync($"{_flashcardsRoute}/999999", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldDeleteFlashcard()
    {
        // Act
        var response = await Client.DeleteAsync($"{_flashcardsRoute}/{_secondTestFlashcard.Id.Value}");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseDto = await response.Content.ReadFromJsonAsync<FlashcardDto>();
        responseDto.Should().NotBeNull();
        responseDto!.Id.Should().Be(_secondTestFlashcard.Id.Value);
        responseDto.Term.Should().Be(_secondTestFlashcard.Term);
        responseDto.Definition.Should().Be(_secondTestFlashcard.Definition);

        var dbFlashcard = await Context.Flashcards.FirstOrDefaultAsync(x => x.Id == _secondTestFlashcard.Id);
        dbFlashcard.Should().BeNull();
    }

    [Fact]
    public async Task ShouldNotDeleteFlashcardBecauseFlashcardNotFound()
    {
        // Act
        var response = await Client.DeleteAsync($"{_flashcardsRoute}/999999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Verify existing record is intact
        var dbFlashcard = await Context.Flashcards.FirstOrDefaultAsync(x => x.Id == _secondTestFlashcard.Id);
        dbFlashcard.Should().NotBeNull();
    }

    [Fact]
    public async Task ShouldGetFlashcardById()
    {
        // Act
        var response = await Client.GetAsync($"{_flashcardsRoute}/{_secondTestFlashcard.Id.Value}");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var responseDto = await response.Content.ReadFromJsonAsync<FlashcardDto>();
        responseDto.Should().NotBeNull();
        responseDto!.Id.Should().Be(_secondTestFlashcard.Id.Value);
        responseDto.Term.Should().Be(_secondTestFlashcard.Term);
        responseDto.Definition.Should().Be(_secondTestFlashcard.Definition);
    }

    [Fact]
    public async Task ShouldGetFlashcardsPaged()
    {
        // Act
        var response = await Client.GetAsync($"{_flashcardsRoute}?pageNumber=1&pageSize=10");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var responseDto = await response.Content.ReadFromJsonAsync<PagedResult<FlashcardDto>>();
        responseDto.Should().NotBeNull();
        responseDto!.Items.Should().Contain(x => x.Id == _secondTestFlashcard.Id.Value);
    }
}
