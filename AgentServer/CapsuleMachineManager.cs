using AgentServer.Holders;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AgentServer
{
    public partial class CapsuleMachineManager : Form
    {
        public CapsuleMachineManager()
        {
            InitializeComponent();
        }

        private void CapsuleMachineManager_Load(object sender, EventArgs e)
        {
            LoadUsingMachineList();
        }

        private void LoadUsingMachineList()
        {
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand("SELECT * FROM essencapsulemachineusinglist", con);
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    CapsuleMachineListBox.Items.Add(reader["fdMachineNum"]);
                }
                cmd.Dispose();
                reader.Close();
                con.Close();
            }
        }
        private void LoadMachineData(int MachineItemNum)
        {
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_capsuleMachineGetMachineInfoForTool";
                cmd.Parameters.Add("machineNum", MySqlDbType.Int32).Value = MachineItemNum;
                MySqlDataAdapter MyDA = new MySqlDataAdapter(cmd);
                cmd.ExecuteNonQuery();
                DataTable dataTable = new DataTable();
                MyDA.Fill(dataTable);
                CapsuleMachineData.DataSource = dataTable;
                cmd.Dispose();
                //reader.Close();
                con.Close();
            }
            CapsuleMachineData.Columns[0].Visible = false;
        }
        private byte EditItem(int MachineNum, int ItemNum, int ItemAmount, int ItemMax)
        {
            byte ret = 0;
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_capsuleMachineEditItem";
                cmd.Parameters.Add("machineNum", MySqlDbType.Int32).Value = MachineNum;
                cmd.Parameters.Add("itemNum", MySqlDbType.Int32).Value = ItemNum;
                cmd.Parameters.Add("itemAmount", MySqlDbType.Int32).Value = ItemAmount;
                cmd.Parameters.Add("itemMax", MySqlDbType.Int32).Value = ItemMax;
                MySqlDataReader reader = cmd.ExecuteReader();
                reader.Read();
                ret = Convert.ToByte(reader["ret"]);       
                cmd.Dispose();
                reader.Close();
                con.Close();
            }
            return ret;
        }
        private byte DeleteItem(int MachineNum, int ItemNum)
        {
            byte ret = 0;
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_capsuleMachineDeleteItem";
                cmd.Parameters.Add("machineNum", MySqlDbType.Int32).Value = MachineNum;
                cmd.Parameters.Add("itemNum", MySqlDbType.Int32).Value = ItemNum;
                MySqlDataReader reader = cmd.ExecuteReader();
                reader.Read();
                ret = Convert.ToByte(reader["ret"]);
                cmd.Dispose();
                reader.Close();
                con.Close();
            }
            return ret;
        }
        private byte AddItem(int MachineNum, int Level, int ItemNum, int ItemMax)
        {
            byte ret = 0;
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_capsuleMachineInsertItem";
                cmd.Parameters.Add("machineNum", MySqlDbType.Int32).Value = MachineNum;
                cmd.Parameters.Add("level", MySqlDbType.Int32).Value = Level;
                cmd.Parameters.Add("itemNum", MySqlDbType.Int32).Value = ItemNum;
                cmd.Parameters.Add("itemMax", MySqlDbType.Int32).Value = ItemMax;
                MySqlDataReader reader = cmd.ExecuteReader();
                reader.Read();
                ret = Convert.ToByte(reader["ret"]);
                cmd.Dispose();
                reader.Close();
                con.Close();
            }
            return ret;
        }
        private byte ResetMachine(int MachineNum)
        {
            byte ret = 0;
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                var cmd = new MySqlCommand(string.Empty, con);
                cmd.Parameters.Clear();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "usp_capsuleMachineReset";
                cmd.Parameters.Add("machineNum", MySqlDbType.Int32).Value = MachineNum;
                cmd.Parameters.Add("returnRate", MySqlDbType.Int32).Value = 0;
                MySqlDataReader reader = cmd.ExecuteReader();
                reader.Read();
                ret = Convert.ToByte(reader["retval"]);
                cmd.Dispose();
                reader.Close();
                con.Close();
            }
            return ret;
        }


        private void CapsuleMachineListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cmb = sender as ComboBox;
            LoadMachineData(Convert.ToInt32(cmb.Text));
        }

        private void CapsuleMachineData_SelectionChanged(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in CapsuleMachineData.SelectedRows)
            {
                label4.Text = row.Cells[2].Value.ToString();
                textBox1.Text = row.Cells[3].Value.ToString();
                textBox2.Text = row.Cells[4].Value.ToString();
            }
        }

        private void btnEditApply_Click(object sender, EventArgs e)
        {
            if (CapsuleMachineListBox.Text == string.Empty || textBox1.Text == string.Empty || textBox2.Text == string.Empty)
            {
                MessageBox.Show("不能為空", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            byte ret = EditItem(Convert.ToInt32(CapsuleMachineListBox.Text), Convert.ToInt32(label4.Text), Convert.ToInt32(textBox1.Text), Convert.ToInt32(textBox2.Text));
            if(ret == 0)
            {
                LoadMachineData(Convert.ToInt32(CapsuleMachineListBox.Text));
                MessageBox.Show("修改成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (CapsuleMachineListBox.Text == string.Empty || textBox1.Text == string.Empty || textBox2.Text == string.Empty)
            {
                MessageBox.Show("不能為空", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            byte ret = DeleteItem(Convert.ToInt32(CapsuleMachineListBox.Text), Convert.ToInt32(label4.Text));
            if (ret == 0)
            {
                LoadMachineData(Convert.ToInt32(CapsuleMachineListBox.Text));
                MessageBox.Show("刪除物品成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
        }

        private void btnAddItem_Click(object sender, EventArgs e)
        {
            if (CapsuleMachineData.Rows.Count < 10)
            {
                if(CapsuleMachineListBox.Text == string.Empty || textBox3.Text == string.Empty || textBox4.Text == string.Empty || textBox5.Text == string.Empty)
                {
                    MessageBox.Show("不能為空", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                byte ret = AddItem(Convert.ToInt32(CapsuleMachineListBox.Text), Convert.ToInt32(textBox4.Text), Convert.ToInt32(textBox3.Text), Convert.ToInt32(textBox5.Text));
                if (ret == 0)
                {
                    LoadMachineData(Convert.ToInt32(CapsuleMachineListBox.Text));
                    MessageBox.Show("物品新增成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (ret == 1)
                {
                    MessageBox.Show("扭蛋機已有此物品", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else if (ret == 2)
                {
                    MessageBox.Show("等級只可以1-5", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else if (ret == 3)
                {
                    MessageBox.Show("沒有此扭蛋機或物品", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                MessageBox.Show("此扭蛋機已經有10個物品", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }


        private void btnReload_Click(object sender, EventArgs e)
        {
            if (CapsuleMachineListBox.Text == string.Empty)
            {
                MessageBox.Show("請先選擇扭蛋機", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            CapsuleMachineHolder.LoadCapsuleMachineInfo();
            LoadMachineData(Convert.ToInt32(CapsuleMachineListBox.Text));
            MessageBox.Show("Reload OK", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
