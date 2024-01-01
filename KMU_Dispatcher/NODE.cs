using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KMU_Dispatcher
{
    [Serializable]
    public class NODE
    {
        public long node_id;
        private double lat;
        private double lon;
        public bool turn_p;
        public double g;
        public double f;

        public NODE target;
        public NODE Parent;

        private static List<NODE> node_list = new List<NODE>();

        public NODE(long node_id, double X, double Y, bool turn_p)
        {
            try
            {
                this.node_id = node_id;
                this.lat = Y;
                this.lon = X;
                this.turn_p = turn_p;
            }
            catch (Exception ex)
            {
                throw new DispatchException("[" + DispatchException.DispatchExeptionErrorCode.InputDataError + "] - " + ex.Message)
                {
                    ErrorCode = DispatchException.DispatchExeptionErrorCode.InputDataError
                };
            }
        }
        public NODE(NODE v)
        {
            this.node_id = v.node_id;
            this.lat = v.lat;
            this.lon = v.lon;
            this.turn_p = v.turn_p;
            this.g = v.g;
            this.f = v.f;
            this.Parent = v.Parent;
        }

        public float H(NODE destination)
        {
            //calculate the Manhatan distance from this vertex to the destination vertex
            //return (float)Math.Sqrt(Math.Pow((destination.lat - this.lat), 2) + Math.Pow((destination.lon - this.lon), 2));


            if (this.node_id == destination.node_id)
                return 0.0f;


            //double theta, dist;
            //theta = this.lon - destination.lon;

            //dist = Math.Sin(deg2rad(this.lat)) * Math.Sin(deg2rad(destination.lat)) + Math.Cos(deg2rad(this.lat))
            //     * Math.Cos(deg2rad(destination.lat)) * Math.Cos(deg2rad(theta));
            //dist = Math.Acos(dist);
            //dist = rad2deg(dist);

            //dist = dist * 60 * 1.1515;
            //dist = dist * 1.609344;    // 단위 mile 에서 km 변환.  
            //                           //dist = dist * 1000.0;      // 단위  km 에서 m 로 변환  

            //dist = dist / 50 * 3600.0f;
            //return (float)dist;

            GeoCoordinate p_1 = new GeoCoordinate(this.lat, this.lon);
            GeoCoordinate p_2 = new GeoCoordinate(destination.lat,destination.lon);

            return (float)p_1.GetDistanceTo(p_2);
        }

        private double deg2rad(double deg)
        {
            return (double)(deg * Math.PI / (double)180d);
        }
        private double rad2deg(double rad)
        {
            return (double)(rad * (double)180d / Math.PI);
        }


        public static void setNodeList(List<NODE> node_list)
        {
            NODE.node_list = node_list;
        }

        public static List<NODE> getNodeList()
        {
            return NODE.node_list;
        }
    }
}
