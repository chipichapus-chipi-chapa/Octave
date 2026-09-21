using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kurs_musik_club.Classes
{
    public class courses
    {
        public int id_course {  get; set; }
        public int id_teacher { get; set; }
        public double duration { get; set; }
     
        public string name_course { get; set; }
        public string description { get; set; }
        public string teacher_name { get; set; }

        public courses(int id_course, int id_teacher, double duration, string name_course, string description)
        {
            this.id_course = id_course;
            this.id_teacher = id_teacher;
            this.duration = duration;
           
            this.name_course = name_course;
            this.description = description;
            this.teacher_name = "Преподаватель не назначен";
        }

        public courses(int id_course, int id_teacher, double duration,  string name_course, string description, string teacher_name)
        {
            this.id_course = id_course;
            this.id_teacher = id_teacher;
            this.duration = duration;
        
            this.name_course = name_course;
            this.description = description;
            this.teacher_name = teacher_name;
        }
    }
}
