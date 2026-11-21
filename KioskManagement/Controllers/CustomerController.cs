
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using KioskManagement.Models;
using KioskManagement.Helpers;

namespace KioskManagement.Controllers
{
    [AuthorizeSession]
    public class CustomerController : Controller
    {
        private KioskManagementDBEntities db = new KioskManagementDBEntities();

        // 1. DANH SACH KHACH HANG
        public ActionResult Index(string search)
        {
            var customers = db.Customers.AsQueryable();

            if (!String.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                customers = customers.Where(c =>
                    c.FullName.Contains(search) ||
                    c.Phone.Contains(search) ||
                    (c.Email != null && c.Email.Contains(search)));
            }

            ViewBag.Search = search;

            return View(customers
                .OrderByDescending(c => c.CreatedAt)
                .ToList());
        }

        // 2. CHI TIET KHACH HANG
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Customer customer = db.Customers
                .Include(c => c.Contracts)
                .Include(c => c.MaintenanceRequests)
                .FirstOrDefault(c => c.CustomerId == id);

            if (customer == null)
                return HttpNotFound();

            return View(customer);
        }

        // 3. HIEN THI FORM THEM KHACH HANG
        public ActionResult Create()
        {
            return View();
        }

        // 4. LUU KHACH HANG MOI
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            [Bind(Include = "FullName,Phone,Email,Address")] Customer customer)
        {
            customer.FullName = customer.FullName == null
                ? null : customer.FullName.Trim();

            customer.Phone = customer.Phone == null
                ? null : customer.Phone.Trim();

            customer.Email = String.IsNullOrWhiteSpace(customer.Email)
                ? null : customer.Email.Trim();

            if (String.IsNullOrWhiteSpace(customer.FullName))
                ModelState.AddModelError("FullName", "Vui lòng nhập họ tên.");

            if (String.IsNullOrWhiteSpace(customer.Phone))
                ModelState.AddModelError("Phone", "Vui lòng nhập số điện thoại.");

            if (!String.IsNullOrWhiteSpace(customer.Phone) &&
                db.Customers.Any(c => c.Phone == customer.Phone))
            {
                ModelState.AddModelError("Phone", "Số điện thoại đã tồn tại.");
            }

            if (!String.IsNullOrWhiteSpace(customer.Email) &&
                db.Customers.Any(c => c.Email == customer.Email))
            {
                ModelState.AddModelError("Email", "Email đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                customer.CreatedAt = DateTime.Now;

                db.Customers.Add(customer);
                db.SaveChanges();

                TempData["Success"] = "Thêm khách hàng thành công!";
                return RedirectToAction("Index");
            }

            return View(customer);
        }

        // 5. HIEN THI FORM CHINH SUA
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Customer customer = db.Customers.Find(id);

            if (customer == null)
                return HttpNotFound();

            return View(customer);
        }

        // 6. LUU THONG TIN CHINH SUA
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            [Bind(Include = "CustomerId,FullName,Phone,Email,Address")] Customer input)
        {
            Customer customer = db.Customers.Find(input.CustomerId);

            if (customer == null)
                return HttpNotFound();

            input.FullName = input.FullName == null
                ? null : input.FullName.Trim();

            input.Phone = input.Phone == null
                ? null : input.Phone.Trim();

            input.Email = String.IsNullOrWhiteSpace(input.Email)
                ? null : input.Email.Trim();

            if (String.IsNullOrWhiteSpace(input.FullName))
                ModelState.AddModelError("FullName", "Vui lòng nhập họ tên.");

            if (String.IsNullOrWhiteSpace(input.Phone))
                ModelState.AddModelError("Phone", "Vui lòng nhập số điện thoại.");

            if (!String.IsNullOrWhiteSpace(input.Phone) &&
                db.Customers.Any(c =>
                    c.Phone == input.Phone &&
                    c.CustomerId != input.CustomerId))
            {
                ModelState.AddModelError("Phone", "Số điện thoại đã tồn tại.");
            }

            if (!String.IsNullOrWhiteSpace(input.Email) &&
                db.Customers.Any(c =>
                    c.Email == input.Email &&
                    c.CustomerId != input.CustomerId))
            {
                ModelState.AddModelError("Email", "Email đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                customer.FullName = input.FullName;
                customer.Phone = input.Phone;
                customer.Email = input.Email;
                customer.Address = input.Address;

                // Khong thay doi CreatedAt khi chinh sua
                db.SaveChanges();

                TempData["Success"] = "Cập nhật khách hàng thành công!";
                return RedirectToAction("Index");
            }

            input.CreatedAt = customer.CreatedAt;
            return View(input);
        }

        // 7. HIEN THI XAC NHAN XOA
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Customer customer = db.Customers
                .Include(c => c.Contracts)
                .Include(c => c.MaintenanceRequests)
                .FirstOrDefault(c => c.CustomerId == id);

            if (customer == null)
                return HttpNotFound();

            return View(customer);
        }

        // 8. XOA KHACH HANG
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Customer customer = db.Customers.Find(id);

            if (customer == null)
                return HttpNotFound();

            bool hasContracts = db.Contracts.Any(c => c.CustomerId == id);
            bool hasRequests = db.MaintenanceRequests.Any(m => m.CustomerId == id);

            if (hasContracts || hasRequests)
            {
                TempData["Error"] =
                    "Không thể xóa khách hàng vì đang có hợp đồng hoặc yêu cầu bảo trì liên quan.";

                return RedirectToAction("Index");
            }

            db.Customers.Remove(customer);
            db.SaveChanges();

            TempData["Success"] = "Xóa khách hàng thành công!";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();

            base.Dispose(disposing);
        }
    }
}
