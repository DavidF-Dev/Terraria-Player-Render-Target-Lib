/*
 *  PlayerRenderTargetExtensions.cs
 *  DavidFDev
 */

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PlayerRenderTargetLib.Code.Internals;
using Terraria;

namespace PlayerRenderTargetLib.Code;

/// <summary>
///     Extension methods for retrieving a player's render target.
/// </summary>
public static class PlayerRenderTargetExtensions
{
    #region Static Methods

    public static bool TryGetRenderTarget(this Player _, out RenderTarget2D target)
    {
        if (!PlayerRenderTargetSystem.CanUseTarget)
        {
            target = null;
            return false;
        }

        target = PlayerRenderTargetSystem.Target;
        return true;
    }

    public static bool CanUseRenderTarget(this Player _)
    {
        return PlayerRenderTargetSystem.CanUseTarget;
    }

    public static RenderTarget2D GetRenderTarget(this Player _)
    {
        return PlayerRenderTargetSystem.Target;
    }

    public static Rectangle GetRenderTargetSourceRectangle(this Player player)
    {
        return PlayerRenderTargetSystem.GetTargetSourceRectangle(player.whoAmI);
    }

    public static Vector2 GetRenderTargetPosition(this Player player)
    {
        return PlayerRenderTargetSystem.GetTargetPosition(player.whoAmI);
    }

    #endregion
}