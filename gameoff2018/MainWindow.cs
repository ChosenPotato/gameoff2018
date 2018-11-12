using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace gameoff2018
{
    public sealed class MainWindow : GameWindow
    {
        Bitmap bitmap = new Bitmap("tex.png");
        int texture = -1;
        double angle = 0.0;

        public MainWindow(): base
            (
                1280, // initial width
                720, // initial height
                GraphicsMode.Default,
                "Game Off 2018",  // initial title
                GameWindowFlags.Default,
                DisplayDevice.Default,
                2, // OpenGL major version
                1, // OpenGL minor version
                GraphicsContextFlags.ForwardCompatible
            )
        {
            Title += ": OpenGL Version: " + GL.GetString(StringName.Version);
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(-1.0, 1.0, -1.0, 1.0, 0.0, 4.0);
        }

        protected override void OnLoad(EventArgs e)
        {
            CursorVisible = true;

            Color4 backColor;
            backColor.A = 1.0f;
            backColor.R = 0.1f;
            backColor.G = 0.1f;
            backColor.B = 0.3f;
            GL.ClearColor(backColor);

            GL.Enable(EnableCap.Texture2D);

            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

            GL.GenTextures(1, out texture);
            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            bitmap.UnlockBits(data);
        }

        protected override void OnUnload(EventArgs e)
        {
            GL.DeleteTextures(1, ref texture);

            base.OnUnload(e);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            angle += e.Time * Math.PI;

            HandleKeyboard();
        }

        private void HandleKeyboard()
        {
            var keyState = Keyboard.GetState();

            if (keyState.IsKeyDown(Key.Escape))
            {
                Exit();
            }
        }

        public double RadiansToDegrees(double rads)
        {
            return rads * (180 / Math.PI);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            Title = $"(Vsync: {VSync}) FPS: {1f / e.Time:0}";

            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.Rotate(RadiansToDegrees(angle), 0, 0, 1);
            GL.BindTexture(TextureTarget.Texture2D, texture);

            GL.Begin(PrimitiveType.Quads);

            GL.TexCoord2(0.0f, 1.0f); GL.Vertex2(-0.4f, -0.4f);
            GL.TexCoord2(1.0f, 1.0f); GL.Vertex2(0.4f, -0.4f);
            GL.TexCoord2(1.0f, 0.0f); GL.Vertex2(0.4f, 0.4f);
            GL.TexCoord2(0.0f, 0.0f); GL.Vertex2(-0.4f, 0.4f);

            GL.End();

            SwapBuffers();
        }
    }
}
