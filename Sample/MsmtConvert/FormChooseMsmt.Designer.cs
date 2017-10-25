namespace Plugin.Demo.Sample.MsmtConvert
{
    partial class FormChooseMsmt
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
            this.chkListMsmt = new System.Windows.Forms.CheckedListBox();
            this.cmbPrediction = new System.Windows.Forms.ComboBox();
            this.cmbRef = new System.Windows.Forms.ComboBox();
            this.lbxResult = new System.Windows.Forms.ListBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.chkMetreXY = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // chkListMsmt
            // 
            this.chkListMsmt.FormattingEnabled = true;
            this.chkListMsmt.Location = new System.Drawing.Point(12, 49);
            this.chkListMsmt.Name = "chkListMsmt";
            this.chkListMsmt.Size = new System.Drawing.Size(258, 276);
            this.chkListMsmt.TabIndex = 0;
            // 
            // cmbPrediction
            // 
            this.cmbPrediction.FormattingEnabled = true;
            this.cmbPrediction.Location = new System.Drawing.Point(12, 12);
            this.cmbPrediction.Name = "cmbPrediction";
            this.cmbPrediction.Size = new System.Drawing.Size(258, 20);
            this.cmbPrediction.TabIndex = 1;
            // 
            // cmbRef
            // 
            this.cmbRef.FormattingEnabled = true;
            this.cmbRef.Location = new System.Drawing.Point(290, 12);
            this.cmbRef.Name = "cmbRef";
            this.cmbRef.Size = new System.Drawing.Size(285, 20);
            this.cmbRef.TabIndex = 2;
            // 
            // lbxResult
            // 
            this.lbxResult.FormattingEnabled = true;
            this.lbxResult.ItemHeight = 12;
            this.lbxResult.Location = new System.Drawing.Point(290, 73);
            this.lbxResult.Name = "lbxResult";
            this.lbxResult.Size = new System.Drawing.Size(285, 208);
            this.lbxResult.TabIndex = 3;
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(419, 302);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 4;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(500, 302);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 5;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // chkMetreXY
            // 
            this.chkMetreXY.AutoSize = true;
            this.chkMetreXY.Location = new System.Drawing.Point(290, 49);
            this.chkMetreXY.Name = "chkMetreXY";
            this.chkMetreXY.Size = new System.Drawing.Size(198, 16);
            this.chkMetreXY.TabIndex = 6;
            this.chkMetreXY.Text = "Check and append x/y by metre";
            this.chkMetreXY.UseVisualStyleBackColor = true;
            // 
            // FormChooseMsmt
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(585, 334);
            this.Controls.Add(this.chkMetreXY);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.lbxResult);
            this.Controls.Add(this.cmbRef);
            this.Controls.Add(this.cmbPrediction);
            this.Controls.Add(this.chkListMsmt);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormChooseMsmt";
            this.Text = "FormChooseMsmt";
            this.Load += new System.EventHandler(this.FormChooseMsmt_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckedListBox chkListMsmt;
        private System.Windows.Forms.ComboBox cmbPrediction;
        private System.Windows.Forms.ComboBox cmbRef;
        private System.Windows.Forms.ListBox lbxResult;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.CheckBox chkMetreXY;
    }
}