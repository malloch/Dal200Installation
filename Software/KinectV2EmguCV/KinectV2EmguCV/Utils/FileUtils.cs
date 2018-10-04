using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace KinectV2EmguCV.Utils
{
    class FileUtils
    {
        /// <summary>
        /// Wrapper around Windows UI to load the kinect
        /// background reference plane to a binary file
        /// </summary>
        /// <param name="frameWidth"></param>
        /// <param name="frameHeight"></param>
        /// <returns></returns>
        public static ushort[] LoadFrameFromFile(int frameWidth, int frameHeight)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "Frame Files | *.fra";
            bool? result = dialog.ShowDialog();

            ushort[] referenceFrame = null;

            if (result == true)
            {
                using (FileStream stream = new FileStream(dialog.FileName, FileMode.Open))
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    referenceFrame = new ushort[frameWidth * frameHeight];
                    var fileLengh = stream.Length;
                    var pos = 0;
                    while (pos < fileLengh)
                    {
                        referenceFrame[pos / sizeof(ushort)] = reader.ReadUInt16();
                        pos += sizeof(ushort);
                    }
                }
            }

            return referenceFrame;
        }

        /// <summary>
        /// Wrapper around Windows UI to save the kinect
        /// background reference plane to a binary file
        /// </summary>
        /// <param name="referenceFrame"></param>
        public static void SaveFrameToFile(ushort[] referenceFrame)
        {
            if (referenceFrame == null)
            {
                throw new ArgumentNullException("Reference frame cannot be null");
            }

            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.Filter = "Frame Files | *.fra";
            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                using (FileStream stream = new FileStream(dialog.FileName, FileMode.Create))
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    for (int i = 0; i < referenceFrame.Length; i++)
                    {
                        writer.Write(referenceFrame[i]);
                    }
                }
            }
        }

        /// <summary>
        /// Wrapper around Windows UI to load the kinect
        /// background reference plane to a image file
        /// </summary>
        /// <param name="referenceFrame"></param>
        /// <param name="frameWidth"></param>
        /// <param name="frameHeight"></param>
        public static void SaveReferenceFrameTo16BitImage(ushort[] referenceFrame, int frameWidth, int frameHeight)
        {
            if (referenceFrame == null)
            {
                throw new ArgumentNullException("Reference frame cannot be null");
            }

            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.Filter = "Image Files | *.png";
            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                Mat frame = new Mat(frameHeight, frameWidth, DepthType.Cv16U, 1);
                frame.SetTo(referenceFrame);
                frame.ToImage<Gray, ushort>().Save(dialog.FileName);
            }
        }
        
        /// <summary>
        /// Wrapper around the Windows UI to load an image
        /// used as the mask for the tracker
        /// </summary>
        /// <param name="frameWidth"></param>
        /// <param name="frameHeight"></param>
        /// <returns></returns>
        public static byte[] LoadMaskFromFile(int frameWidth, int frameHeight)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "Mask Files | *.jpg";
            bool? result = dialog.ShowDialog();

            byte[] maskPixels = null;
            if (result == true)
            {
                var bitmapSource = new BitmapImage(new Uri(dialog.FileName));
                maskPixels = new byte[frameWidth * frameHeight];
                bitmapSource.CopyPixels(maskPixels, frameWidth, 0);
            }

            return maskPixels;
        }

        /// <summary>
        /// Save an image to file
        /// </summary>
        /// <param name="image"></param>
        /// <returns>True if file was saved</returns>
        public static bool SaveImageToFile(BitmapSource image)
        {
            if (image == null)
                return false;

            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.Filter = "Image Files | *.png";
            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                using (var fileStream = new FileStream(dialog.FileName,FileMode.Create))
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(image));
                    encoder.Save(fileStream);
                }

                return true;
            }

            return false;
        }

    }
}
