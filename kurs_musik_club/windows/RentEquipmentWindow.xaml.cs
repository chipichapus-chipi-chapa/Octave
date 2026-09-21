using System;
using System.Collections.Generic;
using System.Windows;
using System.Data;
using System.Data.SqlClient;
using kurs_musik_club.Classes;
using static kurs_musik_club.windows.profil;
using Npgsql;

namespace kurs_musik_club.windows
{
    public class RentedEquipment
    {
        public int id_rent_mus_eq { get; set; }
        public string equipment_name { get; set; }
        public string type_name { get; set; }
        public string condition_name { get; set; }
    }

    public partial class RentEquipmentWindow : Window
    {
        private int currentUserId;
        private string connectionString;

        public RentEquipmentWindow(int userId)
        {
            InitializeComponent();
            currentUserId = userId;
            //  connectionString = "Server=localhost;Port=5432;User id=postgres;Password=123;Database=music_club;";
            LoadEquipment();
        }

        private void LoadEquipment()
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(Connection.connectionStr))
                {
                    connection.Open();
                    var command = new NpgsqlCommand(@"
                        SELECT 
                            me.id_musical_equipment,
                            tme.name_type_musical_equipment,
                            bme.brand_musical_equipment,
                            ame.availability_musical_equipment,
                            COALESCE(c.cost_hour, 0) as cost_hour,
                            COALESCE(rme.id_user, 0) as id_user
                        FROM musical_equipment me
                        JOIN type_musical_equipment tme ON me.id_type_musical_equipment = tme.id_type_musical_equipment
                        JOIN brand_musical_equipment bme ON tme.id_brand_musical_equipment = bme.id_brand_musical_equipment
                        JOIN availability_musical_equipment ame ON me.id_availability_musical_equipment = ame.id_availability_musical_equipment
                        LEFT JOIN rent_mus_eq rme ON me.id_musical_equipment = rme.id_musical_equipment
                        LEFT JOIN cost c ON rme.id_cost = c.id_cost
                        WHERE ame.availability_musical_equipment = true
                        ORDER BY me.id_musical_equipment", connection);

                    var reader = command.ExecuteReader();
                    var equipment = new List<RentedEquipment>();
                    while (reader.Read())
                    {
                        equipment.Add(new RentedEquipment
                        {
                            id_rent_mus_eq = Convert.ToInt32(reader["id_musical_equipment"]),
                            equipment_name = reader["name_type_musical_equipment"].ToString(),
                            type_name = reader["brand_musical_equipment"].ToString(),
                            condition_name = reader["availability_musical_equipment"].ToString()
                        });
                    }
                    equipmentDataGrid.ItemsSource = equipment;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке оборудования: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RentEquipment_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedEquipment = equipmentDataGrid.SelectedItem as RentedEquipment;
                if (selectedEquipment == null)
                {
                    MessageBox.Show("Пожалуйста, выберите оборудование для аренды", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                using (NpgsqlConnection connection = new NpgsqlConnection(Connection.connectionStr))
                {
                    connection.Open();

                    // Начинаем транзакцию
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Создаем запись об аренде
                            var rentCommand = new NpgsqlCommand(@"
                                INSERT INTO rent_mus_eq (id_musical_equipment, id_user)
                                VALUES (@id_musical_equipment, @id_user)", connection, transaction);

                            rentCommand.Parameters.AddWithValue("@id_musical_equipment", selectedEquipment.id_rent_mus_eq);
                            rentCommand.Parameters.AddWithValue("@id_user", currentUserId);
                            rentCommand.ExecuteNonQuery();

                            // Обновляем статус доступности оборудования
                            var updateCommand = new NpgsqlCommand(@"
                                UPDATE musical_equipment 
                                SET id_availability_musical_equipment = 2 
                                WHERE id_musical_equipment = @id_musical_equipment", connection, transaction);

                            updateCommand.Parameters.AddWithValue("@id_musical_equipment", selectedEquipment.id_rent_mus_eq);
                            updateCommand.ExecuteNonQuery();

                            // Подтверждаем транзакцию
                            transaction.Commit();

                            MessageBox.Show("Оборудование успешно арендовано!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadEquipment(); // Обновляем список оборудования
                        }
                        catch (Exception ex)
                        {
                            // Откатываем транзакцию в случае ошибки
                            transaction.Rollback();
                            throw new Exception($"Ошибка при аренде оборудования: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при аренде оборудования: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}