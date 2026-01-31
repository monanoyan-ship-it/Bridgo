# Bridgo Platform - Module and Parameter Reference

Bu dokuman platformdaki tum modulleri, parametreleri ve bunlari kimin yonettigini listeler.

## Yonetim Rolleri (Bu Dokumandaki)

| Rol | Aciklama |
|-----|----------|
| **Admin** | Platform yoneticisi - tum sistemi yonetir |
| **Vendor** | Firma kullanicilari |
| **Developer** | TypeDefinitions.cs ve kod tabaninda tanimlar |

---

## Firma Ici Yetki Seviyeleri (VendorUserRole)

**ONEMLI**: Bu, is fonksiyonu DEGIL, yonetim yetkisidir!
Is fonksiyonlari (Order Manager, Catalog Manager vb.) CompanyRoles ile yonetilir.

| ID | Kod | Turkce | Yetki Kapsamı |
|----|-----|--------|---------------|
| 0 | Owner | Firma Sahibi | Her seye erisim, firma sahipligini devredebilir |
| 1 | Admin | Yonetici (Tam Yetki) | Her seye erisim ama sahip degil |
| 2 | Supervisor | Denetci | Islemler, uygulamalar, ayarlar (kullanici yonetimi haric) |
| 3 | Manager | Birim Yoneticisi | Islemler, kisitli hesap erisimi |
| 4 | Employee | Calisan | Sadece atanan moduller (VARSAYILAN) |

**Varsayilan**: Yeni eklenen tum kullanicilar "Calisan" olarak baslar.

### Fark Ne?
- **Owner vs Admin**: Ikisi de tam yetkiye sahip, ama Owner firma sahibidir ve bu hakki baskasina devredebilir.
- **Supervisor vs Manager**: Supervisor daha genis yetki (ayarlar dahil), Manager sadece gunluk islemleri yonetir.
- **Manager vs Employee**: Manager kendi birimindeki calisanlari gorebilir, Employee sadece kendi islerini gorur.

### Is Fonksiyonu Rolleri (CompanyRoles - RBAC)

Bu roller modul erisimini kontrol eder:
- Order Manager - Siparis yonetimi
- Catalog Manager - Urun/kategori yonetimi
- Stock Manager - Stok/fiyat yonetimi
- Finance Manager - Finans islemleri
- vb.

Bir kullanici hem "Calisan" yetki seviyesinde olup hem de "Order Manager" is rolune sahip olabilir.

---

## 1. TypeDefinitions (Developer Yonetir)

`Models/Enums/TypeDefinitions.cs` dosyasinda tanimlanan statik tipler.
Bu tipler kod tabaninda degistirilir ve migration gerektirmez.

### 1.1 Kategori Silme Durumlari (CategoryDeletionStatuses)
| ID | SystemName | Aciklama |
|----|------------|----------|
| 0 | Pending | Onay bekliyor |
| 1 | Approved | Onaylandi |
| 2 | Rejected | Reddedildi |

### 1.2 Adres Tipleri (AddressTypes)
| ID | SystemName | Aciklama |
|----|------------|----------|
| 1 | Billing | Fatura adresi |
| 2 | Shipping | Teslimat adresi |
| 3 | Headquarters | Merkez ofis adresi |
| 4 | Warehouse | Depo adresi |
| 5 | Branch | Sube adresi |
| 6 | Return | Iade adresi |

### 1.3 Depo Tipleri (WarehouseTypes)
| ID | SystemName | Aciklama |
|----|------------|----------|
| 1 | Main | Ana depo |
| 2 | Distribution | Dagitim deposu |
| 3 | Returns | Iade deposu |
| 4 | Temporary | Gecici depo |
| 5 | Virtual | Sanal depo (dropship) |
| 6 | Consignment | Konsinye depo |

### 1.4 Vendor Durumlari (VendorStatuses)
| ID | SystemName | Aciklama |
|----|------------|----------|
| 1 | Pending | Onay bekliyor |
| 2 | Active | Aktif |
| 3 | Suspended | Askiya alinmis |
| 4 | Rejected | Reddedildi |
| 5 | Deleted | Silindi |

### 1.5 Urun Durumlari (ProductStatuses)
| ID | SystemName | Aciklama |
|----|------------|----------|
| 1 | Draft | Taslak |
| 2 | Pending | Onay bekliyor |
| 3 | Active | Aktif |
| 4 | Inactive | Pasif |
| 5 | OutOfStock | Stokta yok |
| 6 | Discontinued | Satisi durdurulmus |

### 1.6 Siparis Durumlari (OrderStatuses)
| ID | SystemName | Aciklama |
|----|------------|----------|
| 1 | Pending | Beklemede |
| 2 | Confirmed | Onaylandi |
| 3 | Processing | Isleniyor |
| 4 | Shipped | Kargolandi |
| 5 | Delivered | Teslim edildi |
| 6 | Cancelled | Iptal edildi |
| 7 | Refunded | Iade edildi |

### 1.7 Talep Durumlari (DemandStatuses)
| ID | SystemName | Aciklama |
|----|------------|----------|
| 0 | Draft | Taslak |
| 1 | Pending | Beklemede |
| 2 | Active | Aktif |
| 3 | Closed | Kapandi |
| 4 | Cancelled | Iptal edildi |
| 5 | Expired | Suresi doldu |

### 1.8 Team Member Durumlari (TeamMemberStatuses)
| ID | SystemName | Aciklama |
|----|------------|----------|
| 1 | Invited | Davet edildi |
| 2 | Active | Aktif |
| 3 | Suspended | Askiya alinmis |
| 4 | Removed | Cikarildi |

### 1.9 Talep Yanit Durumlari (DemandResponseStatuses)
| ID | SystemName | Aciklama |
|----|------------|----------|
| 1 | Pending | Beklemede |
| 2 | Accepted | Kabul edildi |
| 3 | Rejected | Reddedildi |
| 4 | Withdrawn | Geri cekildi |
| 5 | Expired | Suresi doldu |
| 6 | Ordered | Siparis verildi |

### 1.10 Urun Talep Durumlari (ProductInquiryStatuses)
| ID | SystemName | Aciklama |
|----|------------|----------|
| 1 | Open | Acik |
| 2 | Responded | Yanitlandi |
| 3 | Accepted | Kabul edildi |
| 4 | Rejected | Reddedildi |
| 5 | Cancelled | Iptal edildi |
| 6 | Expired | Suresi doldu |

### 1.11 Odeme Durumlari (PaymentStatuses)
| ID | SystemName | Aciklama |
|----|------------|----------|
| 1 | Pending | Beklemede |
| 2 | Completed | Tamamlandi |
| 3 | Failed | Basarisiz |
| 4 | Cancelled | Iptal edildi |
| 5 | Refunded | Iade edildi |
| 6 | PartiallyRefunded | Kismi iade |

### 1.12 Para Birimleri (Currencies) - ISO 4217
| Code | Aciklama | Sembol |
|------|----------|--------|
| USD | US Dollar | $ |
| EUR | Euro | € |
| GBP | British Pound | £ |
| JPY | Japanese Yen | ¥ |
| CHF | Swiss Franc | Fr |
| CNY | Chinese Yuan | ¥ |
| TRY | Turkish Lira | ₺ |
| AED | UAE Dirham | د.إ |
| ... | ... | ... |

### 1.13 KYC Belge Kategorileri (KycDocumentCategories)
| ID | SystemName | Aciklama |
|----|------------|----------|
| 1 | identity | Kimlik Belgeleri |
| 2 | company | Firma Belgeleri |
| 3 | bank | Banka Belgeleri |
| 4 | auth | Yetki Belgeleri |

### 1.14 Tehlike Siniflari (DangerClasses) - ADR/IMDG
| ID | SystemName | Aciklama |
|----|------------|----------|
| 1 | Explosives | Sinif 1: Patlayicilar |
| 2 | Gases | Sinif 2: Gazlar |
| 3 | FlammableLiquids | Sinif 3: Yanici Sivilar |
| ... | ... | ... |

---

## 2. Database Tables (Admin Yonetir)

### 2.1 Platform Modulleri (PlatformModules)
**Yoneten**: Admin (Admin Panel > Modules)

| Alan | Aciklama |
|------|----------|
| Name | Modul sistem adi |
| DisplayName | Gorunen ad |
| Route | URL yolu |
| Icon | Bootstrap icon |
| ParentId | Ust modul |
| IsMenuSection | Menu basligi mi? |
| IsActive | Aktif mi? |

### 2.2 Vendor Yetenekleri (VendorCapabilities)
**Yoneten**: Admin (sabit kayitlar)

| ID | Name | Aciklama |
|----|------|----------|
| 1 | Platform | Platform yonetimi |
| 2 | Seller | Satici |
| 3 | Buyer | Alici |
| 4 | Carrier | Tasimaci |
| 5 | Insurance | Sigorta |
| 6 | CustomsBroker | Gumruk Musaviri |

### 2.3 Firma Rolleri (CompanyRoles)
**Yoneten**: Admin (varsayilan roller) + Vendor (ozel roller)

Sistem rolleri (IsSystem=true):
- Account Manager
- Order Staff
- Support Staff
- Finance Staff

### 2.4 Urun Kategorileri (ProductCategories)
**Yoneten**: Admin (global kategoriler)
- Hiyerarsik yapi (ParentId)
- Global, VendorId yok

### 2.5 Ulkeler ve Eyaletler (Countries, States)
**Yoneten**: Developer (seed data)
- Sabit veriler, migration ile yuklenir

---

## 3. Vendor Yonetimli Veriler

### 3.1 Firma Bilgileri
**Yoneten**: Vendor Owner/Admin
- Sirket bilgileri
- Logo
- Fatura bilgileri
- Banka hesaplari (IBAN, SWIFT, Currency)
- Adresler

### 3.2 Team Members
**Yoneten**: Vendor Owner/Admin
- Kullanici davetleri
- Rol atamalari
- Yetki yonetimi

### 3.3 Urunler
**Yoneten**: Vendor (Satici capability)
- Urun CRUD
- Fiyatlandirma
- Stok yonetimi
- Depo atamalari

### 3.4 Siparisler
**Yoneten**: Vendor (Alici/Satici)
- Siparis olusturma (Buyer)
- Siparis ishleme (Seller)
- Durum guncellemeleri

### 3.5 Talepler (Demands)
**Yoneten**: Vendor (Alici)
- Talep olusturma
- Teklif degerlendirme

### 3.6 Teklifler
**Yoneten**: Vendor (Satici)
- Taleplere teklif verme
- ProductInquiry yanitlama

---

## 4. API Endpoints

### 4.1 Types API (`/api/types/*`)
Tum TypeDefinitions icin dinamik endpoint'ler.

| Endpoint | Aciklama |
|----------|----------|
| GET /api/types/address | Adres tipleri |
| GET /api/types/warehouse | Depo tipleri |
| GET /api/types/vendor-status | Vendor durumlari |
| GET /api/types/product-status | Urun durumlari |
| GET /api/types/order-status | Siparis durumlari |
| GET /api/types/currency | Para birimleri |
| GET /api/types/kyc-document-category | KYC belge kategorileri |
| GET /api/types/kyc-document-type | KYC belge tipleri |

---

## 5. Yeni Modul/Parametre Eklerken

### 5.1 TypeDefinitions Ekleme (Developer)
1. `Models/Enums/TypeDefinitions.cs` dosyasina yeni sinif ekle
2. `Controllers/Api/TypesApiController.cs` dosyasina endpoint ekle
3. Bu dokumani guncelle

### 5.2 Database Table Ekleme
1. Entity sinifi olustur (`Models/Entities/`)
2. `ApplicationDbContext`'e DbSet ekle
3. Migration olustur
4. Admin panel'e CRUD ekle (gerekirse)
5. Bu dokumani guncelle

### 5.3 Vendor Feature Ekleme
1. Entity ve Service olustur
2. API Controller ekle
3. View ve JS dosyalari ekle
4. PlatformModules'e kayit ekle (Admin panel veya migration)
5. CapabilityModuleMappings'e izin ekle
6. Bu dokumani guncelle

---

## 6. Degisiklik Gecmisi

| Tarih | Degisiklik | Kim |
|-------|------------|-----|
| 2024-12-24 | Ilk versiyon olusturuldu | Claude |
| 2024-12-24 | Currencies (Para Birimleri) eklendi | Claude |
| 2024-12-24 | Bank Account international fields (SWIFT, Currency) | Claude |
