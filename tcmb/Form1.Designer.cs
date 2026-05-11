namespace tcmb
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        private Panel pnlHeader;
        private Label lblTitle;
        private Label lblSubtitle;

        private Panel pnlBody;
        private Panel pnlFilter;
        private TableLayoutPanel tlpFilter;
        private Label lblFilterTitle;
        private Label lblTarih;
        private DateTimePicker dtpTarih;
        private Label lblDoviz;
        private ListBox lstDovizler;
        private Button btnGoster;

        private Panel pnlResult;
        private Label lblResultTitle;
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

            // Tema renkleri (TCMB kurum tonu)
            var clrPrimary    = Color.FromArgb(26, 58, 92);     // #1A3A5C lacivert
            var clrPrimaryAlt = Color.FromArgb(42, 82, 120);    // hover
            var clrBg         = Color.FromArgb(245, 247, 250);  // gövde arkaplan
            var clrPanelBg    = Color.White;
            var clrBorder     = Color.FromArgb(224, 228, 234);
            var clrAltRow     = Color.FromArgb(248, 250, 253);
            var clrTextPri    = Color.FromArgb(31, 41, 55);
            var clrTextSec    = Color.FromArgb(107, 114, 128);

            var fontTitle    = new Font("Segoe UI", 16F, FontStyle.Bold);
            var fontSubtitle = new Font("Segoe UI", 9F, FontStyle.Regular);
            var fontSection  = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
            var fontLabel    = new Font("Segoe UI", 9.5F, FontStyle.Regular);
            var fontControl  = new Font("Segoe UI", 10F, FontStyle.Regular);
            var fontButton   = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);

            pnlHeader   = new Panel();
            lblTitle    = new Label();
            lblSubtitle = new Label();

            pnlBody         = new Panel();
            pnlFilter       = new Panel();
            tlpFilter       = new TableLayoutPanel();
            lblFilterTitle  = new Label();
            lblTarih        = new Label();
            dtpTarih        = new DateTimePicker();
            lblDoviz        = new Label();
            lstDovizler     = new ListBox();
            btnGoster       = new Button();

            pnlResult       = new Panel();
            lblResultTitle  = new Label();
            dgvKur          = new DataGridView();

            statusStrip = new StatusStrip();
            lblStatus   = new ToolStripStatusLabel();
            pbProgress  = new ToolStripProgressBar();

            ((System.ComponentModel.ISupportInitialize)dgvKur).BeginInit();
            pnlHeader.SuspendLayout();
            pnlBody.SuspendLayout();
            pnlFilter.SuspendLayout();
            tlpFilter.SuspendLayout();
            pnlResult.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();

            // ---------- pnlHeader ----------
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Height = 76;
            pnlHeader.BackColor = clrPrimary;
            pnlHeader.Controls.Add(lblSubtitle);
            pnlHeader.Controls.Add(lblTitle);

            lblTitle.AutoSize = true;
            lblTitle.Text = "TCMB Döviz Kurları";
            lblTitle.ForeColor = Color.White;
            lblTitle.Font = fontTitle;
            lblTitle.Location = new Point(28, 12);

            lblSubtitle.AutoSize = true;
            lblSubtitle.Text = "Türkiye Cumhuriyet Merkez Bankası — Günlük Kur Bilgi Sistemi";
            lblSubtitle.ForeColor = Color.FromArgb(180, 200, 220);
            lblSubtitle.Font = fontSubtitle;
            lblSubtitle.Location = new Point(30, 46);

            // ---------- pnlBody ----------
            pnlBody.Dock = DockStyle.Fill;
            pnlBody.BackColor = clrBg;
            pnlBody.Padding = new Padding(24, 20, 24, 20);
            // dock sırası: önce Fill (pnlResult), sonra Left (pnlFilter) eklenmeli
            pnlBody.Controls.Add(pnlResult);
            pnlBody.Controls.Add(pnlFilter);

            // ---------- pnlFilter (sol panel) ----------
            pnlFilter.Dock = DockStyle.Left;
            pnlFilter.Width = 280;
            pnlFilter.BackColor = clrPanelBg;
            pnlFilter.Padding = new Padding(18, 16, 18, 16);
            pnlFilter.BorderStyle = BorderStyle.FixedSingle;
            pnlFilter.Controls.Add(tlpFilter);

            // ---------- tlpFilter (filtre içi grid) ----------
            tlpFilter.Dock = DockStyle.Fill;
            tlpFilter.ColumnCount = 1;
            tlpFilter.RowCount = 6;
            tlpFilter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpFilter.RowStyles.Add(new RowStyle(SizeType.AutoSize));            // 0: başlık
            tlpFilter.RowStyles.Add(new RowStyle(SizeType.AutoSize));            // 1: Tarih etiketi
            tlpFilter.RowStyles.Add(new RowStyle(SizeType.AutoSize));            // 2: DateTimePicker
            tlpFilter.RowStyles.Add(new RowStyle(SizeType.AutoSize));            // 3: Döviz etiketi
            tlpFilter.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));       // 4: ListBox (genişler)
            tlpFilter.RowStyles.Add(new RowStyle(SizeType.AutoSize));            // 5: Buton
            tlpFilter.BackColor = clrPanelBg;

            lblFilterTitle.AutoSize = true;
            lblFilterTitle.Text = "Sorgu Parametreleri";
            lblFilterTitle.ForeColor = clrPrimary;
            lblFilterTitle.Font = fontSection;
            lblFilterTitle.Margin = new Padding(2, 4, 2, 14);

            lblTarih.AutoSize = true;
            lblTarih.Text = "Tarih";
            lblTarih.ForeColor = clrTextSec;
            lblTarih.Font = fontLabel;
            lblTarih.Margin = new Padding(2, 0, 2, 4);

            dtpTarih.Format = DateTimePickerFormat.Short;
            dtpTarih.Font = fontControl;
            dtpTarih.Dock = DockStyle.Fill;
            dtpTarih.Margin = new Padding(2, 0, 2, 14);

            lblDoviz.AutoSize = true;
            lblDoviz.Text = "Döviz";
            lblDoviz.ForeColor = clrTextSec;
            lblDoviz.Font = fontLabel;
            lblDoviz.Margin = new Padding(2, 0, 2, 4);

            lstDovizler.Dock = DockStyle.Fill;
            lstDovizler.Font = fontControl;
            lstDovizler.BorderStyle = BorderStyle.FixedSingle;
            lstDovizler.IntegralHeight = false;
            lstDovizler.Margin = new Padding(2, 0, 2, 14);

            btnGoster.Dock = DockStyle.Fill;
            btnGoster.Height = 42;
            btnGoster.Text = "KURU GÖSTER";
            btnGoster.Font = fontButton;
            btnGoster.ForeColor = Color.White;
            btnGoster.BackColor = clrPrimary;
            btnGoster.FlatStyle = FlatStyle.Flat;
            btnGoster.FlatAppearance.BorderSize = 0;
            btnGoster.FlatAppearance.MouseOverBackColor = clrPrimaryAlt;
            btnGoster.FlatAppearance.MouseDownBackColor = clrPrimaryAlt;
            btnGoster.Cursor = Cursors.Hand;
            btnGoster.UseVisualStyleBackColor = false;
            btnGoster.Margin = new Padding(2, 0, 2, 2);
            btnGoster.MinimumSize = new Size(0, 42);
            btnGoster.Click += btnGoster_Click;

            tlpFilter.Controls.Add(lblFilterTitle, 0, 0);
            tlpFilter.Controls.Add(lblTarih,        0, 1);
            tlpFilter.Controls.Add(dtpTarih,        0, 2);
            tlpFilter.Controls.Add(lblDoviz,        0, 3);
            tlpFilter.Controls.Add(lstDovizler,     0, 4);
            tlpFilter.Controls.Add(btnGoster,       0, 5);

            // ---------- pnlResult (sağ panel) ----------
            pnlResult.Dock = DockStyle.Fill;
            pnlResult.BackColor = clrPanelBg;
            pnlResult.Padding = new Padding(20, 16, 20, 16);
            pnlResult.BorderStyle = BorderStyle.FixedSingle;
            // dock sırası: önce Fill (dgvKur), sonra Top (lblResultTitle)
            pnlResult.Controls.Add(dgvKur);
            pnlResult.Controls.Add(lblResultTitle);

            lblResultTitle.Dock = DockStyle.Top;
            lblResultTitle.AutoSize = false;
            lblResultTitle.Height = 32;
            lblResultTitle.Text = "Kur Bilgisi";
            lblResultTitle.ForeColor = clrPrimary;
            lblResultTitle.Font = fontSection;
            lblResultTitle.TextAlign = ContentAlignment.MiddleLeft;
            lblResultTitle.Padding = new Padding(0, 0, 0, 8);

            dgvKur.Dock = DockStyle.Fill;
            dgvKur.BackgroundColor = clrPanelBg;
            dgvKur.BorderStyle = BorderStyle.None;
            dgvKur.AllowUserToAddRows = false;
            dgvKur.AllowUserToDeleteRows = false;
            dgvKur.AllowUserToResizeRows = false;
            dgvKur.ReadOnly = true;
            dgvKur.RowHeadersVisible = false;
            dgvKur.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvKur.MultiSelect = false;
            dgvKur.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvKur.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvKur.GridColor = clrBorder;
            dgvKur.EnableHeadersVisualStyles = false;
            dgvKur.ColumnHeadersHeight = 40;
            dgvKur.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvKur.ColumnHeadersDefaultCellStyle.BackColor = clrPrimary;
            dgvKur.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvKur.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
            dgvKur.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvKur.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 8, 0);
            dgvKur.ColumnHeadersDefaultCellStyle.SelectionBackColor = clrPrimary;
            dgvKur.DefaultCellStyle.Font = fontControl;
            dgvKur.DefaultCellStyle.ForeColor = clrTextPri;
            dgvKur.DefaultCellStyle.BackColor = clrPanelBg;
            dgvKur.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 232, 245);
            dgvKur.DefaultCellStyle.SelectionForeColor = clrTextPri;
            dgvKur.DefaultCellStyle.Padding = new Padding(8, 4, 8, 4);
            dgvKur.AlternatingRowsDefaultCellStyle.BackColor = clrAltRow;
            dgvKur.RowTemplate.Height = 36;
            dgvKur.RowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 232, 245);

            // ---------- statusStrip ----------
            statusStrip.Items.AddRange(new ToolStripItem[] { lblStatus, pbProgress });
            statusStrip.SizingGrip = false;
            statusStrip.BackColor = Color.FromArgb(235, 238, 243);
            statusStrip.Font = fontLabel;

            lblStatus.Text = "Hazır";
            lblStatus.Spring = true;
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            lblStatus.ForeColor = clrTextSec;
            lblStatus.Padding = new Padding(12, 0, 0, 0);

            pbProgress.Size = new Size(220, 16);
            pbProgress.Visible = false;
            pbProgress.Style = ProgressBarStyle.Continuous;

            // ---------- Form1 ----------
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(980, 620);
            MinimumSize = new Size(900, 600);
            BackColor = clrBg;
            // Dock sırası: önce Fill (pnlBody), sonra Top (pnlHeader), en son Bottom (statusStrip)
            Controls.Add(pnlBody);
            Controls.Add(pnlHeader);
            Controls.Add(statusStrip);
            Font = fontControl;
            Text = "TCMB Döviz Kurları";
            StartPosition = FormStartPosition.CenterScreen;
            Load += Form1_Load;

            ((System.ComponentModel.ISupportInitialize)dgvKur).EndInit();
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            tlpFilter.ResumeLayout(false);
            tlpFilter.PerformLayout();
            pnlFilter.ResumeLayout(false);
            pnlResult.ResumeLayout(false);
            pnlResult.PerformLayout();
            pnlBody.ResumeLayout(false);
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}
