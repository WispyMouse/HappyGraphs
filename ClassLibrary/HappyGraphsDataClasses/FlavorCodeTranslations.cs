using System;

public static class FlavorCodeTranslations
{
    public static string ColorFromFlavorCode(CardData toGet)
    {
        return HexCodeFromInteger(toGet.FlavorCode);
    }

    public static string GetSeedColor(Random randomEngine)
    {
        int r = randomEngine.Next(256);
        int g = randomEngine.Next(256);
        int b = randomEngine.Next(256);
        return HexCodeFromColors(r, g, b);
    }

    public static string GetShiftedHue(string colorCode, int steps, int maximum)
    {
        int adjustedSteps = steps % maximum;
        int huePerStep = (int)(360f / (float)maximum);

        int r = IntegerFromHexCode(colorCode.Substring(0, 2));
        int g = IntegerFromHexCode(colorCode.Substring(2, 2));
        int b = IntegerFromHexCode(colorCode.Substring(4, 2));

        int hue;
        float saturation, value;
        GetHSV(r, g, b, out hue, out saturation, out value);
        hue = (hue + (adjustedSteps * huePerStep)) % 360;

        GetRGB(hue, saturation, value, out r, out g, out b);
        return HexCodeFromColors(r, g, b);
    }

    public static int IntegerFromHexCode(string hex)
    {
        return int.Parse(hex, System.Globalization.NumberStyles.HexNumber);
    }

    public static string HexCodeFromInteger(int integer)
    {
        return integer.ToString("X6");
    }

    public static string HexCodeFromColors(int r, int g, int b)
    {
        return $"{r.ToString("X2")}{g.ToString("X2")}{b.ToString("X2")}";
    }

    static void GetHSV(int r, int g, int b, out int hue, out float saturation, out float value)
    {
        float fR = r / 255f;
        float fG = g / 255f;
        float fB = b / 255f;

        float cmax = Math.Max(fR, Math.Max(fG, fB));
        float cmin = Math.Min(fR, Math.Min(fG, fB));
        float diff = cmax - cmin;

        if (cmax == cmin)
        {
            hue = 0;
        }
        else if (cmax == fR)
        {
            hue = (int)(60f * ((fG - fB) / diff) + 360f) % 360;
        }
        else if (cmax == fG)
        {
            hue = (int)(60f * ((fB - fR) / diff) + 120f) % 360;
        }
        else // must be cmax == b
        {
            hue = (int)(60f * ((fR - fG) / diff) + 240f) % 360;
        }

        if (cmax == 0)
        {
            saturation = 0;
        }
        else
        {
            saturation = diff / cmax;
        }

        value = cmax;
    }

    static void GetRGB(int hue, float saturation, float value, out int r, out int g, out int b)
    {
        float chroma = value * saturation;
        float flattenedHue = (float)hue / 60f;
        float intermediate = chroma * (1f - (flattenedHue % 2 - 1f));

        float r1, g1, b1;

        if (flattenedHue <= 1)
        {
            r1 = chroma;
            g1 = intermediate;
            b1 = 0;
        }
        else if (flattenedHue <= 2)
        {
            r1 = intermediate;
            g1 = chroma;
            b1 = 0;
        }
        else if (flattenedHue <= 3)
        {
            r1 = 0;
            g1 = chroma;
            b1 = intermediate;
        }
        else if (flattenedHue <= 4)
        {
            r1 = 0;
            g1 = intermediate;
            b1 = chroma;
        }
        else if (flattenedHue <= 5)
        {
            r1 = intermediate;
            g1 = 0;
            b1 = chroma;
        }
        else // <= 6
        {
            r1 = chroma;
            g1 = 0;
            b1 = intermediate;
        }

        float m = value - chroma;
        r = (int)((r1 + m) * 255f);
        g = (int)((g1 + m) * 255f);
        b = (int)((b1 + m) * 255f);
    }
}