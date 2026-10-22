namespace BBEcom.Migrations
{
    using System.Data.Entity.Migrations;
    using global::Models.EF;

    internal sealed class Configuration
        : DbMigrationsConfiguration<KeyDbContext>
    {
        public Configuration()
        {
            // Cho phép cập nhật schema local khi model thay đổi (ví dụ CostPrice).
            AutomaticMigrationsEnabled = true;
            ContextKey = "Models.EF.KeyDbContext";
        }

        protected override void Seed(KeyDbContext context)
        {
            // Thêm dữ liệu mẫu ở đây nếu cần.
        }
    }
}