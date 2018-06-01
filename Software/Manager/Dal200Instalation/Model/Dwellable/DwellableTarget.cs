using System;

namespace Dal200Instalation.Model.Dwellable
{
    public class DwellableTarget
    {
        public Point Position { get; }
        public Guid UUID { get; }
        public string Label { get; }
        public int Type { get; }
        public int Page { get; }

        private DateTime timeDwellDetected;
        private bool isAlreadyIn = false;

        public DwellableTarget(Point position, string label, int type, int page)
        {
            Position = position;
            timeDwellDetected = DateTime.UtcNow;
            UUID = Guid.NewGuid();
            Label = label;
            Type = type;
            Page = page;
        }

        public bool DetectDwell(Tracked positionData, int radius, TimeSpan time)
        {
            if (Point.IsInsideRaidus(Position, positionData.position, radius))
            {
                //TODO: this does not have to be an int =] a bool will do
                if(!isAlreadyIn)
                    timeDwellDetected = DateTime.UtcNow;
                isAlreadyIn = true;

                var timeDiff = (DateTime.UtcNow) - timeDwellDetected;
                if (timeDiff > time)
                {
                    timeDwellDetected = DateTime.UtcNow;
                    return true;
                }

            }
            else
            {
                isAlreadyIn = false;
                timeDwellDetected = DateTime.UtcNow;
                return false;
            }
            
            return false;
        }


    }
}
