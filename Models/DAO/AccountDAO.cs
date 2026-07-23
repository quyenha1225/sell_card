using Models.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PagedList;

namespace Models.DAO
{
    public class AccountDAO
    {
        private KeyDbContext _context = null;

        public AccountDAO()
        {
            _context = new KeyDbContext();
        }

        public int InsertUser(Account account)
        {
            _context.Accounts.Add(account);
            _context.SaveChanges();
            return account.Id;

        }

        public bool UpdateUser(Account account)
        {
            try
            {
                var user = _context.Accounts.Find(account.Id);
                user.Name = account.Name;
                if (!string.IsNullOrEmpty(account.Password))
                {
                    user.Password = account.Password;
                }
                user.Status = account.Status;
                _context.SaveChanges();
                return true;

            }
            catch (Exception ex)
            {

                return false;

            }
        }

        public bool DeleteUser(int id)
        {
            try
            {
                var user = _context.Accounts.Find(id);
                _context.Accounts.Remove(user);
                _context.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;

            }

        }

        public Account GetAccountById(int id)
        {
            return _context.Accounts.Find(id);
        }

        public Account GetAccountByUsername(string userName)
        {
            var user = _context.Accounts.SingleOrDefault(x => x.UserName == userName);
            return user;
        }

        public int Login(string u, string p)
        {
            var result = _context.Accounts.SingleOrDefault(x => x.UserName == u && x.Password == p);
            if (result != null)
            {
                if (result.Status == true)
                {

                    return 1; //Suc
                }
                else
                {
                    return -1; //Locked
                }
            }
            else
            {
                return 0; //Fail

            }
        }

        public IEnumerable<Account> GetListAccounts(string txtSearch, int page, int pageSize)
        {
            IQueryable<Account> model = _context.Accounts;
            if (!string.IsNullOrEmpty(txtSearch))
            {
                model = model.Where(x => x.UserName.Contains(txtSearch) || x.Name.Contains(txtSearch));
            }
            return model.OrderByDescending(x => x.Id).ToPagedList(page, pageSize);
        }
    }
}