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
        public Point position { get; set; }
        public string track;
        public string mediaName;

        public Tracked(int id, int x, int y)
        {
            this.id = id;
            position = new Point(x, y);
        }

        public Tracked(int id, int x, int y, string track, string mediaName)
        {
            this.id = id;
            position = new Point(x, y);
            this.track = track;
            this.mediaName = mediaName;
        }

        public Tracked(int id, Point pos, string track, string mediaName)
        {
            this.id = id;
            position = pos;
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

    public class DwellData
    {
        public int dwellIndex;
    }

}
