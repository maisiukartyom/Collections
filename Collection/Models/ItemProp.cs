using System.ComponentModel.DataAnnotations;

namespace Collection.Models
{
    public class ItemProp
    {
        [Key]
        public int Id { get; set; }

        public int ItemId { get; set; }
        public int ColPropId { get; set; }
        public string Value { get; set; }

    }
}
