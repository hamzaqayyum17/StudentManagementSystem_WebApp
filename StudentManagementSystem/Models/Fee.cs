namespace StudentManagementSystem.Models
{
    public class Fee
    {
        public int fid { get; set; }
        public string sid { get; set; }
        public string studentName { get; set; }
        public string month { get; set; }
        public decimal amount { get; set; }
        public string status { get; set; }
        public DateTime? paidDate { get; set; }
    }
}