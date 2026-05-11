using tcmb.Models;
using tcmb.Services;

namespace tcmb
{
    public partial class Form1 : Form
    {
        private sealed class DovizItem
        {
            public string Kod { get; }
            public string Isim { get; }
            public DovizItem(string kod, string isim) { Kod = kod; Isim = isim; }
            public override string ToString() => $"{Kod} — {Isim}";
        }

        // TCMB'nin yayın sırasıyla 22 döviz (kod + Türkçe ad)
        private static readonly DovizItem[] DovizListesi = new[]
        {
            new DovizItem("USD", "ABD DOLARI"),
            new DovizItem("AUD", "AVUSTRALYA DOLARI"),
            new DovizItem("DKK", "DANİMARKA KRONU"),
            new DovizItem("EUR", "EURO"),
            new DovizItem("GBP", "İNGİLİZ STERLİNİ"),
            new DovizItem("CHF", "İSVİÇRE FRANGI"),
            new DovizItem("SEK", "İSVEÇ KRONU"),
            new DovizItem("CAD", "KANADA DOLARI"),
            new DovizItem("KWD", "KUVEYT DİNARI"),
            new DovizItem("NOK", "NORVEÇ KRONU"),
            new DovizItem("SAR", "SUUDİ ARABİSTAN RİYALİ"),
            new DovizItem("JPY", "JAPON YENİ"),
            new DovizItem("BGN", "BULGAR LEVASI"),
            new DovizItem("RON", "RUMEN LEYİ"),
            new DovizItem("RUB", "RUS RUBLESİ"),
            new DovizItem("CNY", "ÇİN YUANI"),
            new DovizItem("PKR", "PAKİSTAN RUPİSİ"),
            new DovizItem("QAR", "KATAR RİYALİ"),
            new DovizItem("KRW", "GÜNEY KORE WONU"),
            new DovizItem("AZN", "AZERBAYCAN MANATI"),
            new DovizItem("AED", "BİRLEŞİK ARAP EMİRLİKLERİ DİRHEMİ"),
            new DovizItem("XDR", "ÖZEL ÇEKME HAKKI (SDR)")
        };

        private readonly TcmbClient _tcmb = new();
        private readonly KurRepository _repo = new();

        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object? sender, EventArgs e)
        {
            foreach (var d in DovizListesi)
                lstDovizler.Items.Add(d);
            lstDovizler.SelectedIndex = 0;

            dtpTarih.MaxDate = DateTime.Today;
            dtpTarih.Value = DateTime.Today;

            BuildGridColumns();

            try
            {
                lblStatus.Text = "Veritabanı hazırlanıyor...";
                await _repo.EnsureSchemaAsync();
                lblStatus.Text = "Hazır";
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Veritabanı hatası";
                MessageBox.Show(
                    "MySQL bağlantısı kurulamadı. Servisin çalıştığından ve root/12345 ile bağlanılabildiğinden emin olun.\n\n" + ex.Message,
                    "Veritabanı Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _ = Task.Run(Fill2025InBackgroundAsync);
        }

        private void BuildGridColumns()
        {
            dgvKur.Columns.Clear();

            dgvKur.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Tarih",
                HeaderText = "Tarih",
                FillWeight = 12
            });
            dgvKur.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Kod",
                HeaderText = "Kod",
                FillWeight = 8
            });
            dgvKur.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Isim",
                HeaderText = "Döviz İsmi",
                FillWeight = 22
            });
            dgvKur.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Birim",
                HeaderText = "Birim",
                FillWeight = 8,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            var sayiStil = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleRight,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                Padding = new Padding(8, 4, 12, 4)
            };

            dgvKur.Columns.Add(new DataGridViewTextBoxColumn { Name = "DovizAlis",    HeaderText = "Döviz Alış",    FillWeight = 12, DefaultCellStyle = sayiStil });
            dgvKur.Columns.Add(new DataGridViewTextBoxColumn { Name = "DovizSatis",   HeaderText = "Döviz Satış",   FillWeight = 12, DefaultCellStyle = sayiStil });
            dgvKur.Columns.Add(new DataGridViewTextBoxColumn { Name = "EfektifAlis",  HeaderText = "Efektif Alış",  FillWeight = 12, DefaultCellStyle = sayiStil });
            dgvKur.Columns.Add(new DataGridViewTextBoxColumn { Name = "EfektifSatis", HeaderText = "Efektif Satış", FillWeight = 12, DefaultCellStyle = sayiStil });
        }

        private async Task Fill2025InBackgroundAsync()
        {
            IReadOnlyList<DateTime> missing;
            try
            {
                missing = await _repo.Get2025MissingDatesAsync();
            }
            catch (Exception ex)
            {
                SetStatus("2025 dolumu başlatılamadı: " + ex.Message, progressVisible: false);
                return;
            }

            if (missing.Count == 0)
            {
                SetStatus("2025 verileri hazır", progressVisible: false);
                return;
            }

            SetProgressMax(missing.Count);
            int done = 0;

            using var sem = new SemaphoreSlim(4);
            var tasks = missing.Select(async d =>
            {
                await sem.WaitAsync();
                try
                {
                    var rates = await _tcmb.FetchAsync(d);
                    if (rates == null)
                        await _repo.MarkHolidayAsync(d);
                    else if (rates.Count > 0)
                        await _repo.UpsertDayAsync(d, rates);
                    else
                        await _repo.MarkHolidayAsync(d);
                }
                catch
                {
                    // tek bir günün hatası diğerlerini engellemesin
                }
                finally
                {
                    var c = Interlocked.Increment(ref done);
                    SetStatus($"2025 verisi çekiliyor: {c}/{missing.Count}", progressVisible: true, progressValue: c);
                    sem.Release();
                }
            });

            await Task.WhenAll(tasks);
            SetStatus("2025 verileri hazır", progressVisible: false);
        }

        private void SetStatus(string text, bool progressVisible, int? progressValue = null)
        {
            if (statusStrip.IsDisposed) return;
            if (statusStrip.InvokeRequired)
            {
                statusStrip.BeginInvoke(new Action(() => SetStatus(text, progressVisible, progressValue)));
                return;
            }
            lblStatus.Text = text;
            pbProgress.Visible = progressVisible;
            if (progressValue.HasValue && progressValue.Value <= pbProgress.Maximum)
                pbProgress.Value = progressValue.Value;
        }

        private void SetProgressMax(int max)
        {
            if (statusStrip.IsDisposed) return;
            if (statusStrip.InvokeRequired)
            {
                statusStrip.BeginInvoke(new Action(() => SetProgressMax(max)));
                return;
            }
            pbProgress.Minimum = 0;
            pbProgress.Maximum = max;
            pbProgress.Value = 0;
        }

        private async void btnGoster_Click(object? sender, EventArgs e)
        {
            var tarih = dtpTarih.Value.Date;
            var secili = lstDovizler.SelectedItem as DovizItem;

            if (secili == null)
            {
                MessageBox.Show("Lütfen listeden bir döviz seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var kod = secili.Kod;
            dgvKur.Rows.Clear();
            btnGoster.Enabled = false;

            try
            {
                CurrencyRate? rate = null;

                if (tarih.Year == 2025)
                {
                    rate = await _repo.GetAsync(tarih, kod);

                    if (rate == null)
                    {
                        var has = await _repo.HasDateAsync(tarih);
                        if (has == false)
                        {
                            MessageBox.Show($"{tarih:dd.MM.yyyy} tarihi için TCMB kur açıklamamış (hafta sonu/tatil).",
                                "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                        var live = await _tcmb.FetchAsync(tarih);
                        if (live == null)
                        {
                            await _repo.MarkHolidayAsync(tarih);
                            MessageBox.Show($"{tarih:dd.MM.yyyy} tarihi için TCMB kur açıklamamış (hafta sonu/tatil).",
                                "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                        await _repo.UpsertDayAsync(tarih, live);
                        rate = live.FirstOrDefault(r => r.DovizKodu == kod);
                    }
                }
                else
                {
                    var live = await _tcmb.FetchAsync(tarih);
                    if (live == null)
                    {
                        MessageBox.Show($"{tarih:dd.MM.yyyy} tarihi için TCMB kur açıklamamış (hafta sonu/tatil).",
                            "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    rate = live.FirstOrDefault(r => r.DovizKodu == kod);
                }

                if (rate == null)
                {
                    MessageBox.Show($"{tarih:dd.MM.yyyy} tarihinde {kod} dövizi bulunamadı.",
                        "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                dgvKur.Rows.Add(
                    rate.Tarih.ToString("dd.MM.yyyy"),
                    rate.DovizKodu,
                    rate.DovizIsim ?? secili.Isim,
                    rate.Birim.ToString(),
                    FormatDecimal(rate.DovizAlis),
                    FormatDecimal(rate.DovizSatis),
                    FormatDecimal(rate.EfektifAlis),
                    FormatDecimal(rate.EfektifSatis)
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veri alınırken hata oluştu:\n" + ex.Message,
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnGoster.Enabled = true;
            }
        }

        private static readonly System.Globalization.CultureInfo TrCulture =
            System.Globalization.CultureInfo.GetCultureInfo("tr-TR");

        private static string FormatDecimal(decimal? value)
        {
            return value.HasValue ? value.Value.ToString("N4", TrCulture) : "-";
        }
    }
}
