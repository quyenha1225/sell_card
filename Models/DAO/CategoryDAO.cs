using Models.EF;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Models.DAO
{
    public class CategoryDAO
    {
        private readonly KeyDbContext _context;

        public CategoryDAO()
        {
            _context = new KeyDbContext();
        }

        public IEnumerable<Category> GetCategories(
            string txtSearch,
            int page,
            int pageSize)
        {
            page = Math.Max(1, page);
            pageSize = pageSize > 0 ? pageSize : 10;

            IQueryable<Category> query = _context.Categories.AsNoTracking();

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

        public List<Category> GetCategoriesName(int? top)
        {
            IQueryable<Category> query = _context.Categories
                .AsNoTracking()
                .Where(x => x.Status == true)
                .OrderByDescending(x => x.CreatedDate)
                .ThenByDescending(x => x.Id);

            if (top.HasValue && top.Value > 0)
            {
                query = query.Take(top.Value);
            }

            return query.ToList();
        }

        public Category GetCategoryById(int id)
        {
            return _context.Categories
                .AsNoTracking()
                .FirstOrDefault(x => x.Id == id);
        }

        public int GetCountProductCategory(int categoryId)
        {
            return _context.Products.Count(x =>
                x.CategoryId == categoryId &&
                x.Status == true);
        }

        public int AddCategory(Category category)
        {
            if (category == null)
            {
                throw new ArgumentNullException(nameof(category));
            }

            _context.Categories.Add(category);
            _context.SaveChanges();
            return category.Id;
        }

        public bool UpdateCategory(Category category)
        {
            if (category == null)
            {
                return false;
            }

            try
            {
                var currentCategory = _context.Categories.Find(category.Id);
                if (currentCategory == null)
                {
                    return false;
                }

                currentCategory.Name = category.Name;
                currentCategory.Alias = category.Alias;
                currentCategory.Image = category.Image;
                currentCategory.Status = category.Status;

                _context.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool DeleteCategory(int id)
        {
            try
            {
                var category = _context.Categories.Find(id);
                if (category == null)
                {
                    return false;
                }

                _context.Categories.Remove(category);
                _context.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
