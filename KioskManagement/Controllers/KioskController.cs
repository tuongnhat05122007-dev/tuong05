using System;
using System.Linq;
using System.Net;
using System.Data.Entity;
using System.Web.Mvc;
using KioskManagement.Models;
using KioskManagement.Helpers;

namespace KioskManagement.Controllers
{
    [AuthorizeSession]
    public class KioskController : Controller
    {
        private KioskManagementDBEntities db =
            new KioskManagementDBEntities();

        // Danh sach trang thai kios
        private string[] GetStatuses()
        {
            return new string[]
            {
        "Available",
        "Rented",
        "Maintenance",
        "Inactive"
            };
        }

        // Nap dropdown Khu vuc va Du an
        private void LoadDropdowns(
     int? areaId = null,
     int? projectId = null,
     string status = null)
        {
            ViewBag.AreaId = new SelectList(
                db.Areas.ToList(),
                "AreaId",
                "AreaName",
                areaId
            );

            ViewBag.ProjectId = new SelectList(
                db.Projects.ToList(),
                "ProjectId",
                "ProjectName",
                projectId
            );

            ViewBag.StatusList = new SelectList(
                GetStatuses(),
                status
            );
        }

        // Kiem tra du lieu nhap
        private void ValidateKiosk(Kiosk kiosk)
        {
            if (string.IsNullOrWhiteSpace(kiosk.KioskName))
            {
                ModelState.AddModelError(
                    "KioskName", "Vui lòng nhập tên kios.");
            }
            else if (kiosk.KioskName.Length > 150)
            {
                ModelState.AddModelError(
                    "KioskName", "Tên kios không được vượt quá 150 ký tự.");
            }

            if (kiosk.AreaId <= 0 ||
                !db.Areas.Any(a => a.AreaId == kiosk.AreaId))
            {
                ModelState.AddModelError(
                    "AreaId", "Vui lòng chọn khu vực hợp lệ.");
            }

            if (kiosk.ProjectId <= 0 ||
                !db.Projects.Any(p => p.ProjectId == kiosk.ProjectId))
            {
                ModelState.AddModelError(
                    "ProjectId", "Vui lòng chọn dự án hợp lệ.");
            }

            if (kiosk.AreaSize <= 0)
            {
                ModelState.AddModelError(
                    "AreaSize",
                    "Diện tích phải lớn hơn 0."
                );
            }

            if (kiosk.RentalPrice < 0)
            {
                ModelState.AddModelError(
                    "RentalPrice", "Giá thuê không được âm.");
            }

            if (string.IsNullOrWhiteSpace(kiosk.Status))
            {
                ModelState.AddModelError(
                    "Status", "Vui lòng chọn trạng thái.");
            }
            else if (!GetStatuses().Contains(kiosk.Status))
            {
                ModelState.AddModelError(
                    "Status", "Trạng thái không hợp lệ.");
            }

            if (kiosk.ImageUrl != null && kiosk.ImageUrl.Length > 255)
            {
                ModelState.AddModelError(
                    "ImageUrl", "Đường dẫn hình ảnh không được vượt quá 255 ký tự.");
            }
        }

        // GET: Kiosk
        public ActionResult Index(string search)
        {
            var kiosks = db.Kiosks
                .Include(k => k.Area)
                .Include(k => k.Project)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                kiosks = kiosks.Where(k =>
                    k.KioskName.Contains(search));
            }

            ViewBag.Search = search;

            return View(kiosks.ToList());
        }

        // GET: Kiosk/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest);
            }

            var kiosk = db.Kiosks
                .Include(k => k.Area)
                .Include(k => k.Project)
                .FirstOrDefault(k => k.KioskId == id.Value);

            if (kiosk == null)
            {
                return HttpNotFound();
            }

            return View(kiosk);
        }

        // GET: Kiosk/Create
        public ActionResult Create()
        {
            LoadDropdowns();
            return View();
        }

        // POST: Kiosk/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Kiosk kiosk)
        {
            ValidateKiosk(kiosk);

            if (ModelState.IsValid)
            {
                try
                {
                    db.Kiosks.Add(kiosk);
                    db.SaveChanges();

                    TempData["Success"] = "Thêm kios thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    string error = ex.GetBaseException().Message;

                    ModelState.AddModelError("", error);
                }
            }

            LoadDropdowns(
                kiosk.AreaId,
                kiosk.ProjectId,
                kiosk.Status
            );

            return View(kiosk);
        }

        // GET: Kiosk/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest);
            }

            var kiosk = db.Kiosks.Find(id);

            if (kiosk == null)
            {
                return HttpNotFound();
            }
            LoadDropdowns(kiosk.AreaId, kiosk.ProjectId, kiosk.Status);

            return View(kiosk);
        }

        // POST: Kiosk/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            [Bind(Include = "KioskId,AreaId,ProjectId,KioskName,AreaSize,RentalPrice,Description,ImageUrl,Status")]
            Kiosk kiosk)
        {
            ValidateKiosk(kiosk);

            if (ModelState.IsValid)
            {
                try
                {
                    var existingKiosk = db.Kiosks.Find(kiosk.KioskId);

                    if (existingKiosk == null)
                    {
                        return HttpNotFound();
                    }

                    // Cap nhat tung thuoc tinh
                    existingKiosk.KioskName = kiosk.KioskName;
                    existingKiosk.AreaId = kiosk.AreaId;
                    existingKiosk.ProjectId = kiosk.ProjectId;
                    existingKiosk.AreaSize = kiosk.AreaSize;
                    existingKiosk.RentalPrice = kiosk.RentalPrice;
                    existingKiosk.Description = kiosk.Description;
                    existingKiosk.ImageUrl = kiosk.ImageUrl;
                    existingKiosk.Status = kiosk.Status;

                    db.SaveChanges();

                    TempData["Success"] = "Cập nhật kios thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception)
                {
                    ModelState.AddModelError(
                        "",
                        "Không thể cập nhật kios. Vui lòng kiểm tra lại dữ liệu.");
                }
            }

            LoadDropdowns(kiosk.AreaId, kiosk.ProjectId, kiosk.Status);
            return View(kiosk);
        }

        // GET: Kiosk/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest);
            }

            var kiosk = db.Kiosks
                .Include(k => k.Area)
                .Include(k => k.Project)
                .FirstOrDefault(k => k.KioskId == id.Value);

            if (kiosk == null)
            {
                return HttpNotFound();
            }

            return View(kiosk);
        }

        // POST: Kiosk/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var kiosk = db.Kiosks.Find(id);

            if (kiosk == null)
            {
                return HttpNotFound();
            }

            // Khong xoa kios neu da co hop dong
            bool hasContract = db.Contracts.Any(
                c => c.KioskId == id);

            // Khong xoa kios neu da co yeu cau bao tri
            bool hasMaintenance = db.MaintenanceRequests.Any(
                m => m.KioskId == id);

            if (hasContract || hasMaintenance)
            {
                TempData["Error"] =
                    "Không thể xóa kios vì đang có hợp đồng hoặc yêu cầu bảo trì liên quan.";

                return RedirectToAction("Index");
            }

            try
            {
                // Xoa cac lien ket dich vu neu co
                db.Entry(kiosk)
                    .Collection(k => k.Services)
                    .Load();

                kiosk.Services.Clear();

                db.Kiosks.Remove(kiosk);
                db.SaveChanges();

                TempData["Success"] = "Xóa kios thành công!";
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "Không thể xóa kios. Có thể kios đang được dữ liệu khác tham chiếu.";
            }

            return RedirectToAction("Index");
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