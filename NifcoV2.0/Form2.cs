using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NifcoV2._0
{
    public partial class Form2 : Form
    {
        string datetime_from = null;
        string datetime_to = null;
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public Form2()
        {
            InitializeComponent();
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void Form2_Load(object sender, EventArgs e)
        {
            

            comboBox1.Items.Add(Form1.Line1_ini);
            comboBox1.Items.Add(Form1.Line2_ini);
            comboBox1.Items.Add(Form1.Line3_ini);
            comboBox1.Items.Add(Form1.Line4_ini);
            comboBox1.Items.Add(Form1.Line5_ini);
            comboBox1.Items.Add(Form1.Line6_ini);

            comboBox2.Items.Add(Form1.Machine_Type1_ini);
            comboBox2.Items.Add(Form1.Machine_Type2_ini);

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
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
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
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        void clear()
        {
            dataGridView1.Rows.Clear();
            dataGridView1.Refresh();

            label112.Text = "";
            label111.Text = "";
            label110.Text = "";

        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            datetime_from = dateTimePicker1.Value.ToString("yyyy-MM-dd HH:mm:ss");

            clear();
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            datetime_to = dateTimePicker2.Value.ToString("yyyy-MM-dd HH:mm:ss");

            clear();
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
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
                string all = "Date, Time, Bank, No, WeldTime, HoldTime, Collapse, Energy, PeakPower, Pressure, CollapseControlMode, EnergyControlMode, ControlResult, WeldTimeEvaluation, CollapseEvaluation, EnergyEvaluation, PeakPowerEvaluation, PressureEvaluation, WeldDelayTime, PressureDelayTime, CollapseSetup, TotalCollapseSetup, EnergySetup, PowerLevel, SoftStart, DownSpeed1, DownSpeed2, TriggerType, TouchResponse, DateTime, MC_Name, Result, LineNumber";

                string query = "";

                if (comboBox3.Text == "All")
                {
                    query = "SELECT " + all + " FROM tbl_AutoSyncData WHERE DateTime BETWEEN '" + datetime_from + "' AND '" + datetime_to + "' AND LineNumber=" + "'" + comboBox1.Text + "'" + " AND MC_Name=" + "'" + comboBox2.Text + "'" + " GROUP BY " + all + " ORDER BY DateTime asc ";
                }
                else if (comboBox3.Text == "OK")
                {
                    query = "SELECT " + all + " FROM tbl_AutoSyncData WHERE DateTime BETWEEN '" + datetime_from + "' AND '" + datetime_to + "' AND LineNumber=" + "'" + comboBox1.Text + "'" + " AND MC_Name=" + "'" + comboBox2.Text + "'" + " AND Result='OK' " + " GROUP BY " + all + " ORDER BY DateTime asc ";
                }
                else if (comboBox3.Text == "NG")
                {
                    query = "SELECT " + all + " FROM tbl_AutoSyncData WHERE DateTime BETWEEN '" + datetime_from + "' AND '" + datetime_to + "' AND LineNumber=" + "'" + comboBox1.Text + "'" + " AND MC_Name=" + "'" + comboBox2.Text + "'" + " AND Result='NG' " + " GROUP BY " + all + " ORDER BY DateTime asc ";
                }

                SqlConnection conn = new SqlConnection(Form1.myConnection);
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
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            clear();
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            clear();
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void button5_Click(object sender, EventArgs e)
        {
            if (comboBox2.Text != "")
            {
                label12.Visible = true;
                ReadDatabase_To_GridView();
                label12.Visible = false;
            }
            else
            {
                label12.Visible = false;
                MessageBox.Show("Please Select Machine Name", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void button1_Click(object sender, EventArgs e)
        {
            Form3 frm = new Form3();
            frm.ShowDialog();            
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void button2_Click(object sender, EventArgs e)
        {
            string filename = comboBox1.Text + "_" + comboBox2.Text;
            ExportCSV(filename);
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
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
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            clear();
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    }
}
