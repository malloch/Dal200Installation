using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Rug.Osc;

namespace Dal200Instalation.Model
{
    class KinetOSCHandler
    {
        
        public Semaphore ReceiverSemaphore { get; }
        public bool OscConnected { get; private set; }

        public delegate void DataReceived(OscPacket data);
        public event DataReceived OnDataReceived;

        private OscPacket data;
        private readonly OscReceiver oscReceiver;
        private Thread receivingThread;
        private int port;
        

        public KinetOSCHandler(int port)
        {
            this.port = port;
            OscConnected = false;
            ReceiverSemaphore = new Semaphore(1,1);
            oscReceiver = new OscReceiver(port);
        }

        public void StartReceiving()
        {
            receivingThread = new Thread(new ThreadStart(ReceiverMethod));
            OscConnected = true;
        }

        private void ReceiverMethod()
        {
            try
            {
                while (oscReceiver.State != OscSocketState.Closed)
                {
                    
                    if (oscReceiver.State == OscSocketState.Connected)
                    {
                        ReceiverSemaphore.WaitOne();
                        data = oscReceiver.Receive();
#if DEBUG
                        Console.WriteLine(data.ToString());
#endif
                        OnDataReceived?.Invoke(data);
                        ReceiverSemaphore.Release();
                    }

                    
                }
            }
            catch (Exception ex)
            {
                if (oscReceiver.State == OscSocketState.Connected)
                {
                    Console.WriteLine("Exception in listen loop");
                    Console.WriteLine(ex.Message);
                }
            }
        }

        //TODO: get message format
        public string StripPositionData()
        {
            var result = data.ToString();
            return result;

        }


    }
}
