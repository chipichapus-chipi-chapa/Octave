using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kurs_musik_club.Classes
{
    public class teachers
    {
        public int id_teacher { get; set; }
        public int id_employee { get; set; }
        public string fio { get; set; }
        public Uri photo { get; set; }

        public teachers()
        {
        }

        public teachers(int id_teacher, int id_employee, string fio)
        {
            this.id_teacher = id_teacher;
            this.id_employee = id_employee;
            this.fio = fio;
        }
    }
}
