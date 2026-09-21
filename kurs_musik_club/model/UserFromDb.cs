using kurs_musik_club.Classes;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace kurs_musik_club.model
{
    public class UserFromDb
    {

        public User GetUser(string login, string password) //не получается войти в аккаунт
        {
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
                            }
                            else
                            {
                                MessageBox.Show("NEVERNIY PAROL");
                            }
                        }
                        else
                        {
                            MessageBox.Show("VVEDITE PAROL!");
                        }
                    }
                    else
                    {
                        MessageBox.Show("NET TAKOVO POLZOVATELY");
                    }
                    return user;
                }
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show($"Ошибка базы данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return user;
            }
        }

        public static bool CheckPassword(string password, string passRepeat)
        {
            if (password.Length < 6)
            {
                MessageBox.Show("Длина пароля не может быть меньше 6 символов", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            else
            {
                bool hasDigit = false;
                bool hasUpper = false;
                bool hasSpecial = false;
                string specialChars = "!@#$%^";

                foreach (char c in password)
                {
                    if (char.IsDigit(c)) hasDigit = true;
                    if (char.IsUpper(c)) hasUpper = true;
                    if (specialChars.Contains(c)) hasSpecial = true;
                }

                if (!hasDigit || !hasUpper)
                {
                    MessageBox.Show("Пароль должен содержать хотя бы одну цифру и одну заглавную букву!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                if (!hasSpecial)
                {
                    MessageBox.Show("Пароль должен содержать один из символов: !@#$%^", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                if (password != passRepeat)
                {
                    MessageBox.Show("Пароли не совпадают", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                return true;
            }
        }

        public static bool UserChek(string login)
        {
            try
            {
                using (NpgsqlConnection connect = new NpgsqlConnection(Connection.connectionStr))
                {
                    connect.Open();
                    string sqlQuery = "SELECT login FROM users WHERE login = @login";
                    NpgsqlCommand cmd = new NpgsqlCommand(sqlQuery, connect);
                    cmd.Parameters.AddWithValue("@login", login);
                    NpgsqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        MessageBox.Show("Такой логин уже есть"); return false;
                    }
                    else
                    {
                        reader.Close();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message); return false;
            }
        }
        public static void UserAdd(string login, string password, string fio)
        {
            using (NpgsqlConnection connect = new NpgsqlConnection(Connection.connectionStr))
                try
                {
                    connect.Open();

                    string sqlQuery = "Insert into users (login, password, fio) Values (@login, @password, @fio)";
                    string shifrPass = Verification.GetSHA512Hash(password);
                    NpgsqlCommand cmd = new NpgsqlCommand(sqlQuery, connect);
                    cmd.Parameters.AddWithValue("login", login);
                    cmd.Parameters.AddWithValue("password", shifrPass);
                    cmd.Parameters.AddWithValue("fio", fio);


                    int i = cmd.ExecuteNonQuery();
                    if (i == 1)
                    {
                        MessageBox.Show("Данные добавлены");
                    }
                }
                catch (SqlException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            //sqlConnection.Close();
        }


        public bool UpdateUser(int userId, string username, string email, string fio, string phone, DateTime? birthDate)
        {
            try
            {
                using (NpgsqlConnection connect = new NpgsqlConnection(Connection.connectionStr))
                {
                    connect.Open();
                    string sqlQuery = @"UPDATE users SET 
                                        username = @username,
                                        email = @email,
                                        fio = @fio,
                                        phone = @phone,
                                        birth_date = @birthDate
                                        WHERE id = @userId";

                    NpgsqlCommand cmd = new NpgsqlCommand(sqlQuery, connect);
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.AddWithValue("@fio", fio);
                    cmd.Parameters.AddWithValue("@phone", phone);
                    cmd.Parameters.AddWithValue("@userId", userId);

                    if (birthDate.HasValue)
                        cmd.Parameters.AddWithValue("@birthDate", birthDate.Value);
                    else
                        cmd.Parameters.AddWithValue("@birthDate", DBNull.Value);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected == 1;
                }
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show($"Ошибка обновления данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public bool ChangePassword(int userId, string currentPassword, string newPassword)
        {
            try
            {
                using (NpgsqlConnection connect = new NpgsqlConnection(Connection.connectionStr))
                {
                    connect.Open();

                    // Сначала проверяем текущий пароль
                    string checkQuery = "SELECT password_hash FROM users WHERE id = @userId";
                    NpgsqlCommand checkCmd = new NpgsqlCommand(checkQuery, connect);
                    checkCmd.Parameters.AddWithValue("@userId", userId);

                    string currentHashedPassword = checkCmd.ExecuteScalar()?.ToString();

                    if (currentHashedPassword == null || !Verification.VerifySHA512Hash(currentPassword, currentHashedPassword))
                    {
                        MessageBox.Show("Текущий пароль неверен", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }

                    // Обновляем пароль
                    string updateQuery = "UPDATE users SET password_hash = @newPassword WHERE id = @userId";
                    NpgsqlCommand updateCmd = new NpgsqlCommand(updateQuery, connect);
                    updateCmd.Parameters.AddWithValue("@newPassword", Verification.GetSHA512Hash(newPassword));
                    updateCmd.Parameters.AddWithValue("@userId", userId);

                    int rowsAffected = updateCmd.ExecuteNonQuery();
                    return rowsAffected == 1;
                }
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show($"Ошибка смены пароля: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }
}
