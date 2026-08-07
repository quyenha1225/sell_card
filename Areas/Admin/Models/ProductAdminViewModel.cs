using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace BBEcom.Areas.Admin.Models
{
    public class ProductAdminViewModel : IValidatableObject
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm.")]
        [StringLength(100)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục.")]
        public int? CategoryId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn đường dẫn ảnh.")]
        [StringLength(500)]
        public string Images { get; set; }
        [Range(0, Int32.MaxValue)]
        public int CostPrice { get; set; }
        [Range(1000, Int32.MaxValue)]
        public int Price { get; set; }

        [Range(0, Int32.MaxValue)]
        public int? PriceSale { get; set; }

        [AllowHtml]
        public string Detail { get; set; }

        public bool Status { get; set; }

        public IEnumerable<ValidationResult> Validate(
    ValidationContext validationContext)
        {
            if (CostPrice <= 0)
            {
                yield return new ValidationResult(
                    "Giá nhập phải lớn hơn 0.",
                    new[] { "CostPrice" });
            }

            if (Price <= CostPrice)
            {
                yield return new ValidationResult(
                    "Giá bán phải lớn hơn giá nhập.",
                    new[] { "Price" });
            }

            if (PriceSale.HasValue
                && PriceSale.Value > 0)
            {
                if (PriceSale.Value >= Price)
                {
                    yield return new ValidationResult(
                        "Giá khuyến mãi phải nhỏ hơn giá bán.",
                        new[] { "PriceSale" });
                }

                if (PriceSale.Value <= CostPrice)
                {
                    yield return new ValidationResult(
                        "Giá khuyến mãi phải lớn hơn giá nhập.",
                        new[] { "PriceSale" });
                }
            }
        }
    }
}
