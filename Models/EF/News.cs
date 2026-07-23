namespace Models.EF
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    using System.Web.Mvc;

    public partial class News
    {
        public int Id { get; set; }

        [StringLength(100)]
        public string Title { get; set; }

        [AllowHtml]
        public string Description { get; set; }

        [StringLength(100)]
        public string Alias { get; set; }

        [StringLength(100)]
        public string Image { get; set; }

        public DateTime? CreateDate { get; set; }
    }
}
