using System;
using System.Windows;
using Npgsql;
using kurs_musik_club.model;

namespace kurs_musik_club.windows
{
    public partial class ChangeRoleWindow : Window
    {
        private User selectedUser;

        public ChangeRoleWindow(User user)
        {
            InitializeComponent();
            selectedUser = user;

            // Устанавливаем текущую роль пользователя
            if (user.Role.ToLower() == "teacher")
            {
                teacherRole.IsChecked = true;
            }
            else
            {
                clientRole.IsChecked = true;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string newRole = teacherRole.IsChecked == true ? "teacher" : "client";

                using (NpgsqlConnection connection = new NpgsqlConnection(Connection.connectionStr))
                {
                    connection.Open();
                    string query = "UPDATE users SET role = @role WHERE id = @id";
                    
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@role", newRole);
                        command.Parameters.AddWithValue("@id", selectedUser.Id);
                        
                        command.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Роль пользователя успешно изменена");
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при изменении роли: {ex.Message}");
            }
        }
    }
} 