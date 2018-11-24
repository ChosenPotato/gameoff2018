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
            {Constants.TEX_ID_SPRITE_SUIT, new TexObject(@"assets\sprite-suit.png")}
        };
        SpriteTexObject spriteTexObject = new SpriteTexObject(@"assets\sprite-suit.png", 256, 8);
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
            spriteTexObject.GlInit();
        }

        public void RenderFrame()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.MatrixMode(MatrixMode.Modelview);

            RenderLevel();
        }

        public void RenderLevel()
        {

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
                    if (texObjects.TryGetValue(textureToUse, out TexObject texObject))
                        texObject.GlRenderFromCorner(Constants.TILE_SIZE);
                }

            GL.LoadIdentity();
            GL.Translate(256 + Level.XPosition, 256, 0);
            int frameToRender = (int)(Level.SpriteAnimationPosition * spriteTexObject.TexCount);
            if (frameToRender < 0)
                frameToRender = 0;
            if (frameToRender >= spriteTexObject.TexCount)
                frameToRender = spriteTexObject.TexCount - 1;
            spriteTexObject.GlRenderFromCorner(Constants.SPRITE_SIZE, frameToRender, Level.facing == CharacterFacing.Right);

            foreach (LavaBombEntity lavaBomb in Level.LavaBombs)
            {
                GL.LoadIdentity();
                GL.Translate(lavaBomb.Position.X + Level.XPosition, lavaBomb.Position.Y, 0);
                GL.Rotate(Util.RadiansToDegrees(Level.Angle), 0, 0, 1);
                if (texObjects.TryGetValue(Constants.TEX_ID_LAVA_BOMB, out TexObject texObject))
                    texObject.GlRenderFromMiddle(Constants.LAVA_BOMB_SIZE * lavaBomb.Level);
            }
        }

        public void Dispose()
        {
            foreach (var texObject in texObjects.Values)
                texObject.Dispose();
        }
    }
}
