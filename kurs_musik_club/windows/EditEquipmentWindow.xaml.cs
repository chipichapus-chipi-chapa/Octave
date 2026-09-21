using kurs_musik_club.Classes;
using Npgsql;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace kurs_musik_club.windows
{
    public partial class EditEquipmentWindow : Window
    {
        private readonly int equipmentId;
        private readonly string connectionString;

        public EditEquipmentWindow(int id)
        {
            InitializeComponent();
            equipmentId = id;
            connectionString = Connection.connectionStr;
            LoadComboBoxData();
            LoadEquipmentData();
        }

        private void LoadEquipmentData()
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"
                        SELECT id_musical_equipment, id_type_musical_equipment, id_condition_musical_equipment, id_availability_musical_equipment
                        FROM musical_equipment
                        WHERE id_musical_equipment = @EquipmentId";

                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@EquipmentId", equipmentId);
                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                typeComboBox.SelectedValue = reader["id_type_musical_equipment"];
                                conditionComboBox.SelectedValue = reader["id_condition_musical_equipment"];
                                availabilityComboBox.SelectedValue = reader["id_availability_musical_equipment"];
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных оборудования: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadComboBoxData()
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    // Загрузка типов оборудования
                    string typeQuery = "SELECT id_type_musical_equipment, name_type_musical_equipment FROM type_musical_equipment";
                    using (NpgsqlCommand command = new NpgsqlCommand(typeQuery, connection))
                    {
                        var typeDataTable = new DataTable();
                        using (var reader = command.ExecuteReader())
                        {
                            typeDataTable.Load(reader);
                        }
                        typeComboBox.ItemsSource = typeDataTable.DefaultView;
                        typeComboBox.DisplayMemberPath = "name_type_musical_equipment";
                        typeComboBox.SelectedValuePath = "id_type_musical_equipment";
                    }

                    // Загрузка брендов
                    string brandQuery = "SELECT id_brand_musical_equipment, brand_musical_equipment FROM brand_musical_equipment";
                    using (NpgsqlCommand command = new NpgsqlCommand(brandQuery, connection))
                    {
                        var brandDataTable = new DataTable();
                        using (var reader = command.ExecuteReader())
                        {
                            brandDataTable.Load(reader);
                        }
                        brandComboBox.ItemsSource = brandDataTable.DefaultView;
                        brandComboBox.DisplayMemberPath = "brand_musical_equipment";
                        brandComboBox.SelectedValuePath = "id_brand_musical_equipment";
                    }

                    // Загрузка состояний
                    string conditionQuery = "SELECT id_condition_musical_equipment, condition_musical_equipment FROM condition_musical_equipment";
                    using (NpgsqlCommand command = new NpgsqlCommand(conditionQuery, connection))
                    {
                        var conditionDataTable = new DataTable();
                        using (var reader = command.ExecuteReader())
                        {
                            conditionDataTable.Load(reader);
                        }
                        conditionComboBox.ItemsSource = conditionDataTable.DefaultView;
                        conditionComboBox.DisplayMemberPath = "condition_musical_equipment";
                        conditionComboBox.SelectedValuePath = "id_condition_musical_equipment";
                    }

                    // Загрузка доступности
                    string availabilityQuery = "SELECT id_availability_musical_equipment, availability_musical_equipment FROM availability_musical_equipment";
                    using (NpgsqlCommand command = new NpgsqlCommand(availabilityQuery, connection))
                    {
                        var availabilityDataTable = new DataTable();
                        using (var reader = command.ExecuteReader())
                        {
                            availabilityDataTable.Load(reader);
                        }
                        availabilityComboBox.ItemsSource = availabilityDataTable.DefaultView;
                        availabilityComboBox.DisplayMemberPath = "availability_musical_equipment";
                        availabilityComboBox.SelectedValuePath = "id_availability_musical_equipment";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (typeComboBox.SelectedValue != null)
            {
                try
                {
                    using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                    {
                        connection.Open();
                        string query = @"
                            SELECT b.id_brand_musical_equipment, b.brand_musical_equipment 
                            FROM brand_musical_equipment b
                            INNER JOIN type_musical_equipment t ON b.id_brand_musical_equipment = t.id_brand_musical_equipment
                            WHERE t.id_type_musical_equipment = @TypeId";

                        using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@TypeId", typeComboBox.SelectedValue);
                            var brandDataTable = new DataTable();
                            using (var reader = command.ExecuteReader())
                            {
                                brandDataTable.Load(reader);
                            }
                            brandComboBox.ItemsSource = brandDataTable.DefaultView;
                            brandComboBox.DisplayMemberPath = "brand_musical_equipment";
                            brandComboBox.SelectedValuePath = "id_brand_musical_equipment";
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке брендов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (typeComboBox.SelectedValue == null || brandComboBox.SelectedValue == null || 
                conditionComboBox.SelectedValue == null || availabilityComboBox.SelectedValue == null)
            {
                MessageBox.Show("Пожалуйста, заполните все поля", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"
                        UPDATE musical_equipment 
                        SET id_type_musical_equipment = @TypeId, 
                            id_condition_musical_equipment = @ConditionId,
                            id_availability_musical_equipment = @AvailabilityId
                        WHERE id_musical_equipment = @EquipmentId";

                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@TypeId", typeComboBox.SelectedValue);
                        command.Parameters.AddWithValue("@ConditionId", conditionComboBox.SelectedValue);
                        command.Parameters.AddWithValue("@AvailabilityId", availabilityComboBox.SelectedValue);
                        command.Parameters.AddWithValue("@EquipmentId", equipmentId);

                        command.ExecuteNonQuery();
                    }
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
} 