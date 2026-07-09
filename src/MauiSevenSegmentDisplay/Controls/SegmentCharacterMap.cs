namespace MauiSevenSegmentDisplay.Controls;

/// <summary>
/// Converts text characters into the segment flags used by the renderer.
///
/// The segment names follow the standard layout:
///   -- A --
/// F       B
///   -- G --
/// E       C
///   -- D --
/// Unsupported characters intentionally map to blank so the reusable control can
/// display arbitrary bound text without throwing from the drawing path.
/// </summary>
public static class SegmentCharacterMap
{
    // The lookup table is intentionally explicit; it makes the blog sample easy
    // to audit and avoids clever conversion code that would be harder to explain.
    private static readonly IReadOnlyDictionary<char, SevenSegmentParts> Map =
        new Dictionary<char, SevenSegmentParts>
        {
            ['0'] = SevenSegmentParts.A | SevenSegmentParts.B | SevenSegmentParts.C | SevenSegmentParts.D | SevenSegmentParts.E | SevenSegmentParts.F,
            ['1'] = SevenSegmentParts.B | SevenSegmentParts.C,
            ['2'] = SevenSegmentParts.A | SevenSegmentParts.B | SevenSegmentParts.D | SevenSegmentParts.E | SevenSegmentParts.G,
            ['3'] = SevenSegmentParts.A | SevenSegmentParts.B | SevenSegmentParts.C | SevenSegmentParts.D | SevenSegmentParts.G,
            ['4'] = SevenSegmentParts.B | SevenSegmentParts.C | SevenSegmentParts.F | SevenSegmentParts.G,
            ['5'] = SevenSegmentParts.A | SevenSegmentParts.C | SevenSegmentParts.D | SevenSegmentParts.F | SevenSegmentParts.G,
            ['6'] = SevenSegmentParts.A | SevenSegmentParts.C | SevenSegmentParts.D | SevenSegmentParts.E | SevenSegmentParts.F | SevenSegmentParts.G,
            ['7'] = SevenSegmentParts.A | SevenSegmentParts.B | SevenSegmentParts.C,
            ['8'] = SevenSegmentParts.All,
            ['9'] = SevenSegmentParts.A | SevenSegmentParts.B | SevenSegmentParts.C | SevenSegmentParts.D | SevenSegmentParts.F | SevenSegmentParts.G,

            ['A'] = SevenSegmentParts.A | SevenSegmentParts.B | SevenSegmentParts.C | SevenSegmentParts.E | SevenSegmentParts.F | SevenSegmentParts.G,
            ['b'] = SevenSegmentParts.C | SevenSegmentParts.D | SevenSegmentParts.E | SevenSegmentParts.F | SevenSegmentParts.G,
            ['C'] = SevenSegmentParts.A | SevenSegmentParts.D | SevenSegmentParts.E | SevenSegmentParts.F,
            ['c'] = SevenSegmentParts.D | SevenSegmentParts.E | SevenSegmentParts.G,
            ['d'] = SevenSegmentParts.B | SevenSegmentParts.C | SevenSegmentParts.D | SevenSegmentParts.E | SevenSegmentParts.G,
            ['E'] = SevenSegmentParts.A | SevenSegmentParts.D | SevenSegmentParts.E | SevenSegmentParts.F | SevenSegmentParts.G,
            ['F'] = SevenSegmentParts.A | SevenSegmentParts.E | SevenSegmentParts.F | SevenSegmentParts.G,
            ['H'] = SevenSegmentParts.B | SevenSegmentParts.C | SevenSegmentParts.E | SevenSegmentParts.F | SevenSegmentParts.G,
            ['h'] = SevenSegmentParts.C | SevenSegmentParts.E | SevenSegmentParts.F | SevenSegmentParts.G,
            ['L'] = SevenSegmentParts.D | SevenSegmentParts.E | SevenSegmentParts.F,
            ['P'] = SevenSegmentParts.A | SevenSegmentParts.B | SevenSegmentParts.E | SevenSegmentParts.F | SevenSegmentParts.G,
            ['r'] = SevenSegmentParts.E | SevenSegmentParts.G,
            ['U'] = SevenSegmentParts.B | SevenSegmentParts.C | SevenSegmentParts.D | SevenSegmentParts.E | SevenSegmentParts.F,
            ['u'] = SevenSegmentParts.C | SevenSegmentParts.D | SevenSegmentParts.E,
            ['Y'] = SevenSegmentParts.B | SevenSegmentParts.C | SevenSegmentParts.D | SevenSegmentParts.F | SevenSegmentParts.G,

            ['R'] = SevenSegmentParts.A | SevenSegmentParts.B | SevenSegmentParts.C | SevenSegmentParts.E | SevenSegmentParts.F | SevenSegmentParts.G,
            ['O'] = SevenSegmentParts.A | SevenSegmentParts.B | SevenSegmentParts.C | SevenSegmentParts.D | SevenSegmentParts.E | SevenSegmentParts.F,
            ['N'] = SevenSegmentParts.A | SevenSegmentParts.B | SevenSegmentParts.C | SevenSegmentParts.E | SevenSegmentParts.F,
            ['S'] = SevenSegmentParts.A | SevenSegmentParts.C | SevenSegmentParts.D | SevenSegmentParts.F | SevenSegmentParts.G,
            ['D'] = SevenSegmentParts.B | SevenSegmentParts.C | SevenSegmentParts.D | SevenSegmentParts.E | SevenSegmentParts.G,

            ['-'] = SevenSegmentParts.G,
            ['_'] = SevenSegmentParts.D,
            [' '] = SevenSegmentParts.None,
            [':'] = SevenSegmentParts.None,
            ['.'] = SevenSegmentParts.None
        };

    /// <summary>
    /// Returns the active segment flags for a character, or a blank pattern for unsupported input.
    /// </summary>
    public static SevenSegmentParts GetSegments(char character) =>
        Map.TryGetValue(character, out SevenSegmentParts segments) ? segments : SevenSegmentParts.None;
}
