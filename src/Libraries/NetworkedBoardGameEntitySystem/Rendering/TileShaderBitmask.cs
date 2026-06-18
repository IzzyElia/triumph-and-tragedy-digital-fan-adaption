namespace TT2026.Libraries.NetworkedBoardGameEntitySystem.Rendering;

/// <summary>
/// Builds the 32-bit per-tile feature mask consumed by tile.gdshader's
/// <c>tile_mask</c> uniform. Layout (must match the shader exactly):
///
///   bits  0- 7 : main border  (8 dirs, incl. corners)
///   bits  8-15 : alt  border  (8 dirs, incl. corners)
///   bits 16-23 : river        (8 dirs, incl. corners) - all set = ocean
///   bits 24-27 : mountains    (N E S W) - each covers half the tile
///   bits 28-31 : forests      (N E S W) - each covers half the tile
///
/// Within every 8-direction group the local bit order is:
///   N=0 E=1 S=2 W=3 NE=4 SE=5 SW=6 NW=7
///
/// Apply with: material.SetShaderParameter("tile_mask", mask.GetBitmask());
/// </summary>
public struct TileShaderBitmask
{
    // Main border
    public bool Up, Right, Down, Left, UpRight, DownRight, DownLeft, UpLeft;

    // Alt border
    public bool AltUp, AltRight, AltDown, AltLeft,
                AltUpRight, AltDownRight, AltDownLeft, AltUpLeft;

    // Rivers
    public bool RiverUp, RiverRight, RiverDown, RiverLeft,
                RiverUpRight, RiverDownRight, RiverDownLeft, RiverUpLeft;

    // Mountains (cardinal only; each covers the matching half of the tile)
    public bool MountainUp, MountainRight, MountainDown, MountainLeft;

    // Forests (cardinal only; each covers the matching half of the tile)
    public bool ForestUp, ForestRight, ForestDown, ForestLeft;

    // Local bit indices within an 8-direction group.
    private const int N = 0, E = 1, S = 2, W = 3, NE = 4, SE = 5, SW = 6, NW = 7;

    // Group base offsets.
    private const int Main = 0, Alt = 8, River = 16, Mountain = 24, Forest = 28;

    public uint GetBitmask()
    {
        uint mask = 0u;

        // Main border
        if (Up)        mask |= 1u << (Main + N);
        if (Right)     mask |= 1u << (Main + E);
        if (Down)      mask |= 1u << (Main + S);
        if (Left)      mask |= 1u << (Main + W);
        if (UpRight)   mask |= 1u << (Main + NE);
        if (DownRight) mask |= 1u << (Main + SE);
        if (DownLeft)  mask |= 1u << (Main + SW);
        if (UpLeft)    mask |= 1u << (Main + NW);

        // Alt border
        if (AltUp)        mask |= 1u << (Alt + N);
        if (AltRight)     mask |= 1u << (Alt + E);
        if (AltDown)      mask |= 1u << (Alt + S);
        if (AltLeft)      mask |= 1u << (Alt + W);
        if (AltUpRight)   mask |= 1u << (Alt + NE);
        if (AltDownRight) mask |= 1u << (Alt + SE);
        if (AltDownLeft)  mask |= 1u << (Alt + SW);
        if (AltUpLeft)    mask |= 1u << (Alt + NW);

        // Rivers
        if (RiverUp)        mask |= 1u << (River + N);
        if (RiverRight)     mask |= 1u << (River + E);
        if (RiverDown)      mask |= 1u << (River + S);
        if (RiverLeft)      mask |= 1u << (River + W);
        if (RiverUpRight)   mask |= 1u << (River + NE);
        if (RiverDownRight) mask |= 1u << (River + SE);
        if (RiverDownLeft)  mask |= 1u << (River + SW);
        if (RiverUpLeft)    mask |= 1u << (River + NW);

        // Mountains (cardinal)
        if (MountainUp)    mask |= 1u << (Mountain + N);
        if (MountainRight) mask |= 1u << (Mountain + E);
        if (MountainDown)  mask |= 1u << (Mountain + S);
        if (MountainLeft)  mask |= 1u << (Mountain + W);

        // Forests (cardinal)
        if (ForestUp)    mask |= 1u << (Forest + N);
        if (ForestRight) mask |= 1u << (Forest + E);
        if (ForestDown)  mask |= 1u << (Forest + S);
        if (ForestLeft)  mask |= 1u << (Forest + W);

        return mask;
    }

    /// <summary>True when every river bit is set, i.e. the tile is open ocean.</summary>
    public readonly bool IsOcean =>
        RiverUp && RiverRight && RiverDown && RiverLeft &&
        RiverUpRight && RiverDownRight && RiverDownLeft && RiverUpLeft;
}