using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using StudentManagementSystem.Models;
using System.Data;

namespace StudentManagementSystem.Services
{
    public class GoogleSheetsService
    {
        private readonly IConfiguration _config;
        private readonly DBAccess _db;

        public GoogleSheetsService(IConfiguration config)
        {
            _config = config;
            _db = new DBAccess();
        }

        private SheetsService GetSheetsService()
        {
            string credPath = _config["GoogleSheets:CredentialsPath"];
            GoogleCredential credential = GoogleCredential
                .FromFile(credPath)
                .CreateScoped(SheetsService.Scope.SpreadsheetsReadonly);
            return new SheetsService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "StudentMS"
            });
        }

        // ── Class wise sync ──
        public async Task<int> SyncClassAttendanceAsync(int classId)
        {
            // Class ki sheet ID aur sections lo
            string qClass = "select googleSheetId from Class where classId=" + classId;
            DataTable dtClass = _db.GetDataTable(qClass);

            if (dtClass.Rows.Count == 0 ||
                string.IsNullOrEmpty(dtClass.Rows[0]["googleSheetId"]?.ToString()))
                throw new Exception("Is class ki Google Sheet assign nahi hai!");

            string spreadsheetId = dtClass.Rows[0]["googleSheetId"].ToString();

            // Sections fetch karo
            string qSec = "select sectionId, sectionName from Section where classId=" + classId;
            DataTable dtSections = _db.GetDataTable(qSec);

            var service = GetSheetsService();
            int totalSynced = 0;

            // Har section ki tab read karo
            foreach (DataRow secRow in dtSections.Rows)
            {
                string sectionName = secRow["sectionName"].ToString();
                int sectionId = int.Parse(secRow["sectionId"].ToString());

                try
                {
                    // Sheet tab ka naam = Section name (A, B, C)
                    var request = service.Spreadsheets.Values.Get(
                        spreadsheetId, $"{sectionName}!A1:ZZ");
                    var response = await request.ExecuteAsync();
                    var rows = response.Values;

                    if (rows == null || rows.Count < 2) continue;

                    // Row 1 = Headers: RollNumber | Name | Date1 | Date2 ...
                    var headers = rows[0];

                    for (int col = 2; col < headers.Count; col++)
                    {
                        string dateStr = headers[col].ToString().Trim();
                        if (string.IsNullOrEmpty(dateStr)) continue;
                        if (!DateTime.TryParse(dateStr, out DateTime date)) continue;

                        string sqlDate = date.ToString("yyyy-MM-dd");

                        for (int row = 1; row < rows.Count; row++)
                        {
                            var rowData = rows[row];
                            if (rowData.Count < 1) continue;

                            string rollNumber = rowData[0].ToString().Trim();
                            if (string.IsNullOrEmpty(rollNumber)) continue;

                            string status = "Absent";
                            if (col < rowData.Count &&
                                !string.IsNullOrEmpty(rowData[col]?.ToString()))
                                status = rowData[col].ToString().Trim();


                            status = status.ToUpper().Trim();
                            if (status == "P") status = "Present";
                            else if (status == "A") status = "Absent";                            
                            else status = "Absent";

                            // RollNumber se student ID
                            string qSid = "select sid from Student where rollNumber='" +
                                          rollNumber + "'";
                            DataTable dtSid = _db.GetDataTable(qSid);
                            if (dtSid.Rows.Count == 0) continue;

                            string sid = dtSid.Rows[0]["sid"].ToString();

                            // Already exist?
                            string checkQ = "select count(*) from Attendance " +
                                            "where sid='" + sid + "' and date='" + sqlDate + "'";
                            int count = int.Parse(_db.GetDataTable(checkQ).Rows[0][0].ToString());

                            if (count == 0)
                            {
                                string insertQ = "insert into Attendance(sid,date,status,markedBy)" +
                                                 " values('" + sid + "','" + sqlDate + "','" +
                                                 status + "','GoogleSheet-" + sectionName + "')";
                                _db.IUD(insertQ);
                                totalSynced++;
                            }
                            else
                            {
                                string updateQ = "update Attendance set status='" + status +
                                                 "', markedBy='GoogleSheet-" + sectionName +
                                                 "' where sid='" + sid +
                                                 "' and date='" + sqlDate + "'";
                                _db.IUD(updateQ);
                            }
                        }
                    }
                }
                catch { continue; } // Agar tab nahi mili to skip karo
            }

            return totalSynced;
        }

        // ── Sab classes sync (pehle wala) ──
        public async Task<int> SyncAttendanceAsync()
        {
            string spreadsheetId = _config["GoogleSheets:SpreadsheetId"];
            string sheetName = _config["GoogleSheets:SheetName"];
            var service = GetSheetsService();
            var request = service.Spreadsheets.Values.Get(spreadsheetId, $"{sheetName}!A1:ZZ");
            var response = await request.ExecuteAsync();
            var rows = response.Values;
            if (rows == null || rows.Count < 2) return 0;
            var headers = rows[0];
            int synced = 0;
            for (int col = 2; col < headers.Count; col++)
            {
                string dateStr = headers[col].ToString().Trim();
                if (!DateTime.TryParse(dateStr, out DateTime date)) continue;
                string sqlDate = date.ToString("yyyy-MM-dd");
                for (int row = 1; row < rows.Count; row++)
                {
                    var rowData = rows[row];
                    if (rowData.Count < 1) continue;
                    string rollNumber = rowData[0].ToString().Trim();
                    if (string.IsNullOrEmpty(rollNumber)) continue;
                    string status = col < rowData.Count ? rowData[col]?.ToString().Trim() : "Absent";
                    if (status != "Present" && status != "Absent" && status != "Late") status = "Absent";
                    string qSid = "select sid from Student where rollNumber='" + rollNumber + "'";
                    DataTable dtSid = _db.GetDataTable(qSid);
                    if (dtSid.Rows.Count == 0) continue;
                    string sid = dtSid.Rows[0]["sid"].ToString();
                    string checkQ = "select count(*) from Attendance where sid='" + sid + "' and date='" + sqlDate + "'";
                    int count = int.Parse(_db.GetDataTable(checkQ).Rows[0][0].ToString());
                    if (count == 0) { _db.IUD("insert into Attendance(sid,date,status,markedBy) values('" + sid + "','" + sqlDate + "','" + status + "','GoogleSheet')"); synced++; }
                    else { _db.IUD("update Attendance set status='" + status + "', markedBy='GoogleSheet' where sid='" + sid + "' and date='" + sqlDate + "'"); }
                }
            }
            return synced;
        }
    }
}