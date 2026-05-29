using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace StudentManagementSystem.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string toName,
                                         string subject, string body)
        {
            var email = new MimeMessage();

            email.From.Add(new MailboxAddress(
                _config["EmailSettings:DisplayName"],
                _config["EmailSettings:Email"]));

            email.To.Add(new MailboxAddress(toName, toEmail));
            email.Subject = subject;

            email.Body = new TextPart("html") { Text = body };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(
                _config["EmailSettings:Host"],
                int.Parse(_config["EmailSettings:Port"]),
                SecureSocketOptions.StartTls);

            await smtp.AuthenticateAsync(
                _config["EmailSettings:Email"],
                _config["EmailSettings:Password"]);

            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }

        // ── Fee Reminder Email ──
        public async Task SendFeeReminderAsync(string toEmail, string name,
                                                string month, decimal amount)
        {
            string subject = $"Fee Reminder — {month}";
            string body = $@"
                <div style='font-family:Arial,sans-serif;max-width:500px;margin:auto;'>
                    <div style='background:#1e293b;padding:20px;border-radius:10px 10px 0 0;'>
                        <h2 style='color:#fff;margin:0;'>EduTrack SMS</h2>
                        <p style='color:#94a3b8;margin:4px 0 0;'>Fee Reminder</p>
                    </div>
                    <div style='background:#f8fafc;padding:24px;border:1px solid #e2e8f0;'>
                        <p style='color:#0f172a;'>Dear <strong>{name}</strong>,</p>
                        <p style='color:#475569;'>This is a reminder that your fee for
                           <strong>{month}</strong> is pending.</p>
                        <div style='background:#fef2f2;border:1px solid #fecaca;
                                    border-radius:8px;padding:16px;margin:16px 0;'>
                            <p style='margin:0;color:#dc2626;font-size:18px;font-weight:700;'>
                                Amount Due: Rs. {amount:N0}
                            </p>
                        </div>
                        <p style='color:#475569;'>Please pay your fee as soon as possible
                           to avoid any inconvenience.</p>
                    </div>
                    <div style='background:#f1f5f9;padding:12px;border-radius:0 0 10px 10px;
                                text-align:center;'>
                        <p style='color:#94a3b8;font-size:12px;margin:0;'>
                            EduTrack Student Management System
                        </p>
                    </div>
                </div>";

            await SendEmailAsync(toEmail, name, subject, body);
        }

        // ── Result Published Email ──
        public async Task SendResultEmailAsync(string toEmail, string name,
                                                string courseName, string grade)
        {
            string subject = $"Result Published — {courseName}";
            string gradeColor = grade is "A+" or "A" or "A-" ? "#10b981" :
                                grade is "B+" or "B" or "B-" ? "#6366f1" :
                                grade is "F" ? "#ef4444" : "#f59e0b";
            string body = $@"
                <div style='font-family:Arial,sans-serif;max-width:500px;margin:auto;'>
                    <div style='background:#1e293b;padding:20px;border-radius:10px 10px 0 0;'>
                        <h2 style='color:#fff;margin:0;'>EduTrack SMS</h2>
                        <p style='color:#94a3b8;margin:4px 0 0;'>Result Notification</p>
                    </div>
                    <div style='background:#f8fafc;padding:24px;border:1px solid #e2e8f0;'>
                        <p style='color:#0f172a;'>Dear <strong>{name}</strong>,</p>
                        <p style='color:#475569;'>Your result for
                           <strong>{courseName}</strong> has been published.</p>
                        <div style='background:#f0fdf4;border:1px solid #bbf7d0;
                                    border-radius:8px;padding:16px;margin:16px 0;
                                    text-align:center;'>
                            <p style='margin:0;color:#64748b;font-size:13px;'>Your Grade</p>
                            <p style='margin:4px 0 0;color:{gradeColor};
                                      font-size:36px;font-weight:700;'>{grade}</p>
                        </div>
                    </div>
                    <div style='background:#f1f5f9;padding:12px;border-radius:0 0 10px 10px;
                                text-align:center;'>
                        <p style='color:#94a3b8;font-size:12px;margin:0;'>
                            EduTrack Student Management System
                        </p>
                    </div>
                </div>";

            await SendEmailAsync(toEmail, name, subject, body);
        }

        // ── Absent Notification Email ──
        public async Task SendAbsentEmailAsync(string toEmail, string name, string date)
        {
            string subject = $"Attendance Alert — {date}";
            string body = $@"
                <div style='font-family:Arial,sans-serif;max-width:500px;margin:auto;'>
                    <div style='background:#1e293b;padding:20px;border-radius:10px 10px 0 0;'>
                        <h2 style='color:#fff;margin:0;'>EduTrack SMS</h2>
                        <p style='color:#94a3b8;margin:4px 0 0;'>Attendance Alert</p>
                    </div>
                    <div style='background:#f8fafc;padding:24px;border:1px solid #e2e8f0;'>
                        <p style='color:#0f172a;'>Dear <strong>{name}</strong>,</p>
                        <div style='background:#fef2f2;border:1px solid #fecaca;
                                    border-radius:8px;padding:16px;margin:16px 0;'>
                            <p style='margin:0;color:#dc2626;font-weight:600;'>
                                ✗ You were marked Absent on {date}
                            </p>
                        </div>
                        <p style='color:#475569;'>If this is incorrect, please contact
                           your teacher.</p>
                    </div>
                    <div style='background:#f1f5f9;padding:12px;border-radius:0 0 10px 10px;
                                text-align:center;'>
                        <p style='color:#94a3b8;font-size:12px;margin:0;'>
                            EduTrack Student Management System
                        </p>
                    </div>
                </div>";

            await SendEmailAsync(toEmail, name, subject, body);
        }
    }
}