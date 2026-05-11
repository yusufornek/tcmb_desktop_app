# TCMB Döviz Kuru Uygulaması

Türkiye Cumhuriyet Merkez Bankası'nın günlük yayınladığı XML kur dosyalarını çeken, MySQL veritabanında saklayan ve seçilen tarih ile döviz koduna göre Döviz Alış, Döviz Satış, Efektif Alış, Efektif Satış değerlerini görüntüleyen Windows Forms uygulamasıdır.

## Özellikler

- Kullanıcı bir tarih (DateTimePicker) ve bir döviz (ListBox) seçerek o güne ait kur verisini DataGridView üzerinde görüntüler.
- Uygulama ilk açılışta, içinde bulunulan yıla kadar olan **2025 yılı verilerini** TCMB'den arka planda paralel olarak çeker ve MySQL veritabanına yazar. Aynı uygulama yeniden açıldığında yalnızca eksik günler tamamlanır.
- 2025 yılı için sorgular yerel veritabanından, diğer yıllar için doğrudan TCMB'nin yayın URL'inden alınır.
- Hafta sonu ve resmi tatil günleri için TCMB veri yayınlamadığından bu günler ayrı bir tabloda işaretlenir ve tekrar çekilmeye çalışılmaz.
- TCMB'nin yayınladığı 22 döviz kodunun tamamı desteklenir.

## Sistem Gereksinimleri

| Bileşen | Sürüm |
|---|---|
| İşletim Sistemi | Windows 10 / 11 |
| .NET SDK | .NET 10 (`net10.0-windows`) |
| Geliştirme Ortamı | Visual Studio 2026 veya `dotnet` CLI |
| Veritabanı | MySQL Server 5.7 veya üzeri (varsayılan port 3306) |
| NuGet | MySqlConnector 2.4.0 (proje açılışında otomatik restore edilir) |

## Kurulum

```bash
git clone https://github.com/yusufornek/tcmb_desktop_app.git
cd tcmb_desktop_app
dotnet restore tcmb/tcmb.csproj
dotnet build tcmb/tcmb.csproj
```

Visual Studio kullanılıyorsa `tcmb.sln` açılıp F5 ile doğrudan çalıştırılabilir. NuGet paketleri ilk derlemede otomatik indirilir.

## Yapılandırma

Uygulamanın bağlantı ve veri kaynağı ayarları aşağıdaki dosyalardadır.

### 1. MySQL Bağlantı Bilgileri

**Dosya:** `tcmb/Services/KurRepository.cs`

Dosyanın en üstünde yer alan iki sabit, kullanıcı adı, şifre, sunucu adresi ve veritabanı adını içerir:

```csharp
private const string ServerConnString = "Server=localhost;User ID=root;Password=12345;Charset=utf8mb4";
private const string DbConnString     = "Server=localhost;User ID=root;Password=12345;Database=tcmb_kurlar;Charset=utf8mb4";
```

Değiştirilebilecek parametreler:

| Parametre | Açıklama | Varsayılan |
|---|---|---|
| `Server` | MySQL sunucu adresi. Uzak sunucu için IP veya host adı girilir. | `localhost` |
| `Port` | Standart dışı port kullanılıyorsa bağlantı string'ine `Port=3307` şeklinde eklenir. | 3306 |
| `User ID` | MySQL kullanıcısı | `root` |
| `Password` | MySQL şifresi | `12345` |
| `Database` | Veritabanı adı (`DbConnString` içinde). Değiştirilirse `EnsureSchemaAsync` metodundaki `CREATE DATABASE` satırı da güncellenmelidir. | `tcmb_kurlar` |

Her iki sabit aynı anda güncellenmelidir.

### 2. TCMB Veri Kaynağı URL'i

**Dosya:** `tcmb/Services/TcmbClient.cs`

`FetchAsync` metodunun ilk satırı, çekilecek XML dosyasının URL'ini üretir:

```csharp
var url = $"https://www.tcmb.gov.tr/kurlar/{tarih:yyyyMM}/{tarih:ddMMyyyy}.xml";
```

TCMB URL kalıbı `YYYYMM/DDMMYYYY.xml` şeklindedir. Bu kalıp değiştirilmemelidir; aksi halde XML formatı uyumsuz hale gelir. Proxy üzerinden gidiliyorsa `HttpClient`'a `HttpClientHandler` ile proxy ataması yapılabilir (aynı dosyadaki `CreateHttpClient` metodu).

### 3. Veritabanına Yazılacak Yıl Aralığı

**Dosya:** `tcmb/Services/KurRepository.cs`, `Get2025MissingDatesAsync` metodu

```csharp
var start    = new DateTime(2025, 1, 1);
var endLimit = new DateTime(2025, 12, 31);
```

Farklı yıllar için önbellekleme istenirse bu iki tarih güncellenir. Aynı zamanda `Form1.cs` içindeki yıl yönlendirme koşulu da güncellenmelidir:

```csharp
if (tarih.Year == 2025) { ... }
```

### 4. Paralel İndirme Limiti

**Dosya:** `tcmb/Form1.cs`, `Fill2025InBackgroundAsync` metodu

```csharp
using var sem = new SemaphoreSlim(4);
```

Aynı anda TCMB sunucusuna gidecek istek sayısıdır. 4-8 arası önerilir. Yüksek değerler hem TCMB'ye yük bindirir hem de geçici hatalara neden olabilir.

## Çalışma Mantığı

1. **Uygulama açılışı (`Form1_Load`)**
   - ListBox 22 döviz kodu ile doldurulur.
   - `EnsureSchemaAsync` çağrısı `tcmb_kurlar` veritabanını ve tabloları (yoksa) oluşturur.
   - Arka plan görevi başlatılır: `Get2025MissingDatesAsync` ile henüz çekilmemiş 2025 günleri belirlenir, 4 paralel iş parçacığı ile TCMB'den XML çekilir, sonuç veritabanına yazılır.
   - StatusStrip üzerinde "2025 verisi çekiliyor: N/365" ilerlemesi gösterilir.

2. **Kullanıcı sorgusu (`btnGoster_Click`)**
   - Seçilen tarihin yılı 2025 ise: önce veritabanından sorgulanır. Veritabanında yoksa fakat tatil olarak işaretlenmişse uyarı gösterilir; hiç bilgi yoksa anlık olarak TCMB'den çekilip veritabanına yazılır.
   - Seçilen yıl 2025 dışındaysa: doğrudan TCMB'den anlık çekim yapılır.
   - 404 yanıtı alınırsa (hafta sonu, resmi tatil) kullanıcıya "O gün için kur açıklanmamış" mesajı gösterilir.

3. **Veri formatı**
   - TCMB XML değerleri `CultureInfo.InvariantCulture` ile parse edilir (ondalık ayracı nokta).
   - Boş elemanlar (XDR için BanknoteBuying/Selling vb.) `NULL` olarak veritabanına yazılır.
   - JPY ve KRW için `Birim = 100` olarak yayınlanır; arayüzde bu değer ayrı sütunda gösterilir.

## Veritabanı Şeması

`EnsureSchemaAsync` aşağıdaki şemayı oluşturur:

```sql
CREATE DATABASE IF NOT EXISTS tcmb_kurlar
  CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

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
);

CREATE TABLE IF NOT EXISTS yoklanan_gunler (
  Tarih    DATE    PRIMARY KEY,
  VeriVar  TINYINT NOT NULL
);
```

`yoklanan_gunler` tablosu, daha önce TCMB'ye sorgu gönderilmiş günleri tutar. `VeriVar = 1` veri yazıldı, `VeriVar = 0` o gün için TCMB 404 döndü anlamına gelir. Bu sayede tatil günleri için tekrar tekrar istek gönderilmez.

## Proje Yapısı

```
tcmb/
  tcmb.csproj                .NET 10 WinForms proje dosyası
  Program.cs                 Uygulama giriş noktası
  Form1.cs                   Olay yöneticileri ve orkestrasyon
  Form1.Designer.cs          Tasarım kodu (UI bileşenleri)
  Models/
    CurrencyRate.cs          Kur kaydı POCO (record)
  Services/
    TcmbClient.cs            TCMB XML indirme ve parse
    KurRepository.cs         MySQL erişim katmanı
```

## Derleme ve Çalıştırma

```bash
dotnet build tcmb/tcmb.csproj
dotnet run --project tcmb/tcmb.csproj
```

Veya Visual Studio üzerinden `tcmb.sln` açılıp F5 ile çalıştırılabilir.

## Güvenlik Notu

Mevcut sürümde MySQL kullanıcı adı ve şifresi kaynak kodda sabit (hardcoded) olarak tanımlıdır. Üretim ortamı veya kamuya açık depo kullanımı için bu değerlerin `appsettings.json`, ortam değişkenleri (`Environment.GetEnvironmentVariable`) veya Windows Credential Manager gibi bir yapılandırma kaynağına taşınması önerilir.

## Sürüm Geçmişi

### v2.0 — Arayüz İyileştirmesi

Uygulama işlevsel olarak değişmedi; arayüz baştan tasarlandı.

- TCMB kurum tonunda (#1A3A5C lacivert) üst banner ve uygulama başlığı eklendi.
- Segoe UI tipografi ailesi uygulandı; başlıklar, etiketler ve veri alanları için tutarlı yazı tipi hiyerarşisi oluşturuldu.
- Sol panel "Sorgu Parametreleri" ve sağ panel "Kur Bilgisi" olarak görsel olarak ayrıldı; her iki panel beyaz arkaplan, ince çerçeve ve iç boşluklarla kart görünümüne kavuştu.
- DataGridView yeniden biçimlendirildi: lacivert başlık satırı, beyaz başlık yazısı, satır arası alternatif zebra renk, gizli kılavuz çizgileri, satır yüksekliği 36 piksel.
- Sayısal sütunlar (Döviz Alış, Döviz Satış, Efektif Alış, Efektif Satış) sağa hizalandı ve `tr-TR` kültürü ile binlik ayraçlı 4 ondalık formatla gösteriliyor.
- "Kuru Göster" butonu flat stil, lacivert dolgu ve hover renk değişimiyle yeniden tasarlandı.
- ListBox öğeleri artık `USD — ABD DOLARI` biçiminde döviz kodu ve Türkçe ad ile birlikte gösteriliyor.
- Filtre paneli `TableLayoutPanel` üzerine taşındı; pencere yeniden boyutlandırıldığında ListBox dikey olarak genişler, "Kuru Göster" butonu her durumda panelin altında konumlanmış kalır.
- Form yeniden boyutlandırılabilir hale getirildi; minimum boyut 900x600 olarak sabitlendi.

### v1.0 — İlk Sürüm

- TCMB XML kur dosyalarından veri çekme.
- MySQL üzerinde 2025 verisi önbellekleme.
- Tarih ve döviz koduna göre Döviz Alış / Döviz Satış / Efektif Alış / Efektif Satış görüntüleme.
- Hafta sonu ve resmi tatil günleri için bilgilendirme.

## Lisans

Bu uygulama eğitim amaçlı geliştirilmiştir. TCMB tarafından yayınlanan kur verileri kamuya açıktır; veri kullanımına ilişkin TCMB'nin yayın koşulları geçerlidir.
