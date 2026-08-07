using Models.EF;
using System;

namespace BBEcom.Models
{
    [Serializable]
    public class Cart
    {
        public Product product { get; set; }
        public int Quantity { get; set; }
    }
}
