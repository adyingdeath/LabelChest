using Microsoft.Xna.Framework;

namespace LabelChest {
    public enum TextColorType {
        Fixed,
        Inverted,
        FollowBox
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
        public TextColorType TextColorType { get; set; } = TextColorType.Fixed;
        /// <summary>
        /// Text color. Default is white.
        /// Value can be:
        /// 1. Color object
        /// 2. {}
        /// </summary>
        public Color TextColor { get; set; } = Color.White;

        /// <summary>
        /// Resets the configuration to default values.
        /// </summary>
        public void ResetToDefaults() {
            FontSize = 1.0f;
            TextColorType = TextColorType.Fixed;
            TextColor = Color.White;
        }
    }
}
