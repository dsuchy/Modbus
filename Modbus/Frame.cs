using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Modbus.Enums;

namespace Modbus
{
    class Frame
    {
        #region Properties

        /// <summary>
        /// Adres.
        /// </summary>
        public byte Address { get; set; }

        /// <summary>
        /// Rozkaz.
        /// </summary>
        public Function Function { get; set; }

        /// <summary>
        /// Dane przesyłane w ramce.
        /// </summary>
        public byte[] Data { get; set; }

        #endregion

        #region Public methods

        public byte[] ToByteStream()
        {
            byte[] byteStream = new byte[getSize()];

            byteStream[0] = Address;
            byteStream[1] = (byte)Function;
            System.Array.Copy(Data, 0, byteStream, 2, Data.Length);

            return byteStream;
        }

        public int getSize()
        {
            return 2 + Data.Length;
        }

        #endregion
    }
}
