namespace Mulvey.RpnCalculator
{
	partial class MainForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			//this.panel1 = new System.Windows.Forms.Panel();
			//this.panel2 = new System.Windows.Forms.Panel();
			//this.nixieDisplay = new RpnCalculator.NixieDisplay();
			this.SuspendLayout();
			// 
			// panel1
			// 
			//this.panel1.BackColor = System.Drawing.Color.Transparent;
			//this.panel1.Location = new System.Drawing.Point(940, 60);
			//this.panel1.Name = "panel1";
			//this.panel1.Size = new System.Drawing.Size(59, 26);
			//this.panel1.TabIndex = 35;
			//this.panel1.Click += new System.EventHandler(this.panel1_Click);
			// 
			// panel2
			// 
			//this.panel2.BackColor = System.Drawing.Color.Transparent;
			//this.panel2.Location = new System.Drawing.Point(940, 31);
			//this.panel2.Name = "panel2";
			//this.panel2.Size = new System.Drawing.Size(59, 26);
			//this.panel2.TabIndex = 36;
			//this.panel2.Click += new System.EventHandler(this.panel2_Click);
			// 
			// nixieDisplay
			// 
			//this.nixieDisplay.BackColor = System.Drawing.Color.Black;
			//this.nixieDisplay.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("nixieDisplay.BackgroundImage")));
			//this.nixieDisplay.Location = new System.Drawing.Point(35, 55);
			//this.nixieDisplay.Name = "nixieDisplay";
			//this.nixieDisplay.Size = new System.Drawing.Size(846, 43);
			//this.nixieDisplay.TabIndex = 34;
			//this.nixieDisplay.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseDown);
			//this.nixieDisplay.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseMove);
			//this.nixieDisplay.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseUp);
			// 
			// Form1
			// 
			//this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			//this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			//this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			//this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
			//this.ClientSize = new System.Drawing.Size(1015, 119);
			//this.Controls.Add(this.panel2);
			//this.Controls.Add(this.panel1);
			//this.Controls.Add(this.nixieDisplay);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			//this.KeyPreview = true;
			//this.MaximizeBox = false;
			//this.Name = "Form1";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "RPN Scientific Calculator by Bret Mulvey";
			//this.TransparencyKey = System.Drawing.Color.Cyan;
			this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseUp);
			this.Click += new System.EventHandler(this.Form1_Click);
			this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Form1_KeyPress);
			this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseMove);
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
			this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseDown);
			this.ResumeLayout(false);

		}

		#endregion

		//private NixieDisplay nixieDisplay;
		//private System.Windows.Forms.Panel panel1;
		//private System.Windows.Forms.Panel panel2;

	}
}

