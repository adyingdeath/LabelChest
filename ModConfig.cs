using Microsoft.Xna.Framework;

namespace LabelChest {
    public enum TextColorType {
        Fixed,
        Inverted,
        FollowBox
    }

    public enum OutlineColorType {
        Fixed,
        Inverted
    }

    /// <summary>
    /// Configuration class for the LabelChest mod.
    /// </summary>
    public class ModConfig {
        /// <summary>
        /// Font size scaling factor for chest labels. Default is 1.0.
        /// Final scale = FontSize * WorldFontScale.
        /// </summary>
        public float FontSize { get; set; } = 1.0f;
        
        /// <summary>
        /// Whether to show the label only when the mouse/cursor is hovering over the chest.
        /// </summary>
        public bool HoverOnly { get; set; } = false;

        public TextColorType TextColorType { get; set; } = TextColorType.Fixed;
        /// <summary>
        /// Text color. Default is white.
        /// </summary>
        public Color TextColor { get; set; } = Color.White;
        public OutlineColorType OutlineColorType { get; set; } = OutlineColorType.Inverted;
        /// <summary>
        /// Outline color. Default is black.
        /// </summary>
        public Color OutlineColor { get; set; } = Color.Black;

        /// <summary>
        /// Resets the configuration to default values.
        /// </summary>
        public void ResetToDefaults() {
            FontSize = 1.0f;
            HoverOnly = false;
            TextColorType = TextColorType.Fixed;
            TextColor = Color.White;
            OutlineColorType = OutlineColorType.Inverted;
            OutlineColor = Color.Black;
        }
    }
}