using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WebSocketSharp;

namespace Dal200Instalation.Model
{
    class WebSocketProjector
    {
        public WebSocket Socket { get; }
        public bool WebSocketConnected { get; private set; }

        public WebSocketProjector(string websockeAddress)
        {
            WebSocketConnected = false;
            Socket = new WebSocket(websockeAddress);
            Socket.OnOpen += (sender, e) => WebSocketConnected = true;
            Socket.OnClose += (sender, e) => WebSocketConnected = false;
        }

    }
}
