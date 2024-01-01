using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KMU_Dispatcher
{
    public class DispatchResult
    {
        public Bus bus;
        public DispatchResultState state;

        public enum DispatchResultState
        {
            Non,
            OverMinRadius,
            OverMaxUser,
            NoPath,
            Overlap,
            OverWaitTime,
            OverDetourTime,
            Success,
        }

        public DispatchResult(Bus bus, DispatchResultState state)
        {
            this.bus = bus;
            this.state = state;
            //this.DispatchResultState = state;
        }

        public DispatchResult(Bus bus)
        {
            this.bus = bus;
            this.state = DispatchResultState.Non;
        }
    }
}
