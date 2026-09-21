using System;
using System.Windows;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Text;

namespace kurs_musik_club.windows
{
    public partial class AuthWindow : Window
    {
        // VK OAuth credentials
        private const string VK_CLIENT_ID = "YOUR_VK_CLIENT_ID";
        private const string VK_CLIENT_SECRET = "YOUR_VK_CLIENT_SECRET";
        private const string VK_REDIRECT_URI = "https://oauth.vk.com/blank.html";
        
        // Google OAuth credentials
        private const string GOOGLE_CLIENT_ID = "YOUR_GOOGLE_CLIENT_ID";
        private const string GOOGLE_CLIENT_SECRET = "YOUR_GOOGLE_CLIENT_SECRET";
        private const string GOOGLE_REDIRECT_URI = "http://localhost:8080/auth/google/callback";

        public AuthWindow()
        {
            InitializeComponent();
        }

        private void VKLogin_Click(object sender, RoutedEventArgs e)
        {
            string authUrl = $"https://oauth.vk.com/authorize?client_id={VK_CLIENT_ID}&display=page&redirect_uri={VK_REDIRECT_URI}&scope=email&response_type=code&v=5.131";
            System.Diagnostics.Process.Start(authUrl);
        }

        private void GoogleLogin_Click(object sender, RoutedEventArgs e)
        {
            string authUrl = $"https://accounts.google.com/o/oauth2/v2/auth?client_id={GOOGLE_CLIENT_ID}&redirect_uri={GOOGLE_REDIRECT_URI}&response_type=code&scope=email%20profile";
            System.Diagnostics.Process.Start(authUrl);
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
                    
                    // Save token and user info
                    Properties.Settings.Default.AccessToken = tokenData.access_token;
                    Properties.Settings.Default.UserId = tokenData.user_id;
                    Properties.Settings.Default.Save();

                    MessageBox.Show("Успешная авторизация через ВКонтакте!");
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
                    
                    // Save token and user info
                    Properties.Settings.Default.AccessToken = tokenData.access_token;
                    Properties.Settings.Default.UserEmail = userData.email;
                    Properties.Settings.Default.Save();

                    MessageBox.Show("Успешная авторизация через Google!");
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
} 