using System.ComponentModel.DataAnnotations;

namespace Collection.Models
{
    public class Tag
    {
        [Key]
        public int Id { get; set; }
        public int ItemId { get; set; }
        public string Name { get; set; }
    }
}
