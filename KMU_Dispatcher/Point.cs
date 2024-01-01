using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KMU_Dispatcher
{
    [Serializable]
    public class Point
    {
        public double x;
        public double y;

        public Point()
        {
            this.x = 0;
            this.y = 0;
        }

        public Point(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public Point(string geom)
        {
            string[] location;
            location = geom.Split(' ');

            this.x = double.Parse(location[0]);
            this.y = double.Parse(location[1]);
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", this.x, this.y);
        }

        public static Point operator +(Point a, Point b)
        {
            return new Point(a.x + b.x, a.y + b.y);
        }

        public static Point operator -(Point a, Point b)
        {
            return new Point(a.x - b.x, a.y - b.y);
        }

        public static Point operator -(Point a)
        {
            return new Point(-a.x, -a.y);
        }

        public static Point operator *(Point a, double d)
        {
            return new Point(a.x * d, a.y * d);
        }

        public static Point operator *(float d, Point a)
        {
            return new Point(a.x * d, a.y * d);
        }

        public static Point operator /(Point a, double d)
        {
            return new Point(a.x / d, a.y / d);
        }

        public static bool operator ==(Point lhs, Point rhs)
        {
            return Point.SqrMagnitude(lhs - rhs) < 0.0 / 1.0;
        }

        public static bool operator !=(Point lhs, Point rhs)
        {
            return (double)Point.SqrMagnitude(lhs - rhs) >= 0.0 / 1.0;
        }


        public static Point Lerp(Point from, Point to, double t)
        {
            if (t < 0.0)
                t = 0.0d;
            if (t > 1.0)
                t = 1d;

            return new Point(from.x + (to.x - from.x) * t, from.y + (to.y - from.y) * t);
        }

        public static double SqrMagnitude(Point a)
        {
            return (a.x * a.x + a.y * a.y);
        }
        public static Point normalize(Point a)
        {
            double magn = Math.Sqrt(Point.SqrMagnitude(a));
            return new Point(a.x / magn, a.y / magn);
        }
        private static double deg2rad(double deg)
        {
            return (double)(deg * Math.PI / (double)180d);
        }
        private static double rad2deg(double rad)
        {
            return (double)(rad * (double)180d / Math.PI);
        }


        public static float DotProduct(Point p1, Point p2)
        {
            return (float)(p1.x * p2.x + p1.y * p2.y);
        }

        public static float CrossProduct(Point p1, Point p2)
        {
            return (float)(p1.x * p2.y - p1.y * p2.x);
        }

        public static float Distance(Point p1, Point p2)
        {
            return (float) Math.Sqrt(Point.SqrMagnitude(p2 - p1));
        }
        public static float Distance_latlon(Point p1, Point p2)
        {
            //double theta, dist;

            //theta = p1.y - p2.y;

            //dist = Math.Sin(deg2rad(p1.x)) * Math.Sin(deg2rad(p2.x)) + Math.Cos(deg2rad(p1.x))
            //     * Math.Cos(deg2rad(p2.x)) * Math.Cos(deg2rad(theta));
            //dist = Math.Acos(dist);
            //dist = rad2deg(dist);

            //dist = dist * 60 * 1.1515;
            //dist = dist * 1.609344;    // 단위 mile 에서 km 변환.  
            //dist = dist * 1000.0;      // 단위  km 에서 m 로 변환  

            ////dist = dist/spd * 3600.0f;
            //return (float)dist;

            GeoCoordinate p_1 = new GeoCoordinate(p1.y, p1.x);
            GeoCoordinate p_2 = new GeoCoordinate(p2.y, p2.x);

            return (float)p_1.GetDistanceTo(p_2);
        }

        //public static float Distance_latlon_2(Point p1, Point p2)
        //{
        //    var d1 = p1.y * (Math.PI / 180.0);
        //    var num1 = p1.x * (Math.PI / 180.0);
        //    var d2 = p2.y * (Math.PI / 180.0);
        //    var num2 = p2.x * (Math.PI / 180.0) - num1;
        //    var d3 = Math.Pow(Math.Sin((d2 - d1) / 2.0), 2.0) + Math.Cos(d1) * Math.Cos(d2) * Math.Pow(Math.Sin(num2 / 2.0), 2.0);

        //    return (float)(6376500.0 * (2.0*Math.Atan2(Math.Sqrt(d3),Math.Sqrt(1.0-d3))));

        //}

        //public static float Distance_latlon_3(Point p1, Point p2)
        //{
        //    GeoCoordinate p_1 = new GeoCoordinate(p1.y, p1.x);
        //    GeoCoordinate p_2 = new GeoCoordinate(p2.y, p2.x);

        //    return (float)p_1.GetDistanceTo(p_2); 
        //}


        public static float Distance_LineAndPoint(Point line_p1, Point line_p2, Point p)
        {
            Point p1_p = p - line_p1;
            Point p1_p2 = line_p2 - line_p1;
            Point p2_p1 = line_p1 - line_p2;
            Point p2_p = p - line_p2;

            if(Point.DotProduct(p1_p,p1_p2) * Point.DotProduct(p2_p1,p2_p) >= 0)
            {
                return Math.Abs(Point.CrossProduct(p1_p, p1_p2) / Point.Distance(line_p1, line_p2));
            }
            else
            {
                return Math.Min(Point.Distance(line_p1, p), Point.Distance(line_p2, p));
            }
        }

        public static float Distance_LinkAndPoint(LINK l, Point p)
        {
            float result = float.MaxValue;

            for (int i = 0; i < l.geom.Count - 1; i++)
            {
                Point p1 = l.geom[i];
                Point p2 = l.geom[i + 1];

                float dis = Point.Distance_LineAndPoint(p1, p2, p);

                if (result > dis)
                {
                    result = dis;
                }
            }

            return result;
        }

        public override bool Equals(object other)
        {
            if (!(other is Point))
                return false;
            Point vector = (Point)other;
            if (this.x.Equals(vector.x))
                return this.y.Equals(vector.y);
            else
                return false;
        }
    }
}
