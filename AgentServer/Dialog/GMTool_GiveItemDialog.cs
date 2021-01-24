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

namespace AgentServer.Dialog
{
    public partial class GMTool_GiveItemDialog : Form
    {
        public GMTool_GiveItemDialog()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox1.Text) || string.IsNullOrEmpty(textBox2.Text) || string.IsNullOrEmpty(textBox3.Text))
            {
                MessageBox.Show("不能為空", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            using (var con = new MySqlConnection(Conf.Connstr))
            {
                con.Open();
                using (var cmd = new MySqlCommand(string.Empty, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "usp_giveItemDescByNickname";
                    cmd.Parameters.Add("itemdesc", MySqlDbType.Int32).Value = textBox2.Text;
                    cmd.Parameters.Add("nickname", MySqlDbType.VarString).Value = textBox1.Text;
                    cmd.Parameters.Add("pGiveCount", MySqlDbType.Int32).Value = Convert.ToInt32(textBox3.Text);
                    using (MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        reader.Read();
                        if (Convert.ToInt32(reader["nRet"]) == 0)
                            MessageBox.Show("派發成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        else
                            MessageBox.Show("派發失敗", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }
    }
}
