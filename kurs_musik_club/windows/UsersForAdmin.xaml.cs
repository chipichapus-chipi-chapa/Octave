using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace kurs_musik_club.windows
{
    /// <summary>
    /// Логика взаимодействия для UsersForAdmin.xaml
    /// </summary>
    public partial class UsersForAdmin : Window
    {
        public UsersForAdmin()
        {
            InitializeComponent();
            data1.AutoGenerateColumns = false;
            data1.Columns[0].DataPropertyName = "UsertId";
            data1.Columns[1].DataPropertyName = "FirsName";
            data1.Columns[2].DataPropertyName = "Patronymic";
            data1.Columns[3].DataPropertyName = "LastName";
            data1.Columns[4].DataPropertyName = "RoleId";
            data1.Columns[0].Visible = false;
        }
    }
}
