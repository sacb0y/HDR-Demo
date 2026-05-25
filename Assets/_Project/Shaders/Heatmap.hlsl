/*
2026-05-23 AI-Tag
This was created with the help of Assistant, a Unity Artificial Intelligence product.
*/
void Heatmap_float(float3 InColor, float WhitePoint, out float3 OutColor)
{
    // 1: Calculate luminance in nits.
    // Assumes input is in a linear space (like scRGB) and scales by WhitePoint (default 80.0)
    float nits = dot(float3(0.2126, 0.7152, 0.0722), InColor) * WhitePoint;

    // 2: Define piecewise stops and colors based on Microsoft's HDR Heatmap spec
    float stops[9] = { 0.0, 3.16, 10.0, 31.6, 100.0, 316.0, 1000.0, 3160.0, 10000.0 };
    
    float3 colors[9] = {
        float3(0.0, 0.0, 0.0), // Stop 0: Black
        float3(0.0, 0.0, 1.0), // Stop 1: Blue
        float3(0.0, 1.0, 1.0), // Stop 2: Cyan
        float3(0.0, 1.0, 0.0), // Stop 3: Green
        float3(1.0, 1.0, 0.0), // Stop 4: Yellow
        float3(1.0, 0.2, 0.0), // Stop 5: Orange
        float3(1.0, 0.0, 0.0), // Stop 6: Red
        float3(1.0, 0.0, 1.0), // Stop 7: Magenta
        float3(1.0, 1.0, 1.0)  // Stop 8: White
    };

    // 3: Determine segment and calculate interpolated color
    if (nits <= stops[0]) 
    {
        OutColor = colors[0];
    } 
    else if (nits >= stops[8]) 
    {
        OutColor = colors[8];
    } 
    else 
    {
        // Piecewise linear interpolation
        OutColor = colors[0]; 
        for (int i = 0; i < 8; i++) 
        {
            if (nits >= stops[i] && nits < stops[i+1]) 
            {
                float t = (nits - stops[i]) / (stops[i+1] - stops[i]);
                OutColor = lerp(colors[i], colors[i+1], t);
                break;
            }
        }
    }
}

// Optimization for half precision
void Heatmap_half(half3 InColor, half WhitePoint, out half3 OutColor)
{
    float3 res;
    Heatmap_float((float3)InColor, (float)WhitePoint, res);
    OutColor = (half3)res;
}