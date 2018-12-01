using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
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
        };
        Dictionary<int, SpriteTexObject> spriteTexObjects = new Dictionary<int, SpriteTexObject>
        {
            { Constants.TEX_ID_SPRITE_SUIT, new SpriteTexObject(@"assets\sprite-suit.png", 256, 8)},
            { Constants.TEX_ID_SPRITE_FONT, new SpriteTexObject(@"assets\sprite-font.png", 32, 95)},
            { Constants.TEX_ID_SPRITE_LAVA_LAKE, new SpriteTexObject(@"assets\sprite-lava-lake.png", 128, 2)},
            { Constants.TEX_ID_SPRITE_LAVA_SURFACE, new SpriteTexObject(@"assets\sprite-lava-surface.png", 128, 2)}
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

        public void RenderLevel(int screenWidth, int screenHeight)
        {
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
                                case 1:
                                    textureToUse = Constants.TEX_ID_ROCK;
                                    break;
                                case 2:
                                    textureToUse = Constants.TEX_ID_SPITTER;
                                    break;
                                case 0:
                                default:
                                    break;
                            }
                        }

                        if (texObjects.TryGetValue(textureToUse, out TexObject tileTexObject))
                            tileTexObject.GlRenderFromCorner(Constants.TILE_SIZE);
                    }
                    GL.PopMatrix();
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

            // Render lava bombs.
            foreach (LavaBombEntity lavaBomb in Level.LavaBombs)
            {
                GL.PushMatrix();
                {
                    GL.Translate(lavaBomb.Position.X, lavaBomb.Position.Y, 0);
                    GL.Rotate(Util.RadiansToDegrees(Level.Angle), 0, 0, 1);
                    if (texObjects.TryGetValue(Constants.TEX_ID_LAVA_BOMB, out TexObject texObject))
                        texObject.GlRenderFromMiddle(Constants.LAVA_BOMB_SIZE);
                }
                GL.PopMatrix();
            }

            RenderString(300, 300, "How not to escape a volcano...");
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
                    GL.Translate(size - Constants.TEXT_KERNING, 0, 0);
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
