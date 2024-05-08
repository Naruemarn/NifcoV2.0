using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NifcoV2._0
{
    public partial class Form3 : Form
    {
        string datetime_from = null;
        string datetime_to = null;

        public Form3()
        {
            InitializeComponent();
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void Form3_Load(object sender, EventArgs e)
        {
            textBox2.Select();
            textBox2.Focus();

            dateTimePicker1.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 8, 0, 0);
            dateTimePicker2.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 8, 0, 0);

            dateTimePicker2.Value = dateTimePicker2.Value.AddDays(1);

            datetime_from = dateTimePicker1.Value.ToString("yyyy-MM-dd") + " 00:00:00";
            datetime_to = dateTimePicker2.Value.ToString("yyyy-MM-dd") + " 23:59:59";
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void button1_Click(object sender, EventArgs e)
        {
            if ((textBox1.Text == Form1.Admin_User_ini) && (textBox2.Text == Form1.Admin_Pass_ini))
            {

                //DateTime End = Convert.ToDateTime(datetime_to);
                //DateTime Start = Convert.ToDateTime(datetime_from);
                //double Total = (End - Start).TotalDays;

                DialogResult res = MessageBox.Show("Are you sure!!! ?\r\n\r\n" + "Start : " + datetime_from + "\r\n" + "End  : " + datetime_to, "Confirm", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);                                                                            
                if (res.ToString() == "OK")
                {
                    string affected = Delete_CountRow();

                    Log_Delete(datetime_from, datetime_to, affected);

                    this.Close();
                }
                else
                {

                }
            }
            else
            {
                MessageBox.Show("Wrong! User Password", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBox1.Text = "";
                textBox2.Text = "";
            }
            //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
            //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            datetime_from = dateTimePicker1.Value.ToString("yyyy-MM-dd") + " 00:00:00";
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            datetime_to = dateTimePicker2.Value.ToString("yyyy-MM-dd" + " 23:59:59");
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public string Delete_CountRow()
        {
            string cntrow = "";
            try
            {                
                using (SqlConnection con = new SqlConnection(Form1.myConnection))
                {

                    string sql = "DELETE FROM tbl_AutoSyncData WHERE DateTime BETWEEN '" + datetime_from + "' and '" + datetime_to + "';  SELECT @@ROWCOUNT";

                    using (SqlCommand command = new SqlCommand(sql, con))
                    {
                        con.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                cntrow = reader[0].ToString();                                
                            }
                        }
                        con.Close();
                    }
                }

                MessageBox.Show(cntrow +" rows affected", "Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (SystemException ex)
            {
                MessageBox.Show(string.Format("An error occurred: {0}", ex.Message));
            }

            return cntrow;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public static void Log_Delete(string start , string end, string affected)
        {
            try
            {
                //string strPath = @"C:\AutoSyncData_Log\Connected_log.txt";

                string path = Directory.GetCurrentDirectory();
                string strPath = path + "\\Log\\Delete.txt";

                if (!File.Exists(strPath))
                {
                    File.Create(strPath).Dispose();
                }

                string dt = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");

                using (StreamWriter sw = File.AppendText(strPath))
                {
                    sw.WriteLine(dt + "    " + "Start: " + start + "    " + "End: " + end + "    " + affected + " rows affected");
                    sw.Close();
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            textBox2.PasswordChar = checkBox1.Checked ? '\0' : '*';
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                //textBox2.Select();
                //textBox2.Focus();
            }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}
