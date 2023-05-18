namespace SPFverter
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            button1 = new Button();
            button2 = new Button();
            groupBox1 = new GroupBox();
            groupBox2 = new GroupBox();
            button4 = new Button();
            button3 = new Button();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(26, 43);
            button1.Margin = new Padding(2);
            button1.Name = "button1";
            button1.Size = new Size(159, 33);
            button1.TabIndex = 0;
            button1.Text = "SPF -> Modern";
            button1.UseVisualStyleBackColor = true;
            button1.Click += BtnSpfToModern_Click;
            // 
            // button2
            // 
            button2.Location = new Point(26, 82);
            button2.Margin = new Padding(2);
            button2.Name = "button2";
            button2.Size = new Size(159, 33);
            button2.TabIndex = 1;
            button2.Text = "Modern -> SPF";
            button2.UseVisualStyleBackColor = true;
            button2.Click += BtnModernToSpf_Click;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(button1);
            groupBox1.Controls.Add(button2);
            groupBox1.Location = new Point(10, 10);
            groupBox1.Margin = new Padding(2);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(2);
            groupBox1.Size = new Size(207, 141);
            groupBox1.TabIndex = 10;
            groupBox1.TabStop = false;
            groupBox1.Text = "Single File Process";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(button4);
            groupBox2.Controls.Add(button3);
            groupBox2.Location = new Point(221, 11);
            groupBox2.Margin = new Padding(2);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new Padding(2);
            groupBox2.Size = new Size(203, 141);
            groupBox2.TabIndex = 11;
            groupBox2.TabStop = false;
            groupBox2.Text = "Folder Conversions";
            // 
            // button4
            // 
            button4.Location = new Point(22, 80);
            button4.Margin = new Padding(2);
            button4.Name = "button4";
            button4.Size = new Size(159, 33);
            button4.TabIndex = 3;
            button4.Text = "Folder -> SPF";
            button4.UseVisualStyleBackColor = true;
            button4.Click += BtnFolderToSpf_Click;
            // 
            // button3
            // 
            button3.Location = new Point(22, 43);
            button3.Margin = new Padding(2);
            button3.Name = "button3";
            button3.Size = new Size(159, 33);
            button3.TabIndex = 2;
            button3.Text = "Folder -> PNGs";
            button3.UseVisualStyleBackColor = true;
            button3.Click += BtnSpfToFolder_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ButtonHighlight;
            ClientSize = new Size(450, 177);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "SPFverter";
            groupBox1.ResumeLayout(false);
            groupBox2.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Button button1;
        private Button button2;
        private GroupBox groupBox1;
        private GroupBox groupBox2;
        private Button button3;
        private Button button4;
    }
}