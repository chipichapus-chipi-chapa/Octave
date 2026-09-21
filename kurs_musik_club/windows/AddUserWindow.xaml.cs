using System;
using System.Windows;
using Npgsql;
using System.Security.Cryptography;
using System.Text;
using kurs_musik_club.Classes;

namespace kurs_musik_club.windows
{
    public partial class AddUserWindow : Window
    {
        private User existingUser;
        private bool isEditMode;

        public AddUserWindow(User user = null)
        {
            InitializeComponent();
            existingUser = user;
            isEditMode = user != null;

            if (isEditMode)
            {
                windowTitle.Text = "Редактирование пользователя";
                txtFio.Text = user.Fio;
                txtLogin.Text = user.Login;
                cmbRole.SelectedValue = user.Role;
            }
            else
            {
                cmbRole.SelectedIndex = 0;
            }
        }

        private string GetSHA512Hash(string input)
        {
            using (SHA512 sha512 = SHA512.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(input);
                byte[] hash = sha512.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFio.Text) || 
                string.IsNullOrWhiteSpace(txtLogin.Text) || 
                (!isEditMode && string.IsNullOrWhiteSpace(txtPassword.Password)))
            {
                MessageBox.Show("Пожалуйста, заполните все обязательные поля", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(Connection.connectionStr))
                {
                    connection.Open();
                    string query;
                    NpgsqlCommand command;

                    if (isEditMode)
                    {
                        query = @"UPDATE users 
                                SET fio = @fio, login = @login, role_user = @role 
                                WHERE id = @id";
                        command = new NpgsqlCommand(query, connection);
                        command.Parameters.AddWithValue("@id", existingUser.Id);
                    }
                    else
                    {
                        query = @"INSERT INTO users (fio, login, password, role_user) 
                                VALUES (@fio, @login, @password, @role)";
                        command = new NpgsqlCommand(query, connection);
                        command.Parameters.AddWithValue("@password", GetSHA512Hash(txtPassword.Password));
                    }

                    command.Parameters.AddWithValue("@fio", txtFio.Text);
                    command.Parameters.AddWithValue("@login", txtLogin.Text);
                    command.Parameters.AddWithValue("@role", ((ComboBoxItem)cmbRole.SelectedItem).Tag.ToString());

                    command.ExecuteNonQuery();
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении пользователя: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
} 