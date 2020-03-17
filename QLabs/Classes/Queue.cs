using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLiteNetExtensions.Attributes;
using SQLite;

namespace QLabs.Classes
{
    [Table("Queue")]
    public class Queue
    {
        [PrimaryKey, AutoIncrement, NotNull]
        public int QueueId { get; set; }
        [Unique]
        public int QueueNum { get; set; }
        [TextBlob(nameof(AddBlobbed))]
        public Dictionary<int, float> Add { get; set; }
        public string AddBlobbed { get; set; }
        [TextBlob(nameof(DeletedBlobbed))]
        public Dictionary<int, bool> Deleted { get; set; }
        public string DeletedBlobbed { get; set; }

        public Queue()
        {
            QueueNum = -1;
            Add = new Dictionary<int, float>();
            Deleted = new Dictionary<int, bool>();
        }
    }
}
