using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KMU_Dispatcher
{
    [Serializable]
    public class Bus
    {


        public bool state;

        public int id;
        public string no;
        public int type;
        public List<Order> orderList;
        public List<BusStop> lockedBusStop;

        public PathResult path;

        public LINK current_link;
        public Point current_location;
        public List<Point> location_list;

        //public int curTimeCount;

        public int MaxDriveTime; // 초단위

        private int _maxCurrentPassengerCount;
        public int maxCurrentPassengerCount
        {
            set
            {
                if (value > _maxCurrentPassengerCount)
                {
                    _maxCurrentPassengerCount = value;
                }
            }
            get { return _maxCurrentPassengerCount; }
        }


        private int _currentPassengerCount;
        public int currentPassengerCount
        {
            set
            {
                _currentPassengerCount = value;
                maxCurrentPassengerCount = value;
            }
            get { return _currentPassengerCount; }
        }
        public int reservationPassengerCount;
        public int PassengerCount
        {
            get { return currentPassengerCount + reservationPassengerCount; }
        }

        public int MaxPassengerCount;

        private static List<Bus> bus_list = new List<Bus>();

        private List<BusStop> WayPointList;

        public Bus(int id, string name, int MaxPassangerCount, Point currrnetLoc, LINK current_link, List<Point> location_list, List<Order> demand_list, List<BusStop> LockedBusStop)
        {
            try
            {
                this.id = id;
                this.no = name;
                this.MaxPassengerCount = MaxPassangerCount;
                //this.current_link = current_link;


                orderList = demand_list;
                lockedBusStop = LockedBusStop;

                _maxCurrentPassengerCount = 0;
                _currentPassengerCount = 0;
                reservationPassengerCount = 0;

                this.path = new PathResult();
                this.state = true;

                this.current_link = current_link;
                this.current_location = currrnetLoc;
                this.location_list = location_list;

                WayPointList = new List<BusStop>();
            }
            catch (Exception ex)
            {
                throw new DispatchException("[" + DispatchException.DispatchExeptionErrorCode.InputDataError + "] - " + ex.Message)
                {
                    ErrorCode = DispatchException.DispatchExeptionErrorCode.InputDataError
                };
            }
            
        }

        public void setWaypoint(List<BusStop> stopover)
        {
            if (this.path.stopoverByClosestLink.Count > 0)
            {
                List<BusStop> waypoint = new List<BusStop>();
                //List<BusStop> stopover = new List<BusStop>();
                //BusStop bs;
                //List<(int,long)> waypoint = new List<int>();
                


                foreach (LINK link in this.path.stopoverByClosestLink)
                {
                    List<BusStop> tmp_waypoint = new List<BusStop>();
                    foreach (BusStop bs in stopover.FindAll(x => x.closest_link_id == link.link_id))
                    {
                        int containCheck = tmp_waypoint.FindIndex(x => x.busStop_id == bs.busStop_id);
                        if (containCheck == -1)
                        {
                            tmp_waypoint.Add(bs);
                        }
                    }

                    tmp_waypoint.OrderBy(x => Point.Distance(link.geom[0], x.location));

                    waypoint.AddRange(tmp_waypoint);
                }

                this.WayPointList = waypoint;


            }
            else
            {
                this.WayPointList = new List<BusStop>();
            }
        }
        public List<BusStop> getWaypoint()
        {
            return this.WayPointList;
        }
        //public List<BusStop> getWaypoint()
        //{
        //    if (this.path.stopoverByClosestLink.Count > 0)
        //    {
        //        List<BusStop> waypoint = new List<BusStop>();
        //        List<BusStop> stopover = new List<BusStop>();
        //        //BusStop bs;
        //        //List<(int,long)> waypoint = new List<int>();
        //        foreach (Order od in orderList)
        //        {
        //            BusStop tmp_bs;
        //            tmp_bs = BusStop.getBusStopList().FirstOrDefault(x => x.busStop_id == od.f_st_no);
        //            if(tmp_bs != null)
        //                stopover.Add(tmp_bs);

        //            tmp_bs = BusStop.getBusStopList().FirstOrDefault(x => x.busStop_id == od.t_st_no);
        //            if (tmp_bs != null)
        //                stopover.Add(tmp_bs);

        //        }


        //        foreach (LINK link in this.path.stopoverByClosestLink)
        //        {
        //            List<BusStop> tmp_waypoint = new List<BusStop>();
        //            foreach (BusStop bs in stopover.FindAll(x => x.closest_link_id == link.link_id))
        //            {
        //                int containCheck = tmp_waypoint.FindIndex(x => x.busStop_id == bs.busStop_id);
        //                if (containCheck == -1)
        //                {
        //                    tmp_waypoint.Add(bs);
        //                }
        //            }

        //            tmp_waypoint.OrderBy(x => Point.Distance(link.geom[0], x.loaction));

        //            waypoint.AddRange(tmp_waypoint);
        //        }

        //        return waypoint;


        //    }
        //    else
        //    {
        //        return new List<BusStop>();
        //    }
        //}

        public static void setBusList(List<Bus> bus_list)
        {
            Bus.bus_list = bus_list;
        }

        public static List<Bus> getBusList()
        {
            return Bus.bus_list;
        }

    }
}
