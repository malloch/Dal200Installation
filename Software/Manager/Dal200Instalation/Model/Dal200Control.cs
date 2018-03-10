using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dal200Instalation.Utils;
using WebSocketSharp.Server;

namespace Dal200Instalation.Model
{
    //TODO: process direction
    //TODO: detect dwell (how long someone stand in a place within target area
    //TODO: send DTDT with ID + POS
    class Dal200Control
    {

        

        public readonly KinetOSCHandler dtdtHandler;
        private readonly WebSocketServer wsServer;
        private Dictionary<int, DtdtSubject> activeUsers;
        
        public Dal200Control(int dtdtPort, int dwellRadius, int dwellTime)
        {
            activeUsers = new Dictionary<int, DtdtSubject>();
            
            dtdtHandler = new KinetOSCHandler(dtdtPort);
            dtdtHandler.OnDataReceived += DtdtDataReceived;
            dtdtHandler.StartReceiving();
            

            wsServer = new WebSocketServer($"ws://{NetworkUtils.GetLocalIPAddress()}");
            wsServer.AddWebSocketService<Dall200Messages>("/Dal200");
            wsServer.Start();
        }
        private void DtdtDataReceived(Rug.Osc.OscPacket data)
        {
            var positionData = dtdtHandler.StripPositionData();

            //SendPositonData(positionData);
            //UpdateActiveUsersDict(positionData);
            //TODO: Change this for the key from DTDT
            //activeUsers[0] = subject updated

            
            

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
