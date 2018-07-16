using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WebSocketSharp;

namespace WebsocketListener
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var ws = new WebSocket("ws://192.168.1.120/Dal200"))
            {
                ws.OnMessage += (sender, e) =>
                    Console.WriteLine(e.Data);

                ws.OnClose += (sender, eventArgs) =>
                    Console.WriteLine("Connection closed!!!");



                ws.Connect();
                Console.WriteLine("Hit any key to close WS");
                Console.ReadKey(true);
            }
        }
    }
}
