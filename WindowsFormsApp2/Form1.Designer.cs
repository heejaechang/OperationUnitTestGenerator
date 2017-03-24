namespace WindowsFormsApp2
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
            this.button1 = new System.Windows.Forms.Button();
            this.unittestOutput = new System.Windows.Forms.TextBox();
            this.language = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.button1.Location = new System.Drawing.Point(0, 478);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(719, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "generate unit test";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.OnUnitTestGenerated);
            // 
            // unittestOutput
            // 
            this.unittestOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.unittestOutput.Location = new System.Drawing.Point(0, 0);
            this.unittestOutput.Multiline = true;
            this.unittestOutput.Name = "unittestOutput";
            this.unittestOutput.ReadOnly = true;
            this.unittestOutput.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.unittestOutput.Size = new System.Drawing.Size(719, 457);
            this.unittestOutput.TabIndex = 2;
            this.unittestOutput.WordWrap = false;
            // 
            // language
            // 
            this.language.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.language.FormattingEnabled = true;
            this.language.Items.AddRange(new object[] {
            "C#",
            "VB"});
            this.language.Location = new System.Drawing.Point(0, 457);
            this.language.Name = "language";
            this.language.Size = new System.Drawing.Size(719, 21);
            this.language.TabIndex = 4;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(719, 501);
            this.Controls.Add(this.unittestOutput);
            this.Controls.Add(this.language);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox unittestOutput;
        private System.Windows.Forms.ComboBox language;
    }
}

