using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading.Tasks;
using Dal200Instalation.Utils;
using Newtonsoft.Json;
using WebSocketSharp.Server;

namespace Dal200Instalation.Model
{
    //TODO: process direction (to be done in DTDT code)
    //TODO: detect dwell (how long someone stand in a place within target area
    class Dal200Control
    {
        //TEMP TIMER FOR FAKE DWELL
        private Timer fakeDwellTimer;
        

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

            fakeDwellTimer = new Timer(30 * 1000);
            fakeDwellTimer.Elapsed += FakeDwellTimer_Elapsed;
            fakeDwellTimer.Start();
        }

        private void FakeDwellTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var individual = new Tracked(0,20,90,"Canadian","VideoCanadian1");
            var data = new JsonData();
            data.trackerData.Add(individual);

            wsServer.WebSocketServices["/Dal200"].Sessions.BroadcastAsync(JsonConvert.SerializeObject(data), null);
        }

        public void Shutdown()
        {
            wsServer.Stop();
            dtdtHandler.StopReceiving();
        }

        private void DtdtDataReceived(Rug.Osc.OscPacket data)
        {
            var positionData = dtdtHandler.StripPositionData();
            SendPositonData(positionData);
            //UpdateActiveUsersDict(positionData);
            //TODO: Change this for the key from DTDT
            //activeUsers[0] = subject updated
        }
        private void SendPositonData(JsonData data)
        {
            wsServer.WebSocketServices["/Dal200"].Sessions.BroadcastAsync(JsonConvert.SerializeObject(data), null);
        }

        private void UpdateActiveUsersDict(string positionData)
        {

        }

    }
}
