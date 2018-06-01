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
                        if (((OscMessage) data).Address == "/DTDT")
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


        //[id,x,y,height,orX,orY]
        public JsonData StripDTDTPositionData()
        {
            var dtdtTracking = (OscMessage)data;
            var count = dtdtTracking[0];
            JsonData positionData = new JsonData();
            for (int id = 0; id < (int)dtdtTracking[0]; id++)
            {

                var clientID = (int)dtdtTracking[id * 6 + 1];
                var x = (int)dtdtTracking[id * 6 + 2];
                var y = (int)dtdtTracking[id * 6 + 3];
                var individualTracking = new Tracked(clientID, x, y);

                positionData.trackerData.Add(individualTracking);
            }
            return positionData;

        }

        public JsonData StripBlobPositionData()
        {
            var blobData = (OscMessage) data;
            JsonData positionData = new JsonData();
            var x = (float) blobData[0];
            var y = (float)blobData[1];
            var clientId = 1;
            var individualTracking = new Tracked(clientId,(int)x,(int)y);
            positionData.trackerData.Add(individualTracking);

            return positionData;
        }


        public void StopReceiving()
        {
            oscReceiver.Close();
            receivingThread.Join();
        }
    }
}
