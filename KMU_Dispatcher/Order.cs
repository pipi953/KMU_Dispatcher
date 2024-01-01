using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KMU_Dispatcher
{
    [Serializable]
    public class Order
    {
        public enum State
        {
            request, 
            fail,
            matched,
            on_board, 
            operation_end
        }

        public int user_id;
        public int id;
        public DateTime requestTime;
        public State state;

        public int f_st_no;
        public long f_st_link;
        public int t_st_no;
        public long t_st_link;

        public int passengerCount;

        public int researchInterval = 30;
        public int cur_searchCount = 30;

        public int start_count = 0;
        public int end_count = 0;


        public Order(int id, DateTime requestTime, int f_st_no, long f_st_link, int t_st_no, long t_st_link, int count)
        {
            try
            {
                if (count <= 0) throw new DispatchException("[" + DispatchException.DispatchExeptionErrorCode.InputDataError + "] - this order's count is Invalid data ") { ErrorCode = DispatchException.DispatchExeptionErrorCode.InputDataError };

                this.user_id = -1;
                this.id = id;
                this.requestTime = requestTime;
                this.f_st_no = f_st_no;
                this.f_st_link = f_st_link;
                this.t_st_no = t_st_no;
                this.t_st_link = t_st_link;
                this.passengerCount = count;
                this.state = State.request;
            }
            catch (DispatchException dex)
            {
                throw dex;
            }
            catch (Exception ex)
            {
                throw new DispatchException("[" + DispatchException.DispatchExeptionErrorCode.InputDataError + "] - " + ex.Message)
                {
                    ErrorCode = DispatchException.DispatchExeptionErrorCode.InputDataError
                };
            }
            
        }
        public Order(int id, DateTime requestTime, int f_st_no, long f_st_link, int t_st_no, long t_st_link, int count, State state)
        {
            try
            {
                if (count <= 0) throw new DispatchException("[" + DispatchException.DispatchExeptionErrorCode.InputDataError + "] - this order's count is Invalid data ") { ErrorCode = DispatchException.DispatchExeptionErrorCode.InputDataError };

                this.user_id = -1;
                this.id = id;
                this.requestTime = requestTime;
                this.f_st_no = f_st_no;
                this.f_st_link = f_st_link;
                this.t_st_no = t_st_no;
                this.t_st_link = t_st_link;
                this.passengerCount = count;
                this.state = state;
            }
            catch (DispatchException dex)
            {
                throw dex;
            }
            catch (Exception ex)
            {
                throw new DispatchException("[" + DispatchException.DispatchExeptionErrorCode.InputDataError + "] - " + ex.Message)
                {
                    ErrorCode = DispatchException.DispatchExeptionErrorCode.InputDataError
                };
            }

        }

        public Order(int id, DateTime requestTime, BusStop f_stop, BusStop t_stop, int count)
        {
            try
            {
                if (f_stop == null) throw new DispatchException("[" + DispatchException.DispatchExeptionErrorCode.InputDataError + "] - f_stop is null ") { ErrorCode = DispatchException.DispatchExeptionErrorCode.InputDataError };
                if (t_stop == null) throw new DispatchException("[" + DispatchException.DispatchExeptionErrorCode.InputDataError + "] - t_stop is null ") { ErrorCode = DispatchException.DispatchExeptionErrorCode.InputDataError };
                if (count <= 0) throw new DispatchException("[" + DispatchException.DispatchExeptionErrorCode.InputDataError + "] - this order's count is Invalid data ") { ErrorCode = DispatchException.DispatchExeptionErrorCode.InputDataError };

                this.user_id = -1;
                this.id = id;
                this.requestTime = requestTime;
                this.f_st_no = f_stop.busStop_id;
                this.f_st_link = f_stop.closest_link_id;
                this.t_st_no = t_stop.busStop_id;
                this.t_st_link = t_stop.closest_link_id;
                this.passengerCount = count;
                this.state = State.request;
            }
            catch(DispatchException dex)
            {
                throw dex;
            }
            catch(Exception ex)
            {
                throw new DispatchException("[" + DispatchException.DispatchExeptionErrorCode.InputDataError + "] - " + ex.Message)
                {
                    ErrorCode = DispatchException.DispatchExeptionErrorCode.InputDataError
                };
            }
        }

        public Order(int id, DateTime requestTime, BusStop f_stop, BusStop t_stop, int count, State state)
        {
            try
            {
                if (f_stop == null) throw new DispatchException("[" + DispatchException.DispatchExeptionErrorCode.InputDataError + "] - f_stop is null ") { ErrorCode = DispatchException.DispatchExeptionErrorCode.InputDataError };
                if (t_stop == null) throw new DispatchException("[" + DispatchException.DispatchExeptionErrorCode.InputDataError + "] - t_stop is null ") { ErrorCode = DispatchException.DispatchExeptionErrorCode.InputDataError };
                if (count <= 0) throw new DispatchException("[" + DispatchException.DispatchExeptionErrorCode.InputDataError + "] - this order's count is Invalid data ") { ErrorCode = DispatchException.DispatchExeptionErrorCode.InputDataError };

                this.user_id = -1;
                this.id = id;
                this.requestTime = requestTime;
                this.f_st_no = f_stop.busStop_id;
                this.f_st_link = f_stop.closest_link_id;
                this.t_st_no = t_stop.busStop_id;
                this.t_st_link = t_stop.closest_link_id;
                this.passengerCount = count;
                this.state = state;
            }
            catch (DispatchException dex)
            {
                throw dex;
            }
            catch (Exception ex)
            {
                throw new DispatchException("[" + DispatchException.DispatchExeptionErrorCode.InputDataError + "] - " + ex.Message)
                {
                    ErrorCode = DispatchException.DispatchExeptionErrorCode.InputDataError
                };
            }
        }


        public Order(Order order)
        {
            try
            {
                this.user_id = order.user_id;
                this.id = order.id;
                this.requestTime = new DateTime(order.requestTime.Ticks);
                this.f_st_no = order.f_st_no;
                this.f_st_link = order.f_st_link;
                this.t_st_no = order.t_st_no;
                this.t_st_link = order.t_st_link;
                this.passengerCount = order.passengerCount;
                this.state = order.state;
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
            this.state = State.request;
            this.cur_searchCount = researchInterval;
        }

        public bool checkSearch()
        {
            if (cur_searchCount == researchInterval)
            {
                cur_searchCount = 0;
                return true;
            }
            else
            {
                cur_searchCount++;
                return false;
            }
        }

    }

}
