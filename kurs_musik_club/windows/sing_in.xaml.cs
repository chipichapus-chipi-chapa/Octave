using kurs_musik_club.Classes;
using kurs_musik_club.model;
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
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Diagnostics;
using Npgsql;
using kurs_musik_club.Properties;
using System.Security.Cryptography;
using System.Configuration;

namespace kurs_musik_club.windows
{
    /// <summary>
    /// Логика взаимодействия для sing_in.xaml
    /// </summary>
    public partial class sing_in : Window
    {
        UserFromDb userFromDb = new UserFromDb();
        public static User currentUser { get; set; } = null;

        // VK OAuth credentials
        private const string VK_CLIENT_ID = "53450776"; // Replace with your numeric Application ID
        private const string VK_CLIENT_SECRET = "wkNdwQcEmRj4oZydpTma"; // Replace with your Secret Key
        private const string VK_REDIRECT_URI = "https://oauth.vk.com/blank.html";
        
        // Yandex OAuth credentials
        private const string YANDEX_CLIENT_ID = "YOUR_YANDEX_CLIENT_ID";
        private const string YANDEX_CLIENT_SECRET = "YOUR_YANDEX_CLIENT_SECRET";
        private const string YANDEX_REDIRECT_URI = "http://localhost:8080/auth/yandex/callback";

        public sing_in()
        {
            InitializeComponent();
        }

        private void txt_reg_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            registration registration =  new registration();
            registration.Show();
            this.Close();
        }

        private void btn_sing_in_Click(object sender, RoutedEventArgs e)
        {
            string login = txtEmail.Text;
            string password = txtPassword.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Пожалуйста, введите логин и пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                User user = userFromDb.GetUser(login, password);
                if (user != null)
                {
                    try
                    {
                        // Создаем объект пользователя
                        currentUser = user;

                        // Устанавливаем значения в глобальные настройки (без сохранения)
                        Settings.Default.UserId = user.Id.ToString();
                        Settings.Default.UserLogin = user.Login ?? string.Empty;
                        Settings.Default.UserFio = user.Fio ?? string.Empty;
                        Settings.Default.UserRole = user.Role ?? string.Empty;
                        Settings.Default.AccessToken = Guid.NewGuid().ToString();

                        MessageBox.Show("Успешная авторизация!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        mainWindow mainWindow = new mainWindow();
                        mainWindow.Show();
                    this.Hide();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при авторизации: {ex.Message}", 
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Неверный логин или пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при авторизации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        
        private void VKLogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (VK_CLIENT_ID == "YOUR_VK_APP_ID")
                {
                    MessageBox.Show("VK авторизация не настроена. Пожалуйста, настройте VK приложение и укажите правильный Application ID.");
                    return;
                }

                string authUrl = $"https://oauth.vk.com/authorize?client_id={VK_CLIENT_ID}&display=page&redirect_uri={VK_REDIRECT_URI}&scope=email,offline&response_type=code&v=5.131";
                Process.Start(new ProcessStartInfo
                {
                    FileName = authUrl,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии страницы авторизации ВКонтакте: {ex.Message}");
            }
        }

        private void Yandexbtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (YANDEX_CLIENT_ID == "YOUR_YANDEX_CLIENT_ID")
                {
                    MessageBox.Show("Яндекс авторизация не настроена. Пожалуйста, настройте Яндекс приложение и укажите правильный Client ID.");
                    return;
                }

                string authUrl = $"https://oauth.yandex.ru/authorize?response_type=code&client_id={YANDEX_CLIENT_ID}&redirect_uri={YANDEX_REDIRECT_URI}";
                Process.Start(new ProcessStartInfo
                {
                    FileName = authUrl,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии страницы авторизации Яндекс: {ex.Message}");
            }
        }

        private async void HandleVKCallback(string code)
        {
            try
            {
                string tokenUrl = $"https://oauth.vk.com/access_token?client_id={VK_CLIENT_ID}&client_secret={VK_CLIENT_SECRET}&redirect_uri={VK_REDIRECT_URI}&code={code}";
                
                using (WebClient client = new WebClient())
                {
                    string response = await client.DownloadStringTaskAsync(tokenUrl);
                    var tokenData = JsonConvert.DeserializeObject<VKTokenResponse>(response);
                    
                    // Проверяем, существует ли пользователь в базе
                    using (NpgsqlConnection connection = new NpgsqlConnection(Connection.connectionStr))
                    {
                        connection.Open();
                        string query = "SELECT * FROM users WHERE vk_id = @vk_id";
                        using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@vk_id", tokenData.user_id);
                            
                            using (NpgsqlDataReader reader = command.ExecuteReader())
                            {
                                if (!reader.Read())
                                {
                                    // Если пользователь не существует, создаем нового
                                    connection.Close();
                                    connection.Open();
                                    string insertQuery = "INSERT INTO users (login, password, vk_id, email) VALUES (@login, @password, @vk_id, @email)";
                                    using (NpgsqlCommand insertCommand = new NpgsqlCommand(insertQuery, connection))
                                    {
                                        insertCommand.Parameters.AddWithValue("@login", $"vk_{tokenData.user_id}");
                                        insertCommand.Parameters.AddWithValue("@password", Guid.NewGuid().ToString());
                                        insertCommand.Parameters.AddWithValue("@vk_id", tokenData.user_id);
                                        insertCommand.Parameters.AddWithValue("@email", tokenData.email);
                                        insertCommand.ExecuteNonQuery();
                                    }
                                }
                            }
                        }
                    }

                    MessageBox.Show("Успешная авторизация через ВКонтакте!");
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка авторизации: {ex.Message}");
            }
        }

        private async void HandleYandexCallback(string code)
        {
            try
            {
                string tokenUrl = "https://oauth.yandex.ru/token";
                string postData = $"grant_type=authorization_code&code={code}&client_id={YANDEX_CLIENT_ID}&client_secret={YANDEX_CLIENT_SECRET}";
                
                using (WebClient client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    string response = await client.UploadStringTaskAsync(tokenUrl, postData);
                    var tokenData = JsonConvert.DeserializeObject<YandexTokenResponse>(response);
                    
                    // Get user info
                    string userInfoUrl = $"https://login.yandex.ru/info?format=json&oauth_token={tokenData.access_token}";
                    string userInfo = await client.DownloadStringTaskAsync(userInfoUrl);
                    var userData = JsonConvert.DeserializeObject<YandexUserInfo>(userInfo);
                    
                    // Проверяем, существует ли пользователь в базе
                    using (NpgsqlConnection connection = new NpgsqlConnection(Connection.connectionStr))
                    {
                        connection.Open();
                        string query = "SELECT * FROM users WHERE yandex_id = @yandex_id";
                        using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@yandex_id", userData.id);
                            
                            using (NpgsqlDataReader reader = command.ExecuteReader())
                            {
                                if (!reader.Read())
                                {
                                    // Если пользователь не существует, создаем нового
                                    connection.Close();
                                    connection.Open();
                                    string insertQuery = "INSERT INTO users (login, password, yandex_id, email) VALUES (@login, @password, @yandex_id, @email)";
                                    using (NpgsqlCommand insertCommand = new NpgsqlCommand(insertQuery, connection))
                                    {
                                        insertCommand.Parameters.AddWithValue("@login", $"yandex_{userData.id}");
                                        insertCommand.Parameters.AddWithValue("@password", Guid.NewGuid().ToString());
                                        insertCommand.Parameters.AddWithValue("@yandex_id", userData.id);
                                        insertCommand.Parameters.AddWithValue("@email", userData.default_email);
                                        insertCommand.ExecuteNonQuery();
                                    }
                                }
                            }
                        }
                    }

                    MessageBox.Show("Успешная авторизация через Яндекс!");
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка авторизации: {ex.Message}");
            }
        }
    }

    public class VKTokenResponse
    {
        public string access_token { get; set; }
        public int user_id { get; set; }
        public string email { get; set; }
    }

    public class YandexTokenResponse
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
    }

    public class YandexUserInfo
    {
        public string id { get; set; }
        public string default_email { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string display_name { get; set; }
        public string real_name { get; set; }
    }
}
