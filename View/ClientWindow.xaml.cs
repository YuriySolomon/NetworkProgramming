using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace NetworkProgramming.View
{
    /// <summary>
    /// Логика взаимодействия для ClientWindow.xaml
    /// </summary>
    public partial class ClientWindow : Window
    {
        private Models.NetworkConfig? networkConfig;     // из стартового окна
        
        public ClientWindow()
        {
            InitializeComponent();
        }      

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {            
            if (this.Tag is Models.NetworkConfig config)
            {
                networkConfig = config;                    // сохраняем полученную конфигурацию               
            }
            else
            {
                MessageBox.Show("Configuration error");
                Close();
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            // В отличие от сервера клиент не держит постоянное подключение,
            // а подключается по мере необходимости передачи данных
            try
            {
                Socket clientSocket = new Socket(          // конфигурация
                    AddressFamily.InterNetwork,            // сокета-клиента
                    SocketType.Stream,                     // такая же, как у
                    ProtocolType.Tcp);                     // сервера

                clientSocket.Connect(                      // Сервер должен быть запущен
                    networkConfig.EndPoint);               // и мы к нему подключаемся

                // --берем данные из поля ввода, перобразуем в байты и отправляем
                //clientSocket.Send(
                //   networkConfig.Encoding.GetBytes(
                //       ClientMessage.Text
                //       ));

                // формируем команду для сервера
                Models.ClientRequest request = new()
                {
                    Command = "CREATE",
                    Data = ClientMessage.Text
                };
                // переводим объект в текст (JSON) -> в байты ->  отправляем
                clientSocket.Send(
                    networkConfig.Encoding.GetBytes(
                        JsonSerializer.Serialize(request)
                        ));
                // сервер получает данные и отвечает нам, принимаем ответ
                byte[] buffer = new byte[2048];
                String str = "";
                int n;
                do
                {
                    n = clientSocket.Receive(buffer);
                    str += networkConfig.Encoding.GetString(buffer, 0, n);
                } while (clientSocket.Available > 0);

                var temp = JsonSerializer.Deserialize<Models.ServerResponse>(str);

                switch (temp?.Status)
                {
                    case "200":
                        Dispatcher.Invoke(() => { Log.Text += str + "\n"; });
                        break;
                    default:
                        Dispatcher.Invoke(() => { Log.Text += "Errors\n"; });
                        break;
                }

                //// выводим ответ сервера в "лог
                //Dispatcher.Invoke(() => { Log.Text += str + "\n"; });

                // закрываем соединение с сервером
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
            }                                               
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => { Log.Text += ex.Message + "\nОбмін даними зупинено"; });
            }
        }
    }
}
