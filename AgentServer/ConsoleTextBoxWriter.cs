using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AgentServer
{
    public class ConsoleTextBoxWriter : TextWriter
    {
        private TextBox textBox;
        private RichTextBox rtextBox;

        public ConsoleTextBoxWriter(RichTextBox rtextBox)
        {
            Console.SetOut(this);
            //this.textBox = textBox;
            this.rtextBox = rtextBox;
        }
        public override Encoding Encoding { get { return Encoding.UTF8; } }

        public override void Write(string value)
        {
            WriteImp(value);
        }

        public override void WriteLine(string value)
        {
            WriteImp(value + Environment.NewLine);
        }

        private void WriteImp(string value)
        {
            if (this.rtextBox.InvokeRequired)
                this.rtextBox.Invoke(new MethodInvoker(delegate ()
                {
                    rtextBox.AppendText(value);
                }));
            else
                rtextBox.AppendText(value);
        }
    }
}
