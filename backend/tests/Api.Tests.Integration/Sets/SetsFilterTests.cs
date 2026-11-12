using System.Net.Http.Json;
using Application.Common.Dtos;
using Application.Common.Models;
using Domain.Sets;
using FluentAssertions;
using Tests.Common;
using Tests.Data.Sets;
using Xunit;

namespace Api.Tests.Integration.Sets;

public class SetsFilterTests : BaseIntegrationTest, IAsyncLifetime
{
    private readonly Set _firstTestSet;
    private readonly Set _secondTestSet;
    private readonly Set _thirdTestSet;
    private readonly string _setsRoute;

    public SetsFilterTests(IntegrationTestWebFactory factory) : base(factory)
    {
        _firstTestSet = SetsTestData.FirstTestSet();
        _secondTestSet = SetsTestData.SecondTestSet();
        _thirdTestSet = SetsTestData.ThirdTestSet(); 
        _setsRoute = "api/sets";
    }

    public async Task InitializeAsync()
    {
        await Context.Sets.AddRangeAsync(_firstTestSet, _secondTestSet, _thirdTestSet);
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
    public async Task ShouldGetSetsPaged()
    {
        // Act
        var response = await Client.GetAsync($"{_setsRoute}?pageNumber=1&pageSize=2");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var responseDto = await response.Content.ReadFromJsonAsync<PagedResult<SetDto>>();
        responseDto.Should().NotBeNull();
        responseDto.TotalCount.Should().Be(3);
        responseDto.PageSize.Should().Be(2);
        responseDto.PageNumber.Should().Be(1);
        responseDto.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task ShouldFilterSetsBySearchTerm()
    {
        // Act
        var response = await Client.GetAsync($"{_setsRoute}?searchTerm=English");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var responseDto = await response.Content.ReadFromJsonAsync<PagedResult<SetDto>>();
        responseDto.Should().NotBeNull();
        responseDto.TotalCount.Should().Be(1);
        responseDto.Items.Should().Contain(x => x.Id == _firstTestSet.Id.Value);
        responseDto.Items.Should().NotContain(x => x.Id == _secondTestSet.Id.Value);
        responseDto.Items.Should().NotContain(x => x.Id == _thirdTestSet.Id.Value);
    }

    [Fact]
    public async Task ShouldFilterSetsByLanguage()
    {
        // Act
        var response = await Client.GetAsync($"{_setsRoute}?fromLang=de&toLang=uk");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var responseDto = await response.Content.ReadFromJsonAsync<PagedResult<SetDto>>();
        responseDto.Should().NotBeNull();
        responseDto.TotalCount.Should().Be(1);
        responseDto.Items.Should().Contain(x => x.Id == _secondTestSet.Id.Value);
        responseDto.Items.Should().NotContain(x => x.Id == _firstTestSet.Id.Value);
        responseDto.Items.Should().NotContain(x => x.Id == _thirdTestSet.Id.Value);
    }

    [Fact]
    public async Task ShouldFilterSetsByPublicity()
    {
        // Act
        var response = await Client.GetAsync($"{_setsRoute}?isPublic=false");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var responseDto = await response.Content.ReadFromJsonAsync<PagedResult<SetDto>>();
        responseDto.Should().NotBeNull();
        responseDto.TotalCount.Should().Be(1);
        responseDto.Items.Should().Contain(x => x.Id == _thirdTestSet.Id.Value);
        responseDto.Items.Should().NotContain(x => x.Id == _firstTestSet.Id.Value);
        responseDto.Items.Should().NotContain(x => x.Id == _secondTestSet.Id.Value);
    }

    [Fact]
    public async Task ShouldSortSetsByNameAscending()
    {
        // Act - sort by name ascending
        var response = await Client.GetAsync($"{_setsRoute}?sortBy=name&sortDescending=false");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var responseDto = await response.Content.ReadFromJsonAsync<PagedResult<SetDto>>();
        responseDto.Should().NotBeNull();
        var items = responseDto.Items;
        items.Should().HaveCount(3);
        // Alphabetical order: English Vocabulary -> German Grammar -> History Dates
        items[0].Name.Should().Be(_firstTestSet.Name);
        items[1].Name.Should().Be(_secondTestSet.Name);
        items[2].Name.Should().Be(_thirdTestSet.Name);
    }

    [Fact]
    public async Task ShouldSortSetsByNameDescending()
    {
        // Act - sort by name descending
        var response = await Client.GetAsync($"{_setsRoute}?sortBy=name&sortDescending=true");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var responseDto = await response.Content.ReadFromJsonAsync<PagedResult<SetDto>>();
        responseDto.Should().NotBeNull();
        var items = responseDto.Items;
        items.Should().HaveCount(3);
        // Reverse alphabetical order: History Dates -> German Grammar -> English Vocabulary
        items[0].Name.Should().Be(_thirdTestSet.Name);
        items[1].Name.Should().Be(_secondTestSet.Name);
        items[2].Name.Should().Be(_firstTestSet.Name);
    }
}
