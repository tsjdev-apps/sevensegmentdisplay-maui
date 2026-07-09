using MauiSevenSegmentDisplay.Controls;

namespace MauiSevenSegmentDisplay.Tests;

/// <summary>
/// Unit tests for the character-to-segment lookup used by the renderer.
/// </summary>
[TestFixture]
public sealed class SegmentCharacterMapTests
{
    /// <summary>
    /// Verifies the canonical digit mappings that make up the clock and countdown displays.
    /// </summary>
    [TestCase('0', SevenSegmentParts.A | SevenSegmentParts.B | SevenSegmentParts.C | SevenSegmentParts.D | SevenSegmentParts.E | SevenSegmentParts.F)]
    [TestCase('1', SevenSegmentParts.B | SevenSegmentParts.C)]
    [TestCase('2', SevenSegmentParts.A | SevenSegmentParts.B | SevenSegmentParts.D | SevenSegmentParts.E | SevenSegmentParts.G)]
    [TestCase('3', SevenSegmentParts.A | SevenSegmentParts.B | SevenSegmentParts.C | SevenSegmentParts.D | SevenSegmentParts.G)]
    [TestCase('4', SevenSegmentParts.B | SevenSegmentParts.C | SevenSegmentParts.F | SevenSegmentParts.G)]
    [TestCase('5', SevenSegmentParts.A | SevenSegmentParts.C | SevenSegmentParts.D | SevenSegmentParts.F | SevenSegmentParts.G)]
    [TestCase('6', SevenSegmentParts.A | SevenSegmentParts.C | SevenSegmentParts.D | SevenSegmentParts.E | SevenSegmentParts.F | SevenSegmentParts.G)]
    [TestCase('7', SevenSegmentParts.A | SevenSegmentParts.B | SevenSegmentParts.C)]
    [TestCase('8', SevenSegmentParts.All)]
    [TestCase('9', SevenSegmentParts.A | SevenSegmentParts.B | SevenSegmentParts.C | SevenSegmentParts.D | SevenSegmentParts.F | SevenSegmentParts.G)]
    public void GetSegments_ReturnsExpectedDigitPattern(char character, SevenSegmentParts expectedSegments)
    {
        SevenSegmentParts actualSegments = SegmentCharacterMap.GetSegments(character);

        Assert.That(actualSegments, Is.EqualTo(expectedSegments));
    }

    /// <summary>
    /// Verifies the letter mappings used by the status-display demo.
    /// </summary>
    [TestCase('R', SevenSegmentParts.A | SevenSegmentParts.B | SevenSegmentParts.C | SevenSegmentParts.E | SevenSegmentParts.F | SevenSegmentParts.G)]
    [TestCase('E', SevenSegmentParts.A | SevenSegmentParts.D | SevenSegmentParts.E | SevenSegmentParts.F | SevenSegmentParts.G)]
    [TestCase('A', SevenSegmentParts.A | SevenSegmentParts.B | SevenSegmentParts.C | SevenSegmentParts.E | SevenSegmentParts.F | SevenSegmentParts.G)]
    [TestCase('D', SevenSegmentParts.B | SevenSegmentParts.C | SevenSegmentParts.D | SevenSegmentParts.E | SevenSegmentParts.G)]
    [TestCase('Y', SevenSegmentParts.B | SevenSegmentParts.C | SevenSegmentParts.D | SevenSegmentParts.F | SevenSegmentParts.G)]
    public void GetSegments_ReturnsExpectedStatusLetterPattern(char character, SevenSegmentParts expectedSegments)
    {
        SevenSegmentParts actualSegments = SegmentCharacterMap.GetSegments(character);

        Assert.That(actualSegments, Is.EqualTo(expectedSegments));
    }

    /// <summary>
    /// Ensures punctuation that is drawn by custom symbol logic does not light normal bar segments.
    /// </summary>
    [TestCase(':')]
    [TestCase('.')]
    [TestCase(' ')]
    public void GetSegments_ReturnsBlankPatternForSymbols(char character)
    {
        SevenSegmentParts actualSegments = SegmentCharacterMap.GetSegments(character);

        Assert.That(actualSegments, Is.EqualTo(SevenSegmentParts.None));
    }

    /// <summary>
    /// Ensures unexpected characters render as blank instead of throwing during painting.
    /// </summary>
    [Test]
    public void GetSegments_ReturnsBlankPatternForUnsupportedCharacter()
    {
        SevenSegmentParts actualSegments = SegmentCharacterMap.GetSegments('?');

        Assert.That(actualSegments, Is.EqualTo(SevenSegmentParts.None));
    }
}
