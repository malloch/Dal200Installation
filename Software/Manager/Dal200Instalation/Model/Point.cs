using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dal200Instalation.Model
{
    public class Point
    {
        public int x { get; private set; }
        public int y { get; private set; }

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

        public static Point expoAverage(Point old, Point newPoint, float coef)
        {
            var result = new Point();
            float calcualtedCoef = 1 - coef;
            result.x = (int) (old.x * coef + newPoint.x * calcualtedCoef);
            result.y = (int)(old.y * coef + newPoint.y * calcualtedCoef);
            return result;
        }
    }
}
