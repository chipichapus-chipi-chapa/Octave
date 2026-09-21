using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Oauth2.v2.Data;
using Google.Apis.Oauth2.v2;
using Google.Apis.Services;
using kurs_musik_club.Classes;
using kurs_musik_club.model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
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
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Util.Store;
using System.Diagnostics;
using static Google.Apis.Oauth2.v2.Oauth2Service;
using System.IO;

namespace kurs_musik_club.windows
{
    /// <summary>
    /// Логика взаимодействия для registration.xaml
    /// </summary>
    public partial class registration : Window //решить вопрос с ролями,чтобы они записывались  в бд
    {
        public registration()
        {
            InitializeComponent();
        }

        private void txt_sign_in_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            sing_in sing_In = new sing_in();
            sing_In.Show();
            this.Close();
        }

        private void btn_reg_Click(object sender, RoutedEventArgs e)
        {
            if (UserFromDb.UserChek(txtEmail.Text) && UserFromDb.CheckPassword(txtPassword.Text, txtConfirmPassword.Text))
            {
                UserFromDb.UserAdd(txtEmail.Text, txtPassword.Text, txtName.Text);
                this.Close();
                sing_in sing_In = new sing_in();
                sing_In.Show();

            }
        }

        private void cmbInterest_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private async void btn_google_Click(object sender, RoutedEventArgs e)
        {
           // AuthorizeAsync();

        }

        /*private const string ClientId = "ВАШ_CLIENT_ID.apps.googleusercontent.com";
        private const string ClientSecret = "ВАШ_CLIENT_SECRET";
        private static readonly string[] Scopes = { "https://www.googleapis.com/auth/userinfo.email", "https://www.googleapis.com/auth/userinfo.profile" };
        private async void AuthorizeAsync()
        {
            UserCredential credential;

            using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                // Загрузка клиентских секретов из файла
                var credPath = "token.json";
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true));
            }

            MessageBoxResult messageBoxResult = MessageBox.Show($"Access Token: {credential.Token.AccessToken}");
        }*/
    }
}



