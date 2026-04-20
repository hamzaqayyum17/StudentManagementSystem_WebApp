using Microsoft.AspNetCore.Mvc;
using StudentManagementSystem.Models;
using System.Data;

namespace StudentManagementSystem.Controllers
{
    public class CourseController : BaseController
    {
        DBAccess db = new DBAccess();

        // ================= ADD COURSE =================
        [HttpGet]
        public IActionResult AddCourse()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddCourse(Course c)
        {
            if (HttpContext.Session.GetString("role") != "admin")
            {
                return RedirectToAction("SignIn", "Student");
            }
            string q = "insert into Course values('" + c.cname + "','" + c.creditHours + "')";
            db.IUD(q);

            return RedirectToAction("ViewCourses");
        }

        // ================= VIEW COURSES =================
        public IActionResult ViewCourses()
        {
            if (HttpContext.Session.GetString("role") != "admin")
            {
                return RedirectToAction("SignIn", "Student");
            }
            List<Course> list = new List<Course>();

            string q = "select * from Course";
            DataTable dt = db.GetDataTable(q);

            foreach (DataRow row in dt.Rows)
            {
                Course c = new Course();
                c.cid = int.Parse(row["cid"].ToString());
                c.cname = row["cname"].ToString();
                c.creditHours = int.Parse(row["creditHours"].ToString());

                list.Add(c);
            }

            return View(list);
        }
        public IActionResult DeleteCourse(int cid)
        {
            if (HttpContext.Session.GetString("role") != "admin")
                return RedirectToAction("SignIn", "Student");

            string q = "delete from Course where cid=" + cid;
            db.IUD(q);
            return RedirectToAction("ViewCourses");
        }

        [HttpGet]
        public IActionResult EditCourse(int cid)
        {
            string q = "select * from Course where cid=" + cid;
            DataTable dt = db.GetDataTable(q);
            Course c = new Course();
            if (dt.Rows.Count > 0)
            {
                c.cid = int.Parse(dt.Rows[0]["cid"].ToString());
                c.cname = dt.Rows[0]["cname"].ToString();
                c.creditHours = int.Parse(dt.Rows[0]["creditHours"].ToString());
            }
            return View(c);
        }

        [HttpPost]
        public IActionResult EditCourse(Course c)
        {
            string q = "update Course set cname='" + c.cname + "',creditHours=" + c.creditHours + " where cid=" + c.cid;
            db.IUD(q);
            return RedirectToAction("ViewCourses");
        }
    }
}
