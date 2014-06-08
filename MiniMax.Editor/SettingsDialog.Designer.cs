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
            this.radioButtonBranchAndBoundMulti = new System.Windows.Forms.RadioButton();
            this.radioButtonParagraph42 = new System.Windows.Forms.RadioButton();
            this.radioButtonBranchAndBoundTree = new System.Windows.Forms.RadioButton();
            this.radioButtonGomory = new System.Windows.Forms.RadioButton();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radioButtonBranchAndBoundMulti);
            this.groupBox1.Controls.Add(this.radioButtonParagraph42);
            this.groupBox1.Controls.Add(this.radioButtonBranchAndBoundTree);
            this.groupBox1.Controls.Add(this.radioButtonGomory);
            this.groupBox1.Location = new System.Drawing.Point(103, 58);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(467, 174);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Алгоритм";
            // 
            // radioButtonBranchAndBoundMulti
            // 
            this.radioButtonBranchAndBoundMulti.AutoSize = true;
            this.radioButtonBranchAndBoundMulti.Location = new System.Drawing.Point(67, 92);
            this.radioButtonBranchAndBoundMulti.Name = "radioButtonBranchAndBoundMulti";
            this.radioButtonBranchAndBoundMulti.Size = new System.Drawing.Size(370, 21);
            this.radioButtonBranchAndBoundMulti.TabIndex = 3;
            this.radioButtonBranchAndBoundMulti.TabStop = true;
            this.radioButtonBranchAndBoundMulti.Text = "Двоичный метод ветвей и границ (множественный)";
            this.radioButtonBranchAndBoundMulti.UseVisualStyleBackColor = true;
            // 
            // radioButtonParagraph42
            // 
            this.radioButtonParagraph42.AutoSize = true;
            this.radioButtonParagraph42.Location = new System.Drawing.Point(67, 119);
            this.radioButtonParagraph42.Name = "radioButtonParagraph42";
            this.radioButtonParagraph42.Size = new System.Drawing.Size(115, 21);
            this.radioButtonParagraph42.TabIndex = 2;
            this.radioButtonParagraph42.TabStop = true;
            this.radioButtonParagraph42.Text = "Параграф 42";
            this.radioButtonParagraph42.UseVisualStyleBackColor = true;
            // 
            // radioButtonBranchAndBoundTree
            // 
            this.radioButtonBranchAndBoundTree.AutoSize = true;
            this.radioButtonBranchAndBoundTree.Location = new System.Drawing.Point(67, 65);
            this.radioButtonBranchAndBoundTree.Name = "radioButtonBranchAndBoundTree";
            this.radioButtonBranchAndBoundTree.Size = new System.Drawing.Size(312, 21);
            this.radioButtonBranchAndBoundTree.TabIndex = 1;
            this.radioButtonBranchAndBoundTree.TabStop = true;
            this.radioButtonBranchAndBoundTree.Text = "Двоичный метод ветвей и границ (дерево)";
            this.radioButtonBranchAndBoundTree.UseVisualStyleBackColor = true;
            // 
            // radioButtonGomory
            // 
            this.radioButtonGomory.AutoSize = true;
            this.radioButtonGomory.Location = new System.Drawing.Point(67, 41);
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
        private System.Windows.Forms.RadioButton radioButtonBranchAndBoundTree;
        private System.Windows.Forms.RadioButton radioButtonGomory;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.RadioButton radioButtonParagraph42;
        private System.Windows.Forms.RadioButton radioButtonBranchAndBoundMulti;
    }
}