using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace gameoff2018
{
    public class LavaBombEntity
    {
        public Vector2d Position, Velocity;
        public int Level;
        public long TimeCreated;

        public LavaBombEntity(Vector2d pos, Vector2d vel, int level)
        {
            Position = pos;
            Velocity = vel;
            Level = level;
            TimeCreated = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }
    }

    public sealed class MainWindow : GameWindow
    {
        public static readonly double LAVA_BOMB_SIZE = 20;
        public static readonly double GRAVITY = 360.0;

        public List<LavaBombEntity> LavaBombs = new List<LavaBombEntity>();

        Bitmap TexBmp = new Bitmap(@"assets\lava-bomb.png");
        int Texture = -1;
        double Angle = 0.0;
        double XPosition = 0.0;
        KeyboardState LatestKeyState;
        double ScreenHeight = 720.0;

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

            GL.GenTextures(1, out Texture);
            GL.BindTexture(TextureTarget.Texture2D, Texture);

            BitmapData data = TexBmp.LockBits(new System.Drawing.Rectangle(0, 0, TexBmp.Width, TexBmp.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            TexBmp.UnlockBits(data);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            LavaBombs.Add(new LavaBombEntity(new Vector2d(300, 300), new Vector2d(0, 360), 4));
            Debug.WriteLine("Added from OnLoad");
        }

        protected override void OnUnload(EventArgs e)
        {
            GL.DeleteTextures(1, ref Texture);

            base.OnUnload(e);
        }

        public static Vector2d VectorFromAngle(double radians)
        {
            return new Vector2d
            (
                Math.Sin(radians),
                Math.Cos(radians)
            );
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            Angle += e.Time * Math.PI;

            HandleKeyboard();

            if (LatestKeyState.IsKeyDown(Key.Left))
                XPosition -= e.Time * 200;
            if (LatestKeyState.IsKeyDown(Key.Right))
                XPosition += e.Time * 200;

            if (XPosition < -100)
                XPosition = -100;
            if (XPosition > 100)
                XPosition = 100;

            foreach (LavaBombEntity b in LavaBombs)
            {
                b.Velocity.Y -= GRAVITY * e.Time;
                b.Position += b.Velocity * e.Time;
            }

            IEnumerable<LavaBombEntity> toSpawnFrom =
                LavaBombs
                .Where(x => x.Level > 1 && x.TimeCreated + 1000 <= DateTimeOffset.Now.ToUnixTimeMilliseconds());
            LavaBombs =
                LavaBombs
                .Where(x => x.TimeCreated + 1000 > DateTimeOffset.Now.ToUnixTimeMilliseconds())
                .ToList();
            IEnumerable<Vector2d> vecs = Enumerable.Range(1, 10).Select(x => Math.PI * 2 / 10 * x).Select(x => VectorFromAngle(x) * 100.0);
            IEnumerable<LavaBombEntity> spawned =
                toSpawnFrom.SelectMany(x => vecs.Select(y => new LavaBombEntity(x.Position, x.Velocity + y * x.Level, x.Level - 1)));

            LavaBombs = LavaBombs.Concat(spawned).ToList();
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            Debug.WriteLine($"(X: {e.X}) Y: {e.Y}");

            double x = e.X;
            double y = ScreenHeight - e.Y;
            
            LavaBombs.Add(new LavaBombEntity(new Vector2d(x, y), new Vector2d(0, 360), 3));
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

            foreach (LavaBombEntity lavaBomb in LavaBombs)
            {
                GL.LoadIdentity();
                GL.Translate(lavaBomb.Position.X + XPosition, lavaBomb.Position.Y, 0);
                GL.Rotate(RadiansToDegrees(Angle), 0, 0, 1);
                GL.BindTexture(TextureTarget.Texture2D, Texture);

                GL.Begin(PrimitiveType.Quads);

                GL.TexCoord2(0.0f, 1.0f); GL.Vertex2(-LAVA_BOMB_SIZE * lavaBomb.Level, -LAVA_BOMB_SIZE * lavaBomb.Level);
                GL.TexCoord2(1.0f, 1.0f); GL.Vertex2(LAVA_BOMB_SIZE * lavaBomb.Level, -LAVA_BOMB_SIZE * lavaBomb.Level);
                GL.TexCoord2(1.0f, 0.0f); GL.Vertex2(LAVA_BOMB_SIZE * lavaBomb.Level, LAVA_BOMB_SIZE * lavaBomb.Level);
                GL.TexCoord2(0.0f, 0.0f); GL.Vertex2(-LAVA_BOMB_SIZE * lavaBomb.Level, LAVA_BOMB_SIZE * lavaBomb.Level);

                GL.End();
            }

            SwapBuffers();
        }
    }
}
