using System.ComponentModel.DataAnnotations;

namespace Collection.Models
{
    public class Like
    {
        [Key]
        public int Id { get; set; }

        public int ItemId { get; set; }

        public string Owner { get; set; }

    }
}
