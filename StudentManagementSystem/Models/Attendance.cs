namespace StudentManagementSystem.Models
{
    public class Attendance
    {
        public int aid { get; set; }
        public string sid { get; set; }
        public string studentName { get; set; }
        public DateTime date { get; set; }
        public string status { get; set; }
        public string markedBy { get; set; }
    }
}