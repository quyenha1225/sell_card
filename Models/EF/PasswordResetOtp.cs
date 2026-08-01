namespace Models.EF
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("PasswordResetOtp")]
    public class PasswordResetOtp
    {
        public long Id { get; set; }
        public int AccountId { get; set; }

        [Required]
        [StringLength(128)]
        public string CodeHash { get; set; }

        public DateTime ExpiresAtUtc { get; set; }
        public int FailedAttempts { get; set; }
        public DateTime? UsedAtUtc { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
