using Models.EF;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DAO
{
    public class GiftcardDAO
    {
        private KeyDbContext _context;

        public GiftcardDAO()
        {
            _context = new KeyDbContext();
        }

        public Giftcard TakeAndDelete(int categoryProduct)
        {
            var code = _context.Giftcards.FirstOrDefault(x => x.Product.Id == categoryProduct);
            _context.Giftcards.Remove(code);
            _context.SaveChanges();
            return code;
        }

        public bool CheckStock(int productID, int count)
        {
            using (KeyDbContext dbContext = new KeyDbContext())
            {
                var check = _context.Giftcards.Where(x => x.ProductId == productID).ToList();
                return check.Count >= count;
            }
        }

        public IEnumerable<Giftcard> GetGifts(string txtSearch, int page, int pageSize)
        {
            IQueryable<Giftcard> model = _context.Giftcards;
            if (!string.IsNullOrEmpty(txtSearch))
            {
                model = model.Where(x => x.Product.Name == txtSearch);
            }
            return model.OrderByDescending(x => x.Id).ToPagedList(page, pageSize);
        }

        public bool DeleteGift(int id)
        {
            try
            {
                var temp = _context.Giftcards.Find(id);
                _context.Giftcards.Remove(temp);
                _context.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public Giftcard GetGiftbyId(int id)
        {
            return _context.Giftcards.Find(id);
        }

        public int AddGift(Giftcard model)
        {
            _context.Giftcards.Add(model);
            _context.SaveChanges();
            return model.Id;
        }

        public bool UpdateGift(Giftcard model)
        {
            try
            {
                var temp = _context.Giftcards.Find(model.Id);
                temp.Code = model.Code;
                temp.ProductId = model.ProductId;
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
