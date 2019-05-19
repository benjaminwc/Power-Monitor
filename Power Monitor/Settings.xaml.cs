using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Power_Monitor
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void Link_click(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/sowa705/Power-Monitor");
        }

        private void Lic_click(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("https://raw.githubusercontent.com/sowa705/Power-Monitor/master/LICENSE");
        }
    }
}
