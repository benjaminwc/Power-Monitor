using BatteryInfo;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
        Line[] lines;
        int gindex=0;
        public MainWindow()
        {
            InitializeComponent();
            battery = new Battery();
            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += UpdateTask;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 2);
            dispatcherTimer.Start();
            CreateLines();
        }

        public void UpdateTask(object sender, EventArgs e)
        {
            battery.Update();
            GraphUpdate();
            UIUpdate();
        }
        public static Task Delay(double milliseconds)
        {
            var tcs = new TaskCompletionSource<bool>();
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Elapsed += (obj, args) =>
            {
                tcs.TrySetResult(true);
            };
            timer.Interval = milliseconds;
            timer.AutoReset = false;
            timer.Start();
            return tcs.Task;
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
                Icon.Source = ImageSourceForBitmap(Power_Monitor.Properties.Resources.UIcon);
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
                RealCap.Content= (Math.Round(battery.RealCapacity * 10) / 10f) + $" Wh ({Math.Round((battery.RealCapacity/battery.NominalCapacity)*100)}%)";
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
            gindex++;
            if (gindex>=20)
            {
                gindex = 0;
                Array.Copy(ChargeRate, 1, ChargeRate, 0, ChargeRate.Length - 1);
                Array.Copy(CurrentCharge, 1, CurrentCharge, 0, ChargeRate.Length - 1);
                ChargeRate[99] = battery.ChargeRate;
                CurrentCharge[99] = battery.ChargePercent;
                DrawGraph();
            }
        }
        void CreateLines()
        {
            lines = new Line[99];
            for (int i = 1; i < 100; i++)
            {
                double w1 = GraphRect.ActualWidth / (i - 1);
                if (i - 1 == 0)
                {
                    w1 = 0;
                }
                double h1 = CurrentCharge[i - 1];
                double w2 = GraphRect.ActualWidth / i;
                double h2 = CurrentCharge[i];
                Line l = new Line();
                l.X1 = w1;
                l.X2 = w2;
                l.Y1 = h1;
                l.Y2 = h2;
                l.StrokeThickness = 3;
                l.Stroke = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, (byte)(127/i), 0));
                lines[i-1] = l;
                MainGrid.Children.Add(l);
            }
        }
        public void DrawGraph()
        {
            float mi = Math.Abs(CurrentCharge.Min());
            float mx = Math.Abs(CurrentCharge.Max());
            if (GraphTypeCombo.SelectedIndex==1)
            {
                mi = Math.Abs(ChargeRate.Min());
                mx = Math.Abs(ChargeRate.Max());
            }
            
            float h = 1;
            if (mi>mx)
            {
                h = mi+1;
            }
            else
            {
                h = mx + 1;
            }
            
            for (int i = 1; i < 100; i++)
            {
                double xd = GraphRect.ActualWidth;
                double w1 = 25+ (i-1) * xd / 100f;
                double w2 = 25+ i * xd / 100f;
                double h1 = MainGrid.ActualHeight - 60 - Math.Abs(CurrentCharge[i - 1]) / h * GraphRect.ActualHeight;
                double h2 = MainGrid.ActualHeight - 60 - Math.Abs(CurrentCharge[i]) / h * GraphRect.ActualHeight;
                if (GraphTypeCombo.SelectedIndex == 1)
                {
                    h1 = MainGrid.ActualHeight - 60 - Math.Abs(ChargeRate[i - 1]) / h * GraphRect.ActualHeight;
                    h2 = MainGrid.ActualHeight - 60 - Math.Abs(ChargeRate[i]) / h * GraphRect.ActualHeight;
                }
                
                
                lines[i - 1].X1 = w1;
                lines[i - 1].X2 = w2;
                lines[i - 1].Y1 = h1;
                lines[i - 1].Y2 = h2;

                
                if (ChargeRate[i-1] > 0)
                {
                    lines[i - 1].Stroke = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, (byte)(127* i/100f), 255));
                }
                else
                {
                    lines[i - 1].Stroke = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, (byte)(127*i/100f), 0));
                }
                if (ChargeRate[i-1] == 0)
                {
                    lines[i - 1].Stroke = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
                }
                if (ChargeRate[i] == 0)
                {
                    lines[i - 1].Stroke = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
                }
            }
        }

        private void WindowSize(object sender, SizeChangedEventArgs e)
        {
            DrawGraph();
        }

        private void GraphTypeCombo_SelectionChanged(object sender, EventArgs e)
        {
            DrawGraph();
        }

        private void SettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            Settings s = new Settings();
            s.Show();
            
        }
    }
}
