# Order Management System - Implementation Plan

## Overview
Buyer ve Seller perspektifinden sipariş yönetimi. Uluslararası B2B için kargo takibi her iki taraf için kritik.
Ayrıca servis sağlayıcılar (Lojistik, Gümrük, Sigorta, Gözetim) ve yatırımcılar da sisteme dahil.

## User Requirements (Confirmed)
- ✅ Sipariş kaynağı: Hem tekliflerden hem doğrudan ürün sayfasından
- ✅ Kısmi teslimat: Birden fazla satıcıdan alım desteklenecek
- ✅ Talepler: Kısmi karşılama sonrası kalan miktar için talep devam edecek
- ✅ Ödeme: Stripe ile tam entegrasyon
- ✅ Strateji: Tüm özellikler birden
- ✅ Servis sağlayıcılar: Lojistik, Gümrük, Sigorta, Gözetim firmalarından teklif toplama
- ✅ Yatırım ortağı sistemi: Finansmana yardımcı olmak isteyenler için

## Implementation Progress

### TAMAMLANDI ✅

#### Database Entities
- [x] Order (Ana sipariş + Sözleşme durumu)
- [x] OrderItem (Sipariş kalemleri)
- [x] OrderShipment (Kargo bilgileri)
- [x] OrderShipmentItem (Kargo kalemleri)
- [x] OrderStatusHistory (Durum geçmişi)
- [x] StripePayment (Ödeme kayıtları)
- [x] OrderServiceRequest (Servis talepleri - Lojistik/Gümrük/Sigorta/Gözetim)
- [x] OrderServiceQuote (Servis teklifleri)
- [x] OrderParticipant (Sipariş katılımcıları)
- [x] OrderTask (Görev listesi)
- [x] OrderInvestment (Yatırım ortaklığı)
- [x] PublicDemand güncellemesi (RemainingQuantity, FulfillmentStatus)

#### Enums (TypeDefinitions.cs)
- [x] OrderStatuses (16 durum)
- [x] PaymentStatuses
- [x] ShipmentStatuses
- [x] OrderSourceTypes
- [x] DemandFulfillmentStatuses
- [x] ServiceTypes (Logistics, Customs, Insurance, Survey)
- [x] ServiceRequestStatuses
- [x] ServiceQuoteStatuses
- [x] TransportModes
- [x] CustomsOperationTypes
- [x] InsuranceTypes
- [x] ParticipantRoles (Seller, Logistics, Customs, Insurance, Survey, Investor)
- [x] ParticipantStatuses
- [x] TaskTypes
- [x] TaskStatuses
- [x] InvestmentTypes
- [x] InvestmentStatuses

#### DbContext
- [x] Orders, OrderItems, OrderShipments, OrderShipmentItems
- [x] OrderStatusHistory, StripePayments
- [x] OrderServiceRequests, OrderServiceQuotes
- [x] OrderParticipants, OrderTasks, OrderInvestments
- [x] Tüm entity konfigürasyonları

#### Migrations
- [x] AddOrderSystem migration
- [x] AddOrderServiceSystem migration

#### DTOs (DTOs/Order/OrderDtos.cs)
- [x] CreateOrderDto, CreateOrderFromInquiryDto, CreateOrderFromDemandDto
- [x] OrderListDto, OrderDetailDto, OrderItemDto
- [x] OrderShipmentDto, CreateShipmentDto, UpdateShipmentDto
- [x] OrderStatusHistoryDto
- [x] BuyerOrderStatsDto, SellerOrderStatsDto
- [x] PaymentIntentResultDto, PaymentStatusDto, RefundRequestDto
- [x] CreateServiceRequestDto, ServiceRequestListDto, ServiceRequestDetailDto
- [x] CreateServiceQuoteDto, UpdateServiceQuoteDto, ServiceQuoteDto
- [x] OrderTaskDto
- [x] InvestmentOpportunityDto, CreateInvestmentDto, OrderInvestmentDto

#### Service Interfaces
- [x] IOrderService (tüm operasyonlar tanımlı)

#### Services
- [x] OrderService implementation
- [x] IStripeService interface
- [x] StripeService implementation
- [x] Stripe.net NuGet paketi (v50.1.0)

#### API Controllers
- [x] OrdersApiController (Buyer + Seller + Service Provider + Task + Investment endpoints)
- [x] PaymentsApiController (Stripe webhooks)

#### Dashboard Pages
- [x] MyOrders.cshtml + MyOrders.js (Buyer)
- [x] SellerOrders.cshtml + SellerOrders.js (Seller)
- [ ] ServiceRequests.cshtml + ServiceRequests.js (Service Providers)
- [ ] InvestmentOpportunities.cshtml + InvestmentOpportunities.js (Investors)
- [ ] MyTasks.cshtml + MyTasks.js (Tüm katılımcılar)

#### Module Registration
- [x] PlatformModules seed (my-orders, seller-orders, service-requests, investment-opportunities)
- [x] CapabilityModuleMappings
- [x] Survey ve Investor capabilities eklendi
- [ ] Localization keys

#### Product Updates
- [x] HSCode, GTIN, CountryOfOrigin fields eklendi (uluslararası ticaret için)

### DEVAM EDECEK 🔄

## Sipariş Akışı (Genişletilmiş)

```
1. Ürün teklifi kabul edildi veya doğrudan satın alım
   → Order(Draft) oluşturulur

2. Buyer opsiyonel servis talepleri oluşturabilir:
   - Lojistik talebi → Transport capability'li firmalar teklif verir
   - Gümrük talebi → Customs capability'li firmalar teklif verir
   - Sigorta talebi → Insurance capability'li firmalar teklif verir
   - Gözetim talebi → Survey capability'li firmalar teklif verir

3. Buyer yatırım ihtiyacı varsa:
   - Investor capability'li firmalar yatırım teklifi verebilir

4. Tüm teklifler seçildi
   → Order(AwaitingContract)

5. Buyer sözleşmeyi onayladı (IsContractAccepted = true)
   → Order(PendingPayment)

6. Ödeme alındı (Stripe)
   → Order(Active)
   → Tüm katılımcılara (Seller + Service Providers) görev listesi oluşturulur

7. Her katılımcı görevlerini tamamladıkça:
   → OrderTask.Status güncellenir
   → Order durumu otomatik güncellenir (Processing → Shipped → Delivered)

8. Tüm görevler tamamlandı
   → Order(Completed)
   → Katılımcılara ödemeler yapılır
```

## Servis Tipleri

| Tip | Capability | Açıklama |
|-----|------------|----------|
| Seller | Seller | Ürün satıcısı |
| Buyer | Buyer | Alıcı |
| Logistics | Transport | Taşıma/Lojistik firması |
| Customs | Customs | Gümrük müşaviri |
| Insurance | Insurance | Sigorta şirketi |
| Survey | Survey | Gözetim/Ekspertiz firması |
| Investor | Investor | Yatırımcı/Finansman ortağı |

## Database Tables (Oluşturuldu)

```
Orders
OrderItems
OrderShipments
OrderShipmentItems
OrderStatusHistory
StripePayments
OrderServiceRequests
OrderServiceQuotes
OrderParticipants
OrderTasks
OrderInvestments
```

## API Endpoints (Planlanmış)

### Buyer APIs
```
GET  /api/orders/my                           - Siparişlerim
GET  /api/orders/my/{id}                      - Sipariş detayı
POST /api/orders                              - Doğrudan sipariş
POST /api/orders/from-inquiry/{responseId}    - Inquiry'den sipariş
POST /api/orders/from-demand/{responseId}     - Demand'dan sipariş
PUT  /api/orders/{id}/cancel                  - İptal et
PUT  /api/orders/{id}/confirm-delivery        - Teslim aldım
PUT  /api/orders/{id}/accept-contract         - Sözleşme onayla
POST /api/orders/{id}/service-requests        - Servis talebi oluştur
PUT  /api/orders/quotes/{id}/accept           - Servis teklifini kabul et
PUT  /api/orders/quotes/{id}/reject           - Servis teklifini reddet
```

### Seller APIs
```
GET  /api/orders/seller                       - Gelen siparişler
GET  /api/orders/seller/{id}                  - Sipariş detayı
PUT  /api/orders/{id}/confirm                 - Onayla
PUT  /api/orders/{id}/reject                  - Reddet
PUT  /api/orders/{id}/status                  - Durum güncelle
POST /api/orders/{id}/shipments               - Kargo ekle
PUT  /api/orders/{id}/shipments/{sid}         - Kargo güncelle
```

### Service Provider APIs
```
GET  /api/service-requests/open               - Açık talepler
GET  /api/service-requests/{id}               - Talep detayı
POST /api/service-requests/{id}/quote         - Teklif ver
PUT  /api/service-requests/quotes/{id}        - Teklif güncelle
DELETE /api/service-requests/quotes/{id}      - Teklif geri çek
```

### Task APIs
```
GET  /api/orders/{id}/tasks                   - Görevlerim
PUT  /api/orders/tasks/{id}/start             - Görevi başlat
PUT  /api/orders/tasks/{id}/complete          - Görevi tamamla
```

### Investment APIs
```
GET  /api/investments/opportunities           - Yatırım fırsatları
POST /api/investments                         - Yatırım teklifi ver
PUT  /api/investments/{id}/accept             - Yatırım kabul (Buyer)
PUT  /api/investments/{id}/reject             - Yatırım reddet (Buyer)
```

### Payment APIs
```
POST /api/payments/create-intent              - PaymentIntent oluştur
POST /api/payments/webhook                    - Stripe webhook
GET  /api/payments/{orderId}/status           - Ödeme durumu
POST /api/payments/{orderId}/refund           - İade
```

## NuGet Packages
```xml
<PackageReference Include="Stripe.net" Version="43.*" />
```

## Notes
- Her satıcı için ayrı Order oluşur (multi-supplier support)
- Kısmi teslimat: Bir Order'da birden fazla Shipment olabilir
- PublicDemand kısmi karşılanınca RemainingQuantity güncellenir
- Stripe webhooks için HTTPS gerekli (ngrok for local dev)
- Sözleşme onaylanmadan ödeme alınmaz
- Ödeme alınmadan kimse sorumlu olmaz (görevler oluşmaz)
