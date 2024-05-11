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
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Net;
using System.IO;
using Gajatko.IniFiles;
using Microsoft.Win32;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using SuperSimpleTcp;

namespace NifcoV2._0
{   
    public partial class Form1 : Form
    {
        // ปิด Sleep หน้าจอ , ปิด Screen Saver
        [FlagsAttribute]
        public enum EXECUTION_STATE : uint
        {
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS = 0x80000000,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_SYSTEM_REQUIRED = 0x00000001
            // Legacy flag, should not be used.
            // ES_USER_PRESENT = 0x00000004
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

        SimpleTcpServer server;
        //----------------------------------------------------------------------------

        public static string myConnection = null;

        string Connect_MC_ini = null;

        string ServerDB_ini = null;
        string User_ini = null;
        string Password_ini = null;
        string Timeout_ini = null;
        string Database_ini = null;

        public static string Admin_User_ini = null;
        public static string Admin_Pass_ini = null;

        string Server_IP_Port_ini = null;

        //public static string[] MC_IP_ini = new string[12];


        public static string Line1_ini = null;
        public static string Line2_ini = null;
        public static string Line3_ini = null;
        public static string Line4_ini = null;
        public static string Line5_ini = null;
        public static string Line6_ini = null;

        public static string Machine_Type1_ini = null;
        public static string Machine_Type2_ini = null;

        public static string Timeout_Idle_ini = null;

        public static string[] MC_Connected = new string[12];


        public static bool[] Status_OK = new bool[13];
        public static bool[] Status_NG = new bool[13];

        public static int[] Cnt_OK = new int[13];
        public static int[] Cnt_NG = new int[13];

        int[] timeout_running = new int[13];
        bool[] f_running = new bool[13];

        //public static bool[] f_disconnect = new bool[12];

        StringBuilder messageData = new StringBuilder();

        int cntWrongFormmat = 0;
        string[] ip_port = new string[13];
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public Form1()
        {
            InitializeComponent();
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public int Is_Inserted(string dateTime, string machineName, string lineNumber)
        {
            // SELECT COUNT(*) FROM [NIFCO].[dbo].[tbl_AutoSyncData] WHERE DateTime='2023-03-20 15:50:13:000' AND MC_Name = 'Body-Bone Welding' AND LineNumber='FA-01-3' 

            string stmt = "SELECT COUNT(*) FROM tbl_AutoSyncData WHERE DateTime='" + dateTime + "' AND MC_Name = '" + machineName + "' AND LineNumber = '" + lineNumber + "'";
            int count = 0;

            using (SqlConnection cn = new SqlConnection(myConnection))
            {
                using (SqlCommand cmdCount = new SqlCommand(stmt, cn))
                {
                    cn.Open();
                    count = (int)cmdCount.ExecuteScalar();
                }
            }
            return count;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public bool Insert_Database(string id,  string MachineName, string LineNumber, string dtFormat, List<string> data)
        {
            bool res_ = false;

            SqlConnection cn = new SqlConnection(myConnection);

            try
            {
                SqlCommand cmd = new SqlCommand();
                SqlTransaction myTran = null;

                cmd = cn.CreateCommand();
                cmd.Connection = cn;

                string sql_str = "INSERT INTO tbl_AutoSyncData(" +
                    "Date," +
                    "Time," +
                    "Bank," +
                    "No," +
                    "WeldTime," +
                    "HoldTime," +
                    "Collapse," +
                    "Energy," +
                    "PeakPower," +
                    "Pressure," +
                    "CollapseControlMode," +
                    "EnergyControlMode," +
                    "ControlResult," +

                    "WeldTimeEvaluation," +
                    "CollapseEvaluation," +
                    "EnergyEvaluation," +
                    "PeakPowerEvaluation," +
                    "PressureEvaluation," +

                    "WeldDelayTime," +
                    "PressureDelayTime," +
                    "CollapseSetup," +
                    "TotalCollapseSetup," +
                    "EnergySetup," +
                    "PowerLevel," +

                    "SoftStart," +

                    "DownSpeed1," +
                    "DownSpeed2," +

                    "TriggerType," +

                    "TouchResponse," +

                    "DateTime," +
                    "MC_Name," +
                    "Result," +
                    "LineNumber)values (" +
                    "@buf_Date," +
                    "@buf_Time," +
                    "@buf_Bank," +
                    "@buf_No," +
                    "@buf_WeldTime," +
                    "@buf_HoldTime," +
                    "@buf_Collapse," +
                    "@buf_Energy," +
                    "@buf_PeakPower," +
                    "@buf_Pressure," +
                    "@buf_CollapseControlMode," +
                    "@buf_EnergyControlMode," +
                    "@buf_ControlResult," +

                    "@buf_WeldTimeEvaluation," +
                    "@buf_CollapseEvaluation," +
                    "@buf_EnergyEvaluation," +
                    "@buf_PeakPowerEvaluation," +
                    "@buf_PressureEvaluation," +

                    "@buf_WeldDelayTime," +
                    "@buf_PressureDelayTime," +
                    "@buf_CollapseSetup," +
                    "@buf_TotalCollapseSetup," +
                    "@buf_EnergySetup," +
                    "@buf_PowerLevel," +

                    "@buf_SoftStart," +

                    "@buf_DownSpeed1," +
                    "@buf_DownSpeed2," +

                    "@buf_TriggerType," +

                    "@buf_TouchResponse," +

                    "@buf_DateTime," +
                    "@buf_MC_Name," +
                    "@buf_Result," +
                    "@buf_LineNumber)";


                if (cn.State == ConnectionState.Closed)
                {
                    cn.Open();
                }

                myTran = cn.BeginTransaction(IsolationLevel.ReadCommitted);
                cmd.CommandText = sql_str;
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@buf_Date", data[1].Substring(3, 6));
                cmd.Parameters.AddWithValue("@buf_Time", data[2]);
                cmd.Parameters.AddWithValue("@buf_Bank", data[3]);
                cmd.Parameters.AddWithValue("@buf_No", data[4]);
                cmd.Parameters.AddWithValue("@buf_WeldTime", data[5]);
                cmd.Parameters.AddWithValue("@buf_HoldTime", data[6]);
                cmd.Parameters.AddWithValue("@buf_Collapse", data[7]);
                cmd.Parameters.AddWithValue("@buf_Energy", data[8]);
                cmd.Parameters.AddWithValue("@buf_PeakPower", data[9]);
                cmd.Parameters.AddWithValue("@buf_Pressure", data[10]);


                // Collapse Control Mode
                switch(data[11])
                {
                    case "0":
                        cmd.Parameters.AddWithValue("@buf_CollapseControlMode", "Monitor");
                        break;
                    case "1":
                        cmd.Parameters.AddWithValue("@buf_CollapseControlMode", "Collapse");
                        break;
                    case "2":
                        cmd.Parameters.AddWithValue("@buf_CollapseControlMode", "Total collaspe");
                        break;
                    case "3":
                        cmd.Parameters.AddWithValue("@buf_CollapseControlMode", "Absolute");
                        break;
                    case "4":
                        cmd.Parameters.AddWithValue("@buf_CollapseControlMode", "Total absolute");
                        break;
                    default:
                        cmd.Parameters.AddWithValue("@buf_CollapseControlMode", data[11]);
                        break;
                }


                // Energy Mode
                switch (data[12])
                {
                    case "0":
                        cmd.Parameters.AddWithValue("@buf_EnergyControlMode", "Monitor");
                        break;
                    case "1":
                        cmd.Parameters.AddWithValue("@buf_EnergyControlMode", "Energy");
                        break;
                    default:
                        cmd.Parameters.AddWithValue("@buf_EnergyControlMode", data[12]);
                        break;

                }


                // Control Result
                switch (data[13])
                {
                    case "0":
                        cmd.Parameters.AddWithValue("@buf_ControlResult", "Weld time");
                        break;
                    case "1":
                        cmd.Parameters.AddWithValue("@buf_ControlResult", "Hold time");
                        break;
                    case "2":
                        cmd.Parameters.AddWithValue("@buf_ControlResult", "Collapse");
                        break;
                    case "3":
                        cmd.Parameters.AddWithValue("@buf_ControlResult", "Total collapse");
                        break;
                    case "4":
                        cmd.Parameters.AddWithValue("@buf_ControlResult", "Absolute");
                        break;
                    case "5":
                        cmd.Parameters.AddWithValue("@buf_ControlResult", "Total absolute");
                        break;
                    case "6":
                    case "8":
                        cmd.Parameters.AddWithValue("@buf_ControlResult", "Energy");
                        break;
                    default:
                        cmd.Parameters.AddWithValue("@buf_ControlResult", data[13]);
                        break;

                }

                // 0=OK , 1=NG
                switch (data[14])
                {
                    case "0":
                        cmd.Parameters.AddWithValue("@buf_WeldTimeEvaluation", "Good");
                        break;
                    case "1":
                        cmd.Parameters.AddWithValue("@buf_WeldTimeEvaluation", "No Good");
                        break;                   
                    default:
                        cmd.Parameters.AddWithValue("@buf_WeldTimeEvaluation", data[14]);
                        break;

                }

                // 0=OK , 1=NG
                switch (data[15])
                {
                    case "0":
                        cmd.Parameters.AddWithValue("@buf_CollapseEvaluation", "Good");
                        break;
                    case "1":
                        cmd.Parameters.AddWithValue("@buf_CollapseEvaluation", "No Good");
                        break;
                    default:
                        cmd.Parameters.AddWithValue("@buf_CollapseEvaluation", data[15]);
                        break;
                }

                // 0=OK , 1=NG
                switch (data[16])
                {
                    case "0":
                        cmd.Parameters.AddWithValue("@buf_EnergyEvaluation", "Good");
                        break;
                    case "1":
                        cmd.Parameters.AddWithValue("@buf_EnergyEvaluation", "No Good");
                        break;
                    default:
                        cmd.Parameters.AddWithValue("@buf_EnergyEvaluation", data[16]);
                        break;
                }


                // 0=OK , 1=NG
                switch (data[17])
                {
                    case "0":
                        cmd.Parameters.AddWithValue("@buf_PeakPowerEvaluation", "Good");
                        break;
                    case "1":
                        cmd.Parameters.AddWithValue("@buf_PeakPowerEvaluation", "No Good");
                        break;
                    default:
                        cmd.Parameters.AddWithValue("@buf_PeakPowerEvaluation", data[17]);
                        break;
                }


                // 0=OK , 1=NG
                switch (data[18])
                {
                    case "0":
                        cmd.Parameters.AddWithValue("@buf_PressureEvaluation", "Good");
                        break;
                    case "1":
                        cmd.Parameters.AddWithValue("@buf_PressureEvaluation", "No Good");
                        break;
                    default:
                        cmd.Parameters.AddWithValue("@buf_PressureEvaluation", data[18]);
                        break;
                }


                if ((data[14] == "0") && (data[15] == "0") && (data[16] == "0") && (data[17] == "0") && (data[18] == "0"))   // OK
                {
                    Status_OK[int.Parse(id)] = true;
                    Cnt_OK[int.Parse(id)]++;

                    cmd.Parameters.AddWithValue("@buf_MC_Name", MachineName);
                    cmd.Parameters.AddWithValue("@buf_Result", "OK");
                    cmd.Parameters.AddWithValue("@buf_LineNumber", LineNumber);                                            
                }
                else // NG
                {
                    Status_NG[int.Parse(id)] = true;
                    Cnt_NG[int.Parse(id)]++;
                           
                    cmd.Parameters.AddWithValue("@buf_MC_Name", MachineName);
                    cmd.Parameters.AddWithValue("@buf_Result", "NG");
                    cmd.Parameters.AddWithValue("@buf_LineNumber", LineNumber);                     
                }

                cmd.Parameters.AddWithValue("@buf_WeldDelayTime", data[19]);
                cmd.Parameters.AddWithValue("@buf_PressureDelayTime", data[20]);
                cmd.Parameters.AddWithValue("@buf_CollapseSetup", data[21]);
                cmd.Parameters.AddWithValue("@buf_TotalCollapseSetup", data[22]);
                cmd.Parameters.AddWithValue("@buf_EnergySetup", data[23]);
                cmd.Parameters.AddWithValue("@buf_PowerLevel", data[24]);


                // Soft Start
                switch (data[25])
                {
                    case "1":
                        cmd.Parameters.AddWithValue("@buf_SoftStart", "0.025");
                        break;
                    case "2":
                        cmd.Parameters.AddWithValue("@buf_SoftStart", "0.050");
                        break;
                    case "3":
                        cmd.Parameters.AddWithValue("@buf_SoftStart", "0.100");
                        break;
                    case "4":
                        cmd.Parameters.AddWithValue("@buf_SoftStart", "0.150");
                        break;
                    case "5":
                        cmd.Parameters.AddWithValue("@buf_SoftStart", "0.200");
                        break;
                    default:
                        cmd.Parameters.AddWithValue("@buf_SoftStart", data[25]);
                        break;
                }

                cmd.Parameters.AddWithValue("@buf_DownSpeed1", data[26]);
                cmd.Parameters.AddWithValue("@buf_DownSpeed2", data[27]);

                // Trigger Type
                switch (data[28])
                {
                    case "0":
                        cmd.Parameters.AddWithValue("@buf_TriggerType", "Start");
                        break;
                    case "1":
                        cmd.Parameters.AddWithValue("@buf_TriggerType", "Swith");
                        break;
                    case "2":
                        cmd.Parameters.AddWithValue("@buf_TriggerType", "Touch");
                        break;
                    case "3":
                        cmd.Parameters.AddWithValue("@buf_TriggerType", "Distance");
                        break;
                    default:
                        cmd.Parameters.AddWithValue("@buf_TriggerType", data[28]);
                        break;
                }

                // Touch Response
                switch (data[29])
                {
                    case "0":
                        cmd.Parameters.AddWithValue("@buf_TouchResponse", "Fast");
                        break;
                    case "1":
                        cmd.Parameters.AddWithValue("@buf_TouchResponse", "Standard");
                        break;
                    case "2":
                        cmd.Parameters.AddWithValue("@buf_TouchResponse", "Slow");
                        break;
                    default:
                        cmd.Parameters.AddWithValue("@buf_TouchResponse", data[29]);
                        break;
                }

                cmd.Parameters.AddWithValue("@buf_DateTime", dtFormat);

                cmd.Transaction = myTran;
                int a = cmd.ExecuteNonQuery();

                //commit db
                myTran.Commit();

                if (a == 0)// record error
                {
                    res_ = false;
                }
                else// complete record
                {
                    res_ = true;
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
                //MessageBox.Show(MachineIP + "   " + MachineIP);
                res_ = false;
                ErrorLogging(ex);
            }
            finally
            {
                cn.Close();
            }


            SaveCounter(id, Cnt_OK[int.Parse(id)], Cnt_NG[int.Parse(id)], Status_OK[int.Parse(id)], Status_NG[int.Parse(id)]);


            return res_;
        }
        //--------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------
        public void SaveCounter(string id_, int ok_, int ng_, bool ok_color_, bool ng_color_)
        {
            try
            {
                string query = @"IF EXISTS(SELECT * FROM tbl_maindisplay WHERE id = @id) 
                                    UPDATE tbl_maindisplay SET ok=@ok, ng=@ng, ok_color=@ok_color, ng_color=@ng_color
                                    WHERE id = @id 
                                ELSE 
                                    INSERT INTO tbl_maindisplay(ok, ng, ok_color, ng_color) VALUES(@ok, @ng, @ok_color, @ng_color);";

                // create connection and command in "using" blocks
                using (SqlConnection conn = new SqlConnection(myConnection))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = id_;
                    cmd.Parameters.Add("@ok", SqlDbType.Int).Value = ok_;
                    cmd.Parameters.Add("@ng", SqlDbType.Int).Value = ng_;
                    cmd.Parameters.Add("@ok_color", SqlDbType.Bit).Value = ok_color_;
                    cmd.Parameters.Add("@ng_color", SqlDbType.Bit).Value = ng_color_;

                    // open connection, execute query, close connection
                    conn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                ErrorLogging(ex);
                //MessageBox.Show(ex.ToString());
            }
        }
        //--------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------
        void Display_Run_Idle(int id, string status)
        {
            if (id == 1)
            {
                if (status == "True")
                {
                    label98.Text = "Run";
                    label98.BackColor = Color.Lime;
                }
                else
                {
                    label98.Text = "Idle";
                    label98.BackColor = Color.Gray;
                }
            }
            else if (id == 2)
            {
                if (status == "True")
                {
                    label99.Text = "Run";
                    label99.BackColor = Color.Lime;
                }
                else
                {
                    label99.Text = "Idle";
                    label99.BackColor = Color.Gray;
                }
            }
            else if (id == 3)
            {
                if (status == "True")
                {
                    label100.Text = "Run";
                    label100.BackColor = Color.Lime;
                }
                else
                {
                    label100.Text = "Idle";
                    label100.BackColor = Color.Gray;
                }
            }
            else if (id == 4)
            {
                if (status == "True")
                {
                    label101.Text = "Run";
                    label101.BackColor = Color.Lime;
                }
                else
                {
                    label101.Text = "Idle";
                    label101.BackColor = Color.Gray;
                }
            }
            else if (id == 5)
            {
                if (status == "True")
                {
                    label102.Text = "Run";
                    label102.BackColor = Color.Lime;
                }
                else
                {
                    label102.Text = "Idle";
                    label102.BackColor = Color.Gray;
                }
            }
            else if (id == 6)
            {
                if (status == "True")
                {
                    label103.Text = "Run";
                    label103.BackColor = Color.Lime;
                }
                else
                {
                    label103.Text = "Idle";
                    label103.BackColor = Color.Gray;
                }
            }
            else if (id == 7)
            {
                if (status == "True")
                {
                    label104.Text = "Run";
                    label104.BackColor = Color.Lime;
                }
                else
                {
                    label104.Text = "Idle";
                    label104.BackColor = Color.Gray;
                }
            }
            else if (id == 8)
            {
                if (status == "True")
                {
                    label105.Text = "Run";
                    label105.BackColor = Color.Lime;
                }
                else
                {
                    label105.Text = "Idle";
                    label105.BackColor = Color.Gray;
                }
            }
            else if (id == 9)
            {
                if (status == "True")
                {
                    label106.Text = "Run";
                    label106.BackColor = Color.Lime;
                }
                else
                {
                    label106.Text = "Idle";
                    label106.BackColor = Color.Gray;
                }
            }
            else if (id == 10)
            {
                if (status == "True")
                {
                    label107.Text = "Run";
                    label107.BackColor = Color.Lime;
                }
                else
                {
                    label107.Text = "Idle";
                    label107.BackColor = Color.Gray;
                }
            }
            else if (id == 11)
            {
                if (status == "True")
                {
                    label108.Text = "Run";
                    label108.BackColor = Color.Lime;
                }
                else
                {
                    label108.Text = "Idle";
                    label108.BackColor = Color.Gray;
                }
            }
            else if (id == 12)
            {
                if (status == "True")
                {
                    label109.Text = "Run";
                    label109.BackColor = Color.Lime;
                }
                else
                {
                    label109.Text = "Idle";
                    label109.BackColor = Color.Gray;
                }
            }
        }
        //--------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------
        public void WrongDataLogging(string id, string ip, string data)
        {
            try
            {
                //string strPath = @"C:\AutoSyncData_Log\Error_log.txt";

                string path = Directory.GetCurrentDirectory();
                string strPath = path + "\\Log\\WrongData.txt";

                if (!File.Exists(strPath))
                {
                    File.Create(strPath).Dispose();
                }

                string dt = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");

                byte[] ba = Encoding.Default.GetBytes(data);
                var hexString = BitConverter.ToString(ba);
                hexString = hexString.Replace("-", "");

                using (StreamWriter sw = File.AppendText(strPath))
                {
                    sw.WriteLine(dt + "    ID: " + id + "    IP: " + ip  + "    Data: " + data + "    " + hexString);
                    sw.Close();
                }

            }
            catch (Exception ex1)
            {
                //MessageBox.Show(ex1.ToString());
            }
        }
        //--------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------
        public void ErrorLogging(Exception ex)
        {
            try
            {
                //string strPath = @"C:\AutoSyncData_Log\Error_log.txt";

                string path = Directory.GetCurrentDirectory();
                string strPath = path + "\\Log\\Error.txt";

                if (!File.Exists(strPath))
                {
                    File.Create(strPath).Dispose();
                }

                string dt = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");

                using (StreamWriter sw = File.AppendText(strPath))
                {
                    //sw.WriteLine("=============Error Logging ===========");
                    sw.WriteLine("DateTime: " + dt);
                    sw.WriteLine("Error Message: " + ex.Message);
                    sw.WriteLine("Stack Trace: " + ex.StackTrace);
                    sw.WriteLine("===========End=============");
                    sw.WriteLine("\r\n\r\n");
                    sw.Close();
                }

            }
            catch (Exception ex1)
            {
                //MessageBox.Show(ex1.ToString());
            }
        }
        //--------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------
        public void ResetLogging(int id, string ok, string ng)
        {
            try
            {
                string line_num = "";
                string mc_name = "";


                if (id == 1)
                {
                    line_num = Line1_ini;
                    mc_name = Machine_Type1_ini;
                }
                else if (id == 2)
                {
                    line_num = Line1_ini;
                    mc_name = Machine_Type2_ini;
                }
                else if (id == 3)
                {
                    line_num = Line2_ini;
                    mc_name = Machine_Type1_ini;
                }
                else if (id == 4)
                {
                    line_num = Line2_ini;
                    mc_name = Machine_Type2_ini;
                }
                else if (id == 5)
                {
                    line_num = Line3_ini;
                    mc_name = Machine_Type1_ini;
                }
                else if (id == 6)
                {
                    line_num = Line3_ini;
                    mc_name = Machine_Type2_ini;
                }
                else if (id == 7)
                {
                    line_num = Line4_ini;
                    mc_name = Machine_Type1_ini;
                }
                else if (id == 8)
                {
                    line_num = Line4_ini;
                    mc_name = Machine_Type2_ini;
                }
                else if (id == 9)
                {
                    line_num = Line5_ini;
                    mc_name = Machine_Type1_ini;
                }
                else if (id == 10)
                {
                    line_num = Line5_ini;
                    mc_name = Machine_Type2_ini;
                }
                else if (id == 11)
                {
                    line_num = Line6_ini;
                    mc_name = Machine_Type1_ini;
                }
                else if (id == 12)
                {
                    line_num = Line6_ini;
                    mc_name = Machine_Type2_ini;
                }
                


                //string strPath = @"C:\AutoSyncData_Log\Reset_log.txt";

                string path = Directory.GetCurrentDirectory();
                string strPath = path + "\\Log\\Reset.txt";

                if (!File.Exists(strPath))
                {
                    File.Create(strPath).Dispose();
                }

                string actual = (int.Parse(ok) + int.Parse(ng)).ToString();

                string dt = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");

                using (StreamWriter sw = File.AppendText(strPath))
                {
                    sw.WriteLine("ID: " + id + "    " + "Time: " + dt + "    " + "Line: " + line_num + "    " + "Name: " + mc_name + "    " + "Actual: " + actual + "    " + "OK: " + ok + "    " + "NG: " + ng);
                    sw.Close();
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }
        }
        //--------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------
        public void read_ini_config()
        {
            try
            {
                IniFile cn_file = IniFile.FromFile("Config.ini");

                Connect_MC_ini = cn_file["CONFIG"]["CONNECT_MC"];

                ServerDB_ini = cn_file["CONFIG"]["SERVER_DB"];
                User_ini = cn_file["CONFIG"]["USER_DB"];
                Password_ini = cn_file["CONFIG"]["PASSWORD_DB"];
                Timeout_ini = cn_file["CONFIG"]["TIMEOUT_DB"];
                Database_ini = cn_file["CONFIG"]["DATABASE_NAME_DB"];

                Server_IP_Port_ini = cn_file["CONFIG"]["SERVER_IP_PORT"];

                /*MC_IP_ini[0] = cn_file["CONFIG"]["MC1_IP"];
                MC_IP_ini[1] = cn_file["CONFIG"]["MC2_IP"];
                MC_IP_ini[2] = cn_file["CONFIG"]["MC3_IP"];
                MC_IP_ini[3] = cn_file["CONFIG"]["MC4_IP"];
                MC_IP_ini[4] = cn_file["CONFIG"]["MC5_IP"];
                MC_IP_ini[5] = cn_file["CONFIG"]["MC6_IP"];
                MC_IP_ini[6] = cn_file["CONFIG"]["MC7_IP"];
                MC_IP_ini[7] = cn_file["CONFIG"]["MC8_IP"];
                MC_IP_ini[8] = cn_file["CONFIG"]["MC9_IP"];
                MC_IP_ini[9] = cn_file["CONFIG"]["MC10_IP"];
                MC_IP_ini[10] = cn_file["CONFIG"]["MC11_IP"];
                MC_IP_ini[11] = cn_file["CONFIG"]["MC12_IP"];*/



                Line1_ini = cn_file["CONFIG"]["LINE1"];
                Line2_ini = cn_file["CONFIG"]["LINE2"];
                Line3_ini = cn_file["CONFIG"]["LINE3"];
                Line4_ini = cn_file["CONFIG"]["LINE4"];
                Line5_ini = cn_file["CONFIG"]["LINE5"];
                Line6_ini = cn_file["CONFIG"]["LINE6"];


                Timeout_Idle_ini = cn_file["CONFIG"]["TIMEOUT_IDLE"];

                Machine_Type1_ini = cn_file["CONFIG"]["MACHINE_TYPE1"];
                Machine_Type2_ini = cn_file["CONFIG"]["MACHINE_TYPE2"];

                Admin_User_ini = cn_file["CONFIG"]["ADMIN_USER"];
                Admin_Pass_ini = cn_file["CONFIG"]["ADMIN_PASSWORD"];

            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
                ErrorLogging(ex);
            }
        }
        //--------------------------------------------------------------------------------
        //--------------------------------------------------------------------------------
        public void InsertFirst(int id_)
        {
            try
            {
                string query = @"INSERT INTO tbl_maindisplay (id) SELECT @id WHERE NOT EXISTS (SELECT * FROM tbl_maindisplay WHERE id=@id);";

                // create connection and command in "using" blocks
                using (SqlConnection conn = new SqlConnection(myConnection))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = id_;

                    // open connection, execute query, close connection
                    conn.Open();

                    int rowsAffected = cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
                ErrorLogging(ex);
            }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public void Read_MainDisplay(int id, Label Reset, Label Actual, Label OK, Label NG)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(myConnection))
                {
                    con.Open();

                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM tbl_maindisplay WHERE id='" + id + "'", con))
                    {
                        using (IDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {

                                // Counter
                                OK.Text = "OK      :  " + dr[3].ToString() + " Units";
                                NG.Text = "NG      :  " + dr[4].ToString() + " Units";
                                Actual.Text = "Actual :  " + (int.Parse(dr[3].ToString()) + int.Parse(dr[4].ToString())).ToString() + " Units";

                                // Restore ตอนเปิดโปรแกรมใหม่
                                Cnt_OK[id] = int.Parse(dr[3].ToString());
                                Cnt_NG[id] = int.Parse(dr[4].ToString());

                                // Color
                                if (dr[5].ToString() == "True")
                                {
                                    OK.BackColor = Color.FromArgb(0, 192, 0);   // Green
                                    Status_OK[id] = true;
                                }
                                else
                                {
                                    OK.BackColor = Color.Gray;
                                    Status_OK[id] = false;
                                }

                                if (dr[6].ToString() == "True")
                                {
                                    NG.BackColor = Color.Red;
                                    Status_NG[id] = true;
                                }
                                else
                                {
                                    NG.BackColor = Color.Gray;
                                    Status_NG[id] = false;
                                }

                                // Reset time
                                Reset.Text = dr[7].ToString();

                                // Connected / Disconnected
                                //string Status1 = dr[8].ToString();
                                //Display_Connected(id, Status1);

                                // Run / Idle
                                string Status2 = dr[9].ToString();
                                Display_Run_Idle(id, Status2);
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public void Update_Line_MC(int id_, string line_, string mc_)
        {
            try
            {
                string query = "UPDATE tbl_maindisplay SET line=@line, mc=@mc WHERE id=@id";

                // create connection and command in "using" blocks
                using (SqlConnection conn = new SqlConnection(myConnection))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = id_;
                    cmd.Parameters.Add("@line", SqlDbType.VarChar, 50).Value = line_;
                    cmd.Parameters.Add("@mc", SqlDbType.VarChar, 50).Value = mc_;

                    // open connection, execute query, close connection
                    conn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                ErrorLogging(ex);
            }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public void Update_Reset(int id_, int ok_, int ng_, bool ok_color_, bool ng_color_, string reset_)
        {
            try
            {
                string query = "UPDATE tbl_maindisplay SET ok=@ok, ng=@ng, ok_color=@ok_color, ng_color=@ng_color, reset=@reset WHERE id=@id";

                // create connection and command in "using" blocks
                using (SqlConnection conn = new SqlConnection(myConnection))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = id_;
                    cmd.Parameters.Add("@ok", SqlDbType.Int).Value = ok_;
                    cmd.Parameters.Add("@ng", SqlDbType.Int).Value = ng_;
                    cmd.Parameters.Add("@ok_color", SqlDbType.Bit).Value = ok_color_;
                    cmd.Parameters.Add("@ng_color", SqlDbType.Bit).Value = ng_color_;
                    cmd.Parameters.Add("@reset", SqlDbType.DateTime).Value = reset_;

                    // open connection, execute query, close connection
                    conn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
                ErrorLogging(ex);
            }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------       
        public void Update_MC_Run_Idle(int id_, bool status)
        {
            try
            {
                string query = "UPDATE tbl_maindisplay SET run=@run WHERE id=@id";

                // create connection and command in "using" blocks
                using (SqlConnection conn = new SqlConnection(myConnection))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = id_;
                    cmd.Parameters.Add("@run", SqlDbType.Bit).Value = status;

                    // open connection, execute query, close connection
                    conn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    conn.Close();
                }                
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
                ErrorLogging(ex);
            }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void prevent_screensaver(bool sw)
        {
            if (sw)
            {
                SetThreadExecutionState(EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_CONTINUOUS);
            }
            else
            {
                SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
            }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        void Show_LineNum_MachineType()
        {
            label1.Text = Line1_ini;
            label3.Text = Line2_ini;
            label28.Text = Line3_ini;
            label43.Text = Line4_ini;
            label41.Text = Line5_ini;
            label32.Text = Line6_ini;

            label2.Text = Machine_Type1_ini;
            label5.Text = Machine_Type2_ini;

            label8.Text = Machine_Type1_ini;
            label6.Text = Machine_Type2_ini;

            label27.Text = Machine_Type1_ini;
            label7.Text = Machine_Type2_ini;

            label34.Text = Machine_Type1_ini;
            label33.Text = Machine_Type2_ini;

            label57.Text = Machine_Type1_ini;
            label55.Text = Machine_Type2_ini;

            label58.Text = Machine_Type1_ini;
            label56.Text = Machine_Type2_ini;

        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void Form1_Load(object sender, EventArgs e)
        {
            Directory.CreateDirectory("Log");

            read_ini_config();

            Show_LineNum_MachineType();

            myConnection = "user id=" + User_ini + ";" +
                         "password=" + Password_ini + ";server=" + ServerDB_ini + ";" +
                         "Trusted_Connection=false;" +
                         "database=" + Database_ini + "; " +
                         "connection timeout=" + Timeout_ini + "";

            prevent_screensaver(true);  // Disable Sleep Mode Windows10


            server = new SimpleTcpServer(Server_IP_Port_ini);
            server.Events.ClientConnected += Events_ClientConnected;
            server.Events.ClientDisconnected += Events_ClientDisconnected;
            server.Events.DataReceived += Events_DataReceived;



            for (int i = 1; i <= 12; i++)
            {
                string line_num = "";
                string mc_name = "";

                if (i == 1)
                {
                    line_num = Line1_ini;
                    mc_name = Machine_Type1_ini;
                }
                else if (i == 2)
                {
                    line_num = Line1_ini;
                    mc_name = Machine_Type2_ini;
                }
                else if (i == 3)
                {
                    line_num = Line2_ini;
                    mc_name = Machine_Type1_ini;
                }
                else if (i == 4)
                {
                    line_num = Line2_ini;
                    mc_name = Machine_Type2_ini;
                }
                else if (i == 5)
                {
                    line_num = Line3_ini;
                    mc_name = Machine_Type1_ini;
                }
                else if (i == 6)
                {
                    line_num = Line3_ini;
                    mc_name = Machine_Type2_ini;
                }
                else if (i == 7)
                {
                    line_num = Line4_ini;
                    mc_name = Machine_Type1_ini;
                }
                else if (i == 8)
                {
                    line_num = Line4_ini;
                    mc_name = Machine_Type2_ini;
                }
                else if (i == 9)
                {
                    line_num = Line5_ini;
                    mc_name = Machine_Type1_ini;
                }
                else if (i == 10)
                {
                    line_num = Line5_ini;
                    mc_name = Machine_Type2_ini;
                }
                else if (i == 11)
                {
                    line_num = Line6_ini;
                    mc_name = Machine_Type1_ini;
                }
                else if (i == 12)
                {
                    line_num = Line6_ini;
                    mc_name = Machine_Type2_ini;
                }

                InsertFirst(i);

                Update_Line_MC(i, line_num, mc_name);
            }

            Update_UI();

            if (Connect_MC_ini == "1")   // 1 สำหรับใช้ในโรงงาน ติดต่อเครื่องจักร , 0 สำหรับแสดงผลเฉยๆ
            {
                server.Start();
            }
            else
            {
                //MessageBox.Show("User Mode"); // ไม่ติดต่อเครื่องจักร ไว้ดูอย่างเดียว กับ Export รายงาน
            }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        void Confirm_Recieved(string Client_IP_Port, string Dat)
        {
            if(server.IsListening)
            {
                server.Send(Client_IP_Port, Dat);
                Debug.WriteLine("### ID: " + Dat + "    IP: " + Client_IP_Port + "    Confirm Received!!!  ---> OK");
            }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public string  ConvertToDateTimeFormat(List<string> data)
        {
            string DateTime = "";
            try
            {
                // DateTime
                string YYMMDD = data[1].Substring(3, 6);
                string Year = YYMMDD.Substring(0, 2);
                string Month = YYMMDD.Substring(2, 2);
                string Day = YYMMDD.Substring(4, 2);
                string Date = "20" + Year + "-" + Month + "-" + Day;


                string HHMMSS = data[2];
                string Hour = HHMMSS.Substring(0, 2);
                string Minute = HHMMSS.Substring(2, 2);
                string Sec = HHMMSS.Substring(4, 2);
                string Time = Hour + ":" + Minute + ":" + Sec;
                DateTime = Date + " " + Time;
            }
            catch(Exception ex)
            {

            }
            return DateTime;
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        void InsertDataToDatabase(string id, string ip, string ip_port, string machineName, string lineNum, List<string> data)            
        {
            try
            {
                string dtFormat = ConvertToDateTimeFormat(data);

                int cnt = Is_Inserted(dtFormat, machineName, lineNum);
                if (cnt > 0)
                {
                    // data already in DB 
                    Confirm_Recieved(ip_port, id);
                    Debug.WriteLine("### ID: " + id + "    IP: " + ip + "    Duplicate Cnt = " + cnt.ToString() + "    ---> Failed");
                }
                else
                {
                    // Insert Database
                    bool finish = Insert_Database(id, machineName, lineNum, dtFormat, data);
                    if (finish)
                    {
                        Confirm_Recieved(ip_port, id);
                        Debug.WriteLine("### ID: " + id + "    IP: " + ip + "    Inserted             ---> OK");

                    }
                    else
                    {
                        // Client Send again
                    }
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        void saveIpPort(string ip, string ipPort)
        {
            switch (ip)
            {
                case "192.168.0.1":
                    ip_port[1] = ipPort;
                    break;
                case "192.168.0.2":
                    ip_port[2] = ipPort;
                    break;
                case "192.168.0.3":
                    ip_port[3] = ipPort;
                    break;
                case "192.168.0.4":
                    ip_port[4] = ipPort;
                    break;
                case "192.168.0.5":
                    ip_port[5] = ipPort;
                    break;
                case "192.168.0.6":
                    ip_port[6] = ipPort;
                    break;
                case "192.168.0.7":
                    ip_port[7] = ipPort;
                    break;
                case "192.168.0.8":
                    ip_port[8] = ipPort;
                    break;
                case "192.168.0.9":
                    ip_port[9] = ipPort;
                    break;
                case "192.168.0.10":
                    ip_port[10] = ipPort;
                    break;
                case "192.168.0.11":
                    ip_port[11] = ipPort;
                    break;
                case "192.168.0.12":
                    ip_port[12] = ipPort;
                    break;
                default:
                    break;
            }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void Events_DataReceived(object sender, SuperSimpleTcp.DataReceivedEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                try
                {
                    string[] ip = e.IpPort.Split(':');

                    saveIpPort(ip[0], e.IpPort);

                    string inputData = Encoding.UTF8.GetString(e.Data.Array);                    
                    //textBox1.Text += $"{e.IpPort}: {dat}{Environment.NewLine}";
                    Debug.WriteLine("----------------------------------------------------------------------------------------------------------------------------");

                    string[] strDat = inputData.Split(new[] { "START," }, StringSplitOptions.None);
                    strDat = strDat.Where(x => !string.IsNullOrEmpty(x)).ToArray(); // Remove array if emply

                    foreach (string str in strDat)
                    {

                        if (str.IndexOf("\u0004") > -1) // <CR><EOT>  \r\u0004q
                        {
                            string dat = str.TrimEnd('\0'); // remove \0\0\0\0\0\0\0\0\0 in data

                            List<String> listStr = new List<String>();

                            listStr = dat.Split(',').ToList();

                            int len = listStr.Count();
                            if (len >= 31)
                            {
                                string id = listStr[0];
                                int idInt = int.Parse(id);
                                string ipaddress = ip_port[idInt].ToString();
                                Debug.Write("### ID: " + id + "    " + "IP: " + ipaddress + "    " + dat);

                                string Header = "";

                                List<char> characters = listStr[1].ToCharArray().ToList(); // แก้บัค หากมีขยะเข้ามาที HRD --> "8,�HRD240511,123203,16,1476,0.219,0.500,0.700,242.15,1596,0.350,2,0,0,0,0,0,0,0,0.500,0.400,0.650,0.750,,4,2,118,2.3,1,0,<CR><EOT>"
                                int cnt_char = characters.Count();
                                if(cnt_char > 9) // Error
                                {
                                   characters.RemoveAt(0);
                                    listStr[1] = new string(characters.ToArray());
                                    Header = listStr[1].Substring(0, 3);
                                }
                                else
                                {
                                    Header = listStr[1].Substring(0, 3);
                                }

                                

                                if (Header == "HRD")
                                {
                                    string line_num = "";
                                    string mc_name = "";

                                    switch (id)
                                    {
                                        case "1":
                                            line_num = Line1_ini;
                                            mc_name = Machine_Type1_ini;
                                            break;
                                        case "2":
                                            line_num = Line1_ini;
                                            mc_name = Machine_Type2_ini;
                                            break;
                                        case "3":
                                            line_num = Line2_ini;
                                            mc_name = Machine_Type1_ini;
                                            break;
                                        case "4":
                                            line_num = Line2_ini;
                                            mc_name = Machine_Type2_ini;
                                            break;
                                        case "5":
                                            line_num = Line3_ini;
                                            mc_name = Machine_Type1_ini;
                                            break;
                                        case "6":
                                            line_num = Line3_ini;
                                            mc_name = Machine_Type2_ini;
                                            break;
                                        case "7":
                                            line_num = Line4_ini;
                                            mc_name = Machine_Type1_ini;
                                            break;
                                        case "8":
                                            line_num = Line4_ini;
                                            mc_name = Machine_Type2_ini;
                                            break;
                                        case "9":
                                            line_num = Line5_ini;
                                            mc_name = Machine_Type1_ini;
                                            break;
                                        case "10":
                                            line_num = Line5_ini;
                                            mc_name = Machine_Type2_ini;
                                            break;
                                        case "11":
                                            line_num = Line6_ini;
                                            mc_name = Machine_Type1_ini;
                                            break;
                                        case "12":
                                            line_num = Line6_ini;
                                            mc_name = Machine_Type2_ini;
                                            break;
                                        default:
                                            break;
                                    }


                                    f_running[idInt] = true;
                                    timeout_running[idInt] = 0;

                                    Update_MC_Run_Idle(idInt, true);
                                    InsertDataToDatabase(id, ipaddress, ip_port[idInt], mc_name, line_num, listStr);

                                }
                                else
                                {
                                    if (cntWrongFormmat++ >= 3)
                                    {
                                        cntWrongFormmat = 0;
                                        WrongDataLogging(id, ipaddress, dat);
                                        Confirm_Recieved(ip_port[idInt], ipaddress);
                                        Debug.WriteLine("ID: " + id + "    " + "IP: " + ipaddress + "    " + "Wrong Format HRD!!!");
                                    }
                                }
                            }
                            else
                            {
                                // 1 packet = 31 arrays
                                // if <31 --> Don't care
                            }                            
                        }
                        else
                        {
                            return;
                        }
                    }
                }
                catch(Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            });
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void Events_ClientDisconnected(object sender, ConnectionEventArgs e)
        {
            // Disconnected
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void Events_ClientConnected(object sender, ConnectionEventArgs e)
        {
            // Connected
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public void Update_UI()
        {
            Read_MainDisplay(1, label75, label9, label10, label11);
            Read_MainDisplay(2, label77, label12, label17, label22);

            Read_MainDisplay(3, label78, label13, label18, label23);
            Read_MainDisplay(4, label80, label14, label19, label24);

            Read_MainDisplay(5, label82, label15, label20, label25);
            Read_MainDisplay(6, label84, label16, label21, label26);

            Read_MainDisplay(7, label86, label35, label37, label38);
            Read_MainDisplay(8, label88, label36, label39, label40);

            Read_MainDisplay(9, label90, label42, label44, label45);
            Read_MainDisplay(10, label92, label46, label47, label48);

            Read_MainDisplay(11, label94, label49, label52, label54);
            Read_MainDisplay(12, label96, label50, label51, label53);
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        
        public void timer1_Tick(object sender, EventArgs e)
        {
            Update_UI(); // Connected , Offline , Run , Idle

            //textBox1.AppendText("Timer\r\n");

            Check_Running(1, label110);
            Check_Running(2, label111); 
            Check_Running(3, label112); 
            Check_Running(4, label113); 
            Check_Running(5, label114); 
            Check_Running(6, label115); 
            Check_Running(7, label116); 
            Check_Running(8, label117);
            Check_Running(9, label118); 
            Check_Running(10, label119); 
            Check_Running(11, label120); 
            Check_Running(12, label121);




        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        void Check_Running(int id, Label label)
        {
            int sec = int.Parse(Timeout_Idle_ini);  // 30sec

            if (f_running[id])
            {
                if (++timeout_running[id] >= sec) // 10sec
                { 
                    label.Text = "";
                    f_running[id] = false;
                    Update_MC_Run_Idle(id, false);
                }
                else
                {
                    int cnt_down = sec - timeout_running[id];
                    label.Text = cnt_down.ToString() + " sec";
                }
            }                      
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        void Reset(int id, Label label_ok, Label label_ng)
        {
            DialogResult res = MessageBox.Show("Do you want to Reset?", "Confirm", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);

            if (res.ToString() == "OK")
            {
                ResetLogging(id, Cnt_OK[id].ToString(), Cnt_NG[id].ToString());

                string dt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                label_ok.BackColor = Color.Gray;
                label_ng.BackColor = Color.Gray;

                Update_Reset(id, 0, 0, false, false, dt);

                Update_UI();


            }
            else
            {

            }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void pictureBox5_Click(object sender, EventArgs e)
        {
            Reset(1, label10, label11);
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Reset(2, label17, label22);
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Reset(3, label18, label23);
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void pictureBox3_Click(object sender, EventArgs e)
        {
            Reset(4, label19, label24);
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void pictureBox4_Click(object sender, EventArgs e)
        {
            Reset(5, label20, label25);
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void pictureBox6_Click(object sender, EventArgs e)
        {
            Reset(6, label21, label26);
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void pictureBox7_Click(object sender, EventArgs e)
        {
            Reset(7, label37, label38);
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void pictureBox8_Click(object sender, EventArgs e)
        {
            Reset(8, label39, label40);
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void pictureBox9_Click(object sender, EventArgs e)
        {
            Reset(9, label44, label45);
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void pictureBox10_Click(object sender, EventArgs e)
        {
            Reset(10, label47, label48);
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void pictureBox11_Click(object sender, EventArgs e)
        {
            Reset(11, label52, label54);
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void pictureBox12_Click(object sender, EventArgs e)
        {
            Reset(12, label51, label53);
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult res = MessageBox.Show("Do you want to Exit?", "Confirm", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);

            if (res.ToString() == "OK")
            {
                try
                {
                    if (Connect_MC_ini == "1")
                    {
                        for (int i = 1; i <= 12; i++)
                        {
                            Update_MC_Run_Idle(i, false);
                        }
                    }

                    prevent_screensaver(false); // Enable Sleep Mode Windows10

                    //Environment.Exit(0);
                    Environment.Exit(1);
                }
                catch (Exception ex)
                {
                    ErrorLogging(ex);
                }
            }
            else
            {
                e.Cancel = true;
            }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        private void button3_Click(object sender, EventArgs e)
        {
            string filename;
            string path = Directory.GetCurrentDirectory();

            var loadDialog = new OpenFileDialog { Filter = "Text File|*.txt", InitialDirectory = path + "\\Log\\" };
            if (loadDialog.ShowDialog() == DialogResult.OK)
            {
                filename = loadDialog.FileName;
                Process.Start("notepad.exe", filename);
            }
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void button5_Click(object sender, EventArgs e)
        {
            Form2 frm = new Form2();
            frm.Show();
        }
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------                     
    }
}
