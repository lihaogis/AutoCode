namespace AutoCode
{
    partial class frmMain
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.btnTestConnect = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtIp = new System.Windows.Forms.TextBox();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.txtBDName = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtUserName = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtUserPsw = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.btnBeginTask = new System.Windows.Forms.Button();
            this.txtOutPath = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.btnOpenPath = new System.Windows.Forms.Button();
            this.comDBType = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(32, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "数据库类型：";
            // 
            // btnTestConnect
            // 
            this.btnTestConnect.Location = new System.Drawing.Point(279, 193);
            this.btnTestConnect.Name = "btnTestConnect";
            this.btnTestConnect.Size = new System.Drawing.Size(75, 23);
            this.btnTestConnect.TabIndex = 1;
            this.btnTestConnect.Text = "测试连接";
            this.btnTestConnect.UseVisualStyleBackColor = true;
            this.btnTestConnect.Click += new System.EventHandler(this.btnTestConnect_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(50, 89);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "端口号：";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(50, 53);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 3;
            this.label3.Text = "主机名：";
            // 
            // txtIp
            // 
            this.txtIp.Location = new System.Drawing.Point(106, 49);
            this.txtIp.Name = "txtIp";
            this.txtIp.Size = new System.Drawing.Size(149, 21);
            this.txtIp.TabIndex = 4;
            // 
            // txtPort
            // 
            this.txtPort.Location = new System.Drawing.Point(106, 85);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(149, 21);
            this.txtPort.TabIndex = 5;
            // 
            // txtBDName
            // 
            this.txtBDName.Location = new System.Drawing.Point(106, 118);
            this.txtBDName.Name = "txtBDName";
            this.txtBDName.Size = new System.Drawing.Size(149, 21);
            this.txtBDName.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(28, 122);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(77, 12);
            this.label4.TabIndex = 6;
            this.label4.Text = "数据库名称：";
            // 
            // txtUserName
            // 
            this.txtUserName.Location = new System.Drawing.Point(106, 154);
            this.txtUserName.Name = "txtUserName";
            this.txtUserName.Size = new System.Drawing.Size(149, 21);
            this.txtUserName.TabIndex = 9;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(50, 158);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 12);
            this.label5.TabIndex = 8;
            this.label5.Text = "用户名：";
            // 
            // txtUserPsw
            // 
            this.txtUserPsw.Location = new System.Drawing.Point(106, 191);
            this.txtUserPsw.Name = "txtUserPsw";
            this.txtUserPsw.Size = new System.Drawing.Size(149, 21);
            this.txtUserPsw.TabIndex = 11;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(62, 195);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(41, 12);
            this.label6.TabIndex = 10;
            this.label6.Text = "密码：";
            // 
            // btnBeginTask
            // 
            this.btnBeginTask.Location = new System.Drawing.Point(122, 278);
            this.btnBeginTask.Name = "btnBeginTask";
            this.btnBeginTask.Size = new System.Drawing.Size(154, 23);
            this.btnBeginTask.TabIndex = 12;
            this.btnBeginTask.Text = "开始生成";
            this.btnBeginTask.UseVisualStyleBackColor = true;
            this.btnBeginTask.Click += new System.EventHandler(this.btnBeginTask_Click);
            // 
            // txtOutPath
            // 
            this.txtOutPath.Location = new System.Drawing.Point(106, 234);
            this.txtOutPath.Name = "txtOutPath";
            this.txtOutPath.Size = new System.Drawing.Size(209, 21);
            this.txtOutPath.TabIndex = 14;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(14, 238);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(89, 12);
            this.label7.TabIndex = 13;
            this.label7.Text = "输出文件目录：";
            // 
            // btnOpenPath
            // 
            this.btnOpenPath.Location = new System.Drawing.Point(326, 234);
            this.btnOpenPath.Name = "btnOpenPath";
            this.btnOpenPath.Size = new System.Drawing.Size(26, 23);
            this.btnOpenPath.TabIndex = 15;
            this.btnOpenPath.Text = "...";
            this.btnOpenPath.UseVisualStyleBackColor = true;
            // 
            // comDBType
            // 
            this.comDBType.FormattingEnabled = true;
            this.comDBType.Items.AddRange(new object[] {
            "PostgrelSQL"});
            this.comDBType.Location = new System.Drawing.Point(107, 13);
            this.comDBType.Name = "comDBType";
            this.comDBType.Size = new System.Drawing.Size(149, 20);
            this.comDBType.TabIndex = 16;
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(373, 325);
            this.Controls.Add(this.comDBType);
            this.Controls.Add(this.btnOpenPath);
            this.Controls.Add(this.txtOutPath);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.btnBeginTask);
            this.Controls.Add(this.txtUserPsw);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtUserName);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtBDName);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtPort);
            this.Controls.Add(this.txtIp);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnTestConnect);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "PostgreSQL三层生成";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnTestConnect;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtIp;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.TextBox txtBDName;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtUserName;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtUserPsw;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnBeginTask;
        private System.Windows.Forms.TextBox txtOutPath;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button btnOpenPath;
        private System.Windows.Forms.ComboBox comDBType;
    }
}

