# INCOTERMS + FINANSMAN SISTEMI ANALIZ DOKUMANI

**Tarih:** 2025-12-28
**Durum:** Implementasyon devam ediyor

---

## 1. GENEL BAKIS

### 1.1 Amac
Uluslararasi ticarette Incoterms 2020 kurallarini sisteme entegre ederek:
- Alici ve saticinin sorumluluklarini netlestirilmesi
- Sorumluluga gore finansman talebi olusturabilmesi
- Servis saglayici (lojistik, gumruk, sigorta) tekliflerinin dogru tarafa yonlendirilmesi

### 1.2 Kapsam
- 11 Incoterm terimi (7 tum modlar + 4 deniz)
- 6 sorumluluk kalemi
- 5 finansman konusu
- Checkout flow guncellemesi
- Siparis detay sayfasi guncellemesi

---

## 2. INCOTERMS 2020 TAM REFERANS

### 2.0 INCOTERMS ACILIMLARI VE ANLAMLARI

| Kod | Ingilizce Acilim | Turkce Anlam |
|-----|------------------|--------------|
| **EXW** | **Ex** **W**orks | Is yerinde (fabrikada/depoda) teslim |
| **FCA** | **F**ree **C**arrier | Tasiyiciya teslim edilmis (serbest) |
| **CPT** | **C**arriage **P**aid **T**o | Tasima (navlun) odenmis olarak |
| **CIP** | **C**arriage and **I**nsurance **P**aid to | Tasima ve sigorta odenmis olarak |
| **DAP** | **D**elivered **A**t **P**lace | Belirlenen yerde teslim edilmis |
| **DPU** | **D**elivered at **P**lace **U**nloaded | Belirlenen yerde bosaltilmis teslim |
| **DDP** | **D**elivered **D**uty **P**aid | Gumruk vergisi odenmis teslim |
| **FAS** | **F**ree **A**longside **S**hip | Gemi bordasi boyunca serbest |
| **FOB** | **F**ree **O**n **B**oard | Gemi guvertesinde serbest |
| **CFR** | **C**ost and **Fr**eight | Mal bedeli ve navlun dahil |
| **CIF** | **C**ost, **I**nsurance and **F**reight | Mal bedeli, sigorta ve navlun dahil |

### 2.1 Tum Tasima Modlari Icin (7 Terim)

| Kod | Ingilizce | Turkce | Aciklama |
|-----|-----------|--------|----------|
| **EXW** | Ex Works | Is Yerinde Teslim | Satici mali kendi tesisinde hazir eder. Alici tum tasima, sigorta ve gumruk masraflarini ustlenir. En az sorumluluk saticida. |
| **FCA** | Free Carrier | Tasiyiciya Teslim | Satici mali belirtilen yerde tasiyiciya teslim eder ve ihracat gumrugunu yapar. Risk bu noktada aliciya gecer. |
| **CPT** | Carriage Paid To | Tasima Odenmis | Satici navlunu oder ama risk tasiyiciya teslimde gecer. Sigorta alicinin sorumlulugunuda. |
| **CIP** | Carriage and Insurance Paid To | Tasima ve Sigorta Odenmis | CPT gibi ama satici ayrica sigorta da yaptirir. Sigorta tum riskleri kapsar (Institute Cargo Clauses A). |
| **DAP** | Delivered at Place | Belirlenen Yerde Teslim | Satici mali varis noktasinda bosaltmaya hazir teslim eder. Ithalat gumrugu alicida. |
| **DPU** | Delivered at Place Unloaded | Belirlenen Yerde Bosaltilmis Teslim | Satici mali varis noktasinda bosaltarak teslim eder. Tek bosaltma sorumlulugu saticida olan terim. |
| **DDP** | Delivered Duty Paid | Gumruk Vergileri Odenmis Teslim | Satici tum masraflari ustlenir: tasima, sigorta, ihracat/ithalat gumruk, vergiler. En fazla sorumluluk saticida. |

### 2.2 Sadece Deniz/Ic Su Tasimaciligi Icin (4 Terim)

| Kod | Ingilizce | Turkce | Aciklama |
|-----|-----------|--------|----------|
| **FAS** | Free Alongside Ship | Gemi Dogrultusunda Teslim | Satici mali yukleme limaninda geminin yanina getirir. Gemiye yukleme ve sonrasi alicinin sorumlulugunda. |
| **FOB** | Free on Board | Gemi Bordasinda Teslim | Satici mali gemiye yukler, risk gemi guvertesinde aliciya gecer. En yaygin kullanilan deniz terimidir. |
| **CFR** | Cost and Freight | Mal Bedeli ve Navlun | Satici navlunu oder ama risk gemiye yuklemede gecer. Sigorta alicinin sorumlulugunda. |
| **CIF** | Cost, Insurance and Freight | Mal Bedeli, Sigorta ve Navlun | CFR gibi ama satici ayrica sigorta da yaptirir. Minimum sigorta yeterli (Institute Cargo Clauses C). |

### 2.3 Detayli Aciklamalar

#### EXW - Ex Works (Is Yerinde Teslim)
- **Anlami:** "Ex" = disarida, cikis. "Works" = fabrika, is yeri
- **Ne demek:** Mal fabrikanin/deponun kapisinda hazir
- **Satici:** Sadece mali hazirlar, baska hicbir sey yapmaz
- **Alici:** Yukleme, tasima, sigorta, ihracat gumruk, ithalat gumruk HER SEY alicida
- **Kullanim:** Minimum satici sorumlulugu istenen durumlarda

#### FOB - Free On Board (Gemi Bordasinda Teslim)
- **Anlami:** "Free" = serbest (sorumluluktan). "On Board" = gemi guvertesinde
- **Ne demek:** Mal gemi guvertesine yuklenince satici serbest
- **Satici:** Mali hazirlar, ihracat gumruk yapar, gemiye yukler
- **Alici:** Navlun, sigorta, ithalat gumruk, bosaltma
- **Kullanim:** En yaygin deniz ticareti terimi

#### CIF - Cost, Insurance and Freight (Maliyet, Sigorta ve Navlun)
- **Anlami:** "Cost" = mal bedeli, "Insurance" = sigorta, "Freight" = navlun
- **Ne demek:** Satici mal bedelini, sigortayi ve navlunu karsilar
- **Satici:** Mal + ihracat gumruk + yukleme + navlun + sigorta
- **Alici:** Sadece ithalat gumruk ve bosaltma
- **Kullanim:** Alicinin sadece ithalat islemleriyle ugrasmasi istendiginde

#### DDP - Delivered Duty Paid (Gumruk Vergisi Odenmis Teslim)
- **Anlami:** "Delivered" = teslim edilmis, "Duty" = vergi, "Paid" = odenmis
- **Ne demek:** Her sey satici tarafindan odenmis, kapiya teslim
- **Satici:** MAL + ihracat + navlun + sigorta + ithalat + vergiler = HER SEY
- **Alici:** Sadece kabulunu yapar
- **Kullanim:** Maksimum satici sorumlulugu istenen durumlarda

---

## 3. SORUMLULUK MATRISI

### 3.1 Tum Modlar

| Sorumluluk | EXW | FCA | CPT | CIP | DAP | DPU | DDP |
|------------|:---:|:---:|:---:|:---:|:---:|:---:|:---:|
| **Ihracat Gumruk** | A | S | S | S | S | S | S |
| **Yukleme** | A | S | S | S | S | S | S |
| **Ana Tasima (Navlun)** | A | A | S | S | S | S | S |
| **Sigorta** | A | A | A | S | - | - | S |
| **Ithalat Gumruk** | A | A | A | A | A | A | S |
| **Bosaltma** | A | A | A | A | A | S | S |

**S** = Satici Sorumlu | **A** = Alici Sorumlu | **-** = Opsiyonel

### 3.2 Deniz Tasimaciligi

| Sorumluluk | FAS | FOB | CFR | CIF |
|------------|:---:|:---:|:---:|:---:|
| **Ihracat Gumruk** | S | S | S | S |
| **Yukleme** | A | S | S | S |
| **Ana Tasima (Navlun)** | A | A | S | S |
| **Sigorta** | A | A | A | S |
| **Ithalat Gumruk** | A | A | A | A |
| **Bosaltma** | A | A | A | A |

### 3.3 Risk Transfer Noktalari

| Incoterm | Risk Nerede Aliciya Gecer? |
|----------|---------------------------|
| **EXW** | Saticinin tesisinde, mal hazir oldugunda |
| **FCA** | Tasiyiciya teslim edildiginde |
| **FAS** | Mal gemi yanina getirildiginde |
| **FOB** | Mal gemi guvertesine yuklendiginde |
| **CFR** | Mal gemi guvertesine yuklendiginde (navlun saticida olsa da) |
| **CIF** | Mal gemi guvertesine yuklendiginde (navlun+sigorta saticida olsa da) |
| **CPT** | Ilk tasiyiciya teslim edildiginde |
| **CIP** | Ilk tasiyiciya teslim edildiginde |
| **DAP** | Varis noktasinda, bosaltmaya hazir oldugunda |
| **DPU** | Varis noktasinda, bosaltildiktan sonra |
| **DDP** | Varis noktasinda, bosaltmaya hazir oldugunda |

---

## 4. FINANSMAN PERSPEKTIFI

### 4.1 Kim Hangi Kalem Icin Finansman Isteyebilir?

| Incoterm | Satici Finansman Isteyebilecegi Kalemler | Alici Finansman Isteyebilecegi Kalemler |
|----------|------------------------------------------|----------------------------------------|
| **EXW** | Mal bedeli | Lojistik, Ihracat Gumruk, Sigorta, Ithalat Gumruk |
| **FCA** | Mal bedeli, Ihracat Gumruk | Lojistik, Sigorta, Ithalat Gumruk |
| **FAS** | Mal bedeli, Ihracat Gumruk | Yukleme, Lojistik, Sigorta, Ithalat Gumruk |
| **FOB** | Mal bedeli, Ihracat Gumruk | Lojistik, Sigorta, Ithalat Gumruk |
| **CFR** | Mal bedeli, Ihracat Gumruk, Lojistik | Sigorta, Ithalat Gumruk |
| **CIF** | Mal bedeli, Ihracat Gumruk, Lojistik, Sigorta | Ithalat Gumruk |
| **CPT** | Mal bedeli, Ihracat Gumruk, Lojistik | Sigorta, Ithalat Gumruk |
| **CIP** | Mal bedeli, Ihracat Gumruk, Lojistik, Sigorta | Ithalat Gumruk |
| **DAP** | Mal bedeli, Ihracat Gumruk, Lojistik | Ithalat Gumruk, Bosaltma |
| **DPU** | Mal bedeli, Ihracat Gumruk, Lojistik, Bosaltma | Ithalat Gumruk |
| **DDP** | Mal bedeli, Ihracat Gumruk, Lojistik, Sigorta, Ithalat Gumruk | - |

### 4.2 Finansman Konulari (FinancingSubjects)

| ID | Kod | Aciklama |
|----|-----|----------|
| 1 | ProductCost | Mal Bedeli |
| 2 | Logistics | Lojistik/Navlun |
| 3 | Insurance | Sigorta |
| 4 | ExportCustoms | Ihracat Gumruk |
| 5 | ImportCustoms | Ithalat Gumruk |

---

## 5. TEKNIK IMPLEMENTASYON

### 5.1 Entity Degisiklikleri

#### Order.cs
```csharp
// Yeni alanlar
public int? IncotermId { get; set; }
public string? IncotermLocation { get; set; }
```

#### FinancingRequest.cs
```csharp
// Yeni alan
public int? FinancingSubject { get; set; }
```

### 5.2 TypeDefinitions.cs Eklemeleri

```csharp
// Incoterms - 11 terim
public static class Incoterms { ... }

// Sorumluluk Tipleri - 6 kalem
public static class IncotermResponsibilityTypes { ... }

// Sorumluluk Matrisi - Kim ne yapar?
public static class IncotermResponsibilities { ... }

// Finansman Konulari - 5 kalem
public static class FinancingSubjects { ... }
```

### 5.3 API Endpoint'leri

```
GET  /api/types/incoterms                         - Incoterm listesi
GET  /api/types/incoterms/{id}                    - Incoterm detay
GET  /api/types/incoterms/{id}/responsibilities   - Sorumluluk matrisi
GET  /api/types/financing-subjects                - Finansman konulari

POST /api/checkout/initiate                       - Guncellendi (IncotermId ekle)
GET  /api/orders/{id}/responsibilities            - Siparis sorumluluklar
```

### 5.4 DTO'lar

```csharp
// Incoterm detay
public class IncotermDto
{
    public int Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Icon { get; set; }
    public string CssClass { get; set; }
    public bool IsSeaOnly { get; set; }
}

// Sorumluluk matrisi
public class IncotermResponsibilitiesDto
{
    public int IncotermId { get; set; }
    public string IncotermCode { get; set; }
    public List<ResponsibilityItemDto> SellerResponsibilities { get; set; }
    public List<ResponsibilityItemDto> BuyerResponsibilities { get; set; }
}

public class ResponsibilityItemDto
{
    public int TypeId { get; set; }
    public string Name { get; set; }
    public string Icon { get; set; }
    public bool CanRequestFinancing { get; set; }
    public int? FinancingSubjectId { get; set; }
}

// Checkout guncelleme
public class InitiateCheckoutDto
{
    // ... mevcut alanlar
    public int IncotermId { get; set; }
    public string? IncotermLocation { get; set; }
}
```

---

## 6. UI/UX AKISI

### 6.1 Checkout Flow (5 Adim)

```
ADIM 1: SEPET OZETI
├── Urunler listesi
├── Teslimat adresi secimi
└── [Ileri]

ADIM 2: INCOTERM SECIMI
├── Incoterm dropdown
├── Incoterm lokasyonu
├── Sorumluluk tablosu (dinamik)
│   ├── Satici sorumlu kalemler
│   └── Alici sorumlu kalemler
├── Bilgi notu
└── [Ileri]

ADIM 3: HIZMET SECIMI
├── Sorumlu olunan hizmetler icin teklif isteme
│   ├── Lojistik (eger alici sorumluysa)
│   ├── Sigorta (eger alici sorumluysa)
│   └── Gumruk (eger alici sorumluysa)
└── [Ileri]

ADIM 4: FINANSMAN TALEBI
├── Sorumlu olunan kalemler icin finansman secimi
│   ├── Mal bedeli icin finansman
│   ├── Lojistik icin finansman
│   └── Gumruk icin finansman
├── Tutar ve vade bilgileri
└── [Ileri]

ADIM 5: ONAY
├── Siparis ozeti
├── Secilen Incoterm
├── Sorumluluklar
├── Hizmetler
├── Finansman talepleri
└── [Siparisi Olustur]
```

### 6.2 Incoterm Secim Komponenti

```
┌─────────────────────────────────────────────────────────────────┐
│ Teslimat Sartlari (Incoterm) *                                  │
│ [▼ FOB - Free on Board                                        ] │
│                                                                 │
│ Incoterm Lokasyonu *                                           │
│ [Istanbul Port                                                ] │
│                                                                 │
│ ┌─────────────────────────────────────────────────────────────┐ │
│ │ ℹ️ FOB (Free on Board) Nedir?                               │ │
│ │                                                             │ │
│ │ Satici mali gemiye yukleyene kadar tum masraf ve           │ │
│ │ risklerden sorumludur. Mal gemiye yuklendigi anda          │ │
│ │ sorumluluk aliciya gecer.                                  │ │
│ │                                                             │ │
│ │ ⚠️ NOT: Bu terim sadece deniz tasimaciligi icin gecerlidir.│ │
│ └─────────────────────────────────────────────────────────────┘ │
│                                                                 │
│ ┌─────────────────────────────────────────────────────────────┐ │
│ │                   SORUMLULUK DAGILIMI                       │ │
│ │                                                             │ │
│ │   📦 Satici Sorumlu        │    🛒 Alici Sorumlu           │ │
│ │   ─────────────────        │    ─────────────────           │ │
│ │   ✓ Ihracat gumruk         │    ✓ Ana tasima (navlun)      │ │
│ │   ✓ Yukleme                │    ✓ Sigorta                  │ │
│ │                            │    ✓ Ithalat gumruk           │ │
│ │                            │    ✓ Bosaltma                 │ │
│ │                                                             │ │
│ │   💰 Finansman icin:       │    💰 Finansman icin:         │ │
│ │   • Mal bedeli             │    • Navlun masrafi           │ │
│ │   • Ihracat gumruk         │    • Sigorta primi            │ │
│ │                            │    • Ithalat gumruk           │ │
│ └─────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
```

### 6.3 Satici Perspektifi (Siparis Geldiginde)

```
┌─────────────────────────────────────────────────────────────────┐
│ Siparis: ORD-2024-00123                                        │
│ Alici: ABC Ltd.                                                │
│ Incoterm: CIF Istanbul Port                                    │
│                                                                │
│ ═════════════════════════════════════════════════════════════  │
│ 📋 SIZIN SORUMLULUKLARINIZ (CIF)                              │
│ ─────────────────────────────────────────────────────────────  │
│ ✓ Mali hazirla                                                │
│ ✓ Ihracat gumruk islemlerini yap                              │
│ ✓ Yuklemeyi gerceklestir                                      │
│ ✓ Navlun ode (ana tasima)                                     │
│ ✓ Sigorta yaptir                                              │
│ ═════════════════════════════════════════════════════════════  │
│                                                                │
│ 💰 FINANSMAN TALEBI OLUSTUR                                   │
│ Sorumlu oldugunuz kalemler icin finansman isteyebilirsiniz:   │
│                                                                │
│ □ Mal Bedeli: 50,000 USD                                      │
│ □ Navlun: ~3,000 USD (tahmini)                                │
│ □ Sigorta: ~500 USD (tahmini)                                 │
│ □ Ihracat Gumruk: ~1,000 USD (tahmini)                        │
│                                                                │
│ [Finansman Talep Et]                                          │
└─────────────────────────────────────────────────────────────────┘
```

---

## 7. TEST SENARYOLARI

| # | Senaryo | Incoterm | Alici Gorevi | Satici Gorevi |
|---|---------|----------|--------------|---------------|
| 1 | Minimum satici | EXW | Tum tasima + tum gumruk | Sadece mal hazirla |
| 2 | Deniz ticareti | FOB | Navlun + sigorta + ithalat | Ihracat + yukleme |
| 3 | Satici navlun + sigorta | CIF | Sadece ithalat gumruk | Navlun + sigorta + ihracat |
| 4 | Kapiya teslim | DDP | Hicbir sey | Tumu |
| 5 | Alici finansman | FOB | Navlun icin finansman | Mal bedeli icin finansman |
| 6 | Satici finansman | CIF | Ithalat icin finansman | Navlun + sigorta icin finansman |

---

## 8. MIGRATION PLANI

### Adim 1: TypeDefinitions (TAMAMLANDI)
- Incoterms static class
- IncotermResponsibilities static class
- FinancingSubjects static class

### Adim 2: Entity Degisiklikleri (TAMAMLANDI)
- Order.IncotermId (int?)
- Order.IncotermLocation (string?)
- FinancingRequest.FinancingSubject (int?)

### Adim 3: Migration
- AddIncotermToOrders migration

### Adim 4: API
- TypesApiController endpoint'leri
- CheckoutApiController guncellemesi

### Adim 5: UI
- Checkout wizard Incoterm adimi
- Sorumluluk matrisi komponenti
- Finansman formuna konu secimi

---

## 9. NOTLAR

### 9.1 Onemli Noktalar
- Incoterm secimi siparis olusturulurken zorunlu olmali (yurt ici satislarda opsiyonel olabilir)
- Deniz terimleri (FAS, FOB, CFR, CIF) sadece deniz/ic su tasimaciligi secildiginde gosterilmeli
- Sorumluluk tablosu Incoterm degistiginde dinamik olarak guncellenmeli
- Finansman talepleri sadece sorumlu olunan kalemler icin olusturulabilmeli

### 9.2 Gelecek Gelistirmeler
- Incoterm bazli otomatik gorev atama
- Sorumluluk bazli belge sablonlari
- Risk transfer noktasi takibi
- Incoterm degisikligi talebi (siparis sonrasi)

---

**Son Guncelleme:** 2025-12-28
