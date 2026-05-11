using tcmb.Models;
using tcmb.Services;

namespace tcmb
{
    public partial class Form1 : Form
    {
        private static readonly string[] DovizKodlari = new[]
        {
            "USD", "AUD", "DKK", "EUR", "GBP", "CHF", "SEK", "CAD", "KWD", "NOK",
            "SAR", "JPY", "BGN", "RON", "RUB", "CNY", "PKR", "QAR", "KRW", "AZN", "AED", "XDR"
        };

        private readonly TcmbClient _tcmb = new();
        private readonly KurRepository _repo = new();

        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object? sender, EventArgs e)
        {
            foreach (var kod in DovizKodlari)
                lstDovizler.Items.Add(kod);
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
                lblStatus.Text = "DB hatası";
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
            dgvKur.Columns.Add("Tarih", "Tarih");
            dgvKur.Columns.Add("Kod", "Döviz Kodu");
            dgvKur.Columns.Add("Isim", "İsim");
            dgvKur.Columns.Add("Birim", "Birim");
            dgvKur.Columns.Add("DovizAlis", "Döviz Alış");
            dgvKur.Columns.Add("DovizSatis", "Döviz Satış");
            dgvKur.Columns.Add("EfektifAlis", "Efektif Alış");
            dgvKur.Columns.Add("EfektifSatis", "Efektif Satış");
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
                SetStatus("2025 verileri hazır ✓", progressVisible: false);
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
                    // tek bir günde hata varsa diğerlerini engelleme
                }
                finally
                {
                    var c = Interlocked.Increment(ref done);
                    SetStatus($"2025 verisi çekiliyor: {c}/{missing.Count}", progressVisible: true, progressValue: c);
                    sem.Release();
                }
            });

            await Task.WhenAll(tasks);
            SetStatus("2025 verileri hazır ✓", progressVisible: false);
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
            var kod = lstDovizler.SelectedItem as string;

            if (string.IsNullOrEmpty(kod))
            {
                MessageBox.Show("Lütfen listeden bir döviz seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

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
                        // DB'de henüz yok — TCMB'den canlı çek (arka plan dolumu o güne ulaşmamış olabilir)
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
                    rate.DovizIsim ?? "",
                    rate.Birim,
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

        private static string FormatDecimal(decimal? value)
        {
            return value.HasValue ? value.Value.ToString("N4", System.Globalization.CultureInfo.GetCultureInfo("tr-TR")) : "-";
        }
    }
}
