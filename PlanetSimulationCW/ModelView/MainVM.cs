using PlanetSimulationCW.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Controls;

namespace PlanetSimulationCW.ModelView
{
    class MainVM : Property
    {
        int frame = 0;

        private WriteableBitmap wb;

        private ImageSource imgSource;
        public ImageSource ImgSource { get { return imgSource; } set { imgSource = value; OnPropertyChanged(nameof(ImgSource)); } }

        Simulation simulation;

        public MainVM()
        {
            simulation = new Simulation(100);

            //wb = new WriteableBitmap((int)img.Width, (int)img.Height, 96, 96, PixelFormats.Bgr24, null);
            wb = new WriteableBitmap((int)400, (int)300, 96, 96, PixelFormats.Bgr24, null);

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(30);
            timer.Tick += Render;
            timer.Start();
        }

        private void Render(object? sender, EventArgs e)
        {
            frame++;
            int stride = (wb.PixelWidth * wb.Format.BitsPerPixel + 7) / 8;

            byte[] colorData1 = Enumerable.Range(0, stride * wb.PixelHeight).Select(i => (byte)0).ToArray();
            Int32Rect rect1 = new Int32Rect(0, 0, wb.PixelWidth, wb.PixelHeight);
            wb.WritePixels(rect1, colorData1, stride, 0);

            foreach (Planet planet in simulation.planets)
            {
                int red = 255;
                int green = 255;
                int blue = 255;

                byte[] colorData = { (byte)blue, (byte)green, (byte)red }; // B G R

                if (CheckIfPointInBounds(planet.Position.X, planet.Position.Y))
                {
                    Int32Rect rect = new Int32Rect((int)planet.Position.X, (int)planet.Position.Y, 1, 1);
                    wb.WritePixels(rect, colorData, stride, 0);
                }
            }

            // Show the bitmap in an Image element.
            ImgSource = wb;

            simulation.SimulateStep();
        }

        private bool CheckIfPointInBounds(float x, float y)
        {
            if (x < 0 || y < 0 || x >= wb.PixelWidth || y >= wb.PixelHeight)
            {
                return false;
            }
            return true;
        }
    }
}
