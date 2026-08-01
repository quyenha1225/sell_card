using BBEcom.Common.Auth;
using Models.DAO;
using System.Web.Mvc;

namespace BBEcom.Controllers
{
    public class ProfileController : MemberBaseController
    {
        public ActionResult Index()
        {
            return View(new AccountDAO()
                .GetAccountById(CurrentUser.UserId));
        }

        [ChildActionOnly]
        public ActionResult ProfileMenu()
        {
            return PartialView();
        }

        public ActionResult OrderHistory()
        {
            return View(new OrderDAO()
                .GetListOrderByCustomerID(CurrentUser.UserId));
        }
    }
}
