using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using StudentManagementSystem.Models;
using System.Data;

namespace StudentManagementSystem.Controllers
{
    public class ReportController : BaseController
    {
        DBAccess db = new DBAccess();

        // ================= RESULT CARD PDF =================
        public IActionResult ResultCard(string sid)
        {
            if (!IsLoggedIn())
                return RedirectToAction("SignIn", "Student");

            // ── Student Info ──
            string qS = @"select s.sid, s.name, s.email, s.city,
                                 s.rollNumber, cl.className, sec.sectionName
                          from Student s
                          left join Class cl  on s.classId   = cl.classId
                          left join Section sec on s.sectionId = sec.sectionId
                          where s.sid='" + sid + "'";
            DataTable dtS = db.GetDataTable(qS);

            if (dtS.Rows.Count == 0)
                return NotFound();

            // ── Enrollments + Grades ──
            string qE = @"select c.cname, c.creditHours, e.grade
                          from Enrollment e
                          join Course c on e.cid = c.cid
                          where e.sid='" + sid + "'";
            DataTable dtE = db.GetDataTable(qE);

            // ── PDF Generate ──
            using var ms = new MemoryStream();

            Document doc = new Document(PageSize.A4, 40, 40, 50, 50);
            PdfWriter.GetInstance(doc, ms);
            doc.Open();

            // ── Colors ──
            BaseColor darkBlue = new BaseColor(30, 41, 59);
            BaseColor accent = new BaseColor(99, 102, 241);
            BaseColor lightGray = new BaseColor(248, 250, 252);
            BaseColor green = new BaseColor(16, 185, 129);
            BaseColor red = new BaseColor(239, 68, 68);
            BaseColor white = new BaseColor(255, 255, 255);
            BaseColor border = new BaseColor(226, 232, 240);

            // ── Fonts ──
            Font fontTitle = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 22, white);

            Font fontSub = FontFactory.GetFont(
                FontFactory.HELVETICA,
                10,
                new BaseColor(148, 163, 184));

            Font fontLabel = FontFactory.GetFont(
                FontFactory.HELVETICA_BOLD,
                9,
                new BaseColor(100, 116, 139));

            Font fontValue = FontFactory.GetFont(
                FontFactory.HELVETICA_BOLD,
                10,
                darkBlue);

            Font fontTh = FontFactory.GetFont(
                FontFactory.HELVETICA_BOLD,
                9,
                white);

            Font fontTd = FontFactory.GetFont(
                FontFactory.HELVETICA,
                10,
                darkBlue);

            Font fontGrade = FontFactory.GetFont(
                FontFactory.HELVETICA_BOLD,
                10,
                accent);

            Font fontFooter = FontFactory.GetFont(
                FontFactory.HELVETICA,
                8,
                new BaseColor(148, 163, 184));

            // ══════════════════════════════════════
            //  HEADER BANNER
            // ══════════════════════════════════════
            PdfPTable header = new PdfPTable(1);
            header.WidthPercentage = 100;
            header.SpacingAfter = 20;

            PdfPCell hCell = new PdfPCell();
            hCell.BackgroundColor = darkBlue;
            hCell.Border = Rectangle.NO_BORDER;
            hCell.Padding = 20;

            hCell.AddElement(new Paragraph("EduTrack SMS", fontTitle));
            hCell.AddElement(new Paragraph("Student Result Card", fontSub));

            header.AddCell(hCell);
            doc.Add(header);

            // ══════════════════════════════════════
            //  STUDENT INFO TABLE
            // ══════════════════════════════════════
            var row0 = dtS.Rows[0];
            string studentName = row0["name"].ToString();
            string rollNo = row0["rollNumber"]?.ToString() ?? "-";
            string className = row0["className"]?.ToString() ?? "-";
            string section = row0["sectionName"]?.ToString() ?? "-";
            string email = row0["email"].ToString();

            PdfPTable infoTable = new PdfPTable(2);
            infoTable.WidthPercentage = 100;
            infoTable.SetWidths(new float[] { 1f, 1f });
            infoTable.SpacingAfter = 20;

            void AddInfoRow(string label, string value)
            {
                PdfPCell lCell = new PdfPCell(new Phrase(label, fontLabel));
                lCell.Border = Rectangle.BOTTOM_BORDER;
                lCell.BorderColor = border;
                lCell.BackgroundColor = lightGray;
                lCell.Padding = 10;
                infoTable.AddCell(lCell);

                PdfPCell vCell = new PdfPCell(new Phrase(value, fontValue));
                vCell.Border = Rectangle.BOTTOM_BORDER;
                vCell.BorderColor = border;
                vCell.Padding = 10;
                infoTable.AddCell(vCell);
            }

            AddInfoRow("STUDENT NAME", studentName);
            AddInfoRow("ROLL NUMBER", rollNo);
            AddInfoRow("CLASS", className);
            AddInfoRow("SECTION", section);
            AddInfoRow("EMAIL", email);
            AddInfoRow("DATE", DateTime.Now.ToString("dd MMMM yyyy"));

            doc.Add(infoTable);

            // ══════════════════════════════════════
            //  RESULTS TABLE
            // ══════════════════════════════════════
            Paragraph resultHeading = new Paragraph("Academic Results",
                FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 13, darkBlue));
            resultHeading.SpacingAfter = 10;
            doc.Add(resultHeading);

            PdfPTable resultTable = new PdfPTable(3);
            resultTable.WidthPercentage = 100;
            resultTable.SetWidths(new float[] { 3f, 1.5f, 1f });
            resultTable.SpacingAfter = 20;

            // Header row
            string[] headers = { "COURSE NAME", "CREDIT HOURS", "GRADE" };
            foreach (var h in headers)
            {
                PdfPCell thCell = new PdfPCell(new Phrase(h, fontTh));
                thCell.BackgroundColor = accent;
                thCell.Border = Rectangle.NO_BORDER;
                thCell.Padding = 10;
                thCell.HorizontalAlignment = Element.ALIGN_CENTER;
                resultTable.AddCell(thCell);
            }

            // Grade color helper
            BaseColor GetGradeColor(string g) =>
                g is "A+" or "A" or "A-" ? green :
                g is "B+" or "B" or "B-" ? accent :
                g is "C+" or "C" or "C-" ? new BaseColor(245, 158, 11) : red;

            int totalCredits = 0;
            int passedCredits = 0;
            bool alternate = false;

            foreach (DataRow er in dtE.Rows)
            {
                string cname = er["cname"].ToString();
                string credits = er["creditHours"].ToString();
                string grade = er["grade"].ToString();
                int ch = int.Parse(credits);

                totalCredits += ch;
                if (grade != "F") passedCredits += ch;

                BaseColor rowBg = alternate ? lightGray : white;
                alternate = !alternate;

                PdfPCell c1 = new PdfPCell(new Phrase(cname, fontTd));
                c1.BackgroundColor = rowBg;
                c1.Border = Rectangle.BOTTOM_BORDER;
                c1.BorderColor = border;
                c1.Padding = 10;

                PdfPCell c2 = new PdfPCell(new Phrase(credits + " hrs", fontTd));
                c2.BackgroundColor = rowBg;
                c2.Border = Rectangle.BOTTOM_BORDER;
                c2.BorderColor = border;
                c2.Padding = 10;
                c2.HorizontalAlignment = Element.ALIGN_CENTER;

                Font gFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, GetGradeColor(grade));
                PdfPCell c3 = new PdfPCell(new Phrase(grade, gFont));
                c3.BackgroundColor = rowBg;
                c3.Border = Rectangle.BOTTOM_BORDER;
                c3.BorderColor = border;
                c3.Padding = 10;
                c3.HorizontalAlignment = Element.ALIGN_CENTER;

                resultTable.AddCell(c1);
                resultTable.AddCell(c2);
                resultTable.AddCell(c3);
            }

            doc.Add(resultTable);

            // ══════════════════════════════════════
            //  SUMMARY BOX
            // ══════════════════════════════════════
            PdfPTable summary = new PdfPTable(3);
            summary.WidthPercentage = 100;
            summary.SpacingAfter = 30;

            void AddSummaryCell(string label, string value, BaseColor bg, BaseColor fg)
            {
                PdfPCell sc = new PdfPCell();
                sc.BackgroundColor = bg;
                sc.Border = Rectangle.NO_BORDER;
                sc.Padding = 14;
                sc.HorizontalAlignment = Element.ALIGN_CENTER;
                sc.AddElement(new Paragraph(label,
                    FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8, new BaseColor(100, 116, 139))));
                sc.AddElement(new Paragraph(value,
                    FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16, fg)));
                summary.AddCell(sc);
            }

            bool passed = passedCredits == totalCredits;
            AddSummaryCell("TOTAL CREDITS", totalCredits.ToString(), lightGray, darkBlue);
            AddSummaryCell("PASSED CREDITS", passedCredits.ToString(), lightGray, green);
            AddSummaryCell("STATUS",
                passed ? "PASS" : "FAIL",
                passed ? new BaseColor(220, 252, 231) : new BaseColor(254, 226, 226),
                passed ? green : red);

            doc.Add(summary);

            // ══════════════════════════════════════
            //  FOOTER
            // ══════════════════════════════════════
            PdfPTable footer = new PdfPTable(1);
            footer.WidthPercentage = 100;

            PdfPCell fCell = new PdfPCell(
                new Phrase("Generated by EduTrack SMS  •  " +
                           DateTime.Now.ToString("dd MMM yyyy, hh:mm tt") +
                           "  •  This is a computer generated document.",
                           fontFooter));
            fCell.Border = Rectangle.TOP_BORDER;
            fCell.BorderColor = border;
            fCell.Padding = 12;
            fCell.HorizontalAlignment = Element.ALIGN_CENTER;
            footer.AddCell(fCell);
            doc.Add(footer);

            doc.Close();

            byte[] bytes = ms.ToArray();
            string fileName = $"ResultCard_{studentName.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd}.pdf";

            return File(bytes, "application/pdf", fileName);
        }

        // ================= STUDENT PROGRESS REPORT =================
        public IActionResult ProgressReport(string sid)
        {
            if (!IsLoggedIn())
                return RedirectToAction("SignIn", "Student");

            // ── Student Info ──
            string qS = @"select s.sid, s.name, s.email, s.city,
                         s.rollNumber, cl.className, sec.sectionName
                  from Student s
                  left join Class cl    on s.classId   = cl.classId
                  left join Section sec on s.sectionId = sec.sectionId
                  where s.sid='" + sid + "'";
            DataTable dtStudent = db.GetDataTable(qS);

            if (dtStudent.Rows.Count == 0)
                return NotFound();

            // ── Enrollments + Grades ──
            string qE = @"select c.cname, e.grade
                  from Enrollment e
                  join Course c on e.cid = c.cid
                  where e.sid='" + sid + "'";
            DataTable dtEnrollments = db.GetDataTable(qE);

            // ── Attendance Summary ──
            string qA = @"select 
                    count(*) as total,
                    sum(case when status='Present' then 1 else 0 end) as present,
                    sum(case when status='Absent'  then 1 else 0 end) as absent,
                    sum(case when status='Late'    then 1 else 0 end) as late
                  from Attendance
                  where sid='" + sid + "'";
            DataTable dtAtt = db.GetDataTable(qA);

            // ── Fee Summary ──
            string qF = @"select
                    count(*) as total,
                    sum(case when status='Paid'   then 1 else 0 end) as paid,
                    sum(case when status='Unpaid' then 1 else 0 end) as unpaid,
                    sum(amount) as totalAmount,
                    sum(case when status='Paid'   then amount else 0 end) as paidAmount,
                    sum(case when status='Unpaid' then amount else 0 end) as unpaidAmount
                  from Fee
                  where sid='" + sid + "'";
            DataTable dtFee = db.GetDataTable(qF);

            // ── ViewBag mein sab pass karo ──
            var sr = dtStudent.Rows[0];
            ViewBag.StudentName = sr["name"].ToString();
            ViewBag.RollNumber = sr["rollNumber"]?.ToString() ?? "-";
            ViewBag.ClassName = sr["className"]?.ToString() ?? "-";
            ViewBag.SectionName = sr["sectionName"]?.ToString() ?? "-";
            ViewBag.Email = sr["email"].ToString();
            ViewBag.Sid = sid;

            // Attendance
            var ar = dtAtt.Rows[0];
            int total = int.Parse(ar["total"].ToString());
            int present = int.Parse(ar["present"].ToString());
            int absent = int.Parse(ar["absent"].ToString());
            int late = int.Parse(ar["late"].ToString());
            int attPct = total > 0 ? (int)((present / (double)total) * 100) : 0;

            ViewBag.AttTotal = total;
            ViewBag.AttPresent = present;
            ViewBag.AttAbsent = absent;
            ViewBag.AttLate = late;
            ViewBag.AttPct = attPct;

            // Fee
            var fr = dtFee.Rows[0];
            ViewBag.FeePaid = fr["paid"].ToString();
            ViewBag.FeeUnpaid = fr["unpaid"].ToString();
            ViewBag.FeeTotalAmount = decimal.Parse(fr["totalAmount"] == DBNull.Value ? "0" : fr["totalAmount"].ToString());
            ViewBag.FeePaidAmount = decimal.Parse(fr["paidAmount"] == DBNull.Value ? "0" : fr["paidAmount"].ToString());
            ViewBag.FeeUnpaidAmount = decimal.Parse(fr["unpaidAmount"] == DBNull.Value ? "0" : fr["unpaidAmount"].ToString());

            // Enrollments
            ViewBag.Enrollments = dtEnrollments;

            return View();
        }
    }
}