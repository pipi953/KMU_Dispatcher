using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KMU_Dispatcher
{
    public class PathPerSecond
    {
        public int idx;
        public string point;
        public PathPerSecond(int idx, string point)
        {
            this.idx = idx;
            this.point = point;
        }
    }
}
