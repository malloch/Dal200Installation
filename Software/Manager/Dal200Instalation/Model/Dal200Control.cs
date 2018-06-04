using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using Dal200Instalation.Model.Dwellable;
using Dal200Instalation.Utils;
using Newtonsoft.Json;
using Rug.Osc;
using WebSocketSharp.Server;

namespace Dal200Instalation.Model
{
    //TODO: process direction (to be done in DTDT code)
    class Dal200Control
    {
        public readonly KinetOSCHandler dtdtHandler;
        private readonly WebSocketServer wsServer;
        private readonly Dictionary<int, DtdtSubject> activeUsers;
        public DwellableCollection DwellableCollection { get; private set; }

        private JsonData oldTrackingData;
        private int oldTargetId = -1;
        public delegate void DataFiltered(OscPacket data);

        public event DataFiltered OnDataFiltered;
        
        public Dal200Control(int dtdtPort, int dwellRadius, int dwellTime)
        {
            activeUsers = new Dictionary<int, DtdtSubject>();
            DwellableCollection = new DwellableCollection(dwellRadius,TimeSpan.FromSeconds(dwellTime));

            oldTrackingData = new JsonData();
            
            dtdtHandler = new KinetOSCHandler(dtdtPort);
            dtdtHandler.OnDataReceived += DtdtDataReceived;
            dtdtHandler.StartReceiving();

            wsServer = new WebSocketServer($"ws://{NetworkUtils.GetLocalIPAddress()}");
            wsServer.AddWebSocketService<Dall200Messages>("/Dal200");
            wsServer.Start();
        }

        public Dal200Control(int dtdtPort, int dwellRadius, int dwellTime, string filename) : this(dtdtPort,
            dwellRadius, dwellTime)
        {
            DwellableCollection.LoadTargetsFromFile(filename);
            DwellableCollection.OnDwellDetected += DwellDetected;
        }

        public void SendFakeDwell(int x, int y, string track, string mediaName)
        {
            var individual = new Tracked(999, x,y, track, mediaName);
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
            var positionData = dtdtHandler.StripBlobPositionData();

            for (int i = 0; i < positionData.trackerData.Count; i++)
            {
                var id = positionData.trackerData[i].id;
                for (int j = 0; j < oldTrackingData.trackerData.Count; j++)
                {
                    if (id == oldTrackingData.trackerData[j].id)
                    {
                        positionData.trackerData[i].position = Point.expoAverage(oldTrackingData.trackerData[j].position,
                            positionData.trackerData[i].position, 0.8f);
                        break;
                        
                    }
                }
            }

            SendPositonData(positionData);
            DwellableCollection.DetectDwell(positionData);

            oldTrackingData = positionData;
            //UpdateActiveUsersDict(positionData);

        }

        private void DwellDetected(Tracked targetData)
        {
            if (targetData.id == oldTargetId)
                return;
            oldTargetId = targetData.id;
            Console.WriteLine(targetData.id);

            var data = new DwellData();
            data.dwellIndex = targetData.id;
            wsServer.WebSocketServices["/Dal200"].Sessions.BroadcastAsync(JsonConvert.SerializeObject(data), null);
        }

        private void SendPositonData(JsonData data)
        {
            wsServer.WebSocketServices["/Dal200"].Sessions.BroadcastAsync(JsonConvert.SerializeObject(data), null);
        }

        //TODO: Evaluate if we really need this
        private void UpdateActiveUsersDict(JsonData positionData)
        {
            var trackedIDs = new List<int>();
            foreach (var tracked in positionData.trackerData)
            {
                trackedIDs.Add(tracked.id);
                DtdtSubject subject;
                if (!activeUsers.TryGetValue(tracked.id, out subject))
                {
                    subject = new DtdtSubject(tracked.id, tracked.position);
                    activeUsers.Add(subject.DtdtId,subject);
                }
                
            }     
        }
    }
}
