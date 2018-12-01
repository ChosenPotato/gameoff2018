using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace gameoff2018
{
    public class TexObject: IDisposable
    {
        public Bitmap Bitmap = null;
        public int Id = -1;

        public TexObject(string filename)
        {
            Bitmap = new Bitmap(filename);
        }

        public void GlInit()
        {
            GL.GenTextures(1, out Id);
            GL.BindTexture(TextureTarget.Texture2D, Id);

            BitmapData data = Bitmap.LockBits(new Rectangle(0, 0, Bitmap.Width, Bitmap.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            Bitmap.UnlockBits(data);
        }

        public void GlRenderFromCorner(double scale, bool flip = false, bool debug = false)
        {
            GL.Scale(scale, scale, 1.0);
            if (flip)
            {
                GL.Translate(1.0, 0.0, 0.0);
                GL.Scale(-1.0, 1.0, 1.0);
            }

            if (debug)
            {
                GL.Color3(0.0, 1.0, 0.0);
                GL.Begin(PrimitiveType.Lines);
                GL.Vertex2(-5, 0);
                GL.Vertex2(+5, 0);
                GL.Vertex2(0, -5);
                GL.Vertex2(0, +5);
                GL.End();
                GL.Color3(1.0, 1.0, 1.0);
            }

            GL.BindTexture(TextureTarget.Texture2D, Id);

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

        public void GlRenderFromMiddle(double scale)
        {
            GL.Scale(scale, scale, 1.0);
            GL.BindTexture(TextureTarget.Texture2D, Id);

            GL.Begin(PrimitiveType.Quads);
            {
                GL.TexCoord2(0.0f, 1.0f);
                GL.Vertex2(-1.0, -1.0);

                GL.TexCoord2(1.0f, 1.0f);
                GL.Vertex2(1.0, -1.0);

                GL.TexCoord2(1.0f, 0.0f);
                GL.Vertex2(1.0, 1.0);

                GL.TexCoord2(0.0f, 0.0f);
                GL.Vertex2(-1.0, 1.0);
            }
            GL.End();

            GL.BindTexture(TextureTarget.Texture2D, -1);
        }

        public void Dispose()
        {
            GL.DeleteTextures(1, ref Id);
        }
    };
}
