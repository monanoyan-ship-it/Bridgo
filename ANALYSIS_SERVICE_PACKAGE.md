# Servis Paketi Analizi

## Mevcut Durum

### Entity'ler MEVCUT (Dogru Tasarlanmis!)

```
Order
  |-- OrderItem (urunler)
  |-- OrderServiceRequest (hizmet talepleri)
  |     |-- ServiceType: 1=Logistics, 2=Customs, 3=Insurance (4=Survey EKSIK)
  |     |-- Kaynak/Hedef adres, agirlik, hacim, HS kodu vs.
  |     |-- SelectedQuoteId (secilen teklif)
  |     +-- OrderServiceQuote[] (gelen teklifler)
  |           |-- ProviderVendorId (servis saglayici)
  |           |-- QuoteAmount, Currency
  |           |-- EstimatedDays, EstimatedDeliveryDate
  |           +-- Status: Pending, Accepted, Rejected
  |
  |-- OrderParticipant (katilimcilar)
  |     |-- Role: Seller, LogisticsProvider, CustomsBroker, InsuranceProvider, Investor
  |     |-- Amount, CommissionAmount, NetAmount
  |     +-- IsPaid, IsTaskCompleted
  |
  +-- OrderInvestment (finansman)
        |-- InvestorVendorId
        |-- Amount, ReturnRate, ExpectedReturn
        +-- IsFunded, IsRepaid
```

### Yanlis Yapilan
Ayri ayri talep sayfalari olusturulmus:
- LogisticsRequests
- CustomsRequests
- InsuranceRequests
- SurveyRequests
- FinancingRequests

Bunlar Order'a bagli olmadan bagimsiz islemler gibi tasarlanmis.

## Dogru Akis

### 1. Urun + Adres Belli Oldugunda

```
Kaynak: Saticinin deposu (Warehouse/Address)
Hedef:  Alicinin teslimat adresi
Urun:   Agirlik, hacim, HS kodu, deger, miktar
```

Bu bilgiler belli oldugunda tum servis saglayicilardan teklif alinabilir.

### 2. Checkout Akisi

```
CART (Sepet)
    |
    v
CHECKOUT
    |
    +-- Teslimat Adresi Sec
    |
    +-- Hizmet Ihtiyaclari:
    |       [x] Lojistik (Tasima)
    |       [x] Gumruk (Ithalat/Ihracat islemleri)
    |       [x] Gozetim (Kalite kontrolu)
    |       [x] Sigorta (Nakliye sigortasi)
    |       [ ] Finansman (Siparis finansmani) -- opsiyonel
    |
    +-- Servis Saglayicilardan Teklifler Gelir
    |       - Tasimaci A: 500 USD
    |       - Tasimaci B: 450 USD
    |       - Gumrukcu X: 200 USD
    |       - Sigortaci Y: 50 USD
    |       - vs.
    |
    +-- Her Kategoriden Bir Teklif Sec
    |
    +-- Toplam Tutar:
    |       Urun Bedeli:     10,000 USD
    |       Lojistik:           450 USD
    |       Gumruk:             200 USD
    |       Sigorta:             50 USD
    |       -----------------------
    |       TOPLAM:          10,700 USD
    |
    v
ODEME --> Siparis Baslar
```

### 3. Finansman Secenegi

Alici "Finansman istiyorum" derse:
- Siparis bilgileri (tutar, urunler, adresler) yatirimcilara gider
- Yatirimcilar teklif verir (faiz orani, vade)
- Alici secerse:
  - Yatirimci odemeyi yapar
  - Alici yatirimciya vade sonunda geri oder

### 4. Kritik Noktalar

- Satici onayi YOK - stok varsa veya teklif verdiyse hazir
- Sepet tek seferlik - surekli ekleme yapilan bir sepet degil
- Tum hizmetler tek pakette - ayri talepler degil
- Checkout'ta hepsi bir arada secilip odeniyor

## Mevcut Sistem Incelenmesi Gereken Yerler

1. **Cart Sistemi**
   - Controllers/Api/CartApiController.cs
   - Views/Cart veya Views/Products/Cart
   - Checkout akisi nasil?

2. **Order Sistemi**
   - Order entity'si
   - OrderService
   - Siparis durumu akisi

3. **Servis Talepleri**
   - Mevcut LogisticsRequest, CustomsRequest vs. entity'leri
   - Bunlar Order'a nasil baglanacak?

4. **Servis Saglayicilar**
   - Tasimaci, Gumrukcu, Sigortaci, Gozetmen, Yatirimci capability'leri
   - Bunlar teklif nasil verecek?

## Yeni Tasarim Onerileri

### Entity Degisiklikleri

```
Order
  +-- OrderServiceRequests (hangi hizmetler isteniyor)
        +-- ServiceType: Logistics, Customs, Survey, Insurance, Financing
        +-- Status: Pending, OffersReceived, Selected, InProgress, Completed
        +-- SelectedOfferId (secilen teklif)

ServiceOffer
  +-- OrderServiceRequestId
  +-- ProviderVendorId (servis saglayici)
  +-- ProviderType: Carrier, CustomsBroker, Surveyor, Insurer, Investor
  +-- Price, Currency
  +-- Details (JSON - hizmete ozel detaylar)
  +-- ValidUntil
  +-- Status: Pending, Accepted, Rejected, Expired
```

### Akis

1. Alici sepeti olusturur
2. Checkout'ta adres + hizmet ihtiyaclarini belirtir
3. Sistem ilgili servis saglayicilara bildirim gonderir
4. Servis saglayicilar teklif verir
5. Alici teklifleri gorur, secer
6. Odeme yapar
7. Siparis + secilen hizmetler baslar

## Eksikler ve Yapilacaklar

### 1. TypeDefinitions Eksikleri
```csharp
// ServiceTypes - Survey eksik
public static readonly TypeItem Survey = new(4, "Survey", "Gozetim", ...);
```

### 2. Cart -> Checkout Akisi (EKSIK)

CartApiController'da sadece sepet CRUD var.
Checkout endpoint'i YOK.

Gerekli endpoint'ler:
```
POST /api/cart/checkout
  - Cart'i Order'a donustur
  - Hizmet ihtiyaclarini al (logistics, customs, survey, insurance, financing)
  - OrderServiceRequest'ler olustur
  - Servis saglayicilara bildirim gonder

GET /api/orders/{id}/service-quotes
  - Gelen teklifleri listele

POST /api/orders/{id}/service-quotes/{quoteId}/select
  - Teklif sec

POST /api/orders/{id}/confirm
  - Tum hizmetler secildikten sonra onayla
  - Odeme adimina gec
```

### 3. Servis Saglayici Paneli (EKSIK)

Tasimaci, Gumrukcu, Gozetmen, Sigortaci icin:
```
GET /api/provider/service-requests
  - Bana gelen talepleri listele

POST /api/provider/service-requests/{id}/quote
  - Teklif ver

GET /api/provider/my-quotes
  - Verdiklerim teklifler
```

### 4. UI Sayfalari

ALICI TARAFI:
- /Cart - Sepet sayfasi (mevcut mi?)
- /Checkout - Checkout sayfasi
  - Hizmet ihtiyaclari secimi
  - Gelen teklifleri gorme
  - Teklif secimi
  - Odeme

SERVIS SAGLAYICI TARAFI:
- /Provider/Requests - Gelen talepler (capability bazli)
- Teklif verme modali

### 5. Bildirim Entegrasyonu

Yeni talep geldiginde ilgili servis saglayicilara:
- In-app bildirim
- E-posta (opsiyonel)
- Kategori takibi gibi (CategorySubscription benzeri)

---

## Mimari Karar

### Secenek A: Order-Centric (Onerilen)
- Checkout sirasinda Order olusur
- OrderServiceRequest'ler Order'a bagli
- Tum teklifler Order uzerinden yonetilir
- Mevcut entity'ler bunu destekliyor!

### Secenek B: Pre-Order Service Request
- Checkout oncesi servis talepleri olusur
- Teklifler gelir, secilir
- Sonra Order olusur
- Daha karmasik, gereksiz

---

## Uygulama Plani

### Faz 1: TypeDefinitions + API
1. ServiceTypes'a Survey ekle
2. CheckoutService olustur
3. Cart checkout endpoint'leri
4. Servis saglayici teklif endpoint'leri

### Faz 2: Alici UI
1. Cart sayfasini guncelle/olustur
2. Checkout sayfasi (hizmet secimi + teklif goruntuleme)
3. Teklif karsilastirma ve secim
4. Odeme entegrasyonu

### Faz 3: Servis Saglayici UI
1. Gelen talepler sayfasi
2. Teklif verme modali
3. Tekliflerim sayfasi

### Faz 4: Bildirimler
1. Yeni talep bildirimi
2. Teklif geldi bildirimi
3. Teklif kabul/red bildirimi
