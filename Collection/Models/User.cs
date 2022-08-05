using System.ComponentModel.DataAnnotations;

namespace Collection.Models
{
    public class User
    {
        [Key]
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool isAdmin { get; set; }
        public bool isBanned { get; set; }
    }
}
