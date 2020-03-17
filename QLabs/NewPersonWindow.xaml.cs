using System.Windows;
using QLabs.Classes;

namespace QLabs
{
    /// <summary>
    /// Interaction logic for NewPersonWindow.xaml
    /// </summary>
    public partial class NewPersonWindow : Window
    {
        public NewPersonWindow()
        {
            InitializeComponent();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!App.people.Exists(c => c.Name == tbName.Text) && tbName.Text != "")
            {
                Person person = new Person()
                {
                    Name = tbName.Text
                };
                App.people.Add(person);
                App.peopleAm++;
                Close();
            }
            else
            {
                MessageBox.Show("That person is already in queue list or the field is empty.", "Adding error", MessageBoxButton.OK, MessageBoxImage.Error);
                tbName.Text = "";
            }
        }
    }
}
