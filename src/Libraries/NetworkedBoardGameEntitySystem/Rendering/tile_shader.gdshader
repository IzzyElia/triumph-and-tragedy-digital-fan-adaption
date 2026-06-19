shader_type spatial;
render_mode cull_back, diffuse_burley, specular_schlick_ggx;

// ============================================================================
//  Per-tile 32-bit feature mask (set per tile via a ShaderMaterial uniform).
//
//  The mask is split into four byte-aligned groups. Within every 8-direction
//  group the local bit order is:
//      N=0  E=1  S=2  W=3   NE=4  SE=5  SW=6  NW=7
//
//      bits  0- 7 : main border   (8 dirs, incl. corners)
//      bits  8-15 : alt  border   (8 dirs, incl. corners)
//      bits 16-23 : river         (8 dirs, incl. corners)  -> all set = ocean
//      bits 24-27 : mountains     (N E S W only - each covers half the tile)
//      bits 28-31 : forests       (N E S W only - each covers half the tile)
//
//  Keep this layout in sync with TileShaderBitmask.GetBitmask() on the C# side.
// ============================================================================
uniform uint tile_mask = 0u;

// --- Border / fill colors ----------------------------------------------------
uniform vec4 border_color     : source_color = vec4(1.0, 1.0, 1.0, 1.0);
uniform vec4 alt_border_color : source_color = vec4(1.0, 0.65, 0.0, 1.0);
uniform vec4 albedo_color     : source_color = vec4(1.0, 1.0, 1.0, 1.0);
uniform vec4 water_color      : source_color = vec4(0.0, 0.1, 1.0, 1.0);

// Main border AND rivers share this thickness. Alt border has its own.
uniform float border_thickness     : hint_range(0.0, 0.5) = 0.12;
uniform float alt_border_thickness : hint_range(0.0, 0.5) = 0.12;

// How strongly the river (water_color) tints a coincident main border.
// 0 = pure border, 1 = pure water.
uniform float river_border_blend : hint_range(0.0, 1.0) = 0.5;

// --- Water Fresnel ------------------------------------------------------------
// Tint applied to water at grazing view angles (rim / reflective sheen).
// Only affects pixels that actually show water (ocean + rivers); proportional
// to how much water is in the pixel, so it fades out across border/water blends.
// NOTE: on flat top-down tiles the view angle stays near head-on, so keep
// fresnel_power low (~1.0-1.5) and/or fresnel_strength high to make it visible.
uniform vec4  fresnel_color    : source_color = vec4(0.6, 0.85, 1.0, 1.0);
uniform float fresnel_power    : hint_range(0.5, 8.0) = 1.5;   // edge sharpness
uniform float fresnel_strength : hint_range(0.0, 1.0) = 0.6;   // overall intensity

// --- Terrain textures (sampled in world XZ space so they tile across tiles) --
uniform sampler2D mountain_texture : source_color, repeat_enable, filter_linear_mipmap;
uniform sampler2D forest_texture   : source_color, repeat_enable, filter_linear_mipmap;
// World units per texture tile. Larger = texture stretched over more ground.
uniform float terrain_uv_scale = 1.0;

// --- Bit constants ------------------------------------------------------------
// Local direction bits (within one 8-dir group, base offset 0).
const uint DIR_N  = 1u << 0u;
const uint DIR_E  = 1u << 1u;
const uint DIR_S  = 1u << 2u;
const uint DIR_W  = 1u << 3u;
const uint DIR_NE = 1u << 4u;
const uint DIR_SE = 1u << 5u;
const uint DIR_SW = 1u << 6u;
const uint DIR_NW = 1u << 7u;

// Group base shifts.
const uint MAIN_SHIFT  = 0u;
const uint ALT_SHIFT   = 8u;
const uint RIVER_SHIFT = 16u;

// All river bits set => ocean tile.
const uint RIVER_ALL = 0xFFu << RIVER_SHIFT;

// Terrain half-cover bits (cardinal only).
const uint MTN_N = 1u << 24u;
const uint MTN_ALL = 0xFu << 24u; // bits 24-27

const uint FOR_N = 1u << 28u;
const uint FOR_ALL = 0xFu << 28u; // bits 28-31

bool is_set(uint mask, uint bit) {
    return (mask & bit) != 0u;
}

// --- Gradient noise (Inigo Quilez - iq/2013) ---------------------------------
// https://www.shadertoy.com/view/XdXGW8
vec2 random2(vec2 st){
    st = vec2( dot(st,vec2(127.1,311.7)),
              dot(st,vec2(269.5,183.3)) );
    return -1.0 + 2.0*fract(sin(st)*43758.5453123);
}
float noise(vec2 st) {
    vec2 i = floor(st);
    vec2 f = fract(st);
    vec2 u = f*f*(3.0-2.0*f);
    return mix( mix( dot( random2(i + vec2(0.0,0.0) ), f - vec2(0.0,0.0) ),
                     dot( random2(i + vec2(1.0,0.0) ), f - vec2(1.0,0.0) ), u.x),
                mix( dot( random2(i + vec2(0.0,1.0) ), f - vec2(0.0,1.0) ),
                     dot( random2(i + vec2(1.0,1.0) ), f - vec2(1.0,1.0) ), u.x), u.y);
}

// --- View-angle Fresnel -------------------------------------------------------
// dot(normal, view) is 1 facing the camera, ~0 at grazing angles, so
// (1 - dot)^power is the classic Fresnel falloff that brightens the edges.
// `normal` and `view` are expected in the same space (Godot's view space).
float fresnel(vec3 normal, vec3 view, float power) {
    float ndv = clamp(dot(normalize(normal), normalize(view)), 0.0, 1.0);
    return pow(1.0 - ndv, power);
}

// Resolve which single 8-dir region (as a low-byte DIR_* bit) the pixel falls
// into for a given border thickness. 3x3 grid: 4 corners, 4 edges, 1 center.
// Returns 0u in the center (no border).
uint region_bit(vec2 uv, float t) {
    bool top    = uv.y < t;
    bool bottom = uv.y > 1.0 - t;
    bool left   = uv.x < t;
    bool right  = uv.x > 1.0 - t;
    if (top && left)     { return DIR_NW; }
    if (top && right)    { return DIR_NE; }
    if (bottom && left)  { return DIR_SW; }
    if (bottom && right) { return DIR_SE; }
    if (top)             { return DIR_N;  }
    if (bottom)          { return DIR_S;  }
    if (left)            { return DIR_W;  }
    if (right)           { return DIR_E;  }
    return 0u;
}

// True if the pixel lies in any half-cover selected by the 4 cardinal bits of a
// terrain group. `base_n` is that group's North bit (E/S/W are <<1, <<2, <<3).
// Opposite halves are split at exactly 0.5 so N+S (or E+W) unions to full tile.
bool half_cover(uint mask, uint base_n, vec2 uv) {
    if (is_set(mask, base_n << 0u) && uv.y <  0.5) { return true; } // N: top
    if (is_set(mask, base_n << 1u) && uv.x >= 0.5) { return true; } // E: right
    if (is_set(mask, base_n << 2u) && uv.y >= 0.5) { return true; } // S: bottom
    if (is_set(mask, base_n << 3u) && uv.x <  0.5) { return true; } // W: left
    return false;
}

varying vec3 world_pos;

void vertex() {
    world_pos = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
}

void fragment() {
    vec2 uv = UV; // per-tile UV: (0,0) top-left .. (1,1) bottom-right.
    bool ocean = (tile_mask & RIVER_ALL) == RIVER_ALL;

    // Tracks how much of the final pixel is water (0 = none, 1 = full water).
    // Used to scale the Fresnel so it only touches water and fades across the
    // river/border blend.
    float water_amount = 0.0;

    // ---- Base fill + terrain overlays ----------------------------------------
    vec3 col = albedo_color.rgb;

    if (ocean) {
        col = water_color.rgb;
        water_amount = 1.0;
    } else {
        vec2 t_uv = world_pos.xz * terrain_uv_scale;

        // Forests first, mountains composited on top.
        // The any_* guards are uniform branches, so tiles with no terrain pay
        // nothing; the texture fetch stays inside the uniform block so its
        // derivatives (mip selection) remain well-defined.
        if (is_set(tile_mask, FOR_ALL)) {
            float cover = half_cover(tile_mask, FOR_N, uv) ? 1.0 : 0.0;
            vec4 f = texture(forest_texture, t_uv);
            col = mix(col, f.rgb, f.a * cover);
        }
        if (is_set(tile_mask, MTN_ALL)) {
            float cover = half_cover(tile_mask, MTN_N, uv) ? 1.0 : 0.0;
            vec4 m = texture(mountain_texture, t_uv);
            col = mix(col, m.rgb, m.a * cover);
        }
    }

    // ---- Border / river overlay (drawn on top of fill & terrain) -------------
    uint main_region = region_bit(uv, border_thickness);     // main + river share
    uint alt_region  = region_bit(uv, alt_border_thickness);

    bool main_set  = is_set(tile_mask, main_region << MAIN_SHIFT);
    bool alt_set   = is_set(tile_mask, alt_region  << ALT_SHIFT);
    bool river_set = is_set(tile_mask, main_region << RIVER_SHIFT);

    if (ocean) {
        // All water already; just lay borders on top (main wins over alt).
        if (main_set)     
        {
            if (noise(world_pos.xz * 5.0) > 0.1)
            {
                col = border_color.rgb; water_amount = 0.0; 
            }
            else
            {
                col = water_color.rgb; water_amount = 1.0;
            }
        }
        else if (alt_set) 
        {
            col = alt_border_color.rgb; water_amount = 0.0; 
         }
    } else {
        if (main_set && river_set) {
            col = mix(border_color.rgb, water_color.rgb, river_border_blend);
            water_amount = river_border_blend;
        } else if (main_set) {
            col = border_color.rgb;       // water_amount stays 0
        } else if (river_set) {
            col = water_color.rgb;        // river overwrites the alt border
            water_amount = 1.0;
        } else if (alt_set) {
            col = alt_border_color.rgb;   // water_amount stays 0
        }
    }

    // ---- Water Fresnel (view-angle rim, water pixels only) -------------------
    if (water_amount > 0.0) {
        float f = fresnel(NORMAL, VIEW, fresnel_power);
        col = mix(col, fresnel_color.rgb, f * fresnel_strength * water_amount);
    }

    ALBEDO = col;
}