﻿using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace gameoff2018
{
    class OpenGlContext : IDisposable
    {
        Dictionary<int, TexObject> texObjects = new Dictionary<int, TexObject>
        {
            {Constants.TEX_ID_LAVA_BOMB, new TexObject(@"assets\lava-bomb.png")},
            {Constants.TEX_ID_ROCK, new TexObject(@"assets\tile.png")},
            {Constants.TEX_ID_BG, new TexObject(@"assets\bg1.png")},
            {Constants.TEX_ID_STANDING, new TexObject(@"assets\sprite-standing.png")},
            {Constants.TEX_ID_SPITTER, new TexObject(@"assets\spitter.png")},
            {Constants.TEX_ID_BULLET, new TexObject(@"assets\lava-bullet-2.png")},
            {Constants.TEX_ID_FLAG_RED, new TexObject(@"assets\flag-red.png")},
            {Constants.TEX_ID_FLAG_WHITE, new TexObject(@"assets\flag-white.png")},
            {Constants.TEX_ID_FLAME_SPITTER, new TexObject(@"assets\spitter-flame.png")}
        };
        Dictionary<int, SpriteTexObject> spriteTexObjects = new Dictionary<int, SpriteTexObject>
        {
            { Constants.TEX_ID_SPRITE_SUIT, new SpriteTexObject(@"assets\sprite-suit.png", 256, 8)},
            { Constants.TEX_ID_SPRITE_FONT, new SpriteTexObject(@"assets\sprite-font.png", 32, 95)},
            { Constants.TEX_ID_SPRITE_LAVA_LAKE, new SpriteTexObject(@"assets\sprite-lava-lake.png", 128, 2)},
            { Constants.TEX_ID_SPRITE_LAVA_SURFACE, new SpriteTexObject(@"assets\sprite-lava-surface.png", 128, 2)},
            { Constants.TEX_ID_SPRITE_FLAMES_BIG, new SpriteTexObject(@"assets\sprite-flames-big.png", 256, Constants.SPRITE_FLAMES_FRAMES)}
        };
        ActiveLevel Level = null;

        public OpenGlContext(ActiveLevel level)
        {
            Level = level;

            Color4 backColor;
            backColor.A = 1.0f;
            backColor.R = 0.5f;
            backColor.G = 0.5f;
            backColor.B = 0.5f;
            GL.ClearColor(backColor);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            GL.Enable(EnableCap.Texture2D);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

            foreach (var texObject in texObjects.Values)
                texObject.GlInit();
            foreach (var texObject in spriteTexObjects.Values)
                texObject.GlInit();
        }

        public void RenderFrame(int width, int height)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.MatrixMode(MatrixMode.Modelview);

            RenderLevel(width, height);
        }

        public void RenderVictoryScreen(int screenWidth, int screenHeight)
        {
            GL.LoadIdentity();
            string victoryText1 = "!!! VICTORY !!!";
            string victoryText2 = "You escaped from the volcano.";
            RenderString(screenWidth / 2 - (victoryText1.Length * (32 - 14)) / 2, screenHeight / 2, victoryText1);
            RenderString(screenWidth / 2 - (victoryText2.Length * (32 - 14)) / 2, screenHeight / 2 - 40, victoryText2);

            // Render sprite suit.
            {
                GL.PushMatrix();
                GL.Translate(200.0, 200.0, 0.0);

                {
                    if (spriteTexObjects.TryGetValue(Constants.TEX_ID_SPRITE_SUIT, out SpriteTexObject suitTexObject))
                    {
                        int frameToRender = (int)(Level.SpriteAnimationPosition * suitTexObject.TexCount);
                        if (frameToRender < 0)
                            frameToRender = 0;
                        if (frameToRender >= suitTexObject.TexCount)
                            frameToRender = suitTexObject.TexCount - 1;
                        suitTexObject.GlRenderFromCorner(Constants.SPRITE_SUIT_SIZE, frameToRender, Level.facing == CharacterFacing.Right);
                    }
                }
                GL.PopMatrix();
            }
        }

        public void RenderLevel(int screenWidth, int screenHeight)
        {
            if (Level.GameWon)
            {
                RenderVictoryScreen(screenWidth, screenHeight);
                return;
            }

            double scaleFactor = Level.WorldToScreenScaleFactor(screenWidth);

            // Render background.
            GL.LoadIdentity();
            GL.Scale(scaleFactor * 0.5, scaleFactor * 0.5, 1.0);
            // account for border
            GL.Translate(Constants.BG_TILE_SIZE, Constants.BG_TILE_SIZE, 0.0);
            // if they are more than x tiles up
            if (Level.McPosition.Y > Constants.TILE_SIZE * 12)
            {
                // character appears some distance from the bottom of the screen
                GL.Translate(0.0, Constants.TILE_SIZE * 12, 0.0);
                // account for character position in level
                GL.Translate(0.0, -Level.McPosition.Y, 0.0);
            }

            foreach (int x in Enumerable.Range(-1, Constants.LEVEL_WIDTH + 2))
                foreach (int y in Enumerable.Range(-1, Constants.LEVEL_HEIGHT + 2))
                {
                    GL.PushMatrix();
                    {
                        GL.Translate(Constants.BG_TILE_SIZE * x, Constants.BG_TILE_SIZE * y, 0);

                        if (texObjects.TryGetValue(Constants.TEX_ID_BG, out TexObject tileTexObject))
                            tileTexObject.GlRenderFromCorner(Constants.BG_TILE_SIZE);
                    }
                    GL.PopMatrix();
                }

            // Set up matrices for foreground objects.
            GL.LoadIdentity();
            GL.Scale(scaleFactor, scaleFactor, 1.0);

            Vector2d offset = Level.GetWorldToScreenOffset();
            // account for border
            GL.Translate(offset.X, offset.Y, 0.0);

            // Render lava bombs.
            foreach (LavaBombEntity lavaBomb in Level.LavaBombs)
            {
                GL.PushMatrix();
                {
                    GL.Translate(lavaBomb.Position.X, lavaBomb.Position.Y, 0);
                    GL.Rotate(Util.RadiansToDegrees(Level.Angle), 0, 0, 1);
                    switch (lavaBomb.Level)
                    {
                        case 1:
                            if (texObjects.TryGetValue(Constants.TEX_ID_BULLET, out TexObject bulletTexObject))
                                bulletTexObject.GlRenderFromMiddle(Constants.LAVA_BULLET_SIZE);
                            break;
                        case 2:
                        default:
                            if (texObjects.TryGetValue(Constants.TEX_ID_LAVA_BOMB, out TexObject bombTexObject))
                                bombTexObject.GlRenderFromMiddle(Constants.LAVA_BOMB_SIZE);
                            break;
                    }
                }
                GL.PopMatrix();
            }

            int lavaFrameToRender = (int)(Level.LavaAnimationLoopValue * Constants.LAVA_LAKE_SPRITE_FRAMES);
            if (lavaFrameToRender < 0)
                lavaFrameToRender = 0;
            if (lavaFrameToRender >= Constants.LAVA_LAKE_SPRITE_FRAMES)
                lavaFrameToRender = Constants.LAVA_LAKE_SPRITE_FRAMES - 1;

            // Determine how many lava lake tiles to render along Y.
            // Involves lava height from bottom of worldviewport (world coords) and tile width - 1 (the surface).
            double bottomOfViewInWorldCoords = 0;
            double lavaHeightInView = Level.LavaHeight - bottomOfViewInWorldCoords;
            // this value + 1 to round up instead of down, + 1 border tile, and - 1 surface tile
            int tilesInView = (int)(lavaHeightInView / Constants.TILE_SIZE);
            if (tilesInView < 0)
                tilesInView = 0;

            // Render lava surface.
            foreach (int x in Enumerable.Range(-1, Constants.LEVEL_WIDTH + 2))
            {
                GL.PushMatrix();
                {
                    GL.Translate(x * Constants.LAVA_SURFACE_SPRITE_SIZE, Level.LavaHeight - Constants.LAVA_SURFACE_SPRITE_SIZE, 0);
                    if (spriteTexObjects.TryGetValue(Constants.TEX_ID_SPRITE_LAVA_SURFACE, out SpriteTexObject lavaSurfaceTexObject))
                        lavaSurfaceTexObject.GlRenderFromCorner(Constants.LAVA_SURFACE_SPRITE_SIZE, lavaFrameToRender);
                }
                GL.PopMatrix();
            }

            // Render lava lake.
            foreach (int x in Enumerable.Range(-1, Constants.LEVEL_WIDTH + 2))
                for (int y = 0; y < tilesInView; y++)
                {
                    GL.PushMatrix();
                    {
                        GL.Translate(x * Constants.LAVA_LAKE_SPRITE_SIZE, Level.LavaHeight - (y + 2) * Constants.LAVA_LAKE_SPRITE_SIZE, 0);

                        if (spriteTexObjects.TryGetValue(Constants.TEX_ID_SPRITE_LAVA_LAKE, out SpriteTexObject spriteLavaLakeTexObject))
                            spriteLavaLakeTexObject.GlRenderFromCorner(Constants.LAVA_LAKE_SPRITE_SIZE, lavaFrameToRender);
                    }
                    GL.PopMatrix();
                }

            // Render tiles.
            foreach (int x in Enumerable.Range(-1, Constants.LEVEL_WIDTH + 2))
                foreach (int y in Enumerable.Range(-1, Constants.LEVEL_HEIGHT + 2))
                {
                    GL.PushMatrix();
                    {
                        GL.Translate(Constants.TILE_SIZE * x, Constants.TILE_SIZE * y, 0);
                        int textureToUse = -1;
                        
                        if
                        (
                            x < 0
                            || x >= Constants.LEVEL_WIDTH
                            || y < 0
                            || y >= Constants.LEVEL_HEIGHT
                        )
                        {
                            textureToUse = Constants.TEX_ID_ROCK;
                        }
                        else
                        {
                            switch (Level.Tiles[x, y])
                            {
                                case Constants.TILE_ID_ROCK:
                                    textureToUse = Constants.TEX_ID_ROCK;
                                    break;
                                case Constants.TILE_ID_SPITTER:
                                    textureToUse = Constants.TEX_ID_SPITTER;
                                    break;
                                case Constants.TILE_ID_FLAG_RED:
                                    textureToUse = Constants.TEX_ID_FLAG_RED;
                                    break;
                                case Constants.TILE_ID_FLAG_WHITE:
                                    textureToUse = Constants.TEX_ID_FLAG_WHITE;
                                    break;
                                case Constants.TILE_ID_FLAME_SPITTER:
                                    textureToUse = Constants.TEX_ID_FLAME_SPITTER;
                                    break;
                                case Constants.TILE_ID_EMPTY:
                                default:
                                    break;
                            }
                        }

                        if (texObjects.TryGetValue(textureToUse, out TexObject tileTexObject))
                            tileTexObject.GlRenderFromCorner(Constants.TILE_SIZE, false);
                    }
                    GL.PopMatrix();
                }

                if (Level.FlameSpitterLoopValue > 0.33)
                {
                    // Render flames
                    foreach (int x in Enumerable.Range(-1, Constants.LEVEL_WIDTH + 2))
                        foreach (int y in Enumerable.Range(-1, Constants.LEVEL_HEIGHT + 2))
                        {
                            GL.PushMatrix();
                            {
                                GL.Translate(Constants.TILE_SIZE * x - Constants.SPRITE_FLAMES_SIZE / 4, Constants.TILE_SIZE * (y + 1), 0);

                                if
                                (!(
                                    x < 0
                                    || x >= Constants.LEVEL_WIDTH
                                    || y < 0
                                    || y >= Constants.LEVEL_HEIGHT
                                ))
                                {
                                    if (Level.Tiles[x, y] == Constants.TILE_ID_FLAME_SPITTER)
                                    {
                                        if (spriteTexObjects.TryGetValue(Constants.TEX_ID_SPRITE_FLAMES_BIG, out SpriteTexObject flamesTexObject))
                                            flamesTexObject.GlRenderFromCorner(Constants.SPRITE_FLAMES_SIZE, Level.FlamesLoopValue > 0.5 ? 0 : 1, false);
                                    }
                                }
                            }
                            GL.PopMatrix();
                        }
                }

            // Render sprite suit.
            {
                GL.PushMatrix();
                GL.Translate(Level.McPosition.X, Level.McPosition.Y, 0);

                {
                    if (!Level.McRunning)
                    {
                        if (texObjects.TryGetValue(Constants.TEX_ID_STANDING, out TexObject standTexObject))
                            standTexObject.GlRenderFromCorner(Constants.SPRITE_SUIT_SIZE, Level.facing == CharacterFacing.Right);
                    }
                    else
                    {
                        if (spriteTexObjects.TryGetValue(Constants.TEX_ID_SPRITE_SUIT, out SpriteTexObject suitTexObject))
                        {
                            int frameToRender = (int)(Level.SpriteAnimationPosition * suitTexObject.TexCount);
                            if (frameToRender < 0)
                                frameToRender = 0;
                            if (frameToRender >= suitTexObject.TexCount)
                                frameToRender = suitTexObject.TexCount - 1;
                            suitTexObject.GlRenderFromCorner(Constants.SPRITE_SUIT_SIZE, frameToRender, Level.facing == CharacterFacing.Right);
                        }
                    }
                }
                GL.PopMatrix();
            }

            GL.LoadIdentity();
            RenderString(0, screenHeight - 20, $"Level {Level.LevelNumber}" + (Level.EditorMode ? " (editor)" : ""), 16);
        }

        public void RenderString(double x, double y, string text, double size = Constants.TEXT_DEFAULT_HEIGHT)
        {
            GL.PushMatrix();
            {
                GL.Translate(x, y, 0);
                for (int i = 0; i < text.Length; ++i)
                {
                    if (spriteTexObjects.TryGetValue(Constants.TEX_ID_SPRITE_FONT, out SpriteTexObject spriteFontTexObject))
                        spriteFontTexObject.GlRenderFromCorner(size, CorrectIndex(text[i]));
                    GL.Translate(size - (Constants.TEXT_KERNING / Constants.TEXT_DEFAULT_HEIGHT * size), 0, 0);
                }
            }
            GL.PopMatrix();
        }

        public int CorrectIndex(int i)
        {
            char c = '?';
            if (i >= 32 && i <= 126)
                c = (char)i;
            return c - 32;
        }

        public void Dispose()
        {
            foreach (var texObject in texObjects.Values)
                texObject.Dispose();
            foreach (var texObject in spriteTexObjects.Values)
                texObject.Dispose();
        }
    }
}
