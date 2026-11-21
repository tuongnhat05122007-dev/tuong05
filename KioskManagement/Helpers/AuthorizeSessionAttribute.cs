
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KioskManagement.Helpers
{
    public class AuthorizeSessionAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(
            AuthorizationContext filterContext)
        {
            var session = filterContext.HttpContext.Session;

            // KIEM TRA DANG NHAP
            if (session["UserId"] == null)
            {
                filterContext.Result = new RedirectResult(
                    "~/Login/Index"
                );

                return;
            }

            // KIEM TRA QUYEN TRUY CAP
            string role = session["Role"] as string;

            if (string.IsNullOrEmpty(role))
            {
                filterContext.Result = new HttpStatusCodeResult(403);
                return;
            }

            // CHI CHO PHEP ADMIN VA STAFF
            if (role != "Admin" && role != "Staff")
            {
                filterContext.Result = new HttpStatusCodeResult(403);
            }
        }
    }
}
