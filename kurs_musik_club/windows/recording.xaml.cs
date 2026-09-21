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
using System.Data.SqlClient;
using System.Configuration;
using Npgsql;

namespace kurs_musik_club.windows
{
    /// <summary>
    /// Логика взаимодействия для recording.xaml
    /// </summary>
    public partial class recording : Window
    {
        private string connectionString;

        public recording()
        {
            InitializeComponent();
            NpgsqlConnection connect = new NpgsqlConnection(Connection.connectionStr);
        }
        record_mix_fromDb record_s = new record_mix_fromDb();
        public List<recording_and_mixing> recording_s = new List<recording_and_mixing>();
        public void ViewAllrecord()
        {
  
               var recording_s = record_s.GetRecording_And_Mixings();
                listViewcards.ItemsSource = recording_s;
            

        }
        private void listViewcards_Loaded(object sender, RoutedEventArgs e)
        {
            LoadRecordingServices();
        }

        private void LoadRecordingServices()
        {
            try
            {
                MessageBox.Show("Начало загрузки услуг");
                using (NpgsqlConnection connection = new NpgsqlConnection(Connection.connectionStr))
                {
                    connection.Open();
                    string query = "SELECT id_recording_and_mixing, recording_duration, name_rec, price FROM recording_and_mixing";
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            int count = 0;
                            while (reader.Read())
                            {
                                count++;
                                var service = new
                                {
                                    id = reader["id_recording_and_mixing"].ToString(),
                                    name_rec = reader["name_rec"].ToString(),
                                    recording_duration = reader["recording_duration"].ToString(),
                                    price = reader["price"].ToString()
                                };
                                listViewcards.Items.Add(service);
                            }
                            MessageBox.Show($"Загружено услуг: {count}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке услуг: {ex.Message}");
            }
        }

        private void btnBook_Click(object sender, RoutedEventArgs e)
        {
            if (listViewcards.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите услугу");
                return;
            }

            var bookingWindow = new RecordingBookingWindow(listViewcards.SelectedItem);
            bookingWindow.ShowDialog();
        }
    }
}
