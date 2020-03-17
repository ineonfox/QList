using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace QLabs.Classes
{
    [Table("Person")]
    public class Person
    {
        [PrimaryKey, AutoIncrement, NotNull]
        public int PersonId { get; set; }
        [Unique]
        public string Name { get; set; }
        public float Points { get; set; }

        public Person()
        {
            this.Name = "";
            this.Points = 0;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
