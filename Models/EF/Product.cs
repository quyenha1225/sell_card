namespace Models.EF
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    using System.Web.Mvc;

    [Table("Product")]
    public partial class Product
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Product()
        {
            Giftcard = new HashSet<Giftcard>();
            OrderDetail = new HashSet<OrderDetail>();
        }

        public int Id { get; set; }

        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(100)]
        public string Alias { get; set; }

        public int? CategoryId { get; set; }

        [StringLength(500)]
        public string Images { get; set; }

        public DateTime? CreatedDate { get; set; }

        // Giá nhập / giá vốn. Chỉ dùng trong khu vực quản trị và tính lợi nhuận.
        public int CostPrice { get; set; }

        // Giá khuyến mãi; null/0 nghĩa là không giảm giá.
        public int? PriceSale { get; set; }

        // Giá bán niêm yết.
        public int Price { get; set; }

        [AllowHtml]
        public string Detail { get; set; }

        public bool? Status { get; set; }

        public virtual Category Category { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Giftcard> Giftcard { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<OrderDetail> OrderDetail { get; set; }
    }
}
