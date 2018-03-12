using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dal200Instalation.Model
{
    class Point
    {
        private readonly int x, y;

        public Point()
        {
            x = 0;
            y = 0;
        }

        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static bool IsInsideRaidus(Point center, Point target, int radius)
        {
            var dx = (target.x - center.x);
            var dy = (target.y - center.y);

            return (dx * dx) + (dy * dy) <= radius;
        }
    }
}
