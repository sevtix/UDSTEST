using O.IsoTp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISOTPTEST
{
    public class UDS
    {
        private ISOTP isotp;
        public UDS(ISOTP isotp)
        {
            this.isotp = isotp;
        }
        public IsoTpMessage SendReadMemoryByAddress(ushort address, ushort length)
        {
            byte addressHigh = (byte)((address >> 8) & 0xFF);
            byte addressLow = (byte)(address & 0xFF);
            byte lengthHigh = (byte)((length >> 8) & 0xFF);
            byte lengthLow = (byte)(length & 0xFF);
            return isotp.SendCommand(new byte[] { 0x23, addressHigh, addressLow, lengthHigh, lengthLow });
        }

        public IsoTpMessage SendWriteMemoryByAddress(ushort address, byte[] bytes)
        {
            byte addressHigh = (byte)((address >> 8) & 0xFF);
            byte addressLow = (byte)(address & 0xFF);
            return isotp.SendCommand([0x3D, addressHigh, addressLow, .. bytes]);
        }
    }
}
