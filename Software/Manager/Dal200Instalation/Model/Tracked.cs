using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dal200Instalation.Model
{
    public class Tracked
    {
        public int id { get; }
        public int[] position;
        public string track;
        public string mediaName;

        public Tracked(int id, int x, int y)
        {
            this.id = id;
            position = new[] {x, y};
        }

        public Tracked(int id, int x, int y, string track, string mediaName)
        {
            this.id = id;
            position = new[] { x, y };
            this.track = track;
            this.mediaName = mediaName;
        }
    }

    public class JsonData
    {
        public List<Tracked> trackerData;

        public JsonData()
        {
            trackerData = new List<Tracked>();
        }

    }
}
