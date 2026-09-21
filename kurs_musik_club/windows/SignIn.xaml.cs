using System;
using System.Windows;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Text;
using Npgsql;
using System.Diagnostics;
using System.Security.Cryptography;

namespace kurs_musik_club.windows
{
    public partial class SignIn : Window
    {
        // VK OAuth credentials
        private const string VK_CLIENT_ID = "YOUR_VK_CLIENT_ID";
        private const string VK_CLIENT_SECRET = "YOUR_VK_CLIENT_SECRET";
        private const string VK_REDIRECT_URI = "https://oauth.vk.com/blank.html";
        
        // Google OAuth credentials
        private const string GOOGLE_CLIENT_ID = "YOUR_GOOGLE_CLIENT_ID";
        private const string GOOGLE_CLIENT_SECRET = "YOUR_GOOGLE_CLIENT_SECRET";
        private const string GOOGLE_REDIRECT_URI = "http://localhost:8080/auth/google/callback";

        public SignIn()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text;
            string password = PasswordBox.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Пожалуйста, введите логин и пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            User user = null;
            try
            {
                using (NpgsqlConnection connect = new NpgsqlConnection(Connection.connectionStr))
                {
                    connect.Open();
                    string sqlQuery = "SELECT * FROM users WHERE login = @login";
                    NpgsqlCommand cmd = new NpgsqlCommand(sqlQuery, connect);
                    cmd.Parameters.AddWithValue("@login", login);
                    NpgsqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        reader.Read();

                        if (password != "")
                        {
                            if (Verification.VerifySHA512Hash(password, (string)reader["password"].ToString()))
                            {
                                user = new User(
                                    Convert.ToInt32(reader[0]),
                                    reader["login"].ToString(),
                                    reader["fio"].ToString(),
                                    reader["role_user"].ToString()
                                );

                                // Сохраняем информацию о пользователе
                                Properties.Settings.Default.UserId = user.Id;
                                Properties.Settings.Default.UserLogin = user.Login;
                                Properties.Settings.Default.UserFio = user.Fio;
                                Properties.Settings.Default.UserRole = user.Role;
                                Properties.Settings.Default.AccessToken = Guid.NewGuid().ToString(); // Генерируем временный токен
                                Properties.Settings.Default.Save();

                                // Открываем главное окно
                                MainWindow main = new MainWindow();
                                main.Show();
                                this.Close();
                            }
                            else
                            {
                                MessageBox.Show("Неверный пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Введите пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Пользователь не найден", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
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
                string authUrl = $"https://oauth.vk.com/authorize?client_id={VK_CLIENT_ID}&display=page&redirect_uri={VK_REDIRECT_URI}&scope=email&response_type=code&v=5.131";
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

        private void GoogleLogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string authUrl = $"https://accounts.google.com/o/oauth2/v2/auth?client_id={GOOGLE_CLIENT_ID}&redirect_uri={GOOGLE_REDIRECT_URI}&response_type=code&scope=email%20profile";
                Process.Start(new ProcessStartInfo
                {
                    FileName = authUrl,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии страницы авторизации Google: {ex.Message}");
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

        private async void HandleGoogleCallback(string code)
        {
            try
            {
                string tokenUrl = "https://oauth2.googleapis.com/token";
                string postData = $"code={code}&client_id={GOOGLE_CLIENT_ID}&client_secret={GOOGLE_CLIENT_SECRET}&redirect_uri={GOOGLE_REDIRECT_URI}&grant_type=authorization_code";
                
                using (WebClient client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    string response = await client.UploadStringTaskAsync(tokenUrl, postData);
                    var tokenData = JsonConvert.DeserializeObject<GoogleTokenResponse>(response);
                    
                    // Get user info
                    string userInfoUrl = $"https://www.googleapis.com/oauth2/v2/userinfo?access_token={tokenData.access_token}";
                    string userInfo = await client.DownloadStringTaskAsync(userInfoUrl);
                    var userData = JsonConvert.DeserializeObject<GoogleUserInfo>(userInfo);
                    
                    // Проверяем, существует ли пользователь в базе
                    using (NpgsqlConnection connection = new NpgsqlConnection(Connection.connectionStr))
                    {
                        connection.Open();
                        string query = "SELECT * FROM users WHERE google_id = @google_id";
                        using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@google_id", userData.id);
                            
                            using (NpgsqlDataReader reader = command.ExecuteReader())
                            {
                                if (!reader.Read())
                                {
                                    // Если пользователь не существует, создаем нового
                                    connection.Close();
                                    connection.Open();
                                    string insertQuery = "INSERT INTO users (login, password, google_id, email) VALUES (@login, @password, @google_id, @email)";
                                    using (NpgsqlCommand insertCommand = new NpgsqlCommand(insertQuery, connection))
                                    {
                                        insertCommand.Parameters.AddWithValue("@login", $"google_{userData.id}");
                                        insertCommand.Parameters.AddWithValue("@password", Guid.NewGuid().ToString());
                                        insertCommand.Parameters.AddWithValue("@google_id", userData.id);
                                        insertCommand.Parameters.AddWithValue("@email", userData.email);
                                        insertCommand.ExecuteNonQuery();
                                    }
                                }
                            }
                        }
                    }

                    MessageBox.Show("Успешная авторизация через Google!");
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

        private void RegisterText_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Registration registration = new Registration();
            registration.Show();
            this.Close();
        }
    }

    public class VKTokenResponse
    {
        public string access_token { get; set; }
        public int user_id { get; set; }
        public string email { get; set; }
    }

    public class GoogleTokenResponse
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
    }

    public class GoogleUserInfo
    {
        public string id { get; set; }
        public string email { get; set; }
        public string name { get; set; }
        public string picture { get; set; }
    }

    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Fio { get; set; }
        public string Role { get; set; }

        public User(int id, string login, string fio, string role)
        {
            Id = id;
            Login = login;
            Fio = fio;
            Role = role;
        }
    }

    public static class Verification
    {
        public static bool VerifySHA512Hash(string input, string hash)
        {
            using (SHA512 sha512 = SHA512.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = sha512.ComputeHash(inputBytes);
                string computedHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

                return computedHash == hash.ToLower();
            }
        }
    }
} 