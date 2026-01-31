# Sipariş Akışı İyileştirme Planı

> **Son Güncelleme:** 2026-01-26
> **Durum:** Faz 5 Temel UI Tamamlandı ✅ (Checkout Step Tracking dahil)

## 1. Mevcut Akış (AS-IS)

### 1.1 Talep Akışı
```
PublicDemand (Alıcı oluşturur)
    ↓
DemandResponse (Satıcılar teklif verir)
    ↓
Negotiation (Opsiyonel pazarlık)
    ↓
Accept → ORDER oluşur
    ↓
Checkout (Servis seçimi - checkbox)
    ↓
TÜM ServiceRequest'ler AYNI ANDA oluşur
```

### 1.2 Direkt Alım Akışı
```
Catalog → Cart → Checkout
    ↓
Servis Seçimi (checkbox)
    ↓
ORDER + TÜM ServiceRequest'ler AYNI ANDA oluşur
```

### 1.3 Mevcut Sorunlar

| Sorun | Açıklama | Etki |
|-------|----------|------|
| Eşzamanlı Servis Oluşturma | Lojistik, Gümrük, Sigorta, Gözetim hepsi birden oluşuyor | Gözetim mantıksız (lojistik belli değilken gözetim nereye?) |
| Gözetim Tetikleme Yok | Survey, lojistik seçildikten sonra anlamlı | Gereksiz teklif toplama |
| Finansman Bağlantısız | FinancingRequest tamamen ayrı modül | Otomatik tetikleme yok |
| Orchestration Yok | Servisler arası bağımlılık yönetilmiyor | Manuel süreç gerekli |

---

## 2. Hedef Akış (TO-BE)

### 2.1 Aşamalı Servis Akışı

```
┌─────────────────────────────────────────────────────────────────────────┐
│                           AŞAMA 1: SİPARİŞ                              │
├─────────────────────────────────────────────────────────────────────────┤
│  Talep Kabul / Direkt Alım                                              │
│       ↓                                                                 │
│  ORDER oluştu (Status: Draft)                                           │
│       ↓                                                                 │
│  Servis ihtiyaçları belirlendi (checkbox)                               │
│       - [ ] Lojistik gerekli                                            │
│       - [ ] Gümrük gerekli                                              │
│       - [ ] Sigorta gerekli                                             │
│       - [ ] Gözetim gerekli (Lojistik seçildikten sonra aktif olacak)   │
│       - [ ] Finansman gerekli (Tüm servisler seçildikten sonra)         │
└─────────────────────────────────────────────────────────────────────────┘
                                    ↓
┌─────────────────────────────────────────────────────────────────────────┐
│                    AŞAMA 2: TEMEL SERVİSLER (Paralel)                   │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  ┌─────────────┐    ┌─────────────┐    ┌─────────────┐                 │
│  │  Lojistik   │    │   Gümrük    │    │   Sigorta   │                 │
│  │  Request    │    │   Request   │    │   Request   │                 │
│  │   (Open)    │    │   (Open)    │    │   (Open)    │                 │
│  └──────┬──────┘    └──────┬──────┘    └──────┬──────┘                 │
│         ↓                  ↓                  ↓                         │
│     Teklifler          Teklifler          Teklifler                     │
│         ↓                  ↓                  ↓                         │
│      SEÇİLDİ            SEÇİLDİ            SEÇİLDİ                      │
│         │                  │                  │                         │
└─────────┼──────────────────┼──────────────────┼─────────────────────────┘
          │                  │                  │
          └──────────────────┼──────────────────┘
                             ↓
                    Lojistik Seçildi mi?
                             │
              ┌──────────────┴──────────────┐
              │ EVET                        │ HAYIR
              ↓                             ↓
┌─────────────────────────────────────┐     │
│        AŞAMA 3: GÖZETİM             │     │
├─────────────────────────────────────┤     │
│  Survey Request otomatik oluşur     │     │
│  (Lojistik bilgileriyle)            │     │
│       ↓                             │     │
│   Gözetim Teklifleri                │     │
│       ↓                             │     │
│    SEÇİLDİ                          │     │
└──────────────┬──────────────────────┘     │
               │                            │
               └────────────┬───────────────┘
                            ↓
                   TÜM SERVİSLER SEÇİLDİ
                            ↓
┌─────────────────────────────────────────────────────────────────────────┐
│                      AŞAMA 4: FİNANSMAN (Opsiyonel)                     │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │  "Toplam tutar: 125,000 TRY. Finansman desteği ister misiniz?"  │   │
│  │  [ ] Evet, yatırımcı teklifleri toplansın                       │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                                                                         │
│  EVET ise:                                                              │
│       ↓                                                                 │
│  FinancingRequest otomatik oluşur                                       │
│  (Order + Servis maliyetleri ile)                                       │
│       ↓                                                                 │
│  Yatırımcı Teklifleri                                                   │
│       ↓                                                                 │
│  Teklif SEÇİLDİ                                                         │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────────────────┐
│                      AŞAMA 5: SÖZLEŞME & ÖDEME                          │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  Sözleşme Özeti:                                                        │
│  - Ürün bedeli: 100,000 TRY                                             │
│  - Lojistik: 5,000 TRY (ABC Lojistik)                                   │
│  - Gümrük: 2,000 TRY (XYZ Müşavirlik)                                   │
│  - Sigorta: 1,500 TRY (DEF Sigorta)                                     │
│  - Gözetim: 800 TRY (GHI Gözetim)                                       │
│  - Finansman faizi: 2,500 TRY                                           │
│  ─────────────────────────                                              │
│  TOPLAM: 111,800 TRY                                                    │
│                                                                         │
│  [Sözleşmeyi Onayla] → [Ödemeye Geç]                                    │
│                                                                         │
│  Ödeme alındı → Order.Status = Confirmed                                │
│              → Tüm katılımcılara görevler oluşur                        │
│              → Yatırımcı transferi beklenir                             │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 2.2 Gözetim Tipleri ve Zamanlaması

| Gözetim Tipi | Ne Zaman | Tetikleyen |
|--------------|----------|------------|
| PreLoading (Yükleme Öncesi) | Mallar hazırlandığında | Seller: "Hazır" |
| Loading (Yükleme) | Lojistik başlarken | Logistics: "Yükleme başladı" |
| Unloading (Boşaltma) | Varışta | Logistics: "Varış" |
| DamageAssessment (Hasar) | Sorun olduğunda | Buyer/Seller talep |

### 2.3 Finansman Tetikleme Koşulları

```javascript
// Otomatik FinancingRequest oluşturma koşulları
const shouldTriggerFinancing = (order) => {
    return order.requiresFinancing === true &&
           order.allServicesSelected === true &&
           order.totalAmount >= MIN_FINANCING_AMOUNT;
};

// Finansman tutarı hesaplama
const calculateFinancingAmount = (order) => {
    return order.itemsTotal +
           order.selectedQuotes.reduce((sum, q) => sum + q.amount, 0);
};
```

---

## 3. Uygulama Planı

### Faz 1: Temel Altyapı (Öncelik: YÜKSEK) ✅ TAMAMLANDI

- [x] **3.1.1** Order entity'ye `RequiresFinancing` (bool) alanı ekle
- [x] **3.1.2** Order entity'ye `SurveyTriggerStatus` enum alanı ekle
  - `NotRequired` = Gözetim gerekli değil
  - `WaitingForLogistics` = Lojistik seçimi bekleniyor
  - `Ready` = Gözetim request oluşturulabilir
  - `Created` = Gözetim request oluşturuldu
- [x] **3.1.3** OrderServiceRequest'e `DependsOnServiceRequestId` (int?) ekle
- [x] **3.1.4** Migration oluştur ve uygula

**Ek değişiklikler (2026-01-26):**
- Order.AllServicesSelectedAt (DateTime?) - Tüm servisler seçildiğinde
- Order.FinancingRequestId (int?) - Otomatik oluşturulan finansman talebi FK
- OrderServiceRequest.AutoCreated (bool) - Otomatik mi oluşturuldu
- OrderServiceRequest.TriggerSource (string) - Tetikleme kaynağı
- SurveyTriggerStatuses enum (TypeDefinitions.cs)

### Faz 2: Gözetim Tetikleme (Öncelik: YÜKSEK) ✅ TAMAMLANDI

- [x] **3.2.1** `IOrderOrchestrationService` interface oluştur
- [x] **3.2.2** `OrderOrchestrationService` implementasyonu:
  ```csharp
  // Lojistik teklifi kabul edildiğinde çağrılır
  Task OnServiceQuoteAcceptedAsync(int orderId, int serviceRequestId, int quoteId);

  // Otomatik Survey request oluşturur
  Task<int> CreateSurveyRequestFromLogisticsAsync(int orderId, int logisticsRequestId, int logisticsQuoteId);

  // Tüm servisler seçildi mi kontrol
  Task<bool> CheckAllServicesSelectedAsync(int orderId);

  // Finansman tetikleme (hazır, faz 3'te aktif)
  Task<int?> TriggerFinancingIfNeededAsync(int orderId);
  ```
- [x] **3.2.3** `OrderService.AcceptServiceQuoteAsync` metoduna tetikleme ekle
- [x] **3.2.4** Checkout'ta Gözetim koşullu mantığı ekle

**Ek değişiklikler (2026-01-26):**
- CheckoutService: Lojistik + Gözetim seçildiyse → Survey oluşturma, SurveyTriggerStatus = WaitingForLogistics
- OrderService: Teklif kabul sonrası orchestration service çağrısı
- Program.cs: IOrderOrchestrationService DI kaydı

### Faz 3: Finansman Tetikleme (Öncelik: ORTA) ✅ TAMAMLANDI

- [x] **3.3.1** Finansman metodları `IOrderOrchestrationService`'e eklendi
- [x] **3.3.2** `OrderOrchestrationService` finansman implementasyonu:
  ```csharp
  // Tüm servisler seçildiğinde çağrılır
  Task<bool> CheckAllServicesSelectedAsync(int orderId);

  // Finansman gerekiyorsa FinancingRequest oluşturur
  Task<int?> TriggerFinancingIfNeeded(int orderId);

  // Manuel finansman talebi
  Task<int?> RequestFinancingAsync(int orderId, int buyerVendorId);

  // Finansman durumu sorgula
  Task<FinancingStatusDto> GetFinancingStatusAsync(int orderId);
  ```
- [x] **3.3.3** Teklif kabul flow'una finansman kontrolü ekle
- [x] **3.3.4** API endpoint'leri eklendi:
  - `GET /api/orders/{orderId}/financing-status`
  - `POST /api/orders/{orderId}/request-financing`
- [ ] **3.3.5** "Finansman İster misiniz?" modal UI oluştur (Faz 5'e taşındı)

**Ek değişiklikler (2026-01-26):**
- QuoteAcceptedResult DTO: CreatedSurveyRequestId, AllServicesSelected, FinancingAvailable, TotalAmount, FinancingRequestId
- FinancingStatusDto: RequiresFinancing, AllServicesSelected, CanTriggerFinancing, ExistingFinancingRequestId, ItemsTotal, ServicesTotal, TotalAmount, Currency, PendingServiceRequests, SelectedServiceRequests
- OnServiceQuoteAcceptedAsync: Tam orchestration sonucu döner
- TriggerFinancingIfNeededAsync: Otomatik FinancingRequest oluşturma
- RequestFinancingAsync: Manuel finansman talebi
- GetFinancingStatusAsync: Finansman durumu sorgulama
- OrdersApiController: financing-status ve request-financing endpoint'leri

### Faz 4: Risk Hesaplama (Öncelik: ORTA) ✅ TAMAMLANDI

- [x] **3.4.1** Risk hesaplama algoritması tasarla:
  ```
  Faktörler ve Ağırlıkları (toplam 100):
  - Firma doğrulama durumu: %20
  - Firma yaşı: %10
  - Profil tamamlanma: %10
  - Sipariş geçmişi: %20
  - Ödeme geçmişi: %15
  - Teminat tipi: %15
  - Tutar/Geçmiş oranı: %10
  ```
- [x] **3.4.2** `IRiskScoringService` interface
  - CalculateRiskScoreAsync(financingRequestId)
  - CalculateOrderRiskScoreAsync(orderId, amount, days)
  - CalculateVendorRiskScoreAsync(vendorId)
  - UpdateRiskScoreAsync(financingRequestId)
- [x] **3.4.3** `RiskScoringService` implementasyonu
  - 7 faktörlü ağırlıklı risk hesaplama
  - RiskScoreResult: Score (0-100), Level (Low/Medium/High/VeryHigh), Factors
  - RiskFactorDto: Name, Category, Score, Weight, WeightedScore, Description, IsPositive
- [x] **3.4.4** FinancingRequest oluşturulurken otomatik risk skoru
  - OrderOrchestrationService.TriggerFinancingIfNeededAsync içinde entegre
- [x] **3.4.5** API Endpoints eklendi:
  - GET /api/investment/{requestId}/risk-score - Risk skorunu getir
  - POST /api/investment/{requestId}/recalculate-risk - Yeniden hesapla
  - GET /api/investment/vendor/{vendorId}/risk-score - Firma risk skoru

**Risk Seviyeleri:**
- 0-25: Low (success) - Düşük risk
- 26-50: Medium (warning) - Orta risk
- 51-75: High (danger) - Yüksek risk
- 76-100: VeryHigh (dark) - Çok yüksek risk

### Faz 5: UI İyileştirmeleri (Öncelik: DÜŞÜK) ✅ TEMEL TAMAMLANDI

- [x] **3.5.1** Checkout'ta seçilmemiş teklif uyarısı:
  - Kullanıcı teklif seçmeden checkout yaparsa uyarı göster
  - "Şu servisler için teklif seçmediniz: Lojistik, Gümrük. Devam etmek istediğinize emin misiniz?"
  - Tüm teklifler seçildiyse uyarı gösterme
  - **NOT:** Teklif seçimi ZORUNLU DEĞİL, sadece uyarı
  - **Uygulama:** MyOrders.cshtml + MyOrders.js - unselectedQuotesModal
- [x] **3.5.2** "Finansman İster misiniz?" modal UI oluştur
  - Sipariş tamamlandığında finansman seçeneği göster
  - Toplam tutar ve detaylar görüntülenir
  - Kullanıcı kabul ederse finansman talebi oluşturulur
  - Başarı modalı ile onay
  - **Uygulama:** financingOptionModal, financingSuccessModal
- [x] **3.5.3** Checkout adım takibi (CheckoutSteps):
  - Order.CheckoutStep field'ı eklendi
  - CheckoutSteps enum: ServiceRequested(1), WaitingForQuotes(2), QuotesReceived(3), QuotesSelected(4), FinancingPending(5), PaymentPending(6), Completed(7)
  - CheckoutProgressDto ve CheckoutStepInfo DTO'ları
  - API: GET /api/orders/{orderId}/checkout-progress
  - Checkout timeline UI: Adım göstergesi ile görsel ilerleme
  - Otomatik adım güncellemeleri:
    - Sipariş oluşturulduğunda → ServiceRequested
    - Teklif geldiğinde → QuotesReceived
    - Tüm teklifler seçildiğinde → QuotesSelected
- [x] **3.5.4** Checkout wizard adımları (Fullscreen Modal):
  1. Teslimat Adresi (onay/seçim)
  2. Teklif Seçimi (hizmet teklifleri)
  3. Finansman Seçimi (opsiyonel)
  4. Özet & Ödeme
  **Not:** 7 adım yerine 4 adıma sadeleştirildi (daha iyi UX)
- [x] **3.5.5** Sipariş durumu timeline gösterimi (ayrı sayfa)
  - `/Orders/Timeline/{id}` route
  - OrderTimeline.cshtml + OrderTimeline.js
- [x] **3.5.6** Real-time teklif bildirimleri (SignalR)
  - NotificationHub + NotificationService entegrasyonu
  - Teklif geldiğinde otomatik toastr bildirimi

**Tamamlanan Faz 5 Özellikleri (2026-01-26):**
- proceedToCheckout(): Seçilmemiş teklif kontrolü
- unselectedServices observable: Seçilmemiş servis listesi
- unselectedQuotesModal: Uyarı modalı
- confirmProceedWithoutAllQuotes(): Uyarıyı kabul et
- checkFinancingAvailability(): Finansman durumu kontrolü
- financingStatus observable: Finansman durumu bilgisi
- financingOptionModal: Finansman seçeneği modalı
- requestFinancing(): Finansman talebi oluştur
- skipFinancing(): Finansmanı atla
- financingSuccessModal: Başarı modalı
- **Checkout Step Tracking (2026-01-26):**
  - Order.CheckoutStep field'ı
  - CheckoutSteps enum (TypeDefinitions.cs)
  - IOrderOrchestrationService: GetCheckoutProgressAsync, UpdateCheckoutStepOnQuoteReceivedAsync, UpdateCheckoutStepAsync
  - CheckoutProgressDto, CheckoutStepInfo DTO'ları
  - API: GET /api/orders/{orderId}/checkout-progress
  - Checkout timeline UI (MyOrders.cshtml)
  - loadOrderQuotes(): Paralel yükleme (quotes + checkout-progress)

---

## 4. Veritabanı Değişiklikleri

### 4.1 Order Tablosu Değişiklikleri

```sql
ALTER TABLE "Orders" ADD COLUMN "RequiresFinancing" boolean DEFAULT false;
ALTER TABLE "Orders" ADD COLUMN "SurveyTriggerStatus" integer DEFAULT 0;
ALTER TABLE "Orders" ADD COLUMN "AllServicesSelectedAt" timestamp NULL;
ALTER TABLE "Orders" ADD COLUMN "FinancingRequestId" integer NULL;
ALTER TABLE "Orders" ADD COLUMN "CheckoutStep" integer DEFAULT 1;
```

### 4.2 OrderServiceRequest Değişiklikleri

```sql
ALTER TABLE "OrderServiceRequests" ADD COLUMN "DependsOnServiceRequestId" integer NULL;
ALTER TABLE "OrderServiceRequests" ADD COLUMN "AutoCreated" boolean DEFAULT false;
ALTER TABLE "OrderServiceRequests" ADD COLUMN "TriggerSource" varchar(50) NULL;
```

### 4.3 Yeni Enum: SurveyTriggerStatus

```csharp
public static class SurveyTriggerStatuses
{
    public static readonly TypeItem NotRequired = new(0, "NotRequired", "Enum.SurveyTriggerStatus.NotRequired");
    public static readonly TypeItem WaitingForLogistics = new(1, "WaitingForLogistics", "Enum.SurveyTriggerStatus.WaitingForLogistics");
    public static readonly TypeItem Ready = new(2, "Ready", "Enum.SurveyTriggerStatus.Ready");
    public static readonly TypeItem Created = new(3, "Created", "Enum.SurveyTriggerStatus.Created");
}
```

### 4.4 Yeni Enum: CheckoutSteps

```csharp
public static class CheckoutSteps
{
    public static readonly TypeItem ServiceRequested = new(1, "ServiceRequested", "Enum.CheckoutStep.ServiceRequested");
    public static readonly TypeItem WaitingForQuotes = new(2, "WaitingForQuotes", "Enum.CheckoutStep.WaitingForQuotes");
    public static readonly TypeItem QuotesReceived = new(3, "QuotesReceived", "Enum.CheckoutStep.QuotesReceived");
    public static readonly TypeItem QuotesSelected = new(4, "QuotesSelected", "Enum.CheckoutStep.QuotesSelected");
    public static readonly TypeItem FinancingPending = new(5, "FinancingPending", "Enum.CheckoutStep.FinancingPending");
    public static readonly TypeItem PaymentPending = new(6, "PaymentPending", "Enum.CheckoutStep.PaymentPending");
    public static readonly TypeItem Completed = new(7, "Completed", "Enum.CheckoutStep.Completed");
}
```

---

## 5. API Değişiklikleri

### 5.1 Yeni Endpoint'ler

```
POST /api/orders/{id}/trigger-survey
  - Lojistik seçildikten sonra manuel tetikleme

POST /api/orders/{id}/trigger-financing
  - Tüm servisler seçildikten sonra finansman talebi oluştur

GET /api/orders/{id}/flow-status
  - Sipariş akış durumunu getir (hangi aşamada, ne bekleniyor)
```

### 5.2 Mevcut Endpoint Değişiklikleri

```
PUT /api/orders/quotes/{id}/accept
  - Ekleme: Lojistik ise → Survey tetikle
  - Ekleme: Tüm servisler seçili mi kontrol et → Finansman modal tetikle
```

---

## 6. Test Senaryoları

### 6.1 Gözetim Tetikleme

| # | Senaryo | Beklenen Sonuç |
|---|---------|----------------|
| 1 | Lojistik seçilmeden gözetim istendi | Gözetim request oluşmaz, uyarı göster |
| 2 | Lojistik seçildi, gözetim istenmişti | Otomatik Survey request oluşur |
| 3 | Lojistik iptal edildi | Survey request iptal edilir |

### 6.2 Finansman Tetikleme

| # | Senaryo | Beklenen Sonuç |
|---|---------|----------------|
| 1 | Tüm servisler seçildi, finansman istendi | FinancingRequest oluşur |
| 2 | Toplam < MIN_AMOUNT | Finansman opsiyonu gösterilmez |
| 3 | Firma verified değil | Risk skoru yüksek |

---

## 7. İlerleme Takibi

### Tamamlanan Maddeler
- [x] Mevcut akış analizi (2026-01-26)
- [x] Hedef akış tasarımı (2026-01-26)
- [x] Bu dokümanın oluşturulması (2026-01-26)
- [x] **Faz 1: Temel Altyapı** (2026-01-26)
  - Order: RequiresFinancing, SurveyTriggerStatus, AllServicesSelectedAt, FinancingRequestId
  - OrderServiceRequest: DependsOnServiceRequestId, AutoCreated, TriggerSource
  - TypeDefinitions: SurveyTriggerStatuses enum
  - Migration: AddOrderFlowManagement
- [x] **Faz 2: Gözetim Tetikleme** (2026-01-26)
  - IOrderOrchestrationService interface
  - OrderOrchestrationService implementasyonu
  - CheckoutService: Lojistik + Survey koşullu mantık
  - OrderService: Teklif kabul sonrası orchestration
- [x] **Faz 3: Finansman Tetikleme** (2026-01-26)
  - QuoteAcceptedResult ve FinancingStatusDto DTO'ları
  - OnServiceQuoteAcceptedAsync: Tam sonuç döner
  - TriggerFinancingIfNeededAsync: Otomatik FinancingRequest oluşturma
  - RequestFinancingAsync: Manuel finansman talebi
  - GetFinancingStatusAsync: Finansman durumu sorgulama
  - API: financing-status, request-financing endpoint'leri
- [x] **Faz 4: Risk Hesaplama** (2026-01-26)
  - IRiskScoringService interface ve RiskScoringService implementasyonu
  - 7 faktörlü ağırlıklı risk hesaplama algoritması
  - RiskScoreResult ve RiskFactorDto DTO'ları
  - FinancingRequest oluşturulurken otomatik risk skoru
  - API: risk-score, recalculate-risk, vendor risk-score endpoint'leri
  - Risk seviyeleri: Low (0-25), Medium (26-50), High (51-75), VeryHigh (76-100)
- [x] **Faz 5: UI İyileştirmeleri - Temel** (2026-01-26)
  - Seçilmemiş teklif uyarısı (unselectedQuotesModal)
  - Finansman seçeneği modalı (financingOptionModal)
  - Finansman başarı modalı (financingSuccessModal)
  - JavaScript: proceedToCheckout, checkFinancingAvailability, requestFinancing
  - **Checkout Step Tracking:**
    - Order.CheckoutStep field ve CheckoutSteps enum
    - CheckoutProgressDto, CheckoutStepInfo DTO'ları
    - API: GET /api/orders/{orderId}/checkout-progress
    - Checkout timeline UI görsel göstergesi
    - Otomatik adım güncellemeleri (sipariş oluşturma, teklif gelme, teklif seçme)

### Devam Eden Maddeler
- [x] **Faz 5: UI İyileştirmeleri - Checkout Wizard** (2026-01-27)
  - Fullscreen modal ile checkout wizard implementasyonu
  - 4 adımlı wizard: Teslimat → Teklifler → Finansman → Özet/Ödeme
  - Wizard step navigation ve state management
  - Adım bazlı validasyon (teklif seçimi uyarısı)
  - Finansman seçeneği wizard içine entegre
  - CSS: wizard-steps, wizard-step-icon stilleri
  - JavaScript: wizardStep, wizardNextStep, wizardPrevStep, wizardStep2Next, completeWizard

- [x] **Faz 5: UI İyileştirmeleri - Timeline Sayfası** (2026-01-27)
  - `/Orders/Timeline/{id}` sipariş takip sayfası
  - Timeline görsel tasarımı (completed/current/pending states)
  - Sipariş bilgileri, katılımcılar, hizmet talepleri
  - Kargo takip bilgileri
  - Teslimat onayı ve değerlendirme aksiyonları
  - MyOrders'dan "Takip" butonu ile erişim

- [x] **Faz 5: UI İyileştirmeleri - SignalR Bildirimleri** (2026-01-27)
  - Teklif oluşturulduğunda alıcıya real-time bildirim
  - OrderService.CreateServiceQuoteAsync → NotificationService.CreateAsync
  - SignalR Hub: /hubs/notifications
  - Frontend: DashboardLayout'ta ReceiveNotification event handler
  - Toastr ile bildirim gösterimi

### Notlar
- Her faz tamamlandığında bu doküman güncellenecek
- Major değişiklikler için PROJECT_STATUS.xml de güncellenecek
- **Önemli:** Servis teklifi seçimi zorunlu değil - checkout'ta sadece uyarı gösterilecek

---

## 8. Referanslar

- [ORDER_SYSTEM_PLAN.md](./ORDER_SYSTEM_PLAN.md) - Entity ve API detayları
- [GUMRUK_BEYANNAME_REHBERI.md](./GUMRUK_BEYANNAME_REHBERI.md) - Gümrük entegrasyonu
- [DEVELOPMENT_PATTERNS.md](../DEVELOPMENT_PATTERNS.md) - Kod standartları
