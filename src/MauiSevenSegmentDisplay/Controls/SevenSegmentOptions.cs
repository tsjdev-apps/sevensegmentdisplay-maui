namespace MauiSevenSegmentDisplay.Controls;

/// <summary>
/// Design-space constants used by the renderer. The control scales this model to
/// the actual pixel size of the Skia canvas during each paint pass.
/// </summary>
public static class SevenSegmentOptions
{
    /// <summary>
    /// Width of a regular character measured in multiples of the segment thickness.
    /// </summary>
    public const float DefaultCharacterWidthRatio = 6.5f;

    /// <summary>
    /// Height of a regular character measured in multiples of the segment thickness.
    /// </summary>
    public const float DefaultCharacterHeightRatio = 11f;

    /// <summary>
    /// Width of punctuation-like symbols such as colon and decimal point.
    /// </summary>
    public const float SymbolWidthRatio = 2.2f;

    /// <summary>
    /// Lower bound that keeps geometry calculations valid for very small thickness values.
    /// </summary>
    public const float MinimumDesignThickness = 1f;
}
