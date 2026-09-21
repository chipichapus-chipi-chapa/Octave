using kurs_musik_club.Classes;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.Windows.Forms.Design;

namespace kurs_musik_club.model
{
    public class CorsesFromDb
    {
        public List<courses> GetCourses()
        {
            var courses = new List<courses>();

            using (NpgsqlConnection connection = new NpgsqlConnection(Connection.connectionStr))
            {
                connection.Open();
                var command = new NpgsqlCommand(@"
                    SELECT c.id_course, c.name_course, c.description, c.id_teacher, c.price_per_hour, c.id_hall 
                    FROM courses c 
                    JOIN teachers t ON c.id_teacher = t.id_teacher 
                    JOIN employees e ON t.id_employee = e.id_employee", connection);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        courses.Add(new courses(
                            reader.GetInt32(0),
                            reader.GetInt32(1),
                            reader.GetDouble(2),
                            reader.GetString(3),
                            reader.GetString(4),
                            reader.GetString(5)
                        ));
                    }
                }
            }

            return courses;
        }
    }
}
