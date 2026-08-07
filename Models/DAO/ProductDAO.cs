using Models.EF;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Models.DAO
{
    public class ProductDAO
    {
        private readonly KeyDbContext _context;

        public ProductDAO()
        {
            _context = new KeyDbContext();
        }

        // Danh sách dành cho Admin: lấy cả sản phẩm đang bật và đã tắt.
        public IEnumerable<Product> GetProducts(string txtSearch, int page, int pageSize)
        {
            page = Math.Max(1, page);
            pageSize = pageSize > 0 ? pageSize : 10;

            IQueryable<Product> query = _context.Products
                .AsNoTracking()
                .Include(x => x.Category);

            if (!string.IsNullOrWhiteSpace(txtSearch))
            {
                txtSearch = txtSearch.Trim();
                query = query.Where(x =>
                    x.Name.Contains(txtSearch) ||
                    x.Alias.Contains(txtSearch));
            }

            return query
                .OrderByDescending(x => x.Id)
                .ToPagedList(page, pageSize);
        }

        public Product GetProductById(int id)
        {
            return _context.Products
                .AsNoTracking()
                .Include(x => x.Category)
                .Include(x => x.Giftcard)
                .FirstOrDefault(x => x.Id == id);
        }

        public int AddProduct(Product product)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }

            _context.Products.Add(product);
            _context.SaveChanges();
            return product.Id;
        }

        public bool UpdateProduct(Product newProduct)
        {
            if (newProduct == null)
            {
                return false;
            }

            try
            {
                var currentProduct = _context.Products.Find(newProduct.Id);
                if (currentProduct == null)
                {
                    return false;
                }

                currentProduct.Name = newProduct.Name;
                currentProduct.Alias = newProduct.Alias;
                currentProduct.CategoryId = newProduct.CategoryId;
                currentProduct.Images = newProduct.Images;
                currentProduct.CostPrice = newProduct.CostPrice;
                currentProduct.Price = newProduct.Price;
                currentProduct.PriceSale = newProduct.PriceSale;
                currentProduct.Detail = newProduct.Detail;
                currentProduct.Status = newProduct.Status;

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
                var product = _context.Products.Find(id);
                if (product == null)
                {
                    return false;
                }

                _context.Products.Remove(product);
                _context.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Sản phẩm nổi bật ở menu. Chỉ lấy sản phẩm đang hoạt động.
        public List<Product> GetListProduct(int? top)
        {
            IQueryable<Product> query = _context.Products
                .AsNoTracking()
                .Where(x => x.Status == true)
                .OrderByDescending(x =>
                    x.PriceSale.HasValue &&
                    x.PriceSale.Value > 0 &&
                    x.PriceSale.Value < x.Price)
                .ThenByDescending(x => x.CreatedDate)
                .ThenByDescending(x => x.Id);

            if (top.HasValue && top.Value > 0)
            {
                query = query.Take(top.Value);
            }

            return query.ToList();
        }

        // Sản phẩm trang chủ.
        public IPagedList<Product> GetStorefrontProducts(int page, int pageSize)
        {
            page = Math.Max(1, page);
            pageSize = pageSize > 0 ? pageSize : 8;

            return _context.Products
                .AsNoTracking()
                .Include(x => x.Category)
                .Include(x => x.Giftcard)
                .Where(x => x.Status == true)
                .OrderByDescending(x =>
                    x.PriceSale.HasValue &&
                    x.PriceSale.Value > 0 &&
                    x.PriceSale.Value < x.Price)
                .ThenByDescending(x => x.CreatedDate)
                .ThenByDescending(x => x.Id)
                .ToPagedList(page, pageSize);
        }

        // Tìm kiếm sản phẩm theo tên, alias hoặc tên danh mục.
        public IPagedList<Product> SearchStorefrontProducts(
            string keyword,
            string sort,
            int page,
            int pageSize)
        {
            page = Math.Max(1, page);
            pageSize = pageSize > 0 ? pageSize : 12;

            IQueryable<Product> query = _context.Products
                .AsNoTracking()
                .Include(x => x.Category)
                .Include(x => x.Giftcard)
                .Where(x => x.Status == true);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();

                query = query.Where(x =>
                    x.Name.Contains(keyword) ||
                    x.Alias.Contains(keyword) ||
                    (x.Category != null && x.Category.Name.Contains(keyword)));
            }

            query = ApplyStorefrontSorting(query, sort);
            return query.ToPagedList(page, pageSize);
        }

        // Danh sách sản phẩm theo danh mục, có sắp xếp và phân trang.
        public IPagedList<Product> GetListProductByCategory(
            int categoryId,
            string sort,
            int page,
            int pageSize)
        {
            page = Math.Max(1, page);
            pageSize = pageSize > 0 ? pageSize : 8;

            IQueryable<Product> query = _context.Products
                .AsNoTracking()
                .Include(x => x.Category)
                .Include(x => x.Giftcard)
                .Where(x =>
                    x.CategoryId == categoryId &&
                    x.Status == true);

            query = ApplyStorefrontSorting(query, sort);
            return query.ToPagedList(page, pageSize);
        }

        // Giữ tương thích với những chỗ cũ đang gọi hàm ba tham số.
        public IEnumerable<Product> GetListProductByCategory(
            int categoryId,
            int page,
            int pageSize)
        {
            return GetListProductByCategory(
                categoryId,
                "latest",
                page,
                pageSize);
        }

        private static IQueryable<Product> ApplyStorefrontSorting(
            IQueryable<Product> query,
            string sort)
        {
            switch ((sort ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "price_asc":
                    return query
                        .OrderBy(x =>
                            x.PriceSale.HasValue &&
                            x.PriceSale.Value > 0 &&
                            x.PriceSale.Value < x.Price
                                ? x.PriceSale.Value
                                : x.Price)
                        .ThenByDescending(x => x.Id);

                case "price_desc":
                    return query
                        .OrderByDescending(x =>
                            x.PriceSale.HasValue &&
                            x.PriceSale.Value > 0 &&
                            x.PriceSale.Value < x.Price
                                ? x.PriceSale.Value
                                : x.Price)
                        .ThenByDescending(x => x.Id);

                case "name_asc":
                    return query
                        .OrderBy(x => x.Name)
                        .ThenByDescending(x => x.Id);

                case "name_desc":
                    return query
                        .OrderByDescending(x => x.Name)
                        .ThenByDescending(x => x.Id);

                case "latest":
                    return query
                        .OrderByDescending(x => x.CreatedDate)
                        .ThenByDescending(x => x.Id);

                case "sale":
                default:
                    return query
                        .OrderByDescending(x =>
                            x.PriceSale.HasValue &&
                            x.PriceSale.Value > 0 &&
                            x.PriceSale.Value < x.Price)
                        .ThenByDescending(x => x.CreatedDate)
                        .ThenByDescending(x => x.Id);
            }
        }
    }
}
