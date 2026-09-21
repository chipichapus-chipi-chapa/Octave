using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kurs_musik_club.Classes
{
    public class user_roles
    {
        public int id_role { get; set; }
        public string name_role{  get; set; }
       

        public user_roles(string name_role, int id_role)
        {
            this.name_role = name_role;
            this.id_role = id_role;
        }
    }
}
