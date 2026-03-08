using Xunit;
#if (IncludeFluentAssertions)
using FluentAssertions;
#endif
#if (IncludeNSubstitute)
using NSubstitute;
#endif
#if (IncludeBogus)
using Bogus;
#endif

namespace Company.ProjectName.UnitTests;

public class SampleTests
{
    [Fact]
    public void Sample_ShouldReturnExpectedValue()
    {
        // Arrange
        var expected = "Hello, World!";

        // Act
        var result = expected;

        // Assert
#if (IncludeFluentAssertions)
        result.Should().Be(expected);
#else
        Assert.Equal(expected, result);
#endif
    }

#if (IncludeNSubstitute)
    public interface ISampleService
    {
        string GetGreeting();
    }

    [Fact]
    public void SampleMock_ShouldReturnSubstitutedValue()
    {
        // Arrange
        var service = Substitute.For<ISampleService>();
        service.GetGreeting().Returns("Hello from NSubstitute!");

        // Act
        var result = service.GetGreeting();

        // Assert
#if (IncludeFluentAssertions)
        result.Should().Be("Hello from NSubstitute!");
#else
        Assert.Equal("Hello from NSubstitute!", result);
#endif
    }
#endif

#if (IncludeBogus)
    [Fact]
    public void SampleBogus_ShouldGenerateFakeData()
    {
        // Arrange
        var faker = new Faker();

        // Act
        var name = faker.Name.FullName();
        var email = faker.Internet.Email();

        // Assert
#if (IncludeFluentAssertions)
        name.Should().NotBeNullOrEmpty();
        email.Should().Contain("@");
#else
        Assert.False(string.IsNullOrEmpty(name));
        Assert.Contains("@", email);
#endif
    }
#endif
}
