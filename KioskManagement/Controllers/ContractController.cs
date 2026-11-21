
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
    public class ContractController : Controller
    {
        private KioskManagementDBEntities db = new KioskManagementDBEntities();

        // 1. DANH SACH HOP DONG
        public ActionResult Index(string search, string status)
        {
            var contracts = db.Contracts
                .Include(c => c.Customer)
                .Include(c => c.Kiosk)
                .AsQueryable();

            if (!String.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                contracts = contracts.Where(c =>
                    c.Customer.FullName.Contains(search) ||
                    c.Kiosk.KioskName.Contains(search));
            }

            if (!String.IsNullOrWhiteSpace(status))
            {
                contracts = contracts.Where(c => c.Status == status);
            }

            ViewBag.Search = search;
            ViewBag.Status = status;
            LoadStatuses();

            return View(contracts
                .OrderByDescending(c => c.StartDate)
                .ToList());
        }

        // 2. CHI TIET HOP DONG
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Contract contract = db.Contracts
                .Include(c => c.Customer)
                .Include(c => c.Kiosk)
                .Include(c => c.Payments)
                .FirstOrDefault(c => c.ContractId == id);

            if (contract == null)
                return HttpNotFound();

            return View(contract);
        }

        // 3. FORM THEM HOP DONG
        public ActionResult Create()
        {
            LoadDropdowns();
            return View();
        }

        // 4. LUU HOP DONG MOI
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
     [Bind(Include = "CustomerId,KioskId,StartDate,EndDate,RentalPrice,Deposit,Status")] Contract contract)
        {
            ValidateContract(contract);

            // Kiem tra Kiosk co the duoc thue hay khong
            if (contract.Status == "Active")
            {
                var kiosk = db.Kiosks.Find(contract.KioskId);

                if (kiosk != null &&
                    (kiosk.Status == "Maintenance" ||
                     kiosk.Status == "Inactive"))
                {
                    ModelState.AddModelError("KioskId",
                        "Kiosk đang bảo trì hoặc ngừng hoạt động, không thể cho thuê.");
                }

                bool hasActiveContract = db.Contracts.Any(c =>
                    c.KioskId == contract.KioskId &&
                    c.Status == "Active");

                if (hasActiveContract)
                {
                    ModelState.AddModelError("KioskId",
                        "Kiosk này đã có hợp đồng đang hoạt động.");
                }
            }

            if (ModelState.IsValid)
            {
                db.Contracts.Add(contract);
                db.SaveChanges();

                // Dong bo trang thai Kiosk
                UpdateKioskStatus(contract.KioskId);
                db.SaveChanges();

                TempData["Success"] = "Thêm hợp đồng thành công!";
                return RedirectToAction("Index");
            }

            LoadDropdowns(contract.CustomerId, contract.KioskId);
            return View(contract);
        }

        // 5. FORM CHINH SUA HOP DONG
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Contract contract = db.Contracts.Find(id);

            if (contract == null)
                return HttpNotFound();

            LoadDropdowns(contract.CustomerId, contract.KioskId);
            return View(contract);
        }

        // 6. LUU THAY DOI HOP DONG
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
    [Bind(Include = "ContractId,CustomerId,KioskId,StartDate,EndDate,RentalPrice,Deposit,Status")] Contract input)
        {
            Contract contract = db.Contracts.Find(input.ContractId);

            if (contract == null)
                return HttpNotFound();

            // Luu lai Kiosk cu
            int oldKioskId = contract.KioskId;

            ValidateContract(input);

            // Kiem tra Kiosk co the duoc thue hay khong
            if (input.Status == "Active")
            {
                var kiosk = db.Kiosks.Find(input.KioskId);

                if (kiosk != null &&
                    (kiosk.Status == "Maintenance" ||
                     kiosk.Status == "Inactive"))
                {
                    ModelState.AddModelError("KioskId",
                        "Kiosk đang bảo trì hoặc ngừng hoạt động, không thể cho thuê.");
                }

                bool hasActiveContract = db.Contracts.Any(c =>
                    c.KioskId == input.KioskId &&
                    c.Status == "Active" &&
                    c.ContractId != input.ContractId);

                if (hasActiveContract)
                {
                    ModelState.AddModelError("KioskId",
                        "Kiosk này đã có hợp đồng đang hoạt động.");
                }
            }

            if (ModelState.IsValid)
            {
                contract.CustomerId = input.CustomerId;
                contract.KioskId = input.KioskId;
                contract.StartDate = input.StartDate;
                contract.EndDate = input.EndDate;
                contract.RentalPrice = input.RentalPrice;
                contract.Deposit = input.Deposit;
                contract.Status = input.Status;

                db.SaveChanges();

                // Dong bo Kiosk cu
                UpdateKioskStatus(oldKioskId);

                // Dong bo Kiosk moi
                if (oldKioskId != contract.KioskId)
                {
                    UpdateKioskStatus(contract.KioskId);
                }

                db.SaveChanges();

                TempData["Success"] = "Cập nhật hợp đồng thành công!";
                return RedirectToAction("Index");
            }

            LoadDropdowns(input.CustomerId, input.KioskId);
            return View(input);
        }

        // 7. FORM XAC NHAN XOA
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Contract contract = db.Contracts
                .Include(c => c.Customer)
                .Include(c => c.Kiosk)
                .Include(c => c.Payments)
                .FirstOrDefault(c => c.ContractId == id);

            if (contract == null)
                return HttpNotFound();

            return View(contract);
        }

        // 8. XOA HOP DONG

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Contract contract = db.Contracts
                .Include(c => c.Payments)
                .FirstOrDefault(c => c.ContractId == id);

            if (contract == null)
                return HttpNotFound();

            if (contract.Payments.Any())
            {
                TempData["Error"] =
                    "Không thể xóa hợp đồng vì đã có dữ liệu thanh toán liên quan.";

                return RedirectToAction("Index");
            }

            // Luu lai ma Kiosk truoc khi xoa hop dong
            int kioskId = contract.KioskId;

            // Xoa hop dong
            db.Contracts.Remove(contract);
            db.SaveChanges();

            // Dong bo lai trang thai Kiosk
            UpdateKioskStatus(kioskId);
            db.SaveChanges();

            TempData["Success"] = "Xóa hợp đồng thành công!";

            return RedirectToAction("Index");
        }


        // =====================================
        // DONG BO TRANG THAI KIOSK
        // =====================================

        private void UpdateKioskStatus(int kioskId)
        {
            var kiosk = db.Kiosks.Find(kioskId);

            if (kiosk == null)
                return;

            // Khong thay doi Kiosk dang bao tri hoac ngung hoat dong
            if (kiosk.Status == "Maintenance" ||
                kiosk.Status == "Inactive")
                return;

            // Kiem tra co hop dong dang hoat dong khong
            bool hasActiveContract = db.Contracts.Any(c =>
                c.KioskId == kioskId &&
                c.Status == "Active");

            if (hasActiveContract)
            {
                kiosk.Status = "Rented";
            }
            else
            {
                kiosk.Status = "Available";
            }
        }

        // KIEM TRA DU LIEU HOP DONG
        private void ValidateContract(Contract contract)
        {
            if (!db.Customers.Any(c => c.CustomerId == contract.CustomerId))
            {
                ModelState.AddModelError("CustomerId",
                    "Vui lòng chọn khách hàng hợp lệ.");
            }

            if (!db.Kiosks.Any(k => k.KioskId == contract.KioskId))
            {
                ModelState.AddModelError("KioskId",
                    "Vui lòng chọn Kios hợp lệ.");
            }

            if (contract.EndDate <= contract.StartDate)
            {
                ModelState.AddModelError("EndDate",
                    "Ngày kết thúc phải sau ngày bắt đầu.");
            }

            if (contract.RentalPrice <= 0)
            {
                ModelState.AddModelError("RentalPrice",
                    "Giá thuê phải lớn hơn 0.");
            }

            if (contract.Deposit < 0)
            {
                ModelState.AddModelError("Deposit",
                    "Tiền đặt cọc không được âm.");
            }

            string[] statuses =
            {
                "Pending",
                "Active",
                "Completed",
                "Cancelled"
            };

            if (!statuses.Contains(contract.Status))
            {
                ModelState.AddModelError("Status",
                    "Trạng thái hợp đồng không hợp lệ.");
            }
        }

        // NAP DANH SACH KHACH HANG VA KIOS
        private void LoadDropdowns(int? customerId = null, int? kioskId = null)
        {
            ViewBag.CustomerId = new SelectList(
                db.Customers.OrderBy(c => c.FullName).ToList(),
                "CustomerId",
                "FullName",
                customerId);

            ViewBag.KioskId = new SelectList(
                db.Kiosks.OrderBy(k => k.KioskName).ToList(),
                "KioskId",
                "KioskName",
                kioskId);
        }

        // NAP TRANG THAI HOP DONG
        private void LoadStatuses()
        {
            ViewBag.StatusList = new SelectList(
                new[]
                {
                    new { Value = "Pending", Text = "Chờ xác nhận" },
                    new { Value = "Active", Text = "Đang hiệu lực" },
                    new { Value = "Completed", Text = "Đã hoàn thành" },
                    new { Value = "Cancelled", Text = "Đã hủy" }
                },
                "Value",
                "Text",
                ViewBag.Status);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();

            base.Dispose(disposing);
        }
    }
}
