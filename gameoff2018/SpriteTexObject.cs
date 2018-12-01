using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace gameoff2018
{
    public class SpriteTexObject : IDisposable
    {
        public Bitmap[] Bitmaps = null;
        public int[] Ids = null;
        public int TexCount = 0;

        public SpriteTexObject(string filename, int textureWidth, int frameCount)
        {
            TexCount = frameCount;
            Bitmaps = new Bitmap[frameCount];
            var wholeFileBitmap = new Bitmap(filename);

            foreach (int frame in Enumerable.Range(0, frameCount))
            {
                Rectangle cropRect = new Rectangle(frame * textureWidth, 0, textureWidth, textureWidth);
                Bitmaps[frame] = new Bitmap(cropRect.Width, cropRect.Height);
                
                Bitmap target = Bitmaps[frame];

                using (Graphics g = Graphics.FromImage(target))
                {
                    g.DrawImage(
                        wholeFileBitmap,
                        new Rectangle(0, 0, target.Width, target.Height),
                        cropRect,
                        GraphicsUnit.Pixel
                    );
                }
            }
        }

        public void GlInit()
        {
            Ids = new int[TexCount];
            GL.GenTextures(TexCount, Ids);

            foreach (int frame in Enumerable.Range(0, TexCount))
            {
                Bitmap frameBitmap = Bitmaps[frame];

                GL.BindTexture(TextureTarget.Texture2D, Ids[frame]);

                BitmapData data = frameBitmap.LockBits(new Rectangle(0, 0, frameBitmap.Width, frameBitmap.Height),
                    ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                    OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

                frameBitmap.UnlockBits(data);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scale"></param>
        /// <param name="frame">Must be >= 0 and < TexCount.</param>
        /// <param name="flip">Flips horizontally.</param>
        public void GlRenderFromCorner(double scale, int frame, bool flip = false)
        {
            GL.PushMatrix();
            {
                GL.Scale(scale, scale, 1.0);
                if (flip)
                {
                    GL.Translate(1.0, 0.0, 0.0);
                    GL.Scale(-1.0, 1.0, 1.0);
                }
                GL.BindTexture(TextureTarget.Texture2D, Ids[frame]);

                GL.Begin(PrimitiveType.Quads);
                {
                    GL.TexCoord2(0.0f, 1.0f);
                    GL.Vertex2(0.0, 0.0);

                    GL.TexCoord2(1.0f, 1.0f);
                    GL.Vertex2(1.0, 0.0);

                    GL.TexCoord2(1.0f, 0.0f);
                    GL.Vertex2(1.0, 1.0);

                    GL.TexCoord2(0.0f, 0.0f);
                    GL.Vertex2(0.0, 1.0);
                }
                GL.End();

                GL.BindTexture(TextureTarget.Texture2D, -1);
            }
            GL.PopMatrix();
        }

        public void Dispose()
        {
            GL.DeleteTextures(TexCount, Ids);
        }
    }
}
