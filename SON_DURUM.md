# Bridgo - Son Durum ve Yapilacaklar

> Son guncelleme: 15 Subat 2026

## EN SON TAMAMLANAN ISLER

### Waitlist API Refactoring (Task #152 - 6 Subat 2026)
CLAUDE.md pattern ihlalleri duzeltildi:
- Controller'dan DbContext kullanimi kaldirildi -> IWaitlistService kullaniliyor
- WaitlistRequest DTO -> DTOs/Waitlist/WaitlistDtos.cs'e tasindi
- IUnitOfWork + UnitOfWork'e WaitlistEntries repository eklendi
- IWaitlistService + WaitlistService olusturuldu
- Program.cs'e DI kaydi eklendi

### Google Cloud Deploy (Task #153 - 7 Subat 2026)
- Cloud Run: bridgo-app (europe-west1)
- Cloud SQL: bridgo-db (PostgreSQL 15)
- Cloudflare Worker corplynk.com -> Cloud Run proxy

### Landing Page (Task #155 - 7 Subat 2026)
- Dark tema, glassmorphism, waitlist formu
- corplynk.com uzerinden erisiliyor

---

## PROJE BILGILERI DOSYASI

Tum hesap bilgileri, sifreler, API token'lar, DNS ayarlari ve basvuru durumlari:
**C:\Users\Ahmet\Downloads\moltbook\proje-bilgileri.md**

---

## HIZLI REFERANS

### Lokal Gelistirme
- **IDE:** Visual Studio (ASLA `dotnet run` kullanma!)
- **HTTPS:** https://localhost:7083
- **HTTP:** http://localhost:5279
- **DB:** PostgreSQL localhost:5432, BridgoDb, postgres / 1123Azs+-
- **psql:** `PGPASSWORD='1123Azs+-' "/c/Program Files/PostgreSQL/17/bin/psql.exe" -h localhost -U postgres -d BridgoDb`

### Production (Google Cloud)
- **Cloud Run:** bridgo-app (europe-west1)
- **Cloud Run URL:** https://bridgo-app-267313553839.europe-west1.run.app
- **Cloud SQL:** bridgo-db (PostgreSQL 15, Bridgo2026Prod)
- **Proje ID:** project-648e1c7b-87e5-472b-bf1
- **Deploy:** `gcloud run deploy bridgo-app --source=. --region=europe-west1 --project=project-648e1c7b-87e5-472b-bf1 --quiet`
- **gcloud CLI:** "C:\Program Files (x86)\Google\Cloud SDK\google-cloud-sdk\bin\gcloud.cmd"

### DNS Yonlendirme
```
corplynk.com -> Cloudflare (Proxied) -> Worker (corplynk-proxy) -> Cloud Run (bridgo-app) -> Kestrel (8080)
```

### Hesaplar
- **Cloudflare:** corplynkcmon@gmail.com
- **GCP:** corplynkcmon@gmail.com
- **AWS:** corplynkcmon@gmail.com (Hesap: 329437500522)
- **Namecheap:** monanoyan
- **Bridgo Admin:** admin@bridgo.com / Admin123!

### Onemli Kurallar (CLAUDE.md)
1. Controller'da DbContext KULLANILMAZ - Service layer zorunlu
2. ID'ler int (auto-increment) - GUID KULLANILMAZ
3. Her entity BaseEntity'den turetilir
4. Explicit route: `[Route("api/xxx")]` - [controller] KULLANILMAZ
5. Her is sonrasi PROJECT_STATUS.xml guncelle
6. CDN KULLANILMAZ - tum kutuphaneler lokal
7. Native confirm() KULLANILMAZ - Bootstrap modal

---

## BEKLEYEN ISLER

| # | Gorev | Durum |
|---|-------|-------|
| 1 | Microsoft for Startups (yeni hesap) | BEKLEMEDE |
| 2 | AWS Activate sonucu ($1,000) | BEKLEMEDE |
| 3 | Estonya e-Residency (ortak basvuracak) | BEKLEMEDE |
| 4 | Ahmet pasaport yenileme | BEKLEMEDE |
| 5 | Landing page gelistirme | BEKLEMEDE |
