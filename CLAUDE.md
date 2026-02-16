# Claude Code Talimatlari

Bu dosya, Claude Code'un bu projede calismasi icin gereken talimatlari icerir.

## !!! EN KRITIK KURAL - ASLA UNUTMA !!!

### SEN (CLAUDE) SORU SORDUYSAN CEVAP BEKLE
```
YANLIS:
1. "Geri mi alayim?" diye sor
2. Cevap beklemeden geri al
3. Kullanici sinirlensin

YANLIS:
1. "Bu UI'i da ekleyeyim mi?" diye sor
2. Cevap beklemeden eklemeye basla
3. Kullanici sinirlensin

DOGRU:
1. Soru sor ("Geri mi alayim?", "Ekleyeyim mi?", "Devam edeyim mi?")
2. MESAJI GONDER VE DUR
3. KULLANICININ CEVABINI BEKLE
4. Cevap gelince ona gore hareket et

NOT: Soru sorduktan sonra AYNI MESAJDA islem yapma!
Soru ayri mesaj, islem ayri mesaj olmali.
```

### "BITTI" DEME - EMIN OLANA KADAR
```
YANLIS:
- "Code alanlarini kaldirdim" (kontrol etmeden)
- "Tamamlandi" (yarim kalmis olabilir)
- "Duzeltildi" (test etmeden)

DOGRU:
- "Code alanlarini kaldirdim, ama hepsini kontrol edemedim"
- "Su dosyalari guncelledim: X, Y, Z. Baska var mi bilmiyorum"
- "Build basarili ama runtime test etmedim"
```

### KULLANICIYI TATMIN ETMIS GIBI DAVRANMA
- Kullanici tatmin oldugunda KENDISI baska is soyler
- "Tamam mi?" diye sorma - kullanici kontrol edecek
- Yarim is icin "bitti" deme - durumu acikca soyle

### TEHLIKELI KOMUTLAR ICIN MUTLAKA ONAY AL
```
ASLA SORMADAN CALISTIRMA:
- git checkout -- .
- git reset --hard
- rm -rf / del /s
- DROP TABLE
- Herhangi bir geri alinamaz islem
```

### YENI KURAL OLUSTUGUNDA BURAYA YAZ
- Kullanici ile calısırken yeni bir kural/pattern olusursa CLAUDE.md'ye ekle
- Bir hata yapip duzeltildiyse, o hatayi tekrarlamamak icin kural yaz
- "Bundan sonra soyle yap" denildiyse, o kurali buraya ekle

### ONCE KODA BAK, DOKUMANTASYONA DEGIL
```
YANLIS:
1. XML/MD oku
2. Oradaki bilgiye goven
3. Yanlis bilgiyle islem yap

DOGRU:
1. Gercek koda bak (entity, controller, service, dbcontext)
2. Kodu anla
3. Islem yap
4. Gerekirse dokumantasyonu KODDAN bakarak guncelle
```

### DOKUMANTASYONA UYDURMA BILGI YAZMA
- Tablo/entity yapisi yazacaksan ONCE ilgili .cs dosyasini oku
- Field isimleri, tipler, iliskiler - hepsi KODDAN alinmali
- Hic bir seyi "hatirlayarak" veya "tahmin ederek" yazma

---

## !!! KRITIK - ONCE OKU !!!

**DEVELOPMENT_PATTERNS.md** dosyasini MUTLAKA oku! Tum frontend ve backend pattern'leri orada detayli aciklanmis.

## Proje Hakkinda
- **Proje**: Bridgo B2B Multi-Tenant Platform
- **Framework**: .NET 9 + PostgreSQL + KnockoutJS
- **Durum Takibi**: ClaudeManager (Proje ID: 16, API: http://127.0.0.1:41847)
- **Pattern Dosyasi**: `DEVELOPMENT_PATTERNS.md` (MUTLAKA OKU!)
- **Arsiv**: `PROJECT_STATUS.xml` (artik sadece arsiv, guncelleme YAPMA)

## Onemli Kurallar

### 1. Gorev Takibi (ClaudeManager)
- `PROJECT_STATUS.xml` ARTIK ARSIV - guncelleme YAPMA
- Tum gorev takibi ClaudeManager uzerinden yapilir
- Is tamamlandiginda: `curl -s -X PUT "http://127.0.0.1:41847/api/tasks/{id}" -H "Content-Type: application/json" -d '{"status":"completed"}'`
- Yeni gorev eklerken: once faz bul/olustur, sonra gorev ekle
- Yeni kural/hata ogrenildiginde pattern olarak kaydet

### 2. Kod Standartlari
- ID'ler **int** (GUID kullanilmaz)
- Controller'da DbContext kullanilmaz - Service layer kullan
- Her modul tek `Index.cshtml` + tek `Index.js`
- CDN kullanilmaz (offline uyumlu)
- Native `confirm()` kullanilmaz - Bootstrap modal kullan

### 3. Frontend Pattern'leri (KnockoutJS)

#### Liste/Tablo kurallari
- **Liste varsa CRUD butonlari SART**: Duzenle (pencil), Sil (trash)
- Context'e gore ozel butonlar eklenir (Dogrula, Onayla, vb.)
- Tabloda islem butonlari son sutunda, `text-end` ile saga yasli

#### Onay diyaloglari
```javascript
// YANLIS - Native confirm KULLANILMAZ
if (!confirm('Emin misiniz?')) return;

// DOGRU - Silme icin
showDeleteConfirm(item.name, function() {
    // silme islemi
});

// DOGRU - Ozel onay icin
showConfirmModal({
    title: 'Baslik',
    message: 'Mesaj',
    type: 'success', // success, danger, warning, info
    confirmText: 'Onayla',
    confirmIcon: 'bi bi-check',
    onConfirm: function() { /* islem */ }
});
```

#### Bildirimler
```javascript
toastr.success('Basarili mesaj');
toastr.error('Hata mesaji');
toastr.warning('Uyari mesaji');
toastr.info('Bilgi mesaji');
```

#### Modal icindeki butonlar
- Foreach icerisindeyse `$parent.fonksiyonAdi` kullan
- Modal, Knockout binding scope'u icinde olmali (`#app-id` div'inin icinde)

#### Kayit sonrasi yenileme
```javascript
// YANLIS - Tum sayfayi yeniler, tab state bozulur
self.loadAllData();

// DOGRU - Sadece ilgili listeyi yenile
self.loadSpecificList();
```

#### Status/Type alanlari
- Enum KULLANILMAZ
- TypeDefinitions.cs'de TypeItem pattern kullanilir
- API'den dinamik yuklenir
- JSON camelCase donduruyor: `id`, `name`, `cssClass` (PascalCase DEGIL)

```javascript
// DOGRU
optionsValue: 'id', optionsText: 'name'
memberType.cssClass

// YANLIS
optionsValue: 'Id', optionsText: 'Name'
memberType.CssClass
```

### 4. API Controller Pattern'leri

#### Route tanimlama
```csharp
// DOGRU - Acik route
[Route("api/team")]
public class TeamApiController : ControllerBase

// YANLIS - [controller] placeholder beklenmedik sonuc verir
[Route("api/[controller]")]  // Bu "api/TeamApi" olur, "api/team" degil!
```

#### Standart CRUD endpoint'leri
```
GET    /api/resource          - Liste
GET    /api/resource/{id}     - Tek kayit
POST   /api/resource          - Ekle
PUT    /api/resource/{id}     - Guncelle
DELETE /api/resource/{id}     - Sil
POST   /api/resource/{id}/action - Ozel islem (verify, approve, vb.)
```

### 3. Mimari
- **Pattern**: SPA Modal + Repository/Service Pattern
- **Rol Sistemi**: VendorUserRole (Owner, Admin, Manager, Employee) + RBAC (UserCompanyRole)
- **Multi-tenant**: VendorId ile veri izolasyonu

### 4. Klasor Yapisi
```
Controllers/          - MVC Controller'lar
Controllers/Api/      - API Controller'lar
Services/            - Business logic (Interface + Implementation)
Views/Dashboard/     - Dashboard layout ve sayfalari
Views/Company/       - Firma bilgileri sayfalari
wwwroot/js/          - JavaScript (modul bazli)
```

### 5. Dashboard Layout
Tum firma icin sayfalar `~/Views/Dashboard/_DashboardLayout.cshtml` kullanir.
Menu rollere gore filtrelenir (isOwnerOrAdmin, isManagerOrAbove).

### 6. PostgreSQL Baglantisi (psql)
```bash
# psql yolu
"/c/Program Files/PostgreSQL/17/bin/psql.exe"

# Ornek kullanim (sifre ile)
PGPASSWORD='1123Azs+-' "/c/Program Files/PostgreSQL/17/bin/psql.exe" -h localhost -U postgres -d BridgoDb -c "SELECT * FROM \"Tablo\";"

# Coklu satir SQL
PGPASSWORD='1123Azs+-' "/c/Program Files/PostgreSQL/17/bin/psql.exe" -h localhost -U postgres -d BridgoDb << 'EOF'
SELECT * FROM "PlatformModules";
EOF
```

### 7. Gelistirme Ortami ve Calistirma
- **IDE**: Visual Studio (HTTPS debug modu)
- **Port**: https://localhost:7083 (HTTPS), http://localhost:5279 (HTTP)
- **Debug**: Kullanici Visual Studio'dan HTTPS ile debug eder
- **ASLA `dotnet run` ile projeyi baslatma** - kullanici VS'den debug ediyor

### 7. Migration Islemleri
Migration gerektiginde SADECE migration'i calistir, projeyi baslatma:
```bash
dotnet ef database update
```
Projeyi baslatmak kullanicinin isi - Visual Studio'dan HTTPS ile debug eder.

### 8. HTTPS Sertifika Sorunu Olursa
```bash
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

### 9. Localization
- Ceviriler `App_Data/Localization/resources.tr.xml` ve `resources.en.xml` dosyalarinda
- Veritabaninda degil, XML'de tutulur
- T("Key", "Default") helper'i ile kullanilir

## Veritabani Semalari

### RBAC Sistemi
```
PlatformModules (Id, Name, DisplayName, DisplayNameResourceKey, Route, Icon, ParentId, IsMenuSection, IsActive)
  - Tum moduller burada tanimli (tek master liste)
  - ParentId: Ust modul (menu section icin)
  - IsMenuSection: true ise menu basligi, false ise sayfa
  - RbacSeeder.cs ile seed edilir

CapabilityModuleMappings (Id, CapabilityId, PlatformModuleId, IsDeleted)
  - Hangi capability hangi modulleri gorebilir
  - Sidebar'da capability bazli menu section'lari gostermek icin DOLDURULMALI
  - _DashboardLayout.cshtml modulesByCapability ile okur
  - Admin Panel'den veya SQL ile eklenir

Capabilities (TypeDefinitions.cs - TABLO DEGIL!)
  - Seller(2), Buyer(3), Carrier(4), Insurance(5), Customs(6), Survey(7), Investor(8)
  - Capabilities.Ids.Seller, Capabilities.GetById(id) seklinde kullanilir

CompanyRoles (Id, Name, NameResourceKey, Description, CapabilityId, IsDefault, IsSystem)
  - Firma icindeki roller (Account Manager, Order Staff, vb.)

CompanyRoleModulePermissions (Id, CompanyRoleId, PlatformModuleId, CanView, CanCreate, CanEdit, CanDelete)
  - Rol bazli modul izinleri
  - FK: CompanyRoles.Id, PlatformModules.Id

CompanyRoleUserMappings (Id, UserId, CompanyRoleId, VendorId)
  - Kullaniciya atanan roller
```

### Capability-Module Mapping Ornekleri
```
Seller (2): Products, Categories, Stock, Warehouses, SellerOrders, SupplierOffers, SupplierProfile
Buyer (3): MyOrders, MyDemands, DiscoverSuppliers, FavoriteSuppliers, Proposals
Carrier (4): Logistics Requests, MyLogisticsJobs
Insurance (5): Insurance Requests, MyInsuranceJobs
Customs (6): Customs Requests, MyCustomsJobs
Survey (7): Survey Requests, MySurveyJobs
Investor (8): InvestmentOpportunities, MyInvestments
```

### Kullanici ve Firma
```
Users (ApplicationUser - IdentityUser'dan turetilir)
  - Id, Email, UserName (Identity'den)
  - FirstName, LastName, ProfileImageUrl
  - IsActive, CreatedAt, LastLoginAt
  - LanguageId, VendorId, IsSystemAdmin
  - FullName, HasVendor (computed)

Vendors (Id, CompanyName, Email, Phone, TaxNumber, VendorStatusId, IsProfileComplete, IsVerified)
  - Multi-tenant root entity

VendorTeamMembers (Id, VendorId, UserId, Email, TeamMemberStatusId, InvitedByUserId)
  - Firma uyeleri ve davetler

VendorCapabilityMappings (Id, VendorId, CapabilityId)
  - Firmanin sahip oldugu capability'ler
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
  - Status: Draft(0), Pending(1), Active(2), Closed(3), Cancelled(4), Expired(5)

DemandResponses (Id, DemandId, VendorId, UnitPrice, TotalPrice, Currency, Status)
  - Talebe gelen teklifler

CategorySubscriptions (Id, VendorId, CategoryId, NotifyInApp, NotifyByEmail, KeywordFilter)
  - Satici kategori takibi

ProductInquiries (Id, ProductId, BuyerVendorId, SellerVendorId, Quantity, Status, IsReadBySeller)
  - Urune direkt teklif istegi

ProductInquiryResponses (Id, InquiryId, UnitPrice, Currency, OfferedQuantity, Status)
  - Urun istegine teklif
```

### Diger
```
Addresses (Id, VendorId, Title, AddressTypeId, CountryId, StateId, City, AddressLine)
Countries (Id, Name, Iso2Code, PhoneCode, CurrencyCode)
States (Id, CountryId, Name, StateCode) -- Sadece 13 ulke icin
Notifications (Id, VendorId, UserId, Type, Title, Message, IsRead, ActionUrl)
Languages (Id, Name, LanguageCulture, UniqueSeoCode, IsActive, IsDefault)
LocaleStringResources (Id, LanguageId, ResourceName, ResourceValue)
```

## ClaudeManager Entegrasyonu
Bu proje ClaudeManager ile entegredir. Her oturumda kullan:
```bash
# Oturum basinda proje rehberini oku
curl -s "http://127.0.0.1:41847/api/guide?cwd=$(pwd)"

# Pattern/kural ekle (HASSAS BILGI YAZMA - notes kullan!)
curl -s -X POST "http://127.0.0.1:41847/api/patterns" -H "Content-Type: application/json" \
  -d '{"project_id":16,"type":"rule|preference|mistake","title":"...","description":"..."}'

# Roadmap goruntule
curl -s "http://127.0.0.1:41847/api/projects/16/roadmap"
```
- **Proje ID:** 16
- **API:** http://127.0.0.1:41847
- **Pattern tipleri:** rule, preference, mistake (pattern tipi YOK!)
- **Roadmap:** Fazlar (phases) ve gorevler (tasks) ile takip edilir
- Yeni kural/hata/tercih ogrenildiginde ClaudeManager'a pattern olarak kaydet
- Gorev tamamlandiginda roadmap'i guncelle

### Notes API (Hassas/Ozel Bilgiler)
Pattern'lere **ASLA** hassas bilgi yazma! API key, sifre, TC, telefon, wallet key gibi ozel bilgiler icin **Notes** kullan:
```bash
# Not listele
curl -s "http://127.0.0.1:41847/api/projects/16/notes"

# Kategoriye gore filtrele (teknik)
curl -s "http://127.0.0.1:41847/api/projects/16/notes?category=teknik"

# Not olustur
curl -s -X POST "http://127.0.0.1:41847/api/projects/16/notes" -H "Content-Type: application/json" \
  -d '{"title":"Baslik","content":"Icerik","category":"teknik"}'

# Not guncelle
curl -s -X PUT "http://127.0.0.1:41847/api/notes/{id}" -H "Content-Type: application/json" \
  -d '{"title":"Yeni Baslik","content":"Yeni Icerik"}'

# Not sil
curl -s -X DELETE "http://127.0.0.1:41847/api/notes/{id}"
```

**Kural: Roadmap vs Journal vs Notes vs Pattern ayrimi:**
| Yer | Ne Yazilir |
|-----|-----------|
| **Roadmap** | Sadece yazilim gelistirme gorevleri (kod, modul, feature) |
| **Journal** | Gunluk isler, basvuru durumlari, pazarlama, operasyonel kayitlar |
| **Notes** | SADECE hassas bilgiler (API key, sifre, TC, wallet key, credential) |
| **Pattern** | Kod kurallari (rule), tercihler (preference), hatalar (mistake) |

**KRITIK: Roadmap'e gunluk/operasyonel is YAZMA! Journal kullan.**
**KRITIK: Notes'a gunluk is YAZMA! Notes SADECE hassas bilgiler icin.**

### Journal API (Gunluk Kayitlar)
```bash
# Journal listele
curl -s "http://127.0.0.1:41847/api/projects/16/journal"

# Journal olustur
curl -s -X POST "http://127.0.0.1:41847/api/projects/16/journal" -H "Content-Type: application/json" \
  -d '{"title":"Baslik","content":"Icerik","category":"kategori","entry_date":"2026-02-16"}'

# Kategoriler: pazarlama, gelistirme, basvuru, finans, guvenlik, altyapi, gelir, kisisel
```

**Mevcut Notes (SADECE hassas bilgiler):**
- Kisisel Bilgiler (TC, tel, adres)
- Hesap Bilgileri - Cloud & Domain (GCP, AWS, Cloudflare, Namecheap)
- Email Yonlendirme (info@corplynk.com)
- Twitter/X - Hesap ve API Keys
- Bagis Kanallari - WAX & Solana (wallet key'ler dahil)
- Moltbook API Key

## Her Oturum Baslangicinda
1. Bu dosyayi oku
2. `curl -s "http://127.0.0.1:41847/api/guide?cwd=$(pwd)"` ile ClaudeManager rehberini oku
3. Kullanicinin istegini dinle
4. Is bitince ClaudeManager roadmap'i guncelle
