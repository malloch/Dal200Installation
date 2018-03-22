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
        public delegate void SubjectIsDweling(int subjectId);
        public event SubjectIsDweling OnDwellDetected;

        public readonly List<DwellableTarget> dwellableTargets;
        private readonly TimeSpan time;
        private readonly int radius;

        public DwellableCollection(int toleranceRadius, TimeSpan detectionTimeSpan)
        {
            dwellableTargets = new List<DwellableTarget>();
            time = detectionTimeSpan;
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
                var target = new DwellableTarget(new Point(entry.x,entry.y));
                AddTarget(target);
            }
        }

        public void SaveTargetsToFile(string filename)
        {
            var fileHelpers = new DelimitedFileEngine<DwellableFileEntry>();
            var fileCollection = new List<DwellableFileEntry>();
            foreach (var target in dwellableTargets)
            {
                fileCollection.Add(new DwellableFileEntry(
                                            target.Position.x, 
                                            target.Position.y));
            }

            fileHelpers.WriteFile(filename,fileCollection);
        }

        public void DetectDwell(JsonData positionData)
        {
            foreach (var target in dwellableTargets)
            {
                foreach (var tracked in positionData.trackerData)
                {
                    if (target.DetectDwell(tracked, radius, time))
                    {
                        OnDwellDetected?.Invoke(tracked.id);
                    }
                }
            }
        }
    }
}
          
