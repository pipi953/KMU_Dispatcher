using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KMU_Dispatcher
{
    [Serializable]
    public class LINK
    {
        public long link_id;
        public long f_node;
        public long t_node;
        public int max_spd;
        public int[] spd_array;
        public float length;
        public List<Point> geom;

        public string road_type;
        public string connect_type;

        private static List<LINK> link_list = new List<LINK>();

        public LINK(long link_id, long f_node, long t_node, int max_spd, int[] spd_array, float length, List<Point> geom, string road_type, string connect_type)
        {
            try
            {
                this.link_id = link_id;
                this.f_node = f_node;
                this.t_node = t_node;
                this.max_spd = max_spd;
                this.spd_array = spd_array;
                this.length = length;
                this.geom = geom;
                this.road_type = road_type;
                this.connect_type = connect_type;
            }
            catch (Exception ex)
            {
                throw new DispatchException("[" + DispatchException.DispatchExeptionErrorCode.InputDataError + "] - " + ex.Message)
                {
                    ErrorCode = DispatchException.DispatchExeptionErrorCode.InputDataError
                };
            }
        }

        public float getTime()
        {
            float result;

            result = (length / (max_spd * 1000.0f)) * 3600;

            return result;
        }

        public string getGeomText()
        {
            string result = "";

            for (int i = 0; i < geom.Count; i++)
            {
                if (i == 0)
                {
                    result += geom[i];
                }
                else
                {
                    result += ("," + geom[i]);
                }
            }

            return result;
        }

        public static void setLinkList(List<LINK> link_list)
        {
            LINK.link_list = link_list;
        }

        public static List<LINK> getLinkList()
        {
            return LINK.link_list;
        }

        public static LINK getCurrentLink(List<Point> point_list)
        {
            LINK result = null;

            if (point_list == null || point_list.Count == 0) throw new ArgumentNullException();

            Point curPoint = point_list[0];

            if (point_list.Count == 1)
            {
                return getClosestLink(curPoint, 1)[0];
            }
            else
            {
                Point v1;
                for (int i = 1; i < point_list.Count; i++)
                {
                    v1 = curPoint - point_list[i];

                    if (Point.SqrMagnitude(v1) > 0)
                    {
                        v1 = Point.normalize(v1);

                        List<LINK> closestLinks = getClosestLink(curPoint, 4);


                        double minRad = double.MaxValue;

                        for (int j = 0; j < closestLinks.Count; j++)
                        {
                            Point v2 = getDirectionPoint(closestLinks[j], curPoint);

                            double rad = Math.Acos(Point.DotProduct(v1, v2)) * 180.0f / Math.PI;
                            rad = Math.Abs(rad);

                            if (minRad > rad)
                            {
                                minRad = rad;
                                result = closestLinks[j];
                            }

                        }

                        break;

                    }
                }

                return getClosestLink(curPoint, 1)[0];

            }
        }
        public static LINK getCurrentLink(Point prev_point, Point cur_point)
        {
            LINK result = null;

            Point v1 = Point.normalize(cur_point - prev_point);



            List<LINK> closestLinks = getClosestLink(cur_point, 4);

            double minRad = double.MaxValue;

            for (int i = 0; i < closestLinks.Count; i++)
            {
                //Point c_p = getClosestPoint(closestLinks[i], cur_point);
                //Point p_p = getClosestPoint(closestLinks[i], prev_point);

                //Point v2 = Point.normalize(c_p - p_p);
                Point v2 = getDirectionPoint(closestLinks[i], cur_point);

                double rad = Math.Abs(Math.Asin(Point.CrossProduct(v1, v2)));


                if (minRad > rad)
                {
                    minRad = rad;
                    result = closestLinks[i];
                }

            }

            return result;
        }

        static Point getDirectionPoint(LINK l, Point p)
        {
            Point result = new Point();
            Point min = null;
            int minIdx = -1;
            float minDis = float.MaxValue;

            List<Point> geom = Copy.DeepClone(l.geom);

            //geom.OrderBy(x => Point.Distance(x, p));

            for (int i = 0; i < l.geom.Count; i++)
            {
                float dis = Point.Distance(l.geom[i], p);
                if (minDis > dis)
                {
                    minDis = dis;
                    min = Copy.DeepClone(l.geom[i]);
                    minIdx = i;
                }
            }

            if (minIdx != -1)
            {
                if (minIdx > 0)
                {
                    result = l.geom[minIdx] - l.geom[minIdx - 1];
                }
                else
                {
                    result = l.geom[minIdx + 1] - l.geom[minIdx];
                }
            }

            return Point.normalize(result);
        }

        public static List<LINK> getClosestLink(Point p, int count)
        {
            List<LINK> link_list = Copy.DeepClone(KMU_Dispatcher.Dispatcher.getLink());

            link_list = link_list.OrderBy(x => Point.Distance_LinkAndPoint(x, p)).ToList();

            return link_list.GetRange(0, count);

            //LINK minLink = null;
            //float minDis = float.MaxValue;
            //foreach (LINK link in LINK.link_list)
            //{
            //    for (int li = 0; li < link.geom.Count - 1; li++)
            //    {
            //        Point p1 = link.geom[li];
            //        Point p2 = link.geom[li + 1];

            //        float dis = Point.Distance_LineAndPoint(p1, p2, p);

            //        if(minDis > dis)
            //        {
            //            minDis = dis;
            //            minLink = link;
            //        }
            //    }
            //}

            //return minLink;
        }
    }
}
