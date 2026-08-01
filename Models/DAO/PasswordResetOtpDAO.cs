using Models.EF;
using System;
using System.Linq;

namespace Models.DAO
{
    public class PasswordResetOtpDAO
    {
        private readonly KeyDbContext _context = new KeyDbContext();

        public PasswordResetOtp Create(int accountId, string codeHash)
        {
            var oldItems = _context.PasswordResetOtps
                .Where(x => x.AccountId == accountId && x.UsedAtUtc == null)
                .ToList();

            foreach (var item in oldItems)
                item.UsedAtUtc = DateTime.UtcNow;

            var entity = new PasswordResetOtp
            {
                AccountId = accountId,
                CodeHash = codeHash,
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(10),
                FailedAttempts = 0,
                CreatedAtUtc = DateTime.UtcNow
            };

            _context.PasswordResetOtps.Add(entity);
            _context.SaveChanges();
            return entity;
        }

        public PasswordResetOtp GetLatestActive(int accountId)
        {
            return _context.PasswordResetOtps
                .Where(x =>
                    x.AccountId == accountId
                    && x.UsedAtUtc == null
                    && x.ExpiresAtUtc > DateTime.UtcNow
                    && x.FailedAttempts < 5)
                .OrderByDescending(x => x.Id)
                .FirstOrDefault();
        }

        public void IncreaseFailure(long id)
        {
            var entity = _context.PasswordResetOtps.Find(id);
            if (entity == null) return;
            entity.FailedAttempts += 1;
            _context.SaveChanges();
        }

        public void MarkUsed(long id)
        {
            var entity = _context.PasswordResetOtps.Find(id);
            if (entity == null) return;
            entity.UsedAtUtc = DateTime.UtcNow;
            _context.SaveChanges();
        }
    }
}
