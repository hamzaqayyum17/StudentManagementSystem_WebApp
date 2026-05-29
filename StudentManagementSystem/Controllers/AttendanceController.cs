using Microsoft.AspNetCore.Mvc;
using StudentManagementSystem.Models;
using StudentManagementSystem.Services;
using System.Data;

namespace StudentManagementSystem.Controllers
{
    public class AttendanceController : BaseController
    {
        private readonly GoogleSheetsService _sheetsService;
        private readonly EmailService _emailService;

        public AttendanceController(
            GoogleSheetsService sheetsService,
            EmailService emailService)
        {
            _sheetsService = sheetsService;
            _emailService = emailService;
        }
        // ================= SYNC FROM GOOGLE SHEETS =================
        public async Task<IActionResult> SyncFromGoogleSheet()
        {
            if (!IsAdmin() && HttpContext.Session.GetString("role") != "teacher")
                return RedirectToAction("SignIn", "Student");

            try
            {
                int synced = await _sheetsService.SyncAttendanceAsync();
                TempData["msg"] = $"Sync Complete! {synced} new records added.";
            }
            catch (Exception ex)
            {
                TempData["error"] = "Sync failed: " + ex.Message;
            }

            return RedirectToAction("ViewAttendance");
        }
        DBAccess db = new DBAccess();

        // ================= VIEW ATTENDANCE (Admin) =================
        public IActionResult ViewAttendance(int? classId)
        {
            if (!IsLoggedIn())
                return RedirectToAction("SignIn", "Student");

            ViewBag.Classes = db.GetDataTable("select classId, className from Class order by classId");
            ViewBag.SelectedClass = classId;

            string role = HttpContext.Session.GetString("role");
            List<Attendance> list = new List<Attendance>();

            string q = @"select a.aid, s.name as studentName,
                        a.date, a.status, a.markedBy,
                        cl.className
                 from Attendance a
                 join Student s on a.sid = s.sid
                 left join Class cl on s.classId = cl.classId
                 where 1=1";

            if (classId.HasValue)
                q += " and s.classId=" + classId.Value;

            q += " order by a.date desc";

            DataTable dt = db.GetDataTable(q);
            foreach (DataRow row in dt.Rows)
            {
                Attendance a = new Attendance();
                a.aid = int.Parse(row["aid"].ToString());
                a.studentName = row["studentName"].ToString();
                a.date = DateTime.Parse(row["date"].ToString());
                a.status = row["status"].ToString();
                a.markedBy = row["markedBy"].ToString();
                list.Add(a);
            }
            return View(list);
        }
        // ================= MY ATTENDANCE (Student) =================
        public IActionResult MyAttendance()
        {
            if (HttpContext.Session.GetString("role") != "student")
                return RedirectToAction("SignIn", "Student");

            string sid = HttpContext.Session.GetString("sid");

            string q = @"select date, status 
                         from Attendance 
                         where sid='" + sid + @"'
                         order by date desc";

            DataTable dt = db.GetDataTable(q);

            // Summary bhi calculate karo
            int total = dt.Rows.Count;
            int present = 0;
            int absent = 0;
            int late = 0;

            List<Attendance> list = new List<Attendance>();

            foreach (DataRow row in dt.Rows)
            {
                Attendance a = new Attendance();
                a.date = DateTime.Parse(row["date"].ToString());
                a.status = row["status"].ToString();
                list.Add(a);

                if (a.status == "Present") present++;
                else if (a.status == "Absent") absent++;
                else if (a.status == "Late") late++;
            }

            ViewBag.Total = total;
            ViewBag.Present = present;
            ViewBag.Absent = absent;
            ViewBag.Late = late;

            return View(list);
        }

        // ================= DELETE ATTENDANCE (Admin) =================
        public IActionResult DeleteAttendance(int aid)
        {
            if (!IsAdmin())
                return RedirectToAction("SignIn", "Student");

            string q = "delete from Attendance where aid=" + aid;
            db.IUD(q);

            TempData["msg"] = "Record Deleted!";
            return RedirectToAction("ViewAttendance");
        }
        // ================= SEND ABSENT EMAIL =================
        public async Task<IActionResult> SendAbsentEmail(int aid)
        {
            if (!IsAdmin() && HttpContext.Session.GetString("role") != "teacher")
                return RedirectToAction("SignIn", "Student");

            string q = @"select s.name, s.email, a.date
                 from Attendance a
                 join Student s on a.sid = s.sid
                 where a.aid=" + aid;

            DataTable dt = db.GetDataTable(q);

            if (dt.Rows.Count > 0)
            {
                string name = dt.Rows[0]["name"].ToString();
                string email = dt.Rows[0]["email"].ToString();
                string date = DateTime.Parse(dt.Rows[0]["date"].ToString())
                                       .ToString("dd MMM yyyy");

                await _emailService.SendAbsentEmailAsync(email, name, date);
                TempData["msg"] = $"Absent notification sent to {name}!";
            }

            return RedirectToAction("ViewAttendance");
        }

        // ================= SYNC BY CLASS =================
        public async Task<IActionResult> SyncByClass(int classId)
        {
            if (!IsAdmin() && HttpContext.Session.GetString("role") != "teacher")
                return RedirectToAction("SignIn", "Student");
            try
            {
                int synced = await _sheetsService.SyncClassAttendanceAsync(classId);
                TempData["msg"] = $"Sync Complete! {synced} new records added.";
            }
            catch (Exception ex)
            {
                TempData["msg"] = "Error: " + ex.Message;
            }
            return RedirectToAction("ViewAttendance");
        }
        // ================= GET SHEET URL =================
        public IActionResult OpenSheet(int classId)
        {
            if (!IsLoggedIn())
                return RedirectToAction("SignIn", "Student");

            string q = "select googleSheetId, className from Class where classId=" + classId;
            DataTable dt = db.GetDataTable(q);

            if (dt.Rows.Count > 0 && dt.Rows[0]["googleSheetId"] != DBNull.Value)
            {
                string sheetId = dt.Rows[0]["googleSheetId"].ToString();
                string url = "https://docs.google.com/spreadsheets/d/" + sheetId + "/edit";
                return Redirect(url);
            }

            TempData["error"] = "Is class ki Google Sheet set nahi hai!";
            return RedirectToAction("ViewAttendance");
        }

        public IActionResult GetSheetLink(int classId)
        {
            if (!IsLoggedIn())
                return RedirectToAction("SignIn", "Student");

            string q = "select googleSheetId from Class where classId=" + classId;
            DataTable dt = db.GetDataTable(q);

            if (dt.Rows.Count > 0 && dt.Rows[0]["googleSheetId"] != DBNull.Value)
            {
                string sheetId = dt.Rows[0]["googleSheetId"].ToString();
                string url = "https://docs.google.com/spreadsheets/d/" + sheetId + "/edit";
                return Json(new { success = true, url = url });
            }

            return Json(new { success = false, message = "Is class ki Google Sheet set nahi hai." });
        }


        public IActionResult GetMySheetLink()
        {
            if (!IsLoggedIn())
                return RedirectToAction("SignIn", "Student");

            string sid = HttpContext.Session.GetString("sid");

            // Student ki class ka googleSheetId fetch karo
            string q = @"select c.googleSheetId 
                 from Student s
                 join Class c on s.classId = c.classId
                 where s.sid='" + sid + "'";

            DataTable dt = db.GetDataTable(q);

            if (dt.Rows.Count > 0 && dt.Rows[0]["googleSheetId"] != DBNull.Value)
            {
                string sheetId = dt.Rows[0]["googleSheetId"].ToString();
                string url = "https://docs.google.com/spreadsheets/d/" + sheetId + "/edit";
                return Json(new { success = true, url = url });
            }

            return Json(new { success = false, message = "Aapki class ki Google Sheet set nahi hai." });
        }
    }

}