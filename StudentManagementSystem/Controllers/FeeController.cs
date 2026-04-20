using Microsoft.AspNetCore.Mvc;
using StudentManagementSystem.Models;
using System.Data;

namespace StudentManagementSystem.Controllers
{
    public class FeeController : BaseController
    {
        DBAccess db = new DBAccess();

        // ================= ADD FEE =================
        [HttpGet]
        public IActionResult AddFee()
        {
            if (!IsAdmin())
                return RedirectToAction("SignIn", "Student");

            // Students ki list dropdown ke liye
            string q = "select sid, name from Student where role='student'";
            DataTable dt = db.GetDataTable(q);
            ViewBag.Students = dt;

            return View();
        }

        [HttpPost]
        public IActionResult AddFee(Fee f)
        {
            if (!IsAdmin())
                return RedirectToAction("SignIn", "Student");

            string q = "insert into Fee(sid, month, amount, status)" +
                       " values('" + f.sid + "','" + f.month + "','" +
                       f.amount + "','Unpaid')";
            db.IUD(q);

            TempData["msg"] = "Fee Record Added!";
            return RedirectToAction("ViewFees");
        }

        // ================= VIEW ALL FEES (Admin) =================
        public IActionResult ViewFees()
        {
            if (!IsAdmin())
                return RedirectToAction("SignIn", "Student");

            List<Fee> list = new List<Fee>();

            string q = @"select f.fid, s.name as studentName, 
                                f.month, f.amount, 
                                f.status, f.paidDate
                         from Fee f
                         join Student s on f.sid = s.sid
                         order by f.fid desc";

            DataTable dt = db.GetDataTable(q);

            foreach (DataRow row in dt.Rows)
            {
                Fee f = new Fee();
                f.fid = int.Parse(row["fid"].ToString());
                f.studentName = row["studentName"].ToString();
                f.month = row["month"].ToString();
                f.amount = decimal.Parse(row["amount"].ToString());
                f.status = row["status"].ToString();
                f.paidDate = row["paidDate"] == DBNull.Value
                                ? null
                                : DateTime.Parse(row["paidDate"].ToString());
                list.Add(f);
            }

            return View(list);
        }

        // ================= MARK AS PAID =================
        public IActionResult MarkPaid(int fid)
        {
            if (!IsAdmin())
                return RedirectToAction("SignIn", "Student");

            string date = DateTime.Now.ToString("yyyy-MM-dd");
            string q = "update Fee set status='Paid', paidDate='" + date +
                       "' where fid=" + fid;
            db.IUD(q);

            TempData["msg"] = "Fee Marked as Paid!";
            return RedirectToAction("ViewFees");
        }

        // ================= DELETE FEE =================
        public IActionResult DeleteFee(int fid)
        {
            if (!IsAdmin())
                return RedirectToAction("SignIn", "Student");

            string q = "delete from Fee where fid=" + fid;
            db.IUD(q);

            TempData["msg"] = "Fee Record Deleted!";
            return RedirectToAction("ViewFees");
        }

        // ================= MY FEES (Student) =================
        public IActionResult MyFees()
        {
            if (HttpContext.Session.GetString("role") != "student")
                return RedirectToAction("SignIn", "Student");

            string sid = HttpContext.Session.GetString("sid");

            string q = @"select fid, month, amount, 
                                status, paidDate
                         from Fee
                         where sid='" + sid + @"'
                         order by fid desc";

            DataTable dt = db.GetDataTable(q);

            // Summary
            decimal totalFee = 0;
            decimal paidFee = 0;
            decimal unpaidFee = 0;

            List<Fee> list = new List<Fee>();

            foreach (DataRow row in dt.Rows)
            {
                Fee f = new Fee();
                f.fid = int.Parse(row["fid"].ToString());
                f.month = row["month"].ToString();
                f.amount = decimal.Parse(row["amount"].ToString());
                f.status = row["status"].ToString();
                f.paidDate = row["paidDate"] == DBNull.Value
                             ? null
                             : DateTime.Parse(row["paidDate"].ToString());
                list.Add(f);

                totalFee += f.amount;
                if (f.status == "Paid") paidFee += f.amount;
                else unpaidFee += f.amount;
            }

            ViewBag.TotalFee = totalFee;
            ViewBag.PaidFee = paidFee;
            ViewBag.UnpaidFee = unpaidFee;

            return View(list);
        }

        // ================= UNPAID LIST (Admin) =================
        public IActionResult UnpaidFees()
        {
            if (!IsAdmin())
                return RedirectToAction("SignIn", "Student");

            List<Fee> list = new List<Fee>();

            string q = @"select f.fid, s.name as studentName,
                                f.month, f.amount, f.status
                         from Fee f
                         join Student s on f.sid = s.sid
                         where f.status = 'Unpaid'
                         order by f.fid desc";

            DataTable dt = db.GetDataTable(q);

            foreach (DataRow row in dt.Rows)
            {
                Fee f = new Fee();
                f.fid = int.Parse(row["fid"].ToString());
                f.studentName = row["studentName"].ToString();
                f.month = row["month"].ToString();
                f.amount = decimal.Parse(row["amount"].ToString());
                f.status = row["status"].ToString();
                list.Add(f);
            }

            return View(list);
        }
    }
}