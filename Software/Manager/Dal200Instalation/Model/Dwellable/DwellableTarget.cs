using System;

namespace Dal200Instalation.Model.Dwellable
{
    public class DwellableTarget
    {
        public Point Position { get; }
        public Guid UUID { get; }
        public string Label { get; }

        private DateTime timeDwellDetected;

        public DwellableTarget(Point position)
        {
            Position = position;
            timeDwellDetected = DateTime.UtcNow;
            UUID = Guid.NewGuid();
        }

        public bool DetectDwell(Tracked positionData, int radius, TimeSpan time)
        {
            if (Point.IsInsideRaidus(Position, positionData.position, radius))
            {
                if (DateTime.UtcNow - timeDwellDetected > time)
                    return true;
            }
            else
            {
                timeDwellDetected = DateTime.UtcNow;
                return false;
            }
            timeDwellDetected = DateTime.UtcNow;
            return false;
        }


    }
}
