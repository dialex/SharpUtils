namespace SoftLife.CSharp
{
    partial class SqlOutputConsole
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
            this.txtOutputConsole = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // txtOutputConsole
            // 
            this.txtOutputConsole.BackColor = System.Drawing.Color.White;
            this.txtOutputConsole.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtOutputConsole.Font = new System.Drawing.Font("Consolas", 10F);
            this.txtOutputConsole.Location = new System.Drawing.Point(0, 0);
            this.txtOutputConsole.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.txtOutputConsole.Multiline = true;
            this.txtOutputConsole.Name = "txtOutputConsole";
            this.txtOutputConsole.ReadOnly = true;
            this.txtOutputConsole.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtOutputConsole.Size = new System.Drawing.Size(784, 361);
            this.txtOutputConsole.TabIndex = 0;
            this.txtOutputConsole.WordWrap = false;
            this.txtOutputConsole.TextChanged += new System.EventHandler(this.txtOutputConsole_TextChanged);
            // 
            // SqlOutputConsole
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(784, 361);
            this.Controls.Add(this.txtOutputConsole);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.Name = "SqlOutputConsole";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SQL Output Console";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SqlOutputConsole_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtOutputConsole;
    }
}