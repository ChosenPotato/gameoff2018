using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System;
using System.Diagnostics;
using System.Drawing;

namespace gameoff2018
{
    public sealed class MainWindow : GameWindow
    {
        OpenGlContext GlContext = null;
        ActiveLevel Level = new ActiveLevel();

        KeyboardState LatestKeyState;
        double ScreenHeight = 720.0;

        int CurrentScreenWidth = Constants.INITIAL_SCREEN_WIDTH;
        int CurrentScreenHeight = Constants.INITIAL_SCREEN_WIDTH;

        public MainWindow(): base
            (
                Constants.INITIAL_SCREEN_WIDTH,
                Constants.INITIAL_SCREEN_HEIGHT,
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
            CurrentScreenWidth = Width;
            CurrentScreenHeight = Height;

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
            Level.Update(LatestKeyState, Keyboard.GetState(), e.Time);

            HandleKeyboard();
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            Debug.WriteLine($"(X: {e.X}) Y: {e.Y}");

            Vector2d worldCoords = Level.ConvertScreenToWorldCoords(
                new Point(e.X, (int)ScreenHeight - e.Y),
                CurrentScreenWidth);

            // Scale from world coords to tile coords (same origin = no translation).
            int tileX = (int)(worldCoords.X / Constants.TILE_SIZE);
            int tileY = (int)(worldCoords.Y / Constants.TILE_SIZE);

            if (tileX >= 0 && tileX < Constants.LEVEL_WIDTH)
                if (tileY >= 0 && tileY < Constants.LEVEL_HEIGHT)
                {
                    if (e.Button == MouseButton.Left)
                    {
                        ref int tile = ref Level.Tiles[tileX, tileY];
                        if (tile < Constants.TILE_ID_FLAME_SPITTER)
                            ++tile;
                        else
                            tile = Constants.TILE_ID_EMPTY;
                    }
                    if (e.Button == MouseButton.Right)
                        Level.Tiles[tileX, tileY] = 0;
                }
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
            Title = "Hot Rocks, Cold Feet";

            GlContext.RenderFrame(CurrentScreenWidth, CurrentScreenHeight);

            SwapBuffers();
        }
    }
}
