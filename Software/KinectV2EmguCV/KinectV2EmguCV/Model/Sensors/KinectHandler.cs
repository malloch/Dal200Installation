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

        /// <summary>
        /// Array to the last captured depth frame. Use this instead
        /// of trying to access the Kinect data directly if you are not
        /// trying to access the frame withing the kinect refresh cycle.
        /// </summary>
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

        /// <summary>
        /// Forces the kinect SDK to refresh the kinect and TRY to get
        /// an depth frame from the sensor. Call this method once in the
        /// application refresh cycle. There is a maximum refresh rate
        /// defined in the SDK.
        /// </summary>
        /// <returns></returns>
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