using kurs_musik_club.Classes;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kurs_musik_club.model
{
    internal class Halls_fromDb
    {
        public List<halls> GetAllHalls()
        {
            var hallsList = new List<halls>();

            using (NpgsqlConnection connection = new NpgsqlConnection(Connection.connectionStr))
            {
                connection.Open();
                var command = new NpgsqlCommand("SELECT id_hall, hall_name, сapacity, id_cost, description, image_path FROM halls", connection);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        hallsList.Add(new halls(
                            id_hall: reader.GetInt32(0),
                            hall_name: reader.GetString(1),
                            capacity: reader.GetInt32(2),
                            id_cost: reader.GetInt32(3),
                            description: reader.IsDBNull(4) ? null : reader.GetString(4),
                            image_path: reader.IsDBNull(5) ? null : reader.GetString(5)
                        ));
                    }
                }
            }

            return hallsList;
        }
    }
}
