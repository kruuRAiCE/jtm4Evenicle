using System;
using System.Windows.Forms;
using System.Drawing;

namespace JoyToAny
{

    #region エラー用ダイアログ

    public class frmErrorForm : Form
    {
        private string ErrMessage;

        public frmErrorForm(string errMsg)
        {
            InitializeComponent();
            ErrMessage = errMsg;
        }

        private void ErrorForm_Load(object sender, EventArgs e)
        {
            textBox1.Text = ErrMessage;
        }

        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.textBox1 = new TextBox();
            this.SuspendLayout();

            // textBox1
            this.textBox1.Dock = DockStyle.Fill;
            this.textBox1.Location = new Point(0, 0);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ScrollBars = ScrollBars.Vertical;
            this.textBox1.Size = new Size(832, 356);
            this.textBox1.TabIndex = 0;

            // Form2
            this.AutoScaleDimensions = new SizeF(6F, 12F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(832, 356);
            this.Controls.Add(this.textBox1);
            this.Name = "Form2";
            this.Text = "エラーメッセージ";
            this.Load += new EventHandler(this.ErrorForm_Load);
            this.ResumeLayout(false);

            this.PerformLayout();
        }

        private TextBox textBox1;
    }

    #endregion
}
