using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
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
using System.Xml;

namespace NetworkProgramming.View
{
    /// <summary>
    /// Логика взаимодействия для HttpWindow.xaml
    /// </summary>
    public partial class HttpWindow : Window
    {
        public HttpWindow()
        {
            InitializeComponent();
        }

        private void HtmlRequestButton_Click(object sender, RoutedEventArgs e)
        {          
            HttpClient httpClient = new()
            {                                                           // Создаем клиент для отправки запросов на
                BaseAddress = new Uri(textBoxUrl.Text)                  // сайт из textBoxUr (https://itstep.org)
            };                                                          // 
            /*
            String response = await httpClient.GetStringAsync("/");
            textBlockResponse.Text = response;            
            */
            httpClient.GetStringAsync("/")                              // отправляем запрос на домашеюю страницу (/)
                .ContinueWith(t =>                                      // Добавляем "нить" - задачу. запускаемую после получения результата
                Dispatcher.Invoke(                                      // Поскольку выполнение в отдельном потоке - 
                () =>                                                   // вызываем Dispatcher с задачей вывода полученного
                textBlockResponse.Text = t.Result));                    // текста в textBlockResponse
        }

        private void XMLRequestButton_Click(object sender, RoutedEventArgs e)
        {
            HttpClient httpClient = new();                               // Вариант: при создании клиента
            httpClient.GetStringAsync(textBoxXmlUrl.Text)                // мы не указали базовый адрес,
                .ContinueWith(t => Dispatcher.Invoke(() =>               // поэтому в Get-метод передаем
                {                                                        // полный URL, а не толькот "/"
                    textBlockResponse.Text = t.Result;                   // Также дополнительно 
                    httpClient.Dispose();                                // освобождаем ресурс httpClient
                    ParseXmlRates();                                     // 
                }));                                                      
        }                                                                 

        private void ParseXmlRates()
        {
            XmlDocument rates = new();                           // XmlDocument - основа для работы с XML
            rates.LoadXml(                                       // загружаем текст в документ для 
                textBlockResponse.Text);                         // дальнейшей обработки
            XmlNodeList? currencies =                            // Селектор - запрос на выборку узлов,
                rates                                            // соответствующих определенным критериям.
                ?.DocumentElement                                // Сам контент (без строки заголовка <?xml ...)
                ?.SelectNodes("currency");                       // Отбор по имени тегов (<currency>)
                                                                 // 
            if (currencies is null) return;                      // Проверка на успешный разбор документа
                                                                 // 
            treeView1.Items.Clear();                                                                 
            foreach (XmlNode node                                // Итерирование колекции - элементы XmlNode
                in currencies)
            {                                                    // node.InnerText - текст узла (без тегов) если у узла есть 
                TreeViewItem item = new()                        // внутренние узлы, то все их InnerText соединяются в строку
                {                                                // node.ChildNodes - коллекция внутренних узлов
                    Header = node                                // порядок их следования - как в исходном док-те
                    .ChildNodes[1]                               // [0] - r030      [3] - cc
                    ?.InnerText                                  // [1] - txt       [4] - exchangedate
                };                                               // [2] - rate
                item.Items.Add(new TreeViewItem { Header = "r030: " + node.ChildNodes[0]?.InnerText });
                item.Items.Add(new TreeViewItem { Header = "cc: "   + node.ChildNodes[3]?.InnerText });
                item.Items.Add(new TreeViewItem { Header = "rate: " + node.ChildNodes[2]?.InnerText });
                item.Items.Add(new TreeViewItem { 
                    Header =
                    String.Format("1 {0} = {1} HRN",
                    node.ChildNodes[3]?.InnerText,
                    node.ChildNodes[2]?.InnerText)});

                item.Items.Add(new TreeViewItem
                {
                    Header =
                   String.Format("1 HRN = {1:F2} {0}",                   // F2 - Float with 2 digits after '.'
                   node.ChildNodes[3]?.InnerText,                        // 
                    1 / Convert.ToSingle(                                // В "наших" ОС десятичная точка
                        node.ChildNodes[2]?.InnerText,                   // считается запятой. Для "сброса" этого
                        CultureInfo.InvariantCulture.NumberFormat))      // правила выбираем InvariantCulture
                });

                item.Items.Add(new TreeViewItem { Header = "date: " + node.ChildNodes[4]?.InnerText });

                treeView1.Items.Add(item);
            }                                                     
        }

        private async void JsonRequestButton_Click(object sender, RoutedEventArgs e)
        {
            using HttpClient httpClient = new()                          // Здесь демонстрируем разделение
            {                                                            // базового адреса: https://bank.gov.ua
                BaseAddress = new Uri("https://bank.gov.ua")             // и запроса /NBUStatService/v1/statdirectory/exchange?json
            };                                                           // Использование await требует
            textBlockResponse.Text = await                               // указать async в сигнатуре методжа
                httpClient.GetStringAsync(textBoxJsonUrl.Text);          // но разрешает не использовать Dispatcher
                                                                         // а также упрощает блок using вместо Dispatcher
            ParseJsonRates();
        }                                                                

        // переводим JSON из текста textBlockResponse в объекты treeView
        private void ParseJsonRates()
        {
            var ratesList =
            JsonSerializer.Deserialize<            // Models.NbuJsonRate - один объект
                List<Models.NbuJsonRate>           // ответ - коллекция объектов
                >(textBlockResponse.Text);         // textBlockResponse.Text - JSON (текст)
            if (ratesList is null) return;

            treeView1.Items.Clear();
            foreach (Models.NbuJsonRate rate in ratesList)
            {
                if (rate.txt == "Долар США" || rate.txt == "Данська крона" || rate.txt == "Єна")
                {
                    // создаем узел с сокращенным названием валюты
                    TreeViewItem item = new TreeViewItem()
                    {
                        Header = rate.cc
                    };
                    // заполняем узел под-узлами сл всеми данными
                    item.Items.Add(new TreeViewItem { Header = rate.txt });
                    item.Items.Add(new TreeViewItem { Header = "rate: " + rate.rate });
                    item.Items.Add(new TreeViewItem { Header = "r030: " + rate.r030 });
                    item.Items.Add(new TreeViewItem { Header = rate.exchangedate });

                    // добавляем удел к "дереву"
                    treeView1.Items.Add(item);
                }
            }
            // Задание: вывести не все курсы, а только Доллар США, Евро, Ена
        }
    }
}
/* Добавить в окно элемент для ввода/выбора даты
 * Реализовать отображение курсов валют за выбранную дату
 * 
 * Обратить внимание: дата собирается в строку без разделителей
 * 20200302 - 2020-03-02 (02.03.2020)
 * Детальней на 
 */
