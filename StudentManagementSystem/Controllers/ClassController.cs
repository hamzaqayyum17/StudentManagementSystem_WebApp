using Microsoft.AspNetCore.Mvc;
using StudentManagementSystem.Models;
using System.Data;

namespace StudentManagementSystem.Controllers
{
    public class ClassController : BaseController
    {
        DBAccess db = new DBAccess();

        // ================= VIEW ALL CLASSES =================
        public IActionResult ViewClasses()
        {
            if (!IsAdmin())
                return RedirectToAction("SignIn", "Student");

            List<Class> list = new List<Class>();
            DataTable dt = db.GetDataTable("select * from Class order by classId");

            foreach (DataRow row in dt.Rows)
            {
                Class c = new Class();
                c.classId = int.Parse(row["classId"].ToString());
                c.className = row["className"].ToString();
                list.Add(c);
            }
            return View(list);
        }

        // ================= ADD CLASS =================
        [HttpGet]
        public IActionResult AddClass()
        {
            if (!IsAdmin())
                return RedirectToAction("SignIn", "Student");
            return View();
        }

        [HttpPost]
        public IActionResult AddClass(Class c)
        {
            if (!IsAdmin())
                return RedirectToAction("SignIn", "Student");

            string q = "insert into Class(className) values('" + c.className + "')";
            db.IUD(q);
            TempData["msg"] = "Class Added Successfully!";
            return RedirectToAction("ViewClasses");
        }

        // ================= DELETE CLASS =================
        public IActionResult DeleteClass(int classId)
        {
            if (!IsAdmin())
                return RedirectToAction("SignIn", "Student");

            db.IUD("delete from Class where classId=" + classId);
            TempData["msg"] = "Class Deleted!";
            return RedirectToAction("ViewClasses");
        }

        // ================= EDIT CLASS =================
        [HttpGet]
        public IActionResult EditClass(int classId)
        {
            if (!IsAdmin())
                return RedirectToAction("SignIn", "Student");

            DataTable dt = db.GetDataTable("select * from Class where classId=" + classId);
            Class c = new Class();
            if (dt.Rows.Count > 0)
            {
                c.classId = int.Parse(dt.Rows[0]["classId"].ToString());
                c.className = dt.Rows[0]["className"].ToString();
            }
            return View(c);
        }

        [HttpPost]
        public IActionResult EditClass(Class c)
        {
            string q = "update Class set className='" + c.className + "' where classId=" + c.classId;
            db.IUD(q);
            TempData["msg"] = "Class Updated!";
            return RedirectToAction("ViewClasses");
        }

        // ================= VIEW SECTIONS =================
        public IActionResult ViewSections(int classId)
        {
            if (!IsAdmin())
                return RedirectToAction("SignIn", "Student");

            // Class info
            DataTable dtC = db.GetDataTable("select * from Class where classId=" + classId);
            if (dtC.Rows.Count > 0)
                ViewBag.ClassName = dtC.Rows[0]["className"].ToString();
            ViewBag.ClassId = classId;

            // Sections
            List<Section> list = new List<Section>();
            string q = @"select s.sectionId, s.sectionName, s.classId, c.className
                         from Section s
                         join Class c on s.classId = c.classId
                         where s.classId=" + classId;

            DataTable dt = db.GetDataTable(q);
            foreach (DataRow row in dt.Rows)
            {
                Section s = new Section();
                s.sectionId = int.Parse(row["sectionId"].ToString());
                s.sectionName = row["sectionName"].ToString();
                s.classId = int.Parse(row["classId"].ToString());
                s.className = row["className"].ToString();
                list.Add(s);
            }
            return View(list);
        }

        // ================= ADD SECTION =================
        [HttpPost]
        public IActionResult AddSection(string sectionName, int classId)
        {
            if (!IsAdmin())
                return RedirectToAction("SignIn", "Student");

            string q = "insert into Section(sectionName, classId) values('" +
                       sectionName + "'," + classId + ")";
            db.IUD(q);
            TempData["msg"] = "Section Added!";
            return RedirectToAction("ViewSections", new { classId });
        }

        // ================= DELETE SECTION =================
        public IActionResult DeleteSection(int sectionId, int classId)
        {
            if (!IsAdmin())
                return RedirectToAction("SignIn", "Student");

            db.IUD("delete from Section where sectionId=" + sectionId);
            TempData["msg"] = "Section Deleted!";
            return RedirectToAction("ViewSections", new { classId });
        }

        // ================= CLASS STUDENTS =================
        public IActionResult ClassStudents(int classId, int? sectionId)
        {
            if (!IsAdmin())
                return RedirectToAction("SignIn", "Student");

            // Class + Section info
            DataTable dtC = db.GetDataTable("select * from Class where classId=" + classId);
            if (dtC.Rows.Count > 0)
                ViewBag.ClassName = dtC.Rows[0]["className"].ToString();
            ViewBag.ClassId = classId;

            // Sections dropdown
            ViewBag.Sections = db.GetDataTable(
                "select * from Section where classId=" + classId);
            ViewBag.SelectedSection = sectionId;

            // Students
            string q = @"select s.sid, s.name, s.rollNumber, s.email,
                                sec.sectionName
                         from Student s
                         left join Section sec on s.sectionId = sec.sectionId
                         where s.classId=" + classId + " and s.role='student'";

            if (sectionId.HasValue)
                q += " and s.sectionId=" + sectionId.Value;

            q += " order by s.rollNumber";

            List<Student> list = new List<Student>();
            DataTable dt = db.GetDataTable(q);
            foreach (DataRow row in dt.Rows)
            {
                Student st = new Student();
                st.sid = row["sid"].ToString();
                st.name = row["name"].ToString();
                st.rollNumber = row["rollNumber"].ToString();
                st.email = row["email"].ToString();
                st.sectionName = row["sectionName"].ToString();
                list.Add(st);
            }
            return View(list);
        }
    }
}