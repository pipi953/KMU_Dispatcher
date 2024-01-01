using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KMU_Dispatcher
{
    [Serializable]
    public class PathResult
    {
        public double distance;
        public List<LINK> link_list;
        public List<LINK> stopoverByClosestLink;
        //public List<int> stopoverByStNo;\
        
        //public List<(long, long)> saveOpenLinks;
        public PathResult()
        {
            this.distance = 0.0d;
            this.link_list = new List<LINK>();
            this.stopoverByClosestLink = new List<LINK>();
            //this.stopoverByStNo = new List<int>();
            //this.saveOpenLinks = new List<(long, long)>();
        }

        public PathResult(double distance, List<LINK> link_list, List<LINK> stopoverByClosestLink)
        {
            this.distance = distance;
            this.link_list = link_list;
            this.stopoverByClosestLink = stopoverByClosestLink;
            //this.saveOpenLinks = new List<(long, long)>();
        }

        public string getPathGeomFromText()
        {
            List<string> points = new List<string>();

            foreach(LINK link in link_list)
            {
                foreach(Point p in link.geom)
                {
                    points.Add(p.ToString());
                }
            }

            string geom = string.Join(",", points);

            string result = "MULTILINESTRING ((" + geom + "))";
            return result;
        }

        public float getTravelTime(TimeSpan curTime)
        {
            int oneDayTotalMinutes = 24 * 60;
            int interval = 5;
            int m = oneDayTotalMinutes / interval;
            TimeSpan Time = new TimeSpan(curTime.Ticks);

            foreach (LINK link in link_list)
            {
                int time_idx = (int)(Time.TotalMinutes / 5) % m;
                float spd = (1000.0f / 3600.0f) * (float)link.spd_array[time_idx];
                if (spd == 0)
                    spd = (1000.0f / 3600.0f) * (link.max_spd * 0.5f);

                float length = link.length;
                float time = length / spd;

                Time += TimeSpan.FromSeconds(time);

            }

            return (float)(Time - curTime).TotalSeconds;
        }


        public double getDistance()
        {
            double result = 0.0d;
            if (link_list.Count == 0)
            {
                return -1;
            }

            for (int i = 0; i < link_list.Count; i++)
            {
                result += (float)link_list[i].length;
            }

            return result;
        }

        public float getTime()
        {
            float result = 0.0f;

            if (link_list.Count == 0)
            {
                return -1;
            }

            for (int i = 0; i < link_list.Count; i++)
            {
                result += (float)link_list[i].getTime();
            }

            return result;
        }

        public float getTime(long link_id)
        {
            float result = 0.0f;

            int idx = link_list.FindIndex(x => x.link_id == link_id);

            if (idx == -1)
            {
                return 0;
            }

            for (int i = 0; i < idx; i++)
            {
                result += (float)link_list[i].getTime();
            }

            return result;
        }

        public float getTime(long start_link, long end_link)
        {
            float result = 0.0f;

            int start_idx = link_list.FindIndex(x => x.link_id == start_link);
            int end_idx = link_list.FindIndex(x => x.link_id == end_link);

            if (start_idx == -1 || end_idx == -1)
            {
                return -1;
            }

            for (int i = start_idx; i <= end_idx; i++)
            {
                result += (float)link_list[i].getTime();
            }

            return result;
        }

        public float getWaitingTime(long link_id)
        {
            float result = 0.0f;

            int seq = stopoverByClosestLink.FindIndex(x => x.link_id == link_id);

            if (seq == -1)
            {
                return -1;
            }

            if (seq == 0)
            {
                return 0.0f;
            }


            int idx = link_list.FindIndex(x => x.link_id == stopoverByClosestLink[seq].link_id);
            int prev_idx = link_list.FindIndex(x => x.link_id == stopoverByClosestLink[seq - 1].link_id);

            for (int i = prev_idx; i < idx; i++)
            {
                result += (float)link_list[i].getTime();
            }

            return result;
        }

        //public List<pathPerSecond> getTravelPathPerSecond()
        //{
        //    List<pathPerSecond> geom = new List<pathPerSecond>();

        //    int idx = 0;
        //    geom.Add(new pathPerSecond(idx, link_list[0].geom[0]));

        //    int oneDayTotalMinutes = 24 * 60;
        //    int interval = 5;
        //    int m = oneDayTotalMinutes / interval;
        //    TimeSpan curTime = new TimeSpan(Simulation_Toolkit.instance.curTime.Ticks);

        //    foreach (Link link in link_list)
        //    {
        //        int time_idx = (int)(curTime.TotalMinutes / 5) % m;
        //        float spd = (1000.0f / 3600.0f) * (float)link.spd_array[time_idx];
        //        float length = link.length;
        //        float time = length / spd;

        //        curTime = TimeSpan.FromSeconds(geom.Count);

        //        foreach (string p in link.geom)
        //        {
        //            Vector cur = new Vector(geom.Last().point);

        //            Vector target = new Vector(p);

        //            float dis = Vector.latlon_distance(cur, target);
        //            time = (dis / spd);

        //            if (time >= 1)
        //            {
        //                Vector nextPos;
        //                for (int i = 1; i < time; i++)
        //                {
        //                    nextPos = Vector.Lerp(cur, target, i / (float)time);

        //                    geom.Add(new pathPerSecond(idx, nextPos.ToString()));
        //                }
        //            }
        //        }

        //        idx++;

        //    }

        //    //for(int i = geom.Count; i<optimzeRoute.instance.ondDayTimeForSecond; i++)
        //    //{
        //    //    geom.Add(geom.Last());
        //    //}

        //    return geom;
        //}
    }
}
