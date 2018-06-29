using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KinectV2EmguCV.Model.Tracking;

namespace KinectV2EmguCV.Model.Sensors
{
    public sealed class KinectTrackableSource: ITrackableSource
    {
        public ushort[] DepthFrameData { get; set; }
        public int FrameHeight { get; }
        public int FrameWidth { get; }
        public ushort MinimumRealiableTrackingDistance { get; }
        public ushort MaximumRealiableTrackingDistance { get; }

        public KinectTrackableSource(uint size, ushort minimumRealiableTrackingDistance, ushort maximumRealiableTrackingDistance, int frameHeight, int frameWidth)
        {
            MinimumRealiableTrackingDistance = minimumRealiableTrackingDistance;
            MaximumRealiableTrackingDistance = maximumRealiableTrackingDistance;
            FrameHeight = frameHeight;
            FrameWidth = frameWidth;
            DepthFrameData = new ushort[size];
        }
    }
}
