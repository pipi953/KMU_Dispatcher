using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Schedulers;

namespace KMU_Dispatcher
{
    public static class Dispatcher
    {
        public enum DispatchErrorCode
        {
            dataError,

        }

        public static KMU_Astar Astar;

        private static int MAX_TASK_COUNT;
        private static int MAXWAITINGTIME;
        private static float DETOURTIME;
        private static int TAKEBOARDINGTIME;
        private static int RADIUS;

        public static object lock_obj = new object();

        public static int maxTaskCount
        {
            get { return MAX_TASK_COUNT; }
            set { MAX_TASK_COUNT = value; }
        }
        public static int maxWaitingTime
        {
            get { return MAXWAITINGTIME; }
            set { MAXWAITINGTIME = value; }
        }
        public static float detourTime
        {
            get { return DETOURTIME; }
            set { DETOURTIME = value; }
        }
        public static int radius
        {
            get { return RADIUS; }
            set { RADIUS = value; }
        }


        static Dispatcher()
        {
            // 동시에 계산할 수 있는 최대 대수
            MAX_TASK_COUNT = 10;
            // 승객별 최대 대기시간 (초)
            MAXWAITINGTIME = 30 * 60;
            // 우회시간 최대치(배율)
            DETOURTIME = 1.5f;
            // 승하차시간 (초)
            TAKEBOARDINGTIME = 10;
            // 배차요청지에서 차량까지의 최소 보장 거리 (더 가까울 시 거부)
            RADIUS = 100;

        }


        private static void setNode(List<NODE> n)
        {
            NODE.setNodeList(n);
        }
        public static List<NODE> getNode()
        {
            return NODE.getNodeList();
        }

        private static void setLink(List<LINK> l)
        {
            LINK.setLinkList(l);
        }
        public static List<LINK> getLink()
        {
            return LINK.getLinkList();
        }

        private static void setTurnInfo(List<TurnInfo> t)
        {
            TurnInfo.setTurnInfoList(t);
        }
        public static List<TurnInfo> getTurnInfo()
        {
            return TurnInfo.getTurnInfoList();
        }

        private static void setBusStop(List<BusStop> bs)
        {
            BusStop.setBusStopList(bs);
        }
        public static List<BusStop> getBusStop()
        {
            return BusStop.getBusStopList();
        }

        private static void setBus(List<Bus> b)
        {
            Bus.setBusList(b);
        }
        public static List<Bus> getBus()
        {
            return Bus.getBusList();
        }

        public static void setData(List<NODE> n, List<LINK> l, List<TurnInfo> t, List<BusStop> bs)
        {
            if (n.Count == 0 || n == null) throw new DispatchException("[" + DispatchException.DispatchExeptionErrorCode.InputDataError + "] - NODE data null! ") { ErrorCode = DispatchException.DispatchExeptionErrorCode.InputDataError };
            if (l.Count == 0 || l == null) throw new DispatchException("[" + DispatchException.DispatchExeptionErrorCode.InputDataError + "] - LINK data null! ") { ErrorCode = DispatchException.DispatchExeptionErrorCode.InputDataError };
            if (t.Count == 0 || t == null) throw new DispatchException("[" + DispatchException.DispatchExeptionErrorCode.InputDataError + "] - TurnInfo data null! ") { ErrorCode = DispatchException.DispatchExeptionErrorCode.InputDataError };
            if (bs.Count == 0 || bs == null) throw new DispatchException("[" + DispatchException.DispatchExeptionErrorCode.InputDataError + "] - BusStop data null! ") { ErrorCode = DispatchException.DispatchExeptionErrorCode.InputDataError };

            setNode(n);
            setLink(l);
            setTurnInfo(t);
            setBusStop(bs);

            Astar = new KMU_Astar(getNode(), getLink(), getTurnInfo());

        }

        public static void setData(List<NODE> n, List<LINK> l, List<TurnInfo> t, List<BusStop> bs, List<Bus> b)
        {
            if (n.Count == 0 || n == null) throw new DispatchException("[" + DispatchException.DispatchExeptionErrorCode.InputDataError + "] - NODE data null! ") { ErrorCode = DispatchException.DispatchExeptionErrorCode.InputDataError };
            if (l.Count == 0 || l == null) throw new DispatchException("[" + DispatchException.DispatchExeptionErrorCode.InputDataError + "] - LINK data null! ") { ErrorCode = DispatchException.DispatchExeptionErrorCode.InputDataError };
            if (t.Count == 0 || t == null) throw new DispatchException("[" + DispatchException.DispatchExeptionErrorCode.InputDataError + "] - TurnInfo data null! ") { ErrorCode = DispatchException.DispatchExeptionErrorCode.InputDataError };
            if (bs.Count == 0 || bs == null) throw new DispatchException("[" + DispatchException.DispatchExeptionErrorCode.InputDataError + "] - BusStop data null! ") { ErrorCode = DispatchException.DispatchExeptionErrorCode.InputDataError };
            if (b.Count == 0 || b == null) throw new DispatchException("[" + DispatchException.DispatchExeptionErrorCode.InputDataError + "] - Bus data null! ") { ErrorCode = DispatchException.DispatchExeptionErrorCode.InputDataError };

            setNode(n);
            setLink(l);
            setTurnInfo(t);
            setBusStop(bs);
            setBus(b);

            Astar = new KMU_Astar(getNode(), getLink(), getTurnInfo());

        }

        public static List<DispatchResult> RequestDemand(List<Bus> busList, Order order_, DateTime currentTime)
        {
            if (busList.Count == 0 || busList == null) throw new DispatchException("[" + DispatchException.DispatchExeptionErrorCode.InputDataError + "] - busList data null! ") { ErrorCode = DispatchException.DispatchExeptionErrorCode.InputDataError };

            var scheduler = new LimitedConcurrencyLevelTaskScheduler(MAX_TASK_COUNT);
            TaskFactory taskFac = new TaskFactory(scheduler);
            List<Task> tasks = new List<Task>();

            int busNo = -1;


            float minTime = float.MaxValue;
            float takeTime = TAKEBOARDINGTIME * order_.passengerCount;  //  总搭乘时间

            List<Bus> tmp_busList = Copy.DeepClone(busList);
            //List<Bus> dispatchResult_List = new List<Bus>(); //  保存所有线程获取的 bus no 与 bus 信息
            List<DispatchResult> dispatchResult_List = new List<DispatchResult>();

            foreach (Bus tmp_bus in tmp_busList)
            {
                Bus bus = Copy.DeepClone(tmp_bus);
                Task t = taskFac.StartNew(() =>
                {
                    DispatchResult dispatchResult = new DispatchResult(bus);
                    //  线程开始工作
                    //Console.WriteLine("Task id {0} Time{1}  dealNumber{2}", Task.CurrentId, DateTime.Now, obj);

                    BusStop startBS = getBusStop().Find(x => x.busStop_id == order_.f_st_no);

                    bool radius_check_pass = true;

                    if(bus.location_list.Count > 2)
                    {
                        Point g_point = (bus.location_list[0] + bus.location_list[1] + bus.location_list[2]) / 3;
                        
                        float move_dis = Point.Distance_latlon(bus.current_location, g_point);
                        if (move_dis > 10)                       
                            radius_check_pass = false;                       
                        
                    }
                    
                    float b_bs_dis = Point.Distance_latlon(bus.current_location, startBS.location);

                    if (b_bs_dis > RADIUS || radius_check_pass)
                    {
                        List<Order> orderList = new List<Order>();
                        //List<BusStop> stopover = new List<BusStop>();
                        List<LINK> passLink = new List<LINK>();

                        foreach (Order od in bus.orderList)
                        {
                            switch (od.state)
                            {
                                case Order.State.request:       //요청
                                    bus.reservationPassengerCount += od.passengerCount;
                                    break;
                                case Order.State.fail:

                                    break;
                                case Order.State.matched:      //  배차완료
                                    orderList.Add(new Order(od));
                                    bus.reservationPassengerCount += od.passengerCount;
                                    break;
                                case Order.State.on_board: //탑승중
                                    orderList.Add(new Order(od.id, od.requestTime, -1, bus.current_link.link_id, od.t_st_no, od.t_st_link, od.passengerCount));
                                    bus.currentPassengerCount += od.passengerCount;
                                    LINK l = LINK.getLinkList().FirstOrDefault(x => x.link_id == od.f_st_link);
                                    if (l != null)
                                        passLink.Add(l);
                                    break;
                                case Order.State.operation_end:      //  하차  

                                    break;
                                    //default:                           //   默认

                                    //    break;
                            }
                        }
                        orderList.Add(new Order(order_));
                        bus.reservationPassengerCount += order_.passengerCount;

                        //float passengerCount = bus.PassengerCount;

                        if (bus.PassengerCount <= bus.MaxPassengerCount)
                        {
                            PathResult newPath;
                            //int timeCount = 0;
                            //if (bus.curTimeCount > 0)
                            //{
                            //    timeCount = bus.curTimeCount;
                            //}
                            //  查找 当前点的 link id 
                            LINK cur_link = bus.current_link;

                            //int next_bus_stop_idx = bus.waypoint.FindIndex(x => x.arrived == false);

                            

                            foreach (BusStop busStop in bus.lockedBusStop)
                            {

                                LINK l = LINK.getLinkList().FirstOrDefault(x => x.link_id == busStop.closest_link_id);
                                if (l != null)
                                    passLink.Add(l);


                            }

                            try
                            {
                                newPath = FindPathFromOrder(cur_link.link_id, orderList, passLink);

                                //디버그용 경로 확인
                                string str_path_debug = "";

                                foreach (LINK link in newPath.link_list)
                                {
                                    str_path_debug += "'" + link.link_id + "',";
                                }

                                //Console.WriteLine("KMU_PATH_DEBUG : " + str_path_debug);

                                
                            }
                            catch (DispatchException dex)
                            {
                                throw new DispatchException("bus_no(" + bus.no + ") -> " + dex.Message) { ErrorCode = DispatchException.DispatchExeptionErrorCode.FindPathError };
                            }

                            var duplicates = newPath.link_list.GroupBy(x => x.link_id).Where(g => g.Count() > 1).Select(g => g.Key);
                            //Console.WriteLine(duplicates.Count());
                            if (duplicates.Count() == 0)
                            {
                                
                                if (newPath.distance > 0)
                                {
                                    bus.path = newPath;

                                    //bus.orderList = orderList;
                                    order_.state = Order.State.matched;
                                    bus.orderList.Add(order_);

                                    List<BusStop> stopover = new List<BusStop>();

                                    foreach (Order od in bus.orderList)
                                    {
                                        BusStop tmp_bs;
                                        tmp_bs = BusStop.getBusStopList().FirstOrDefault(x => x.busStop_id == od.f_st_no);
                                        if (tmp_bs != null)
                                            stopover.Add(tmp_bs);

                                        tmp_bs = BusStop.getBusStopList().FirstOrDefault(x => x.busStop_id == od.t_st_no);
                                        if (tmp_bs != null)
                                            stopover.Add(tmp_bs);

                                    }

                                    bus.setWaypoint(stopover);

                                    //  搭乘人数 确认
                                    float driveTime = bus.path.getTime();
                                    //  最大运行时间 确认
                                    if (driveTime <= bus.MaxDriveTime || true)
                                    {
                                        //  线程中止

                                        //  可接受的迂回时间 确认(搭乘时间)
                                        

                                        int order_f_st_idx = bus.getWaypoint().FindIndex(x => x.busStop_id == order_.f_st_no);

                                        // 우회&대기 가능 시간체크(탑승시간)
                                        bool check = true;
                                        foreach (Order order in bus.orderList)
                                        {
                                            //  确认 需要等待的时间 
                                            // 대기시간 체크
                                            float waitingTime = 0.0f;
                                            int getTimeCount = 0;

                                            if (order.state == Order.State.matched)
                                            {

                                                //int curTimeCount = bus.curTimeCount;
                                                int od_f_idx = bus.getWaypoint().FindIndex(x => x.busStop_id == order.f_st_no);

                                                if (order_f_st_idx <= od_f_idx)
                                                {
                                                    waitingTime = bus.path.getTime(cur_link.link_id, order.f_st_link);
                                                    //waitingTime += (float)(currentTime - order.requestTime).TotalSeconds;

                                                    //waitingTime = (float)(bus.start_time + TimeSpan.FromSeconds(getTimeCount) - order.requestTime.TimeOfDay).TotalSeconds;

                                                    if (waitingTime > MAXWAITINGTIME)
                                                    {
                                                        check = false;
                                                        dispatchResult.state = DispatchResult.DispatchResultState.OverWaitTime;
                                                        break;
                                                    }
                                                }

                                                

                                                //우회 시간 체크
                                                float detourTime = bus.path.getTime(order.f_st_link, order.t_st_link);

                                                //float originalTime = (float)astar.FindPath_TwoPoint(start_link, end_link).weigth + (takeBoardingTime * order.passanserCount);

                                                PathResult shortestPath = Astar.FindPath(order.f_st_link, order.t_st_link);



                                                float originalTime = shortestPath.getTime();

                                                //계산 전후 경로 출력 및 비교용.
                                                string str = "";
                                                foreach (LINK link in shortestPath.link_list)
                                                {
                                                    str += "'" + link.link_id + "',";
                                                }

                                                str = "";
                                                foreach (LINK link in bus.path.link_list)
                                                {
                                                    str += "'" + link.link_id + "',";
                                                }

                                                if (detourTime - (originalTime * DETOURTIME) > 0)
                                                {
                                                    check = false;


                                                    dispatchResult.state = DispatchResult.DispatchResultState.OverDetourTime;
                                                    break;
                                                }

                                            }

                                            if (order.state == Order.State.on_board)
                                            {
                                                //우회시간 체크
                                                //float detourTime = bus.path.getTime(start_link, end_link) + (takeBoardingTime * order.passanserCount);
                                                float detourTime = bus.path.getTime(order.t_st_link);

                                                detourTime += (float)(currentTime - order.requestTime).TotalSeconds;
                                                //float originalTime = (float)astar.FindPath_TwoPoint(start_link, end_link).weigth + (takeBoardingTime * order.passanserCount);

                                                PathResult shortestPath = Astar.FindPath(order.f_st_link, order.t_st_link);

                                                float originalTime = shortestPath.getTime();

                                                if (detourTime - (originalTime * DETOURTIME) > 0)
                                                {
                                                    check = false;
                                                    dispatchResult.state = DispatchResult.DispatchResultState.OverDetourTime;
                                                    break;
                                                }
                                            }


                                        }


                                        if (check)
                                        {
                                            //List<BusStop> stopover = new List<BusStop>();

                                            //foreach (Order od in bus.orderList)
                                            //{
                                            //    BusStop tmp_bs;
                                            //    tmp_bs = BusStop.getBusStopList().FirstOrDefault(x => x.busStop_id == od.f_st_no);
                                            //    if (tmp_bs != null)
                                            //        stopover.Add(tmp_bs);

                                            //    tmp_bs = BusStop.getBusStopList().FirstOrDefault(x => x.busStop_id == od.t_st_no);
                                            //    if (tmp_bs != null)
                                            //        stopover.Add(tmp_bs);

                                            //}

                                            //bus.setWaypoint(stopover);

                                            dispatchResult.state = DispatchResult.DispatchResultState.Success;
                                            //lock (dispatchResult_List)
                                            //{
                                            //    dispatchResult_List.Add(dispatchResult);
                                            //}
                                        }

                                    }
                                }
                                else
                                {
                                    dispatchResult.state = DispatchResult.DispatchResultState.NoPath;
                                }
                            }
                            else
                            {
                                dispatchResult.state = DispatchResult.DispatchResultState.Overlap;
                            }

                            


                        }
                        else
                        {
                            dispatchResult.state = DispatchResult.DispatchResultState.OverMaxUser;
                        }
                    }
                    else
                    {
                        dispatchResult.state = DispatchResult.DispatchResultState.OverMinRadius;
                    }

                    lock (dispatchResult_List)
                    {
                        dispatchResult_List.Add(dispatchResult);
                    }


                });
                tasks.Add(t);
            }

            try
            {
                Task.WaitAll(tasks.ToArray());
            }
            catch (AggregateException ae)
            {
                throw ae;
                //foreach (var ex in ae.Flatten().InnerExceptions)
                //{
                //    Console.WriteLine(ex.Message);
                //}
            }

            //dispatchResult_List.OrderBy(x => x.path.getTime());

            return dispatchResult_List;


            //catch (Exception ex)
            //{
            //    return new List<Bus>();

            //}
            //return busNo;
        }

        static PathResult FindPathFromOrder(long startLink, List<Order> orderList, List<LINK> passLinks)
        {
            List<(long, long)> orderList_link = new List<(long, long)>();

            foreach (Order od in orderList)
            {
                orderList_link.Add((od.f_st_link, od.t_st_link));
            }

            return Astar.FindPath(startLink, orderList_link, passLinks);
        }

    }
}
