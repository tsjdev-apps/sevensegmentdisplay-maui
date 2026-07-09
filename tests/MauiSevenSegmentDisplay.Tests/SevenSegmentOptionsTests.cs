using MauiSevenSegmentDisplay.Controls;

namespace MauiSevenSegmentDisplay.Tests;

/// <summary>
/// Unit tests for design-space constants used by the Skia renderer.
/// </summary>
[TestFixture]
public sealed class SevenSegmentOptionsTests
{
    /// <summary>
    /// Confirms regular characters use a wider slot than punctuation symbols.
    /// </summary>
    [Test]
    public void CharacterWidthRatio_IsWiderThanSymbolWidthRatio()
    {
        Assert.That(
            SevenSegmentOptions.DefaultCharacterWidthRatio,
            Is.GreaterThan(SevenSegmentOptions.SymbolWidthRatio));
    }

    /// <summary>
    /// Confirms the display model is taller than it is wide, matching classic LED digits.
    /// </summary>
    [Test]
    public void CharacterHeightRatio_IsTallerThanCharacterWidthRatio()
    {
        Assert.That(
            SevenSegmentOptions.DefaultCharacterHeightRatio,
            Is.GreaterThan(SevenSegmentOptions.DefaultCharacterWidthRatio));
    }

    /// <summary>
    /// Confirms the minimum thickness protects geometry calculations from zero-sized segments.
    /// </summary>
    [Test]
    public void MinimumDesignThickness_IsPositive()
    {
        Assert.That(SevenSegmentOptions.MinimumDesignThickness, Is.GreaterThan(0));
    }
}
