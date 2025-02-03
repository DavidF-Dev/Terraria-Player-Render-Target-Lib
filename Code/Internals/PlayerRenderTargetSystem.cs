/*
 *  PlayerRenderTargetSystem.cs
 *  DavidFDev
 */

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Light;
using Terraria.ModLoader;

namespace PlayerRenderTargetLib.Code.Internals;

// ReSharper disable once ClassNeverInstantiated.Global
[Autoload(Side = ModSide.Client)]
internal sealed class PlayerRenderTargetSystem : ModSystem
{
    #region Static Fields and Constants

    private static readonly int[] PlayerIndexLookup = new int[Main.maxPlayers];
    private static Point _sheetSquare;
    private static int _prevNumPlayers;
    private static Vector2 _oldPos;
    private static Vector2 _positionOffset;

    #endregion

    #region Static Methods

    public static Rectangle GetTargetSourceRectangle(int whoAmI)
    {
        return TryGetPlayerIndex(whoAmI, out var index) ? new Rectangle(index * _sheetSquare.X, 0, _sheetSquare.X, _sheetSquare.Y) : Rectangle.Empty;
    }

    public static Vector2 GetTargetPosition(int whoAmI)
    {
        var gravPosition = Main.ReverseGravitySupport(Main.player[whoAmI].position - Main.screenPosition);
        return gravPosition - new Vector2(_sheetSquare.X / 2f, _sheetSquare.Y / 2f);
    }

    public static Vector2 GetTargetPositionOffset(int whoAmI)
    {
        return TryGetPlayerIndex(whoAmI, out var index) ? new Vector2(index * _sheetSquare.X + _sheetSquare.X / 2f, _sheetSquare.Y / 2f) : Vector2.Zero;
    }

    private static bool TryGetPlayerIndex(int whoAmI, out int index)
    {
        index = whoAmI is >= 0 and < Main.maxPlayers ? PlayerIndexLookup[whoAmI] : -1;
        return index is >= 0 and < Main.maxPlayers;
    }

    private static void Draw()
    {
        // https://github.com/ProjectStarlight/StarlightRiver/blob/fb35df83489a4d840271e946ba38448037fe7cc6/Content/CustomHooks/Visuals.PlayerTarget.cs
        // https://github.com/stormytuna/GrapplingHookAlternatives/blob/main/Common/RenderTargets/PlayerRenderTarget.cs
        
        var activePlayerCount = Main.CurrentFrameFlags.ActivePlayersCount;
        if (activePlayerCount != _prevNumPlayers)
        {
            _prevNumPlayers = activePlayerCount;

            Target.Dispose();
            Target = new RenderTarget2D(Main.graphics.GraphicsDevice, _sheetSquare.Y * activePlayerCount, _sheetSquare.Y);

            var activeCount = 0;
            for (var i = 0; i < Main.maxPlayers; i++)
            {
                PlayerIndexLookup[i] = Main.player[i].active ? activeCount++ : -1;
            }
        }

        var oldTargets = Main.graphics.GraphicsDevice.GetRenderTargets();
        CanUseTarget = false;
        try
        {
            Main.graphics.GraphicsDevice.SetRenderTarget(Target);
            Main.graphics.GraphicsDevice.Clear(Color.Transparent);

            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.EffectMatrix);
            try
            {
                foreach (var player in Main.ActivePlayers)
                {
                    if (player.dye.Length == 0)
                    {
                        continue;
                    }

                    _oldPos = player.position;
                    var oldCenter = player.Center;
                    var oldMountedCenter = player.MountedCenter;
                    var oldScreen = Main.screenPosition;
                    var oldItemLocation = player.itemLocation;
                    var oldHeldProj = player.heldProj;
                    try
                    {
                        _positionOffset = GetTargetPositionOffset(player.whoAmI);
                        player.position = _positionOffset;
                        player.Center = oldCenter - _oldPos + _positionOffset;
                        player.itemLocation = oldItemLocation - _oldPos + _positionOffset;
                        player.MountedCenter = oldMountedCenter - _oldPos + _positionOffset;
                        player.heldProj = -1;
                        Main.screenPosition = Vector2.Zero;

                        Main.PlayerRenderer.DrawPlayer(Main.Camera, player, player.position, player.fullRotation, player.fullRotationOrigin);
                    }
                    finally
                    {
                        player.position = _oldPos;
                        player.Center = oldCenter;
                        Main.screenPosition = oldScreen;
                        player.itemLocation = oldItemLocation;
                        player.MountedCenter = oldMountedCenter;
                        player.heldProj = oldHeldProj;
                    }
                }
            }
            finally
            {
                Main.spriteBatch.End();
            }
        }
        finally
        {
            Main.graphics.GraphicsDevice.SetRenderTargets(oldTargets);
            CanUseTarget = true;
        }
    }

    private static void OnCheckMonoliths(On_Main.orig_CheckMonoliths orig)
    {
        orig.Invoke();
        if (!Main.gameMenu && Main.CurrentFrameFlags.ActivePlayersCount > 0)
        {
            Draw();
        }
    }

    private static Vector3 OnGetLightingEngineColour(On_LightingEngine.orig_GetColor orig, LightingEngine self, int x, int y)
    {
        return CanUseTarget ? orig.Invoke(self, x, y) : orig.Invoke(self, x + (int)((_oldPos.X - _positionOffset.X) / 16), y + (int)((_oldPos.Y - _positionOffset.Y) / 16));
    }

    #endregion

    #region Properties

    /// <summary>
    ///     Shared render target.
    /// </summary>
    public static RenderTarget2D Target { get; private set; }

    /// <summary>
    ///     Whether the render target can currently be used.
    /// </summary>
    public static bool CanUseTarget { get; private set; }

    #endregion

    #region Methods

    public override void Load()
    {
        if (Main.dedServ)
        {
            return;
        }

        Array.Fill(PlayerIndexLookup, -1);
        _sheetSquare = new Point(200, 300);
        _prevNumPlayers = -1;

        Main.QueueMainThreadAction(() => Target = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight));

        On_Main.CheckMonoliths += OnCheckMonoliths;
        On_LightingEngine.GetColor += OnGetLightingEngineColour;
    }

    #endregion
}
