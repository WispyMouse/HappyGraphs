using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ConversionTools
{
    public static UnityEngine.Color UnityColorFromSystemDrawing(System.Drawing.Color baseColor)
    {
        return new Color(baseColor.R, baseColor.G, baseColor.B, baseColor.A);
    }
}
