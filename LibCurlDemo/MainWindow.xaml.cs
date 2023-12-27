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

namespace LibCurlDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private DownLoad _downLoad = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            DownLoad();
        }

        private async void DownLoad()
        {
            _downLoad ??= new DownLoad();
            var sUrl = "";
            var path = "";
            var ret = await _downLoad.Download(sUrl, path);
        }

        private void ButtonBase1_OnClick(object sender, RoutedEventArgs e)
        {
            if(_downLoad == null)
                return;

            _downLoad.Pause();
        }

        private void ButtonBase2_OnClick(object sender, RoutedEventArgs e)
        {
            if (_downLoad == null)
                return;

            _downLoad.Cancel();
        }
    }
}
