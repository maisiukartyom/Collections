using System.ComponentModel.DataAnnotations;

namespace Collection.Models
{
    public class CollectionProp
    {
        [Key]
        public int Id { get; set; }

        public string CollectionName { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }

    }
}
