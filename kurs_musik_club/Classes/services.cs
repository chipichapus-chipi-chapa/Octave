using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kurs_musik_club.Classes
{
    public class services
    {
        public int id_service {  get; set; }
        public string name_service { get; set; }
        public string description_service {  get; set; }
        public int id_cost { get; set; }
        public int id_client { get; set; }

        public services(int id_service, string name_service, string description_service, int id_cost, int id_client)
        {
            this.id_service = id_service;
            this.name_service = name_service;
            this.description_service = description_service;
            this.id_cost = id_cost;
            this.id_client = id_client;
        }
    }
}
