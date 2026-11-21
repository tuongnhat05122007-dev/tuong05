
using System;
using System.Linq;
using System.Web.Mvc;
using KioskManagement.Models;
using KioskManagement.Helpers;

namespace KioskManagement.Controllers
{
    [AdminOnly]
    public class UserController : Controller
    {
        private KioskManagementDBEntities db =
            new KioskManagementDBEntities();

        // 1. DANH SACH TAI KHOAN
        public ActionResult Index(string search)
        {
            var users = db.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                users = users.Where(u =>
                    u.Username.Contains(search) ||
                    u.FullName.Contains(search) ||
                    u.Email.Contains(search) ||
                    u.Role.Contains(search));
            }

            return View(users
                .OrderBy(u => u.UserId)
                .ToList());
        }

        // 2. HIEN THI FORM THEM TAI KHOAN
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        // 3. XU LY THEM TAI KHOAN
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(User user)
        {
            if (string.IsNullOrWhiteSpace(user.Username) ||
                string.IsNullOrWhiteSpace(user.PasswordHash) ||
                string.IsNullOrWhiteSpace(user.FullName) ||
                string.IsNullOrWhiteSpace(user.Email))
            {
                ModelState.AddModelError("",
                    "Vui lòng nhập đầy đủ thông tin bắt buộc!");
            }

            if (user.Role != "Admin" && user.Role != "Staff")
            {
                ModelState.AddModelError("Role",
                    "Vai trò chỉ được chọn Admin hoặc Staff!");
            }

            if (db.Users.Any(u => u.Username == user.Username))
            {
                ModelState.AddModelError("Username",
                    "Tên đăng nhập đã tồn tại!");
            }

            if (db.Users.Any(u => u.Email == user.Email))
            {
                ModelState.AddModelError("Email",
                    "Email đã tồn tại!");
            }

            if (!ModelState.IsValid)
            {
                return View(user);
            }

            user.PasswordHash =
                PasswordHelper.HashPassword(user.PasswordHash);

            user.CreatedAt = DateTime.Now;
            user.IsActive = true;

            db.Users.Add(user);
            db.SaveChanges();

            TempData["Success"] = "Thêm tài khoản thành công!";

            return RedirectToAction("Index");
        }

        // 4. HIEN THI FORM SUA TAI KHOAN
        [HttpGet]
        public ActionResult Edit(int id)
        {
            var user = db.Users.Find(id);

            if (user == null)
            {
                return HttpNotFound();
            }

            return View(user);
        }

        // 5. XU LY CAP NHAT TAI KHOAN
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(User model)
        {
            var user = db.Users.Find(model.UserId);

            if (user == null)
            {
                return HttpNotFound();
            }
            // Bo qua cac truong khong duoc gui tu form Edit
            ModelState.Remove("Username");
            ModelState.Remove("PasswordHash");
            ModelState.Remove("CreatedAt");
            ModelState.Remove("IsActive");

            if (string.IsNullOrWhiteSpace(model.FullName) ||
                string.IsNullOrWhiteSpace(model.Email))
            {
                ModelState.AddModelError("",
                    "Vui lòng nhập đầy đủ họ tên và email!");
            }

            if (model.Role != "Admin" && model.Role != "Staff")
            {
                ModelState.AddModelError("Role",
                    "Vai trò không hợp lệ!");
            }

            if (db.Users.Any(u =>
                u.Email == model.Email &&
                u.UserId != model.UserId))
            {
                ModelState.AddModelError("Email",
                    "Email đã được sử dụng!");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.Phone = model.Phone;
            user.Role = model.Role;

            db.SaveChanges();

            TempData["Success"] =
                "Cập nhật tài khoản thành công!";

            return RedirectToAction("Index");
        }

        // 6. KHOA TAI KHOAN
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Lock(int id)
        {
            var user = db.Users.Find(id);

            if (user == null)
            {
                return HttpNotFound();
            }

            // Khong cho Admin tu khoa tai khoan cua minh
            int currentUserId = Convert.ToInt32(Session["UserId"]);

            if (user.UserId == currentUserId)
            {
                TempData["Error"] =
                    "Bạn không thể khóa tài khoản đang đăng nhập!";
                return RedirectToAction("Index");
            }

            user.IsActive = false;
            db.SaveChanges();

            TempData["Success"] =
                "Khóa tài khoản thành công!";

            return RedirectToAction("Index");
        }

        // 7. MO KHOA TAI KHOAN
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Unlock(int id)
        {
            var user = db.Users.Find(id);

            if (user == null)
            {
                return HttpNotFound();
            }

            user.IsActive = true;
            db.SaveChanges();

            TempData["Success"] =
                "Mở khóa tài khoản thành công!";

            return RedirectToAction("Index");
        }


        // 6. GIAI PHONG TAI NGUYEN
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
