namespace tcmb.Models
{
    public record CurrencyRate(
        DateTime Tarih,
        string DovizKodu,
        string? DovizIsim,
        int Birim,
        decimal? DovizAlis,
        decimal? DovizSatis,
        decimal? EfektifAlis,
        decimal? EfektifSatis
    );
}
