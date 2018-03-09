using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp.Server;

namespace Dal200Instalation.Model
{
    //TODO: process direction
    //TODO: detect dwell (how long someone stand in a place within target area
    //TODO: send DTDT with ID + POS
    class Dal200Control
    {
        public int DtdtPort { get; set; }
        public int DwellRadius { get; set; }
        public TimeSpan DwellTime { get; set; }

        private readonly KinetOSCHandler dtdtHandler;
        private readonly WebSocketServer wsServer;
        private Dictionary<int, DtdtSubject> activeUsers;
        
        public Dal200Control()
        {
            activeUsers = new Dictionary<int, DtdtSubject>();
            dtdtHandler = new KinetOSCHandler(DtdtPort);
            dtdtHandler.OnDataReceived += DtdtDataReceived;
            wsServer = new WebSocketServer("ws://134.190.132.64");
            wsServer.AddWebSocketService<Dall200Messages>("/Dal200");
            wsServer.Start();
        }
        private void DtdtDataReceived(Rug.Osc.OscPacket data)
        {
            dtdtHandler.ReceiverSemaphore.WaitOne();
            var positionData = dtdtHandler.StripPositionData();

            SendPositonData(positionData);
            UpdateActiveUsersDict(positionData);
            //TODO: Change this for the key from DTDT
            //activeUsers[0] = subject updated

            
            dtdtHandler.ReceiverSemaphore.Release();

        }
        private void SendPositonData(string data)
        {
            wsServer.WebSocketServices["/Dal200"].Sessions.BroadcastAsync(data,null);
        }

        private void UpdateActiveUsersDict(string positionData)
        {

        }

    }
}
