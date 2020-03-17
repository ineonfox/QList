using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using QLabs.Classes;

namespace QLabs
{
    /// <summary>
    /// Interaction logic for AddPersonWindow.xaml
    /// </summary>
    public partial class AddPersonWindow : Window
    {
        public AddPersonWindow(List<Person> people)
        {
            InitializeComponent();
            lvAddPeople.ItemsSource = people;
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (lvAddPeople.SelectedItem != null)
            {
                int idToAdd = App.people.Single(c => c.Name == lvAddPeople.SelectedItem.ToString()).PersonId;
                App.people.Single(c => c.PersonId == idToAdd).Points += (float)Math.Round(App.queues[App.currentQueue - 1].Add[idToAdd], 1);
                App.queues[App.currentQueue - 1].Deleted[idToAdd] = false;

                Close();
            }
            else
            {
                MessageBox.Show("No chosen person to add to the queue.", "No selection", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }
    }
}
