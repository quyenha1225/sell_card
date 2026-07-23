using Models.EF;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DAO
{
    public class NewsDAO
    {
        private KeyDbContext _dbContext;

        public NewsDAO()
        {
            _dbContext = new KeyDbContext();
        }

        public IEnumerable<News> GetNews()
        {
            return _dbContext.News.OrderByDescending(x => x.Id).ToList();
        }

        public News GetNewsById(int id)
        {
            return _dbContext.News.Find(id);
        }

        public IEnumerable<News> GetAllNews(string txtSearch, int page, int pageSize)
        {
            IQueryable<News> model = _dbContext.News;
            if (!string.IsNullOrEmpty(txtSearch))
            {
                model = model.Where(x => x.Title.Contains(txtSearch));
            }
            return model.OrderByDescending(x => x.Id).ToPagedList(page, pageSize);
        }

        public int AddNews(News model)
        {
            _dbContext.News.Add(model);
            _dbContext.SaveChanges();
            return model.Id;
        }

        public bool UpdateNews(News model)
        {
            try
            {
                var temp = _dbContext.News.Find(model.Id);
                temp.Title = model.Title;
                temp.Description = model.Description;
                temp.Alias = model.Alias;
                temp.Image = model.Image;
                _dbContext.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool DeleteNews(int id)
        {
            try
            {
                var temp = _dbContext.News.Find(id);
                _dbContext.News.Remove(temp);
                _dbContext.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
