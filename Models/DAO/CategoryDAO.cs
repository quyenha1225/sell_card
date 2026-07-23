using Models.EF;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DAO
{
    public class CategoryDAO
    {
        private KeyDbContext _context;

        public CategoryDAO()
        {
            _context = new KeyDbContext();
        }

        public IEnumerable<Category> GetCategories(string txtSearch, int page, int pageSize)
        {
            IQueryable<Category> model = _context.Categories;
            if (!string.IsNullOrEmpty(txtSearch))
            {
                model = model.Where(x => x.Name.Contains(txtSearch) || x.Alias.Contains(txtSearch));
            }
            return model.OrderByDescending(x => x.Id).ToPagedList(page, pageSize);
        }

        public List<Category> GetCategoriesName(int? top)
        {
            if (top != null)
            {
                return _context.Categories.Where(x => x.Status == true).Take(top.GetValueOrDefault()).OrderByDescending(x => x.CreatedDate).ToList();
            }
            return _context.Categories.Where(x => x.Status == true).ToList();
        }

        public Category GetCategoryById(int id)
        {
            return _context.Categories.Find(id);
        }

        public int GetCountProductCategory(int categoryId)
        {
            return _context.Products.Where(x => x.CategoryId == categoryId).Count();
        }

        public int AddCategory(Category category)
        {
            _context.Categories.Add(category);
            _context.SaveChanges();
            return category.Id;
        }

        public bool UpdateCategory(Category category)
        {
            try
            {
                var tempCategory = _context.Categories.Find(category.Id);
                tempCategory.Name = category.Name;
                tempCategory.Alias = category.Alias;
                tempCategory.Image = category.Image;
                tempCategory.Status = category.Status;
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
                var tempCategory = _context.Categories.Find(id);
                _context.Categories.Remove(tempCategory);
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
