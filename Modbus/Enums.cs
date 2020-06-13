using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modbus
{
    class Enums
    {
        public enum Transmission { T_7E1, T_7O1, T_7N2 };

        public enum Station { SLAVE, MASTER };

        public enum Function : byte { SEND, GET };
    }
}
