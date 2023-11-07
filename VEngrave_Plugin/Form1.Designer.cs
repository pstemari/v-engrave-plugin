namespace VEngraveForCamBam
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
            this.LogCheck = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // LogCheck
            // 
            this.LogCheck.AutoSize = true;
            this.LogCheck.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.LogCheck.Location = new System.Drawing.Point(12, 12);
            this.LogCheck.Name = "LogCheck";
            this.LogCheck.Size = new System.Drawing.Size(131, 17);
            this.LogCheck.TabIndex = 0;
            this.LogCheck.Text = "Enable Log Messages";
            this.LogCheck.UseVisualStyleBackColor = true;
            this.LogCheck.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(174, 46);
            this.Controls.Add(this.LogCheck);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.ShowIcon = false;
            this.Text = "VEngrave";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox LogCheck;
    }
}