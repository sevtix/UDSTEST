using O.IsoTp;
using SAE.J2534;
using System.Diagnostics;


namespace ISOTPTEST
{
    public class ISOTP
    {

        IsoTpMessage lastResponse = null;
        Channel channel;
        IsoTpProtocol protocol;
        CancellationTokenSource cts;
        uint rxID;
        uint txID;
        string name;

        public ISOTP(IsoTpProtocol protocol, string name, uint rxID, uint txID, Channel channel, CancellationTokenSource cts) {
            this.protocol = protocol;
            this.channel = channel;
            this.cts = cts;
            this.rxID = rxID;
            this.txID = txID;
            this.name = name;
            Init();
        }

        private void Init()
        {
            protocol.AddService(name, rxID, txID);
            var messageSubscription = protocol.MessageReceived.Subscribe(e =>
            {
                lastResponse = e;
            });
            var txSubscription = protocol.CanTX.Subscribe(e =>
            {
                byte[] header = BitConverter.GetBytes(e.CanId);
                channel.SendMessage(new Message([0x00, 0x00, header[1], header[0], .. e.Data]));
            });

            Task.Run(async () =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        GetMessageResults messages = channel.GetMessage();
                        foreach (Message message in messages.Messages)
                        {
                            byte[] canidbytes = message.Data.Skip(2).Take(2).ToArray();
                            uint canid = (uint)((canidbytes[0] << 8) | canidbytes[1]);
                            protocol.CanRX(canid, message.Data.Skip(4).ToArray());
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Receiver loop error: {ex.Message}");
                    }
                }
            }, cts.Token);
        }

        public IsoTpMessage SendCommand(byte[] payload)
        {
            channel.ClearRxBuffer();
            channel.ClearTxBuffer();
            lastResponse = null;
            protocol.Send(name, payload, cts.Token);
            while (lastResponse == null) { }
            return lastResponse;
        }

    }
}
