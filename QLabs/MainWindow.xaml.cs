using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using SQLite;
using QLabs.Classes;
using Microsoft.Win32;
using SQLiteNetExtensions.Extensions;
using System.Globalization;

namespace QLabs
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MenuCredits_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("QLabs 1.0.0\nCreated by Mykhailo Masliukh.\nThis app is free to use.\n\nUkraine, Lviv, 2020", "Credits", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MenuHelp_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("When you open QLabs, there are two empty fields:\nLeft - queues in current queue list\nRight - people in current queue list\n\nClick Queue List->New Queue List to create new queue list or if you already have one, click Queue List->Open Queue List\nNow you can use context menus in these fields for editing queues or people list by double - clickng right button on them.\n\nAlso, by double - clicking left button on queue number you can change current queue faster!", "Help", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MenuNew_Click(object sender, RoutedEventArgs e)
        {
            if (AskSaveFile() != MessageBoxResult.Cancel)
            {
                NewWindow newWindow = new NewWindow();
                newWindow.ShowDialog();
                if (App.fPath != null)
                {
                    ReadDatabase();
                    UpdateTextBlock();
                    CheckButtons();
                }
            }
        }

        private void MenuOpen_Click(object sender, RoutedEventArgs e)
        {
            if (AskSaveFile() != MessageBoxResult.Cancel)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "Database files (*.db)|*.db|All files (*.*)|*.*"
                };
                if (openFileDialog.ShowDialog() == true)
                {
                    App.fPath = openFileDialog.FileName;
                    ReadDatabase();
                    UpdateTextBlock();
                    CheckButtons();
                }
                else
                {
                    MessageBox.Show("Error opening database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        void ReadDatabase()
        {
            using (SQLiteConnection connection = new SQLiteConnection(App.fPath))
            {
                connection.CreateTable<Person>();
                App.people = connection.Table<Person>().ToList();

                connection.CreateTable<Queue>();
                App.queues = connection.GetAllWithChildren<Queue>().ToList().OrderBy(c => c.QueueNum).ToList();
            }

            App.maxQueue = App.queues.Count;
            App.currentQueue = App.maxQueue;
            App.peopleAm = App.people.Count;
            cntxQueueMenu.IsEnabled = true;

            UpdateQPeople();

            lbQueue.ItemsSource = null;
            if (App.people != null)
            {
                lbPeople.ItemsSource = App.people;
            }
            if (App.maxQueue > 0)
            {
                lbQueue.ItemsSource = App.qPeople[App.maxQueue - 1];
            }
            else
            {
                cntxDelQPerson.IsEnabled = false;
                cntxAddQPerson.IsEnabled = false;
            }
        }

        void UpdateQPeople()
        {
            App.qPeople.Clear();
            for (int i = 0; i < App.maxQueue; i++)
            {
                List<QPerson> q = new List<QPerson>();
                for (int j = 0; j < App.peopleAm; j++)
                {
                    try
                    {
                        QPerson qPerson = new QPerson
                        {
                            Name = App.people[j].Name,
                            Add = App.queues[i].Add[App.people[j].PersonId],
                            Deleted = App.queues[i].Deleted[App.people[j].PersonId]
                        };

                        if (qPerson.Deleted != true)
                        {
                            q.Add(qPerson);
                        }
                    }
                    catch (KeyNotFoundException)
                    {
                        App.queues[i].Add.Add(App.people[j].PersonId, 0);
                        App.queues[i].Deleted.Add(App.people[j].PersonId, true);
                    }
                }
                q = q.OrderByDescending(c => c.Add).ToList();
                int count = 0;
                foreach (QPerson c in q)
                {
                    count++;
                    c.Place = count;
                }
                App.qPeople.Add(q);
            }
        }

        Queue GenerateQ()
        {
            Queue q = new Queue
            {
                QueueNum = App.queues.Count + 1
            };
            Random rand = new Random();
            List<int> final = new List<int>();
            List<int> cur = new List<int>();
            float curPoints = float.MaxValue;
            float min = 0;
            int sizeCur;
            float currentPlayer = 0;
            while (final.Count != App.peopleAm)
            {   //adding people's Ids to cur, whose points are next-minimum
                for (int i = 0; i < App.peopleAm; i++)
                {
                    currentPlayer = (float)Math.Round(App.people[i].Points, 1);
                    if (curPoints >= currentPlayer && min <= currentPlayer)
                    {
                        if (curPoints == currentPlayer)
                        {
                            cur.Add(App.people[i].PersonId);
                        }
                        else
                        {
                            curPoints = currentPlayer;
                            cur.Clear();
                            cur.Add(App.people[i].PersonId);
                        }
                    }
                }
                sizeCur = cur.Count;
                min = curPoints;
                //If there's > 1 person with equal points, randomly assign them to the final
                if (sizeCur > 1)
                {
                    int index = 0;
                    float randMax = -1;
                    int[] rans = new int[sizeCur];
                    for (int i = 0; i < sizeCur; i++)
                    {
                        rans[i] = rand.Next(0, 100);
                    }
                    for (int i = 0; i < sizeCur; i++)
                    {
                        for (int j = 0; j < sizeCur; j++)
                        {
                            if (randMax < rans[j])
                            {
                                randMax = rans[j];
                                index = j;
                            }
                        }
                        final.Add(cur[index]);
                        q.Deleted.Add(cur[index], false);
                        rans[index] = -1;
                        randMax = -1;
                    }
                }
                else
                {
                    final.Add(cur[0]);
                    q.Deleted.Add(cur[0], false);
                }
                curPoints = float.MaxValue;
                cur.Clear();
                sizeCur = 0;
                min += (float)0.0001;
            }

            //Add points to those guys by dividing them by 11 groups and adding points with 2.0x-1.0x coefs
            double groupSize = App.peopleAm / 11.0;
            int groupNum = 0;
            float pointsToAdd;
            Dictionary<int, float> addToPerson = new Dictionary<int, float>();

            for (int i = 0; i < App.peopleAm; i++)
            {
                if (i + 1 > Convert.ToInt32(groupSize * (groupNum + 1)))
                {
                    groupNum++;
                }
                pointsToAdd = (float)Math.Round((App.peopleAm - i) * (2 - (groupNum / (float)10)), 1);
                App.people.Single(c => c.PersonId == final[i]).Points += (float)Math.Round(pointsToAdd, 1);
                addToPerson.Add(final[i], (float)Math.Round(pointsToAdd, 1));
            }

            q.Add = addToPerson;
            return q;
        }

        void AddToDatabase()
        {
            if (CheckDatabasePath())
            {
                using (SQLiteConnection connection = new SQLiteConnection(App.fPath))
                {
                    connection.CreateTable<Queue>();
                    connection.CreateTable<Person>();
                    connection.InsertOrReplaceAllWithChildren(App.queues);
                    connection.InsertOrReplaceAllWithChildren(App.people);
                }
            }
        }

        void UpdateTextBlock()
        {
            tblCurPos.Text = $"{App.currentQueue}/{App.maxQueue}";
        }

        void UpdatePeopleListView()
        {
            lbPeople.Items.Refresh();
        }
        
        void UpdateQueueListView()
        {
            lbQueue.ItemsSource = App.qPeople[App.currentQueue - 1];
        }

        void CheckButtons()
        {
            if (App.currentQueue == 0)
            {
                btnNext.IsEnabled = false;
                btnPrev.IsEnabled = false;
                cntxDelQPerson.IsEnabled = false;
                cntxAddQPerson.IsEnabled = false;
            }
            else if (App.currentQueue == 1)
            {
                if (App.currentQueue == App.maxQueue)
                    btnNext.IsEnabled = false;
                else
                    btnNext.IsEnabled = true;
                btnPrev.IsEnabled = false;
            }
            else if (App.currentQueue == App.maxQueue)
            {
                btnNext.IsEnabled = false;
                btnPrev.IsEnabled = true;
            }
            else
            {
                btnNext.IsEnabled = true;
                btnPrev.IsEnabled = true;
            }
            if (App.maxQueue > 0)
            {
                cntxDelQPerson.IsEnabled = true;
                cntxAddQPerson.IsEnabled = true;
            }
        }

        private void MenuSave_Click(object sender, RoutedEventArgs e)
        {
            AddToDatabase();
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CntxAddQPerson_Click(object sender, RoutedEventArgs e)
        {
            if (CheckDatabasePath())
            {
                if (App.people.Count != App.qPeople[App.currentQueue - 1].Count)
                {
                    List<Person> p = new List<Person>();
                    foreach (Person per in App.people)
                    {
                        if (App.queues[App.currentQueue - 1].Deleted[per.PersonId] && App.queues[App.currentQueue - 1].Add[per.PersonId] != 0)
                        {
                            p.Add(per);
                        }
                    }
                    AddPersonWindow addPersonWindow = new AddPersonWindow(p);
                    addPersonWindow.ShowDialog();
                    UpdateQPeople();
                    UpdateQueueListView();
                    UpdatePeopleListView();
                }
                else
                {
                    MessageBox.Show("There is already all people in this queue.", "Full queue", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
        }

        private void CntxDelQPerson_Click(object sender, RoutedEventArgs e)
        {
            if (CheckDatabasePath())
            {
                if (CheckSelectedQueue())
                {
                    string name = lbQueue.SelectedItem.ToString();
                    var res = MessageBox.Show($"Are you sure you want to delete {name} from queue?", "Delete person from queue", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (res == MessageBoxResult.Yes)
                    {
                        App.people.Single(c => c.Name == name).Points -= (float)Math.Round(App.qPeople[App.currentQueue - 1][lbQueue.SelectedIndex].Add, 1);
                        App.queues[App.currentQueue - 1].Deleted[App.people.Single(c => c.Name == name).PersonId] = true;
                        UpdateQPeople();
                        UpdatePeopleListView();
                        UpdateQueueListView();
                    }
                }
            }
        }

        private void CntxAddQueue_Click(object sender, RoutedEventArgs e)
        {
            Queue q = GenerateQ();
            App.queues.Add(q);
            App.maxQueue++;
            App.currentQueue = App.maxQueue;
            UpdateTextBlock();
            UpdateQPeople();
            lbQueue.ItemsSource = App.qPeople[App.maxQueue - 1];
            UpdatePeopleListView();
            CheckButtons();
        }

        private void CntxDelQueue_Click(object sender, RoutedEventArgs e)
        {
            if (App.queues.Count != 0)
            {
                var res = MessageBox.Show("Are you sure you want to delete last queue?\nThe queue will be deleted permanently.", "Delete queue", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.Yes)
                {
                    int last = App.queues.Count - 1;
                    for (int i = 0; i < App.peopleAm; i++)
                    {
                        if (!App.queues[last].Deleted[App.people[i].PersonId])
                        {
                            App.people[i].Points -= (float)Math.Round(App.queues[last].Add[App.people[i].PersonId], 1);
                        }
                    }

                    using (SQLiteConnection connection = new SQLiteConnection(App.fPath))
                    {
                        connection.Delete(App.queues.Last());
                    }
                    App.queues.Remove(App.queues.Last());
                    App.qPeople.Remove(App.qPeople.Last());

                    if (App.currentQueue == App.maxQueue)
                    {
                        App.currentQueue--;
                    }
                    App.maxQueue--;
                    UpdateTextBlock();
                    UpdatePeopleListView();

                    if (App.currentQueue != 0)
                    {
                        lbQueue.ItemsSource = App.qPeople[App.currentQueue - 1];
                    }
                    else
                    {
                        lbQueue.ItemsSource = null;
                    }
                    CheckButtons();
                }
            }
            else
            {
                MessageBox.Show("There is no queues to delete.", "Error", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            App.currentQueue++;
            lbQueue.ItemsSource = App.qPeople[App.currentQueue - 1];
            CheckButtons();
            UpdateTextBlock();
        }

        private void BtnPrev_Click(object sender, RoutedEventArgs e)
        {
            App.currentQueue--;
            lbQueue.ItemsSource = App.qPeople[App.currentQueue - 1];
            CheckButtons();
            UpdateTextBlock();
        }

        private void CntxAddPerson_Click(object sender, RoutedEventArgs e)
        {
            if (CheckDatabasePath())
            {
                NewPersonWindow newPersonWindow = new NewPersonWindow();
                newPersonWindow.ShowDialog();
                UpdatePeopleListView();
            }
        }

        private void CntxDelPerson_Click(object sender, RoutedEventArgs e)
        {
            if (CheckDatabasePath())
            {
                if (CheckSelectedPerson())
                {
                    string name = lbPeople.SelectedItem.ToString();
                    var res = MessageBox.Show($"Are you sure you want to delete {name}? {name} will be deleted permanently.", "Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (res == MessageBoxResult.Yes)
                    {
                        Person p = App.people.Single(c => c.Name == name);
                        int id = p.PersonId;
                        for (int i = 0; i < App.maxQueue; i++)
                        {
                            App.queues[i].Add.Remove(id);
                            App.queues[i].Deleted.Remove(id);
                        }
                        App.people.Remove(p);
                        App.peopleAm--;
                        UpdateQPeople();
                        UpdatePeopleListView();
                        UpdateQueueListView();

                        using (SQLiteConnection connection = new SQLiteConnection(App.fPath))
                        {
                            connection.Delete(p);
                        }
                    }
                }
            }
        }

        bool CheckSelectedQueue()
        {
            if (lbQueue.SelectedItem != null)
            {
                return true;
            }
            else
            {
                MessageBox.Show("Select a person in queue.", "No selected person", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }
        }

        bool CheckSelectedPerson()
        {
            if (lbPeople.SelectedItem != null)
            {
                return true;
            }
            else
            {
                MessageBox.Show("Select a person.", "No selected person", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }
        }

        bool CheckDatabasePath()
        {
            if (App.fPath != null)
            {
                return true;
            }
            else
            {
                MessageBox.Show("Open a database first.", "No database opened", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private void CntxEditPoints_Click(object sender, RoutedEventArgs e)
        {
            if (CheckDatabasePath())
            {
                if (CheckSelectedPerson())
                {
                    ListViewItem item = lbPeople.ItemContainerGenerator.ContainerFromIndex(lbPeople.SelectedIndex) as ListViewItem;
                    ContentPresenter templateParent = GetFrameworkElementByName<ContentPresenter>(item);
                    TextBox tbEditPoints = null;
                    TextBlock tblPoints = null;
                    DataTemplate dataTemplate = lbPeople.ItemTemplate;
                    tbEditPoints = dataTemplate.FindName("tbEditPoints", templateParent) as TextBox;
                    tblPoints = dataTemplate.FindName("tblPoints", templateParent) as TextBlock;
                    tbEditPoints.Visibility = Visibility.Visible;
                    tblPoints.Visibility = Visibility.Hidden;
                    tblPoints.Focus();
                }
            }
        }

        //Function by Jim Zhou - MSFT on MSDN
        private static T GetFrameworkElementByName<T>(FrameworkElement referenceElement) where T : FrameworkElement
        {
            FrameworkElement child = null;
            for (Int32 i = 0; i < VisualTreeHelper.GetChildrenCount(referenceElement); i++)
            {
                child = VisualTreeHelper.GetChild(referenceElement, i) as FrameworkElement;
                System.Diagnostics.Debug.WriteLine(child);
                if (child != null && child.GetType() == typeof(T))
                { break; }
                else if (child != null)
                {
                    child = GetFrameworkElementByName<T>(child);
                    if (child != null && child.GetType() == typeof(T))
                    {
                        break;
                    }
                }
            }
            return child as T;
        }

        private MessageBoxResult AskSaveFile()
        {
            if (App.fPath != null)
            {
                var result = MessageBox.Show("Do you want to save changes in this database?", "Exit", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    AddToDatabase();
                }
                return result;
            }
            return MessageBoxResult.No;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (AskSaveFile() == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ListViewItem item = lbPeople.ItemContainerGenerator.ContainerFromIndex(lbPeople.SelectedIndex) as ListViewItem;
            ContentPresenter templateParent = GetFrameworkElementByName<ContentPresenter>(item);
            TextBox tbEditPoints = null;
            TextBlock tblPoints = null;
            DataTemplate dataTemplate = lbPeople.ItemTemplate;
            tbEditPoints = dataTemplate.FindName("tbEditPoints", templateParent) as TextBox;
            tblPoints = dataTemplate.FindName("tblPoints", templateParent) as TextBlock;
            tbEditPoints.Visibility = Visibility.Hidden;
            tblPoints.Visibility = Visibility.Visible;
            try
            {
                App.people.Single(c => c.Name == lbPeople.SelectedItem.ToString()).Points = float.Parse(tbEditPoints.Text, CultureInfo.InvariantCulture.NumberFormat);
                UpdatePeopleListView();
            }
            catch (FormatException)
            {
                MessageBox.Show("Enter a number with a float point in the field.", "Wrong symbols", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TblCurPos_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (CheckDatabasePath())
                {
                    tblCurPos.Visibility = Visibility.Hidden;
                    tbPos.Visibility = Visibility.Visible;
                    tbPos.Text = App.currentQueue.ToString();
                    tbPos.Focus();
                }
            }
        }

        private void TbPos_LostFocus(object sender, RoutedEventArgs e)
        {
            int cur = 0;
            try
            {
                cur = Convert.ToInt32(tbPos.Text);
            }
            catch (FormatException)
            {
                MessageBox.Show("Enter an integer in the field.", "Wrong symbols", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (cur > App.maxQueue)
            {
                MessageBox.Show("Enter an integer smaller than a maximum queue number.", "Wrong number", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                App.currentQueue = cur;
                UpdateTextBlock();
                UpdateQueueListView();
            }
            tblCurPos.Visibility = Visibility.Visible;
            tbPos.Visibility = Visibility.Hidden;
        }
    }
}
