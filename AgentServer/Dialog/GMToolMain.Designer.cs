namespace AgentServer.Dialog
{
    partial class GMTool
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btn_GiveItemDialog = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btn_GiveItemDialog
            // 
            this.btn_GiveItemDialog.Font = new System.Drawing.Font("微軟正黑體", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btn_GiveItemDialog.Location = new System.Drawing.Point(34, 22);
            this.btn_GiveItemDialog.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btn_GiveItemDialog.Name = "btn_GiveItemDialog";
            this.btn_GiveItemDialog.Size = new System.Drawing.Size(134, 48);
            this.btn_GiveItemDialog.TabIndex = 0;
            this.btn_GiveItemDialog.Text = "派發物品";
            this.btn_GiveItemDialog.UseVisualStyleBackColor = true;
            this.btn_GiveItemDialog.Click += new System.EventHandler(this.btn_GiveItemDialog_Click);
            // 
            // GMTool
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(382, 209);
            this.Controls.Add(this.btn_GiveItemDialog);
            this.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.Name = "GMTool";
            this.Text = "GMTool";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btn_GiveItemDialog;
    }
}