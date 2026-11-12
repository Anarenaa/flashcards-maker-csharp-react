using System.Net;
using System.Net.Http.Json;
using Application.Common.Dtos;
using Application.Common.Models;
using Domain.Flashcards;
using Domain.Sets;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tests.Common;
using Tests.Data.Sets;
using Xunit;

namespace Api.Tests.Integration.Sets;

public class SetsControllerTests : BaseIntegrationTest, IAsyncLifetime
{
    private readonly Set _firstTestSet;
    private readonly Set _secondTestSet;
    private readonly Set _thirdTestSet;
    private readonly string _setsRoute;

    public SetsControllerTests(IntegrationTestWebFactory factory) : base(factory)
    {
        _firstTestSet = SetsTestData.FirstTestSet();
        _secondTestSet = SetsTestData.SecondTestSet();
        _thirdTestSet = SetsTestData.ThirdTestSet();
        _setsRoute = "api/sets";
    }

    public async Task InitializeAsync()
    {
        await Context.Sets.AddAsync(_secondTestSet);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();
    }

    public async Task DisposeAsync()
    {
        Context.Flashcards.RemoveRange(Context.Flashcards);
        Context.Sets.RemoveRange(Context.Sets);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task ShouldCreateSet()
    {
        // Arrange
        var request = new CreateSetDto(
            Name: _firstTestSet.Name,
            Description: _firstTestSet.Description,
            Type: _firstTestSet.Type,
            FromLang: _firstTestSet.FromLang,
            ToLang: _firstTestSet.ToLang,
            IsPublic: _firstTestSet.IsPublic,
            IsGenerated: _firstTestSet.IsGenerated);

        // Act
        var response = await Client.PostAsJsonAsync(_setsRoute, request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var responseDto = await response.Content.ReadFromJsonAsync<SetDto>();
        responseDto.Should().NotBeNull();
        responseDto.Id.Should().BeGreaterThan(0);
        responseDto.Name.Should().Be(_firstTestSet.Name);
        responseDto.Description.Should().Be(_firstTestSet.Description);
        responseDto.Type.Should().Be(_firstTestSet.Type);
        responseDto.FromLang.Should().Be(_firstTestSet.FromLang);
        responseDto.ToLang.Should().Be(_firstTestSet.ToLang);
        responseDto.IsPublic.Should().Be(_firstTestSet.IsPublic);
        responseDto.IsGenerated.Should().Be(_firstTestSet.IsGenerated);
        responseDto.FlashcardsCount.Should().Be(0);

        var dbSet = await Context.Sets.FirstOrDefaultAsync(x => x.Id == new SetId(responseDto.Id));
        dbSet.Should().NotBeNull();
        dbSet.Name.Should().Be(_firstTestSet.Name);
        dbSet.FromLang.Should().Be(_firstTestSet.FromLang);
    }

    [Fact]
    public async Task ShouldNotCreateSetBecauseRequestIsInvalid()
    {
        // Arrange - empty name and invalid language code length fail validation rules
        var request = new CreateSetDto(
            Name: "",
            Description: "Description",
            Type: SetType.Language,
            FromLang: "invalid_length",
            ToLang: "toolong",
            IsPublic: true,
            IsGenerated: false);

        // Act
        var response = await Client.PostAsJsonAsync(_setsRoute, request);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var count = await Context.Sets.CountAsync(x => x.Name == "");
        count.Should().Be(0);
    }

    [Fact]
    public async Task ShouldNotCreateSetBecauseNameAlreadyExists()
    {
        // Arrange - name is already taken by _secondTestSet
        var request = new CreateSetDto(
            Name: _secondTestSet.Name,
            Description: "Another Description",
            Type: SetType.Language,
            FromLang: "en",
            ToLang: "uk",
            IsPublic: true,
            IsGenerated: false);

        // Act
        var response = await Client.PostAsJsonAsync(_setsRoute, request);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task ShouldGetSetById()
    {
        // Arrange - attach flashcards to verify FlashcardsCount projection
        var card1 = Flashcard.New("Hello", "Привіт", _secondTestSet.Id);
        var card2 = Flashcard.New("Bye", "Бувай", _secondTestSet.Id);
        await Context.Flashcards.AddRangeAsync(card1, card2);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Act
        var response = await Client.GetAsync($"{_setsRoute}/{_secondTestSet.Id.Value}");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var responseDto = await response.Content.ReadFromJsonAsync<SetDto>();
        responseDto.Should().NotBeNull();
        responseDto.Id.Should().Be(_secondTestSet.Id.Value);
        responseDto.Name.Should().Be(_secondTestSet.Name);
        responseDto.Description.Should().Be(_secondTestSet.Description);
        responseDto.Type.Should().Be(_secondTestSet.Type);
        responseDto.FromLang.Should().Be(_secondTestSet.FromLang);
        responseDto.ToLang.Should().Be(_secondTestSet.ToLang);
        responseDto.FlashcardsCount.Should().Be(2);
    }

    [Fact]
    public async Task ShouldNotGetSetByIdBecauseSetNotFound()
    {
        // Act
        var response = await Client.GetAsync($"{_setsRoute}/999999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldUpdateSet()
    {
        // Arrange - update _secondTestSet with values from _thirdTestSet
        var request = new UpdateSetDto(
            Name: _thirdTestSet.Name,
            Description: _thirdTestSet.Description,
            Type: _thirdTestSet.Type,
            FromLang: _thirdTestSet.FromLang,
            ToLang: _thirdTestSet.ToLang,
            IsPublic: _thirdTestSet.IsPublic,
            IsGenerated: _thirdTestSet.IsGenerated);

        // Act
        var response = await Client.PutAsJsonAsync($"{_setsRoute}/{_secondTestSet.Id.Value}", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var responseDto = await response.Content.ReadFromJsonAsync<SetDto>();
        responseDto.Should().NotBeNull();
        responseDto.Id.Should().Be(_secondTestSet.Id.Value);
        responseDto.Name.Should().Be(_thirdTestSet.Name);
        responseDto.Description.Should().Be(_thirdTestSet.Description);
        responseDto.Type.Should().Be(_thirdTestSet.Type);
        responseDto.FromLang.Should().Be(_thirdTestSet.FromLang);
        responseDto.ToLang.Should().Be(_thirdTestSet.ToLang);
        responseDto.IsPublic.Should().Be(_thirdTestSet.IsPublic);

        var dbSet = await Context.Sets.FirstOrDefaultAsync(x => x.Id == _secondTestSet.Id);
        dbSet.Should().NotBeNull();
        dbSet.Name.Should().Be(_thirdTestSet.Name);
        dbSet.IsPublic.Should().Be(_thirdTestSet.IsPublic);
    }

    [Fact]
    public async Task ShouldNotUpdateSetBecauseSetNotFound()
    {
        // Arrange
        var request = new UpdateSetDto(
            Name: _thirdTestSet.Name,
            Description: _thirdTestSet.Description,
            Type: _thirdTestSet.Type,
            FromLang: _thirdTestSet.FromLang,
            ToLang: _thirdTestSet.ToLang,
            IsPublic: _thirdTestSet.IsPublic,
            IsGenerated: _thirdTestSet.IsGenerated);

        // Act
        var response = await Client.PutAsJsonAsync($"{_setsRoute}/999999", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldNotUpdateSetBecauseNameAlreadyExists()
    {
        // Arrange - add _firstTestSet to DB, then attempt to update _firstTestSet with _secondTestSet's Name
        await Context.Sets.AddAsync(_firstTestSet);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        var request = new UpdateSetDto(
            Name: _secondTestSet.Name,
            Description: _firstTestSet.Description,
            Type: _firstTestSet.Type,
            FromLang: _firstTestSet.FromLang,
            ToLang: _firstTestSet.ToLang,
            IsPublic: _firstTestSet.IsPublic,
            IsGenerated: _firstTestSet.IsGenerated);

        // Act
        var response = await Client.PutAsJsonAsync($"{_setsRoute}/{_firstTestSet.Id.Value}", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task ShouldUpdateSetWithSameNameSuccessfully()
    {
        // Arrange - update description while keeping the same Name
        var request = new UpdateSetDto(
            Name: _secondTestSet.Name,
            Description: "Updated description only",
            Type: _secondTestSet.Type,
            FromLang: _secondTestSet.FromLang,
            ToLang: _secondTestSet.ToLang,
            IsPublic: _secondTestSet.IsPublic,
            IsGenerated: _secondTestSet.IsGenerated);

        // Act
        var response = await Client.PutAsJsonAsync($"{_setsRoute}/{_secondTestSet.Id.Value}", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ShouldDeleteSet()
    {
        // Arrange - add a flashcard to verify cascade delete
        var card = Flashcard.New("Test Term", "Test Definition", _secondTestSet.Id);
        await Context.Flashcards.AddAsync(card);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Act
        var response = await Client.DeleteAsync($"{_setsRoute}/{_secondTestSet.Id.Value}");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseDto = await response.Content.ReadFromJsonAsync<SetDto>();
        responseDto.Should().NotBeNull();
        responseDto.Id.Should().Be(_secondTestSet.Id.Value);
        responseDto.Name.Should().Be(_secondTestSet.Name);

        // Verify set is deleted
        var dbSet = await Context.Sets.FirstOrDefaultAsync(x => x.Id == _secondTestSet.Id);
        dbSet.Should().BeNull();

        // Verify cascade delete: flashcard belonging to this set is also deleted
        var dbCard = await Context.Flashcards.FirstOrDefaultAsync(x => x.Id == card.Id);
        dbCard.Should().BeNull();
    }

    [Fact]
    public async Task ShouldNotDeleteSetBecauseSetNotFound()
    {
        // Act
        var response = await Client.DeleteAsync($"{_setsRoute}/999999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Verify existing set remains intact
        var dbSet = await Context.Sets.FirstOrDefaultAsync(x => x.Id == _secondTestSet.Id);
        dbSet.Should().NotBeNull();
    }

    [Fact]
    public async Task ShouldGetFlashcardsBySet()
    {
        // Arrange - add a third set and flashcards
        await Context.Sets.AddAsync(_thirdTestSet);
        var card1 = Flashcard.New("Apple", "Яблуко", _secondTestSet.Id);
        var card2 = Flashcard.New("Book", "Книга", _secondTestSet.Id);
        var otherCard = Flashcard.New("Hund", "Собака", _thirdTestSet.Id);

        await Context.Flashcards.AddRangeAsync(card1, card2, otherCard);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Act
        var response = await Client.GetAsync($"{_setsRoute}/{_secondTestSet.Id.Value}/flashcards");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var responseDto = await response.Content.ReadFromJsonAsync<PagedResult<FlashcardDto>>();
        responseDto.Should().NotBeNull();
        responseDto.TotalCount.Should().Be(2);
        responseDto.Items.Should().HaveCount(2);
        responseDto.Items.Should().Contain(x => x.Term == "Apple");
        responseDto.Items.Should().Contain(x => x.Term == "Book");
        responseDto.Items.Should().NotContain(x => x.Term == "Hund");
    }
}
