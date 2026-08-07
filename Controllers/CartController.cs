using BBEcom.Common;
using BBEcom.Common.Auth;
using BBEcom.Models;
using Microsoft.Ajax.Utilities;
using Models.DAO;
using Models.EF;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Globalization;
using BBEcom.Models.ViewModels;
using System.Data.Entity;
using System.Security.Cryptography;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
namespace BBEcom.Controllers
{
    public class CartController : MemberBaseController
    {
        private const string CartSession = "CART_SESSION";
        private const int MaxCartItemQuantity = 100;
        // GET: Cart
        public ActionResult Index()
        {
            var list = RefreshCartFromDatabase(
                Session[CartSession] as List<Cart>
            );

            Session[CartSession] = list;
            return View(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddItem(int productID, int quantity)
        {
            var product = new ProductDAO().GetProductById(productID);

            if (product == null || product.Status != true)
            {
                return CartFailure(
                    "Sản phẩm không tồn tại hoặc đã ngừng bán.",
                    Url.Action("Index", "Home")
                );
            }

            quantity = Math.Max(
                1,
                Math.Min(MaxCartItemQuantity, quantity)
            );

            var list =
                Session[CartSession] as List<Cart>
                ?? new List<Cart>();

            var currentItem =
                list.FirstOrDefault(x =>
                    x.product.Id == productID);

            int currentQuantity =
                currentItem == null
                    ? 0
                    : currentItem.Quantity;

            int available =
                new GiftcardDAO()
                    .GetAvailableStock(productID);

            long requestedLong =
                (long)currentQuantity + quantity;

            if (requestedLong > available)
            {
                return CartFailure(
                    "Sản phẩm chỉ còn lại "
                    + available
                    + " cái.",
                    Url.Action(
                        "Detail",
                        "Product",
                        new
                        {
                            alias = product.Alias,
                            id = product.Id
                        }
                    ),
                    available
                );
            }

            if (currentItem == null)
            {
                list.Add(new Cart
                {
                    product = product,
                    Quantity = quantity
                });
            }
            else
            {
                currentItem.product = product;
                currentItem.Quantity = (int)requestedLong;
            }

            Session[CartSession] = list;

            if (Request.IsAjaxRequest())
            {
                return Json(new
                {
                    status = true,
                    cartCount = list.Sum(x => x.Quantity),
                    message = "Đã thêm sản phẩm vào giỏ hàng."
                });
            }

            var returnUrl =
                Request.UrlReferrer == null
                    ? null
                    : Request.UrlReferrer.PathAndQuery;

            if (!String.IsNullOrWhiteSpace(returnUrl)
                && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BuyNow(int productID, int quantity)
        {
            var product = new ProductDAO().GetProductById(productID);
            quantity = Math.Max(
                1,
                Math.Min(MaxCartItemQuantity, quantity)
            );

            if (product == null || product.Status != true)
            {
                TempData["CartError"] =
                    "Sản phẩm không tồn tại hoặc đã ngừng bán.";

                return RedirectToAction("Index", "Home");
            }

            int available =
                new GiftcardDAO()
                    .GetAvailableStock(productID);

            if (quantity > available)
            {
                TempData["CartError"] =
                    "Sản phẩm chỉ còn lại "
                    + available
                    + " cái.";

                return RedirectToAction(
                    "Detail",
                    "Product",
                    new
                    {
                        alias = product.Alias,
                        id = product.Id
                    }
                );
            }

            Session[CartSession] = new List<Cart>
            {
                new Cart
                {
                    product = product,
                    Quantity = quantity
                }
            };

            // Buy Now stops at the cart for user confirmation.
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Update(string cartModel)
        {
            var sessionCart =
                Session[CartSession] as List<Cart>;

            if (sessionCart == null || !sessionCart.Any())
            {
                return Json(new
                {
                    status = false,
                    message = "Giỏ hàng đang trống."
                });
            }

            List<Cart> requested;

            try
            {
                requested =
                    new JavaScriptSerializer()
                        .Deserialize<List<Cart>>(cartModel);
            }
            catch
            {
                return Json(new
                {
                    status = false,
                    message = "Dữ liệu giỏ hàng không hợp lệ."
                });
            }

            if (requested == null)
            {
                return Json(new
                {
                    status = false,
                    message = "Dữ liệu giỏ hàng không hợp lệ."
                });
            }

            var productDao = new ProductDAO();
            var giftcardDao = new GiftcardDAO();

            foreach (var cartItem in sessionCart)
            {
                var requestedItem =
                    requested.FirstOrDefault(x =>
                        x.product != null
                        && x.product.Id == cartItem.product.Id);

                if (requestedItem == null)
                {
                    continue;
                }

                int quantity =
                    Math.Max(
                        1,
                        Math.Min(
                            MaxCartItemQuantity,
                            requestedItem.Quantity
                        )
                    );

                var currentProduct =
                    productDao.GetProductById(
                        cartItem.product.Id
                    );

                if (currentProduct == null
                    || currentProduct.Status != true)
                {
                    return Json(new
                    {
                        status = false,
                        productId = cartItem.product.Id,
                        message = "Một sản phẩm đã ngừng bán."
                    });
                }

                int available =
                    giftcardDao.GetAvailableStock(
                        currentProduct.Id
                    );

                if (quantity > available)
                {
                    return Json(new
                    {
                        status = false,
                        productId = currentProduct.Id,
                        availableStock = available,
                        message =
                            "Sản phẩm "
                            + currentProduct.Name
                            + " chỉ còn lại "
                            + available
                            + " cái."
                    });
                }

                cartItem.product = currentProduct;
                cartItem.Quantity = quantity;
            }

            Session[CartSession] = sessionCart;

            var responseItems = sessionCart.Select(x =>
            {
                int unitPrice =
                    CommonHelper.GetSellingPrice(x.product);

                return new
                {
                    productId = x.product.Id,
                    quantity = x.Quantity,
                    unitPrice = unitPrice,
                    lineTotal = unitPrice * x.Quantity
                };
            }).ToList();

            int total =
                responseItems.Sum(x => x.lineTotal);

            return Json(new
            {
                status = true,
                cartCount = sessionCart.Sum(x => x.Quantity),
                total = total,
                items = responseItems,
                message = "Giỏ hàng đã được cập nhật."
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DeleteAllCart()
        {
            Session[CartSession] = null;

            return Json(new
            {
                status = true,
                cartCount = 0,
                total = 0
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Delete(long id)
        {
            var list =
                Session[CartSession] as List<Cart>
                ?? new List<Cart>();

            list.RemoveAll(x => x.product.Id == id);
            Session[CartSession] = list;

            int total = list.Sum(x =>
                CommonHelper.GetSellingPrice(x.product)
                * x.Quantity
            );

            return Json(new
            {
                status = true,
                cartCount = list.Sum(x => x.Quantity),
                total = total
            });
        }

        [HttpGet]
        public ActionResult Payment()
        {
            var list = RefreshCartFromDatabase(
                Session[CartSession] as List<Cart>
            );

            Session[CartSession] = list;
            return View(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Payment(Order ignoredOrder)
        {
            var list = RefreshCartFromDatabase(
                Session[CartSession] as List<Cart>
            );

            if (list == null || !list.Any())
            {
                TempData["CartError"] = "Giỏ hàng đang trống.";
                return RedirectToAction("Index");
            }

            var merchantId =
                ConfigurationManager.AppSettings["SePayMerchantId"];

            var secretKey =
                ConfigurationManager.AppSettings["SePaySecretKey"];

            if (String.IsNullOrWhiteSpace(merchantId)
                || merchantId.StartsWith("YOUR_")
                || String.IsNullOrWhiteSpace(secretKey)
                || secretKey.StartsWith("YOUR_"))
            {
                TempData["CartError"] =
                    "SePay chưa được cấu hình Merchant ID và Secret Key.";

                return RedirectToAction("Index");
            }

            var giftcardDao = new GiftcardDAO();

            foreach (var item in list)
            {
                int available =
                    giftcardDao.GetAvailableStock(item.product.Id);

                if (item.Quantity > available)
                {
                    TempData["CartError"] =
                        "Sản phẩm "
                        + item.product.Name
                        + " chỉ còn lại "
                        + available
                        + " cái.";

                    Session[CartSession] = list;
                    return RedirectToAction("Index");
                }
            }

            var user =
                CurrentUser.Get();

            if (user == null)
            {
                return RedirectToAction(
                    "Index",
                    "Login",
                    new
                    {
                        returnUrl = Url.Action("Payment", "Cart")
                    }
                );
            }

            var orderCode =
                "DH"
                + DateTime.UtcNow.ToString("yyyyMMddHHmmss")
                + Guid.NewGuid().ToString("N")
                    .Substring(0, 5)
                    .ToUpperInvariant();

            int total = list.Sum(item =>
                CommonHelper.GetSellingPrice(item.product)
                * item.Quantity
            );

            var order = new Order
            {
                OrderCode = orderCode,
                CustomerID = user.UserId,
                TotalAmount = total,
                Status = false,
                CreatedDate = DateTime.Now
            };

            foreach (var item in list)
            {
                int unitPrice =
                    CommonHelper.GetSellingPrice(item.product);

                for (int index = 0;
                     index < item.Quantity;
                     index++)
                {
                    order.OrderDetail.Add(new OrderDetail
                    {
                        ProductID = item.product.Id,
                        Price = unitPrice,
                        Quantity = 1,
                        Code = "PENDING"
                    });
                }
            }

            new OrderDAO().AddOrder(order);
            Session["SEPAY_PENDING_ORDER"] = orderCode;

            var publicUrl = GetExternalBaseUrl();

            var fields = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(
                    "order_amount",
                    total.ToString(CultureInfo.InvariantCulture)
                ),
                new KeyValuePair<string, string>("merchant", merchantId),
                new KeyValuePair<string, string>("currency", "VND"),
                new KeyValuePair<string, string>("operation", "PURCHASE"),
                new KeyValuePair<string, string>(
                    "order_description",
                    "Thanh toan don hang " + orderCode
                ),
                new KeyValuePair<string, string>(
                    "order_invoice_number",
                    orderCode
                ),
                new KeyValuePair<string, string>(
                    "customer_id",
                    user.UserId.ToString(CultureInfo.InvariantCulture)
                ),
                new KeyValuePair<string, string>(
                    "payment_method",
                    "BANK_TRANSFER"
                ),
                new KeyValuePair<string, string>(
                    "success_url",
                    publicUrl
                    + Url.Action(
                        "SePaySuccess",
                        "Cart",
                        new { invoice = orderCode }
                    )
                ),
                new KeyValuePair<string, string>(
                    "error_url",
                    publicUrl
                    + Url.Action(
                        "SePayError",
                        "Cart",
                        new { invoice = orderCode }
                    )
                ),
                new KeyValuePair<string, string>(
                    "cancel_url",
                    publicUrl
                    + Url.Action(
                        "SePayCancel",
                        "Cart",
                        new { invoice = orderCode }
                    )
                )
            };

            var signatureData =
                String.Join(
                    ",",
                    fields.Select(field =>
                        field.Key + "=" + field.Value
                    )
                );

            string signature;

            using (var hmac =
                new HMACSHA256(
                    Encoding.UTF8.GetBytes(secretKey)
                ))
            {
                signature =
                    Convert.ToBase64String(
                        hmac.ComputeHash(
                            Encoding.UTF8.GetBytes(signatureData)
                        )
                    );
            }

            var sandbox =
                String.Equals(
                    ConfigurationManager.AppSettings["SePayEnvironment"],
                    "sandbox",
                    StringComparison.OrdinalIgnoreCase
                );

            var model = new SePayCheckoutViewModel
            {
                CheckoutUrl =
                    sandbox
                        ? "https://pay-sandbox.sepay.vn/v1/checkout/init"
                        : "https://pay.sepay.vn/v1/checkout/init",
                Fields = fields,
                Signature = signature
            };

            Session[CartSession] = list;
            return View("SePayRedirect", model);
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult SePayIpn()
        {
            var expectedSecret = ConfigurationManager.AppSettings["SePaySecretKey"] ?? string.Empty;
            var receivedSecret = Request.Headers["X-Secret-Key"] ?? string.Empty;
            if (!SecureEquals(expectedSecret, receivedSecret))
            {
                Response.StatusCode = 401;
                Response.SuppressFormsAuthenticationRedirect = true;
                return Json(new { success = false, error = "Unauthorized" });
            }

            Request.InputStream.Position = 0;

            SePayIpnRequest payload;

            try
            {
                string body;

                using (var reader =
                    new System.IO.StreamReader(Request.InputStream))
                {
                    body = reader.ReadToEnd();
                }

                payload = new JavaScriptSerializer()
                    .Deserialize<SePayIpnRequest>(body);
            }
            catch
            {
                Response.StatusCode = 400;
                return Json(new
                {
                    success = false,
                    error = "Invalid payload"
                });
            }

            if (payload == null
                || !String.Equals(
                    payload.notification_type,
                    "ORDER_PAID",
                    StringComparison.OrdinalIgnoreCase)
                || payload.order == null)
            {
                return Json(new { success = true });
            }
            if (!String.Equals(
                    payload.order.order_status,
                    "CAPTURED",
                    StringComparison.OrdinalIgnoreCase)
                || !String.Equals(
                    payload.order.order_currency,
                    "VND",
                    StringComparison.OrdinalIgnoreCase))
            {
                Response.StatusCode = 400;
                return Json(new
                {
                    success = false,
                    error = "Invalid payment status"
                });
            }

            decimal paidAmount;
            if (!decimal.TryParse(payload.order.order_amount, NumberStyles.Number, CultureInfo.InvariantCulture, out paidAmount))
            {
                Response.StatusCode = 400;
                return Json(new { success = false, error = "Invalid amount" });
            }
            var completionError = CompletePaidOrder(payload.order.order_invoice_number, paidAmount);
            if (completionError != null)
            {
                Response.StatusCode = completionError == "Insufficient stock" ? 409 : 400;
                return Json(new { success = false, error = completionError });
            }
            return Json(new { success = true });
        }

        [AllowAnonymous]
        public ActionResult SePaySuccess(string invoice)
        {
            var order = new OrderDAO().GetOrder(invoice);

            if (order != null && !CanAccessOrderStatus(order, invoice))
            {
                return HttpNotFound();
            }

            if (order != null && !order.Status && TryConfirmOrderWithSePay(order))
            {
                order = new OrderDAO().GetOrder(invoice);
            }
            ViewBag.TrangThai = order != null && order.Status;
            ViewBag.Invoice = invoice;
            ViewBag.SoTien = order == null ? 0 : order.TotalAmount;
            ViewBag.InnerText = ViewBag.TrangThai ? "Thanh toán thành công. Code đã được lưu trong lịch sử mua hàng." : "SePay đang xác nhận giao dịch. Vui lòng kiểm tra lịch sử mua hàng sau ít phút.";
            if (ViewBag.TrangThai) Session[CartSession] = null;
            return View("VnpayReturn");
        }

        [AllowAnonymous]
        [HttpGet]
        public JsonResult SePayOrderStatus(string invoice)
        {
            var order =
                String.IsNullOrWhiteSpace(invoice)
                    ? null
                    : new OrderDAO().GetOrder(invoice);

            if (order != null && !CanAccessOrderStatus(order, invoice))
            {
                Response.StatusCode = 404;
                Response.TrySkipIisCustomErrors = true;

                return Json(new
                {
                    found = false,
                    paid = false
                }, JsonRequestBehavior.AllowGet);
            }

            if (order != null
                && !order.Status
                && TryConfirmOrderWithSePay(order))
            {
                order = new OrderDAO().GetOrder(invoice);
            }

            if (order != null && order.Status)
            {
                Session[CartSession] = null;
            }

            return Json(new
            {
                found = order != null,
                paid = order != null && order.Status,
                amount = order == null ? 0 : order.TotalAmount,
                historyUrl = Url.Action("OrderHistory", "Profile")
            }, JsonRequestBehavior.AllowGet);
        }

        private bool TryConfirmOrderWithSePay(Order localOrder)
        {
            try
            {
                var merchantId = ConfigurationManager.AppSettings["SePayMerchantId"];
                var secretKey = ConfigurationManager.AppSettings["SePaySecretKey"];
                var sandbox = String.Equals(ConfigurationManager.AppSettings["SePayEnvironment"], "sandbox", StringComparison.OrdinalIgnoreCase);
                var apiBase = sandbox ? "https://pgapi-sandbox.sepay.vn" : "https://pgapi.sepay.vn";
                using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(8) })
                {
                    var basic = Convert.ToBase64String(Encoding.UTF8.GetBytes(merchantId + ":" + secretKey));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basic);
                    var url = apiBase + "/v1/order?per_page=20&q=" + HttpUtility.UrlEncode(localOrder.OrderCode);
                    var response = client.GetAsync(url).GetAwaiter().GetResult();
                    if (!response.IsSuccessStatusCode) return false;
                    var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    var result = new JavaScriptSerializer().Deserialize<SePayOrderListResponse>(json);
                    var paidOrder = result == null || result.data == null ? null : result.data.FirstOrDefault(x =>
                        String.Equals(x.order_invoice_number, localOrder.OrderCode, StringComparison.OrdinalIgnoreCase) &&
                        String.Equals(x.order_status, "CAPTURED", StringComparison.OrdinalIgnoreCase) &&
                        String.Equals(x.order_currency, "VND", StringComparison.OrdinalIgnoreCase));
                    decimal paidAmount;
                    if (paidOrder == null || !decimal.TryParse(paidOrder.order_amount, NumberStyles.Number, CultureInfo.InvariantCulture, out paidAmount)) return false;
                    return CompletePaidOrder(localOrder.OrderCode, paidAmount) == null;
                }
            }
            catch
            {
                return false;
            }
        }

        private static string CompletePaidOrder(
            string invoice,
            decimal paidAmount)
        {
            using (var db = new KeyDbContext())
            using (var transaction =
                db.Database.BeginTransaction(
                    System.Data.IsolationLevel.Serializable))
            {
                var order = db.Orders
                    .Include(x => x.OrderDetail)
                    .FirstOrDefault(x => x.OrderCode == invoice);

                if (order == null || paidAmount != order.TotalAmount)
                {
                    transaction.Rollback();
                    return "Invalid order or amount";
                }

                // IPN/Success polling có thể gọi lặp.
                // Nếu đã hoàn tất thì trả thành công mà không cấp code lần hai.
                if (order.Status)
                {
                    transaction.Commit();
                    return null;
                }

                if (order.OrderDetail == null
                    || !order.OrderDetail.Any())
                {
                    transaction.Rollback();
                    return "Invalid order details";
                }

                var detailGroups = order.OrderDetail
                    .GroupBy(x => x.ProductID)
                    .ToList();

                var allocations =
                    new List<Tuple<OrderDetail, Giftcard>>();

                foreach (var group in detailGroups)
                {
                    var details = group
                        .OrderBy(x => x.Id)
                        .ToList();

                    var gifts = db.Giftcards
                        .Where(x =>
                            x.ProductId == group.Key
                            && x.Code != null
                            && x.Code != "")
                        .OrderBy(x => x.Id)
                        .Take(details.Count)
                        .ToList();

                    if (gifts.Count < details.Count)
                    {
                        transaction.Rollback();
                        return "Insufficient stock";
                    }

                    for (var index = 0;
                         index < details.Count;
                         index++)
                    {
                        allocations.Add(
                            Tuple.Create(
                                details[index],
                                gifts[index]
                            )
                        );
                    }
                }

                foreach (var allocation in allocations)
                {
                    allocation.Item1.Code =
                        allocation.Item2.Code;

                    db.Giftcards.Remove(
                        allocation.Item2
                    );
                }

                order.Status = true;
                db.SaveChanges();
                transaction.Commit();

                return null;
            }
        }

        [AllowAnonymous]
        public ActionResult SePayError(string invoice)
        {
            ViewBag.TrangThai = false;
            ViewBag.InnerText = "Thanh toán SePay không thành công. Đơn hàng " + invoice + " chưa được ghi nhận thanh toán.";
            return View("VnpayReturn");
        }

        [AllowAnonymous]
        public ActionResult SePayCancel(string invoice)
        {
            ViewBag.TrangThai = false;
            ViewBag.InnerText = "Bạn đã hủy thanh toán đơn hàng " + invoice + ".";
            return View("VnpayReturn");
        }

        private ActionResult CartFailure(
            string message,
            string redirectUrl,
            int? availableStock = null)
        {
            if (Request.IsAjaxRequest())
            {
                return Json(new
                {
                    status = false,
                    availableStock = availableStock,
                    message = message
                });
            }

            TempData["CartError"] = message;

            if (!String.IsNullOrWhiteSpace(redirectUrl)
                && Url.IsLocalUrl(redirectUrl))
            {
                return Redirect(redirectUrl);
            }

            return RedirectToAction("Index");
        }

        private List<Cart> RefreshCartFromDatabase(
            List<Cart> cart)
        {
            if (cart == null || !cart.Any())
            {
                return new List<Cart>();
            }

            var productDao = new ProductDAO();
            var refreshed = new List<Cart>();

            foreach (var item in cart)
            {
                if (item == null || item.product == null)
                {
                    continue;
                }

                var product =
                    productDao.GetProductById(item.product.Id);

                if (product == null || product.Status != true)
                {
                    continue;
                }

                var available =
                    new GiftcardDAO()
                        .GetAvailableStock(product.Id);

                if (available <= 0)
                {
                    continue;
                }

                refreshed.Add(new Cart
                {
                    product = product,
                    Quantity = Math.Min(
                        available,
                        Math.Max(
                            1,
                            Math.Min(
                                MaxCartItemQuantity,
                                item.Quantity
                            )
                        )
                    )
                });
            }

            return refreshed;
        }

        private bool CanAccessOrderStatus(
            Order order,
            string invoice)
        {
            if (order == null)
            {
                return false;
            }

            if (CurrentUser.IsAuthenticated
                && CurrentUser.UserId == order.CustomerID)
            {
                return true;
            }

            var pending =
                Convert.ToString(
                    Session["SEPAY_PENDING_ORDER"]
                );

            return !String.IsNullOrWhiteSpace(pending)
                && String.Equals(
                    pending,
                    invoice,
                    StringComparison.OrdinalIgnoreCase
                );
        }

        private string GetExternalBaseUrl()
        {
            var forwardedProto =
                Request.Headers["X-Forwarded-Proto"];

            var scheme =
                !String.IsNullOrWhiteSpace(forwardedProto)
                    ? forwardedProto
                        .Split(',')[0]
                        .Trim()
                    : Request.Url.Scheme;

            if (!String.Equals(
                    scheme,
                    "https",
                    StringComparison.OrdinalIgnoreCase)
                && !String.Equals(
                    scheme,
                    "http",
                    StringComparison.OrdinalIgnoreCase))
            {
                scheme = Request.Url.Scheme;
            }

            var forwardedHost =
                Request.Headers["X-Forwarded-Host"];

            var host =
                !String.IsNullOrWhiteSpace(forwardedHost)
                    ? forwardedHost
                        .Split(',')[0]
                        .Trim()
                    : Request.Url.Authority;

            if (host.EndsWith(
                ".trycloudflare.com",
                StringComparison.OrdinalIgnoreCase))
            {
                scheme = "https";
            }

            return (scheme + "://" + host).TrimEnd('/');
        }

        private static bool SecureEquals(string expected, string received)
        {
            var expectedBytes = Encoding.UTF8.GetBytes(expected);
            var receivedBytes = Encoding.UTF8.GetBytes(received);
            if (expectedBytes.Length == 0 || receivedBytes.Length == 0) return false;
            var difference = expectedBytes.Length ^ receivedBytes.Length;
            for (var index = 0; index < Math.Max(expectedBytes.Length, receivedBytes.Length); index++)
            {
                difference |= expectedBytes[index % expectedBytes.Length] ^ receivedBytes[index % receivedBytes.Length];
            }
            return difference == 0;
        }

        public string UrlPayment(int price, string orderCode)
        {
            //Get Config Info
            string vnp_Returnurl = ConfigurationManager.AppSettings["vnp_Returnurl"]; //URL nhan ket qua tra ve 
            string vnp_Url = ConfigurationManager.AppSettings["vnp_Url"]; //URL thanh toan cua VNPAY 
            string vnp_TmnCode = ConfigurationManager.AppSettings["vnp_TmnCode"]; //Ma định danh merchant kết nối (Terminal Id)
            string vnp_HashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"]; //Secret Key

            //Build URL for VNPAY
            VnPayLibrary vnpay = new VnPayLibrary();

            vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
            vnpay.AddRequestData("vnp_Amount", (price * 100).ToString()); //Số tiền thanh toán. Số tiền không mang các ký tự phân tách thập phân, phần nghìn, ký tự tiền tệ. Để gửi số tiền thanh toán là 100,000 VND (một trăm nghìn VNĐ) thì merchant cần nhân thêm 100 lần (khử phần thập phân), sau đó gửi sang VNPAY là: 10000000
            vnpay.AddRequestData("vnp_Locale", "vn");
            //vnpay.AddRequestData("vnp_BankCode", "VNPAYQR");
            //vnpay.AddRequestData("vnp_BankCode", "VNBANK");

            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", Utils.GetIpAddress());
            vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang:" + orderCode);
            vnpay.AddRequestData("vnp_OrderType", "other"); //default value: other

            vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
            vnpay.AddRequestData("vnp_TxnRef", orderCode.ToString()); // Mã tham chiếu của giao dịch tại hệ thống của merchant. Mã này là duy nhất dùng để phân biệt các đơn hàng gửi sang VNPAY. Không được trùng lặp trong ngày

            //Add Params of 2.1.0 Version
            //Billing

            string paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
            return paymentUrl;
        }

        [HttpGet]
        public ActionResult VnpayReturn()
        {
            if (Request.QueryString.Count > 0)
            {
                string vnp_HashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"]; //Chuoi bi mat
                var vnpayData = Request.QueryString;
                VnPayLibrary vnpay = new VnPayLibrary();

                foreach (string s in vnpayData)
                {
                    //get all querystring data
                    if (!string.IsNullOrEmpty(s) && s.StartsWith("vnp_"))
                    {
                        vnpay.AddResponseData(s, vnpayData[s]);
                    }
                }
                string orderCode = Convert.ToString(vnpay.GetResponseData("vnp_TxnRef"));
                long vnpayTranId = Convert.ToInt64(vnpay.GetResponseData("vnp_TransactionNo"));
                string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
                string vnp_TransactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");
                String vnp_SecureHash = Request.QueryString["vnp_SecureHash"];
                String TerminalID = Request.QueryString["vnp_TmnCode"];
                long vnp_Amount = Convert.ToInt64(vnpay.GetResponseData("vnp_Amount")) / 100;
                String bankCode = Request.QueryString["vnp_BankCode"];

                bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);
                if (checkSignature)
                {
                    if (vnp_ResponseCode == "00" && vnp_TransactionStatus == "00")
                    {
                        var dao = new OrderDAO();
                        var customerID = CurrentUser.UserId;

                        if (customerID <= 0)
                        {
                            ViewBag.TrangThai = false;
                            ViewBag.InnerText =
                                "Phiên đăng nhập đã hết hạn. "
                                + "Vui lòng đăng nhập lại để kiểm tra đơn hàng.";

                            return View();
                        }

                        var cart = Session["CART_SESSION"];
                        var list = new List<Cart>();
                        if (cart != null)
                        {
                            list = (List<Cart>)cart;
                        }

                        Order order = new Order();
                        order.OrderCode = orderCode;
                        order.CustomerID = customerID;
                        order.TotalAmount = (int)vnp_Amount;
                        order.Status = true;
                        order.CreatedDate = DateTime.Now;

                        foreach (var item in list)
                        {
                            for (int i = 0; i < item.Quantity; i++)
                            {
                                var newOrderDetail = new OrderDetail
                                {
                                    ProductID = item.product.Id,
                                    Price = item.product.PriceSale > 0 ? item.product.PriceSale.GetValueOrDefault(0) : item.product.Price,
                                    Quantity = item.Quantity,
                                    Code = new GiftcardDAO().TakeAndDelete(item.product.Id).Code
                                };

                                order.OrderDetail.Add(newOrderDetail);
                            }

                        }

                        dao.AddOrder(order);

                        //list.ToList().ForEach(x => order.OrderDetail.Add(new OrderDetail
                        //{

                        //    ProductID = x.product.Id,
                        //    Price = x.product.PriceSale > 0 ? x.product.PriceSale.GetValueOrDefault(0) : x.product.Price,
                        //    Quantity = x.Quantity,
                        //    Code = new GiftcardDAO().TakeAndDelete(x.product.Id).Code
                        //}));
                        Session["CART_SESSION"] = null;

                        //var itemOrder = db.Orders.FirstOrDefault(x => x.Code == orderCode);
                        //if (itemOrder != null)
                        //{
                        //    itemOrder.Status = 2;//đã thanh toán
                        //    db.Orders.Attach(itemOrder);
                        //    db.Entry(itemOrder).State = System.Data.Entity.EntityState.Modified;
                        //    db.SaveChanges();
                        //}

                        //Thanh toan thanh cong
                        ViewBag.InnerText = "Giao dịch được thực hiện thành công. Cảm ơn quý khách đã sử dụng dịch vụ";
                        ViewBag.SoTien = vnp_Amount.ToString();
                        ViewBag.TrangThai = true;
                        //log.InfoFormat("Thanh toan thanh cong, OrderId={0}, VNPAY TranId={1}", orderId, vnpayTranId);
                    }
                    else
                    {
                        //Thanh toan khong thanh cong. Ma loi: vnp_ResponseCode
                        ViewBag.TrangThai = false;
                        ViewBag.InnerText = "Có lỗi xảy ra trong quá trình xử lý. Mã lỗi: " + vnp_ResponseCode;
                        //log.InfoFormat("Thanh toan loi, OrderId={0}, VNPAY TranId={1},ResponseCode={2}", orderId, vnpayTranId, vnp_ResponseCode);
                    }
                    //displayTmnCode.InnerText = "Mã Website (Terminal ID):" + TerminalID;
                    //displayTxnRef.InnerText = "Mã giao dịch thanh toán:" + orderId.ToString();
                    //displayVnpayTranNo.InnerText = "Mã giao dịch tại VNPAY:" + vnpayTranId.ToString();

                    //displayBankCode.InnerText = "Ngân hàng thanh toán:" + bankCode;
                }
            }
            return View();
        }

        private string CreateVietQrUrl(decimal amount, string orderCode)
        {
            string bankBin =
                ConfigurationManager.AppSettings["BankBin"];

            string accountNumber =
                ConfigurationManager.AppSettings["BankAccountNo"];

            string accountName =
                ConfigurationManager.AppSettings["BankAccountName"];

            string encodedOrderCode =
                HttpUtility.UrlEncode(orderCode);

            string encodedAccountName =
                HttpUtility.UrlEncode(accountName);

            string amountText = decimal
                .Truncate(amount)
                .ToString(CultureInfo.InvariantCulture);

            return string.Format(
                "https://img.vietqr.io/image/{0}-{1}-compact2.png" +
                "?amount={2}&addInfo={3}&accountName={4}",
                bankBin,
                accountNumber,
                amountText,
                encodedOrderCode,
                encodedAccountName
            );
        }


public ActionResult BankTransfer(int orderId)
        {
            var order = new OrderDAO().GetById(orderId);

            if (order == null)
            {
                return HttpNotFound();
            }

            var model = new BankTransferViewModel
            {
                OrderId = order.Id,
                OrderCode = order.OrderCode,
                Amount = order.TotalAmount,

                BankName =
                    ConfigurationManager.AppSettings["BankName"],

                AccountNumber =
                    ConfigurationManager.AppSettings["BankAccountNo"],

                AccountName =
                    ConfigurationManager.AppSettings["BankAccountName"],

                QrUrl = CreateVietQrUrl(
                    order.TotalAmount,
                    order.OrderCode
                )
            };

            return View(model);
        }

    } }

