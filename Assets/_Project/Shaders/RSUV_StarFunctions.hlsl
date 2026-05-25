#ifndef RSUV_STAR_FUNCTIONS_INCLUDED
#define RSUV_STAR_FUNCTIONS_INCLUDED

// Reads Unity's Renderer Shader User Value (uint) and decodes ARGB-like packing:
// A: HDR intensity flag/amount, R/G/B: color
void DecodeStarRSUV_float(out float3 Color, out float EmissionScale, out float Alpha)
{
    uint data = unity_RendererUserValue;

    // Fallback when no RSUV value was set
    if (data == 0u)
    {
        Color = float3(1.0, 1.0, 1.0);
        EmissionScale = 1.0;
        Alpha = 1.0;
        return;
    }

    float r = ((data >> 16) & 0xFFu) / 255.0;
    float g = ((data >> 8)  & 0xFFu) / 255.0;
    float b = ( data        & 0xFFu) / 255.0;
    float a = ((data >> 24) & 0xFFu) / 255.0;

    // Matches current behavior: 1..10 multiplier from A
    EmissionScale = lerp(1.0, 10.0, a);
    Color = float3(r, g, b) * EmissionScale;
    Alpha = 1.0;
}

#endif