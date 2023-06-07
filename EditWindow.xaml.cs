using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для EditWindow.xaml
    /// </summary>
    public partial class EditWindow : Window
    {
        public EditWindow()
        {
            InitializeComponent();
            textB1.Text = Request._name;
            comb.ItemsSource = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
            comb.SelectedValue = Request._semester;
            textB3.Text = Request._comment;
        }

        public void ButtonSaveClick(object sender, RoutedEventArgs e)
        {
            Request._name = textB1.Text;
            Request._semester = (int)comb.SelectedValue;
            Request._comment = textB3.Text;
            this.Close();

        }

        public void ButtonCancelClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
