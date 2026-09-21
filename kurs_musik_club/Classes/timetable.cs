using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kurs_musik_club.Classes
{
    public class timetable
    {
        public int id_timetable {  get; set; }
        public DateTime date_and_time {  get; set; }
        public int id_hall {  get; set; }
        public int id_course {  get; set; }
    }
}
