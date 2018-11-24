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

        public ActiveLevel()
        {
            LavaBombs.Add(new LavaBombEntity(new Vector2d(300, 300), new Vector2d(0, 360), 4));
        }

        public void Update(KeyboardState keyboardState, double elapsedTime)
        {
            Angle += elapsedTime * Math.PI;

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
