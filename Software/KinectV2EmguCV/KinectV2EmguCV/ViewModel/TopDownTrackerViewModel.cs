using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using KinectV2EmguCV.Annotations;
using KinectV2EmguCV.Model.Sensors;
using KinectV2EmguCV.Model.Tracking;
using KinectV2EmguCV.Model.Tracking.OCV;
using KinectV2EmguCV.Utils;
using Rug.Osc;

namespace KinectV2EmguCV.ViewModel
{
    class TopDownTrackerViewModel: INotifyPropertyChanged
    {
        #region UIProperties
        public string OscIp { get; set; } = "127.0.0.1";
        public int OscPort { get; set; } = 6666;
        public float MinimumBlobArea { get; set; } = 20;
        public float BlobThreshold { get; set; } = 50;

        public ImageSource KinectImageSource
        {
            get { return kinectImageSource; }
            private set
            {
                kinectImageSource = value;
                OnPropertyChanged(nameof(KinectImageSource));
            }
        }

        public ImageSource BlobDetectionImageSource
        {
            get { return blobDetectionImageSource;}
            set
            {
                blobDetectionImageSource = value;
                OnPropertyChanged(nameof(BlobDetectionImageSource));
            }
        }

        public int BlobsDetected
        {
            get { return blobsDetected; }
            set
            {
                blobsDetected = value;
                OnPropertyChanged(nameof(BlobsDetected));
            }
        }


        #endregion

        #region Commands

        private ICommand startOscCommand;

        public ICommand StartOscCommand
        {
            get { return startOscCommand ?? (startOscCommand = new RelayCommand(call => StartOsc())); }
        }

        private ICommand updateBlobSettingsCommand;

        public ICommand UpdateBlobSettingsCommand
        {
            get { return updateBlobSettingsCommand ?? (updateBlobSettingsCommand = new RelayCommand(call => UpdateBlobSettings())); }
        }

        private ICommand loadReferenceFrameCommand;

        public ICommand LoadReferenceFrameCommand
        {
            get { return loadReferenceFrameCommand ?? (loadReferenceFrameCommand = new RelayCommand(call => LoadReferenceFrame())); }
        }

        private ICommand loadMaskCommand;

        public ICommand LoadMaskCommand
        {
            get { return loadMaskCommand ?? (loadMaskCommand = new RelayCommand(call => LoadMaskImage())); }
        }
        private ICommand closeCommand;

        public ICommand CloseCommand
        {
            get { return closeCommand ?? (closeCommand = new RelayCommand(call => HandleShutdown())); }
        }

        private ICommand saveMaskCommand;

        public ICommand SaveMaskCommand
        {
            get { return saveMaskCommand ?? (saveMaskCommand = new RelayCommand(call => SaveReferenceFrame())); }
        }

        private ICommand saveSnapshotCommand;

        public ICommand SaveSnapshotCommand
        {
            get { return saveSnapshotCommand ?? (saveSnapshotCommand = new RelayCommand(call => SaveSnapshot())); }
        }

        #endregion

        private readonly KinectHandler kinectHandler;
        private ITopDownTrackingStrategy trackingStrategy;
        private OscSender oscSender;
        private const int KinectRefreshRate = 10;
        private ImageSource kinectImageSource;
        private ImageSource blobDetectionImageSource;
        private int blobsDetected = 0;

        public TopDownTrackerViewModel()
        {
            //kinectImageSource = new BitmapImage(new Uri("c:\\lenna.png"));

            kinectHandler = KinectHandler.Instance;
            trackingStrategy = new SimpleKinectBlobTracker();
            LoadReferenceFrame();
            LoadMaskImage();
            KinectCycle();

            var uiRefreshTimer = new DispatcherTimer();
            uiRefreshTimer.Interval = TimeSpan.FromMilliseconds(500);
            uiRefreshTimer.Tick += UpdateUIImages;
            uiRefreshTimer.Start();

        }

        private void StartOsc()
        {
            oscSender = new OscSender(IPAddress.Parse(OscIp), OscPort);
            oscSender.Connect();
        }

        private void UpdateBlobSettings()
        {
            var tracker = trackingStrategy as SimpleKinectBlobTracker;
            tracker?.UpdateBlobParameters(MinimumBlobArea,BlobThreshold);
        }

        private async void KinectCycle()
        {
            while (kinectHandler.IsKinectOpen)
            {
                var point = trackingStrategy.DetectSinglePerson(kinectHandler.GetDepthFrame());
                if(point.HasValue)
                    oscSender?.Send(new OscMessage("/DTDT", point.Value.X, point.Value.Y));
                await Task.Delay(KinectRefreshRate);
            }
        }

        private void LoadMaskImage()
        {
            var mask = FileUtils.LoadMaskFromFile(kinectHandler.FrameWidth, kinectHandler.FrameHeight);
            var tracker = trackingStrategy as SimpleKinectBlobTracker;
            tracker?.SetTrackerMask(mask, kinectHandler.FrameHeight,kinectHandler.FrameWidth);
        }

        private void LoadReferenceFrame()
        {
            var reference = FileUtils.LoadFrameFromFile(kinectHandler.FrameWidth, kinectHandler.FrameHeight);
            var tracker = trackingStrategy as SimpleKinectBlobTracker;
            if (tracker != null) tracker.BackgroundReference = reference;
        }

        private void SaveReferenceFrame()
        {
            var tracker = trackingStrategy as SimpleKinectBlobTracker;
            if (tracker != null && tracker.IsBackgroundCaptured)
                FileUtils.SaveFrameToFile(tracker.BackgroundReference);
        }

        private void SaveSnapshot()
        {
            FileUtils.SaveImageToFile((BitmapSource) kinectImageSource);
            FileUtils.SaveImageToFile((BitmapSource) blobDetectionImageSource);

            if (trackingStrategy is SimpleKinectBlobTracker tracker)
                FileUtils.SaveReferenceFrameTo16BitImage(kinectHandler.LastCapturedDepthFrame,
                    kinectHandler.FrameWidth,
                    kinectHandler.FrameHeight);
        }

        private void UpdateUIImages(object sender, EventArgs e)
        {
            if(!kinectHandler.IsKinectOpen)
                return;

            if (kinectHandler.LastCapturedDepthFrame != null)
            {
                var imageArray = new byte[kinectHandler.LastCapturedDepthFrame.Length];
                for (int i = 0; i < kinectHandler.LastCapturedDepthFrame.Length; i++)
                {
                    imageArray[i] = (byte) (255 - ((float) kinectHandler.LastCapturedDepthFrame[i] / 4000 * 255));
                }

                KinectImageSource = BitmapSource.Create(kinectHandler.FrameWidth, kinectHandler.FrameHeight, 96, 96,
                    PixelFormats.Gray8, null, imageArray, kinectHandler.FrameWidth);
                
            }

            var tracker = trackingStrategy as SimpleKinectBlobTracker;
            if (tracker != null && tracker.BlobDetectedImage != null)
            {
                BlobDetectionImageSource = CvUtils.ToBitmapSource(tracker.BlobDetectedImage);
                BlobsDetected = tracker.BlobsDetected;
            }



        }

        private void HandleShutdown()
        {
            kinectHandler?.Close();
            oscSender?.Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
