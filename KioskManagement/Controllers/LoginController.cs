
using System.Linq;
using System.Web.Mvc;
using KioskManagement.Models;
using KioskManagement.Helpers;

namespace KioskManagement.Controllers
{
    public class LoginController : Controller
    {
        private KioskManagementDBEntities db =
            new KioskManagementDBEntities();

        // HIEN THI TRANG DANG NHAP
        [HttpGet]
        public ActionResult Index()
        {
            if (Session["UserId"] != null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        // XU LY DANG NHAP
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username)
                || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Vui long nhap ten dang nhap va mat khau!";
                return View();
            }

            var user = db.Users.FirstOrDefault(
                u => u.Username == username
            );

            if (user == null || !user.IsActive)
            {
                ViewBag.Error = "Tai khoan khong ton tai hoac da bi khoa!";
                return View();
            }

            // KIEM TRA MAT KHAU
            bool validPassword =
                PasswordHelper.VerifyPassword(
                    password,
                    user.PasswordHash
                );

            if (!validPassword)
            {
                ViewBag.Error = "Mat khau khong chinh xac!";
                return View();
            }
            // CHI CHO PHEP ADMIN VA STAFF
            if (user.Role != "Admin" && user.Role != "Staff")
            {
                ViewBag.Error = "Tai khoan khong co quyen truy cap!";
                return View();
            }
            // NEU MAT KHAU CU CHUA MA HOA THI CAP NHAT
            if (!PasswordHelper.IsHashed(user.PasswordHash))
            {
                user.PasswordHash =
                    PasswordHelper.HashPassword(password);

                db.SaveChanges();
            }

            // LUU THONG TIN NGUOI DUNG VAO SESSION
            Session.Clear();

            Session["UserId"] = user.UserId;
            Session["Username"] = user.Username;
            Session["FullName"] = user.FullName;
            Session["Role"] = user.Role;

            return RedirectToAction("Index", "Home");
        }

        // DANG XUAT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();

            return RedirectToAction("Index", "Login");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
