using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KMU_Dispatcher
{
    class DispatchException : Exception
    {
        public enum DispatchExeptionErrorCode
        {
            InputDataError,
            DataError,
            FindPathError,
        }

        public DispatchException()
        {

        }

        public DispatchException(string message) : base(message)
        {

        }
        public object ErrorCode
        {
            get;
            set;
        }
        
    }
}
