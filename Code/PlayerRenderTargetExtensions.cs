/*
 *  PlayerRenderTargetExtensions.cs
 *  DavidFDev
 */

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayerRenderTargetLib.Code.Internals;
using Terraria;
using Terraria.ModLoader;

namespace PlayerRenderTargetLib.Code;

/// <summary>
///     Extension methods for retrieving a player's render target.
/// </summary>
public static class PlayerRenderTargetExtensions
{
    #region Static Methods

    public static bool TryGetRenderTarget(this Player _, out RenderTarget2D target)
    {
        var system = ModContent.GetInstance<PlayerRenderTargetSystem>();
        if (!system.CanUseTarget)
        {
            target = null;
            return false;
        }

        target = system.Target;
        return true;
    }

    public static bool CanUseRenderTarget(this Player _)
    {
        return ModContent.GetInstance<PlayerRenderTargetSystem>().CanUseTarget;
    }

    public static RenderTarget2D GetRenderTarget(this Player _)
    {
        return ModContent.GetInstance<PlayerRenderTargetSystem>().Target;
    }

    public static Rectangle GetRenderTargetSourceRectangle(this Player player)
    {
        return ModContent.GetInstance<PlayerRenderTargetSystem>().GetTargetSourceRectangle(player.whoAmI);
    }

    public static Vector2 GetRenderTargetPosition(this Player player)
    {
        return ModContent.GetInstance<PlayerRenderTargetSystem>().GetTargetPosition(player.whoAmI);
    }

    #endregion
}