using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using kurs_musik_club.Models;

namespace kurs_musik_club.windows
{
    public partial class CoursesWindow : Window
    {
        private ObservableCollection<Course> _courses;
        private ObservableCollection<Course> _filteredCourses;

        public CoursesWindow()
        {
            InitializeComponent();
            InitializeCourses();
        }

        private void InitializeCourses()
        {
            _courses = new ObservableCollection<Course>
            {
                new Course { Name = "Гитара для начинающих", TeacherName = "Иван Петров", Price = 5000, ImagePath = "/Images/guitar.jpg" },
                new Course { Name = "Уроки вокала", TeacherName = "Мария Иванова", Price = 6000, ImagePath = "/Images/vocal.jpg" },
                new Course { Name = "Барабаны", TeacherName = "Алексей Сидоров", Price = 5500, ImagePath = "/Images/drums.jpg" }
            };

            _filteredCourses = new ObservableCollection<Course>(_courses);
            CoursesListView.ItemsSource = _filteredCourses;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchTextBox.Text.ToLower().Trim();
            
            var filteredItems = _courses.Where(course =>
                string.IsNullOrEmpty(searchText) ||
                course.Name.ToLower().Contains(searchText) ||
                course.TeacherName.ToLower().Contains(searchText) ||
                course.Price.ToString().Contains(searchText)
            ).ToList();

            _filteredCourses.Clear();
            foreach (var course in filteredItems)
            {
                _filteredCourses.Add(course);
            }
        }
    }
} 