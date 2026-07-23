namespace Models.EF
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("OrderDetail")]
    public partial class OrderDetail
    {
        public int Id { get; set; }

        public int OrderID { get; set; }

        public int ProductID { get; set; }

        public int Price { get; set; }

        public int Quantity { get; set; }

        [Required]
        [StringLength(50)]
        public string Code { get; set; }

        public virtual Order Order { get; set; }

        public virtual Product Product { get; set; }
    }
}
