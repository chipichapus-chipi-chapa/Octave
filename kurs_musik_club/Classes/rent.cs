using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kurs_musik_club.Classes
{
    public class rent
    {
        public int id_rent {  get; set; }
        public int id_cost { get; set; }
        public int id_hall { get; set; }
        public DateTime rent_date {  get; set; }
        public int id_user { get; set; }
    }
}
