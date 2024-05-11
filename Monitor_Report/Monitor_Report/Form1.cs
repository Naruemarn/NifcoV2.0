using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Threading;
using System.Net;
using System.IO;
using Gajatko.IniFiles;
using Microsoft.Win32;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;

namespace Monitor_Report
{
    public partial class Form1 : Form
    {
        string myConnection = null;
        string ServerDB_ini = null;
        string User_ini = null;
        string Password_ini = null;
        string Timeout_ini = null;
        string Database_ini = null;

        string Line1_ini = null;
        string Line2_ini = null;
        string Line3_ini = null;
        string Line4_ini = null;
        string Line5_ini = null;
        string Line6_ini = null;

        string MC1_ini = null;
        string MC2_ini = null;


        string datetime_from = null;
        string datetime_to = null;

        string filename = "";


        public Form1()
        {
            InitializeComponent();

        }
        //-----------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------
        void read_ini_config()
        {
            try
            {
                IniFile cn_file = IniFile.FromFile("Config.ini");

                ServerDB_ini = cn_file["CONFIG"]["SERVER_DB"];
                User_ini = cn_file["CONFIG"]["USER_DB"];
                Password_ini = cn_file["CONFIG"]["PASSWORD_DB"];
                Timeout_ini = cn_file["CONFIG"]["TIMEOUT_DB"];
                Database_ini = cn_file["CONFIG"]["DATABASE_NAME_DB"];

                Line1_ini = cn_file["CONFIG"]["LINE1"];
                Line2_ini = cn_file["CONFIG"]["LINE2"];
                Line3_ini = cn_file["CONFIG"]["LINE3"];
                Line4_ini = cn_file["CONFIG"]["LINE4"];
                Line5_ini = cn_file["CONFIG"]["LINE5"];
                Line6_ini = cn_file["CONFIG"]["LINE6"];

                MC1_ini = cn_file["CONFIG"]["MC1"];
                MC2_ini = cn_file["CONFIG"]["MC2"];
            }
            catch (Exception h)
            {
                MessageBox.Show(h.ToString());
            }
        }
        //-----------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------
        private void timer1_Tick(object sender, EventArgs e)
        {
            ShowDisplay();
        }
        //-----------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------
        private void Form1_Load(object sender, EventArgs e)
        {
            read_ini_config();


            myConnection = "user id=" + User_ini + ";" +
                         "password=" + Password_ini + ";server=" + ServerDB_ini + ";" +
                         "Trusted_Connection=false;" +
                         "database=" + Database_ini + "; " +
                         "connection timeout=" + Timeout_ini + "";

            ShowDisplay();

            comboBox1.Items.Add(Line1_ini);
            comboBox1.Items.Add(Line2_ini);
            comboBox1.Items.Add(Line3_ini);
            comboBox1.Items.Add(Line4_ini);
            comboBox1.Items.Add(Line5_ini);
            comboBox1.Items.Add(Line6_ini);

            comboBox2.Items.Add(MC1_ini);
            comboBox2.Items.Add(MC2_ini);

            // Set color GridView
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.Blue;
            dataGridView1.BackgroundColor = Color.White;
            dataGridView1.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            //dataGridView1.RowHeadersVisible = false;        // Remove First Column

            dataGridView1.DefaultCellStyle.Font = new Font("Tahoma", 8); // font size



            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
            comboBox3.SelectedIndex = 0;

            dateTimePicker1.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 8, 0, 0);
            dateTimePicker2.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 8, 0, 0);

            dateTimePicker2.Value = dateTimePicker2.Value.AddDays(1);

            datetime_from = dateTimePicker1.Value.ToString("yyyy-MM-dd HH:mm");
            datetime_to = dateTimePicker2.Value.ToString("yyyy-MM-dd HH:mm");
        }
        //-----------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------
        void ShowDisplay()
        {
            // Line1
            Display("1", label80, label7, label8, label9, label10, label11, label12, label93);
            Display("2", label81, label16, label17, label13, label14, label15, label18, label94);

            // Line2
            Display("3", label82, label22, label23, label19, label20, label21, label24, label95);
            Display("4", label83, label28, label29, label25, label26, label27, label30, label96);

            // Line3
            Display("5", label84, label34, label35, label31, label32, label33, label36, label97);
            Display("6", label85, label40, label41, label37, label38, label39, label42, label98);

            // Line4
            Display("7", label86, label46, label47, label43, label44, label45, label48, label99);
            Display("8", label87, label52, label53, label49, label50, label51, label54, label100);

            // Line5
            Display("9", label88, label58, label59, label55, label56, label57, label60, label101);
            Display("10", label89, label64, label65, label61, label62, label63, label66, label102);

            // Line6
            Display("11", label90, label70, label71, label67, label68, label69, label78, label103);
            Display("12", label91, label76, label77, label73, label74, label75, label72, label104);
        }
        //-----------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------
        void Display(string id ,Label label_id, Label label_line, Label label_machinename, Label label_actual, Label label_ok, Label label_ng, Label label_reset, Label label_status)
        {
            List<string> res = new List<string>();

            res = Read_Current_Monitor(id);
            label_id.Text = res[0];
            label_line.Text = res[1];
            label_machinename.Text = res[2];
            label_actual.Text = res[3];
            label_ok.Text = res[4];
            label_ng.Text = res[5];
            label_reset.Text = res[6];

            if (res[7] == "True")
            {
                label_status.Text = "Running";
                label_status.BackColor = Color.Green;
            }
            else
            {
                label_status.Text = "";
                label_status.BackColor = Color.Gray;
            }            
        }
        //-----------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------
        public List<string> Read_Current_Monitor(string id_mc)
        {
            string id = "";
            string linenum = "";
            string mc = "";
            string actual = "";
            string ok = "";
            string ng = "";            
            string reset = "";
            string status = "";

            List<string> resultList = new List<string>();

            SqlConnection cn = new SqlConnection(myConnection);
            try
            {
                string sql_string = "SELECT * FROM tbl_monitor WHERE id=@buff_id;";

                if (cn.State == ConnectionState.Closed)
                {
                    cn.Open();
                }

                SqlCommand command = new SqlCommand(sql_string, cn);
                command.Parameters.AddWithValue("@buff_id", id_mc);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        id = reader[0].ToString();
                        linenum = reader[1].ToString();
                        mc = reader[2].ToString();
                        actual = reader[3].ToString();
                        ok = reader[4].ToString();
                        ng = reader[5].ToString();                       
                        reset = reader[6].ToString();
                        status = reader[7].ToString();
                    }
                }

                resultList.Add(id);
                resultList.Add(linenum);
                resultList.Add(mc);
                resultList.Add(actual);
                resultList.Add(ok);
                resultList.Add(ng);               
                resultList.Add(reset);
                resultList.Add(status);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            finally
            {
                cn.Close();
            }

            return resultList;
        }
        //-----------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab == tabPage1)
            {
                Debug.WriteLine("Tabpage 1");
                timer1.Enabled = true;
            }
            else if (tabControl1.SelectedTab == tabPage2)
            {
                Debug.WriteLine("Tabpage 2");
                timer1.Enabled=false;
            }
        }
        //-----------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------
        private void tabControl1_DrawItem(object sender, DrawItemEventArgs e)
        {
            // เปลี่ยนสี Tab
            Graphics g = e.Graphics;
            TabPage tp = tabControl1.TabPages[e.Index];

            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;  //optional

            // This is the rectangle to draw "over" the tabpage title
            RectangleF headerRect = new RectangleF(e.Bounds.X, e.Bounds.Y + 2, e.Bounds.Width, e.Bounds.Height - 2);

            // This is the default colour to use for the non-selected tabs
            SolidBrush sb = new SolidBrush(Color.Black);

            // This changes the colour if we're trying to draw the selected tabpage
            if (tabControl1.SelectedIndex == e.Index)
                sb.Color = Color.Blue;

            // Colour the header of the current tabpage based on what we did above
            g.FillRectangle(sb, e.Bounds);

            //Remember to redraw the text - I'm always using black for title text
            g.DrawString(tp.Text, tabControl1.Font, new SolidBrush(Color.White), headerRect, sf);
        }
        //-----------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------
        void ExportCSV(string filename)
        {
            if (dataGridView1.Rows.Count > 0)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "CSV (*.csv)|*.csv";
                sfd.FileName = filename + ".csv";
                bool fileError = false;
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    if (File.Exists(sfd.FileName))
                    {
                        try
                        {
                            File.Delete(sfd.FileName);
                        }
                        catch (IOException ex)
                        {
                            fileError = true;
                            MessageBox.Show("It wasn't possible to write the data to the disk." + ex.Message);
                        }
                    }
                    if (!fileError)
                    {
                        try
                        {
                            int columnCount = dataGridView1.Columns.Count;
                            string columnNames = "";
                            string[] outputCsv = new string[dataGridView1.Rows.Count + 1];

                            for (int i = 0; i < columnCount; i++)
                            {
                                columnNames += dataGridView1.Columns[i].HeaderText.ToString() + ",";
                            }
                            outputCsv[0] += columnNames;

                            for (int i = 1; (i - 1) < dataGridView1.Rows.Count; i++)
                            {
                                for (int j = 0; j < columnCount; j++)
                                {
                                    outputCsv[i] += dataGridView1.Rows[i - 1].Cells[j].Value.ToString() + ",";
                                }
                            }

                            File.WriteAllLines(sfd.FileName, outputCsv, Encoding.UTF8);
                            MessageBox.Show("Data Exported Successfully !!!", "Info");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error :" + ex.Message);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("No Record To Export !!!", "Info");
            }
        }
        //-----------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------
        private void dataGridView1_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            // Show Line Number
            var grid = sender as DataGridView;
            var rowIdx = (e.RowIndex + 1).ToString();

            var centerFormat = new StringFormat()
            {
                // right alignment might actually make more sense for numbers
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            var headerBounds = new Rectangle(e.RowBounds.Left, e.RowBounds.Top, grid.RowHeadersWidth, e.RowBounds.Height);
            e.Graphics.DrawString(rowIdx, this.Font, SystemBrushes.ControlText, headerBounds, centerFormat);
        }
        //-----------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------
        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            datetime_from = dateTimePicker1.Value.ToString("yyyy-MM-dd HH:mm:ss");

            clear();
        }
        //-----------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------
        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            datetime_to = dateTimePicker2.Value.ToString("yyyy-MM-dd HH:mm:ss");

            clear();
        }
        //-----------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------
        void ReadDatabase_To_GridView()
        {
            int cnt_ok = 0;
            int cnt_ng = 0;
            int cnt_actual = 0;

            dataGridView1.DataSource = null;
            dataGridView1.Rows.Clear();
            dataGridView1.Refresh();

            DataTable dataTable = new DataTable();

            try
            {
                string query = "";

                if (comboBox3.Text == "All")
                {
                    query = "SELECT * FROM tbl_AutoSyncData WHERE DateTime BETWEEN '" + datetime_from + "' AND '" + datetime_to + "' AND LineNumber=" + "'" + comboBox1.Text + "'" + " AND MC_Name=" + "'" + comboBox2.Text + "'" + " ORDER BY DateTime asc ";
                }
                else if (comboBox3.Text == "OK")
                {
                    query = "SELECT * FROM tbl_AutoSyncData WHERE DateTime BETWEEN '" + datetime_from + "' AND '" + datetime_to + "' AND LineNumber=" + "'" + comboBox1.Text + "'" + " AND MC_Name=" + "'" + comboBox2.Text + "'" + " AND Result='OK' ORDER BY DateTime asc ";
                }
                else if (comboBox3.Text == "NG")
                {
                    query = "SELECT * FROM tbl_AutoSyncData WHERE DateTime BETWEEN '" + datetime_from + "' AND '" + datetime_to + "' AND LineNumber=" + "'" + comboBox1.Text + "'" + " AND MC_Name=" + "'" + comboBox2.Text + "'" + " AND Result='NG' ORDER BY DateTime asc ";
                }

                SqlConnection conn = new SqlConnection(myConnection);
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();

                // create data adapter
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);

                DataSet ds = new DataSet();
                adapter.Fill(ds);

                if (ds != null)
                {
                    if (ds.Tables[0].Rows.Count != 0)
                    {
                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            dataGridView1.Rows.Add();

                            dataGridView1.Rows[i].Cells[0].Value = ds.Tables[0].Rows[i]["Date"].ToString();
                            dataGridView1.Rows[i].Cells[1].Value = ds.Tables[0].Rows[i]["Time"].ToString();
                            dataGridView1.Rows[i].Cells[2].Value = ds.Tables[0].Rows[i]["Bank"].ToString();
                            dataGridView1.Rows[i].Cells[3].Value = ds.Tables[0].Rows[i]["No"].ToString();
                            dataGridView1.Rows[i].Cells[4].Value = ds.Tables[0].Rows[i]["WeldTime"].ToString();
                            dataGridView1.Rows[i].Cells[5].Value = ds.Tables[0].Rows[i]["HoldTime"].ToString();
                            dataGridView1.Rows[i].Cells[6].Value = ds.Tables[0].Rows[i]["Collapse"].ToString();
                            dataGridView1.Rows[i].Cells[7].Value = ds.Tables[0].Rows[i]["Energy"].ToString();
                            dataGridView1.Rows[i].Cells[8].Value = ds.Tables[0].Rows[i]["PeakPower"].ToString();
                            dataGridView1.Rows[i].Cells[9].Value = ds.Tables[0].Rows[i]["Pressure"].ToString();
                            dataGridView1.Rows[i].Cells[10].Value = ds.Tables[0].Rows[i]["CollapseControlMode"].ToString();

                            dataGridView1.Rows[i].Cells[11].Value = ds.Tables[0].Rows[i]["EnergyControlMode"].ToString();
                            dataGridView1.Rows[i].Cells[12].Value = ds.Tables[0].Rows[i]["ControlResult"].ToString();
                            dataGridView1.Rows[i].Cells[13].Value = ds.Tables[0].Rows[i]["WeldTimeEvaluation"].ToString();
                            dataGridView1.Rows[i].Cells[14].Value = ds.Tables[0].Rows[i]["CollapseEvaluation"].ToString();
                            dataGridView1.Rows[i].Cells[15].Value = ds.Tables[0].Rows[i]["EnergyEvaluation"].ToString();
                            dataGridView1.Rows[i].Cells[16].Value = ds.Tables[0].Rows[i]["PeakPowerEvaluation"].ToString();
                            dataGridView1.Rows[i].Cells[17].Value = ds.Tables[0].Rows[i]["PressureEvaluation"].ToString();
                            dataGridView1.Rows[i].Cells[18].Value = ds.Tables[0].Rows[i]["WeldDelayTime"].ToString();
                            dataGridView1.Rows[i].Cells[19].Value = ds.Tables[0].Rows[i]["PressureDelayTime"].ToString();
                            dataGridView1.Rows[i].Cells[20].Value = ds.Tables[0].Rows[i]["CollapseSetup"].ToString();

                            dataGridView1.Rows[i].Cells[21].Value = ds.Tables[0].Rows[i]["TotalCollapseSetup"].ToString();
                            dataGridView1.Rows[i].Cells[22].Value = ds.Tables[0].Rows[i]["EnergySetup"].ToString();
                            dataGridView1.Rows[i].Cells[23].Value = ds.Tables[0].Rows[i]["PowerLevel"].ToString();
                            dataGridView1.Rows[i].Cells[24].Value = ds.Tables[0].Rows[i]["SoftStart"].ToString();
                            dataGridView1.Rows[i].Cells[25].Value = ds.Tables[0].Rows[i]["DownSpeed1"].ToString();
                            dataGridView1.Rows[i].Cells[26].Value = ds.Tables[0].Rows[i]["DownSpeed2"].ToString();
                            dataGridView1.Rows[i].Cells[27].Value = ds.Tables[0].Rows[i]["TriggerType"].ToString();
                            dataGridView1.Rows[i].Cells[28].Value = ds.Tables[0].Rows[i]["TouchResponse"].ToString();


                            string a = Convert.ToDateTime(ds.Tables[0].Rows[i]["DateTime"].ToString()).ToString("yyyy-MM-dd HH:mm:ss");
                            string date_ = DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss");

                            DateTime dt = DateTime.ParseExact(a, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                            //dataGridView1.Rows[i].Cells[29].Value = dt.ToString("dd-MM-yyyy HH:mm:ss");
                            dataGridView1.Rows[i].Cells[29].Value = dt.ToString("yyyy-MM-dd HH:mm:ss");


                            dataGridView1.Rows[i].Cells[30].Value = ds.Tables[0].Rows[i]["MC_Name"].ToString();


                            dataGridView1.Rows[i].Cells[31].Value = ds.Tables[0].Rows[i]["Result"].ToString();
                            string res = ds.Tables[0].Rows[i]["Result"].ToString();

                            if (res == "OK")
                            {
                                cnt_ok++;
                            }
                            else
                            {
                                cnt_ng++;
                            }

                            cnt_actual = cnt_ok + cnt_ng;

                                label112.Text = cnt_actual.ToString();
                                label111.Text = cnt_ok.ToString();
                                label110.Text = cnt_ng.ToString();
 


                            dataGridView1.Rows[i].Cells[32].Value = ds.Tables[0].Rows[i]["LineNumber"].ToString();

                        }
                    }
                    else
                    {
                        MessageBox.Show("No data!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                

                conn.Close();
                ds.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());               
            }
        }
        //-----------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------
        void clear()
        {
            dataGridView1.Rows.Clear();
            dataGridView1.Refresh();

            label112.Text = "";
            label111.Text = "";
            label110.Text = "";

        }
        //-----------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            clear();
        }
        //-----------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            clear();
        }
        //-----------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------
        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            clear();
        }
        //-----------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------
        private void button6_Click(object sender, EventArgs e)
        {
            filename = comboBox1.Text + "_" + comboBox2.Text;
            ExportCSV(filename);
        }
        //-----------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------
        private void button5_Click(object sender, EventArgs e)
        {
            if (comboBox2.Text != "")
            {
                ReadDatabase_To_GridView();
            }
            else
            {
                MessageBox.Show("Please Select Machine Name", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        //-----------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------

    }
}
