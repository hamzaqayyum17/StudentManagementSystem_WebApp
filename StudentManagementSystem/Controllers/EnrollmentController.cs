using Microsoft.AspNetCore.Mvc;
using StudentManagementSystem.Models;
using System.Data;

namespace StudentManagementSystem.Controllers
{
    public class EnrollmentController : Controller
    {
        DBAccess db = new DBAccess();

        // ================= ADD ENROLLMENT =================
        [HttpGet]
        public IActionResult AddEnrollment()
        {
            // Students
            string qs = "select sid,name from Student";
            DataTable dtS = db.GetDataTable(qs);
            ViewBag.Students = dtS;

            // Courses
            string qc = "select cid,cname from Course";
            DataTable dtC = db.GetDataTable(qc);
            ViewBag.Courses = dtC;

            return View();
        }

        [HttpPost]
        public IActionResult AddEnrollment(Enrollment e)
        {
            if (HttpContext.Session.GetString("role") != "admin")
            {
                return RedirectToAction("SignIn", "Student");
            }
            string q = "insert into Enrollment values('" + e.sid + "','" + e.cid + "','" + e.grade + "')";
            db.IUD(q);

            return RedirectToAction("ViewEnrollment");
        }

        // ================= VIEW ENROLLMENT =================
        public IActionResult ViewEnrollment()
        {
            if (HttpContext.Session.GetString("role") != "admin")
            {
                return RedirectToAction("SignIn", "Student");
            }
            List<dynamic> list = new List<dynamic>();

            string q = @"SELECT e.eid, s.name AS studentName, c.cname AS courseName, e.grade
                 FROM Enrollment e
                 JOIN Student s ON e.sid = s.sid
                 JOIN Course c ON e.cid = c.cid";

            DataTable dt = db.GetDataTable(q);

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new
                {
                    eid = row["eid"],
                    studentName = row["studentName"],
                    courseName = row["courseName"],
                    grade = row["grade"]
                });
            }

            return View(list);
        }
        public IActionResult Delete(int eid)
        {
            if (HttpContext.Session.GetString("role") != "admin")
                return RedirectToAction("SignIn", "Student");

            string q = "delete from Enrollment where eid=" + eid;
            db.IUD(q);
            return RedirectToAction("ViewEnrollment");
        }
    }
}