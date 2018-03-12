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
        //[id,x,y,height,orX,orY]
        public string StripPositionData()
        {
            var dtdtTracking = (OscMessage)data;
            string positionData = "";
            //There must be a better way to deal with multiple people being tracked
            if (dtdtTracking[0] is object[])
            {
                
                foreach (object[] individualTracking in dtdtTracking)
                {
                    var id = individualTracking[0];
                    var x = individualTracking[1];
                    var y = individualTracking[2];
                    positionData += $"[{id},{x},{y}]";
                }
            }
            else
            {
                var id = dtdtTracking[0];
                var x = dtdtTracking[1];
                var y = dtdtTracking[2];
                positionData = $"[{id},{x},{y}]";
            }

            
            return positionData;

        }


        public void StopReceiving()
        {
            oscReceiver.Close();
            receivingThread.Join();
        }
    }
}
