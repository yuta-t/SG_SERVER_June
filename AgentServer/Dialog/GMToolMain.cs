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
    public partial class GMTool : Form
    {
        public GMTool()
        {
            InitializeComponent();
        }

        private void btn_GiveItemDialog_Click(object sender, EventArgs e)
        {
            var giveitemdialog = new GMTool_GiveItemDialog();
            giveitemdialog.Show();
        }
    }
}
