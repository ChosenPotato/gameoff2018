using OpenTK;
using System;

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

        public BoundingBox GetBoundingBox()
        {
            return new BoundingBox(
                Position.X - Constants.LAVA_BOMB_SIZE / 2,
                Position.X + Constants.LAVA_BOMB_SIZE / 2,
                Position.Y - Constants.LAVA_BOMB_SIZE / 2,
                Position.Y + Constants.LAVA_BOMB_SIZE / 2);
        }
    }
}
