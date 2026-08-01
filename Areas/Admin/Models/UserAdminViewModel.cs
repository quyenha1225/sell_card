using System.ComponentModel.DataAnnotations;

namespace BBEcom.Areas.Admin.Models
{
    public class UserAdminViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 4)]
        public string UserName { get; set; }

        [StringLength(100, MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        public bool IsAdmin { get; set; }
        public bool Status { get; set; }
    }
}
