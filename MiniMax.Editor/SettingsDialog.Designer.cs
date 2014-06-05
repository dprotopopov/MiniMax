namespace MiniMax.Editor
{
    partial class SettingsDialog
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radioButtonBranchAndBound = new System.Windows.Forms.RadioButton();
            this.radioButtonGomory = new System.Windows.Forms.RadioButton();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radioButtonBranchAndBound);
            this.groupBox1.Controls.Add(this.radioButtonGomory);
            this.groupBox1.Location = new System.Drawing.Point(103, 58);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(467, 174);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Алгоритм";
            // 
            // radioButtonBranchAndBound
            // 
            this.radioButtonBranchAndBound.AutoSize = true;
            this.radioButtonBranchAndBound.Location = new System.Drawing.Point(111, 92);
            this.radioButtonBranchAndBound.Name = "radioButtonBranchAndBound";
            this.radioButtonBranchAndBound.Size = new System.Drawing.Size(251, 21);
            this.radioButtonBranchAndBound.TabIndex = 1;
            this.radioButtonBranchAndBound.TabStop = true;
            this.radioButtonBranchAndBound.Text = "Двоичный метод ветвей и границ";
            this.radioButtonBranchAndBound.UseVisualStyleBackColor = true;
            // 
            // radioButtonGomory
            // 
            this.radioButtonGomory.AutoSize = true;
            this.radioButtonGomory.Location = new System.Drawing.Point(111, 64);
            this.radioButtonGomory.Name = "radioButtonGomory";
            this.radioButtonGomory.Size = new System.Drawing.Size(144, 21);
            this.radioButtonGomory.TabIndex = 0;
            this.radioButtonGomory.TabStop = true;
            this.radioButtonGomory.Text = "Алгоритм Гомори";
            this.radioButtonGomory.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button2.Location = new System.Drawing.Point(323, 271);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 17;
            this.button2.Text = "Ok";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button1.Location = new System.Drawing.Point(404, 271);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 16;
            this.button1.Text = "Cancel";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // SettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(681, 328);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.groupBox1);
            this.Name = "SettingsDialog";
            this.Text = "SettingsDialog";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radioButtonBranchAndBound;
        private System.Windows.Forms.RadioButton radioButtonGomory;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
    }
}