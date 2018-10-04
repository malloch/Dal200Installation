using System;
using Microsoft.Kinect;

namespace KinectV2EmguCV.Model.Sensors
{
    /// <summary>
    /// Lazy initialized singleton class that provides 
    /// access to Kinect Depth Frame Data
    /// </summary>
    public sealed class KinectHandler
    {
        private readonly DepthFrameReader depthFrameReader;
        private KinectTrackableSource kinectTrackableSource;
        private static readonly Lazy<KinectHandler> lazy = new Lazy<KinectHandler>(() => new KinectHandler());
        private readonly KinectSensor sensor;
        private KinectTrackableSource lastCollectedFrame;

        public int FrameHeight { get; }
        public int FrameWidth { get; }
        private readonly ushort minimumRealiableTrackingDistance;
        private readonly ushort maximumRealiableTrackingDistance;


        /// <summary>
        /// Singleton instance property
        /// </summary>
        public static KinectHandler Instance => lazy.Value;
        public bool IsKinectOpen { get; }

        public ushort[] LastCapturedDepthFrame => lastCollectedFrame?.DepthFrameData;

        private KinectHandler()
        {
            sensor = KinectSensor.GetDefault();
            sensor.Open();
            IsKinectOpen = sensor.IsOpen;
            if(!IsKinectOpen)
                return;

            depthFrameReader = sensor.DepthFrameSource.OpenReader();
            FrameHeight = sensor.DepthFrameSource.FrameDescription.Height;
            FrameWidth = sensor.DepthFrameSource.FrameDescription.Width;
            minimumRealiableTrackingDistance = sensor.DepthFrameSource.DepthMinReliableDistance;
            maximumRealiableTrackingDistance = sensor.DepthFrameSource.DepthMaxReliableDistance;
        }

        public KinectTrackableSource GetDepthFrame()
        {
            using (var depthFrame = depthFrameReader.AcquireLatestFrame())
            {
                if (depthFrame == null)
                    return null;//throw new ArgumentNullException("Could not get frame from Kinect");

                kinectTrackableSource = new KinectTrackableSource(depthFrame.FrameDescription.LengthInPixels,
                                                                    minimumRealiableTrackingDistance,
                                                                    maximumRealiableTrackingDistance,
                                                                    FrameHeight,FrameWidth );
                depthFrame.CopyFrameDataToArray(kinectTrackableSource.DepthFrameData);
            }

            lastCollectedFrame = kinectTrackableSource;
            return kinectTrackableSource;
        }

        public void Close()
        {
            sensor.Close();
        }
    }
}