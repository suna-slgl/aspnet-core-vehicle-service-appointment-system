# Araç Servis Randevu Sistemi

[English](README.md) | Türkçe

Araç Servis Randevu Sistemi, araç servis randevularını yönetmek için geliştirilmiş bir ASP.NET Core MVC uygulamasıdır. Müşteri tarafında araç ve randevu işlemlerini, yönetici tarafında operasyonel yönetimi, ASP.NET Core Identity ile kimlik doğrulamayı, Entity Framework Core ile veri erişimini ve otomatik testleri içerir.

## Özellikler

- Kullanıcı kayıt, giriş, profil, şifre değiştirme, şifremi unuttum ve şifre sıfırlama akışları
- `Admin` ve `User` rolleriyle rol tabanlı yetkilendirme
- Plaka doğrulama, kullanıcı sahipliği, aktiflik durumu ve isteğe bağlı görsel yükleme desteğiyle araç yönetimi
- Kayıtlı kullanıcılar için randevu oluşturma ve listeleme
- Randevu durum akışı: beklemede, onaylandı, devam ediyor, tamamlandı ve iptal edildi
- Teknisyen atama ve teknisyen uygunluk kontrolleri
- Tahmini süre, fiyat, ikon, renk ve aktiflik bilgileriyle servis türü yönetimi
- Randevular, servis türleri, teknisyenler, kullanıcılar, panel ve raporlar için yönetici alanı
- Randevu, teknisyen ve servis türü istatistikleri için panel/rapor servisleri
- `tr-TR` kültürüyle Türkçe yerelleştirme yapılandırması
- Entity Framework Core migration yapısıyla SQL Server veri kalıcılığı
- Gerektiğinde EF Core InMemory kullanan xUnit test projesi

## Teknolojiler

- .NET 9
- ASP.NET Core MVC
- ASP.NET Core Identity
- Entity Framework Core 9
- SQL Server
- Bootstrap, jQuery, jQuery Validation
- xUnit
- Docker

## Proje Yapısı

```text
.
+-- src/
|   +-- VehicleServiceApp/        Ana ASP.NET Core MVC uygulaması
+-- tests/
|   +-- VehicleServiceApp.Tests/  Otomatik testler
+-- Dockerfile                    Docker derleme tanımı
+-- aspnet-core-vehicle-service-appointment-system.sln
```

## Gereksinimler

- .NET 9 SDK
- SQL Server veya SQL Server Express
- İsteğe bağlı: Docker

## Yapılandırma

Varsayılan bağlantı metni `src/VehicleServiceApp/appsettings.json` dosyasında tanımlıdır:

```json
"DefaultConnection": "Server=.\\SQLEXPRESS;Database=AracServisDb;Trusted_Connection=True;TrustServerCertificate=True;"
```

SQL Server instance, veritabanı adı veya kimlik doğrulama yöntemi farklıysa bu değeri güncelleyin.

Geliştirme yapılandırmasında uygulama başlangıcında migration uygulanması açıktır:

```json
"Database": {
  "ApplyMigrationsOnStartup": true
}
```

Diğer ortamlar için bu değer yapılandırma üzerinden yönetilebilir.

## Başlangıç

Bağımlılıkları geri yükleyin:

```bash
dotnet restore
```

Veritabanı migrationlarını uygulayın:

```bash
dotnet ef database update --project src/VehicleServiceApp
```

Uygulamayı çalıştırın:

```bash
dotnet run --project src/VehicleServiceApp
```

Geliştirme launch ayarlarında kullanılan adresler:

- `http://localhost:5189`
- `https://localhost:7123`

## Testler

Test paketini depo kök dizininden çalıştırın:

```bash
dotnet test
```

## Docker

İmajı oluşturun:

```bash
docker build -t vehicle-service-appointment-system .
```

Containerı çalıştırın:

```bash
docker run -p 8080:80 vehicle-service-appointment-system
```

Container içinde çalıştırırken uygulamanın yapılandırılmış SQL Server instanceına erişebildiğinden emin olun.

## Notlar

- Uygulama, servis türlerini ve teknisyenleri Entity Framework model yapılandırması üzerinden seed eder.
- Uygulama başlangıcında `Admin` ve `User` rolleri oluşturulur.
- Admin ve demo kullanıcıları yalnızca ilgili `SeedUsers` yapılandırma değerleri sağlandığında seed edilir.
- Randevu kayıtlarında aktiflik filtresiyle soft-delete benzeri yaklaşım kullanılır.

## Lisans

Bu proje [LICENSE](LICENSE) dosyasında yer alan lisans koşullarıyla lisanslanmıştır.
