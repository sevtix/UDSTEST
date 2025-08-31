using ISOTPTEST;
using O.IsoTp;
using SAE.J2534;
using System.Diagnostics;

API fac = APIFactory.GetAPI("C:\\WINDOWS\\SysWOW64\\op20pt32.dll");
Device dev = fac.GetDevice();
Channel channel = dev.GetChannel(Protocol.CAN, Baud.CAN_500000, ConnectFlag.NONE, true);
MessageFilter idFilter = new MessageFilter()
{
    FilterType = Filter.PASS_FILTER,
    Mask = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF },
    Pattern = new byte[] { 0x00, 0x00, 0x1, 0x01 },
};
channel.StartMsgFilter(idFilter);

channel.DefaultRxTimeout = 0;

var cts = new CancellationTokenSource();

IsoTpProtocol isoTpProtocol = new IsoTpProtocol();
ISOTP isotp = new ISOTP(isoTpProtocol, "KTMUDS", 0x101, 0x100, channel, cts);
UDS uds = new UDS(isotp);

while (true)
{
    Stopwatch s = Stopwatch.StartNew();
    IsoTpMessage ramread = uds.SendReadMemoryByAddress(0xC4C, 0x02);
    s.Stop();
    ushort value = (ushort)((ramread.Data[1] << 8) | ramread.Data[0]);
    Console.SetCursorPosition(0, 0);
    Console.WriteLine($"{value} {s.Elapsed.Milliseconds} ms");
}

