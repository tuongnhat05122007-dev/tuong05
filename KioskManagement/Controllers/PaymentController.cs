
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
    public class PaymentController : Controller
    {
        private KioskManagementDBEntities db =
            new KioskManagementDBEntities();

        // 1. DANH SACH THANH TOAN
        public ActionResult Index(string search, string status)
        {
            var payments = db.Payments
                .Include(p => p.Contract.Customer)
                .Include(p => p.Contract.Kiosk)
                .AsQueryable();

            if (!String.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                payments = payments.Where(p =>
                    p.Contract.Customer.FullName.Contains(search) ||
                    p.Contract.Kiosk.KioskName.Contains(search));
            }

            if (!String.IsNullOrWhiteSpace(status))
            {
                payments = payments.Where(p => p.Status == status);
            }

            ViewBag.Search = search;
            ViewBag.Status = status;

            LoadStatuses();

            return View(payments
                .OrderByDescending(p => p.PaymentDate)
                .ToList());
        }

        // 2. CHI TIET THANH TOAN
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Payment payment = db.Payments
                .Include(p => p.Contract.Customer)
                .Include(p => p.Contract.Kiosk)
                .FirstOrDefault(p => p.PaymentId == id);

            if (payment == null)
                return HttpNotFound();

            return View(payment);
        }

        // 3. FORM THEM THANH TOAN
        public ActionResult Create()
        {
            LoadDropdowns();
            LoadMethods();
            LoadPaymentStatuses();

            Payment payment = new Payment();
            payment.PaymentDate = DateTime.Now;
            payment.Status = "Pending";
            payment.PaymentMethod = "Cash";

            return View(payment);
        }

        // 4. LUU THANH TOAN MOI
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            [Bind(Include = "ContractId,PaymentDate,Amount,PaymentMethod,Status")] Payment payment)
        {
            ValidatePayment(payment);

            if (ModelState.IsValid)
            {
                db.Payments.Add(payment);
                db.SaveChanges();

                TempData["Success"] = "Thêm thanh toán thành công!";
                return RedirectToAction("Index");
            }

            LoadDropdowns(payment.ContractId);
            LoadMethods(payment.PaymentMethod);
            LoadPaymentStatuses(payment.Status);

            return View(payment);
        }

        // 5. FORM CHINH SUA
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Payment payment = db.Payments.Find(id);

            if (payment == null)
                return HttpNotFound();

            LoadDropdowns(payment.ContractId);
            LoadMethods(payment.PaymentMethod);
            LoadPaymentStatuses(payment.Status);

            return View(payment);
        }

        // 6. LUU THAY DOI
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            [Bind(Include = "PaymentId,ContractId,PaymentDate,Amount,PaymentMethod,Status")] Payment input)
        {
            Payment payment = db.Payments.Find(input.PaymentId);

            if (payment == null)
                return HttpNotFound();

            ValidatePayment(input);

            if (ModelState.IsValid)
            {
                payment.ContractId = input.ContractId;
                payment.PaymentDate = input.PaymentDate;
                payment.Amount = input.Amount;
                payment.PaymentMethod = input.PaymentMethod;
                payment.Status = input.Status;

                db.SaveChanges();

                TempData["Success"] = "Cập nhật thanh toán thành công!";
                return RedirectToAction("Index");
            }

            LoadDropdowns(input.ContractId);
            LoadMethods(input.PaymentMethod);
            LoadPaymentStatuses(input.Status);

            return View(input);
        }

        // 7. FORM XAC NHAN XOA
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Payment payment = db.Payments
                .Include(p => p.Contract.Customer)
                .Include(p => p.Contract.Kiosk)
                .FirstOrDefault(p => p.PaymentId == id);

            if (payment == null)
                return HttpNotFound();

            return View(payment);
        }

        // 8. XOA THANH TOAN
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Payment payment = db.Payments.Find(id);

            if (payment == null)
                return HttpNotFound();

            db.Payments.Remove(payment);
            db.SaveChanges();

            TempData["Success"] = "Xóa thanh toán thành công!";
            return RedirectToAction("Index");
        }


        // 9. KIEM TRA DU LIEU
        private void ValidatePayment(Payment payment)
        {
            // KIEM TRA HOP DONG
            bool contractExists = db.Contracts.Any(c =>
                c.ContractId == payment.ContractId);

            if (!contractExists)
            {
                ModelState.AddModelError("ContractId",
                    "Vui lòng chọn hợp đồng hợp lệ.");
            }

            // KIEM TRA SO TIEN
            if (payment.Amount <= 0)
            {
                ModelState.AddModelError("Amount",
                    "Số tiền thanh toán phải lớn hơn 0.");
            }

            // KIEM TRA NGAY THANH TOAN
            if (payment.PaymentDate == DateTime.MinValue)
            {
                ModelState.AddModelError("PaymentDate",
                    "Ngày thanh toán không hợp lệ.");
            }

            // KIEM TRA PHUONG THUC
            string[] methods =
            {
        "Cash",
        "BankTransfer",
        "Online"
    };

            if (!methods.Contains(payment.PaymentMethod))
            {
                ModelState.AddModelError("PaymentMethod",
                    "Phương thức thanh toán không hợp lệ.");
            }

            // KIEM TRA TRANG THAI
            string[] statuses =
            {
        "Pending",
        "Success",
        "Failed"
    };

            if (!statuses.Contains(payment.Status))
            {
                ModelState.AddModelError("Status",
                    "Trạng thái thanh toán không hợp lệ.");
            }
        }


        // 10. NAP DANH SACH HOP DONG
        private void LoadDropdowns(int? contractId = null)
        {
            var contracts = db.Contracts
                .Include(c => c.Customer)
                .Include(c => c.Kiosk)
                .OrderByDescending(c => c.ContractId)
                .ToList();

            ViewBag.ContractId = new SelectList(
                contracts.Select(c => new SelectListItem
                {
                    Value = c.ContractId.ToString(),
                    Text = "HĐ #" + c.ContractId +
                           " - " + c.Customer.FullName +
                           " - " + c.Kiosk.KioskName
                }),
                "Value",
                "Text",
                contractId);
        }

        // 11. NAP PHUONG THUC THANH TOAN
        private void LoadMethods(string selected = null)
        {
            ViewBag.MethodList = new SelectList(
                new[]
                {
                    new { Value = "Cash", Text = "Tiền mặt" },
                    new { Value = "BankTransfer", Text = "Chuyển khoản" },
                    new { Value = "Online", Text = "Thanh toán trực tuyến" }
                },
                "Value",
                "Text",
                selected);
        }

        // 12. NAP TRANG THAI
        private void LoadStatuses()
        {
            ViewBag.StatusList = new SelectList(
                new[]
                {
                    new { Value = "Pending", Text = "Đang chờ" },
                    new { Value = "Success", Text = "Thành công" },
                    new { Value = "Failed", Text = "Thất bại" }
                },
                "Value",
                "Text",
                ViewBag.Status);
        }

        private void LoadPaymentStatuses(string selected = null)
        {
            ViewBag.PaymentStatusList = new SelectList(
                new[]
                {
                    new { Value = "Pending", Text = "Đang chờ" },
                    new { Value = "Success", Text = "Thành công" },
                    new { Value = "Failed", Text = "Thất bại" }
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
