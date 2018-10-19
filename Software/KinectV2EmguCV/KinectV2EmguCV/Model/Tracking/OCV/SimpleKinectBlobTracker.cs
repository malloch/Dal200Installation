using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using KinectV2EmguCV.Model.Sensors;

namespace KinectV2EmguCV.Model.Tracking.OCV
{
    /// <summary>
    /// Blob tracker strategy
    /// Uses OpenCV simple blob tracker to find one
    /// or more blobs. It fails for tall people
    /// </summary>
    class SimpleKinectBlobTracker: ITopDownTrackingStrategy
    {
        private SimpleBlobDetector blobDetector;
        private SimpleBlobDetectorParams blobParams;
        private ushort[] backgroundReference;
        private Mat blobTrackerMaskMat;
        private byte[] blobTrackerMaskPixels;
        private int referenceCount = 2000;

        public bool IsBackgroundCaptured { get; private set; }
        public ushort[] BackgroundReference
        {
            get { return backgroundReference; }
            set
            {
                if(value == null)
                    return;
                
                backgroundReference = value;
                referenceCount = -1;
                IsBackgroundCaptured = true;
            }
        }
        public Image<Bgr,byte> BlobDetectedImage { get; private set; }
        public int BlobsDetected { get; private set; }

        public SimpleKinectBlobTracker()
        {
            IsBackgroundCaptured = false;
            UpdateBlobParameters(20,50);
        }

        /// <summary>
        /// Creates a background noise removal reference
        /// if there isn't one yet.
        /// </summary>
        /// <param name="source"></param>
        public void CreteBackgroundReference(KinectTrackableSource source)
        {
            if (backgroundReference == null)
            {
                backgroundReference = new ushort[source.DepthFrameData.Length];
                for (int i = 0; i < backgroundReference.Length; i++)
                {
                    backgroundReference[i] = 0;
                }
                for (int i = 0; i < source.DepthFrameData.Length; i++)
                {
                    ushort depthPixel = source.DepthFrameData[i];
                    backgroundReference[i] = depthPixel > source.MinimumRealiableTrackingDistance ? depthPixel : (byte)0;
                }
            }
            else
            {
                for (int i = 0; i < source.DepthFrameData.Length; i++)
                {
                    ushort depthPixel = source.DepthFrameData[i];
                    if (depthPixel > source.MinimumRealiableTrackingDistance)
                        backgroundReference[i] = depthPixel < backgroundReference[i] ? depthPixel : backgroundReference[i];
                }
            }

            referenceCount--;
            if(referenceCount <0)
                IsBackgroundCaptured = true;
        }

        /// <summary>
        /// Defines ignore mask for the blob tracker
        /// Mask is 8-bit gray image
        /// </summary>
        /// <param name="maskPixels"></param>
        /// <param name="height"></param>
        /// <param name="width"></param>
        public void SetTrackerMask(byte[] maskPixels, int height, int width)
        {
            if(maskPixels == null)
                return;
            blobTrackerMaskPixels = maskPixels;
            blobTrackerMaskMat = new Mat(height,width,DepthType.Cv8U,1);
            blobTrackerMaskMat.SetTo(blobTrackerMaskPixels);
        }

        /// <summary>
        /// UI Handler for updating the parameters during runtime
        /// </summary>
        /// <param name="minimumArea"></param>
        /// <param name="minimumThreshold"></param>
        public void UpdateBlobParameters(float minimumArea, float minimumThreshold)
        {
            blobParams = new SimpleBlobDetectorParams();
            blobParams.FilterByArea = true;
            blobParams.FilterByColor = true;
            blobParams.FilterByCircularity = false;
            blobParams.FilterByConvexity = false;
            blobParams.FilterByInertia = false;

            blobParams.blobColor = (byte)255;
            blobParams.MinThreshold = minimumThreshold;
            blobParams.MinArea = minimumArea * minimumArea;
            blobDetector = new SimpleBlobDetector(blobParams);
        }

        /// <summary>
        /// Method for detecting a single person using the blob tracker
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public PointF? DetectSinglePerson(ITrackableSource source)
        {
            if (source == null)
                return null;
            
            var kinectData = (KinectTrackableSource)source;
            if (!IsBackgroundCaptured)
            {
                CreteBackgroundReference(kinectData);
                return null;
            }

            byte[] binaryImage = CreateBinaryImage(kinectData);
            if (blobTrackerMaskMat != null)
            {
                for (int i = 0; i < binaryImage.Length; i++)
                  if (blobTrackerMaskPixels[i] == 0)
                        binaryImage[i] = 0;
            }

            Mat binaryMatrix = new Mat(kinectData.FrameHeight, kinectData.FrameWidth, DepthType.Cv8U, 1);
            binaryMatrix.SetTo(binaryImage);

            using (var kp = DoBlobDetection(binaryMatrix))
            {
                BlobsDetected = 0;
                if (kp.Size > 0)
                {
                    BlobsDetected = kp.Size;
                    return kp[0].Point;
                }
            }

            return null;
        }

        /// <summary>
        /// Placeholder from interface
        /// Throws not implemented exception
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public Dictionary<int, PointF?> DetectMultiplePeople(ITrackableSource source)
        {
            throw new NotImplementedException("Simple Blob Tracker does not support multiple people yet");
        }

        private byte[] CreateBinaryImage(KinectTrackableSource source)
        {
            var binaryImage = new byte[source.FrameHeight * source.FrameWidth];
            for (int i = 0; i < source.DepthFrameData.Length; i++)
            {
                ushort depthPixel = source.DepthFrameData[i];
                if (depthPixel <= source.MinimumRealiableTrackingDistance)
                    binaryImage[i] = 0;
                else
                {
                    if (backgroundReference[i] > source.MinimumRealiableTrackingDistance)
                        binaryImage[i] = (byte)(depthPixel < backgroundReference[i] ? (byte)255 : 0);
                    if (backgroundReference[i] == 0)
                        binaryImage[i] = 255;
                }
            }

            return binaryImage;
        }
       
        private VectorOfKeyPoint DoBlobDetection(Mat sourceMat)
        {
            Image<Gray, byte> blobImage = sourceMat.ToImage<Gray, byte>().PyrDown().PyrUp();
            blobImage.SmoothBlur(10, 10, true);
            
            VectorOfKeyPoint kp = new VectorOfKeyPoint();
            if (blobTrackerMaskMat != null)
                blobDetector.DetectRaw(blobImage, kp, blobTrackerMaskMat);
            else
                blobDetector.DetectRaw(blobImage, kp);

            Mat decoratedMat = new Mat(sourceMat.Rows,sourceMat.Cols, DepthType.Cv8U,3);
            Features2DToolbox.DrawKeypoints(blobImage.Mat,kp,decoratedMat,new Bgr(0,0,255));
            BlobDetectedImage = decoratedMat.ToImage<Bgr,byte>();
            return kp;
        }

    }
}
