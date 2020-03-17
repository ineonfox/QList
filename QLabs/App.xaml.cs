using System.Collections.Generic;
using System.Windows;
using QLabs.Classes;

namespace QLabs
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static List<Person> people = new List<Person>();
        public static List<Queue> queues = new List<Queue>();
        public static List<List<QPerson>> qPeople = new List<List<QPerson>>();
        public static string fPath;
        public static int currentQueue;
        public static int maxQueue = 0;
        public static int peopleAm = 0;
    }
}
