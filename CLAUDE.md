# Claude Code Talimatlari

## Proje Hakkinda
- **Proje**: Bridgo B2B Multi-Tenant Platform
- **Framework**: .NET 9 + PostgreSQL + KnockoutJS
- **Pattern Dosyasi**: `DEVELOPMENT_PATTERNS.md` (detayli frontend/backend pattern'leri)
- **Arsiv**: `PROJECT_STATUS.xml` (artik sadece arsiv, guncelleme YAPMA)

## Kurallar ve Pattern'ler
**Tum kurallar, tercihler ve hatalar ClaudeManager'da tutulur.**
CLAUDE.md'ye kural YAZMA - ClaudeManager'a pattern olarak kaydet.

## ClaudeManager Entegrasyonu
- **Proje ID:** 16
- **API:** http://127.0.0.1:41847
- **Pattern tipleri:** rule, preference, mistake

```bash
# Oturum basinda proje rehberini oku
curl -s "http://127.0.0.1:41847/api/guide?cwd=$(pwd)"

# Pattern/kural ekle (HASSAS BILGI YAZMA - notes kullan!)
curl -s -X POST "http://127.0.0.1:41847/api/patterns" -H "Content-Type: application/json" \
  -d '{"project_id":16,"type":"rule|preference|mistake","title":"...","description":"..."}'

# Roadmap goruntule
curl -s "http://127.0.0.1:41847/api/projects/16/roadmap"
```

### Notes API (Hassas/Ozel Bilgiler)
Pattern'lere **ASLA** hassas bilgi yazma! API key, sifre, TC, telefon, wallet key gibi ozel bilgiler icin **Notes** kullan:
```bash
curl -s "http://127.0.0.1:41847/api/projects/16/notes"
curl -s -X POST "http://127.0.0.1:41847/api/projects/16/notes" -H "Content-Type: application/json" \
  -d '{"title":"Baslik","content":"Icerik","category":"teknik"}'
```

### Journal API (Gunluk Kayitlar)
```bash
curl -s "http://127.0.0.1:41847/api/projects/16/journal"
curl -s -X POST "http://127.0.0.1:41847/api/projects/16/journal" -H "Content-Type: application/json" \
  -d '{"title":"Baslik","content":"Icerik","category":"kategori","entry_date":"2026-02-20"}'
# Kategoriler: pazarlama, gelistirme, basvuru, finans, guvenlik, altyapi, gelir, kisisel
```

| Yer | Ne Yazilir |
|-----|-----------|
| **Roadmap** | Sadece yazilim gelistirme gorevleri (kod, modul, feature) |
| **Journal** | Gunluk isler, basvuru durumlari, pazarlama, operasyonel kayitlar |
| **Notes** | SADECE hassas bilgiler (API key, sifre, TC, wallet key, credential) |
| **Pattern** | Kod kurallari (rule), tercihler (preference), hatalar (mistake) |

## Teknik Referanslar

### PostgreSQL Baglantisi
```bash
PGPASSWORD='1123Azs+-' "/c/Program Files/PostgreSQL/17/bin/psql.exe" -h localhost -U postgres -d BridgoDb -c "SELECT * FROM \"Tablo\";"
```

### Gelistirme Ortami
- **IDE**: Visual Studio (HTTPS debug modu)
- **Port**: https://localhost:7083 (HTTPS), http://localhost:5279 (HTTP)
- **HTTPS sertifika sorunu**: `dotnet dev-certs https --clean && dotnet dev-certs https --trust`

### Klasor Yapisi
```
Controllers/          - MVC Controller'lar
Controllers/Api/      - API Controller'lar
Services/            - Business logic (Interface + Implementation)
Views/Dashboard/     - Dashboard layout ve sayfalari
Views/Company/       - Firma bilgileri sayfalari
wwwroot/js/          - JavaScript (modul bazli)
```

## Veritabani Semalari

### RBAC Sistemi
```
PlatformModules (Id, Name, DisplayName, DisplayNameResourceKey, Route, Icon, ParentId, IsMenuSection, IsActive)
CapabilityModuleMappings (Id, CapabilityId, PlatformModuleId)
Capabilities (TypeDefinitions.cs): Seller(2), Buyer(3), Carrier(4), Insurance(5), Customs(6), Survey(7), Investor(8)
CompanyRoles (Id, Name, NameResourceKey, Description, CapabilityId, IsDefault, IsSystem)
CompanyRoleModulePermissions (Id, CompanyRoleId, PlatformModuleId, CanView, CanCreate, CanEdit, CanDelete)
CompanyRoleUserMappings (Id, UserId, CompanyRoleId, VendorId)
```

### Kullanici ve Firma
```
Users (ApplicationUser - IdentityUser): Id, Email, FirstName, LastName, VendorId, IsSystemAdmin
Vendors (Id, CompanyName, Email, Phone, TaxNumber, VendorStatusId, IsProfileComplete, IsVerified)
VendorTeamMembers (Id, VendorId, UserId, Email, TeamMemberStatusId, InvitedByUserId)
VendorCapabilityMappings (Id, VendorId, CapabilityId)
```

### Urun ve Katalog
```
Products (Id, VendorId, Name, SKU, Slug, CategoryId, Price, Currency, StockQuantity, ProductStatusId)
ProductCategories (Id, Name, Slug, ParentId, Level, Icon, ImageUrl) -- Global, VendorId YOK
ProductImages (Id, ProductId, Url, IsMain, DisplayOrder)
ProductPriceTiers (Id, ProductId, MinQuantity, MaxQuantity, Price)
ProductWarehouseStock (Id, ProductId, WarehouseId, Quantity, ReservedQuantity)
Warehouses (Id, VendorId, Name, WarehouseTypeId, AddressId, IsDefault)
```

### Talep ve Teklif
```
PublicDemands (Id, VendorId, Title, Slug, Description, CategoryId, Quantity, Status, PublishedAt)
DemandResponses (Id, DemandId, VendorId, UnitPrice, TotalPrice, Currency, Status)
CategorySubscriptions (Id, VendorId, CategoryId, NotifyInApp, NotifyByEmail, KeywordFilter)
ProductInquiries (Id, ProductId, BuyerVendorId, SellerVendorId, Quantity, Status, IsReadBySeller)
ProductInquiryResponses (Id, InquiryId, UnitPrice, Currency, OfferedQuantity, Status)
```

### Diger
```
Addresses (Id, VendorId, Title, AddressTypeId, CountryId, StateId, City, AddressLine)
Countries (Id, Name, Iso2Code, PhoneCode, CurrencyCode)
States (Id, CountryId, Name, StateCode)
Notifications (Id, VendorId, UserId, Type, Title, Message, IsRead, ActionUrl)
Languages (Id, Name, LanguageCulture, UniqueSeoCode, IsActive, IsDefault)
```

## Her Oturum Baslangicinda
1. Bu dosyayi oku
2. `curl -s "http://127.0.0.1:41847/api/guide?cwd=$(pwd)"` ile ClaudeManager rehberini oku
3. Kullanicinin istegini dinle
4. Is bitince ClaudeManager roadmap'i guncelle
