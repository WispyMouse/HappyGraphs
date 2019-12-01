﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ConversionTools
{
    public static Color ColorFromHexCode(string hexCode)
    {
        int r = int.Parse(hexCode.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
        int g = int.Parse(hexCode.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
        int b = int.Parse(hexCode.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);
        return new Color((float)r / 255f, (float)g / 255f, (float)b / 255f);
    }
}
