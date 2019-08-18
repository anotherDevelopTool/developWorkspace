namespace SerialBox
{
    partial class SerialBox
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lbl1 = new System.Windows.Forms.Label();
            this.lbl2 = new System.Windows.Forms.Label();
            this.lbl4 = new System.Windows.Forms.Label();
            this.lbl3 = new System.Windows.Forms.Label();
            this.Box1 = new FilterControls.FilterTextBox();
            this.Box2 = new FilterControls.FilterTextBox();
            this.Box3 = new FilterControls.FilterTextBox();
            this.Box4 = new FilterControls.FilterTextBox();
            this.Box5 = new FilterControls.FilterTextBox();
            this.SuspendLayout();
            // 
            // lbl1
            // 
            this.lbl1.AutoSize = true;
            this.lbl1.Location = new System.Drawing.Point(53, 0);
            this.lbl1.Name = "lbl1";
            this.lbl1.Size = new System.Drawing.Size(10, 13);
            this.lbl1.TabIndex = 1;
            this.lbl1.Text = "-";
            // 
            // lbl2
            // 
            this.lbl2.AutoSize = true;
            this.lbl2.Location = new System.Drawing.Point(113, 0);
            this.lbl2.Name = "lbl2";
            this.lbl2.Size = new System.Drawing.Size(10, 13);
            this.lbl2.TabIndex = 3;
            this.lbl2.Text = "-";
            // 
            // lbl4
            // 
            this.lbl4.AutoSize = true;
            this.lbl4.Location = new System.Drawing.Point(233, 0);
            this.lbl4.Name = "lbl4";
            this.lbl4.Size = new System.Drawing.Size(10, 13);
            this.lbl4.TabIndex = 7;
            this.lbl4.Text = "-";
            this.lbl4.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lbl3
            // 
            this.lbl3.AutoSize = true;
            this.lbl3.Location = new System.Drawing.Point(173, 0);
            this.lbl3.Name = "lbl3";
            this.lbl3.Size = new System.Drawing.Size(10, 13);
            this.lbl3.TabIndex = 5;
            this.lbl3.Text = "-";
            // 
            // Box1
            // 
            this.Box1.Location = new System.Drawing.Point(0, 0);
            this.Box1.MaxLength = 5;
            this.Box1.Name = "Box1";
            this.Box1.Size = new System.Drawing.Size(53, 18);
            this.Box1.TabIndex = 8;
            this.Box1.Enter += new System.EventHandler(this.TextBox_Enter);
            this.Box1.TextChanged += new System.EventHandler(this.TextBox_TextChanged);
            // 
            // Box2
            // 
            this.Box2.Location = new System.Drawing.Point(60, 0);
            this.Box2.MaxLength = 5;
            this.Box2.Name = "Box2";
            this.Box2.Size = new System.Drawing.Size(53, 18);
            this.Box2.TabIndex = 9;
            this.Box2.Enter += new System.EventHandler(this.TextBox_Enter);
            this.Box2.TextChanged += new System.EventHandler(this.TextBox_TextChanged);
            // 
            // Box3
            // 
            this.Box3.Location = new System.Drawing.Point(120, 0);
            this.Box3.MaxLength = 5;
            this.Box3.Name = "Box3";
            this.Box3.Size = new System.Drawing.Size(53, 18);
            this.Box3.TabIndex = 10;
            this.Box3.Enter += new System.EventHandler(this.TextBox_Enter);
            this.Box3.TextChanged += new System.EventHandler(this.TextBox_TextChanged);
            // 
            // Box4
            // 
            this.Box4.Location = new System.Drawing.Point(180, 0);
            this.Box4.MaxLength = 5;
            this.Box4.Name = "Box4";
            this.Box4.Size = new System.Drawing.Size(53, 18);
            this.Box4.TabIndex = 11;
            this.Box4.Enter += new System.EventHandler(this.TextBox_Enter);
            this.Box4.TextChanged += new System.EventHandler(this.TextBox_TextChanged);
            // 
            // Box5
            // 
            this.Box5.Location = new System.Drawing.Point(240, 0);
            this.Box5.MaxLength = 5;
            this.Box5.Name = "Box5";
            this.Box5.Size = new System.Drawing.Size(53, 18);
            this.Box5.TabIndex = 12;
            this.Box5.Enter += new System.EventHandler(this.TextBox_Enter);
            this.Box5.TextChanged += new System.EventHandler(this.TextBox_TextChanged);
            // 
            // SerialBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.Box2);
            this.Controls.Add(this.Box3);
            this.Controls.Add(this.Box4);
            this.Controls.Add(this.Box5);
            this.Controls.Add(this.Box1);
            this.Controls.Add(this.lbl3);
            this.Controls.Add(this.lbl4);
            this.Controls.Add(this.lbl2);
            this.Controls.Add(this.lbl1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(178)));
            this.Name = "SerialBox";
            this.Size = new System.Drawing.Size(293, 18);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbl1;
        private System.Windows.Forms.Label lbl2;
        private System.Windows.Forms.Label lbl3;
        private System.Windows.Forms.Label lbl4;
        private FilterControls.FilterTextBox Box1;
        private FilterControls.FilterTextBox Box2;
        private FilterControls.FilterTextBox Box3;
        private FilterControls.FilterTextBox Box4;
        private FilterControls.FilterTextBox Box5;
    }
}
