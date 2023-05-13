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
            textBox1 = new TextBox();
            textBox2 = new TextBox();
            label1 = new Label();
            label2 = new Label();
            groupBox2 = new GroupBox();
            button4 = new Button();
            button3 = new Button();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(31, 52);
            button1.Name = "button1";
            button1.Size = new Size(167, 40);
            button1.TabIndex = 0;
            button1.Text = "SPF -> Modern";
            button1.UseVisualStyleBackColor = true;
            button1.Click += BtnSpfToModern_Click;
            // 
            // button2
            // 
            button2.Location = new Point(31, 98);
            button2.Name = "button2";
            button2.Size = new Size(167, 40);
            button2.TabIndex = 1;
            button2.Text = "Modern -> SPF";
            button2.UseVisualStyleBackColor = true;
            button2.Click += BtnModernToSpf_Click;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(button1);
            groupBox1.Controls.Add(button2);
            groupBox1.Location = new Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(229, 175);
            groupBox1.TabIndex = 2;
            groupBox1.TabStop = false;
            groupBox1.Text = "Single File Process";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(150, 131);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(229, 35);
            textBox1.TabIndex = 3;
            // 
            // textBox2
            // 
            textBox2.Location = new Point(150, 171);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(229, 35);
            textBox2.TabIndex = 4;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(30, 131);
            label1.Name = "label1";
            label1.Size = new Size(114, 30);
            label1.TabIndex = 5;
            label1.Text = "Input Path:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(13, 171);
            label2.Name = "label2";
            label2.Size = new Size(131, 30);
            label2.TabIndex = 6;
            label2.Text = "Output Path:";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(button4);
            groupBox2.Controls.Add(button3);
            groupBox2.Controls.Add(textBox1);
            groupBox2.Controls.Add(textBox2);
            groupBox2.Controls.Add(label2);
            groupBox2.Controls.Add(label1);
            groupBox2.Location = new Point(247, 12);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(409, 229);
            groupBox2.TabIndex = 7;
            groupBox2.TabStop = false;
            groupBox2.Text = "Folder Process";
            // 
            // button4
            // 
            button4.Location = new Point(218, 52);
            button4.Name = "button4";
            button4.Size = new Size(161, 40);
            button4.TabIndex = 9;
            button4.Text = "Folder -> SPF";
            button4.UseVisualStyleBackColor = true;
            button4.Click += BtnFolderToSpf_Click;
            // 
            // button3
            // 
            button3.Location = new Point(30, 52);
            button3.Name = "button3";
            button3.Size = new Size(167, 40);
            button3.TabIndex = 8;
            button3.Text = "Folder -> PNGs";
            button3.UseVisualStyleBackColor = true;
            button3.Click += BtnSpfToFolder_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(12F, 30F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ButtonHighlight;
            ClientSize = new Size(686, 269);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "SPFverter";
            groupBox1.ResumeLayout(false);
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Button button1;
        private Button button2;
        private GroupBox groupBox1;
        private TextBox textBox1;
        private TextBox textBox2;
        private Label label1;
        private Label label2;
        private GroupBox groupBox2;
        private Button button3;
        private Button button4;
    }
}