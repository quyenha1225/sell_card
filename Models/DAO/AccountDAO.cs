using BBEcom.Common.Auth;
using Models.EF;
using PagedList;
using System;
using System.Data.Entity;
using System.Linq;

namespace Models.DAO
{
    public class AccountDAO
    {
        private readonly KeyDbContext _context = new KeyDbContext();

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
                if (user == null) return false;

                user.UserName = account.UserName;
                user.Name = account.Name;
                user.Email = account.Email;
                user.IsAdmin = account.IsAdmin;
                user.Status = account.Status;

                if (!String.IsNullOrWhiteSpace(account.Password))
                    user.Password = account.Password;

                _context.SaveChanges();
                return true;
            }
            catch { return false; }
        }

        public bool DeleteUser(int id)
        {
            try
            {
                var user = _context.Accounts.Find(id);
                if (user == null) return false;
                _context.Accounts.Remove(user);
                _context.SaveChanges();
                return true;
            }
            catch { return false; }
        }

        public Account GetAccountById(int id)
        {
            return _context.Accounts.AsNoTracking()
                .FirstOrDefault(x => x.Id == id);
        }

        public Account GetAccountByUsername(string userName)
        {
            if (String.IsNullOrWhiteSpace(userName)) return null;
            var value = userName.Trim();
            return _context.Accounts.FirstOrDefault(x => x.UserName == value);
        }

        public Account GetAccountByEmail(string email)
        {
            if (String.IsNullOrWhiteSpace(email)) return null;
            var value = email.Trim().ToLower();
            return _context.Accounts.FirstOrDefault(
                x => x.Email != null && x.Email.ToLower() == value);
        }

        public Account ValidateCredentials(string userName, string password)
        {
            var account = GetAccountByUsername(userName);

            if (account == null
                || !PasswordService.Verify(account.Password, password))
                return null;

            if (PasswordService.IsLegacyHash(account.Password))
            {
                account.Password = PasswordService.Hash(password);
                _context.SaveChanges();
            }

            return account;
        }

        public int Login(string u, string p)
        {
            var result = _context.Accounts.FirstOrDefault(
                x => x.UserName == u && x.Password == p);

            if (result == null) return 0;
            return result.Status ? 1 : -1;
        }

        public bool UpdatePassword(int accountId, string passwordHash)
        {
            var account = _context.Accounts.Find(accountId);
            if (account == null) return false;
            account.Password = passwordHash;
            _context.SaveChanges();
            return true;
        }

        public Account GetOrCreateGoogleAccount(
            string email, string displayName)
        {
            var existing = GetAccountByEmail(email);
            if (existing != null) return existing;

            var prefix = (email ?? "google").Split('@')[0]
                .Replace(" ", "").ToLowerInvariant();

            if (String.IsNullOrWhiteSpace(prefix)) prefix = "google";

            var userName = prefix;
            int suffix = 1;

            while (_context.Accounts.Any(x => x.UserName == userName))
                userName = prefix + suffix++;

            var account = new Account
            {
                UserName = userName,
                Name = String.IsNullOrWhiteSpace(displayName)
                    ? userName : displayName,
                Email = email,
                Password = PasswordService.Hash(Guid.NewGuid().ToString("N")),
                IsAdmin = false,
                Status = true
            };

            _context.Accounts.Add(account);
            _context.SaveChanges();
            return account;
        }

        public IPagedList<Account> GetListAccounts(
            string txtSearch, int page, int pageSize)
        {
            page = Math.Max(1, page);
            pageSize = Math.Max(5, Math.Min(100, pageSize));

            IQueryable<Account> query = _context.Accounts.AsNoTracking();

            if (!String.IsNullOrWhiteSpace(txtSearch))
                query = query.Where(x =>
                    x.UserName.Contains(txtSearch)
                    || x.Name.Contains(txtSearch)
                    || x.Email.Contains(txtSearch));

            return query.OrderByDescending(x => x.Id)
                .ToPagedList(page, pageSize);
        }
    }
}
