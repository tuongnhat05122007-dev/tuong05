
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
    public class MaintenanceRequestController : Controller
    {
        private KioskManagementDBEntities db =
            new KioskManagementDBEntities();

        // 1. DANH SACH YEU CAU BAO TRI
        public ActionResult Index(string search, string status)
        {
            var requests = db.MaintenanceRequests
                .Include(r => r.Customer)
                .Include(r => r.Kiosk)
                .AsQueryable();

            if (!String.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                requests = requests.Where(r =>
                    r.Customer.FullName.Contains(search) ||
                    r.Kiosk.KioskName.Contains(search) ||
                    r.Description.Contains(search));
            }

            if (!String.IsNullOrWhiteSpace(status))
            {
                requests = requests.Where(r => r.Status == status);
            }

            ViewBag.Search = search;
            ViewBag.Status = status;

            LoadStatuses(status);

            return View(requests
                .OrderByDescending(r => r.RequestDate)
                .ToList());
        }

        // 2. CHI TIET YEU CAU
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            MaintenanceRequest request = db.MaintenanceRequests
                .Include(r => r.Customer)
                .Include(r => r.Kiosk)
                .FirstOrDefault(r => r.RequestId == id);

            if (request == null)
                return HttpNotFound();

            return View(request);
        }

        // 3. FORM THEM YEU CAU
        public ActionResult Create()
        {
            LoadDropdowns();

            MaintenanceRequest request = new MaintenanceRequest();
            request.RequestDate = DateTime.Now;
            request.Status = "Pending";

            return View(request);
        }

        // 4. LUU YEU CAU MOI
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            [Bind(Include = "KioskId,CustomerId,RequestDate,Description,Status")]
            MaintenanceRequest request)
        {
            ValidateRequest(request);

            if (ModelState.IsValid)
            {
                db.MaintenanceRequests.Add(request);
                db.SaveChanges();

                TempData["Success"] =
                    "Thêm yêu cầu bảo trì thành công!";

                return RedirectToAction("Index");
            }

            LoadDropdowns(request.KioskId, request.CustomerId);
            return View(request);
        }

        // 5. FORM CHINH SUA
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            MaintenanceRequest request =
                db.MaintenanceRequests.Find(id);

            if (request == null)
                return HttpNotFound();

            LoadDropdowns(request.KioskId, request.CustomerId);

            return View(request);
        }

        // 6. LUU THAY DOI
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            [Bind(Include = "RequestId,KioskId,CustomerId,RequestDate,Description,Status")]
            MaintenanceRequest input)
        {
            MaintenanceRequest request =
                db.MaintenanceRequests.Find(input.RequestId);

            if (request == null)
                return HttpNotFound();

            ValidateRequest(input);

            if (ModelState.IsValid)
            {
                request.KioskId = input.KioskId;
                request.CustomerId = input.CustomerId;
                request.RequestDate = input.RequestDate;
                request.Description = input.Description;
                request.Status = input.Status;

                db.SaveChanges();

                TempData["Success"] =
                    "Cập nhật yêu cầu bảo trì thành công!";

                return RedirectToAction("Index");
            }

            LoadDropdowns(input.KioskId, input.CustomerId);
            return View(input);
        }

        // 7. FORM XAC NHAN XOA
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            MaintenanceRequest request = db.MaintenanceRequests
                .Include(r => r.Customer)
                .Include(r => r.Kiosk)
                .FirstOrDefault(r => r.RequestId == id);

            if (request == null)
                return HttpNotFound();

            return View(request);
        }

        // 8. XOA YEU CAU
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            MaintenanceRequest request =
                db.MaintenanceRequests.Find(id);

            if (request == null)
                return HttpNotFound();

            db.MaintenanceRequests.Remove(request);
            db.SaveChanges();

            TempData["Success"] =
                "Xóa yêu cầu bảo trì thành công!";

            return RedirectToAction("Index");
        }

        // KIEM TRA DU LIEU
        private void ValidateRequest(MaintenanceRequest request)
        {
            if (!db.Kiosks.Any(k => k.KioskId == request.KioskId))
            {
                ModelState.AddModelError(
                    "KioskId",
                    "Vui lòng chọn kiosk hợp lệ.");
            }

            if (!db.Customers.Any(c => c.CustomerId == request.CustomerId))
            {
                ModelState.AddModelError(
                    "CustomerId",
                    "Vui lòng chọn khách hàng hợp lệ.");
            }

            if (String.IsNullOrWhiteSpace(request.Description))
            {
                ModelState.AddModelError(
                    "Description",
                    "Vui lòng nhập nội dung yêu cầu.");
            }

            string[] statuses =
            {
                "Pending",
                "InProgress",
                "Completed",
                "Cancelled"
            };

            if (!statuses.Contains(request.Status))
            {
                ModelState.AddModelError(
                    "Status",
                    "Trạng thái không hợp lệ.");
            }
        }

        // NAP DANH SACH KIOSK VA KHACH HANG
        private void LoadDropdowns(
            int? kioskId = null,
            int? customerId = null)
        {
            ViewBag.KioskId = new SelectList(
                db.Kiosks.OrderBy(k => k.KioskName).ToList(),
                "KioskId",
                "KioskName",
                kioskId);

            ViewBag.CustomerId = new SelectList(
                db.Customers.OrderBy(c => c.FullName).ToList(),
                "CustomerId",
                "FullName",
                customerId);
        }

        // NAP TRANG THAI
        private void LoadStatuses(string selected = null)
        {
            ViewBag.StatusList = new SelectList(
                new[]
                {
                    new { Value = "Pending", Text = "Đang chờ" },
                    new { Value = "InProgress", Text = "Đang xử lý" },
                    new { Value = "Completed", Text = "Hoàn thành" },
                    new { Value = "Cancelled", Text = "Đã hủy" }
                },
                "Value",
                "Text",
                selected);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();

            base.Dispose(disposing);
        }
    }
}
