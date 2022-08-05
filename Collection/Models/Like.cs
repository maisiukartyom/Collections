using System.ComponentModel.DataAnnotations;

namespace Collection.Models
{
    public class Like
    {
        [Key]
        public int Id { get; set; }

        public string Item { get; set; }
        public string Name { get; set; }

    }
}
