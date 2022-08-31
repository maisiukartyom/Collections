using System.ComponentModel.DataAnnotations;

namespace Collection.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        public string Content { get; set; }
        public int ItemId { get; set; }
        public string Name { get; set; }

    }
}
