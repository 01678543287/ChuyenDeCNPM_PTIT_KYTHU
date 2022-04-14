using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BackUpandReStore
{
    public partial class DangNhap : Form
    {
        private SqlConnection conn_Server = new SqlConnection();
        public DangNhap()
        {
            InitializeComponent();
        }

        private void lbChiNhanh_Click(object sender, EventArgs e)
        {

        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private int ConnTo_Server(String txtTK, String txtMK, String txtSV)
        {
            if (conn_Server != null && conn_Server.State == ConnectionState.Open)
            {
                conn_Server.Close();  // nếu connection đã mở thì đóng lại
            }
            try
            {
                conn_Server.ConnectionString = "Data Source="+txtSV+";Persist Security Info=True;User ID="+txtTK+";Password="+txtMK+"";  // gán chuối kết nối server vào connection
                conn_Server.Open();   // mở kết nối
                return 1;
            }
            catch (SqlException e)
            {
                MessageBox.Show("Lỗi kết nối tới cơ sở dữ liệu gốc.\nHãy xem lại tên của Server, Tài khoản và mật khẩu của Server" + e.Message);
                return 0;
            }
        }
        private void btnDangNhap_Click(object sender, EventArgs e)
        {
            Program.serverName = txtServerName.Text.Trim();
            Program.TestLogin = txtTaiKhoan.Text.Trim();
            Program.TestPassword = txtMatKhau.Text.Trim();

            if (txtTaiKhoan.Text.Equals("") || txtMatKhau.Text.Equals(""))
            {
                MessageBox.Show("Không được để trống tên đăng nhập hoặc mật khẩu");
                return;
            }
            if(ConnTo_Server(txtTaiKhoan.Text.ToString().Trim(), txtMatKhau.Text.ToString().Trim(), txtServerName.Text.ToString().Trim())==1)
            {
                if (Program.connection() == 0) return;
                MessageBox.Show("Đăng nhập thành công");
                this.Hide();
                BackupAndRestore bk = new BackupAndRestore();
                bk.Closed += (s, args) => this.Close();
                bk.Show();
            }


        }
    }
}
