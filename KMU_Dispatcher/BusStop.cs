using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KMU_Dispatcher
{
    [Serializable]
    public class BusStop
    {
        public int busStop_id = 0;                      //  公交站 id
        public string busStop_name = "";          //  公交站名称
        public Point location;
        public int numOfWaiting = 0;               //  等待人数
        public long closest_link_id;
        public bool arrived;

        private static List<BusStop> busStop_list = new List<BusStop>();

        public BusStop(int id_busStop, string name_busStop, double lat_busStop, double lng_busStop, long closest_link_id)
        {
            try
            {
                this.busStop_id = id_busStop;
                this.busStop_name = name_busStop;
                this.location = new Point(lng_busStop, lat_busStop);
                //this.lat = lat_busStop;
                //this.lng = lng_busStop;
                this.numOfWaiting = 0;
                this.closest_link_id = closest_link_id;
                this.arrived = false;
            }
            catch (Exception ex)
            {
                throw new DispatchException("[" + DispatchException.DispatchExeptionErrorCode.InputDataError + "] - " + ex.Message)
                {
                    ErrorCode = DispatchException.DispatchExeptionErrorCode.InputDataError
                };
            }

        }

        public void init()
        {
            this.numOfWaiting = 0;
            this.arrived = false;
        }

        public static void setBusStopList(List<BusStop> busStop_list)
        {
            BusStop.busStop_list = busStop_list;
        }

        public static List<BusStop> getBusStopList()
        {
            return BusStop.busStop_list;
        }
    }
}
