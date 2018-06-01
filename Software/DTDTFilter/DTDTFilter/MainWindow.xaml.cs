using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Text;
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
using AssSaver.Annotations;
using Rug.Osc;
using Image = System.Drawing.Image;

namespace DTDTFilter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Image kinectGeneratedImage;
        private OscSender oscSender;
        //private readonly string oscSendAddr = "127.0.0.1";
        private readonly string oscSendAddr = "192.168.0.117";
        private readonly int oscSendPort = 5108;

        private OscHandler oscHandler, oscHandler2;

        readonly int width = 150;
        readonly int height = 150;
        private byte[] kinect1Mask, kinect2Mask;
        private List<int> filteredPLayerIndexes;

        private readonly int dtdtX = 8;
        private readonly int dtdtY = 9;

        private Dictionary<int, Tuple<int, int>> oldTrackingValues;

        private enum ApplicationState
        {
            MaskCreation, Filtering
        }

        private ApplicationState applicationState;
        private readonly DispatcherTimer timer;
       
        public MainWindow()
        {
            InitializeComponent();
            applicationState = ApplicationState.MaskCreation;
            filteredPLayerIndexes = new List<int>();
            oldTrackingValues = new Dictionary<int, Tuple<int, int>>();

            kinect1Mask = new byte[width * height];
            kinect2Mask = new byte[width * height];


            oscHandler = new OscHandler(5106);
            
            oscHandler.OnDataReceived += data => NewKinectData(data, ref kinect1Mask, oscHandler.Port);
            oscHandler.StartReceiving();

            oscHandler2 = new OscHandler(5107);
            oscHandler2.OnDataReceived += data => NewKinectData(data, ref kinect2Mask, oscHandler2.Port);
            oscHandler2.StartReceiving();

            oscSender = new OscSender(IPAddress.Parse(oscSendAddr),0,oscSendPort);
            oscSender.Connect();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += UpdateImageSources;
            timer.Start();
            //KinectImage.Source = img;

        }

        private void UpdateImageSources(object sender, EventArgs eventArgs)
        {
            BitmapSource kinect1Source = BitmapSource.Create(width,height,96,96,PixelFormats.Gray8,null,kinect1Mask,width);
            Kinect1SourceImage.Source = kinect1Source;

            BitmapSource kinect2Source = BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, kinect2Mask, width);
            Kinect2SourceImage.Source = kinect2Source;

            
        }

        //TODO: get rid of this port requirement...
        private void UpdateFilteredImagesSources(int x, int y, int listenPort)
        {
            var img = new byte[width * height];
            img[x + (y * width)] = 255;

            BitmapSource filtered = BitmapSource.Create(width,height,96,96,PixelFormats.Gray8,null,img,width);
            filtered.Freeze();
            Application.Current.Dispatcher.Invoke((Action) delegate
            {
                if (listenPort == 5106)
                    Kinect1FilteredImage.Source = filtered;
                else
                    Kinect2FilteredImage.Source = filtered;


            });
            
            
        }
        
        private void NewKinectData(Rug.Osc.OscPacket data, ref byte[] mask, int listenPort)
        {
            var pkg = (OscMessage)data;

            int numPkgs = (int) pkg[1];
            for (int i = 0; i < numPkgs; i++)
            {
                int x = (int)pkg[dtdtX + (i * 8)];
                int y = (int)pkg[dtdtY + (i * 8)];

                switch (applicationState)
                {
                    case ApplicationState.MaskCreation:
                        AddToMask(ref mask, x, y);
                        FillNeighbours(ref mask, 8, x, y);
                        break;
                    case ApplicationState.Filtering:
                        FilterAndSend(data, mask, x, y, listenPort);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
        }
            //Console.WriteLine(data);


        }

        private void AddToMask(ref byte[] mask, int x, int y)
        {
            mask[x + (y * width)] = 255;
        }

        private void FillNeighbours(ref byte[] mask, int windowSize, int x, int y)
        {
            for (int i = windowSize - windowSize / 2; i < windowSize + windowSize / 2; i++)
            {
                for (int j = windowSize - windowSize / 2; j < windowSize + windowSize / 2; j++)
                {
                    var offsetX = x + i;
                    var offsetY = y + j;
                    if (offsetX > 0 && offsetX < width && offsetY > 0 && offsetY < height)
                    {
                        mask[offsetX + (offsetY * width)] = 255;
                    }
                }
            }
        }

        private void FilterAndSend(OscPacket data, byte[] mask, int x, int y, int listenPort)
        {
            if (mask[x + (y * width)] == 0)
            {

                //TODO: find the matching subject from the package and remove the others
                var filteredData = FindMatchingSubject(data, x, y);
                oscSender.Send(filteredData);
                UpdateFilteredImagesSources(x,y, listenPort);
            }
        }

        private OscPacket FindMatchingSubject(OscPacket indata, int x, int y)
        {
            var data = (OscMessage) indata;
            int numPkgs = (int)data[1];
            filteredPLayerIndexes.Clear();
            for (int i = 0; i < numPkgs; i++)
            {
                //Tuple<int, int> oldPos;
                //if (oldTrackingValues.TryGetValue((int)data[2 + (i * 8)], out oldPos))
                //{
                //    x = (int)(x * 0.2 + oldPos.Item1 * 0.8);
                //    y = (int)(y * 0.2 + oldPos.Item2 * 0.8);
                //    oldTrackingValues[(int)data[2 + (i * 8)]] = new Tuple<int, int>(x, y);
                //}
                //else
                //{
                //    oldTrackingValues.Add((int)data[2 + (i * 8)], new Tuple<int, int>(x, y));
                //}



                if ((int)data[dtdtX + (i * 8)] == x &&
                    (int)data[dtdtY + (i * 8)] == y)
                {
                    filteredPLayerIndexes.Add(i);
                    
                }
            }

            //var outboundPackage = new object[2 + filteredPLayerIndexes.Count *8];
            var outboundPackage = new List<object>();
            outboundPackage.Add(data[0]);
            outboundPackage.Add(filteredPLayerIndexes.Count);

            //TODO: This is still wrong =]
            for (int i = 0; i < filteredPLayerIndexes.Count; i++)
            {
                var selectedIndex = filteredPLayerIndexes[i];
                for (int j = 2 + selectedIndex*8; j < 10 + 8*selectedIndex; j++)
                {
                    outboundPackage.Add(data[j]);
                }
            }

            //foreach (var selectedIndex in filteredPLayerIndexes)
            //{
            //    for (int i = ; i < 10; i++)
            //    {
            //        outboundPackage[i] = data[i + (selectedIndex * 8)];
            //    }
            //}
            //outboundPackage[0] = data[0];
            //outboundPackage[1] = filteredPLayerIndexes.Count;
            
            
            var outbound = new OscMessage(data.Address,outboundPackage.ToArray());

            return outbound;

        }

        private void StartSendingFilteredData(object sender, RoutedEventArgs e)
        {
            ((Button) sender).Content = "Filtering";
            applicationState = ApplicationState.Filtering;
        }

        private void LoadMasks(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "PNG Files | *.png";
            dialog.FileName = "Kinect1 Mask";
            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                string filename = dialog.FileName;
                var bitmapSource = new BitmapImage(new Uri(filename));
                bitmapSource.CopyPixels(kinect1Mask, width, 0);
            }

            dialog.FileName = "Kinect2 Mask";
            result = dialog.ShowDialog();
            if (result == true)
            {
                string filename = dialog.FileName;
                var bitmapSource = new BitmapImage(new Uri(filename));
                bitmapSource.CopyPixels(kinect2Mask, width, 0);
            }

            applicationState = ApplicationState.Filtering;
            FilterButton.Content = "Filtering";
        }

        private void SaveMasks(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.Filter = "PNG Files | *.png";
            dialog.FileName = "Kinect1 Mask";
            bool? result = dialog.ShowDialog();

            if (result == true)
            {               
                string filename = dialog.FileName;
                using (var fileStream = new FileStream(filename, FileMode.Create))
                {
                    BitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(
                        BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, kinect1Mask, width)));
                    encoder.Save(fileStream);
                }
            }

            dialog.FileName = "Kinect2 Mask";
            result = dialog.ShowDialog();

            if (result == true)
            {
                string filename = dialog.FileName;
                using (var fileStream = new FileStream(filename, FileMode.Create))
                {
                    BitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(
                        BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, kinect2Mask, width)));
                    encoder.Save(fileStream);
                }
            }

        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            oscSender.Dispose();
            oscHandler.StopReceiving();
            oscHandler2.StopReceiving();

        }
    }
}
