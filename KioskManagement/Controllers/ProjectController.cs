
using System;
using System.Linq;
using System.Data.Entity;
using System.Web.Mvc;
using KioskManagement.Models;
using KioskManagement.Helpers;

namespace KioskManagement.Controllers
{
    [AuthorizeSession]
    public class ProjectController : Controller
    {
        private KioskManagementDBEntities db =
            new KioskManagementDBEntities();

        // Danh sách trạng thái hợp lệ theo SQL Server
        private void LoadStatuses(string selectedStatus = null)
        {
            ViewBag.StatusList = new SelectList(
                new[]
                {
                    new { Value = "Planning", Text = "Đang lên kế hoạch" },
                    new { Value = "InProgress", Text = "Đang thực hiện" },
                    new { Value = "Completed", Text = "Đã hoàn thành" },
                    new { Value = "Cancelled", Text = "Đã hủy" }
                },
                "Value",
                "Text",
                selectedStatus
            );
        }

        // 1. DANH SÁCH DỰ ÁN
        public ActionResult Index(string search)
        {
            var projects = db.Projects
                .Include(p => p.Kiosks)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                projects = projects.Where(p =>
                    p.ProjectName.Contains(search) ||
                    p.Location.Contains(search));
            }

            return View(projects.OrderBy(p => p.ProjectId).ToList());
        }

        // 2. CHI TIẾT DỰ ÁN
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(400);

            Project project = db.Projects
                .Include(p => p.Kiosks)
                .FirstOrDefault(p => p.ProjectId == id);

            if (project == null)
                return HttpNotFound();

            return View(project);
        }

        // 3. FORM THÊM DỰ ÁN
        public ActionResult Create()
        {
            LoadStatuses("Planning");

            Project project = new Project
            {
                StartDate = DateTime.Today,
                Budget = 0,
                Status = "Planning"
            };

            return View(project);
        }

        // 4. XỬ LÝ THÊM DỰ ÁN
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Project project)
        {
            if (string.IsNullOrWhiteSpace(project.ProjectName))
            {
                ModelState.AddModelError(
                    "ProjectName",
                    "Vui lòng nhập tên dự án."
                );
            }

            if (string.IsNullOrWhiteSpace(project.Location))
            {
                ModelState.AddModelError(
                    "Location",
                    "Vui lòng nhập địa điểm."
                );
            }

            if (project.Budget < 0)
            {
                ModelState.AddModelError(
                    "Budget",
                    "Ngân sách không được nhỏ hơn 0."
                );
            }

            if (project.EndDate.HasValue &&
                project.EndDate.Value.Date < project.StartDate.Date)
            {
                ModelState.AddModelError(
                    "EndDate",
                    "Ngày kết thúc phải bằng hoặc sau ngày bắt đầu."
                );
            }

            string[] validStatuses =
            {
                "Planning", "InProgress", "Completed", "Cancelled"
            };

            if (!validStatuses.Contains(project.Status))
            {
                ModelState.AddModelError(
                    "Status",
                    "Trạng thái dự án không hợp lệ."
                );
            }

            if (ModelState.IsValid)
            {
                db.Projects.Add(project);
                db.SaveChanges();

                TempData["Success"] = "Thêm dự án thành công!";
                return RedirectToAction("Index");
            }

            LoadStatuses(project.Status);
            return View(project);
        }

        // 5. FORM CHỈNH SỬA DỰ ÁN
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(400);

            Project project = db.Projects.Find(id);

            if (project == null)
                return HttpNotFound();

            LoadStatuses(project.Status);
            return View(project);
        }

        // 6. XỬ LÝ CHỈNH SỬA DỰ ÁN
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Project project)
        {
            if (string.IsNullOrWhiteSpace(project.ProjectName))
            {
                ModelState.AddModelError(
                    "ProjectName",
                    "Vui lòng nhập tên dự án."
                );
            }

            if (string.IsNullOrWhiteSpace(project.Location))
            {
                ModelState.AddModelError(
                    "Location",
                    "Vui lòng nhập địa điểm."
                );
            }

            if (project.Budget < 0)
            {
                ModelState.AddModelError(
                    "Budget",
                    "Ngân sách không được nhỏ hơn 0."
                );
            }

            if (project.EndDate.HasValue &&
                project.EndDate.Value.Date < project.StartDate.Date)
            {
                ModelState.AddModelError(
                    "EndDate",
                    "Ngày kết thúc phải bằng hoặc sau ngày bắt đầu."
                );
            }

            string[] validStatuses =
            {
                "Planning", "InProgress", "Completed", "Cancelled"
            };

            if (!validStatuses.Contains(project.Status))
            {
                ModelState.AddModelError(
                    "Status",
                    "Trạng thái dự án không hợp lệ."
                );
            }

            if (ModelState.IsValid)
            {
                Project existingProject =
                    db.Projects.Find(project.ProjectId);

                if (existingProject == null)
                    return HttpNotFound();

                existingProject.ProjectName = project.ProjectName;
                existingProject.Location = project.Location;
                existingProject.StartDate = project.StartDate;
                existingProject.EndDate = project.EndDate;
                existingProject.Budget = project.Budget;
                existingProject.Description = project.Description;
                existingProject.Status = project.Status;

                db.SaveChanges();

                TempData["Success"] = "Cập nhật dự án thành công!";
                return RedirectToAction("Index");
            }

            LoadStatuses(project.Status);
            return View(project);
        }

        // 7. TRANG XÁC NHẬN XÓA
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(400);

            Project project = db.Projects
                .Include(p => p.Kiosks)
                .FirstOrDefault(p => p.ProjectId == id);

            if (project == null)
                return HttpNotFound();

            return View(project);
        }

        // 8. XỬ LÝ XÓA
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Project project = db.Projects
                .Include(p => p.Kiosks)
                .FirstOrDefault(p => p.ProjectId == id);

            if (project == null)
                return HttpNotFound();

            if (project.Kiosks.Any())
            {
                TempData["Error"] =
                    "Không thể xóa dự án đang có Kios. " +
                    "Vui lòng xử lý các Kios liên quan trước.";

                return RedirectToAction("Index");
            }

            db.Projects.Remove(project);
            db.SaveChanges();

            TempData["Success"] = "Xóa dự án thành công!";
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
