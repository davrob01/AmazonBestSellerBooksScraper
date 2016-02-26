/* Copyright (c) David Robertson 2016 */
namespace AmazonBestSellers
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.btnStart = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblStatus = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.lblComplete = new System.Windows.Forms.Label();
            this.lblTimeValue = new System.Windows.Forms.Label();
            this.lblTime = new System.Windows.Forms.Label();
            this.lblBooksValue = new System.Windows.Forms.Label();
            this.lblBooks = new System.Windows.Forms.Label();
            this.btnTest = new System.Windows.Forms.Button();
            this.chkDetail = new System.Windows.Forms.CheckBox();
            this.lblSeeResults = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(12, 225);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 23);
            this.btnStart.TabIndex = 0;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.Window;
            this.panel1.Controls.Add(this.lblStatus);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.lblBooksValue);
            this.panel1.Controls.Add(this.lblBooks);
            this.panel1.Location = new System.Drawing.Point(12, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(260, 149);
            this.panel1.TabIndex = 1;
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(3, 50);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(136, 13);
            this.lblStatus.TabIndex = 5;
            this.lblStatus.Text = "Press start to scrape ISBNs";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.lblSeeResults);
            this.panel2.Controls.Add(this.lblComplete);
            this.panel2.Controls.Add(this.lblTimeValue);
            this.panel2.Controls.Add(this.lblTime);
            this.panel2.Location = new System.Drawing.Point(0, 66);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(227, 83);
            this.panel2.TabIndex = 5;
            this.panel2.Visible = false;
            // 
            // lblComplete
            // 
            this.lblComplete.AutoSize = true;
            this.lblComplete.Location = new System.Drawing.Point(3, 10);
            this.lblComplete.Name = "lblComplete";
            this.lblComplete.Size = new System.Drawing.Size(95, 13);
            this.lblComplete.TabIndex = 4;
            this.lblComplete.Text = "Process Complete!";
            // 
            // lblTimeValue
            // 
            this.lblTimeValue.AutoSize = true;
            this.lblTimeValue.Location = new System.Drawing.Point(98, 58);
            this.lblTimeValue.Name = "lblTimeValue";
            this.lblTimeValue.Size = new System.Drawing.Size(0, 13);
            this.lblTimeValue.TabIndex = 1;
            // 
            // lblTime
            // 
            this.lblTime.AutoSize = true;
            this.lblTime.Location = new System.Drawing.Point(3, 58);
            this.lblTime.Name = "lblTime";
            this.lblTime.Size = new System.Drawing.Size(83, 13);
            this.lblTime.TabIndex = 0;
            this.lblTime.Text = "Execution Time:";
            // 
            // lblBooksValue
            // 
            this.lblBooksValue.AutoSize = true;
            this.lblBooksValue.Location = new System.Drawing.Point(92, 20);
            this.lblBooksValue.Name = "lblBooksValue";
            this.lblBooksValue.Size = new System.Drawing.Size(13, 13);
            this.lblBooksValue.TabIndex = 3;
            this.lblBooksValue.Text = "0";
            // 
            // lblBooks
            // 
            this.lblBooks.AutoSize = true;
            this.lblBooks.Location = new System.Drawing.Point(3, 20);
            this.lblBooks.Name = "lblBooks";
            this.lblBooks.Size = new System.Drawing.Size(77, 13);
            this.lblBooks.TabIndex = 2;
            this.lblBooks.Text = "Books Added: ";
            // 
            // btnTest
            // 
            this.btnTest.Location = new System.Drawing.Point(197, 225);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(75, 23);
            this.btnTest.TabIndex = 2;
            this.btnTest.Text = "Test Run";
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // chkDetail
            // 
            this.chkDetail.AutoSize = true;
            this.chkDetail.Location = new System.Drawing.Point(12, 180);
            this.chkDetail.Name = "chkDetail";
            this.chkDetail.Size = new System.Drawing.Size(216, 17);
            this.chkDetail.TabIndex = 3;
            this.chkDetail.Text = "Include book title and category in results";
            this.chkDetail.UseVisualStyleBackColor = true;
            // 
            // lblSeeResults
            // 
            this.lblSeeResults.AutoSize = true;
            this.lblSeeResults.Location = new System.Drawing.Point(3, 35);
            this.lblSeeResults.Name = "lblSeeResults";
            this.lblSeeResults.Size = new System.Drawing.Size(189, 13);
            this.lblSeeResults.TabIndex = 5;
            this.lblSeeResults.Text = "New book data is in the Results folder.";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 292);
            this.Controls.Add(this.chkDetail);
            this.Controls.Add(this.btnTest);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.btnStart);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "Amazon Best Sellers";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lblTimeValue;
        private System.Windows.Forms.Label lblTime;
        private System.Windows.Forms.Label lblBooksValue;
        private System.Windows.Forms.Label lblBooks;
        private System.Windows.Forms.Label lblComplete;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btnTest;
        private System.Windows.Forms.CheckBox chkDetail;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblSeeResults;
    }
}

