using System.ComponentModel.DataAnnotations;

namespace Collection.Models
{
    public class Item
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Tags { get; set; }
        public string Collection { get; set; }

    }
}
