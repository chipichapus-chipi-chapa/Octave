using kurs_musik_club.Classes;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kurs_musik_club.model
{
    public class record_mix_fromDb
    {
        public List<recording_and_mixing> GetRecording_And_Mixings()
        {
            var recording_s = new List<recording_and_mixing>();

            using (NpgsqlConnection connection = new NpgsqlConnection(Connection.connectionStr))
            {
                connection.Open();
                var command = new NpgsqlCommand("SELECT id_recording_and_mixing,recording_duration,name_rec FROM recording_and_mixing", connection);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        recording_s.Add(new recording_and_mixing(
                            id_recording_and_mixing: reader.GetInt32(0),
                            recording_duration: reader.GetDouble(1),
                            name_rec: reader.GetString(2)
                           
                            
                        ));
                    }
                }
            }

            return recording_s;
        }
    }
}
