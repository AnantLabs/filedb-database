﻿namespace FileDbExplorer
{
    partial class EditArrayFrm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose( bool disposing )
        {
            if( disposing && (components != null) )
            {
                components.Dispose();
            }
            base.Dispose( disposing );
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.TxtValues = new System.Windows.Forms.RichTextBox();
            this.BtnOk = new System.Windows.Forms.Button();
            this.BtnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // TxtValues
            // 
            this.TxtValues.Location = new System.Drawing.Point( 0, 0 );
            this.TxtValues.Name = "TxtValues";
            this.TxtValues.Size = new System.Drawing.Size( 343, 378 );
            this.TxtValues.TabIndex = 0;
            this.TxtValues.Text = "";
            // 
            // BtnOk
            // 
            this.BtnOk.Location = new System.Drawing.Point( 95, 388 );
            this.BtnOk.Name = "BtnOk";
            this.BtnOk.Size = new System.Drawing.Size( 110, 32 );
            this.BtnOk.TabIndex = 1;
            this.BtnOk.Text = "OK";
            this.BtnOk.UseVisualStyleBackColor = true;
            this.BtnOk.Click += new System.EventHandler( this.BtnOk_Click );
            // 
            // BtnCancel
            // 
            this.BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.BtnCancel.Location = new System.Drawing.Point( 222, 388 );
            this.BtnCancel.Name = "BtnCancel";
            this.BtnCancel.Size = new System.Drawing.Size( 110, 32 );
            this.BtnCancel.TabIndex = 2;
            this.BtnCancel.Text = "Cancel";
            this.BtnCancel.UseVisualStyleBackColor = true;
            // 
            // EditArrayFrm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF( 8F, 16F );
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size( 344, 428 );
            this.Controls.Add( this.BtnCancel );
            this.Controls.Add( this.BtnOk );
            this.Controls.Add( this.TxtValues );
            this.Name = "EditArrayFrm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Edit Array Values";
            this.Load += new System.EventHandler( this.EditArrayFrm_Load );
            this.ResumeLayout( false );

        }

        #endregion

        private System.Windows.Forms.RichTextBox TxtValues;
        private System.Windows.Forms.Button BtnOk;
        private System.Windows.Forms.Button BtnCancel;
    }
}