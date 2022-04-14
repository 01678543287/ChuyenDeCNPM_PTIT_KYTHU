using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Data;

namespace BackUpandReStore
{
    static class Program
    {
        // khai báo các biến và hàm toàn tục

        public static SqlConnection conn = new SqlConnection();
        public static String StrConn; // chuỗi kết nối tới server phân mảnh của nhân viên đăng nhập tới

        public static SqlDataReader myReader;
        public static SqlDataAdapter da;
        public static String serverName;// reader chứa thông tin đăng nhập của nhân viên đăng nhập
        public static String TestLogin;   // test kết nối csdl login
        public static String TestPassword; // test kết nối csdl pass
        public static int flagRestore = 0;
        public static String strPath = @"E:\Backup\";
        public static String tendevice = "";


        public static int connection()
        {
            if (Program.conn != null && Program.conn.State == ConnectionState.Open) Program.conn.Close();

            try
            {
                Program.StrConn = "Data Source=" + Program.serverName + ";User ID=" + Program.TestLogin + ";Password=" + Program.TestPassword;
                Program.conn.ConnectionString = StrConn;
                Program.conn.Open();
                return 1;
            }
            catch (Exception e)
            {
                MessageBox.Show("Lỗi kết nối cơ sở dữ liệu. \nHãy xem lại username và password.\n" + e.Message);
                return 0;
            }


        }

        public static SqlDataReader ExecuteSqlReader(String sqlStr)
        {
            SqlDataReader myReader;
            SqlCommand cmd = new SqlCommand(sqlStr, Program.conn);
            cmd.CommandType = CommandType.Text;
            if (Program.conn.State == ConnectionState.Closed) Program.conn.Open();
            try
            {
                myReader = cmd.ExecuteReader();
                return myReader;
            }
            catch (Exception e)
            {
                Program.conn.Close();
                MessageBox.Show(e.Message);
                return null;
            }

        }
        public static int ExecSqlNonQuery(String strlenh, String connectionString, String errStr)
        {
            conn = new SqlConnection(connectionString);
            if (conn.State == ConnectionState.Closed) conn.Open();
            SqlCommand Sqlcmd = new SqlCommand(strlenh, conn);
            Sqlcmd.CommandType = CommandType.Text;
            Sqlcmd.CommandTimeout = 600;// 10 phut 
            try
            {
                Sqlcmd.ExecuteNonQuery();
                conn.Close();
                return 0;
            }
            catch (SqlException ex)
            {
                MessageBox.Show(errStr + "\n" + ex.Message);
                conn.Close();
                return ex.State; // trang thai lỗi gởi từ RAISERROR trong SQL Server qua
            }
        }
        public static int ExecuteSqlNonQuery(String sqlStr)
        {
            SqlCommand cmd = new SqlCommand(sqlStr, Program.conn);
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = 600; // thời gian thực thi trong 10 phút
            if (Program.conn.State == ConnectionState.Closed) Program.conn.Open();
            try
            {
                cmd.ExecuteNonQuery();
                Program.conn.Close();
                return 0;
            }
            catch (SqlException e)
            {
                Program.conn.Close();
                MessageBox.Show(e.Message);
                return e.State;  // trang thai lỗi từ RAISERROR trong sql
            }

        }
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]

        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Program.Da = new FrmMain();
            Application.Run(new DangNhap());

        }
    }
}
