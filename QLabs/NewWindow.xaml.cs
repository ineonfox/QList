using System;
using Microsoft.Win32;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using QLabs.Classes;
using SQLite;

namespace QLabs
{
    /// <summary>
    /// Interaction logic for NewWindow.xaml
    /// </summary>
    public partial class NewWindow : Window
    {
        bool saved = false;
        List<Person> people = new List<Person>();

        public NewWindow()
        {
            InitializeComponent();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (tbNewPerson.Text == "")
            {
                MessageBox.Show("Enter person's name.", "Empty Name", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            else
            {
                try
                {
                    Repeats();
                }
                catch(InvalidOperationException)
                {
                    Person person = new Person
                    {
                        Name = tbNewPerson.Text
                    };
                    lstNewPeople.Items.Add(person.ToString());
                    people.Add(person);
                    tbNewPerson.Text = "";
                }
            }
        }

        private void BtnDel_Click(object sender, RoutedEventArgs e)
        {
            var c = MessageBox.Show("Do you really want to delete that person?", "Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (c == MessageBoxResult.Yes)
            {
                people.Remove(people.Single(d => d.Name == lstNewPeople.SelectedItem.ToString()));
                lstNewPeople.Items.Remove(lstNewPeople.SelectedItem);
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (btnEdit.Content.ToString() == "Edit chosen person")
            {
                btnEdit.Content = "Finish editing";
                tbNewPerson.Text = lstNewPeople.SelectedItem.ToString();
                lstNewPeople.IsEnabled = false;
                btnAdd.IsEnabled = false;
                btnCancel.IsEnabled = false;
                btnDel.IsEnabled = false;
                btnSave.IsEnabled = false;
            }
            else
            {
                try
                {
                    Repeats();
                }
                catch
                {
                    btnEdit.Content = "Edit chosen person";
                    people[people.FindIndex(c => c.Name == lstNewPeople.SelectedItem.ToString())].Name = tbNewPerson.Text;
                    lstNewPeople.Items.Remove(lstNewPeople.SelectedItem);
                    lstNewPeople.Items.Add(tbNewPerson.Text);
                    lstNewPeople.IsEnabled = true;
                    btnAdd.IsEnabled = true;
                    btnCancel.IsEnabled = true;
                    btnDel.IsEnabled = true;
                    btnSave.IsEnabled = true;
                }
            }
        }

        void Repeats()
        {
            Person p = people.Single(c => c.Name == tbNewPerson.Text);
            MessageBox.Show($"Sorry, {p.Name} is already in queue.", "Repeating Name", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (tbQName.Text == "")
            {
                MessageBox.Show("Enter queue name.", "Empty queue name", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            else
            {
                SaveFileDialog saveFile = new SaveFileDialog
                {
                    Filter = "Database files (*.db)|*.db",
                    FileName = tbQName.Text,
                    DefaultExt = ".db"
                };
                if (saveFile.ShowDialog() == true)
                {
                    using (SQLiteConnection connection = new SQLiteConnection(saveFile.FileName))
                    {
                        connection.CreateTable<Person>();
                        connection.InsertAll(people.OrderBy(c => c.Name));
                    }
                    saved = true;
                    App.fPath = saveFile.FileName;
                    Close();
                }
                else
                {
                    MessageBox.Show("Error saving file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!saved)
            {
                var c = MessageBox.Show("Are you sure you want to leave?", "New Queue", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                if (c == MessageBoxResult.No)
                {
                    e.Cancel = true;
                }
            }
        }
    }
}
