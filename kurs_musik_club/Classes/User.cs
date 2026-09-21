using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kurs_musik_club.Classes
{
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Fio { get; set; }
        public string Role { get; set; }

        public User()
        {
        }

        public User(int id, string login, string fio, string role)
        {
            Id = id;
            Login = login;
            Fio = fio;
            Role = role;
        }
    }
}
