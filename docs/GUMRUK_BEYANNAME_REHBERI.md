# Gumruk Beyannamesi Hazirlik Rehberi

Bu rehber, Bridgo platformunda gumruk beyannamesi icin gerekli bilgilerin nasil doldurulacagini aciklar.

---

## 1. URUN BILGILERI (Product)

Urun tanimlarken asagidaki alanlarin dogru doldurulmasi gumruk islemleri icin kritiktir.

### 1.1 GTIP Kodu (HS Code)

**Alan:** `HSCode`
**Beyanname Kutusu:** 33
**Zorunlu:** Evet

**Nedir?**
- Gumruk Tarife Istatistik Pozisyonu
- Uluslararasi standart urun siniflandirma kodu
- Ilk 6 hane tum dunyada ayni, sonraki haneler ulkeye ozel

**Nasil Bulunur?**
1. [Ticaret Bakanligi TAREKS](https://uygulama.gtb.gov.tr/Tara) sisteminden arama
2. [AB TARIC](https://ec.europa.eu/taxation_customs/dds2/taric/) veritabanindan
3. Gumruk musavirinizden yardim alin

**Ornekler:**
| Urun | GTIP Kodu |
|------|-----------|
| Kavrulmus kahve | 0901.21.00 |
| Pamuklu t-shirt | 6109.10.00 |
| Dizustu bilgisayar | 8471.30.00 |
| Civata (demir/celik) | 7318.15.90 |

**Dikkat:**
- Yanlis GTIP = Yanlis vergi orani = Ceza riski
- GTIP'e gore ek belgeler gerekebilir (CE, TSE, vb.)
- Bazi GTIP'ler ithalat/ihracat izni gerektirir

---

### 1.2 Mense Ulke (Country of Origin)

**Alan:** `CountryOfOrigin`
**Beyanname Kutusu:** 34
**Zorunlu:** Evet

**Nedir?**
- Urunun uretildigi/imal edildigi ulke
- ISO 3166-1 alpha-2 kodu (2 harf)

**Onemli Noktalar:**
- Sadece son isleme yapilan ulke degil, urunun IMAL edildigi ulke
- Tercihli tarife (STA) uygulamasi icin kritik
- ATR, EUR.1 belgelerinde belirtilir

**Yaygin Kodlar:**
| Ulke | Kod |
|------|-----|
| Turkiye | TR |
| Cin | CN |
| Almanya | DE |
| ABD | US |
| Italya | IT |
| Japonya | JP |

---

### 1.3 Marka ve Model

**Alanlar:** `Brand`, `Model`
**Zorunlu:** Gumruk icin Evet

**Neden Onemli?**
- Beyannamede urun taniminin parcasi
- Sahtecilik/taklit urun tespiti
- Marka tescil kontrolu

**Nasil Doldurulur?**
- Marka: Urunun tescilli markasi (Samsung, Bosch, vb.)
- Model: Uretici model kodu (SM-A546E, GWS 18V-LI, vb.)
- Markasiz urunlerde "MARKASIZ" veya "GENERIC" yazilir

---

## 2. FIRMA BILGILERI (Vendor)

### 2.1 Vergi Numarasi (VKN)

**Alan:** `TaxNumber`
**Beyanname Kutusu:** 2, 8, 14
**Zorunlu:** Evet

**Format:**
- Tuzel kisi: 10 haneli VKN
- Gercek kisi: 11 haneli TCKN

---

### 2.2 EORI Numarasi

**Alan:** `EoriNumber`
**Zorunlu:** AB ile ticarette Evet

**Nedir?**
- Economic Operators Registration and Identification
- AB gumruk islemlerinde zorunlu kimlik numarasi

**Nasil Alinir?**
1. Ticaret Bakanligi TAREKS sistemine basvuru
2. Format: TR + VKN (ornek: TR1234567890)

**Ne Zaman Gerekli?**
- AB ulkelerine ihracat
- AB ulkelerinden ithalat
- Transit gecisler

---

### 2.3 e-Fatura ve KEP

**Alanlar:** `EInvoiceId`, `KepAddress`

**e-Fatura PK/GB:**
- e-Fatura mukelleflerine fatura gondermek icin gerekli
- Format: urn:mail:xxx@hs01.kep.tr (KEP) veya GGGGBBBBBB (Ozel Entegrator)

**KEP Adresi:**
- Resmi elektronik tebligat icin
- Gumruk idaresinden bildirimler buraya gelir

---

### 2.4 Yetkili Kisi Bilgileri

**Alanlar:** `AuthorizedPersonName`, `AuthorizedPersonTaxNo`
**Beyanname Kutusu:** 54 (Beyan sahibi)

**Onemli:**
- Beyanname imzalama yetkisi olan kisi
- Sirket adina hareket yetkisi belgelenmeli
- VKN/TCKN gumruk sisteminde tanimli olmali

---

## 3. SIPARIS / BEYANNAME BILGILERI (Order)

### 3.1 Incoterm (Teslim Sekli)

**Alan:** `IncotermId`
**Beyanname Kutusu:** 20
**Zorunlu:** Evet

**Incoterms 2020:**

| Kod | Aciklama | Navlun | Sigorta | Gumruk (Ihracat) | Gumruk (Ithalat) |
|-----|----------|--------|---------|------------------|------------------|
| EXW | Ex Works | Alici | Alici | Alici | Alici |
| FCA | Free Carrier | Alici | Alici | Satici | Alici |
| FOB | Free On Board | Alici | Alici | Satici | Alici |
| CIF | Cost Insurance Freight | Satici | Satici | Satici | Alici |
| DDP | Delivered Duty Paid | Satici | Satici | Satici | Satici |

**Secim Onerileri:**
- Ilk kez ihracat: **FCA** veya **FOB** (daha az risk)
- Deneyimli ihracatci: **CIF** veya **DDP** (daha fazla kontrol)
- Mikro ihracat (ETGB): Genellikle **DAP** veya **DDP**

---

### 3.2 Navlun ve Sigorta

**Alanlar:** `FreightAmount`, `InsuranceAmount`
**Beyanname Kutusu:** 45 (Duzeltme)

**Navlun (Freight):**
- Tasima ucreti
- Incoterm'e gore satici veya alici odemeli
- CIF ve CIP'de fiyata dahil

**Sigorta (Insurance):**
- Nakliye sigortasi
- CIF'te minimum %110 deger uzerinden
- Sigorta policesi numarasi beyannamede belirtilir

**CIF Hesaplama:**
```
CIF = FOB + Navlun + Sigorta
```

---

### 3.3 Doviz Kuru

**Alanlar:** `ExchangeRate`, `ExchangeRateDate`
**Beyanname Kutusu:** 23 (Doviz kuru)

**Nasil Belirlenir?**
- TCMB efektif SATIS kuru kullanilir
- Beyanname tescil tarihinden ONCEKI gun kuru
- Resmi Gazete'de yayinlanan kur

**Ornek:**
- Tescil tarihi: 15 Ocak 2025
- Kullanilacak kur: 14 Ocak 2025 TCMB efektif satis

---

### 3.4 Istatistiki Kiymet

**Alan:** `StatisticalValue`
**Beyanname Kutusu:** 46

**Hesaplama:**
- **Ihracat:** FOB deger / USD kuru = USD cinsinden FOB
- **Ithalat:** CIF deger / USD kuru = USD cinsinden CIF

**Ornek (Ihracat):**
```
Fatura tutari: 10.000 EUR
EUR/TRY kuru: 35.50
FOB/TRY: 355.000 TRY
USD/TRY kuru: 34.00
Istatistiki Kiymet: 355.000 / 34.00 = 10.441,18 USD
```

---

### 3.5 Gumruk Rejim Kodu

**Alan:** `CustomsRegimeCode`
**Beyanname Kutusu:** 37

**Yaygin Rejimler:**

| Kod | Rejim | Aciklama |
|-----|-------|----------|
| 1000 | Serbest Dolasima Giris | Normal ithalat |
| 1040 | Dahilde Isleme | Uretim icin ithalat (vergisiz) |
| 1007 | Antrepo | Antrepoya aliş |
| 3151 | Kesin Ihracat | Normal ihracat |
| 3141 | Gecici Ihracat | Geri gelecek esya |
| 3171 | Haric Isleme | Yurtdisinda isleme |

---

### 3.6 Tasima Sekli

**Alan:** `TransportModeCode`
**Beyanname Kutusu:** 25

| Kod | Tasima Sekli |
|-----|--------------|
| 1 | Deniz yolu |
| 2 | Demiryolu |
| 3 | Karayolu |
| 4 | Havayolu |
| 5 | Posta |
| 7 | Boru hatti |
| 9 | Diger |

---

### 3.7 Ihracat Tipi

**Alan:** `ExportType`

| Deger | Tip | Aciklama |
|-------|-----|----------|
| 1 | Normal Ihracat | Gumruk beyannamesi ile |
| 2 | Mikro Ihracat (ETGB) | 30.000 EUR ve 600 kg altinda |
| 3 | Hizmet Ihracati | Fiziki esya yok |

**Mikro Ihracat (ETGB) Avantajlari:**
- Gumruk musaviri gerektirmez
- Daha dusuk maliyetler
- Hizli islem (1-2 gun)
- E-ticaret icin ideal

---

## 4. SIPARIS KALEMI BILGILERI (OrderItem)

Her kalem icin ayri beyanname satiri olusur.

### 4.1 Kalem Bazli GTIP

**Alan:** `HSCode` (OrderItem)

- Urun tanindan otomatik kopyalanir
- Gerekirse override edilebilir (farkli varyant)

### 4.2 Agirlik Bilgileri

**Alanlar:** `GrossWeight`, `NetWeight`
**Beyanname Kutusu:** 35 (Brut), 38 (Net)

**Tanimlar:**
- **Brut Agirlik:** Ambalaj DAHIL toplam agirlik (kg)
- **Net Agirlik:** Sadece urun agirligi (kg)

**Ornek:**
- 100 adet urun, her biri 0.5 kg = 50 kg net
- Kutu ve paketleme 5 kg = 55 kg brut

### 4.3 Tamamlayici Olcu Birimi

**Alanlar:** `SupplementaryQuantity`, `SupplementaryUnit`
**Beyanname Kutusu:** 41

Bazi GTIP'ler icin ek olcu birimi zorunludur:

| GTIP Ornegi | Tamamlayici Birim |
|-------------|-------------------|
| Alkol | Saf alkol litresi |
| Sigara | 1000 adet |
| Ayakkabi | Cift |
| Tekstil | m2 veya adet |

---

## 5. BELGE GEREKSINIMLERI

Beyanname ekinde sunulmasi gereken belgeler:

### 5.1 Zorunlu Belgeler

| Belge | Aciklama |
|-------|----------|
| Ticari Fatura | Urun, miktar, fiyat bilgileri |
| Packing List | Ambalaj ve agirlik detaylari |
| Konşimento/CMR | Tasima belgesi |
| ATR/EUR.1 | Tercihli mense belgesi (gerekirse) |

### 5.2 Urune Gore Ek Belgeler

| Urun Grubu | Gerekli Belge |
|------------|---------------|
| Gida | Saglik sertifikasi, Analiz raporu |
| Elektronik | CE belgesi, Test raporu |
| Tekstil | Kompozisyon analizi |
| Kimyasal | MSDS, Guvenlik bilgi formu |
| Tibbi cihaz | Bakanlik izni |

---

## 6. ETGB / MIKRO IHRACAT

### 6.1 Limitler (2025)

| Kriter | Limit |
|--------|-------|
| Deger | 30.000 EUR |
| Agirlik | 600 kg |
| Gonderi basi | Tek alici |

### 6.2 Gerekli Bilgiler

ETGB icin minimum bilgiler:
- Gonderici VKN ve iletisim
- Alici adi, adresi, telefon
- Urun aciklamasi (Turkce yeterli)
- GTIP (6 hane yeterli)
- Fatura tutari ve doviz cinsi
- Incoterm

### 6.3 Islem Akisi

```
1. Urunu hazirla
2. e-Arsiv fatura kes (KDV'siz)
3. Kargo firmasina teslim et (DHL, UPS, FedEx, vb.)
4. Kargo firmasi ETGB'yi olusturur
5. Gumruk onayindan sonra gonderi hareket eder
6. ETGB numarasini faturaya ekle
7. KDV iadesi icin basvur (istege bagli)
```

---

## 7. SIKCA SORULAN SORULAR

### GTIP kodunu nereden bulabilirim?
- [TAREKS](https://uygulama.gtb.gov.tr/Tara) sisteminden arama yapin
- Urun adini Turkce ve Ingilizce deneyin
- Emin degilseniz gumruk musavirine danisin

### EORI numarasi almam gerekiyor mu?
- Sadece AB ulkeleri ile ticaret yapiyorsaniz
- TAREKS uzerinden ucretsiz alinir
- Bir kez alinir, surekli gecerli

### Mikro ihracat mi normal ihracat mi secmeliyim?
- 30.000 EUR ve 600 kg altinda: Mikro ihracat (ETGB)
- Uzerinde: Normal gumruk beyannamesi
- Duzensiz/kucuk siparisler: Mikro ihracat ideal

### Navlun ve sigorta tutarini nereden bulacagim?
- Navlun: Kargo/lojistik firmanizin teklifi
- Sigorta: Sigorta policesi veya navlun dahilse %0.3-0.5 ekleyin

### Doviz kurunu hangi tarih icin almaliyim?
- Beyanname tescil tarihinden bir onceki is gunu
- TCMB efektif SATIS kuru
- [TCMB Kurlar](https://www.tcmb.gov.tr/kurlar/kurlar_tr.html)

---

## 8. FAYDALI LINKLER

- [T.C. Ticaret Bakanligi Gumruk Rehberi](https://gumrukrehberi.gov.tr/)
- [TAREKS Sistemi](https://uygulama.gtb.gov.tr/Tara)
- [TCMB Doviz Kurlari](https://www.tcmb.gov.tr/kurlar/kurlar_tr.html)
- [AB TARIC Veritabani](https://ec.europa.eu/taxation_customs/dds2/taric/)
- [Incoterms 2020 Resmi](https://iccwbo.org/resources-for-business/incoterms-rules/)

---

## 9. CHECKLIST: BEYANNAME ONCESI KONTROL

### Urun Bilgileri
- [ ] GTIP kodu dogru ve guncel
- [ ] Mense ulke dogru
- [ ] Marka ve model girildi
- [ ] Agirlik bilgileri dogru (brut/net)

### Firma Bilgileri
- [ ] VKN/TCKN dogru
- [ ] EORI numarasi var (AB ticareti icin)
- [ ] Adres bilgileri tam
- [ ] Yetkili kisi bilgileri girildi

### Siparis Bilgileri
- [ ] Incoterm secildi
- [ ] Navlun tutari girildi (CIF/CIP ise)
- [ ] Sigorta tutari girildi (CIF/CIP ise)
- [ ] Doviz kuru ve tarihi dogru
- [ ] Istatistiki kiymet hesaplandi

### Belgeler
- [ ] Ticari fatura hazir
- [ ] Packing list hazir
- [ ] Tasima belgesi (konşimento/CMR)
- [ ] Mense belgesi (ATR/EUR.1 gerekirse)
- [ ] Ozel izin belgeleri (gerekirse)

---

*Son Guncelleme: Ocak 2025*
*Bu rehber bilgilendirme amaclidir. Resmi islemler icin gumruk musavirinize danisin.*
