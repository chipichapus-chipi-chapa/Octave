namespace kurs_musik_club.model
{
    public class courses
    {
        public int id_course { get; set; }
        public string name_course { get; set; }
        public string description { get; set; }
        public int id_teacher { get; set; }
        public decimal price_per_hour { get; set; }
        public int id_hall { get; set; }
        public halls Hall { get; set; }

        public courses(int id_course, string name_course, string description, int id_teacher, decimal price_per_hour, int id_hall)
        {
            this.id_course = id_course;
            this.name_course = name_course;
            this.description = description;
            this.id_teacher = id_teacher;
            this.price_per_hour = price_per_hour;
            this.id_hall = id_hall;
        }
    }
} 