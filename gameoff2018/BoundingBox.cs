using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gameoff2018
{
    public class BoundingBox
    {
        public double Left;
        public double Right;
        public double Bottom;
        public double Top;

        public BoundingBox(double left, double right, double bottom, double top)
        {
            Left = left;
            Right = right;
            Bottom = bottom;
            Top = top;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bb1"></param>
        /// <param name="bb2"></param>
        /// <returns>True if an intersection occurred, false otherwise.</returns>
        public static bool TestIntersection(BoundingBox bb1, BoundingBox bb2)
        {
            return
                ! (bb2.Left > bb1.Right
                || bb2.Right < bb1.Left
                || bb2.Top < bb1.Bottom
                || bb2.Bottom > bb1.Top);
        }
    }
}
