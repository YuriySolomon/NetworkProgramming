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
using System.Windows.Shapes;

namespace NetworkProgramming.View
{
    /// <summary>
    /// Логика взаимодействия для ClientServerWindow.xaml
    /// </summary>
    public partial class ClientServerWindow : Window
    {
        public ClientServerWindow()
        {
            InitializeComponent();
        }

        private void StartServerButton_Click(object sender, RoutedEventArgs e)
        {
            var serverWindow = new View.ServerWindow();
            serverWindow.Tag = new Models.NetworkConfig
            {
                Ip = IpTextBlock.Text,
                Port = Convert.ToInt32(PortTextBlock.Text),
                Encoding = Encoding.UTF8    // TODO: реализовать анализ
            };
            serverWindow.Show();
        }

        private void StartClientButton_Click(object sender, RoutedEventArgs e)
        {
            var clientWindow = new View.ClientWindow();
            clientWindow.Tag = new Models.NetworkConfig
            {
                Ip = IpTextBlock.Text,
                Port = Convert.ToInt32(PortTextBlock.Text),
                Encoding = Encoding.UTF8    // TODO: реализовать анализ
            };
            clientWindow.Show();
        }
    }
}
