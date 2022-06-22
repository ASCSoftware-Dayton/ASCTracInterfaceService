
namespace ASCTracInterfaceSample
{
    partial class Form1
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
            this.tbContent = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.lblResultCode = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.lblInterfaceDB = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.btnGo = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.cbFunction = new System.Windows.Forms.ComboBox();
            this.edURL = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // tbContent
            // 
            this.tbContent.Location = new System.Drawing.Point(118, 230);
            this.tbContent.Multiline = true;
            this.tbContent.Name = "tbContent";
            this.tbContent.Size = new System.Drawing.Size(640, 184);
            this.tbContent.TabIndex = 21;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(43, 237);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(47, 13);
            this.label4.TabIndex = 20;
            this.label4.Text = "Content:";
            // 
            // lblResultCode
            // 
            this.lblResultCode.AutoSize = true;
            this.lblResultCode.Location = new System.Drawing.Point(115, 204);
            this.lblResultCode.Name = "lblResultCode";
            this.lblResultCode.Size = new System.Drawing.Size(27, 13);
            this.lblResultCode.TabIndex = 19;
            this.lblResultCode.Text = "N/A";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(43, 204);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(68, 13);
            this.label5.TabIndex = 18;
            this.label5.Text = "Result Code:";
            // 
            // lblInterfaceDB
            // 
            this.lblInterfaceDB.AutoSize = true;
            this.lblInterfaceDB.Location = new System.Drawing.Point(115, 126);
            this.lblInterfaceDB.Name = "lblInterfaceDB";
            this.lblInterfaceDB.Size = new System.Drawing.Size(27, 13);
            this.lblInterfaceDB.TabIndex = 17;
            this.lblInterfaceDB.Text = "N/A";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(43, 126);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(70, 13);
            this.label3.TabIndex = 16;
            this.label3.Text = "Interface DB:";
            // 
            // btnGo
            // 
            this.btnGo.Location = new System.Drawing.Point(118, 152);
            this.btnGo.Name = "btnGo";
            this.btnGo.Size = new System.Drawing.Size(75, 23);
            this.btnGo.TabIndex = 15;
            this.btnGo.Text = "Go";
            this.btnGo.UseVisualStyleBackColor = true;
            this.btnGo.Click += new System.EventHandler(this.btnGo_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(43, 77);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(51, 13);
            this.label2.TabIndex = 14;
            this.label2.Text = "Function:";
            // 
            // cbFunction
            // 
            this.cbFunction.FormattingEnabled = true;
            this.cbFunction.Items.AddRange(new object[] {
            "ASNImport",
            "CustOrderImport",
            "ItemImport",
            "POImport",
            "VendorImport",
            "",
            "CustOrderExport",
            "ParcelExport",
            "POExport - Lines",
            "POExport - Licenses",
            "TranfileExport",
            "",
            "WCS-GetPicks",
            "WCS-Pick",
            "WCS-Repick",
            "WCS-Unpick"});
            this.cbFunction.Location = new System.Drawing.Point(118, 74);
            this.cbFunction.Name = "cbFunction";
            this.cbFunction.Size = new System.Drawing.Size(182, 21);
            this.cbFunction.TabIndex = 13;
            // 
            // edURL
            // 
            this.edURL.Location = new System.Drawing.Point(118, 36);
            this.edURL.Name = "edURL";
            this.edURL.Size = new System.Drawing.Size(353, 20);
            this.edURL.TabIndex = 12;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(43, 39);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 13);
            this.label1.TabIndex = 11;
            this.label1.Text = "Web API Url:";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.tbContent);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.lblResultCode);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.lblInterfaceDB);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnGo);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cbFunction);
            this.Controls.Add(this.edURL);
            this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.Text = "ASCTrac Interface Sample";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbContent;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblResultCode;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lblInterfaceDB;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnGo;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cbFunction;
        private System.Windows.Forms.TextBox edURL;
        private System.Windows.Forms.Label label1;
    }
}

