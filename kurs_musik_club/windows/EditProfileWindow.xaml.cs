using System;
using System.Windows;
using Npgsql;
using kurs_musik_club.model;

namespace kurs_musik_club.windows
{
    public partial class EditProfileWindow : Window
    {
        private User currentUser;

        public EditProfileWindow(User user)
        {
            InitializeComponent();
            currentUser = user;

            // Заполняем поля текущими данными пользователя
            userName.Text = user.Name;
            userEmail.Text = user.Email;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(userName.Text))
            {
                MessageBox.Show("Пожалуйста, введите имя");
                return;
            }

            if (!string.IsNullOrEmpty(newPassword.Password))
            {
                if (newPassword.Password != confirmPassword.Password)
                {
                    MessageBox.Show("Пароли не совпадают");
                    return;
                }

                if (newPassword.Password.Length < 6)
                {
                    MessageBox.Show("Пароль должен содержать не менее 6 символов");
                    return;
                }
            }

            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(Connection.connectionStr))
                {
                    connection.Open();
                    string query;
                    NpgsqlCommand command;

                    if (!string.IsNullOrEmpty(newPassword.Password))
                    {
                        query = "UPDATE users SET name = @name, email = @email, password = @password WHERE id = @id";
                        command = new NpgsqlCommand(query, connection);
                        command.Parameters.AddWithValue("@password", newPassword.Password);
                    }
                    else
                    {
                        query = "UPDATE users SET name = @name, email = @email WHERE id = @id";
                        command = new NpgsqlCommand(query, connection);
                    }

                    command.Parameters.AddWithValue("@name", userName.Text);
                    command.Parameters.AddWithValue("@email", userEmail.Text);
                    command.Parameters.AddWithValue("@id", currentUser.Id);
                    
                    command.ExecuteNonQuery();
                }

                // Обновляем данные текущего пользователя
                currentUser.Name = userName.Text;
                currentUser.Email = userEmail.Text;

                MessageBox.Show("Профиль успешно обновлен");
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении профиля: {ex.Message}");
            }
        }
    }
} 