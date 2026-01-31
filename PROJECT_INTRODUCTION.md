# Bridgo - B2B Global Ticaret Platformu

## Vizyon

Bridgo, uluslararası B2B ticareti tek bir platformda birleştiren yeni nesil bir iş ağıdır. Alıcılar, satıcılar, taşımacılar, sigorta firmaları ve gümrük müşavirleri arasında kesintisiz bir köprü kurarak global ticaretin karmaşıklığını ortadan kaldırır.

---

## Platform Özeti

### Ne Yapıyoruz?

Bridgo, B2B tedarik zincirinin tüm paydaşlarını tek çatı altında toplar:

| Paydaş | Platform Üzerindeki Rolü |
|--------|--------------------------|
| **Satıcılar/Üreticiler** | Ürün kataloglarını yayınlar, taleplere teklif verir, siparişleri yönetir |
| **Alıcılar** | Tedarikçi bulur, toplu talep oluşturur, teklifleri karşılaştırır |
| **Taşımacılar** | Lojistik hizmeti sunar, kargo takibi sağlar |
| **Sigorta Firmaları** | Kargo sigortası teklif eder |
| **Gümrük Müşavirleri** | Gümrükleme hizmeti sunar, beyan süreçlerini yönetir |

### Temel Değer Önerisi

```
Geleneksel Yöntem                    Bridgo ile
─────────────────────────────────    ──────────────────────────────
✗ Tedarikçi bulmak için fuarlara    ✓ Tek platformda binlerce satıcı
✗ Her firma için ayrı iletişim      ✓ Entegre mesajlaşma ve teklif
✗ Manuel fiyat karşılaştırma        ✓ Akıllı teklif karşılaştırma
✗ Lojistik için ayrı anlaşmalar     ✓ Entegre taşımacılık teklifleri
✗ Kağıt tabanlı gümrük süreçleri    ✓ Dijital gümrük yönetimi
```

---

## Platform Özellikleri

### 1. Ürün Katalog Sistemi
- Satıcılar ürünlerini kategorize ederek listeler
- Çoklu görsel, fiyat kademeleri, stok yönetimi
- SKU bazlı envanter takibi
- Çoklu depo desteği

### 2. Talep & Teklif Sistemi
- **Açık Talepler:** Alıcılar toplu talep oluşturur, satıcılar teklif verir
- **Doğrudan Sorgulama:** Belirli bir ürüne özel teklif isteme
- **Teklif Karşılaştırma:** Fiyat, teslimat süresi, minimum sipariş miktarı bazlı analiz
- **Kategori Takibi:** Satıcılar ilgi alanlarını belirler, yeni taleplerden anında haberdar olur

### 3. Sipariş Yönetimi
- Checkout wizard ile adım adım sipariş oluşturma
- Gerçek zamanlı sipariş durumu takibi (Timeline)
- Alıcı ve satıcı için ayrı sipariş panelleri
- Otomatik bildirimler (SignalR ile real-time)

### 4. Entegre Hizmet Pazarı
Sipariş oluşturulduğunda ek hizmetler tek tıkla talep edilebilir:

| Hizmet | Açıklama |
|--------|----------|
| **Lojistik** | Karayolu, denizyolu, havayolu taşımacılık teklifleri |
| **Sigorta** | Kargo sigortası teklif ve poliçe yönetimi |
| **Gümrük** | İthalat/ihracat beyan süreçleri |
| **Ekspertiz** | Mal muayene ve kalite kontrol hizmetleri |

### 5. Firma Yönetimi
- Multi-tenant mimari (her firma izole veri alanı)
- Rol bazlı erişim kontrolü (RBAC)
- Ekip üyeleri ve davet sistemi
- Firma doğrulama ve güven rozetleri

### 6. Sınırsız Dil Desteği
- İstenilen her dilde arayüz desteği (sınırsız dil ekleme)
- Dinamik dil değiştirme
- XML tabanlı esnek yerelleştirme altyapısı
- Ürün ve içerik bazlı çoklu dil yönetimi
- RTL (sağdan sola) dil desteği

---

## Yakında Gelecek: Bridgo Business Feed

### Firmalar İçin Sosyal Reklam Ortamı

Bridgo, geleneksel B2B pazaryeri modelinin ötesine geçerek firmalara **profesyonel bir sosyal ağ deneyimi** sunmayı hedefliyor.

#### Konsept

LinkedIn'in iş ağı yaklaşımını B2B ticaret ekosistemine uyarlamak:

```
┌─────────────────────────────────────────────────────────────┐
│                    BRIDGO BUSINESS FEED                     │
├─────────────────────────────────────────────────────────────┤
│  ┌─────────────────────────────────────────────────────┐   │
│  │ 🏭 ABC Tekstil                           2 saat önce │   │
│  │ ───────────────────────────────────────────────────── │   │
│  │ Yeni sezon organik pamuk koleksiyonumuz hazır! 🌿     │   │
│  │                                                       │   │
│  │ [Görsel: Ürün fotoğrafları]                          │   │
│  │                                                       │   │
│  │ ❤️ 45  💬 12  📤 8     [Teklif İste] [Detaylara Git] │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │ 📦 XYZ Lojistik                        Sponsorlu     │   │
│  │ ───────────────────────────────────────────────────── │   │
│  │ Çin-Türkiye hattında %20 indirimli konteyner        │   │
│  │ taşımacılığı! Kampanya 15 Şubat'a kadar geçerli.    │   │
│  │                                                       │   │
│  │ ❤️ 128  💬 34  📤 56    [Teklif Al] [Kampanya Detay] │   │
│  └─────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

#### Planlanan Özellikler

**Firma Paylaşımları**
- Yeni ürün duyuruları
- Kapasite güncellemeleri
- Sektör haberleri ve trendler
- Başarı hikayeleri ve referanslar
- Fuar & etkinlik katılımları

**Sponsorlu İçerikler**
- Hedefli reklam gösterimi (sektör, lokasyon, firma büyüklüğü)
- Ürün/hizmet tanıtım kampanyaları
- Öne çıkarılmış firma profilleri
- Banner ve native reklam formatları

**Etkileşim Özellikleri**
- Beğeni, yorum, paylaşım
- Firma takip sistemi
- Sektör bazlı hashtag'ler
- Trend konular

**İş Geliştirme Araçları**
- Paylaşımdan doğrudan teklif isteme
- İletişim başlatma (entegre mesajlaşma)
- Firma profiline hızlı erişim
- Ürün kataloguna yönlendirme

#### Gelir Modeli

| Model | Açıklama |
|-------|----------|
| **Sponsorlu Paylaşım** | Firmaların içeriklerini öne çıkarması |
| **Premium Profil** | Gelişmiş analitik ve görünürlük |
| **Hedefli Reklamlar** | Sektör/bölge bazlı reklam gösterimi |
| **Lead Generation** | Nitelikli müşteri adayı paketi |

---

## Teknik Altyapı

### Teknoloji Stack

| Katman | Teknoloji |
|--------|-----------|
| Backend | .NET 9, ASP.NET Core MVC |
| Veritabanı | PostgreSQL |
| Frontend | KnockoutJS, Bootstrap 5 |
| Gerçek Zamanlı | SignalR |
| Kimlik Doğrulama | ASP.NET Identity |
| API | RESTful API |

### Mimari Prensipler

- **Multi-Tenant:** Firma bazlı veri izolasyonu
- **Repository Pattern:** Temiz veri erişim katmanı
- **Service Layer:** İş mantığı soyutlama
- **Capability-Based Auth:** Rol ve yetki bazlı erişim
- **Offline-First:** CDN bağımsız, yerel kütüphaneler

### Güvenlik

- Firma bazlı veri izolasyonu (VendorId)
- RBAC (Role-Based Access Control)
- Capability bazlı yetkilendirme
- Soft-delete ile veri koruma

---

## Hedef Kitle

### Birincil Kullanıcılar

1. **İhracatçı Firmalar**
   - Yeni pazarlara açılmak isteyen üreticiler
   - Global alıcı arayan tedarikçiler

2. **İthalatçı Firmalar**
   - Güvenilir tedarikçi arayan alıcılar
   - Toplu satın alma yapan distribütörler

3. **Lojistik Firmaları**
   - Taşımacılık şirketleri
   - Freight forwarder'lar

4. **Hizmet Sağlayıcılar**
   - Gümrük müşavirleri
   - Sigorta şirketleri
   - Ekspertiz firmaları

### Sektör Odağı

- Tekstil & Hazır Giyim
- Gıda & İçecek
- Makine & Ekipman
- Kimya & Plastik
- Elektronik & Teknoloji

---

## Rekabet Avantajları

| Özellik | Bridgo | Geleneksel B2B | Alibaba/TradeKey |
|---------|--------|----------------|------------------|
| Entegre lojistik | ✅ | ❌ | Kısıtlı |
| Gümrük yönetimi | ✅ | ❌ | ❌ |
| Sigorta entegrasyonu | ✅ | ❌ | ❌ |
| Sosyal ağ özellikleri | ✅ (Yakında) | ❌ | ❌ |
| Türkiye odaklı | ✅ | - | ❌ |
| Sınırsız dil desteği | ✅ | Değişken | Kısıtlı |
| Firma doğrulama | ✅ | ❌ | Kısıtlı |

---

## Yol Haritası

### Faz 1: Temel Platform ✅
- Firma kayıt ve doğrulama
- Ürün katalog sistemi
- Talep/teklif mekanizması
- Sipariş yönetimi
- Entegre hizmet talepleri

### Faz 2: Genişletilmiş Özellikler 🔄
- Gelişmiş arama ve filtreleme
- Teklif karşılaştırma araçları
- Raporlama ve analitik
- Mobil uyumlu arayüz

### Faz 3: Sosyal Ticaret Ağı 📋
- Business Feed (sosyal akış)
- Firma takip sistemi
- İçerik paylaşımı
- Sponsorlu içerik altyapısı

### Faz 4: Yapay Zeka & Otomasyon 📋
- Akıllı eşleştirme önerileri
- Otomatik fiyatlama analizi
- Chatbot desteği
- Tahmine dayalı analitik

---

## İletişim

Proje hakkında detaylı bilgi ve iş birliği için:

- **Web:** [Yakında]
- **E-posta:** [Yakında]

---

*Bu doküman Bridgo B2B Platform projesinin genel tanıtımını içermektedir.*
*Son güncelleme: Ocak 2026*
