namespace tcmb
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        private Label lblTarih;
        private DateTimePicker dtpTarih;
        private Label lblDoviz;
        private ListBox lstDovizler;
        private Button btnGoster;
        private DataGridView dgvKur;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel lblStatus;
        private ToolStripProgressBar pbProgress;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            lblTarih = new Label();
            dtpTarih = new DateTimePicker();
            lblDoviz = new Label();
            lstDovizler = new ListBox();
            btnGoster = new Button();
            dgvKur = new DataGridView();
            statusStrip = new StatusStrip();
            lblStatus = new ToolStripStatusLabel();
            pbProgress = new ToolStripProgressBar();

            ((System.ComponentModel.ISupportInitialize)dgvKur).BeginInit();
            statusStrip.SuspendLayout();
            SuspendLayout();

            // lblTarih
            lblTarih.AutoSize = true;
            lblTarih.Location = new Point(12, 15);
            lblTarih.Text = "Tarih:";

            // dtpTarih
            dtpTarih.Format = DateTimePickerFormat.Short;
            dtpTarih.Location = new Point(60, 12);
            dtpTarih.Size = new Size(150, 23);

            // lblDoviz
            lblDoviz.AutoSize = true;
            lblDoviz.Location = new Point(12, 50);
            lblDoviz.Text = "Döviz:";

            // lstDovizler
            lstDovizler.Location = new Point(12, 70);
            lstDovizler.Size = new Size(130, 320);
            lstDovizler.ItemHeight = 15;

            // btnGoster
            btnGoster.Location = new Point(12, 400);
            btnGoster.Size = new Size(130, 30);
            btnGoster.Text = "Kuru Göster";
            btnGoster.UseVisualStyleBackColor = true;
            btnGoster.Click += btnGoster_Click;

            // dgvKur
            dgvKur.Location = new Point(160, 70);
            dgvKur.Size = new Size(720, 320);
            dgvKur.AllowUserToAddRows = false;
            dgvKur.AllowUserToDeleteRows = false;
            dgvKur.ReadOnly = true;
            dgvKur.RowHeadersVisible = false;
            dgvKur.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvKur.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            // statusStrip
            statusStrip.Items.AddRange(new ToolStripItem[] { lblStatus, pbProgress });
            statusStrip.Location = new Point(0, 450);
            statusStrip.Size = new Size(900, 22);

            // lblStatus
            lblStatus.Text = "Hazır";
            lblStatus.Spring = true;
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;

            // pbProgress
            pbProgress.Size = new Size(200, 16);
            pbProgress.Visible = false;

            // Form1
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(900, 472);
            Controls.Add(lblTarih);
            Controls.Add(dtpTarih);
            Controls.Add(lblDoviz);
            Controls.Add(lstDovizler);
            Controls.Add(btnGoster);
            Controls.Add(dgvKur);
            Controls.Add(statusStrip);
            Text = "TCMB Döviz Kurları";
            Load += Form1_Load;

            ((System.ComponentModel.ISupportInitialize)dgvKur).EndInit();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}
