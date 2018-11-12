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
        Bitmap TexBmp = new Bitmap("tex.png");
        int Texture = -1;
        double Angle = 0.0;
        double XPosition = 0.0;
        KeyboardState LatestKeyState;

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
            GL.Ortho(0, Width, 0, Height, 0, 1);
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
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

            GL.GenTextures(1, out Texture);
            GL.BindTexture(TextureTarget.Texture2D, Texture);

            BitmapData data = TexBmp.LockBits(new System.Drawing.Rectangle(0, 0, TexBmp.Width, TexBmp.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            TexBmp.UnlockBits(data);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        }

        protected override void OnUnload(EventArgs e)
        {
            GL.DeleteTextures(1, ref Texture);

            base.OnUnload(e);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            Angle += e.Time * Math.PI / 6;

            HandleKeyboard();

            if (LatestKeyState.IsKeyDown(Key.Left))
                XPosition -= e.Time * 200;
            if (LatestKeyState.IsKeyDown(Key.Right))
                XPosition += e.Time * 200;

            if (XPosition < -100)
                XPosition = -100;
            if (XPosition > 100)
                XPosition = 100;
        }

        private void HandleKeyboard()
        {
            LatestKeyState = Keyboard.GetState();

            if (LatestKeyState.IsKeyDown(Key.Escape))
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
            GL.Translate(300 + XPosition, 300, 0);
            GL.Rotate(RadiansToDegrees(Angle), 0, 0, 1);
            GL.BindTexture(TextureTarget.Texture2D, Texture);

            GL.Begin(PrimitiveType.Quads);

            GL.TexCoord2(0.0f, 1.0f); GL.Vertex2(-50, -50);
            GL.TexCoord2(1.0f, 1.0f); GL.Vertex2( 50, -50);
            GL.TexCoord2(1.0f, 0.0f); GL.Vertex2( 50,  50);
            GL.TexCoord2(0.0f, 0.0f); GL.Vertex2(-50,  50);

            GL.End();

            SwapBuffers();
        }
    }
}
