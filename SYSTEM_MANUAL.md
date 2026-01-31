# Bridgo B2B Platform - Sistem Manueli

Bu dokuman, Bridgo B2B platformunun tum modullerini, ozelliklerini ve kullanim rehberini icerir.

---

## Genel Bakis

**Bridgo**, isletmeler arasi (B2B) ticaret icin gelistirilmis cok kiracili (multi-tenant) bir e-ticaret platformudur.

- **Framework**: .NET 9
- **Veritabani**: PostgreSQL
- **Frontend**: KnockoutJS + Bootstrap 5
- **Mimari**: MVC + SPA Modal Pattern

---

## Kullanici Rolleri

### Platform Rolleri (VendorUserRole)
| Rol | Yetki Seviyesi | Aciklama |
|-----|----------------|----------|
| Owner | En yuksek | Firma sahibi, tum yetkilere sahip |
| Admin | Yuksek | Firma yoneticisi |
| Supervisor | Orta-Yuksek | Denetleyici |
| Manager | Orta | Mudur |
| Employee | Dusuk | Calisan |

### Firma Yetenekleri (Capabilities)
| ID | Yetenek | Aciklama |
|----|---------|----------|
| 1 | Platform | Platform yonetimi |
| 2 | Satici (Seller) | Urun satisi yapabilir |
| 3 | Alici (Buyer) | Urun satin alabilir |
| 4 | Tasimaci | Lojistik hizmeti verir |
| 5 | Sigorta | Sigorta hizmeti verir |
| 6 | Gumruk | Gumrukleme hizmeti verir |

---

## Moduller

### 1. KATALOG (Catalog)

**Amac**: Platformdaki tum urunlerin sergilenmesi ve aranmasi.

**Sayfalar**:
- `/Catalog` - Ana katalog sayfasi
- `/Catalog/{slug}` - Urun detay sayfasi
- `/Catalog/Category/{slug}` - Kategori sayfasi
- `/Catalog/Vendor/{slug}` - Satici urunleri

**Ozellikler**:
- Urun arama (isim, SKU, marka)
- Kategori filtreleme (hiyerarsik agac yapisi)
- Ozellik filtreleme (Faceted Search) - Marka, Renk, Malzeme vb.
- Fiyat araligi filtreleme
- Stok durumu filtreleme
- Siralama (en yeni, fiyat, populer)
- Grid/Liste gorunumu
- Sayfalama

**API Endpoint'leri**:
```
GET /api/catalog/products - Urun listesi
GET /api/catalog/products/{slug} - Urun detayi
GET /api/catalog/categories - Kategori agaci
GET /api/catalog/stats - Istatistikler
GET /api/catalog/search/suggestions - Arama onerileri
```

---

### 2. URUNLER (Products)

**Amac**: Saticilarin kendi urunlerini yonetmesi.

**Sayfalar**:
- `/Products` - Urun listesi (satici gorunumu)
- `/Products/Create` - Yeni urun ekleme
- `/Products/Edit/{id}` - Urun duzenleme

**Ozellikler**:
- Urun CRUD islemleri
- Coklu gorsel yukleme
- Fiyat kademeleri (toptan fiyatlandirma)
- Paketleme secenekleri (GS1 standardinda)
- Stok takibi
- Kategori atama
- SEO ayarlari (slug, meta)
- Urun durumu (Taslak, Aktif, Pasif)

**Urun Ozellikleri (Attributes)**:
- Global ozellikler (tum kategorilerde)
- Kategori bazli ozellikler
- Ozellik tipleri: Select, MultiSelect, Text, Number, Boolean

---

### 3. SEPET (Cart)

**Amac**: Alicilarin urunleri sepete eklemesi ve yonetmesi.

**Sayfalar**:
- `/Cart` - Sepet sayfasi

**Ozellikler**:
- Urun ekleme/cikarma
- Miktar guncelleme
- Birim secimi (adet, koli, palet vb.)
- Satici bazli gruplama
- Fiyat hesaplama (kademe indirimleri dahil)
- Sepet ozeti

---

### 4. ODEME (Checkout)

**Amac**: Siparis olusturma sureci.

**Sayfalar**:
- `/Checkout` - Odeme sayfasi (3 adimli)

**Adimlar**:
1. Teslimat Adresi Secimi
2. Teslimat Yontemi + Incoterms
3. Odeme ve Onay

**Ozellikler**:
- Adres yonetimi
- Incoterms secimi (EXW, FOB, CIF vb.)
- Siparis ozeti
- Satici bazli siparis ayirma

---

### 5. SIPARISLER (Orders)

**Amac**: Siparis takibi ve yonetimi.

**Sayfalar**:
- `/MyOrders` - Siparislerim (Alici)
- `/Orders` - Gelen Siparisler (Satici)

**Ozellikler**:
- Siparis listesi
- Siparis detayi
- Durum takibi
- Sevkiyat bilgileri
- Fatura bilgileri

**Siparis Durumlari**:
| Durum | Aciklama |
|-------|----------|
| Pending | Beklemede |
| Confirmed | Onaylandi |
| Processing | Hazirlaniyor |
| Shipped | Kargoya verildi |
| Delivered | Teslim edildi |
| Cancelled | Iptal edildi |

---

### 6. TALEPLER (Demands)

**Amac**: Alicilarin toplu urun talepleri olusturmasi.

**Sayfalar**:
- `/Demands/MyDemands` - Taleplerim
- `/Demands/PublicDemands` - Acik Talepler (Saticilar icin)

**Ozellikler**:
- Talep olusturma
- Kategori/urun bazli talep
- Miktar ve sure belirtme
- Teklif alma
- Teklifleri karsilastirma

**Talep Durumlari**:
| Durum | Aciklama |
|-------|----------|
| Draft | Taslak |
| Pending | Onay bekliyor |
| Active | Aktif, teklif aliyor |
| Closed | Kapandi |
| Cancelled | Iptal edildi |
| Expired | Suresi doldu |

---

### 7. URUN SORGULARI (Product Inquiries)

**Amac**: Belirli bir urun icin fiyat/stok sorgusu.

**Sayfalar**:
- `/Inquiries/Incoming` - Gelen Sorular (Satici)
- `/Inquiries/Outgoing` - Gonderdiklerim (Alici)

**Ozellikler**:
- Urune ozel sorgulama
- Miktar belirtme
- Teslimat adresi
- Teklif alma/gonderme

---

### 8. TEKLIFLER (Proposals)

**Amac**: Saticilardan alicilara teklif gonderimi.

**Sayfalar**:
- `/Proposals` - Teklif yonetimi

**Ozellikler**:
- Teklif olusturma
- Fiyat ve kosullar belirleme
- Gecerlilik suresi
- Teklif durumu takibi

---

### 9. SOZLESMELER (Contracts)

**Amac**: Ticari sozlesmelerin yonetimi.

**Sayfalar**:
- `/Contracts` - Sozlesme listesi

**Ozellikler**:
- Sozlesme olusturma
- Taraf bilgileri
- Kosullar ve maddeler
- Imza sureci
- Durum takibi

---

### 10. STOK YONETIMI (Stock)

**Amac**: Depo ve stok yonetimi.

**Sayfalar**:
- `/Warehouses` - Depo listesi
- `/Stock/StockMovements` - Stok hareketleri

**Ozellikler**:
- Depo CRUD
- Stok giris/cikis
- Depolar arasi transfer
- Stok duzeltme
- Hareket gecmisi

**Hareket Tipleri**:
| Tip | Aciklama |
|-----|----------|
| StockIn | Stok girisi |
| StockOut | Stok cikisi |
| Transfer | Transfer |
| Adjustment | Duzeltme |

---

### 11. FINANSMAN (Financing)

**Amac**: Ticari finansman ve yatirim yonetimi.

**Sayfalar**:
- `/FinancingRequests` - Finansman Taleplerim
- `/InvestmentOpportunities` - Yatirim Firsatlari
- `/MyInvestments` - Yatirimlarim

**Ozellikler**:
- Finansman talebi olusturma
- Teminat tipleri
- Faiz orani araligi
- Yatirimci teklifleri
- ROI hesaplama

---

### 12. EKIP YONETIMI (Team)

**Amac**: Firma calisanlarinin yonetimi.

**Sayfalar**:
- `/Team` - Ekip listesi

**Ozellikler**:
- Uye davet etme (email ile)
- Rol atama
- Yetki yonetimi (RBAC)
- Uye durumu (Aktif, Beklemede, Pasif)

---

### 13. FIRMA BILGILERI (Company)

**Amac**: Firma profili yonetimi.

**Sayfalar**:
- `/Company/Profile` - Firma profili
- `/Company/Settings` - Ayarlar

**Ozellikler**:
- Firma bilgileri (isim, vergi no, adres)
- Logo yukleme
- Iletisim bilgileri
- Banka bilgileri

---

### 14. TEDARIKCI PROFILI (SupplierProfile)

**Amac**: Saticilarin herkese acik profili.

**Sayfalar**:
- `/SupplierManagement/SupplierProfile` - Profil duzenleme
- `/Supplier/{slug}` - Herkese acik profil

**Ozellikler**:
- Firma tanitimi
- Urun portfoyu
- Sertifikalar
- Referanslar
- Iletisim formu

---

### 15. KATEGORI ABONELIKLERI (Subscriptions)

**Amac**: Saticilarin belirli kategorileri takip etmesi.

**Sayfalar**:
- `/CategorySubscriptions` - Abonelikler

**Ozellikler**:
- Kategori takibi
- Yeni talep bildirimleri
- Email/uygulama icin bildirim tercihleri
- Anahtar kelime filtreleme

---

### 16. BILDIRIMLER (Notifications)

**Amac**: Sistem bildirimleri.

**Ozellikler**:
- Uygulama ici bildirimler
- Email bildirimleri
- Bildirim tipleri (siparis, teklif, talep vb.)
- Okundu/okunmadi durumu

---

### 17. ADRESLER (Addresses)

**Amac**: Teslimat ve fatura adresleri yonetimi.

**Ozellikler**:
- Adres CRUD
- Adres tipleri (teslimat, fatura, depo)
- Ulke/Sehir/Ilce secimi
- Varsayilan adres belirleme

---

### 18. SERVIS BAGLANTILARI (Service Connections)

**Amac**: Dis servis entegrasyonlari.

**Sayfalar**:
- `/ServiceConnections` - Baglanti yonetimi

**Ozellikler**:
- Lojistik servis baglantisi
- Sigorta servis baglantisi
- Gumruk servis baglantisi
- Baglanti durumu takibi

---

### 19. ADMIN PANELI (Admin)

**Amac**: Platform yonetimi (sadece Platform capability).

**Sayfalar**:
- `/Admin/Users` - Kullanici yonetimi
- `/Admin/Vendors` - Firma yonetimi
- `/Admin/Roles` - Rol yonetimi

**Ozellikler**:
- Kullanici CRUD
- Firma onaylama/reddetme
- Rol ve yetki tanimlama
- Sistem istatistikleri

---

## Teknik Detaylar

### Veritabani Semalari

**Temel Tablolar**:
- `AspNetUsers` - Kullanicilar
- `Vendors` - Firmalar
- `Products` - Urunler
- `ProductCategories` - Kategoriler
- `Orders` - Siparisler
- `Carts` - Sepetler

**Attribute Sistemi**:
- `ProductAttributes` - Ozellik tanimlari
- `ProductAttributeValues` - Ozellik degerleri
- `ProductAttributeMappings` - Urun-ozellik eslesmesi

**RBAC Sistemi**:
- `PlatformModules` - Modul tanimlari
- `VendorCapabilities` - Firma yetenekleri
- `CompanyRoles` - Firma icin roller
- `CompanyRoleModulePermissions` - Rol-modul izinleri

### API Yapisi

Tum API'ler `/api/` prefix'i ile baslar:
- `/api/catalog/*` - Katalog (public)
- `/api/products/*` - Urun yonetimi
- `/api/cart/*` - Sepet islemleri
- `/api/orders/*` - Siparis islemleri
- `/api/team/*` - Ekip yonetimi
- `/api/types/*` - Tip/durum listeleri

### Frontend Pattern'leri

**KnockoutJS Kullanimi**:
```javascript
// ViewModel yapisi
function PageViewModel() {
    var self = this;

    // Observables
    self.items = ko.observableArray([]);
    self.isLoading = ko.observable(false);

    // Functions
    self.loadData = function() { ... };
    self.save = function() { ... };
}
```

**Modal Pattern**:
- Her sayfa tek `Index.cshtml` + tek `Index.js`
- CRUD islemleri modal ile
- Tablo listeleri DataTables veya KnockoutJS foreach

### Localization

- Ceviriler `App_Data/Localization/` klasorunde
- `resources.tr.xml` - Turkce
- `resources.en.xml` - Ingilizce
- `@Html.T("Key", "Default")` ile kullanim

---

## Kurulum ve Calistirma

### Gereksinimler
- .NET 9 SDK
- PostgreSQL 17
- Visual Studio 2022

### Veritabani Kurulumu
```bash
dotnet ef database update
```

### Calistirma
Visual Studio'dan HTTPS modunda debug (F5)
- URL: https://localhost:7083

---

## Versiyon Gecmisi

Detayli degisiklikler icin `PROJECT_STATUS.xml` dosyasina bakiniz.

---

*Bu dokuman Bridgo B2B Platform v1.0 icin hazirlanmistir.*
*Son guncelleme: 2025-12-30*
