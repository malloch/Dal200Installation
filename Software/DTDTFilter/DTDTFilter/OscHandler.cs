using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Rug.Osc;

namespace DTDTFilter
{
    class OscHandler
    {
        public Semaphore ReceiverSemaphore { get; }
        public bool OscConnected { get; private set; }

        public delegate void DataReceived(OscPacket data);
        public event DataReceived OnDataReceived;

        private OscPacket data;
        private readonly OscReceiver oscReceiver;
        private Thread receivingThread;
        public int Port { get;}


        public OscHandler(int port)
        {
            this.Port = port;
            OscConnected = false;
            ReceiverSemaphore = new Semaphore(1, 1);
            oscReceiver = new OscReceiver(port);
        }

        public void StartReceiving()
        {
            receivingThread = new Thread(new ThreadStart(ReceiverMethod));
            oscReceiver.Connect();
            OscConnected = true;
            receivingThread.Start();
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
                        if (((OscMessage)data).Address == "/MultiCam")
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

        public void StopReceiving()
        {
            oscReceiver.Close();
            receivingThread.Join();
        }
    }
}

