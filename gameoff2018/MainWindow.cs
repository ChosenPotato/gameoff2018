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
        OpenGlContext GlContext = null;
        ActiveLevel Level = new ActiveLevel();

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
            GlContext = new OpenGlContext(Level);

            Debug.WriteLine("Added from OnLoad");
        }

        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            HandleKeyboard();

            Level.Update(LatestKeyState, e.Time);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            Debug.WriteLine($"(X: {e.X}) Y: {e.Y}");

            double x = e.X;
            double y = ScreenHeight - e.Y;
            
            Level.LavaBombs.Add(new LavaBombEntity(new Vector2d(x, y), new Vector2d(0, 360), 3));
        }

        private void HandleKeyboard()
        {
            LatestKeyState = Keyboard.GetState();

            if (LatestKeyState.IsKeyDown(Key.Escape))
            {
                Exit();
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            Title = $"(Vsync: {VSync}) FPS: {1f / e.Time:0}";

            GlContext.RenderFrame();

            SwapBuffers();
        }
    }
}
