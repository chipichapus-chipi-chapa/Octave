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
using System.Data.SqlClient;
using System.Configuration;
using kurs_musik_club.Classes;
using Npgsql;

namespace kurs_musik_club.windows
{
    /// <summary>
    /// Логика взаимодействия для halls.xaml
    /// </summary>
    public partial class halls : Window
    {
        private string connectionString;

        public halls()
        {
            InitializeComponent();
            NpgsqlConnection connect = new NpgsqlConnection(Connection.connectionStr);
        }

        public string Description { get; internal set; }
        public object Capacity { get; internal set; }

        private void cmbHalls_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbHalls.SelectedItem != null)
            {
                string selectedHall = (cmbHalls.SelectedItem as ComboBoxItem).Content.ToString();
                LoadHallDetails(selectedHall);
            }
        }

        private void LoadHallDetails(string hallName)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT photo_path, description FROM Halls WHERE name = @hallName";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@hallName", hallName);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string photoPath = reader["photo_path"].ToString();
                                string description = reader["description"].ToString();

                                // Update UI
                                hallImage.Source = new BitmapImage(new Uri(photoPath, UriKind.Relative));
                                hallDescription.Text = description;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных зала: {ex.Message}");
            }
        }

        private void btn_reg_Click(object sender, RoutedEventArgs e)
        {
            if (cmbHalls.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите зал");
                return;
            }

            // Open booking window
          //  var bookingWindow = new HallBookingWindow(cmbHalls.SelectedItem.ToString());
           // bookingWindow.ShowDialog();
        }
    }
}
