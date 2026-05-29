using Microsoft.AspNetCore.Mvc;
using StudentManagementSystem.Models;
using System.Data;

namespace StudentManagementSystem.Controllers
{
    public class TeacherController : BaseController
    {
        DBAccess db = new DBAccess();

        // ================= ALL TEACHERS =================
        public IActionResult AllTeachers()
        {
            if (!IsAdmin())
                return RedirectToAction("SignIn", "Student");

            List<Teacher> list = new List<Teacher>();
            string q = "select tid, name, city, subject from Teacher";
            DataTable dt = db.GetDataTable(q);

            foreach (DataRow row in dt.Rows)
            {
                Teacher t = new Teacher();
                t.tid = row["tid"].ToString();
                t.name = row["name"].ToString();
                t.city = row["city"].ToString();
                t.subject = row["subject"].ToString();
                list.Add(t);
            }
            return View(list);
        }

        // ================= ADD TEACHER =================
        [HttpGet]
        public IActionResult AddTeacher()
        {
            if (!IsAdmin())
                return RedirectToAction("SignIn", "Student");
            return View();
        }

        [HttpPost]
        public IActionResult AddTeacher(Teacher t)
        {
            if (!IsAdmin())
                return RedirectToAction("SignIn", "Student");

            t.tid = t.email.Split('@')[0];
            t.role = "teacher";

            string q = "insert into Teacher values('" + t.tid + "','" + t.name + "','" +
                       t.city + "','" + t.email + "','" + t.password + "','" +
                       t.subject + "','" + t.role + "')";
            db.IUD(q);

            TempData["msg"] = "Teacher Added Successfully!";
            return RedirectToAction("AllTeachers");
        }

        // ================= EDIT TEACHER =================
        [HttpGet]
        public IActionResult EditTeacher(string tid)
        {
            if (!IsAdmin())
                return RedirectToAction("SignIn", "Student");

            string q = "select * from Teacher where tid='" + tid + "'";
            DataTable dt = db.GetDataTable(q);

            Teacher t = new Teacher();
            if (dt.Rows.Count > 0)
            {
                t.tid = dt.Rows[0]["tid"].ToString();
                t.name = dt.Rows[0]["name"].ToString();
                t.city = dt.Rows[0]["city"].ToString();
                t.email = dt.Rows[0]["email"].ToString();
                t.subject = dt.Rows[0]["subject"].ToString();
            }
            return View(t);
        }

        [HttpPost]
        public IActionResult EditTeacher(Teacher t)
        {
            string q = "update Teacher set name='" + t.name + "', city='" + t.city +
                       "', email='" + t.email + "', subject='" + t.subject +
                       "' where tid='" + t.tid + "'";
            db.IUD(q);

            TempData["msg"] = "Teacher Updated!";
            return RedirectToAction("AllTeachers");
        }

        // ================= DELETE TEACHER =================
        public IActionResult DeleteTeacher(string tid)
        {
            if (!IsAdmin())
                return RedirectToAction("SignIn", "Student");

            string q = "delete from Teacher where tid='" + tid + "'";
            db.IUD(q);

            TempData["msg"] = "Teacher Deleted!";
            return RedirectToAction("AllTeachers");
        }

        // ================= TEACHER DETAIL =================
        public IActionResult DetailTeacher(string tid)
        {
            string q = "select * from Teacher where tid='" + tid + "'";
            DataTable dt = db.GetDataTable(q);

            Teacher t = new Teacher();
            if (dt.Rows.Count > 0)
            {
                t.tid = dt.Rows[0]["tid"].ToString();
                t.name = dt.Rows[0]["name"].ToString();
                t.city = dt.Rows[0]["city"].ToString();
                t.email = dt.Rows[0]["email"].ToString();
                t.subject = dt.Rows[0]["subject"].ToString();
            }
            return View(t);
        }

        // ================= TEACHER LOGIN =================
        [HttpGet]
        public IActionResult TeacherSignIn()
        {
            return View();
        }

        [HttpPost]
        public IActionResult TeacherSignIn(string tid, string password)
        {
            string q = "select tid, name from Teacher where tid='" + tid +
                       "' and password='" + password + "'";
            DataTable dt = db.GetDataTable(q);

            if (dt.Rows.Count > 0)
            {
                HttpContext.Session.SetString("sid", dt.Rows[0]["tid"].ToString());
                HttpContext.Session.SetString("name", dt.Rows[0]["name"].ToString());
                HttpContext.Session.SetString("role", "teacher");
                return RedirectToAction("TeacherDashboard");
            }

            ViewBag.Error = "Invalid ID or Password";
            return View();
        }
        public IActionResult TeacherDashboard()
        {
            if (HttpContext.Session.GetString("role") != "teacher")
                return RedirectToAction("TeacherSignIn");

            string tid = HttpContext.Session.GetString("sid");
            ViewBag.Name = HttpContext.Session.GetString("name");

         
            string q1 = "select count(*) from Attendance where markedBy='" + tid +
                        "' and date=CAST(GETDATE() AS DATE)";
            ViewBag.TodayAttendance = db.GetDataTable(q1).Rows[0][0];

         
            string q2 = "select count(*) from Student where role='student'";
            ViewBag.TotalStudents = db.GetDataTable(q2).Rows[0][0];

         
            ViewBag.Classes = db.GetDataTable("select classId, className from Class order by classId");

            return View();
        }
    }
}