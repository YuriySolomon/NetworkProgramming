using NetworkProgramming.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
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
    /// Логика взаимодействия для WebApiWindow.xaml
    /// </summary>
    public partial class WebApiWindow : Window
    {
        public ObservableCollection<AssetModel> Assets { get; set; }
        private bool graphNum;   //

        public WebApiWindow()
        {
            InitializeComponent();
            Assets = new ObservableCollection<AssetModel>();
            DataContext = this;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // DrawLine(10, 10, Graph.ActualWidth - 10, Graph.ActualHeight - 10);
            graphNum = false;
            Task.Run(GetAssets);
        }

        private async void GetAssets()
        {
            using var client = new HttpClient { BaseAddress = new Uri("https://api.coincap.io/") };
            String assets = await client.GetStringAsync("/v2/assets/");
            ProcessAssets(assets);
        }
        private void ProcessAssets(String assets)
        {
            var json = JsonSerializer.Deserialize<Models.AssetData>(assets);
            if (json is null) return;
            
            // Получив и разобрав ассеты запускаем историю по первому из них
            Task.Run(() => GetHistory(json.data[0].id));  // старт задачи (в ноговом потоке)
                        
            Dispatcher.Invoke(() => { //делегируем работу с UI главному потоку
                // выводим название валюти, запущенной на историю
                coinTitleOne.Foreground = Brushes.Tomato;
                coinTitleOne.Content = ": " + json.data[0].name;

                // отображаем полученные ассеты путем помещения ссылок на них в 
                // набллбюдаемую коллекцию Assets, связанную с ListView
                json.data.ForEach(Assets.Add);
            });
            /*Dispatcher.Invoke(() =>
            {
                foreach (AssetModel asset in json.data)
                {
                    Assets.Add(asset);
                }
            });*/
        }

        /// <summary>
        /// Получаем историю курсов валюти
        /// </summary>
        private async void GetHistory(String assetId)
        {
            using var client = new HttpClient { BaseAddress = new Uri("https://api.coincap.io/") };
            String history = await client.GetStringAsync($"/v2/assets/{assetId}/history?interval=d1");
            ProcessHistory(history);
        }
        /// <summary>
        /// Обрабатывает полученные с сайта данные, отображает график
        /// </summary>
        /// <param name="history">Загруженные данные с историей курсов</param>
        private void ProcessHistory(String history)
        {
            var json = JsonSerializer.Deserialize<Models.HistoryData>(history);
            // json.data - массив элементов HistoryModels с годовыми курсами валюти
            if (json is null) return;
            /* Работаем над графиком:
             * по Х время (json.data[].time)
             * по Y курс (json.data[].priceUsd)
             * Данные нужно маштабировать, т.к. они явно не соответствуют
             * размерам холста. Для этого определяем максимальное и минимальное
             * значение по каждой координате.
             */
            Int64 minTime, maxTime;
            Double minPrice, maxPrice;
            minTime = maxTime = json.data[0].time;
            minPrice = maxPrice = json.data[0].price;
            foreach (HistoryModels item in json.data)
            {
                if (item.time < minTime) minTime = item.time;
                if (item.time > maxTime) maxTime = item.time;
                if (item.price < minPrice) minPrice = item.price;
                if (item.price > maxPrice) maxPrice = item.price;
            }
            /* Уще один цикл - маштабирует
             * точка на графике X: (time-minTime) - от нуля до (maxTime-minTime)
             *   ноль нас устраивает, но нужно чтобы максимальное значение
             *   было шириной холста (Graph.ActualWidth):
             *   (time-minTime) / (maxTime-minTime) * Graph.ActualWidth
             * по Y аналогичто, только с price и Graph.ActualHeight, но еще и "перевернуть",
             * т.к. ось Y на холсте направлена вниз
             * y = Graph.ActualHeight - y
             * 
             * Для того чтобы проводить линии нужно помнить предыдущую точку и соединять ее с текущей.
             */
            Double x1 = -1, y1 = -1;            

            foreach (HistoryModels item in json.data)
            {                
                Double x2 = (item.time - minTime) * Graph.ActualWidth  / (maxTime - minTime);
                Double y2 = (item.price - minPrice) * Graph.ActualHeight / (maxPrice - minPrice);
                y2 = Graph.ActualHeight - y2;  // инверсия по Y (вверх ногами
                Brush color = Brushes.Tomato;
                if (x1 != -1) // 
                {
                    if (graphNum) color = Brushes.YellowGreen;
                    Dispatcher.Invoke(() => DrawLine(x1, y1, x2, y2, color));
                }

                x1 = x2;
                y1 = y2;
            }
        }

        /// <summary>
        ///  Рисует линию на холсте Graph из точки (x1, y1) в точку (x2, y2)
        /// </summary>  
        private void DrawLine(double x1, double y1, double x2, double y2, Brush color)
        {
            Graph.Children.Add( new Line
            {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                Stroke = color,
                StrokeThickness = 2
            });            
        }

        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListViewItem item)
            {
                if (item.Content is Models.AssetModel assets)
                {
                    if (MoreCripts.IsChecked == true && !graphNum)
                    {
                        coinTitleTwo.Foreground = Brushes.YellowGreen;
                        coinTitleTwo.Content = ": " + assets.name;
                        graphNum = true;
                    }
                    else
                    {
                        Graph.Children.Clear();
                        coinTitleTwo.Content = "";
                        coinTitleOne.Content = ": " + assets.name;  
                        graphNum = false;
                    }
                    Task.Run(() => GetHistory(assets.id));
                }
            }
        }
    }
}
/* Д.З. Реализовать загрузку и отображение истории той валюти, которую
 * в списке выберет пользователь двойным щелчком мыши.
 * Расширить список доступных валют (вывсети сведения о цене priceUsd и кол-ве supply)
 * ** добавит ьфлажок "Стирать график" состояние которого будет учитываться при
 *      выводе нового графика: очищать или нет холст перед построением нового графика
 */
