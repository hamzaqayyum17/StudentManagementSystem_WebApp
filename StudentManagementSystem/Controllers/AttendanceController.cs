using Microsoft.AspNetCore.Mvc;
using StudentManagementSystem.Models;
using System.Data;

namespace StudentManagementSystem.Controllers
{
    public class AttendanceController : BaseController
    {
        DBAccess db = new DBAccess();

        // ================= MARK ATTENDANCE =================
        [HttpGet]
        public IActionResult MarkAttendance()
        {
            if (HttpContext.Session.GetString("role") != "teacher")
                return RedirectToAction("TeacherSignIn", "Teacher");

            // Sare students lao
            string q = "select sid, name from Student where role='student'";
            DataTable dt = db.GetDataTable(q);
            ViewBag.Students = dt;

            return View();
        }

        [HttpPost]
        public IActionResult MarkAttendance(IFormCollection form)
        {
            if (HttpContext.Session.GetString("role") != "teacher")
                return RedirectToAction("TeacherSignIn", "Teacher");

            string tid = HttpContext.Session.GetString("sid");
            string date = DateTime.Now.ToString("yyyy-MM-dd");

            // Sare students ki list lo
            string q = "select sid from Student where role='student'";
            DataTable dt = db.GetDataTable(q);

            foreach (DataRow row in dt.Rows)
            {
                string sid = row["sid"].ToString();
                string status = form["status_" + sid].ToString();

                if (string.IsNullOrEmpty(status))
                    status = "Absent";

                // Pehle check karo — aaj ki attendance already hai?
                string checkQ = "select count(*) from Attendance where sid='" + sid +
                                "' and date='" + date + "'";
                int count = int.Parse(db.GetDataTable(checkQ).Rows[0][0].ToString());

                if (count == 0)
                {
                    // Nai entry
                    string insertQ = "insert into Attendance(sid, date, status, markedBy)" +
                                     " values('" + sid + "','" + date + "','" +
                                     status + "','" + tid + "')";
                    db.IUD(insertQ);
                }
                else
                {
                    // Update karo agar pehle se hai
                    string updateQ = "update Attendance set status='" + status +
                                     "', markedBy='" + tid +
                                     "' where sid='" + sid +
                                     "' and date='" + date + "'";
                    db.IUD(updateQ);
                }
            }

            TempData["msg"] = "Attendance Saved Successfully!";
            return RedirectToAction("ViewAttendance");
        }

        // ================= VIEW ATTENDANCE (Admin) =================
        public IActionResult ViewAttendance()
        {
            if (!IsLoggedIn())
                return RedirectToAction("SignIn", "Student");

            string role = HttpContext.Session.GetString("role");

            List<Attendance> list = new List<Attendance>();
            string q = "";

            if (role == "admin")
            {
                // Admin — sab dekhe
                q = @"select a.aid, s.name as studentName, 
                             a.date, a.status, a.markedBy
                      from Attendance a
                      join Student s on a.sid = s.sid
                      order by a.date desc";
            }
            else if (role == "teacher")
            {
                // Teacher — sirf apni marked attendance
                string tid = HttpContext.Session.GetString("sid");
                q = @"select a.aid, s.name as studentName,
                             a.date, a.status, a.markedBy
                      from Attendance a
                      join Student s on a.sid = s.sid
                      where a.markedBy='" + tid + @"'
                      order by a.date desc";
            }

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
    }
}