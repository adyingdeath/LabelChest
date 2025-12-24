using System;
using StardewValley;
using StardewValley.Objects;

namespace LabelChest.Managers {
    /// <summary>
    /// Manages chest label data storage and retrieval.
    /// </summary>
    public class ChestLabelManager {
        private const string LabelKey = "LabelChest.label";

        /// <summary>
        /// Gets the label for a chest.
        /// </summary>
        public static string GetLabel(Chest chest) {
            return chest.modData.TryGetValue(LabelKey, out string label) ? label : string.Empty;
        }

        /// <summary>
        /// Sets or clears a label for a chest.
        /// </summary>
        public static void SetLabel(Chest chest, string label) {
            if (string.IsNullOrWhiteSpace(label)) {
                chest.modData.Remove(LabelKey);
            }
            else {
                chest.modData[LabelKey] = label;
            }
        }

        /// <summary>
        /// Checks if a chest has a label.
        /// </summary>
        public static bool HasLabel(Chest chest) {
            return chest.modData.ContainsKey(LabelKey);
        }
    }
}