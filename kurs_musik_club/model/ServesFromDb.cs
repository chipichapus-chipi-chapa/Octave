using kurs_musik_club.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace kurs_musik_club.model
{
    public class ServesFromDb
    {
      /*  public List<Service> LoadServices(string name_service)
        {
            var service = new List<services>();

            using (SqlConnection sqlConnection = new SqlConnection(Connection.connectionStr))
            {
                try
                {
                    sqlConnection.Open();
                    string sqlExp = "SELECT * FROM services";
                    SqlCommand command = new SqlCommand(sqlExp, sqlConnection);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                service.Add(new services(
                                     Convert.ToInt32(reader[0]),
                                   reader[1].ToString(),
                                  reader[2].ToString(),
                                   Convert.ToInt32(reader[3]),
                                  Convert.ToInt32(reader[4]))
                                );
                            }
                        }
                        //return service;
                    }
                    
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке услуг: {ex.Message}");
                   
                }
            }
        }

    }*/
}
}

