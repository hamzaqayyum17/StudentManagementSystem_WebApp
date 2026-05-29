using Microsoft.AspNetCore.Mvc;
using StudentManagementSystem.Models;
using System.Data;

namespace StudentManagementSystem.Controllers
{
    public class StudentController : BaseController
    {
        DBAccess db = new DBAccess();

        // ================= SIGN IN =================
        [HttpGet]
        public IActionResult SignIn()
        {
            if (HttpContext.Session.GetString("sid") != null)
            {
                string role = HttpContext.Session.GetString("role");
                if (role == "admin") return RedirectToAction("AdminDashboard");
                if (role == "student") return RedirectToAction("StudentDashboard");
            }
            return View();
        }

        [HttpPost]
        public IActionResult SignIn(string sid, string password)
        {
            string q = "select sid,name,role from Student " +
                       "where sid='" + sid + "' and password='" + password + "'";
            DataTable dt = db.GetDataTable(q);

            if (dt.Rows.Count > 0)
            {
                HttpContext.Session.SetString("sid", dt.Rows[0]["sid"].ToString());
                HttpContext.Session.SetString("name", dt.Rows[0]["name"].ToString());
                HttpContext.Session.SetString("role", dt.Rows[0]["role"].ToString());

                return dt.Rows[0]["role"].ToString() == "admin"
                    ? RedirectToAction("AdminDashboard")
                    : RedirectToAction("StudentDashboard");
            }

            ViewBag.Error = "Invalid ID or Password";
            return View();
        }

        // ================= LOGOUT =================
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("SignIn");
        }

        // ================= ADMIN DASHBOARD =================
        public IActionResult AdminDashboard()
        {
            if (!IsAdmin()) return RedirectToAction("SignIn");

            ViewBag.Name = HttpContext.Session.GetString("name");
            ViewBag.TotalStudents = db.GetDataTable("select count(*) from Student where role='student'").Rows[0][0];
            ViewBag.TotalTeachers = db.GetDataTable("select count(*) from Teacher").Rows[0][0];
            ViewBag.TotalCourses = db.GetDataTable("select count(*) from Course").Rows[0][0];
            ViewBag.TotalEnrollments = db.GetDataTable("select count(*) from Enrollment").Rows[0][0];
            ViewBag.UnpaidFees = db.GetDataTable("select count(*) from Fee where status='Unpaid'").Rows[0][0];
            ViewBag.TotalClasses = db.GetDataTable("select count(*) from Class").Rows[0][0];

            return View();
        }

        // ================= STUDENT DASHBOARD =================
        public IActionResult StudentDashboard()
        {
            if (!IsStudent()) return RedirectToAction("SignIn");

            string sid = HttpContext.Session.GetString("sid");
            ViewBag.Name = HttpContext.Session.GetString("name");

            // Attendance summary
            string qA = @"select 
                            count(*) as total,
                            sum(case when status='Present' then 1 else 0 end) as present
                          from Attendance where sid='" + sid + "'";
            DataTable dtA = db.GetDataTable(qA);
            int total = int.Parse(dtA.Rows[0]["total"].ToString());
            int present = int.Parse(dtA.Rows[0]["present"].ToString());
            ViewBag.AttPct = total > 0 ? (int)((present / (double)total) * 100) : 0;

            // Fee pending
            ViewBag.UnpaidFees = db.GetDataTable(
                "select count(*) from Fee where sid='" + sid + "' and status='Unpaid'").Rows[0][0];

            // Enrolled courses
            ViewBag.TotalCourses = db.GetDataTable(
                "select count(*) from Enrollment where sid='" + sid + "'").Rows[0][0];

            return View();
        }

        // ================= ALL STUDENTS =================
        [HttpGet]
        public IActionResult AllStudents(string search, int? classId, int? sectionId)
        {
            if (!IsAdmin()) return RedirectToAction("SignIn");

            ViewBag.Search = search;
            ViewBag.ClassId = classId;
            ViewBag.SectionId = sectionId;
            ViewBag.Classes = db.GetDataTable("select * from Class order by classId");

            if (classId.HasValue)
                ViewBag.Sections = db.GetDataTable(
                    "select * from Section where classId=" + classId.Value);

            string q = @"select s.sid, s.name, s.city, s.email,
                                s.rollNumber, cl.className, sec.sectionName
                         from Student s
                         left join Class   cl  on s.classId   = cl.classId
                         left join Section sec on s.sectionId = sec.sectionId
                         where s.role='student'";

            if (!string.IsNullOrEmpty(search))
                q += " and s.name like '%" + search + "%'";
            if (classId.HasValue)
                q += " and s.classId=" + classId.Value;
            if (sectionId.HasValue)
                q += " and s.sectionId=" + sectionId.Value;

            q += " order by cl.classId, sec.sectionName, s.rollNumber";

            DataTable dt = db.GetDataTable(q);
            List<Student> list = new List<Student>();

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new Student
                {
                    sid = row["sid"].ToString(),
                    name = row["name"].ToString(),
                    city = row["city"].ToString(),
                    email = row["email"].ToString(),
                    rollNumber = row["rollNumber"].ToString(),
                    className = row["className"].ToString(),
                    sectionName = row["sectionName"].ToString()
                });
            }

            return View(list);
        }

        // ================= ADD STUDENT =================
        [HttpGet]
        public IActionResult SignUp()
        {
            if (!IsAdmin()) return RedirectToAction("SignIn");
            ViewBag.Classes = db.GetDataTable("select * from Class order by classId");
            return View();
        }

        [HttpPost]
        public IActionResult SignUp(Student s)
        {
            if (!IsAdmin()) return RedirectToAction("SignIn");

            try
            {
                if (string.IsNullOrEmpty(s.email))
                {
                    ViewBag.Msg = "Email is required";
                    ViewBag.Classes = db.GetDataTable("select * from Class order by classId");
                    return View(s);
                }

                s.sid = s.email.Split('@')[0];
                s.role = "student";

                string q = "insert into Student(sid,name,city,email,password,role,rollNumber,classId,sectionId)" +
                           " values('" + s.sid + "','" + s.name + "','" + s.city + "','" +
                           s.email + "','" + s.password + "','student','" + s.rollNumber +
                           "'," + s.classId + "," + s.sectionId + ")";
                db.IUD(q);

                TempData["msg"] = "Student Added Successfully!";
                return RedirectToAction("AllStudents");
            }
            catch (Exception ex)
            {
                ViewBag.Msg = ex.Message;
                ViewBag.Classes = db.GetDataTable("select * from Class order by classId");
                return View(s);
            }
        }

        // ================= EDIT STUDENT =================
        [HttpGet]
        public IActionResult Edit(string sid)
        {
            if (!IsAdmin()) return RedirectToAction("SignIn");

            string q = @"select s.*, cl.className, sec.sectionName
                         from Student s
                         left join Class   cl  on s.classId   = cl.classId
                         left join Section sec on s.sectionId = sec.sectionId
                         where s.sid='" + sid + "'";
            DataTable dt = db.GetDataTable(q);

            Student s2 = new Student();
            if (dt.Rows.Count > 0)
            {
                s2.sid = dt.Rows[0]["sid"].ToString();
                s2.name = dt.Rows[0]["name"].ToString();
                s2.city = dt.Rows[0]["city"].ToString();
                s2.email = dt.Rows[0]["email"].ToString();
                s2.rollNumber = dt.Rows[0]["rollNumber"].ToString();
                s2.classId = dt.Rows[0]["classId"] == DBNull.Value ? null : (int?)int.Parse(dt.Rows[0]["classId"].ToString());
                s2.sectionId = dt.Rows[0]["sectionId"] == DBNull.Value ? null : (int?)int.Parse(dt.Rows[0]["sectionId"].ToString());
                s2.className = dt.Rows[0]["className"].ToString();
                s2.sectionName = dt.Rows[0]["sectionName"].ToString();
            }

            ViewBag.Classes = db.GetDataTable("select * from Class order by classId");
            if (s2.classId.HasValue)
                ViewBag.Sections = db.GetDataTable(
                    "select * from Section where classId=" + s2.classId.Value);

            return View(s2);
        }

        [HttpPost]
        public IActionResult Edit(Student s)
        {
            string q = "update Student set name='" + s.name + "', city='" + s.city +
                       "', email='" + s.email + "', rollNumber='" + s.rollNumber +
                       "', classId=" + s.classId + ", sectionId=" + s.sectionId +
                       " where sid='" + s.sid + "'";
            db.IUD(q);

            TempData["msg"] = "Student Updated!";
            return RedirectToAction("AllStudents");
        }

        // ================= DELETE STUDENT =================
        public IActionResult Delete(string sid)
        {
            if (!IsAdmin()) return RedirectToAction("SignIn");

            db.IUD("delete from Student where sid='" + sid + "'");
            TempData["msg"] = "Student Deleted!";
            return RedirectToAction("AllStudents");
        }

        // ================= DETAIL =================
        public IActionResult Detail(string sid)
        {
            if (!IsAdmin()) return RedirectToAction("SignIn");

            string q = @"select s.*, cl.className, sec.sectionName
                         from Student s
                         left join Class   cl  on s.classId   = cl.classId
                         left join Section sec on s.sectionId = sec.sectionId
                         where s.sid='" + sid + "'";
            DataTable dt = db.GetDataTable(q);

            Student s2 = new Student();
            if (dt.Rows.Count > 0)
            {
                s2.sid = dt.Rows[0]["sid"].ToString();
                s2.name = dt.Rows[0]["name"].ToString();
                s2.city = dt.Rows[0]["city"].ToString();
                s2.email = dt.Rows[0]["email"].ToString();
                s2.rollNumber = dt.Rows[0]["rollNumber"].ToString();
                s2.className = dt.Rows[0]["className"].ToString();
                s2.sectionName = dt.Rows[0]["sectionName"].ToString();
            }

            return View(s2);
        }

        // ================= MY PROFILE =================
        public IActionResult MyProfile()
        {
            if (!IsLoggedIn()) return RedirectToAction("SignIn");

            string sid = HttpContext.Session.GetString("sid");
            string q = @"select s.*, cl.className, sec.sectionName
                         from Student s
                         left join Class   cl  on s.classId   = cl.classId
                         left join Section sec on s.sectionId = sec.sectionId
                         where s.sid='" + sid + "'";
            DataTable dt = db.GetDataTable(q);

            Student s2 = new Student();
            if (dt.Rows.Count > 0)
            {
                s2.sid = dt.Rows[0]["sid"].ToString();
                s2.name = dt.Rows[0]["name"].ToString();
                s2.city = dt.Rows[0]["city"].ToString();
                s2.email = dt.Rows[0]["email"].ToString();
                s2.rollNumber = dt.Rows[0]["rollNumber"].ToString();
                s2.className = dt.Rows[0]["className"].ToString();
                s2.sectionName = dt.Rows[0]["sectionName"].ToString();
            }

            return View(s2);
        }

        // ================= MY COURSES =================
        public IActionResult MyCourses()
        {
            if (!IsLoggedIn()) return RedirectToAction("SignIn");

            string sid = HttpContext.Session.GetString("sid");
            string q = "select c.cname, c.creditHours, e.grade " +
                       "from Enrollment e " +
                       "join Course c on e.cid = c.cid " +
                       "where e.sid='" + sid + "' " +
                       "order by c.cname";

            return View(db.GetDataTable(q));
        }

        // ================= GET SECTIONS (AJAX) =================
        public IActionResult GetSections(int classId)
        {
            DataTable dt = db.GetDataTable(
                "select sectionId, sectionName from Section where classId=" + classId);

            var list = new List<object>();
            foreach (DataRow row in dt.Rows)
                list.Add(new
                {
                    sectionId = row["sectionId"].ToString(),
                    sectionName = row["sectionName"].ToString()
                });

            return Json(list);
        }
    }
}