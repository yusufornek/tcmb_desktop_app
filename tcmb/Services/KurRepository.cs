using MySqlConnector;
using tcmb.Models;

namespace tcmb.Services
{
    public class KurRepository
    {
        private const string ServerConnString = "Server=localhost;User ID=root;Password=12345;Charset=utf8mb4";
        private const string DbConnString = "Server=localhost;User ID=root;Password=12345;Database=tcmb_kurlar;Charset=utf8mb4";

        public async Task EnsureSchemaAsync()
        {
            await using (var conn = new MySqlConnection(ServerConnString))
            {
                await conn.OpenAsync();
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = "CREATE DATABASE IF NOT EXISTS tcmb_kurlar CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;";
                await cmd.ExecuteNonQueryAsync();
            }

            await using (var conn = new MySqlConnection(DbConnString))
            {
                await conn.OpenAsync();
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS kurlar (
  Tarih        DATE          NOT NULL,
  DovizKodu    VARCHAR(5)    NOT NULL,
  DovizIsim    VARCHAR(50)   NULL,
  Birim        INT           NOT NULL DEFAULT 1,
  DovizAlis    DECIMAL(18,6) NULL,
  DovizSatis   DECIMAL(18,6) NULL,
  EfektifAlis  DECIMAL(18,6) NULL,
  EfektifSatis DECIMAL(18,6) NULL,
  PRIMARY KEY (Tarih, DovizKodu)
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS yoklanan_gunler (
  Tarih    DATE    PRIMARY KEY,
  VeriVar  TINYINT NOT NULL
) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;";
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task<CurrencyRate?> GetAsync(DateTime tarih, string kod)
        {
            await using var conn = new MySqlConnection(DbConnString);
            await conn.OpenAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT Tarih, DovizKodu, DovizIsim, Birim, DovizAlis, DovizSatis, EfektifAlis, EfektifSatis
                                FROM kurlar WHERE Tarih = @t AND DovizKodu = @k LIMIT 1;";
            cmd.Parameters.AddWithValue("@t", tarih.Date);
            cmd.Parameters.AddWithValue("@k", kod);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return null;

            return new CurrencyRate(
                Tarih: reader.GetDateTime(0),
                DovizKodu: reader.GetString(1),
                DovizIsim: reader.IsDBNull(2) ? null : reader.GetString(2),
                Birim: reader.GetInt32(3),
                DovizAlis: reader.IsDBNull(4) ? null : reader.GetDecimal(4),
                DovizSatis: reader.IsDBNull(5) ? null : reader.GetDecimal(5),
                EfektifAlis: reader.IsDBNull(6) ? null : reader.GetDecimal(6),
                EfektifSatis: reader.IsDBNull(7) ? null : reader.GetDecimal(7)
            );
        }

        public async Task<bool?> HasDateAsync(DateTime tarih)
        {
            await using var conn = new MySqlConnection(DbConnString);
            await conn.OpenAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT VeriVar FROM yoklanan_gunler WHERE Tarih = @t LIMIT 1;";
            cmd.Parameters.AddWithValue("@t", tarih.Date);

            var result = await cmd.ExecuteScalarAsync();
            if (result == null || result == DBNull.Value)
                return null;
            return Convert.ToInt32(result) == 1;
        }

        public async Task UpsertDayAsync(DateTime tarih, IReadOnlyList<CurrencyRate> rates)
        {
            await using var conn = new MySqlConnection(DbConnString);
            await conn.OpenAsync();
            await using var tx = await conn.BeginTransactionAsync();

            foreach (var r in rates)
            {
                await using var cmd = conn.CreateCommand();
                cmd.Transaction = tx;
                cmd.CommandText = @"INSERT INTO kurlar (Tarih, DovizKodu, DovizIsim, Birim, DovizAlis, DovizSatis, EfektifAlis, EfektifSatis)
                                    VALUES (@t, @k, @i, @b, @da, @ds, @ea, @es)
                                    ON DUPLICATE KEY UPDATE
                                      DovizIsim = VALUES(DovizIsim),
                                      Birim = VALUES(Birim),
                                      DovizAlis = VALUES(DovizAlis),
                                      DovizSatis = VALUES(DovizSatis),
                                      EfektifAlis = VALUES(EfektifAlis),
                                      EfektifSatis = VALUES(EfektifSatis);";
                cmd.Parameters.AddWithValue("@t", r.Tarih.Date);
                cmd.Parameters.AddWithValue("@k", r.DovizKodu);
                cmd.Parameters.AddWithValue("@i", (object?)r.DovizIsim ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@b", r.Birim);
                cmd.Parameters.AddWithValue("@da", (object?)r.DovizAlis ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ds", (object?)r.DovizSatis ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ea", (object?)r.EfektifAlis ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@es", (object?)r.EfektifSatis ?? DBNull.Value);
                await cmd.ExecuteNonQueryAsync();
            }

            await using (var mark = conn.CreateCommand())
            {
                mark.Transaction = tx;
                mark.CommandText = @"INSERT INTO yoklanan_gunler (Tarih, VeriVar) VALUES (@t, 1)
                                     ON DUPLICATE KEY UPDATE VeriVar = 1;";
                mark.Parameters.AddWithValue("@t", tarih.Date);
                await mark.ExecuteNonQueryAsync();
            }

            await tx.CommitAsync();
        }

        public async Task MarkHolidayAsync(DateTime tarih)
        {
            await using var conn = new MySqlConnection(DbConnString);
            await conn.OpenAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO yoklanan_gunler (Tarih, VeriVar) VALUES (@t, 0)
                                ON DUPLICATE KEY UPDATE VeriVar = VeriVar;";
            cmd.Parameters.AddWithValue("@t", tarih.Date);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<IReadOnlyList<DateTime>> Get2025MissingDatesAsync()
        {
            var checkedDates = new HashSet<DateTime>();

            await using (var conn = new MySqlConnection(DbConnString))
            {
                await conn.OpenAsync();
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT Tarih FROM yoklanan_gunler WHERE Tarih >= '2025-01-01' AND Tarih <= '2025-12-31';";
                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                    checkedDates.Add(reader.GetDateTime(0).Date);
            }

            var missing = new List<DateTime>();
            var start = new DateTime(2025, 1, 1);
            var endLimit = new DateTime(2025, 12, 31);
            var end = DateTime.Today < endLimit ? DateTime.Today : endLimit;

            for (var d = start; d <= end; d = d.AddDays(1))
            {
                if (!checkedDates.Contains(d))
                    missing.Add(d);
            }

            return missing;
        }
    }
}
