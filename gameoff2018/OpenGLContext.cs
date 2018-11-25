using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gameoff2018
{
    class OpenGlContext : IDisposable
    {
        Dictionary<int, TexObject> texObjects = new Dictionary<int, TexObject>
        {
            {Constants.TEX_ID_LAVA_BOMB, new TexObject(@"assets\lava-bomb.png")},
            {Constants.TEX_ID_TILE, new TexObject(@"assets\tile.png")},
            {Constants.TEX_ID_BG, new TexObject(@"assets\bg1.png")},
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

        public void RenderFrame()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.MatrixMode(MatrixMode.Modelview);

            RenderLevel();
        }

        public void RenderLevel()
        {
            // Render tiles.
            foreach (int x in Enumerable.Range(0, Constants.LEVEL_WIDTH))
                foreach (int y in Enumerable.Range(0, Constants.LEVEL_HEIGHT))
                {
                    GL.LoadIdentity();
                    GL.Translate(Constants.TILE_SIZE * x, Constants.TILE_SIZE * y, 0);
                    int textureToUse = -1;
                    switch (Level.Tiles[x,y])
                    {
                        case 1:
                            textureToUse = Constants.TEX_ID_TILE;
                            break;
                        case 0:
                        default:
                            textureToUse = Constants.TEX_ID_BG;
                            break;
                    }
                    if (texObjects.TryGetValue(textureToUse, out TexObject tileTexObject))
                        tileTexObject.GlRenderFromCorner(Constants.TILE_SIZE);
                }

            // Render sprite suit
            if (spriteTexObjects.TryGetValue(Constants.TEX_ID_SPRITE_SUIT, out SpriteTexObject suitTexObject))
            {
                GL.LoadIdentity();
                GL.Translate(256 + Level.XPosition, 256, 0);
                int frameToRender = (int)(Level.SpriteAnimationPosition * suitTexObject.TexCount);
                if (frameToRender < 0)
                    frameToRender = 0;
                if (frameToRender >= suitTexObject.TexCount)
                    frameToRender = suitTexObject.TexCount - 1;
                suitTexObject.GlRenderFromCorner(Constants.SPRITE_SUIT_SIZE, frameToRender, Level.facing == CharacterFacing.Right);
            }

            int lavaFrameToRender = (int)(Level.LavaAnimationLoopValue * Constants.LAVA_LAKE_SPRITE_FRAMES);
            if (lavaFrameToRender < 0)
                lavaFrameToRender = 0;
            if (lavaFrameToRender >= Constants.LAVA_LAKE_SPRITE_FRAMES)
                lavaFrameToRender = Constants.LAVA_LAKE_SPRITE_FRAMES - 1;

            // Render lava surface
            for (int i = 0; i < 15; i++)
            {
                GL.LoadIdentity();
                GL.Translate(512 + i * Constants.LAVA_SURFACE_SPRITE_SIZE, 256 + Constants.LAVA_SURFACE_SPRITE_SIZE, 0);
                if (spriteTexObjects.TryGetValue(Constants.TEX_ID_SPRITE_LAVA_SURFACE, out SpriteTexObject lavaSurfaceTexObject))
                    lavaSurfaceTexObject.GlRenderFromCorner(Constants.LAVA_SURFACE_SPRITE_SIZE, lavaFrameToRender);
            }

            // Render lava lake
            for (int i = 0; i < 15; i++)
                for (int j = 0; j < 5; j++)
                {
                    GL.LoadIdentity();
                    GL.Translate(512 + i * Constants.LAVA_LAKE_SPRITE_SIZE, 256 - j * Constants.LAVA_LAKE_SPRITE_SIZE, 0);
                    
                if (spriteTexObjects.TryGetValue(Constants.TEX_ID_SPRITE_LAVA_LAKE, out SpriteTexObject spriteLavaLakeTexObject))
                    spriteLavaLakeTexObject.GlRenderFromCorner(Constants.LAVA_LAKE_SPRITE_SIZE, lavaFrameToRender);
                }

            // Render lava bombs
            foreach (LavaBombEntity lavaBomb in Level.LavaBombs)
            {
                GL.LoadIdentity();
                GL.Translate(lavaBomb.Position.X + Level.XPosition, lavaBomb.Position.Y, 0);
                GL.Rotate(Util.RadiansToDegrees(Level.Angle), 0, 0, 1);
                if (texObjects.TryGetValue(Constants.TEX_ID_LAVA_BOMB, out TexObject texObject))
                    texObject.GlRenderFromMiddle(Constants.LAVA_BOMB_SIZE * lavaBomb.Level);
            }

            RenderString(300, 300, "The quick brown fox?");
        }

        public void RenderString(double x, double y, string text, double size = Constants.TEXT_DEFAULT_HEIGHT)
        {
            for (int i = 0; i < text.Length; ++i)
            {
                GL.LoadIdentity();
                GL.Translate(x + i * (size - Constants.TEXT_KERNING), y, 0);
                if (spriteTexObjects.TryGetValue(Constants.TEX_ID_SPRITE_FONT, out SpriteTexObject spriteFontTexObject))
                    spriteFontTexObject.GlRenderFromCorner(size, CorrectIndex(text[i]));
            }
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
