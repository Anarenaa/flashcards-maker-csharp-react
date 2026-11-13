using System.Text.Json;
using Domain.Sets;
using FluentAssertions;
using Xunit;

namespace Api.Tests.Integration.Sets;

public class SetTypeValueObjectTests
{
    [Fact]
    public void ShouldBeEqualWhenHavingSameValue()
    {
        // Arrange
        var type1 = SetType.Language;
        var type2 = SetType.From("language");

        // Assert
        type1.Should().Be(type2);
        (type1 == type2).Should().BeTrue();
        (type1 != type2).Should().BeFalse();
        type1.GetHashCode().Should().Be(type2.GetHashCode());
    }

    [Fact]
    public void ShouldNotBeEqualWhenHavingDifferentValue()
    {
        // Arrange
        var type1 = SetType.Language;
        var type2 = SetType.Subject;

        // Assert
        type1.Should().NotBe(type2);
        (type1 == type2).Should().BeFalse();
        (type1 != type2).Should().BeTrue();
    }

    [Theory]
    [InlineData("language", SetTypeValues.Language)]
    [InlineData("Language", SetTypeValues.Language)]
    [InlineData("LANGUAGE", SetTypeValues.Language)]
    [InlineData("subject", SetTypeValues.Subject)]
    [InlineData("Subject", SetTypeValues.Subject)]
    [InlineData("SUBJECT", SetTypeValues.Subject)]
    public void ShouldParseCaseInsensitively(string input, string expectedCanonicalValue)
    {
        // Act
        var parsed = SetType.From(input);

        // Assert
        parsed.Value.Should().Be(expectedCanonicalValue);
        (parsed == (expectedCanonicalValue == SetTypeValues.Language ? SetType.Language : SetType.Subject)).Should().BeTrue();
    }

    [Theory]
    [InlineData("Language", true, SetTypeValues.Language)]
    [InlineData("SUBJECT", true, SetTypeValues.Subject)]
    [InlineData("invalid", false, null)]
    [InlineData("", false, null)]
    [InlineData(null, false, null)]
    public void TryParse_ShouldHandleVariousInputs(string? input, bool expectedSuccess, string? expectedValue)
    {
        // Act
        var success = SetType.TryParse(input, out var result);

        // Assert
        success.Should().Be(expectedSuccess);
        if (expectedSuccess)
        {
            result.Should().NotBeNull();
            result!.Value.Should().Be(expectedValue);
        }
        else
        {
            result.Should().BeNull();
        }
    }

    [Fact]
    public void ShouldThrowArgumentExceptionWhenGivenInvalidValue()
    {
        // Act
        var act = () => SetType.From("InvalidType");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Invalid set type: InvalidType*");
    }

    [Fact]
    public void ShouldSerializeAndDeserializeJsonProperly()
    {
        // Arrange
        var original = SetType.Subject;

        // Act
        var json = JsonSerializer.Serialize(original);
        var deserializedFromLower = JsonSerializer.Deserialize<SetType>(json);
        var deserializedFromPascal = JsonSerializer.Deserialize<SetType>("\"Subject\"");

        // Assert
        json.Should().Be("\"subject\"");
        deserializedFromLower.Should().NotBeNull();
        deserializedFromLower.Should().Be(original);
        deserializedFromLower!.Value.Should().Be("subject");

        deserializedFromPascal.Should().NotBeNull();
        deserializedFromPascal.Should().Be(original);
        deserializedFromPascal!.Value.Should().Be("subject");
    }
}
