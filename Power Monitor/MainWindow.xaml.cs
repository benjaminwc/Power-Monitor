using BatteryInfo;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace Power_Monitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Battery battery;
        public float[] ChargeRate = new float[100];
        public float[] CurrentCharge = new float[100];
        public MainWindow()
        {
            InitializeComponent();
            battery = new Battery();
            UpdateTask();
        }

        public async Task UpdateTask()
        {
            while (true)
            {
                battery.Update();
                GraphUpdate();
                UIUpdate();
                await Task.Delay(2000);
            }
        }
        public void UIUpdate()
        {
            if (battery.Status==BatteryStatus.Unavailable)
            {
                ChargeText.Content = "?";
                ChgRate.Content= "?";
                NomCap.Content = "?";
                RealCap.Content = "?";
                TimeRemain.Content = "?";
                Voltage.Content = "?";
                BatteryState.Content = "Battery unavailable";
                ((GradientBrush)BatteryGrad.Fill).GradientStops[0].Color = System.Windows.Media.Color.FromRgb(255, 128, 0);
                ((GradientBrush)BatteryGrad.Fill).GradientStops[1].Color = System.Windows.Media.Color.FromRgb(255, 128, 0);
            }
            else
            {
                ((GradientBrush)BatteryGrad.Fill).GradientStops[0].Offset = (1-battery.ChargePercent/100f);
                ((GradientBrush)BatteryGrad.Fill).GradientStops[1].Offset = (1-battery.ChargePercent / 100f )+ 0.001f;
                ChargeText.Content = Math.Round(battery.ChargePercent) + "%";
                if (battery.Status==BatteryStatus.Charging)
                {
                    ChargeRateLabel.Content = "Charge Rate:";
                    ChgRate.Content = (Math.Round(battery.ChargeRate * 10) / 10f) + " W";
                    BatteryState.Content = "Charging";
                    TimeRemain.Content = "?";
                    Icon.Source =ImageSourceForBitmap( Power_Monitor.Properties.Resources.CIcon);
                    ((GradientBrush)BatteryGrad.Fill).GradientStops[0].Color = System.Windows.Media.Color.FromRgb(200, 200, 200);
                    ((GradientBrush)BatteryGrad.Fill).GradientStops[1].Color = System.Windows.Media.Color.FromRgb(0, 128, 255);

                }
                else
                {
                    ChargeRateLabel.Content = "Discharge Rate:";
                    ChgRate.Content = (Math.Round(-battery.ChargeRate*10)/10f)+" W";
                    BatteryState.Content = "Discharging";
                    TimeRemain.Content = battery.TimeToDischarge.ToString(@"hh\:mm");
                    Icon.Source = ImageSourceForBitmap(Power_Monitor.Properties.Resources.BIcon);
                    ((GradientBrush)BatteryGrad.Fill).GradientStops[0].Color = System.Windows.Media.Color.FromRgb(200, 200, 200);
                    ((GradientBrush)BatteryGrad.Fill).GradientStops[1].Color = System.Windows.Media.Color.FromRgb(32, 255, 32);
                    if (battery.ChargePercent<30)
                    {
                        ((GradientBrush)BatteryGrad.Fill).GradientStops[1].Color = System.Windows.Media.Color.FromRgb(255, 200, 32);
                    }
                    if (battery.ChargePercent<15)
                    {
                        ((GradientBrush)BatteryGrad.Fill).GradientStops[1].Color = System.Windows.Media.Color.FromRgb(255, 32, 32);
                    }
                    
                }
                RealCap.Content= (Math.Round(battery.RealCapacity * 10) / 10f) + " Wh";
                NomCap.Content = (Math.Round(battery.NominalCapacity * 10) / 10f) + " Wh";
                Voltage.Content = (Math.Round(battery.CurrentVoltage * 100) / 100f) + " V";
                


                
            }
            
        }

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        public ImageSource ImageSourceForBitmap(Bitmap bmp)
        {
            var handle = bmp.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally { DeleteObject(handle); }
        }

        public void GraphUpdate()
        {
            for (int i = 0; i < 99; i++)
            {
                ChargeRate[i + 1] = ChargeRate[i];
            }
            ChargeRate[0] = battery.ChargeRate;
        }
        public void DrawGraph()
        {
            
        }
    }
}
