using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.IO;
using System.Web;
using System.Web.Hosting;
using Models.EF;

namespace BBEcom.Common
{
    public class CommonHelper
    {
        private static readonly string strCheck = "áàạảãâấầậẩẫăắằặẳẵÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴéèẹẻẽêếềệểễÉÈẸẺẼÊẾỀỆỂỄ" +
             "óòọỏõôốồộổỗơớờợởỡÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠúùụủũưứừựửữÚÙỤỦŨƯỨỪỰỬỮíìịỉĩÍÌỊỈĨđĐýỳỵỷỹÝỲỴỶỸ~!@#$%^&*()-[{]}|\\/'\"\\.,><;:";

        private static readonly string[] VietNamChar = new string[]
        {
            "aAeEoOuUiIdDyY",
            "áàạảãâấầậẩẫăắằặẳẵ",
            "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",
            "éèẹẻẽêếềệểễ",
            "ÉÈẸẺẼÊẾỀỆỂỄ",
            "óòọỏõôốồộổỗơớờợởỡ",
            "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",
            "úùụủũưứừựửữ",
            "ÚÙỤỦŨƯỨỪỰỬỮ",
            "íìịỉĩ",
            "ÍÌỊỈĨ",
            "đ",
            "Đ",
            "ýỳỵỷỹ",
            "ÝỲỴỶỸ"
        };

        public static string FilterChar(string str)
        {
            str = str.Trim();
            for (int i = 1; i < VietNamChar.Length; i++)
            {
                for (int j = 0; j < VietNamChar[i].Length; j++)
                {
                    str = str.Replace(VietNamChar[i][j], VietNamChar[0][i - 1]);
                }
            }
            str = str.Replace(" ", "-");
            str = str.Replace("--", "-");
            str = str.Replace("---", "-");
            str = str.Replace("?", "");
            str = str.Replace("&", "");
            str = str.Replace(",", "");
            str = str.Replace(":", "");
            str = str.Replace("!", "");
            str = str.Replace("'", "");
            str = str.Replace("\"", "");
            str = str.Replace("|", "");
            str = str.Replace("%", "");
            str = str.Replace("#", "");
            str = str.Replace("$", "");
            str = str.Replace("*", "");
            str = str.Replace("`", "");
            str = str.Replace("~", "");
            str = str.Replace("@", "");
            str = str.Replace("^", "");
            str = str.Replace(".", "");
            str = str.Replace("/", "");
            str = str.Replace(">", "");
            str = str.Replace("<", "");
            str = str.Replace("[", "");
            str = str.Replace("]", "");
            str = str.Replace(";", "");
            str = str.Replace("+", "");
            str = str.Replace("(", "");
            str = str.Replace(")", "");
            return str.ToLower();
        }

        public static string ChuyenCoDauThanhKhongDau(string str)
        {
            str = str.Trim();
            for (int i = 1; i < VietNamChar.Length; i++)
            {
                for (int j = 0; j < VietNamChar[i].Length; j++)
                {
                    str = str.Replace(VietNamChar[i][j], VietNamChar[0][i - 1]);
                }
            }
            str = str.Replace(" ", "-");
            str = str.Replace("--", "-");
            str = str.Replace("?", "");
            str = str.Replace("&", "");
            str = str.Replace(",", "");
            str = str.Replace(":", "");
            str = str.Replace("!", "");
            str = str.Replace("'", "");
            str = str.Replace("\"", "");
            str = str.Replace("%", "");
            str = str.Replace("#", "");
            str = str.Replace("$", "");
            str = str.Replace("*", "");
            str = str.Replace("`", "");
            str = str.Replace("~", "");
            str = str.Replace("@", "");
            str = str.Replace("^", "");
            str = str.Replace(".", "");
            str = str.Replace("/", "");
            str = str.Replace(">", "");
            str = str.Replace("<", "");
            str = str.Replace("[", "");
            str = str.Replace("]", "");
            str = str.Replace(";", "");
            str = str.Replace("+", "");
            str = str.Replace("(", "");
            str = str.Replace(")", "");
            return str.ToLower();
        }

        public static string ConvertMoney(int money)
        {
            if (money <= 0)
            {
                return 0.ToString();
            }
            CultureInfo cul = CultureInfo.GetCultureInfo("vi-VN");   // try with "en-US"
            string a = money.ToString("#,###", cul.NumberFormat);

            return a;

        }

        private static Random random = new Random();

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string RandomNumber(int length)
        {
            const string chars = "0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        // ==================== STOREFRONT / IMAGE / PRICE HELPERS ====================

        /// <summary>
        /// A sale is valid only when PriceSale is positive and lower than the original price.
        /// </summary>
        public static bool HasValidSale(Product product)
        {
            return product != null
                && product.Price > 0
                && product.PriceSale.HasValue
                && product.PriceSale.Value > 0
                && product.PriceSale.Value < product.Price;
        }

        public static int GetSellingPrice(Product product)
        {
            if (product == null)
            {
                return 0;
            }

            return HasValidSale(product)
                ? product.PriceSale.Value
                : product.Price;
        }

        public static int GetDiscountPercent(Product product)
        {
            if (!HasValidSale(product))
            {
                return 0;
            }

            decimal raw =
                (product.Price - product.PriceSale.Value)
                * 100m
                / product.Price;

            int rounded = (int)Math.Round(
                raw,
                MidpointRounding.AwayFromZero
            );

            return Math.Max(0, Math.Min(99, rounded));
        }

        /// <summary>
        /// Product images are resolved in this order:
        /// 1) file named exactly as Product.Alias in /img/products;
        /// 2) configured database path;
        /// 3) fallback image.
        /// A file-version query is appended to prevent stale browser cache.
        /// </summary>
        public static string ResolveProductImage(
            string alias,
            string configuredPath)
        {
            return ResolveImage(
                "~/Content/Client/img/products",
                alias,
                configuredPath,
                "~/Content/Client/img/error.png"
            );
        }

        public static string ResolveNewsImage(
            string alias,
            string configuredPath)
        {
            return ResolveNewsImage(
                alias,
                null,
                configuredPath
            );
        }

        /// <summary>
        /// Resolve ảnh tin tức trong Content/Client/img/news.
        /// Ưu tiên ánh xạ theo Alias/Tiêu đề, sau đó mới dùng đường dẫn DB.
        /// </summary>
        public static string ResolveNewsImage(
            string alias,
            string title,
            string configuredPath)
        {
            const string newsFolder =
                "~/Content/Client/img/news";

            string aliasKey =
                String.IsNullOrWhiteSpace(alias)
                    ? String.Empty
                    : FilterChar(alias);

            string titleKey =
                String.IsNullOrWhiteSpace(title)
                    ? String.Empty
                    : FilterChar(title);

            string searchKey =
                (aliasKey + "-" + titleKey)
                    .ToLowerInvariant();

            string mappedFile = null;

            // Các file ảnh hiện có trong thư mục img/news.
            if (searchKey.Contains("bao-mat")
                || searchKey.Contains("bao-mat-tai-khoan"))
            {
                mappedFile = "bao-mat-tai-khoan.jpg";
            }
            else if (searchKey.Contains("google-play")
                || searchKey.Contains("apple"))
            {
                mappedFile = "google-play-apple.jpg";
            }
            else if (searchKey.Contains("huong-dan-mua")
                || searchKey.Contains("mua-gift-card"))
            {
                mappedFile = "huong-dan-mua-gift-card.jpg";
            }
            else if (searchKey.Contains("playstation-plus"))
            {
                mappedFile = "playstation-plus.jpg";
            }
            else if (searchKey.Contains("steam-wallet"))
            {
                mappedFile = "steam-wallet-an-toan.jpg";
            }
            else if (searchKey.Contains("xbox-game-pass")
                || searchKey.Contains("loi-ich-xbox"))
            {
                mappedFile = "xbox-game-pass.jpg";
            }
            else if (searchKey.Contains("thanh-toan")
                || searchKey.Contains("vnpay")
                || searchKey.Contains("sepay"))
            {
                mappedFile = "thanh-toan-vnpay.jpg";
            }
            else if (searchKey.Contains("xu-ly-code")
                || searchKey.Contains("code-loi"))
            {
                mappedFile = "xu-ly-code-loi.jpg";
            }

            if (!String.IsNullOrWhiteSpace(mappedFile))
            {
                string mappedPath =
                    NormalizeVirtualPath(
                        newsFolder + "/" + mappedFile
                    );

                if (VirtualFileExists(mappedPath))
                {
                    return AppendFileVersion(mappedPath);
                }
            }

            // Tìm ảnh có tên trùng Alias.
            string byAlias =
                ResolveImage(
                    newsFolder,
                    alias,
                    configuredPath,
                    "~/Content/Client/img/error.png"
                );

            if (!String.IsNullOrWhiteSpace(byAlias)
                && !byAlias.Contains("/error.png"))
            {
                return byAlias;
            }

            // Nếu Alias không trùng, thử tên file theo tiêu đề.
            if (!String.IsNullOrWhiteSpace(titleKey))
            {
                string[] extensions =
                {
                    ".jpg",
                    ".png",
                    ".jpeg",
                    ".webp"
                };

                foreach (string extension in extensions)
                {
                    string candidate =
                        NormalizeVirtualPath(
                            newsFolder
                            + "/"
                            + titleKey
                            + extension
                        );

                    if (VirtualFileExists(candidate))
                    {
                        return AppendFileVersion(candidate);
                    }
                }
            }

            return VersionedImage(
                configuredPath,
                "~/Content/Client/img/error.png"
            );
        }

        public static string VersionedImage(
            string configuredPath,
            string fallbackPath = "~/Content/Client/img/error.png")
        {
            string normalized = NormalizeVirtualPath(configuredPath);

            if (VirtualFileExists(normalized))
            {
                return AppendFileVersion(normalized);
            }

            return AppendFileVersion(
                NormalizeVirtualPath(fallbackPath)
            );
        }

        private static string ResolveImage(
            string folder,
            string alias,
            string configuredPath,
            string fallbackPath)
        {
            string safeAlias =
                String.IsNullOrWhiteSpace(alias)
                    ? String.Empty
                    : alias.Trim();

            string[] extensions =
            {
                ".png",
                ".jpg",
                ".jpeg",
                ".webp",
                ".gif"
            };

            // Prefer the new asset whose filename matches the database Alias.
            if (!String.IsNullOrWhiteSpace(safeAlias))
            {
                foreach (string extension in extensions)
                {
                    string candidate =
                        NormalizeVirtualPath(
                            folder + "/" +
                            safeAlias +
                            extension
                        );

                    if (VirtualFileExists(candidate))
                    {
                        return AppendFileVersion(candidate);
                    }
                }
            }

            string configured =
                NormalizeVirtualPath(configuredPath);

            if (VirtualFileExists(configured))
            {
                return AppendFileVersion(configured);
            }

            return AppendFileVersion(
                NormalizeVirtualPath(fallbackPath)
            );
        }

        private static string NormalizeVirtualPath(
            string path)
        {
            if (String.IsNullOrWhiteSpace(path))
            {
                return String.Empty;
            }

            string normalized =
                path.Trim()
                    .Replace("\\", "/")
                    .Replace(
                        "/Content/Client/Images/",
                        "/Content/Client/img/"
                    )
                    .Replace(
                        "~/Content/Client/Images/",
                        "~/Content/Client/img/"
                    );

            if (normalized.StartsWith("~/"))
            {
                return normalized;
            }

            if (normalized.StartsWith("/"))
            {
                return "~" + normalized;
            }

            return "~/" + normalized.TrimStart('/');
        }

        private static bool VirtualFileExists(
            string virtualPath)
        {
            if (String.IsNullOrWhiteSpace(virtualPath))
            {
                return false;
            }

            try
            {
                string physical =
                    HostingEnvironment.MapPath(virtualPath);

                return !String.IsNullOrWhiteSpace(physical)
                    && File.Exists(physical);
            }
            catch
            {
                return false;
            }
        }

        private static string AppendFileVersion(
            string virtualPath)
        {
            if (String.IsNullOrWhiteSpace(virtualPath))
            {
                return String.Empty;
            }

            string absolute =
                VirtualPathUtility.ToAbsolute(virtualPath);

            try
            {
                string physical =
                    HostingEnvironment.MapPath(virtualPath);

                if (!String.IsNullOrWhiteSpace(physical)
                    && File.Exists(physical))
                {
                    long version =
                        File.GetLastWriteTimeUtc(physical).Ticks;

                    return absolute + "?v=" + version;
                }
            }
            catch
            {
                // Return the normal URL if version lookup fails.
            }

            return absolute;
        }

    }

}
