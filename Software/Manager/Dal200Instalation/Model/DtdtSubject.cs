using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dal200Instalation.Model
{
    internal class DtdtSubject
    {
        public Point CurrentPosition { get; set; }
        public int DtdtId { get; set; }
        public Track MyTrack { get; set; }

        public enum Track
        {
            Undecided,
            International,
            Canadian,
            NovaScotian
        }


        public DtdtSubject(int id, Point currentPosition)
        {
            DtdtId = id;
            CurrentPosition = currentPosition;
            MyTrack = Track.Undecided;
        }

        public void UpdatePosition(Point pos)
        {
            CurrentPosition = pos;
            
        }

        
        
    }
}
