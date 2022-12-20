using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
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

namespace NetworkProgramming.View
{
    /// <summary>
    /// Логика взаимодействия для SmtpWindow.xaml
    /// </summary>
    public partial class SmtpWindow : Window
    {
        dynamic? email; // динамические объекты могут менять свой тип в процессе выполнения
                        // программ
        SqlConnection connection;
        Random rnd = new();
        public SmtpWindow()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Загружаем конфигурацию SMTP
            email =                                         // динамический десериализатор 
            JsonSerializer.Deserialize<dynamic>(            // возвращает тип JsonElement
                File.ReadAllText("emailconfig.json"));      // в котором значения извлекаются
            // цепочками вида
            //email.GetProperty("smtp").GetProperty("host").GetString()
            // email.GetProperty("smtp").GetProperty("port").GetInt32()
            if (email is null)
            {
                MessageBox.Show("Email configuration load error");
                this.Close();
            }

            // Загружаем конфигурацию БД и подключаемся к ней
            var db = JsonSerializer.Deserialize<dynamic>(
                File.ReadAllText("db.json"));
            if (db is null)
            {
                MessageBox.Show("DB configuration load error");
                this.Close();
            }            
            try
            {
                //connection = new SqlConnection(
                //    db?.GetProperty("Emaildatabase").GetString());
                String con = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=E:\\Studing\\C#\\Programms\\! WPF\\NetworkProgramming\\EmailDatabase.mdf;Integrated Security=True";
                connection = new SqlConnection(con);
                connection.Open();
            }
            catch(Exception ex)
            {

                MessageBox.Show("DB connection error " + ex.Message);
                this.Close();
                return;
            }
        }

        private void SendEmail_Click(object sender, RoutedEventArgs e)
        {
            if (email is null) return;
            JsonElement smtp = email.GetProperty("smtp");
            String host = smtp.GetProperty("host").GetString()!;
            int port = smtp.GetProperty("port").GetInt32();
            String mailbox = smtp.GetProperty("email").GetString()!;
            String password = smtp.GetProperty("password").GetString()!;
            bool ssl = smtp.GetProperty("ssl").GetBoolean();

            using var smtpClient = new SmtpClient(host)
            {
                Port = port,
                EnableSsl = ssl,
                Credentials = new NetworkCredential(mailbox, password)
            };

            #region DZ
            // Д.З. перед отправкой кода подтверждения убедиться что почта новая
            // и ранее на нее код не отправлялся: выводить соотв. сообщение

            String emailTo = mailTo.Text;
            // проверяем если в БД такой email
            using var SqlCommand1 = new SqlCommand(
                $"SELECT code FROM email_codes WHERE email = N'{emailTo}' ",
                connection);
            String? codeTemp;
            try
            {
                codeTemp = Convert.ToString(SqlCommand1.ExecuteScalar());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            if (codeTemp != "") // почта присутсвует в БД
            {
                MessageBox.Show("Введена пошта вже зареєстрована");
                return;
            }

            #endregion

            // генерируем код подтверждения
            int code = rnd.Next(100000, 1000000);
            smtpClient.Send(mailbox,
                mailTo.Text,
                mailSubj.Text,
                mailBody.Text + " " + code); // добавляем код к тексту письма
            // помещаем код в БД
            using var SqlCommand = new SqlCommand(
                $"INSERT INTO email_codes(email, code) VALUES ( N'{mailTo.Text}', '{code}' )",
                connection);
            SqlCommand.ExecuteNonQuery();
            MessageBox.Show("Код підтвердження надіслано на вказану пошту");
        }

        private void ConfirmCode_Click(object sender, RoutedEventArgs e)
        {
            if (connection is null) return;
            String email = confirmEmail.Text;
            // извлекаем код из БД для этой почты
            using var SqlCommand = new SqlCommand(
                $"SELECT code FROM email_codes WHERE email = N'{email}' ",
                connection);
            String? code;
            try
            {
                code = Convert.ToString(SqlCommand.ExecuteScalar());
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            if (code == "") // нет почты в БД - возврат NULL и в строке ""
            {
                MessageBox.Show("Введена пошта не зареєстрована");
                return;
            }
            if (code == "000000") // почта уже подтверждена
            {
                MessageBox.Show("Введена пошта вже підтверджена");
                return;
            }
            if (code == confirmCode.Text) // код введен правильно
            {
                // выводим сообшение об успешном подтверждении
                MessageBox.Show("Введена пошта успішно підтверджена");
                // "сбрасываем" код в БД - устанавливаем его в "000000"
                using var SqlCommand2 = new SqlCommand(
                $"UPDATE email_codes SET code = '000000' WHERE email = N'{email}' ",
                connection);
                SqlCommand2.ExecuteNonQuery();
            }
            else // код введен неправильно
            {
                MessageBox.Show("Код введено неправильно");
                confirmCode.Text = String.Empty;
                return;
            }
        }
    }
}
