using System.ComponentModel.DataAnnotations;

namespace Collection.Models
{
    public class MCollection
    {
        [Key]
        public string Name { get; set; }

        public string Description { get; set; }

        public string Theme { get; set; }

        public string Image { get; set; }

        public string Owner { get; set; }
        public int Size { get; set; }
        public string InputFields { get; set; }
    }
}
