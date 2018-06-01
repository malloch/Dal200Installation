using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dal200Instalation.Model.Dwellable;
using Dal200Instalation.Model.JsonRepresentation;
using Newtonsoft.Json;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace Dal200Instalation.Model
{
    class Dall200Messages : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs e)
        {
            Console.WriteLine(e.Data);
            var targets = new Targets();
            DwellableCollection dwellableCollection = new DwellableCollection(2, TimeSpan.Zero);
            //TODO: Remove hardoced file name
            dwellableCollection.LoadTargetsFromFile("testTargets");
            foreach (var target in dwellableCollection.dwellableTargets)
            {
                targets.targets.Add(target);
            }
            Send(JsonConvert.SerializeObject(targets));
        }
    }
}
