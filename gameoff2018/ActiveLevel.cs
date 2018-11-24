using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace gameoff2018
{
    public class ActiveLevel
    {
        public List<LavaBombEntity> LavaBombs = new List<LavaBombEntity>();
        public double Angle = 0.0;
        public double XPosition = 0.0;

        /// <summary>
        /// Tiles for the level, eg. background, wall, hazard.
        /// </summary>
        public int[,] Tiles = null;

        /// <summary>
        /// Normalised frame to render (rate of increase may not be 1/sec).
        /// </summary>
        public double SpriteAnimationPosition = 0;

        public ActiveLevel()
        {
            Tiles = new int[Constants.LEVEL_WIDTH, Constants.LEVEL_HEIGHT];
            Tiles[0, 0] = 1;
            Tiles[1, 0] = 1;
            Tiles[2, 0] = 1;

            LavaBombs.Add(new LavaBombEntity(new Vector2d(300, 300), new Vector2d(0, 360), 4));
        }

        public void Update(KeyboardState keyboardState, double elapsedTime)
        {
            Angle += elapsedTime * Math.PI;

            SpriteAnimationPosition += elapsedTime * Constants.SPRITE_FPS / 8;
            if (SpriteAnimationPosition > 1.0)
                SpriteAnimationPosition -= 1.0;

            if (keyboardState.IsKeyDown(Key.Left))
                XPosition -= elapsedTime * 200;
            if (keyboardState.IsKeyDown(Key.Right))
                XPosition += elapsedTime * 200;

            if (XPosition < -100)
                XPosition = -100;
            if (XPosition > 100)
                XPosition = 100;

            foreach (LavaBombEntity b in LavaBombs)
            {
                b.Velocity.Y -= Constants.GRAVITY * elapsedTime;
                b.Position += b.Velocity * elapsedTime;
            }

            IEnumerable<LavaBombEntity> toSpawnFrom =
                LavaBombs
                .Where(x => x.Level > 1 && x.TimeCreated + 1000 <= DateTimeOffset.Now.ToUnixTimeMilliseconds());
            LavaBombs =
                LavaBombs
                .Where(x => x.TimeCreated + 1000 > DateTimeOffset.Now.ToUnixTimeMilliseconds())
                .ToList();
            IEnumerable<Vector2d> vecs = Enumerable.Range(1, 10).Select(x => Math.PI * 2 / 10 * x).Select(x => Util.VectorFromAngle(x) * 100.0);
            IEnumerable<LavaBombEntity> spawned =
                toSpawnFrom.SelectMany(x => vecs.Select(y => new LavaBombEntity(x.Position, x.Velocity + y * x.Level, x.Level - 1)));

            LavaBombs = LavaBombs.Concat(spawned).ToList();
        }
    }
}
