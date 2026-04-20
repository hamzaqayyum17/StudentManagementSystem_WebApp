using DocumentFormat.OpenXml.ExtendedProperties;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using StudentManagementSystem.Models;
using System.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace StudentManagementSystem.Controllers
{
    public class StudentController : Controller
    {
        DBAccess db = new DBAccess();
        
        [HttpGet]
        public IActionResult SignUp()
        {
            return View();
        }
        [HttpPost]
        public IActionResult SignUp(Student s)
        {
            try
            {
                if (string.IsNullOrEmpty(s.email))
                {
                    ViewBag.Msg = "Email is required";
                    return View(s);
                }

                s.sid = s.email.Split('@')[0];

                string role = "student"; // default

                string q = "insert into Student values('" + s.sid + "','" + s.name + "','" + s.city + "','" + s.email + "','" + s.password + "','" + s.role + "')";

                db.IUD(q);

                TempData["msg"] = "Student Added Successfully!";
                return RedirectToAction("SignIn");
            }
            catch (Exception ex)
            {
                ViewBag.Msg = ex.Message; 
                return View(s);
            }
        }
        // ================= ALL STUDENTS =================
        [HttpGet]
        public IActionResult AllStudents(string search)
        {
            if (HttpContext.Session.GetString("role") != "admin")
            {
                return RedirectToAction("SignIn", "Student");
            }
            ViewBag.Search = search;
            List<Student> list = new List<Student>();

            string q = "select sid,name,city from Student";
            if (!string.IsNullOrEmpty(search))
            {
                q += " where name like '%" + search + "%'";
            }
            DataTable dt = db.GetDataTable(q);

            foreach (DataRow row in dt.Rows)
            {
                Student s = new Student();
                s.sid = row["sid"].ToString();
                s.name = row["name"].ToString();
                s.city = row["city"].ToString();

                list.Add(s);
            }

            return View(list);
        }

        // ================= DELETE =================
        [HttpGet]
        public IActionResult Delete(string sid)
        {
            if (HttpContext.Session.GetString("role") != "admin")
            {
                return RedirectToAction("SignIn", "Student");
            }
            string q = "delete from Student where sid='" + sid + "'";
            db.IUD(q);
            TempData["msg"] = "Student Deleted Successfully!";
            return RedirectToAction("AllStudents");
        }

        // ================= DETAIL =================
        [HttpGet]
        public IActionResult Detail(string sid)
        {
            string q = "select * from Student where sid='" + sid + "'";
            DataTable dt = db.GetDataTable(q);

            Student s = new Student();

            if (dt.Rows.Count > 0)
            {
                s.sid = dt.Rows[0]["sid"].ToString();
                s.name = dt.Rows[0]["name"].ToString();
                s.city = dt.Rows[0]["city"].ToString();
                s.email = dt.Rows[0]["email"].ToString();
            }

            return View(s);
        }
        // ================= EDIT =================
        [HttpGet]
        public IActionResult Edit(string sid)
        {
            if (HttpContext.Session.GetString("role") != "admin")
            {
                return RedirectToAction("SignIn", "Student");
            }
            string q = "select * from Student where sid='" + sid + "'";
            DataTable dt = db.GetDataTable(q);

            Student s = new Student();

            if (dt.Rows.Count > 0)
            {
                s.sid = dt.Rows[0]["sid"].ToString();
                s.name = dt.Rows[0]["name"].ToString();
                s.city = dt.Rows[0]["city"].ToString();
                s.email = dt.Rows[0]["email"].ToString();
            }

            return View(s);
        }

        [HttpPost]
        public IActionResult Edit(Student s)
        {
            string q = "update Student set name='" + s.name + "',city='" + s.city + "',email='" + s.email + "' where sid='" + s.sid + "'";
            db.IUD(q);

            return RedirectToAction("AllStudents");
        }
        // ================= SIGN IN =================
        [HttpGet]
        public IActionResult SignIn()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SignIn(string sid, string password)
        {
            string q = "select sid,name,role from Student where sid='" + sid + "' and password='" + password + "'";
            DataTable dt = db.GetDataTable(q);

            if (dt.Rows.Count > 0)
            {
                HttpContext.Session.SetString("sid", dt.Rows[0]["sid"].ToString());
                HttpContext.Session.SetString("name", dt.Rows[0]["name"].ToString());
                HttpContext.Session.SetString("role", dt.Rows[0]["role"].ToString());

                string role = dt.Rows[0]["role"].ToString();

                if (role == "admin")
                {
                    return RedirectToAction("AdminDashboard");
                }
                else
                {
                    return RedirectToAction("StudentDashboard");
                }
            }

            // ❗ MUST RETURN IF LOGIN FAILS
            ViewBag.Error = "Invalid ID or Password";
            return View();
        }
        // ================= DASHBOARD =================        
        public IActionResult AdminDashboard()
        {
            if (HttpContext.Session.GetString("role") != "admin")
                return RedirectToAction("SignIn", "Student");

            string q1 = "select count(*) from Student";
            string q2 = "select count(*) from Course";
            string q3 = "select count(*) from Enrollment";

            ViewBag.TotalStudents = db.GetDataTable(q1).Rows[0][0];
            ViewBag.TotalCourses = db.GetDataTable(q2).Rows[0][0];
            ViewBag.TotalEnrollments = db.GetDataTable(q3).Rows[0][0];

            return View();
        }
        public IActionResult StudentDashboard()
        {
            if (HttpContext.Session.GetString("role") != "student")
                return RedirectToAction("SignIn");

            ViewBag.Name = HttpContext.Session.GetString("name");
            return View();
        }
        public IActionResult MyProfile()
        {
            if (HttpContext.Session.GetString("sid") == null)
            {
                return RedirectToAction("SignIn");
            }
            string sid = HttpContext.Session.GetString("sid");

            string q = "select * from Student where sid='" + sid + "'";
            DataTable dt = db.GetDataTable(q);

            Student s = new Student();

            if (dt.Rows.Count > 0)
            {
                s.sid = dt.Rows[0]["sid"].ToString();
                s.name = dt.Rows[0]["name"].ToString();
                s.city = dt.Rows[0]["city"].ToString();
                s.email = dt.Rows[0]["email"].ToString();
            }

            return View(s);
        }
        public IActionResult MyCourses()
        {
            if (HttpContext.Session.GetString("sid") == null)
            {
                return RedirectToAction("SignIn");
            }
            string sid = HttpContext.Session.GetString("sid");

            string q = @"SELECT c.cname, e.grade
                 FROM Enrollment e
                 JOIN Course c ON e.cid = c.cid
                 WHERE e.sid='" + sid + "'";

            DataTable dt = db.GetDataTable(q);

            return View(dt);
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("SignIn");
        }
    }
}
//Sir, maine ASP.NET Core MVC me Student Management System banaya hai.

//Is system me 2 roles hain:
//1.Admin
//2.Student

//Admin:
//-Students manage karta hai(CRUD)
//- Courses manage karta hai
//- Enrollment manage karta hai

//Student:
//-Apna profile dekh sakta hai
//- Apne courses aur grades dekh sakta hai

//Maine SQL Server database use kiya hai aur DBAccess class ke through database connect kiya hai.