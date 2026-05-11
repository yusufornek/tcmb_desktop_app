using System.Globalization;
using System.Net;
using System.Xml.Linq;
using tcmb.Models;

namespace tcmb.Services
{
    public class TcmbClient
    {
        private static readonly HttpClient _http = CreateHttpClient();

        private static HttpClient CreateHttpClient()
        {
            var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
            client.DefaultRequestHeaders.UserAgent.ParseAdd("tcmb-winforms-app/1.0");
            return client;
        }

        public async Task<IReadOnlyList<CurrencyRate>?> FetchAsync(DateTime tarih, CancellationToken ct = default)
        {
            var url = $"https://www.tcmb.gov.tr/kurlar/{tarih:yyyyMM}/{tarih:ddMMyyyy}.xml";

            using var resp = await _http.GetAsync(url, ct);
            if (resp.StatusCode == HttpStatusCode.NotFound)
                return null;

            resp.EnsureSuccessStatusCode();

            var xml = await resp.Content.ReadAsStringAsync(ct);
            var doc = XDocument.Parse(xml);

            var list = new List<CurrencyRate>(22);
            foreach (var el in doc.Descendants("Currency"))
            {
                var kod = (string?)el.Attribute("Kod");
                if (string.IsNullOrWhiteSpace(kod))
                    continue;

                var isim = (string?)el.Element("Isim");
                var birim = ParseInt(el.Element("Unit")?.Value) ?? 1;

                list.Add(new CurrencyRate(
                    Tarih: tarih.Date,
                    DovizKodu: kod,
                    DovizIsim: isim,
                    Birim: birim,
                    DovizAlis: ParseDecimal(el.Element("ForexBuying")?.Value),
                    DovizSatis: ParseDecimal(el.Element("ForexSelling")?.Value),
                    EfektifAlis: ParseDecimal(el.Element("BanknoteBuying")?.Value),
                    EfektifSatis: ParseDecimal(el.Element("BanknoteSelling")?.Value)
                ));
            }

            return list;
        }

        private static decimal? ParseDecimal(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            return decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var d) ? d : null;
        }

        private static int? ParseInt(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i) ? i : null;
        }
    }
}
