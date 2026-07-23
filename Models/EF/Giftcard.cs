namespace Models.EF
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Giftcard")]
    public partial class Giftcard
    {
        public int Id { get; set; }

        public int? ProductId { get; set; }

        [StringLength(50)]
        public string Code { get; set; }

        public virtual Product Product { get; set; }
    }
}
