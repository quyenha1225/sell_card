using Models.EF;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DAO
{
    public class FaqDAO
    {
        private KeyDbContext _context;

        public FaqDAO()
        {
            _context = new KeyDbContext();
        }

        public IEnumerable<Faq> GetFaqs(string txtSearch, int page, int pageSize)
        {
            IQueryable<Faq> model = _context.Faqs;
            if (!string.IsNullOrEmpty(txtSearch))
            {
                model = model.Where(x => x.Question.Contains(txtSearch));
            }
            return model.OrderByDescending(x => x.Id).ToPagedList(page, pageSize);
        }

        public Faq GetFaq(int id)
        {
            return _context.Faqs.Find(id);
        }

        public int AddFAQ(Faq faq)
        {
            _context.Faqs.Add(faq);
            _context.SaveChanges();
            return faq.Id;
        }

        public bool UpdateFAQ(Faq newFaq)
        {
            try
            {
                var temp = _context.Faqs.Find(newFaq.Id);
                temp.Question = newFaq.Question;
                temp.Answer = newFaq.Answer;

                _context.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool DeleteFaq(int id)
        {
            try
            {
                var temp = _context.Faqs.Find(id);
                _context.Faqs.Remove(temp);
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
