using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace kurs_musik_club.Models
{
    public class Course : INotifyPropertyChanged
    {
        private string _name;
        private string _teacherName;
        private decimal _price;
        private string _imagePath;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public string TeacherName
        {
            get => _teacherName;
            set
            {
                _teacherName = value;
                OnPropertyChanged();
            }
        }

        public decimal Price
        {
            get => _price;
            set
            {
                _price = value;
                OnPropertyChanged();
            }
        }

        public string ImagePath
        {
            get => _imagePath;
            set
            {
                _imagePath = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 