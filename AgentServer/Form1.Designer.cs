namespace AgentServer
{
    partial class Form1
    {
        /// <summary>
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置受控資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改
        /// 這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.btnStopServer = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.chkServerReady = new System.Windows.Forms.CheckBox();
            this.txtNotice = new System.Windows.Forms.TextBox();
            this.btnNotice = new System.Windows.Forms.Button();
            this.btnOpenHash = new System.Windows.Forms.Button();
            this.btnOpenDir = new System.Windows.Forms.Button();
            this.btnShowUserNum = new System.Windows.Forms.Button();
            this.btnReloadtblServerSettingInfo = new System.Windows.Forms.Button();
            this.btnCapsuleMachineManager = new System.Windows.Forms.Button();
            this.btnReloadMap = new System.Windows.Forms.Button();
            this.btnGMTool = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.btnTeleportLeftBottomCorner = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // richTextBox1
            // 
            this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBox1.Location = new System.Drawing.Point(10, 39);
            this.richTextBox1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(712, 288);
            this.richTextBox1.TabIndex = 0;
            this.richTextBox1.Text = "";
            this.richTextBox1.TextChanged += new System.EventHandler(this.richTextBox1_TextChanged);
            // 
            // btnStopServer
            // 
            this.btnStopServer.Location = new System.Drawing.Point(625, 375);
            this.btnStopServer.Name = "btnStopServer";
            this.btnStopServer.Size = new System.Drawing.Size(80, 31);
            this.btnStopServer.TabIndex = 1;
            this.btnStopServer.Text = "Shut Down";
            this.btnStopServer.UseVisualStyleBackColor = true;
            this.btnStopServer.Click += new System.EventHandler(this.btnStopServer_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 16);
            this.label1.TabIndex = 2;
            this.label1.Text = "Server IP:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(78, 13);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 16);
            this.label2.TabIndex = 3;
            this.label2.Text = "127.0.0.1";
            // 
            // chkServerReady
            // 
            this.chkServerReady.AutoSize = true;
            this.chkServerReady.Location = new System.Drawing.Point(586, 350);
            this.chkServerReady.Name = "chkServerReady";
            this.chkServerReady.Size = new System.Drawing.Size(119, 20);
            this.chkServerReady.TabIndex = 4;
            this.chkServerReady.Text = "SetServerREADY";
            this.chkServerReady.UseVisualStyleBackColor = true;
            this.chkServerReady.CheckedChanged += new System.EventHandler(this.chkServerReady_CheckedChanged);
            // 
            // txtNotice
            // 
            this.txtNotice.Location = new System.Drawing.Point(10, 449);
            this.txtNotice.Name = "txtNotice";
            this.txtNotice.Size = new System.Drawing.Size(598, 23);
            this.txtNotice.TabIndex = 5;
            // 
            // btnNotice
            // 
            this.btnNotice.Location = new System.Drawing.Point(616, 447);
            this.btnNotice.Name = "btnNotice";
            this.btnNotice.Size = new System.Drawing.Size(89, 27);
            this.btnNotice.TabIndex = 6;
            this.btnNotice.Text = "Send Notice";
            this.btnNotice.UseVisualStyleBackColor = true;
            this.btnNotice.Click += new System.EventHandler(this.btnNotice_Click);
            // 
            // btnOpenHash
            // 
            this.btnOpenHash.Location = new System.Drawing.Point(221, 375);
            this.btnOpenHash.Name = "btnOpenHash";
            this.btnOpenHash.Size = new System.Drawing.Size(90, 31);
            this.btnOpenHash.TabIndex = 7;
            this.btnOpenHash.Text = "Open Hash";
            this.btnOpenHash.UseVisualStyleBackColor = true;
            this.btnOpenHash.Click += new System.EventHandler(this.btnOpenHash_Click);
            // 
            // btnOpenDir
            // 
            this.btnOpenDir.Location = new System.Drawing.Point(127, 375);
            this.btnOpenDir.Name = "btnOpenDir";
            this.btnOpenDir.Size = new System.Drawing.Size(88, 31);
            this.btnOpenDir.TabIndex = 8;
            this.btnOpenDir.Text = "Open Dir";
            this.btnOpenDir.UseVisualStyleBackColor = true;
            this.btnOpenDir.Click += new System.EventHandler(this.btnOpenDir_Click);
            // 
            // btnShowUserNum
            // 
            this.btnShowUserNum.Location = new System.Drawing.Point(10, 375);
            this.btnShowUserNum.Name = "btnShowUserNum";
            this.btnShowUserNum.Size = new System.Drawing.Size(111, 31);
            this.btnShowUserNum.TabIndex = 9;
            this.btnShowUserNum.Text = "Show User Num";
            this.btnShowUserNum.UseVisualStyleBackColor = true;
            this.btnShowUserNum.Click += new System.EventHandler(this.btnShowUserNum_Click);
            // 
            // btnReloadtblServerSettingInfo
            // 
            this.btnReloadtblServerSettingInfo.Location = new System.Drawing.Point(10, 340);
            this.btnReloadtblServerSettingInfo.Name = "btnReloadtblServerSettingInfo";
            this.btnReloadtblServerSettingInfo.Size = new System.Drawing.Size(186, 30);
            this.btnReloadtblServerSettingInfo.TabIndex = 11;
            this.btnReloadtblServerSettingInfo.Text = "Reload tblServerSettingInfo";
            this.btnReloadtblServerSettingInfo.UseVisualStyleBackColor = true;
            this.btnReloadtblServerSettingInfo.Click += new System.EventHandler(this.btnReloadtblServerSettingInfo_Click);
            // 
            // btnCapsuleMachineManager
            // 
            this.btnCapsuleMachineManager.Location = new System.Drawing.Point(202, 340);
            this.btnCapsuleMachineManager.Name = "btnCapsuleMachineManager";
            this.btnCapsuleMachineManager.Size = new System.Drawing.Size(180, 30);
            this.btnCapsuleMachineManager.TabIndex = 12;
            this.btnCapsuleMachineManager.Text = "CapsuleMachine Manager";
            this.btnCapsuleMachineManager.UseVisualStyleBackColor = true;
            this.btnCapsuleMachineManager.Click += new System.EventHandler(this.btnCapsuleMachineManager_Click);
            // 
            // btnReloadMap
            // 
            this.btnReloadMap.Location = new System.Drawing.Point(318, 375);
            this.btnReloadMap.Name = "btnReloadMap";
            this.btnReloadMap.Size = new System.Drawing.Size(64, 31);
            this.btnReloadMap.TabIndex = 14;
            this.btnReloadMap.Text = "Reload MapInfo";
            this.btnReloadMap.UseVisualStyleBackColor = true;
            this.btnReloadMap.Click += new System.EventHandler(this.btnReloadMap_Click);
            // 
            // btnGMTool
            // 
            this.btnGMTool.Location = new System.Drawing.Point(388, 340);
            this.btnGMTool.Name = "btnGMTool";
            this.btnGMTool.Size = new System.Drawing.Size(122, 30);
            this.btnGMTool.TabIndex = 15;
            this.btnGMTool.Text = "GM Tool";
            this.btnGMTool.UseVisualStyleBackColor = true;
            this.btnGMTool.Click += new System.EventHandler(this.btnGMTool_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(388, 375);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(64, 31);
            this.button1.TabIndex = 16;
            this.button1.Text = "Reload GameRewardGroupInfo";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnTeleportLeftBottomCorner
            // 
            this.btnTeleportLeftBottomCorner.Location = new System.Drawing.Point(10, 412);
            this.btnTeleportLeftBottomCorner.Name = "btnTeleportLeftBottomCorner";
            this.btnTeleportLeftBottomCorner.Size = new System.Drawing.Size(111, 31);
            this.btnTeleportLeftBottomCorner.TabIndex = 17;
            this.btnTeleportLeftBottomCorner.Text = "Teleport";
            this.btnTeleportLeftBottomCorner.UseVisualStyleBackColor = true;
            this.btnTeleportLeftBottomCorner.Click += new System.EventHandler(this.btnTeleportLeftBottomCorner_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(734, 483);
            this.Controls.Add(this.btnTeleportLeftBottomCorner);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnGMTool);
            this.Controls.Add(this.btnReloadMap);
            this.Controls.Add(this.btnCapsuleMachineManager);
            this.Controls.Add(this.btnReloadtblServerSettingInfo);
            this.Controls.Add(this.btnShowUserNum);
            this.Controls.Add(this.btnOpenDir);
            this.Controls.Add(this.btnOpenHash);
            this.Controls.Add(this.btnNotice);
            this.Controls.Add(this.txtNotice);
            this.Controls.Add(this.chkServerReady);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnStopServer);
            this.Controls.Add(this.richTextBox1);
            this.Font = new System.Drawing.Font("Microsoft JhengHei", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "Strugarden Server by Alanlei";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Button btnStopServer;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox chkServerReady;
        private System.Windows.Forms.TextBox txtNotice;
        private System.Windows.Forms.Button btnNotice;
        private System.Windows.Forms.Button btnOpenHash;
        private System.Windows.Forms.Button btnOpenDir;
        private System.Windows.Forms.Button btnShowUserNum;
        private System.Windows.Forms.Button btnReloadtblServerSettingInfo;
        private System.Windows.Forms.Button btnCapsuleMachineManager;
        private System.Windows.Forms.Button btnReloadMap;
        private System.Windows.Forms.Button btnGMTool;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btnTeleportLeftBottomCorner;
    }
}

