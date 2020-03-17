using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLabs.Classes
{
    public class QPerson
    {
        public int Place { get; set; }
        public string Name { get; set; }
        public float Add { get; set; }
        public bool Deleted { get; set; }

        public QPerson()
        {
            Place = 0;
            Name = "";
            Add = 0;
            Deleted = false;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
