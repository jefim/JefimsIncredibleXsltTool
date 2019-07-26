using System.Collections.Generic;
using System.Windows.Media;

namespace JefimsIncredibleXsltTool.Lib
{
    public class ColorTheme
    {
        public string Id { get; set; }
        public string AvalonXmlHighlightResourceName { get; set; }
        public string IconSymbol { get; set; }
        public Color IconHighlightColor { get; set; }
        public Brush Background { get; set; }
        public Brush Foreground { get; set; }
        public Brush EditorForeground { get; set; }
        public Brush EditorBackground { get; set; }
        public Brush EditorReadOnlyBackground { get; set; }
        public Brush ScrollBarForeground { get; set; }
        public Brush ScrollBarBackground { get; set; }
        public Brush MenuForeground { get; set; }
        public Brush MenuBackground { get; set; }
        public Brush ButtonForeground { get; set; }
        public Brush ButtonBackground { get; set; }
        public Brush FoldingLines { get; set; }
        public Brush FoldingBackground { get; set; }
        public Brush SelectedFoldingLines { get; set; }
        public Brush SelectedFoldingBackground { get; set; }
        public Brush Hyperlink { get; set; }

        public static readonly ColorTheme DarkColorTheme = new ColorTheme
        {
            Id = "VS Dark",
            AvalonXmlHighlightResourceName = "JefimsIncredibleXsltTool.Resources.AvalonXmlDarkTheme.xml",
            IconSymbol = "👻",
            IconHighlightColor = Colors.PaleTurquoise,
            Background = BrushFromHex("#2D2D30"),
            Foreground = BrushFromHex("#EEEEEE"),
            EditorForeground = BrushFromHex("#C8C8C8"),
            EditorBackground = BrushFromHex("#1E1E1E"),
            EditorReadOnlyBackground = BrushFromHex("#252526"),
            ScrollBarForeground = BrushFromHex("#686868"),
            ScrollBarBackground = BrushFromHex("#3E3E42"),
            MenuForeground = BrushFromHex("#EEEEEE"),
            MenuBackground = BrushFromHex("#2D2D30"),
            ButtonForeground = BrushFromHex("#EEEEEE"),
            ButtonBackground = BrushFromHex("#333333"),
            FoldingLines = BrushFromHex("#666666"),
            FoldingBackground = BrushFromHex("#111111"),
            SelectedFoldingLines = BrushFromHex("#999999"),
            SelectedFoldingBackground = BrushFromHex("#777777"),
            Hyperlink = BrushFromHex("#8B9ED3")
        };

        public static readonly ColorTheme LightColorTheme = new ColorTheme
        {
            Id = "VS Light",
            AvalonXmlHighlightResourceName = "JefimsIncredibleXsltTool.Resources.AvalonXmlLightTheme.xml",
            IconSymbol = "💡",
            IconHighlightColor = Colors.Gold,
            Background = BrushFromHex("#EEEEF2"),
            Foreground = BrushFromHex("#1E1E1E"),
            EditorForeground = BrushFromHex("#000000"),
            EditorBackground = BrushFromHex("#FFFFFF"),
            EditorReadOnlyBackground = BrushFromHex("#E6E7E8"),
            ScrollBarForeground = BrushFromHex("#C2C3C9"),
            ScrollBarBackground = BrushFromHex("#F5F5F5"),
            MenuForeground = BrushFromHex("#1E1E1E"),
            MenuBackground = BrushFromHex("#EEEEF2"),
            ButtonForeground = BrushFromHex("#1E1E1E"),
            ButtonBackground = BrushFromHex("#DDDDDD"),
            FoldingLines = BrushFromHex("#A5A5A5"),
            FoldingBackground = BrushFromHex("#FFFFFF"),
            SelectedFoldingLines = BrushFromHex("#CCCCCC"),
            SelectedFoldingBackground = BrushFromHex("#FFFFFF"),
            Hyperlink = BrushFromHex("#0000FF")
        };

        public static readonly List<ColorTheme> ColorThemes = new List<ColorTheme> { LightColorTheme, DarkColorTheme };

        public static readonly ColorTheme DefaultColorTheme = ColorTheme.LightColorTheme;

        public static Brush BrushFromHex(string hex)
        {
            var color = (Color)ColorConverter.ConvertFromString(hex);
            return new SolidColorBrush(color);
        }
    }
}
