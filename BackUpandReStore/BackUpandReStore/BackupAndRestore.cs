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
    public partial class BackupAndRestore : Form
    {
        public BackupAndRestore()
        {
            InitializeComponent();
        }

        private void barButtonItem1_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            this.Close();
        }

        private void BackupAndRestore_Load(object sender, EventArgs e)
        {
            //Program.conn.Open();
            String sql = "SELECT name " +
                         "FROM sys.databases " +
                         "WHERE(database_id >= 6) AND(NOT(name LIKE N'ReportServer%')) " +
                         "ORDER BY NAME ";
            SqlCommand com = new SqlCommand(sql, Program.conn); //bat dau truy van
            com.CommandType = CommandType.Text;
            SqlDataAdapter da = new SqlDataAdapter(com); //chuyen du lieu ve
            DataTable dt = new DataTable(); //tạo một kho ảo để lưu trữ dữ liệu
            da.Fill(dt);  // đổ dữ liệu vào kho
            dataCSDL.DataSource = dt;

            timeEdit1.Time = DateTime.Now;
            dateTimePicker1.Value = DateTime.Today;
            Program.conn.Close();

            btnDevice.Enabled = btnPhucHoi.Enabled = btnSaoLuu.Enabled = cbTime.Enabled = false;
        }

        private void dataCSDL_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
        private void loadBackup()
        {
            Program.conn.Open();
            String sql = "SELECT position, backup_start_date , user_name FROM  msdb.dbo.backupset "
                + "WHERE database_name ='"+txtNameCSDL.Text.Trim()+"' AND type='D' AND "
                + "backup_set_id >= "
                + "( SELECT  MAX(backup_set_id) "
                + "FROM msdb.dbo.backupset "
                + "WHERE media_set_id = "
                + "( SELECT MAX(media_set_id) "
                + "FROM msdb.dbo.backupset "
                + "WHERE database_name = '" + txtNameCSDL.Text.Trim() + "' AND type='D' "
                + ") "
                + "AND position=1 "
                + ") "
                + "ORDER BY position DESC ";

            SqlCommand com = new SqlCommand(sql, Program.conn); //bat dau truy van
            com.CommandType = CommandType.Text;
            SqlDataAdapter da = new SqlDataAdapter(com); //chuyen du lieu ve
            DataTable dt = new DataTable(); //tạo một kho ảo để lưu trữ dữ liệu
            da.Fill(dt);  // đổ dữ liệu vào kho
            dataBackUp.DataSource = dt;
            Program.conn.Close();
        }
        private void loadDevice(String a)
        {
            Program.conn.Open();
            String sqlDevice = "SELECT name FROM sys.sysdevices WHERE name ='" + "DEVICE_" + txtNameCSDL.Text + "'";
            SqlDataReader reader;
            reader = Program.ExecuteSqlReader(sqlDevice);
            reader.Read();
            a = "";
            Program.tendevice = a;
            if (reader.HasRows)
            {
                a = reader.GetString(0);
                Program.tendevice = a;
            }
            Program.conn.Close();
            if (a.Equals(""))
            {
                btnDevice.Enabled = true;
            }
            else
            {
                btnSaoLuu.Enabled = btnDevice.Enabled = false;
            }


            if (dataBackUp.Rows.Count == 0 && !a.Equals(""))
            {
                btnSaoLuu.Enabled = true;
                btnPhucHoi.Enabled = cbTime.Enabled = false;
                btnDevice.Enabled = false;
                txtVersion.Text = "0";
            }
            else if (dataBackUp.Rows.Count == 0 && a.Equals(""))
            {
                btnPhucHoi.Enabled = btnSaoLuu.Enabled = cbTime.Enabled = false;
                btnDevice.Enabled = true;
                txtVersion.Text = "0";
            }
            else
            {
                btnPhucHoi.Enabled = btnSaoLuu.Enabled = cbTime.Enabled = true;
                btnDevice.Enabled = false;
                txtVersion.Text = dataBackUp.Rows[0].Cells[1].Value.ToString();
            }
        }
        private void dataCSDL_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                // Lưu dòng dữ liệu vừa chọn
                DataGridViewRow row = this.dataCSDL.Rows[e.RowIndex];
                txtNameCSDL.Text = row.Cells[0].Value.ToString();
            }
            String a="";
            loadBackup();
            loadDevice(a);
            progress.Visible = true;
            for (int i = this.progress.Minimum; i <= this.progress.Maximum; i++)
                progress.Value = i;
            progress.Visible = false;
        }

        private void dataGridView2_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            
            if (e.RowIndex >= 0)
            {   
                DataGridViewRow row = this.dataBackUp.Rows[e.RowIndex];
                txtVersion.Text = row.Cells[1].Value.ToString();
                progress.Visible = true;
                for (int i = this.progress.Minimum; i <= this.progress.Maximum; i++)
                    progress.Value = i;
                progress.Visible = false;
            }

        }

        private void btnTime_CheckedChanged(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (cbTime.Checked == true) pnTime.Visible = true;
            else pnTime.Visible = false;
        }

        private void btnSaoLuu_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            String sql = " BACKUP DATABASE " + txtNameCSDL.Text + " TO DEVICE_" + txtNameCSDL.Text;
            Program.conn.Open();
            if (cbDeleteAll.Checked)
            {
                if (MessageBox.Show("Bạn có chắc chắn muốn xóa các bản sao lưu cũ", "Xác nhận", MessageBoxButtons.OKCancel) == DialogResult.OK)
                    sql += " WITH INIT";
                else return;
            }

            int err = Program.ExecuteSqlNonQuery(sql);
            if (err == 0)
            {
                progress.Visible = true;
                for (int i = this.progress.Minimum; i <= this.progress.Maximum; i++)
                    progress.Value = i;
                progress.Visible = false;
                cbDeleteAll.Checked = false;
            }
            else
            {
                this.progress.Visible = false;
                return;
            }
            MessageBox.Show("Đã sao lưu cơ sở dữ liệu!");
            Program.conn.Close();
            loadBackup();
        }

        private void btnDevice_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            Program.conn.Open();
            String nameDevice = "DEVICE_"+txtNameCSDL.Text;
            String nameDeviceLink = "DEVICE_" + txtNameCSDL.Text + ".bak";
            String sql = "EXEC sp_addumpdevice 'disk', '"+nameDevice+"', '"+Program.strPath+nameDeviceLink+"'";
            Program.ExecuteSqlNonQuery(sql);
            progress.Visible = true;
            for (int i = this.progress.Minimum; i <= this.progress.Maximum; i++)
                progress.Value = i;
            progress.Visible = false;
            Program.conn.Close();
            String a = "";
            loadDevice(a);
        }

        private void btnPhucHoi_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            String strRestore;
            if (txtNameCSDL.Text.Trim() == "" || Program.tendevice == "") return;
            strRestore = "ALTER DATABASE " + txtNameCSDL.Text.Trim() + " SET SINGLE_USER WITH ROLLBACK IMMEDIATE "
                       + "\nUSE tempdb ";
            if (cbTime.Checked == false)
            {
                strRestore += "\n RESTORE DATABASE " + txtNameCSDL.Text.Trim()
                    + "\n FROM   " + Program.tendevice + " WITH FILE =  " + txtVersion.Text + ", REPLACE "
                    + "\n ALTER DATABASE  " + txtNameCSDL.Text.Trim() + " SET MULTI_USER";

                if (MessageBox.Show("Bạn có chắc chắn muốn phục hồi database " + txtNameCSDL.Text.Trim() + " về file " + txtVersion.Text + "?"
                  , "Cảnh báo", MessageBoxButtons.OKCancel) == DialogResult.OK)
                {
                    // Show  progressbar
                    progress.Visible = true;
                    for (int i = this.progress.Minimum; i <= this.progress.Maximum; i++)
                        progress.Value = i;

                    int err = Program.ExecuteSqlNonQuery(strRestore);
                    if (err == 0)
                    {
                        progress.Visible = true;
                        for (int i = this.progress.Minimum; i <= this.progress.Maximum; i++)
                            progress.Value = i;
                        progress.Visible = false;
                        MessageBox.Show("Phục hồi dữ liệu thành công", "Thông báo", MessageBoxButtons.OK);
                    }
                    else progress.Visible = false; return;
                }
                else progress.Visible = false; return;
            }
            // Backup đên 1 thời gian người dùng nhập
            else
            {
                // Ngày giờ stop at > thời điểm sao lưu và nhỏ hơn thời điểm hiện tại ít nhất 3p\
                // Ngày giờ của bản backup được chọn
                MessageBox.Show(dataBackUp.Rows[this.dataBackUp.Rows.Count - int.Parse(txtVersion.Text)].Cells[2].Value.ToString());
                DateTime ngaygioBK = (DateTime)this.dataBackUp.Rows[this.dataBackUp.Rows.Count - int.Parse(txtVersion.Text)].Cells[2].Value;
                MessageBox.Show("Ngày giờ bản backUP: "+ngaygioBK.ToString());

                // ngày h của user nhập
                String strThoiDiemStopAt = dateTimePicker1.Value.Day + "-" + dateTimePicker1.Value.Month + "-" + dateTimePicker1.Value.Year + " " +
                    timeEdit1.Time.Hour + ":" + timeEdit1.Time.Minute + ":" + timeEdit1.Time.Second;
                MessageBox.Show("Ngày giờ bản user: " + strThoiDiemStopAt.ToString());

                //Ngày giờ hiện tại
                DateTime thoiDiemStopAt;
                thoiDiemStopAt = DateTime.Parse(strThoiDiemStopAt);


                // Kiểm tra sau thời điểm hiện tại
                if (thoiDiemStopAt > DateTime.Now)
                {
                    MessageBox.Show("Thời điểm muốn phục hồi phải trước thời điểm hiện tại", "", MessageBoxButtons.OK);
                    progress.Visible = false;
                    return;
                }

                //if (dateStop.DateTime.Date <= ngaygioBK.Date
                //    || thoiDiemStopAt.TimeOfDay.Ticks < ngaygioBK.TimeOfDay.Ticks)
                if ( (dateTimePicker1.Value.Date < ngaygioBK.Date) || ( dateTimePicker1.Value.Date == ngaygioBK.Date && thoiDiemStopAt.TimeOfDay.Ticks < ngaygioBK.TimeOfDay.Ticks ))
                {
                    MessageBox.Show("Thời điểm muốn phục hồi phải sau thời điểm bản sao lưu đã chọn", "", MessageBoxButtons.OK);
                    progress.Visible = false;
                    return;
                }

                try
                {
                    // Restore về 1 thời điểm người dùng nhập
                    strRestore = "ALTER DATABASE " + txtNameCSDL.Text.Trim() + " SET SINGLE_USER WITH ROLLBACK IMMEDIATE \n" +
                           " BACKUP LOG " + txtNameCSDL.Text.Trim() + " TO DISK = '" + Program.strPath + "DEVICE_" + txtNameCSDL.Text.Trim() + ".trn' WITH INIT, NORECOVERY; \n " +
                           " USE tempdb SET DATEFORMAT DMY \n" +
                           " RESTORE DATABASE " + txtNameCSDL.Text.Trim() + " FROM  DEVICE_" + txtNameCSDL.Text.Trim() + " WITH FILE =" + txtVersion.Text.Trim() + ", NORECOVERY; \n" +
                           " RESTORE DATABASE " + txtNameCSDL.Text.Trim() + " FROM DISK = '" + Program.strPath + "DEVICE_" + txtNameCSDL.Text.Trim() + ".trn' " +
                           " WITH STOPAT= '" + dateTimePicker1.Value.Day + "-" + dateTimePicker1.Value.Month + "-" + dateTimePicker1.Value.Year + " " + timeEdit1.Text + "' \n" +
                           " ALTER DATABASE  " + txtNameCSDL.Text.Trim() + " SET MULTI_USER ";
                    MessageBox.Show(strRestore);
                    int err = Program.ExecSqlNonQuery(strRestore, Program.StrConn, "Lỗi phục hồi cơ sở dữ liệu.");
                    if (err == 0)
                    {
                        progress.Visible = true;
                        for (int i = this.progress.Minimum; i <= this.progress.Maximum; i++)
                            progress.Value = i;
                        progress.Visible = false;
                        MessageBox.Show("Phục hồi dữ liệu thành công", "Thông báo", MessageBoxButtons.OK);
                    }
                    else
                    {
                        progress.Visible = false;
                        strRestore = "RESTORE DATABASE " + txtNameCSDL.Text.Trim() + " FROM DEVICE_" + txtNameCSDL.Text.Trim() + " WITH FILE= " + txtVersion.Text.Trim() + " \n"
                               + "ALTER DATABASE " + txtNameCSDL.Text.Trim() + " SET MULTI_USER \n";
                        Program.ExecSqlNonQuery(strRestore, Program.StrConn, "");
                    }
                    cbTime.Checked = false;
                    return;

                }
                catch (Exception ex)
                {
                    
                    MessageBox.Show("Lỗi Restore:\n" + ex, "Xảy ra lỗi", MessageBoxButtons.OK);
                }
            }
        }
    }
}
