﻿using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public void GlRender(double scale)
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