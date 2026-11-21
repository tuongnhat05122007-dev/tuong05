using System;
using System.Linq;
using System.Data.Entity;
using System.Web.Mvc;
using KioskManagement.Models;
using KioskManagement.Helpers;

namespace KioskManagement.Controllers
{
    [AuthorizeSession]
    public class AreaController : Controller
    {
        private KioskManagementDBEntities db =
            new KioskManagementDBEntities();

        // 1. DANH SÁCH KHU VỰC
        public ActionResult Index(string search)
        {
            var areas = db.Areas
                .Include(a => a.Kiosks)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                areas = areas.Where(a =>
                    a.AreaName.Contains(search) ||
                    a.Location.Contains(search));
            }

            return View(areas.OrderBy(a => a.AreaId).ToList());
        }

        // 2. XEM CHI TIẾT
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(400);

            Area area = db.Areas
                .Include(a => a.Kiosks)
                .FirstOrDefault(a => a.AreaId == id);

            if (area == null)
                return HttpNotFound();

            return View(area);
        }

        // 3. HIỂN THỊ FORM THÊM
        public ActionResult Create()
        {
            Area area = new Area
            {
                IsActive = true
            };

            return View(area);
        }

        // 4. XỬ LÝ THÊM KHU VỰC
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Area area)
        {
            if (string.IsNullOrWhiteSpace(area.AreaName))
            {
                ModelState.AddModelError(
                    "AreaName",
                    "Vui lòng nhập tên khu vực."
                );
            }

            if (string.IsNullOrWhiteSpace(area.Location))
            {
                ModelState.AddModelError(
                    "Location",
                    "Vui lòng nhập địa điểm."
                );
            }

            bool duplicate = db.Areas.Any(a =>
                a.AreaName == area.AreaName);

            if (duplicate)
            {
                ModelState.AddModelError(
                    "AreaName",
                    "Tên khu vực này đã tồn tại."
                );
            }

            if (ModelState.IsValid)
            {
                db.Areas.Add(area);
                db.SaveChanges();

                TempData["Success"] = "Thêm khu vực thành công!";
                return RedirectToAction("Index");
            }

            return View(area);
        }

        // 5. HIỂN THỊ FORM CHỈNH SỬA
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(400);

            Area area = db.Areas.Find(id);

            if (area == null)
                return HttpNotFound();

            return View(area);
        }

        // 6. XỬ LÝ CHỈNH SỬA
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Area area)
        {
            if (string.IsNullOrWhiteSpace(area.AreaName))
            {
                ModelState.AddModelError(
                    "AreaName",
                    "Vui lòng nhập tên khu vực."
                );
            }

            if (string.IsNullOrWhiteSpace(area.Location))
            {
                ModelState.AddModelError(
                    "Location",
                    "Vui lòng nhập địa điểm."
                );
            }

            bool duplicate = db.Areas.Any(a =>
                a.AreaName == area.AreaName &&
                a.AreaId != area.AreaId);

            if (duplicate)
            {
                ModelState.AddModelError(
                    "AreaName",
                    "Tên khu vực đã được sử dụng."
                );
            }

            if (ModelState.IsValid)
            {
                Area existingArea = db.Areas.Find(area.AreaId);

                if (existingArea == null)
                    return HttpNotFound();

                existingArea.AreaName = area.AreaName;
                existingArea.Location = area.Location;
                existingArea.Description = area.Description;
                existingArea.IsActive = area.IsActive;

                db.SaveChanges();

                TempData["Success"] = "Cập nhật khu vực thành công!";
                return RedirectToAction("Index");
            }

            return View(area);
        }

        // 7. HIỂN THỊ TRANG XÁC NHẬN XÓA
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(400);

            Area area = db.Areas
                .Include(a => a.Kiosks)
                .FirstOrDefault(a => a.AreaId == id);

            if (area == null)
                return HttpNotFound();

            return View(area);
        }

        // 8. XỬ LÝ XÓA
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Area area = db.Areas
                .Include(a => a.Kiosks)
                .FirstOrDefault(a => a.AreaId == id);

            if (area == null)
                return HttpNotFound();

            if (area.Kiosks.Any())
            {
                TempData["Error"] =
                    "Không thể xóa khu vực đang có Kios. " +
                    "Vui lòng chuyển hoặc xóa các Kios trước.";

                return RedirectToAction("Index");
            }

            db.Areas.Remove(area);
            db.SaveChanges();

            TempData["Success"] = "Xóa khu vực thành công!";
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