using Models.EF;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Models.DAO
{
    public class ProductDAO
    {
        private KeyDbContext _context;

        public ProductDAO()
        {
            _context = new KeyDbContext();
        }

        public IEnumerable<Product> GetProducts(string txtSearch, int page, int pageSize)
        {
            IQueryable<Product> model = _context.Products.Where(x => x.Status == true);
            if (!string.IsNullOrEmpty(txtSearch))
            {
                model = model.Where(x => x.Name.Contains(txtSearch) || x.Alias.Contains(txtSearch));
            }
            return model.OrderByDescending(x => x.Id).ToPagedList(page, pageSize);
        }

        public Product GetProductById(int id)
        {
            return _context.Products.Find(id);
        }

        public int AddProduct(Product product)
        {
            _context.Products.Add(product);
            _context.SaveChanges();
            return product.Id;
        }

        public bool UpdateProduct(Product newProduct)
        {
            try
            {
                var tempProduct = _context.Products.Find(newProduct.Id);
                tempProduct.Name = newProduct.Name;
                tempProduct.Alias = newProduct.Alias;
                tempProduct.CategoryId = newProduct.CategoryId;
                tempProduct.Images = newProduct.Images;
                tempProduct.Price = newProduct.Price;
                tempProduct.PriceSale = newProduct.PriceSale;
                tempProduct.Detail = newProduct.Detail;
                tempProduct.Status = newProduct.Status;
                _context.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool DeleteProduct(int id)
        {
            try
            {
                var temp = _context.Products.Find(id);
                _context.Products.Remove(temp);
                _context.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public List<Product> GetListProduct(int? top)
        {
            if (top != null)
            {
                return _context.Products.OrderByDescending(x => x.CreatedDate).Take(top.Value).ToList();
            }
            return _context.Products.OrderByDescending(x => x.CreatedDate).ToList();
        }

        public IEnumerable<Product> GetListProductByCategory(int categoryId, int page, int pageSize)
        {
            IQueryable<Product> model = _context.Products.Where(x => x.CategoryId == categoryId);
            return model.OrderByDescending(x => x.CreatedDate).ToPagedList(page, pageSize);
        }
    }
}
