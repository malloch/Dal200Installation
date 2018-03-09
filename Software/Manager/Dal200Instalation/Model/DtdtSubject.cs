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

        public delegate void SubjectIsDweling(Point position);
        public event SubjectIsDweling OnDwellDetected;


        private DateTime timeDwellDetected;

        public DtdtSubject(int id)
        {
            DtdtId = id;
            CurrentPosition = new Point();
        }

        private void DetectDwell(string positionData, int radius, TimeSpan time)
        {
            if (Point.IsInsideRaidus(CurrentPosition, new Point(), radius))
            {
                if(DateTime.UtcNow - timeDwellDetected > time)
                    OnDwellDetected?.Invoke(CurrentPosition);
            }
            else
            {
                timeDwellDetected = DateTime.UtcNow;
            }
        }
    }
}
