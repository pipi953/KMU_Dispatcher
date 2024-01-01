using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KMU_Dispatcher
{
    [Serializable]
    public class TurnInfo
    {
        public enum TURN_TYPE
        {
            Empty,
            Unprotected_Turn,
            OnlyBus_Turn,
            No_Turn,
            U_Turn,
            P_Turn,
            No_LeftTurn,
            No_Straight,
            No_RightTurn,
        }
        public long st_node;
        public long md_node;
        public long ed_node;
        public TURN_TYPE turn_type;


        private static List<TurnInfo> turninfo_list = new List<TurnInfo>();

        public TurnInfo(long st_node, long md_node, long ed_node, TURN_TYPE turn_type)
        {
            this.st_node = st_node;
            this.md_node = md_node;
            this.ed_node = ed_node;
            this.turn_type = turn_type;
        }


        public static void setTurnInfoList(List<TurnInfo> turninfo_list)
        {
            TurnInfo.turninfo_list = turninfo_list;
        }

        public static List<TurnInfo> getTurnInfoList()
        {
            return TurnInfo.turninfo_list;
        }
    }
}
