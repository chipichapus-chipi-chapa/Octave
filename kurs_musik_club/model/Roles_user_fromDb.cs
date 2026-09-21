using kurs_musik_club.Classes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace kurs_musik_club.model
{
    internal class Roles_user_fromDb
    {
        public List<user_roles> LoadRoles()
        {

            
            var roles = new List<user_roles>();
            SqlConnection sqlConnection = new SqlConnection(Connection.connectionStr);
            try
            {

                sqlConnection.Open();
                string sqlExp = "select * from user_roles";
                SqlCommand command = new SqlCommand(sqlExp, sqlConnection);
                SqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        roles.Add(new user_roles(reader[0].ToString(),Convert.ToInt32( reader[1])));

                    }
                }
                reader.Close();
                return roles;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return roles;
            }
            finally
            {
                sqlConnection.Close();
            }
        }
    }
   /* public List<user_roles> FiltrRoles(int idRole)
    {
        List<user_roles> roles = new List<user_roles>();

        SqlConnection sqlConnection = new SqlConnection(Connection.connectionStr);
        try
        {
            sqlConnection.Open();
            string sqlExp = "select name_role from user_roles";
            SqlCommand command = new SqlCommand(sqlExp, sqlConnection);
            command.Parameters.AddWithValue("id_role", idRole);
            SqlDataReader reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    roles.Add(new user_roles(reader[0].ToString(), Convert.ToInt32(reader[1])));
                }
            }
            reader.Close();
            return roles;

        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
            return roles;
        }
        finally
        {
            sqlConnection.Close();
        }
    }*/
}
