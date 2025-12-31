using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LabelChest.Utils;

/// <summary>
/// Utility class for quickly switch SpriteBatch between different settings.
/// </summary>
public class SpriteBatchSwitcher {
    public static void SwitchDefault(SpriteBatch b) {
        b.End();
        b.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.PointClamp,
            DepthStencilState.None,
            RasterizerState.CullCounterClockwise,
            null,
            Matrix.Identity
        );
    }
    public static void SwitchAntiAliasing(SpriteBatch b) {
        b.End();
        b.Begin(
            sortMode: SpriteSortMode.Deferred,
            blendState: BlendState.AlphaBlend,
            samplerState: SamplerState.LinearClamp,
            depthStencilState: null,
            rasterizerState: null,
            effect: null,
            transformMatrix: null
        );
    }
}