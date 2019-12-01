using System;

public static class FlavorCodeTranslations
{
    public static string[] ColorHexCodes = { "#4735ff", "#ffe26a", "#08faa7", "#ff10bd" };

    public static string ColorFromFlavorCode(CardData toGet)
    {
        if (toGet.FlavorCode < 0 || toGet.FlavorCode > ColorHexCodes.Length)
        {
            return "#FFFFFF";
        }

        return ColorHexCodes[toGet.FlavorCode];
    }

    public static int GetRandomColorHexCodeIndex(Random randomEngine)
    {
        return randomEngine.Next(ColorHexCodes.Length);
    }
}