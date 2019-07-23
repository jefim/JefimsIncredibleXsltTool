using System.Windows.Media;

namespace JefimsIncredibleXsltTool.Lib
{
    public class ColorTheme
    {
        public Brush Background { get; set; } = BrushFromHex("#2D2D30");
        public Brush Foreground { get; set; } = BrushFromHex("#EEEEEE");
        public Brush EditorForeground { get; set; } = BrushFromHex("#C8C8C8");
        public Brush EditorBackground { get; set; } = BrushFromHex("#1E1E1E");
        public Brush EditorReadOnlyBackground { get; set; } = BrushFromHex("#252526");
        public Brush ScrollBarForeground { get; set; } = BrushFromHex("#686868");
        public Brush ScrollBarBackground { get; set; } = BrushFromHex("#3E3E42");
        public Brush MenuForeground { get; set; } = BrushFromHex("#EEEEEE");
        public Brush MenuBackground { get; set; } = BrushFromHex("#2D2D30");
        public Brush ButtonForeground { get; set; } = BrushFromHex("#EEEEEE");
        public Brush ButtonBackground { get; set; } = BrushFromHex("#333333");
        public Brush FoldingLines { get; set; } = BrushFromHex("#666666");
        public Brush FoldingBackground { get; set; } = BrushFromHex("#111111");
        public Brush SelectedFoldingLines { get; set; } = BrushFromHex("#999999");
        public Brush SelectedFoldingBackground { get; set; } = BrushFromHex("#777777");

        public static readonly ColorTheme DarkColorTheme = new ColorTheme
        {
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
            SelectedFoldingBackground = BrushFromHex("#777777")
        };

        public static readonly ColorTheme LightColorTheme = new ColorTheme
        {
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
            SelectedFoldingBackground = BrushFromHex("#FFFFFF")
        };

        public static Brush BrushFromHex(string hex)
        {
            var color = (Color)ColorConverter.ConvertFromString(hex);
            return new SolidColorBrush(color);
        }
    }
}
