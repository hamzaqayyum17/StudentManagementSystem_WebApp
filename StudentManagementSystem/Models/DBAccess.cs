using Microsoft.Data.SqlClient;
using System.Data;

namespace StudentManagementSystem.Models
{
    public class DBAccess
    {
        static string constr = "Data Source=DESKTOP-ICNAA62;" +
                       "Initial Catalog=StudentDB;" +
                       "Integrated Security=True;" +
                       "TrustServerCertificate=True;";

        SqlConnection con = new SqlConnection(constr);
        SqlCommand cmd = null;
        SqlDataReader sdr = null;
        public void OpenConnection()
        {
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
        }
        public void CloseConnection()
        {
            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
        }
        public void IUD(string query)
        {
            OpenConnection();

            cmd = new SqlCommand(query, con);
            cmd.ExecuteNonQuery();
            CloseConnection();
        }

        public SqlDataReader GetData(string query)
        {
            OpenConnection();
            cmd = new SqlCommand(query, con);
            sdr = cmd.ExecuteReader();
            return sdr;
        }
        public DataTable GetDataTable(string query)
        {
            OpenConnection();
            SqlDataAdapter da = new SqlDataAdapter(query, con);
            DataTable dt = new DataTable();
            da.Fill(dt);
            CloseConnection();
            return dt;
        }

    }
}
