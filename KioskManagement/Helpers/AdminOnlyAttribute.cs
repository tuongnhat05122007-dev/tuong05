
using System.Web.Mvc;

namespace KioskManagement.Helpers
{
    public class AdminOnlyAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(
            AuthorizationContext filterContext)
        {
            var session = filterContext.HttpContext.Session;

            // Kiem tra da dang nhap chua
            if (session["UserId"] == null)
            {
                filterContext.Result =
                    new RedirectResult("~/Login/Index");
                return;
            }

            // Kiem tra quyen Admin
            string role = session["Role"] as string;

            if (role != "Admin")
            {
                filterContext.Result =
                    new HttpStatusCodeResult(403,
                    "Ban khong co quyen truy cap chuc nang nay!");
            }
        }
    }
}
