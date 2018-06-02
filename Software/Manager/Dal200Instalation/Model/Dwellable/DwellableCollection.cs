using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dal200Instalation.Model.Files;
using FileHelpers;

namespace Dal200Instalation.Model.Dwellable
{
    public class DwellableCollection
    {
        public delegate void SubjectIsDweling(Tracked trackData);
        public event SubjectIsDweling OnDwellDetected;

        public readonly List<DwellableTarget> dwellableTargets;
        private readonly TimeSpan time;
        private int radius;

        public DwellableCollection(int toleranceRadius, TimeSpan detectionTimeSpan)
        {
            dwellableTargets = new List<DwellableTarget>();
            time = detectionTimeSpan;
            radius = toleranceRadius;
        }

        public void ChangeRadius(int toleranceRadius)
        {
            radius = toleranceRadius;
        }

        public void AddTarget(DwellableTarget target)
        {
            dwellableTargets.Add(target);
        }

        public void LoadTargetsFromFile(string filename)
        {
            var fileHelpers = new DelimitedFileEngine<DwellableFileEntry>();
            var fileCollection = fileHelpers.ReadFile(filename);
            foreach (var entry in fileCollection)
            {
                var target = new DwellableTarget(new Point(entry.x,entry.y),entry.label,entry.type,entry.page);
                AddTarget(target);
            }
        }

        public void DetectDwell(JsonData positionData)
        {
            foreach (var target in dwellableTargets)
            {
                foreach (var tracked in positionData.trackerData)
                {
                    if (target.DetectDwell(tracked, radius, time))
                    {
                        var t = new Tracked(target.Page, target.Position, "track", target.Label);
                        OnDwellDetected?.Invoke(t);
                    }
                }
            }
        }
    }
}
          
