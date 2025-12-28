namespace LabelChest {
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
        /// Resets the configuration to default values.
        /// </summary>
        public void ResetToDefaults() {
            FontSize = 1.0f;
        }
    }
}
