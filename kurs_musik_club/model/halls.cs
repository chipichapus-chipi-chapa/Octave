namespace kurs_musik_club.model
{
    public class halls
    {
        public int id_hall { get; set; }
        public string hall_name { get; set; }
        public int capasity { get; set; }
        public int id_cost { get; set; }
        public string description { get; set; }
        public string image_path { get; set; }

        public halls(int id_hall, string hall_name, int capasity, int id_cost, string description, string image_path)
        {
            this.id_hall = id_hall;
            this.hall_name = hall_name;
            this.capasity = capasity;
            this.id_cost = id_cost;
            this.description = description;
            this.image_path = image_path;
        }
    }
} 