using UnityEngine;

namespace UI
{
    public static class ColorPaletteContainer
    {
        public static readonly Color UI_Background = HexToColor("#FFF8E8");

        public static readonly Color UI_PureBlack = Color.black;
    
        public static readonly Color UI_Black = HexToColor("#461201");

        public static readonly Color UI_Highlight = HexToColor("#f45f05");

        private static Color HexToColor(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out var color))
                return color;

            Debug.LogWarning($"Failed to parse color from hex: {hex}");
            return Color.magenta;
        }
    }
}
