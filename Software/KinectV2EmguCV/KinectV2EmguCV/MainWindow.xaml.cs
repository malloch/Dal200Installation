using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Microsoft.Kinect;

using System.Drawing;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using Emgu.CV.Util;
using Rug.Osc;
using Image = System.Windows.Controls.Image;
using PointF = System.Drawing.PointF;


namespace KinectV2EmguCV
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region CV
        private SimpleBlobDetector blobDetector;
        private SimpleBlobDetectorParams blobParams;
        private byte[] binaryImage;
        private Mat blobTrackerMaskMat;
        private byte[] maskPixel;
        #endregion

        #region ImageSources
        private BitmapSource rawKinectBitmapSource;
        private BitmapSource binaryMaskBitmapSource;
        private BitmapSource cannyBitmapSource;
        private BitmapSource trackerBitmapSourceSource;
        #endregion

        #region Kinect

        private DepthFrameReader depthFrameReader;
        private ushort[] depthFrameData;
        private int frameHeight;
        private int frameWidth;
        private ushort minTrack;
        private ushort maxTrack;

        #endregion

        #region OSC
        private OscSender oscSender;
        #endregion

        private ushort[] referenceFrame;
        private int remaningReferenceSamples = 300;
        private readonly StreamWriter dataWriter;
        
        public MainWindow()
        {
            InitializeComponent();
            SetUpKinect();
            UpdateBlobParams(null, null);
            binaryImage = new byte[frameWidth*frameHeight];

            //dataWriter = new StreamWriter("data.csv");
            
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(0.1f);
            timer.Tick += DispactherTimerTick;
            timer.Start();

        }

        private void SetUpKinect()
        {
            var sensor = KinectSensor.GetDefault();
            sensor.Open();
            depthFrameReader = sensor.DepthFrameSource.OpenReader();
            frameHeight = sensor.DepthFrameSource.FrameDescription.Height;
            frameWidth = sensor.DepthFrameSource.FrameDescription.Width;
            minTrack = sensor.DepthFrameSource.DepthMinReliableDistance;
            maxTrack = sensor.DepthFrameSource.DepthMaxReliableDistance;
        }

        private void DispactherTimerTick(object sender, EventArgs e)
        {
            var depthFrame = depthFrameReader.AcquireLatestFrame();
            if (depthFrame == null)
            {
                return;
            }

            depthFrameData = new ushort[depthFrame.FrameDescription.LengthInPixels];
            depthFrame.CopyFrameDataToArray(depthFrameData);

            UpdateRawKinectImage(depthFrameData, 3000);
            if (remaningReferenceSamples > 0)
                CreateReferenceFrame(depthFrameData);
            else
            {
                CreateBinaryImage(depthFrameData);
                DetectPeople(frameWidth, frameHeight, binaryImage);
            }
            
            depthFrame.Dispose();
       }

        private void CreateBinaryImage(ushort[] depthFrameData)
        {
            for (int i = 0; i < depthFrameData.Length; i++)
            {
                ushort depthPixel = depthFrameData[i];
                if (depthPixel <= minTrack)
                    binaryImage[i] = 0;
                else
                {
                    if (referenceFrame[i] > minTrack)
                        binaryImage[i] = (byte) (depthPixel < referenceFrame[i] ? (byte) 255 : 0);
                    if (referenceFrame[i] == 0)
                        binaryImage[i] = 255;
                }
                    
                
            }
            binaryMaskBitmapSource = BitmapSource.Create(frameWidth, frameHeight, 96, 96, PixelFormats.Gray8, null, binaryImage,
                frameWidth);
            BinaryMask.Source = binaryMaskBitmapSource;
        }

        private void CreateReferenceFrame(ushort[] depthFrameData)
        {
            if (referenceFrame == null)
            {
                referenceFrame = new ushort[depthFrameData.Length];
                for (int i = 0; i < referenceFrame.Length; i++)
                {
                    referenceFrame[i] = 0;
                }

                for (int i = 0; i < depthFrameData.Length; i++)
                {
                    ushort depthPixel = depthFrameData[i];
                    referenceFrame[i] = depthPixel > minTrack ? depthPixel : (byte)0;
                }
            }
            else
            {
                for (int i = 0; i < depthFrameData.Length; i++)
                {
                    ushort depthPixel = depthFrameData[i];
                    if (depthPixel > minTrack)
                        referenceFrame[i] = depthPixel < referenceFrame[i] ? depthPixel : referenceFrame[i];
                }
            }

            remaningReferenceSamples--;
        }

        private void UpdateRawKinectImage(ushort[] depthFrameData, int MaxDistance)
        {
            var imageArray = new byte[depthFrameData.Length];
            for (int i = 0; i < depthFrameData.Length; i++)
            {
                imageArray[i] = (byte) (255 - ((float)depthFrameData[i] / 4000 * 255));
            }
            rawKinectBitmapSource = BitmapSource.Create(frameWidth,frameHeight,96,96,
                PixelFormats.Gray8,null,imageArray,frameWidth);
            RawKinectImage.Source = rawKinectBitmapSource;
        }

        private void DetectPeople(int w, int h, byte[] img)
        {
            
            Mat workingMatrix = new Mat(h, w, DepthType.Cv8U, 1);
            if(blobTrackerMaskMat != null)
            for (int i = 0; i < img.Length; i++)
            {
                if (maskPixel[i] == 0)
                    img[i] = 0;
            }


            workingMatrix.SetTo(img);
            var workingImage = workingMatrix.ToImage<Gray, byte>().PyrDown().PyrUp();

            workingImage = workingImage.SmoothBlur(10, 10, true);

            Image<Gray, byte> cannyEdges = workingImage.Canny(130, 150);
            CannyMat.Source = ToBitmapSource(workingImage);
            Mat filled = Mat.Zeros(h, w, DepthType.Cv8U, 3);

            VectorOfKeyPoint kp = new VectorOfKeyPoint();
            if(blobTrackerMaskMat != null)
                blobDetector.DetectRaw(workingImage, kp, blobTrackerMaskMat);
            else
                blobDetector.DetectRaw(workingImage, kp);
            Tracked.Text = kp.Size.ToString();
            Features2DToolbox.DrawKeypoints(workingImage.Mat, kp, filled, new Bgr(0, 0, 255));//,Features2DToolbox.KeypointDrawType.DrawRichKeypoints);
            if (kp.Size > 0)
            {
             //   dataWriter.WriteLine($"{kp[0].Point.X},{kp[0].Point.Y}");
                
                oscSender?.Send(new OscMessage("/DTDT", kp[0].Point.X, kp[0].Point.Y));
            }

            kp.Dispose();

            using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
            {
                CvInvoke.FindContours(cannyEdges, contours, null, RetrType.External,
                    ChainApproxMethod.ChainApproxSimple);

                Image<Bgr, byte> filled2 = filled.ToImage<Bgr, byte>();
                for (int i = 0; i < contours.Size; i++)
                {
                    using (VectorOfPoint contour = contours[i])
                    using (VectorOfPoint approxContour = new VectorOfPoint())
                    {
                        CvInvoke.ApproxPolyDP(contour, approxContour, CvInvoke.ArcLength(contour, true) * 0.01, true);
                        //CvInvoke.DrawContours(filled,contours,i,new MCvScalar(0,0,255),2, LineType.Filled);
                        //  filled2.DrawPolyline(approxContour.ToArray(),true,new Bgr(0,0,255),2);
                    }
                }
            }

            TrackMat.Source = ToBitmapSource(filled);
        }

        /// <summary>
        /// Delete a GDI object
        /// </summary>
        /// <param name="o">The poniter to the GDI object to be deleted</param>
        /// <returns></returns>
        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);

        /// <summary>
        /// Convert an IImage to a WPF BitmapSource. The result can be used in the Set Property of Image.Source
        /// </summary>
        /// <param name="image">The Emgu CV Image</param>
        /// <returns>The equivalent BitmapSource</returns>
        public static BitmapSource ToBitmapSource(IImage image)
        {
            using (System.Drawing.Bitmap source = image.Bitmap)
            {
                IntPtr ptr = source.GetHbitmap(); //obtain the Hbitmap

                BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    ptr,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

                DeleteObject(ptr); //release the HBitmap
                return bs;
            }
        }

        private void LoadImage(ref byte[] target, int width)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "PNG Files | *.jpg";
            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                string filename = dialog.FileName;
                var bitmapSource = new BitmapImage(new Uri(filename));
                bitmapSource.CopyPixels(target, width, 0);
            }
        }

        private void UpdateBlobParams(object sender, RoutedEventArgs e)
        {
            blobParams = new SimpleBlobDetectorParams();
            blobParams.FilterByArea = true;
            blobParams.FilterByColor = true;
            blobParams.FilterByCircularity = false;
            blobParams.FilterByConvexity = false;
            blobParams.FilterByInertia = false;

            blobParams.blobColor = (byte) 255;

            blobParams.MinThreshold = float.Parse(MinTh.Text);
            

            blobParams.MinArea = float.Parse(MinArea.Text);
            blobParams.MinArea *= blobParams.MinArea;
            blobParams.MinCircularity = float.Parse(MinCirc.Text);
            blobParams.MinConvexity = float.Parse(MinConv.Text);
            blobParams.MinDistBetweenBlobs = float.Parse(MinDist.Text);
            blobParams.MinInertiaRatio = float.Parse(MinInertia.Text);
            
            blobDetector = new SimpleBlobDetector(blobParams);
        }

        private void LoadFrame(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "Frame Files | *.fra";
            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                using (FileStream stream = new FileStream(dialog.FileName,FileMode.Open))
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    referenceFrame = new ushort[frameWidth * frameHeight];
                    var fileLengh = stream.Length;
                    var pos = 0;
                    while (pos < fileLengh)
                    {
                        referenceFrame[pos/sizeof(ushort)] = reader.ReadUInt16();
                        pos += sizeof(ushort);
                    }
                }
                SaveReferenceFrameTo16BitImage();
                remaningReferenceSamples = 0;
            }
        }

        private void SaveFrame(object sender, RoutedEventArgs e)
        {
            if (referenceFrame == null)
            {
                System.Windows.MessageBox.Show("Reference frame is not initialized yet");
                return;
            }

            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.Filter = "Frame Files | *.fra";
            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                using (FileStream stream = new FileStream(dialog.FileName,FileMode.Create))
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    for (int i = 0; i < referenceFrame.Length; i++)
                    {
                        writer.Write(referenceFrame[i]);   
                    }
                }
            }
        }

        private void LoadMask(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "Mask Files | *.jpg";
            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                var bitmapSource = new BitmapImage(new Uri(dialog.FileName));
                maskPixel = new byte[frameWidth * frameHeight];
                bitmapSource.CopyPixels(maskPixel,frameWidth,0);

                blobTrackerMaskMat = new Mat(frameHeight, frameWidth, DepthType.Cv8U, 1);
                blobTrackerMaskMat.SetTo(maskPixel);
            }
        }

        private void SaveReferenceFrameTo16BitImage()
        {
            Mat frame = new Mat(frameHeight,frameWidth,DepthType.Cv16U,1);
            frame.SetTo(referenceFrame);

            frame.ToImage<Gray,ushort>().Save("hue.png");
        }

        private void StartOSC(object sender, RoutedEventArgs e)
        {
            oscSender = new OscSender(IPAddress.Parse(OscIp.Text), int.Parse(OscPort.Text));
            oscSender.Connect();
        }

        private void MouseDownImage(object sender, MouseButtonEventArgs e)
        {
            
            var controlSpacePosition = e.GetPosition((Image)sender);
            var imageControl = (Image) sender;
            var x = Math.Floor(controlSpacePosition.X * imageControl.Source.Width / imageControl.ActualWidth);
            var y = Math.Floor(controlSpacePosition.Y * imageControl.Source.Height / imageControl.ActualHeight);

            var originalX = x / imageControl.ActualWidth * frameWidth;
            var originalY = y / imageControl.ActualHeight * frameHeight;

            var depthSensorValue = depthFrameData[(int) x + (int) y * frameWidth];
            var referenceFrameValue = referenceFrame[(int)x + (int)y * frameWidth];
        }

        private void WindowClosing(object sender, CancelEventArgs e)
        {
            
            dataWriter?.Flush();
            dataWriter?.Close();
        }
    }

}

