using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Kinect_VisualizandoDepthData
{
    /// <summary>
    /// Interação lógica para MainWindow.xam
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Variaveis
        private KinectSensor _Kinect;
        private WriteableBitmap _DepthImageBitmap;
        private Int32Rect _DepthImageBitmapRect;
        private int _DepthImageStride;
        #endregion Variaveis
        
        public MainWindow()
        {
            InitializeComponent();

            this.Loaded += (s, e) => { DiscoverKinectSensor(); };
            this.Unloaded += (s, e) => { this.Kinect= null; };
        }

        private void DiscoverKinectSensor()
        {
            KinectSensor.KinectSensors.StatusChanged += KinectSensors_StatusChanged;
            this.Kinect = KinectSensor.KinectSensors.FirstOrDefault(x => x.Status == KinectStatus.Connected);
        }

        private void KinectSensors_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            switch (e.Status)
            {
                case KinectStatus.Connected:
                    if(this.Kinect == null)
                    {
                        this.Kinect = e.Sensor;
                        Console.WriteLine("Mudança status: Conectado");
                    }
                    break;
                case KinectStatus.Disconnected:
                    if(this.Kinect == e.Sensor)
                    {
                        this.Kinect = null;
                        // Que q esse pedaço faz???
                        this.Kinect =
                            KinectSensor.KinectSensors.FirstOrDefault(x => x.Status == KinectStatus.Connected);
                        Console.WriteLine("Mudança status: Desconectado");
                        if (this.Kinect == null)
                        {
                            
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Atribuir o status do Kinect (habilitado ou desabilitado)
        /// </summary>
        public KinectSensor Kinect
        {
            get { return this._Kinect;  }
            set
            {
                if(this._Kinect != value)
                {
                    if(this._Kinect == null)
                    {
                        // Desabilita o Kinect
                        UnitializeKinectSensor(this._Kinect);
                        this._Kinect = null;
                    }

                    if(value != null && value.Status == KinectStatus.Connected)
                    {
                        // Habilita o Kinect
                        this._Kinect = value;
                        InitializeKinectSensor(this._Kinect);
                    }
                }
            }
        }

        /// <summary>
        /// Desabilita o sensor Kinect
        /// </summary>
        /// <param name="kinect"></param>
        private void UnitializeKinectSensor(KinectSensor kinect)
        {
            if(kinect != null)
            {
                kinect.Stop();
                this._Kinect.DepthFrameReady += Kinect_DepthFrameReady;
            }
        }

        private void InitializeKinectSensor(KinectSensor kinect)
        {
            if(kinect != null)
            {
                DepthImageStream depthSream = kinect.DepthStream;
                kinect.DepthStream.Enable();
                
                this._DepthImageBitmap = new WriteableBitmap(depthSream.FrameWidth, depthSream.FrameHeight, 96, 96, PixelFormats.Gray16, null);
                this._DepthImageBitmapRect = new Int32Rect(0, 0, depthSream.FrameWidth, depthSream.FrameHeight);
                this._DepthImageStride = depthSream.FrameWidth * depthSream.FrameBytesPerPixel;
                ImagemProfundidade.Source = this._DepthImageBitmap;
                
                // Chama o event handler
                this._Kinect.DepthFrameReady += Kinect_DepthFrameReady;
                //kinect.DepthFrameReady += Kinect_DepthFrameReady;
                kinect.Start();
            }
        }

        private void Kinect_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (DepthImageFrame frame = e.OpenDepthImageFrame())
            {
                if (frame != null)
                {
                    short[] pixelData = new short[frame.PixelDataLength];
                    frame.CopyPixelDataTo(pixelData);
                    this._DepthImageBitmap.WritePixels(this._DepthImageBitmapRect, pixelData, this._DepthImageStride, 0);
                }
            }
        }
    }
}
