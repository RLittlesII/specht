using System.Globalization;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Identifiers;

namespace Specht.Tests.Identifiers;

/// <summary>
/// The Snowflake strategy beneath <c>0001-F19</c> B-007 and B-022 to B-029 (ADR-0033; C-3, C-16): built from a type's
/// <c>snowflake</c> sub-object and the type's name, it draws a number that holds the milliseconds from its epoch to the
/// clock's reading above its datacenter, worker and sequence fields, as decimal digits. An absent epoch is 2026-01-01 at
/// midnight, universal time, and absent widths are 5, 5 and 12 bits. A named datacenter or worker is the one held, and an
/// unnamed datacenter, worker or sequence start is drawn for the request. A clock that reads earlier at a later draw than at
/// the draw before it refuses. The draws come from a <see cref="FixedRandomSource"/> and the time from a
/// <see cref="FixedClock"/>, so nothing here is random or read from the machine.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SnowflakeStrategyUnitTests
{
    [Theory]
    [ClassData(typeof(SnowflakeEpochs))]
    public void ASnowflakeWithAnEpoch_WhenDrawn_ShouldHoldTheMillisecondsFromThatEpochAboveItsFields(
        string settings, string time, int below, long milliseconds)
    {
        // Given
        var clock = new FixedClock(DateTimeOffset.Parse(time, CultureInfo.InvariantCulture));
        var strategy = new SnowflakeStrategy(JsonNode.Parse(settings), "adr", new FixedRandomSource(0), clock);

        // When
        var id = strategy.Next();

        // Then
        (long.Parse(id, NumberStyles.None, CultureInfo.InvariantCulture) >> below).Should().Be(milliseconds);
    }

    [Theory]
    [ClassData(typeof(SnowflakeSettingsNamingNoEpoch))]
    public void ASnowflakeNamingNoEpoch_WhenDrawn_ShouldHoldTheMillisecondsFromMidnightOn20260101UniversalTime(
        string? settings, string time, long milliseconds)
    {
        // Given
        var clock = new FixedClock(DateTimeOffset.Parse(time, CultureInfo.InvariantCulture));
        var strategy = new SnowflakeStrategy(settings is null ? null : JsonNode.Parse(settings), "adr", new FixedRandomSource(0), clock);

        // When
        var id = strategy.Next();

        // Then
        (long.Parse(id, NumberStyles.None, CultureInfo.InvariantCulture) >> 22).Should().Be(milliseconds);
    }

    [Fact]
    public void ASnowflakeNamingNoFieldWidths_WhenDrawn_ShouldHoldItsSequenceInTheLowest12BitsItsWorkerInThe5AboveAndItsDatacenterInThe5AboveThose()
    {
        // Given
        var strategy = new SnowflakeStrategy(
            JsonNode.Parse("""{ "datacenter": 3, "worker": 9 }"""), "adr", new FixedRandomSource(3001), new FixedClock(FixedClock.CheckTime));

        // When
        var number = long.Parse(strategy.Next(), NumberStyles.None, CultureInfo.InvariantCulture);

        // Then
        (number & 0xFFF).Should().Be(3001);
        ((number >> 12) & 0x1F).Should().Be(9);
        ((number >> 17) & 0x1F).Should().Be(3);
        (number >> 22).Should().Be(26827200000);
    }

    [Theory]
    [ClassData(typeof(SnowflakeFieldsLeftUnnamed))]
    public void ASnowflakeFieldLeftUnnamed_WhenTwoRequestsAreDrawn_ShouldHoldTheValueDrawnForEachRequest(
        string settings, int below, int width, int first, int second)
    {
        // Given
        var random = new FixedRandomSource(first, second);
        var clock = new FixedClock(FixedClock.CheckTime);

        // When
        var one = long.Parse(new SnowflakeStrategy(JsonNode.Parse(settings), "adr", random, clock).Next(), NumberStyles.None, CultureInfo.InvariantCulture);
        var two = long.Parse(new SnowflakeStrategy(JsonNode.Parse(settings), "adr", random, clock).Next(), NumberStyles.None, CultureInfo.InvariantCulture);

        // Then
        ((one >> below) & ((1L << width) - 1)).Should().Be(first);
        ((two >> below) & ((1L << width) - 1)).Should().Be(second);
        random.Bounds.Should().Equal(1 << width, 1 << width);
    }

    [Theory]
    [ClassData(typeof(SnowflakeFieldsNamed))]
    public void ASnowflakeNamingADatacenterOrAWorker_WhenDrawn_ShouldHoldThatNumberInItsField(string settings, int below, int width, int named)
    {
        // Given
        var strategy = new SnowflakeStrategy(JsonNode.Parse(settings), "adr", new FixedRandomSource(1), new FixedClock(FixedClock.CheckTime));

        // When
        var number = long.Parse(strategy.Next(), NumberStyles.None, CultureInfo.InvariantCulture);

        // Then
        ((number >> below) & ((1L << width) - 1)).Should().Be(named);
    }

    [Fact]
    public void ASecondDrawWhoseClockReadsEarlierThanAtTheFirst_WhenDrawn_ShouldBeRefused()
    {
        // Given
        var clock = new FixedClock(FixedClock.CheckTime, FixedClock.CheckTime.AddMilliseconds(-1));
        var strategy = new SnowflakeStrategy(null, "adr", new FixedRandomSource(0), clock);
        strategy.Next();

        // When
        var draw = strategy.Next;

        // Then
        draw.Should().Throw<IdRefusedException>();
    }
}
