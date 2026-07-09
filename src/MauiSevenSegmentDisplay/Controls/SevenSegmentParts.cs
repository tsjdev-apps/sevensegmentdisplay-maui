namespace MauiSevenSegmentDisplay.Controls;

/// <summary>
/// The seven LED bars used by a classic seven-segment display.
/// </summary>
[Flags]
public enum SevenSegmentParts
{
    /// <summary>No segment is lit.</summary>
    None = 0,

    /// <summary>Top horizontal segment.</summary>
    A = 1 << 0,

    /// <summary>Upper-right vertical segment.</summary>
    B = 1 << 1,

    /// <summary>Lower-right vertical segment.</summary>
    C = 1 << 2,

    /// <summary>Bottom horizontal segment.</summary>
    D = 1 << 3,

    /// <summary>Lower-left vertical segment.</summary>
    E = 1 << 4,

    /// <summary>Upper-left vertical segment.</summary>
    F = 1 << 5,

    /// <summary>Middle horizontal segment.</summary>
    G = 1 << 6,

    /// <summary>All seven standard segments are lit.</summary>
    All = A | B | C | D | E | F | G
}
