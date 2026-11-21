
using System;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using KioskManagement.Models;
using KioskManagement.Helpers;

namespace KioskManagement.Controllers
{
    [AuthorizeSession]
    public class HomeController : Controller
    {
        private KioskManagementDBEntities db =
            new KioskManagementDBEntities();

        // =====================================
        // 1. TRANG CHU - DASHBOARD
        // =====================================
        public ActionResult Index()
        {
            // TONG SO KIOSK
            ViewBag.TotalKiosks = db.Kiosks.Count();

            // KIOSK THEO TRANG THAI
            ViewBag.AvailableKiosks =
                db.Kiosks.Count(k => k.Status == "Available");

            ViewBag.RentedKiosks =
                db.Kiosks.Count(k => k.Status == "Rented");

            ViewBag.MaintenanceKiosks =
                db.Kiosks.Count(k => k.Status == "Maintenance");

            // =====================================
            // 2. THONG KE KIOSK THEO KHU VUC
            // =====================================
            ViewBag.KiosksByArea = db.Kiosks
                .GroupBy(k => k.Area.AreaName)
                .Select(g => new
                {
                    AreaName = g.Key,
                    Total = g.Count()
                })
                .ToList();

            // =====================================
            // 3. THONG KE KIOSK THEO DU AN
            // =====================================
            ViewBag.KiosksByProject = db.Kiosks
                .GroupBy(k => k.Project.ProjectName)
                .Select(g => new
                {
                    ProjectName = g.Key,
                    Total = g.Count()
                })
                .ToList();

            // =====================================
            // 4. TONG SO KHACH HANG
            // =====================================
            ViewBag.TotalCustomers = db.Customers.Count();

            // =====================================
            // 5. THONG KE HOP DONG
            // =====================================
            ViewBag.TotalContracts = db.Contracts.Count();

            ViewBag.ActiveContracts =
                db.Contracts.Count(c => c.Status == "Active");

            ViewBag.PendingContracts =
                db.Contracts.Count(c => c.Status == "Pending");

            ViewBag.CompletedContracts =
                db.Contracts.Count(c => c.Status == "Completed");

            ViewBag.CancelledContracts =
                db.Contracts.Count(c => c.Status == "Cancelled");

            // =====================================
            // 6. THONG KE THANH TOAN
            // =====================================
            ViewBag.TotalPayments = db.Payments.Count();

            // TONG DOANH THU THANH TOAN THANH CONG
            ViewBag.TotalRevenue = db.Payments
                .Where(p => p.Status == "Success")
                .Select(p => (decimal?)p.Amount)
                .Sum() ?? 0;

            // DOANH THU TIEN MAT
            ViewBag.CashRevenue = db.Payments
                .Where(p => p.Status == "Success"
                         && p.PaymentMethod == "Cash")
                .Select(p => (decimal?)p.Amount)
                .Sum() ?? 0;

            // DOANH THU CHUYEN KHOAN
            ViewBag.BankRevenue = db.Payments
                .Where(p => p.Status == "Success"
                         && p.PaymentMethod == "BankTransfer")
                .Select(p => (decimal?)p.Amount)
                .Sum() ?? 0;

            // DOANH THU TRUC TUYEN
            ViewBag.OnlineRevenue = db.Payments
                .Where(p => p.Status == "Success"
                         && p.PaymentMethod == "Online")
                .Select(p => (decimal?)p.Amount)
                .Sum() ?? 0;

            // =====================================
            // 7. THONG KE BAO TRI
            // =====================================
            ViewBag.TotalMaintenance =
                db.MaintenanceRequests.Count();

            ViewBag.PendingMaintenance =
                db.MaintenanceRequests.Count(
                    r => r.Status == "Pending");

            // =====================================
            // 8. 5 YEU CAU BAO TRI MOI NHAT
            // =====================================
            ViewBag.LatestMaintenanceRequests =
                db.MaintenanceRequests
                    .Include(r => r.Customer)
                    .Include(r => r.Kiosk)
                    .OrderByDescending(r => r.RequestDate)
                    .Take(5)
                    .ToList();

            return View();
        }

        // =====================================
        // 9. TRANG ABOUT
        // =====================================
        public ActionResult About()
        {
            ViewBag.Message =
                "Your application description page.";

            return View();
        }

        // =====================================
        // 10. TRANG CONTACT
        // =====================================
        public ActionResult Contact()
        {
            ViewBag.Message =
                "Your application description page.";

            return View();
        }

        // =====================================
        // 11. GIAI PHONG KET NOI DATABASE
        // =====================================
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
