using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using Emgu.CV;
using Emgu.CV.Structure;

namespace CountPixel2
{
    public partial class Form1 : Form
    {
        private VideoCapture _capture;
        private Thread _captureThread;
        private int _threshold = 150;
        private int hMin, hMax, sMin, sMax, vMin, vMax, hMinRed, hMaxRed, sMinRed, sMaxRed, vMinRed, vMaxRed = 100; //variables for HSV filtering
        private Robot L2bot;   


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _capture = new VideoCapture(1); //get the image from webcam
            _captureThread = new Thread(DisplayWebcam);
            _captureThread.Start();
            thresholdTrackBar.Value = _threshold;
            L2bot = new Robot("COM16"); //serial communication
        }


        private void DisplayWebcam()
        {
            while (_capture.IsOpened)
            {
                Mat frame = _capture.QueryFrame();

                int newHeight = (frame.Size.Height * emguPictureBox.Size.Width) / frame.Size.Width;
                Size newSize = new Size(emguPictureBox.Size.Width, newHeight);
                CvInvoke.Resize(frame, frame, newSize);
                emguPictureBox.Image = frame.Bitmap;

                //binary threshold
                Mat grayFrame = new Mat();
                Mat binaryFrame = new Mat();
                CvInvoke.CvtColor(frame, grayFrame, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);
                CvInvoke.Threshold(grayFrame, binaryFrame, _threshold, 255, Emgu.CV.CvEnum.ThresholdType.Binary);
                binaryPictureBox.Image = binaryFrame.Bitmap;
                
                Image<Gray, byte> binaryImage = binaryFrame.ToImage<Gray, byte>(); //convert to an Image type object

                List<int> PixelCount = new List<int>(); //list for the 7 slices (counting pixels in binary threshold)
                int SliceWidth = binaryFrame.Width / 7;


                for (int slice = 0; slice < 7; slice++)//for loops to access count the pixels in the 7 different slices
                {
                    int SliceXcoordinate = SliceWidth * slice;
                    PixelCount.Add(0);
                    for (int x = SliceXcoordinate; x < (SliceXcoordinate) + SliceWidth; x++)
                    {
                        for (int y = 0; y < binaryImage.Height; y++)
                        {
                            if (binaryImage.Data[y, x, 0] == 255) //if white pixel
                                PixelCount[slice]++;
                        }
                    }
                }

                //sending the number of pixels of each slice to the labels(binary)
                Invoke(new Action(() =>
                {
                    PixelCountLabel1.Text = $"{PixelCount[0]}";
                    PixelCountLabel2.Text = $"{PixelCount[1]}";
                    PixelCountLabel3.Text = $"{PixelCount[2]}";
                    PixelCountLabel4.Text = $"{PixelCount[3]}";
                    PixelCountLabel5.Text = $"{PixelCount[4]}";
                    PixelCountLabel6.Text = $"{PixelCount[5]}";
                    PixelCountLabel7.Text = $"{PixelCount[6]}";
                }));

                //HSV code
                Mat HsvFrame = new Mat();
                CvInvoke.CvtColor(frame, HsvFrame, Emgu.CV.CvEnum.ColorConversion.Bgr2Hsv);
                Mat[] HsvChannels = HsvFrame.Split();

                Mat HueFilter = new Mat();//Hue
                CvInvoke.InRange(HsvChannels[0], new ScalarArray(hMin), new ScalarArray(hMax), HueFilter);
                Invoke(new Action(() => { HuePictureBox.Image = HueFilter.Bitmap; }));

                Mat SaturationFilter = new Mat();//Saturation
                CvInvoke.InRange(HsvChannels[1], new ScalarArray(sMin), new ScalarArray(sMax), SaturationFilter);
                Invoke(new Action(() => { SaturationPictureBox.Image = SaturationFilter.Bitmap; }));

                Mat ValueFilter = new Mat();//Value
                CvInvoke.InRange(HsvChannels[2], new ScalarArray(vMin), new ScalarArray(vMax), ValueFilter);
                Invoke(new Action(() => { ValuePictureBox.Image = ValueFilter.Bitmap; }));
                

                Mat MergedImageYellow = new Mat(); //merged image (yellow line)
                CvInvoke.BitwiseAnd(HueFilter, SaturationFilter, MergedImageYellow);
                CvInvoke.BitwiseAnd(MergedImageYellow, ValueFilter, MergedImageYellow);

                Invoke(new Action(() => { YellowLinePictureBox.Image = MergedImageYellow.Bitmap; }));

                ////////////////////////////
                // YELLOW LINE PIXEL COUNT//
                ////////////////////////////
                Image<Gray, byte> YellowPixelCount = MergedImageYellow.ToImage<Gray, byte>();//converting the MAt object to an Image object to access the pixels


                List<int> PixelCount1 = new List<int>(); //list for the 5 slices
                int SliceWidth1 = MergedImageYellow.Width / 5;


                for (int slice1 = 0; slice1 < 5; slice1++) //for loops to count the yellow pixels in the 5 different slices
                {
                    int SliceXcoordinate1 = SliceWidth1 * slice1;
                    PixelCount1.Add(0);
                    for (int x = SliceXcoordinate1; x < (SliceXcoordinate1) + SliceWidth1; x++)
                    {
                        for (int y = 0; y < binaryImage.Height; y++)
                        {
                            if (YellowPixelCount.Data[y, x, 0] == 255)//if yellow pixel then ++
                                PixelCount1[slice1]++;
                        }
                    }
                }

                //sending the number of pixel to the labels
                Invoke(new Action(() =>
                {
                    label1.Text = $"{PixelCount1[0]}";
                    label2.Text = $"{PixelCount1[1]}";
                    label3.Text = $"{PixelCount1[2]}";
                    label4.Text = $"{PixelCount1[3]}";
                    label5.Text = $"{PixelCount1[4]}";
                }));
                //////////////////////////
                ///RED LINE PIXEL COUNT///
                //////////////////////////
                
                //same idea as for the yellow line
              
                Mat HueFilter1 = new Mat();//hue
                CvInvoke.InRange(HsvChannels[0], new ScalarArray(hMinRed), new ScalarArray(hMaxRed), HueFilter1);
                Invoke(new Action(() => { HuePictureBoxRed.Image = HueFilter1.Bitmap; }));

                Mat SaturationFilter1 = new Mat();//saturation
                CvInvoke.InRange(HsvChannels[1], new ScalarArray(sMinRed), new ScalarArray(sMaxRed), SaturationFilter1);
                Invoke(new Action(() => { SaturationPictureBoxRed.Image = SaturationFilter1.Bitmap; }));

                Mat ValueFilter1 = new Mat();//value 
                CvInvoke.InRange(HsvChannels[2], new ScalarArray(vMinRed), new ScalarArray(vMaxRed), ValueFilter1);
                Invoke(new Action(() => { ValuePictureBoxRed.Image = ValueFilter1.Bitmap; }));

                Mat MergedImageRed = new Mat(); //red line
                CvInvoke.BitwiseAnd(HueFilter1, SaturationFilter1, MergedImageRed);
                CvInvoke.BitwiseAnd(MergedImageRed, ValueFilter1, MergedImageRed);

                Invoke(new Action(() => { RedLinePictureBox.Image = MergedImageRed.Bitmap; }));

                Image<Gray, byte> RedPixelCount = MergedImageRed.ToImage<Gray, byte>();//convert mat object to image objet
                //do not need to use slices here
                //i just count the number of red pixels on the image
                int PixelCountRed=0;
                for (int x = 0; x < MergedImageRed.Width; x++) 
                {
                    for (int y = 0; y < binaryImage.Height; y++) 
                    {
                        if (RedPixelCount.Data[y, x, 0] == 255)
                            PixelCountRed++; 
                    }
                }
                Invoke(new Action(() =>
                {
                    LabelRedPixelCount.Text = $"{PixelCountRed}";//send to label
                }));

                ///////////////////
                ///If statements///
                ///////////////////

                //these if statements determine which of the 5 slices has the most yellow pixels, then sends a command to the robot
                Byte nextInstruction = Robot.FORWARD;

                if (PixelCount1[0] > PixelCount1[1] && PixelCount1[0] > PixelCount1[2] && PixelCount1[0] > PixelCount1[3] && PixelCount1[0] > PixelCount1[4])
                {
                    nextInstruction = Robot.LEFT2; //if the left line has the most pixel then hard turn to the left 
                    L2bot.Move(nextInstruction);
                }
                else if (PixelCount1[1] > PixelCount1[0] && PixelCount1[1] > PixelCount1[2] && PixelCount1[1] > PixelCount1[3] && PixelCount1[1] > PixelCount1[4])
                {
                    nextInstruction = Robot.LEFT;// same idea, slight turn left
                    L2bot.Move(nextInstruction);
                }
                else if (PixelCount1[2] > PixelCount1[0] && PixelCount1[2] > PixelCount1[1] && PixelCount1[2] > PixelCount1[3] && PixelCount1[2] > PixelCount1[4])
                {
                    nextInstruction = Robot.FORWARD;//go forward
                    L2bot.Move(nextInstruction); 
                }
                else if (PixelCount1[3] > PixelCount1[0] && PixelCount1[3] > PixelCount1[1] && PixelCount1[3] > PixelCount1[2] && PixelCount1[3] > PixelCount1[4])
                {
                    nextInstruction = Robot.RIGHT;//slight turn right
                    L2bot.Move(nextInstruction);
                }
                else if (PixelCount1[4] > PixelCount1[0] && PixelCount1[4] > PixelCount1[1] && PixelCount1[4] > PixelCount1[2] && PixelCount1[4] > PixelCount1[3])
                {
                    nextInstruction = Robot.RIGHT2; //hard turn right
                    L2bot.Move(nextInstruction);
                };

                if (PixelCountRed > 800) //if there is more than 800 red pixels captured by the camera, then stop the robot
                {
                    nextInstruction = Robot.STOP;
                    L2bot.Move(nextInstruction);
                }
                
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _captureThread.Abort();
        }


        private void thresholdTrackBar_Scroll(object sender, EventArgs e)
        {
            _threshold = thresholdTrackBar.Value;
        }

        private void HueTrackBar1_Scroll(object sender, EventArgs e)
        {
            hMin = HueTrackBar1.Value;
        }

        private void HueTrackBar2_Scroll(object sender, EventArgs e)
        {
            hMax = HueTrackBar2.Value;
        }

        private void HueTrackBarRed1_Scroll(object sender, EventArgs e)
        {
            hMinRed = HueTrackBarRed1.Value;
        }

        private void HueTrackBarRed2_Scroll(object sender, EventArgs e)
        {
            hMaxRed = HueTrackBarRed2.Value;
        }


        private void SaturationTrackBar1_Scroll(object sender, EventArgs e)
        {
            sMin = SaturationTrackBar1.Value;
        }

        private void SaturationTrackBar2_Scroll(object sender, EventArgs e)
        {
            sMax = SaturationTrackBar2.Value;
        }


        private void SaturationTrackBarRed1_Scroll(object sender, EventArgs e)
        {
            sMinRed = SaturationTrackBarRed1.Value;
        }

        private void SaturationTrackBarRed2_Scroll(object sender, EventArgs e)
        {
            sMaxRed = SaturationTrackBarRed2.Value;
        }
        private void ValueTrackBar1_Scroll(object sender, EventArgs e)
        {
            vMin = ValueTrackBar1.Value;
        }

        private void ValueTrackBar2_Scroll(object sender, EventArgs e)
        {
            vMax = ValueTrackBar2.Value;
        }
        
        private void ValueTrackBarRed1_Scroll(object sender, EventArgs e)
        {
            vMinRed = ValueTrackBarRed1.Value;
        }

        private void ValueTrackBarRed2_Scroll(object sender, EventArgs e)
        {
            vMaxRed = ValueTrackBarRed2.Value;
        }
    }
}