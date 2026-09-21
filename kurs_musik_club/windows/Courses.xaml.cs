using kurs_musik_club.Classes;
using kurs_musik_club.model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.SqlClient;
using System.Configuration;
using Npgsql;

namespace kurs_musik_club.windows
{
    /// <summary>
    /// Логика взаимодействия для Courses.xaml
    /// </summary>
    public partial class Courses : Window
    {
        public ObservableCollection<courses> _corses = new ObservableCollection<courses>();
        public ObservableCollection<courses> _filter_corses = new ObservableCollection<courses>();
        CorsesFromDb corsesFromDb = new CorsesFromDb();
        public List<courses> courses = new List<courses>();
        public List<employees> employees = new List<employees>();
        public List<teachers> teachersList = new List<teachers>();
        int selectedIndex = -1;
        bool noSelectedIndex = true;

        int count_dish_all = 0;
        int count_dish_select = 0;

        private string connectionString;

        public Courses()
        {
            InitializeComponent();
            NpgsqlConnection connect = new NpgsqlConnection(Connection.connectionStr);
            LoadTeachers();
            LoadCourses();
        }

        public void ViewAllCourses()
        {
            var courses = corsesFromDb.GetCourses();
            listViewcards.ItemsSource = courses;
        }

        private void listViewcards_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCourses();
        }

        private void LoadCourses()
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(Connection.connectionStr))
                {
                    connection.Open();
                    string query = @"SELECT c.id_course, COALESCE(c.id_teacher, 0) as id_teacher, c.duration, 
                                 c.name_course, COALESCE(c.description, 'Описание отсутствует') as description, 
                                 COALESCE(e.fio, 'Преподаватель не назначен') as teacher_name
                           FROM courses c
                           LEFT JOIN teachers t ON c.id_teacher = t.id_teacher
                           LEFT JOIN employees e ON t.id_employee = e.id_employee
                           ORDER BY c.name_course";
            
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            var coursesList = new List<courses>();
                            int count = 0;
                            while (reader.Read())
                            {
                                count++;
                                var course = new courses(
                                    reader.GetInt32(0),  // id_course
                                    reader.GetInt32(1),  // id_teacher
                                    reader.GetDouble(2), // duration
                                    reader.GetString(3), // name_course
                                    reader.GetString(4), // description
                                    reader.GetString(5)  // teacher_name
                                );
                                coursesList.Add(course);
                            }
                            
                            if (count == 0)
                            {
                                MessageBox.Show(
                                    "В базе данных нет курсов. Проверьте подключение к базе данных и наличие данных в таблице courses.",
                                    "Информация",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information
                                );
                            }
                            
                            this.courses = coursesList;
                            _corses = new ObservableCollection<courses>(coursesList);
                            _filter_corses = new ObservableCollection<courses>(coursesList);
                            listViewcards.ItemsSource = _filter_corses;
                        }
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show(
                    $"Ошибка базы данных при загрузке курсов: {ex.Message}",
                    "Ошибка базы данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при загрузке курсов: {ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private courses GetCourseById(int courseId)
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(Connection.connectionStr))
                {
                    connection.Open();
                    string query = @"SELECT c.id_course, COALESCE(c.id_teacher, 0) as id_teacher, c.duration, 
                                 c.name_course, COALESCE(c.description, 'Описание отсутствует') as description,
                                 COALESCE(e.fio, 'Преподаватель не назначен') as teacher_name
                                   FROM courses c
                                   LEFT JOIN teachers t ON c.id_teacher = t.id_teacher
                                   LEFT JOIN employees e ON t.id_employee = e.id_employee
                                   WHERE c.id_course = @courseId";

                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@courseId", courseId);
                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var course = new courses(
                                    reader.GetInt32(0),  // id_course
                                    reader.GetInt32(1),  // id_teacher
                                    reader.GetDouble(2), // duration
                                    reader.GetString(3), // name_course
                                    reader.GetString(4), // description
                                    reader.GetString(5)  // teacher_name
                                );
                                return course;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при получении данных курса: {ex.Message}");
            }
            return null;
        }

        private void MenuItem_Details_Click(object sender, RoutedEventArgs e)
        {
            var selectedCourse = (courses)listViewcards.SelectedItem;
            if (selectedCourse != null)
            {
                var detailsWindow = new CourseDetailsWindow(selectedCourse);
                detailsWindow.ShowDialog();
            }
        }

        private void MenuItem_Enroll_Click(object sender, RoutedEventArgs e)
        {
            var selectedCourse = (courses)listViewcards.SelectedItem;
            if (selectedCourse != null)
            {
                var enrollWindow = new CourseEnrollWindow(selectedCourse);
                enrollWindow.ShowDialog();
            }
        }

        private void Grid_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var grid = sender as Grid;
            if (grid != null)
            {
                var listViewItem = FindParent<ListViewItem>(grid);
                if (listViewItem != null)
                {
                    listViewItem.IsSelected = true;
                }
            }
        }

        private T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(child);
            if (parent == null) return null;
            if (parent is T) return parent as T;
            return FindParent<T>(parent);
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void LoadTeachers()
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(Connection.connectionStr))
                {
                    connection.Open();
                    string query = @"
                        SELECT t.id_teacher, e.fio, e.id_employee
                        FROM teachers t
                        JOIN employees e ON t.id_employee = e.id_employee
                        ORDER BY e.fio";

                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            var teachersList = new List<teachers>();
                            while (reader.Read())
                            {
                                var teacher = new teachers(
                                    reader.GetInt32(0),  // id_teacher
                                    reader.GetInt32(2),  // id_employee
                                    reader.GetString(1)   // fio
                                );
                                teachersList.Add(teacher);
                            }

                            if (teachersList.Count == 0)
                            {
                                MessageBox.Show(
                                    "В базе данных нет преподавателей.",
                                    "Информация",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information
                                );
                            }

                            this.teachersList = teachersList;
                        }
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show(
                    $"Ошибка базы данных при загрузке преподавателей: {ex.Message}",
                    "Ошибка базы данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при загрузке преподавателей: {ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchTextBox.Text.ToLower().Trim();
            
            var filteredItems = _corses.Where(course =>
                string.IsNullOrEmpty(searchText) ||
                course.name_course.ToLower().Contains(searchText) ||
                course.teacher_name.ToLower().Contains(searchText)
            ).ToList();

            _filter_corses.Clear();
            foreach (var course in filteredItems)
            {
                _filter_corses.Add(course);
            }
        }
    }
}
