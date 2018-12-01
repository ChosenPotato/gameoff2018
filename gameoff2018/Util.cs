using OpenTK;
using System;

namespace gameoff2018
{
    public static class Util
    {
        public static double RadiansToDegrees(double rads)
        {
            return rads * (180 / Math.PI);
        }

        public static Vector2d VectorFromAngle(double radians)
        {
            return new Vector2d
            (
                Math.Sin(radians),
                Math.Cos(radians)
            );
        }
    }
}
