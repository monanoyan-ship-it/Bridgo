using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bridgo.Models.Enums;

/// <summary>
/// Code-based tip tanimlari icin base class
/// Database yerine kod icerisinde tanimlanan tipler icin kullanilir
/// </summary>
public class TypeItem
{
    public int Id { get; }
    public string SystemName { get; }
    public string NameResourceKey { get; }
    public string? Description { get; }
    public string? Icon { get; }
    public string? CssClass { get; }
    public int DisplayOrder { get; }
    public bool IsDefault { get; }
    public bool IsActive { get; }
    public bool IsSystem { get; }

    public TypeItem(
        int id,
        string systemName,
        string nameResourceKey,
        string? description = null,
        string? icon = null,
        string? cssClass = null,
        int displayOrder = 0,
        bool isDefault = false,
        bool isActive = true,
        bool isSystem = true)
    {
        Id = id;
        SystemName = systemName;
        NameResourceKey = nameResourceKey;
        Description = description;
        Icon = icon;
        CssClass = cssClass;
        DisplayOrder = displayOrder;
        IsDefault = isDefault;
        IsActive = isActive;
        IsSystem = isSystem;
    }
}

// ============================================================
// CATEGORY DELETION STATUSES
// ============================================================
public static class CategoryDeletionStatuses
{
    public static readonly TypeItem Pending = new(0, "Pending", "CategoryDeletionStatus.Pending", "Onay bekliyor", "bi-hourglass-split", "bg-warning text-dark", 0, isDefault: true);
    public static readonly TypeItem Approved = new(1, "Approved", "CategoryDeletionStatus.Approved", "Onaylandi", "bi-check-circle", "bg-success", 1);
    public static readonly TypeItem Rejected = new(2, "Rejected", "CategoryDeletionStatus.Rejected", "Reddedildi", "bi-x-circle", "bg-danger", 2);

    public static IEnumerable<TypeItem> All => new[] { Pending, Approved, Rejected };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int? id) => id.HasValue ? All.FirstOrDefault(x => x.Id == id.Value) : null;
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);
}

// ============================================================
// ADDRESS TYPES
// ============================================================
public static class AddressTypes
{
    public static readonly TypeItem Billing = new(1, "Billing", "AddressType.Billing", "Fatura adresi", "bi-receipt", null, 1);
    public static readonly TypeItem Shipping = new(2, "Shipping", "AddressType.Shipping", "Teslimat adresi", "bi-truck", null, 2);
    public static readonly TypeItem Headquarters = new(3, "Headquarters", "AddressType.Headquarters", "Merkez ofis adresi", "bi-building", null, 3, isDefault: true);
    public static readonly TypeItem Warehouse = new(4, "Warehouse", "AddressType.Warehouse", "Depo adresi", "bi-box-seam", null, 4);
    public static readonly TypeItem Branch = new(5, "Branch", "AddressType.Branch", "Sube adresi", "bi-shop", null, 5);
    public static readonly TypeItem Return = new(6, "Return", "AddressType.Return", "Iade adresi", "bi-arrow-return-left", null, 6);

    public static IEnumerable<TypeItem> All => new[] { Billing, Shipping, Headquarters, Warehouse, Branch, Return };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);
}

// ============================================================
// WAREHOUSE TYPES
// ============================================================
public static class WarehouseTypes
{
    public static readonly TypeItem Main = new(1, "Main", "WarehouseType.Main", "Ana depo", "bi-house-door", null, 1, isDefault: true);
    public static readonly TypeItem Distribution = new(2, "Distribution", "WarehouseType.Distribution", "Dagitim deposu", "bi-diagram-3", null, 2);
    public static readonly TypeItem Returns = new(3, "Returns", "WarehouseType.Returns", "Iade deposu", "bi-arrow-return-left", null, 3);
    public static readonly TypeItem Temporary = new(4, "Temporary", "WarehouseType.Temporary", "Gecici depo", "bi-clock-history", null, 4);
    public static readonly TypeItem Virtual = new(5, "Virtual", "WarehouseType.Virtual", "Sanal depo (dropship)", "bi-cloud", null, 5);
    public static readonly TypeItem Consignment = new(6, "Consignment", "WarehouseType.Consignment", "Konsinye depo", "bi-box-arrow-in-right", null, 6);

    public static IEnumerable<TypeItem> All => new[] { Main, Distribution, Returns, Temporary, Virtual, Consignment };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);
}

// ============================================================
// VENDOR STATUSES
// ============================================================
public static class VendorStatuses
{
    public static readonly TypeItem PendingProfile = new(1, "PendingProfile", "VendorStatus.PendingProfile", "Profil tamamlanmayi bekliyor", "bi-hourglass", "bg-warning text-dark", 1, isDefault: true);
    public static readonly TypeItem PendingVerification = new(2, "PendingVerification", "VendorStatus.PendingVerification", "Dogrulama bekliyor", "bi-shield-exclamation", "bg-info", 2);
    public static readonly TypeItem Active = new(3, "Active", "VendorStatus.Active", "Aktif", "bi-check-circle", "bg-success", 3);
    public static readonly TypeItem Suspended = new(4, "Suspended", "VendorStatus.Suspended", "Askiya alinmis", "bi-pause-circle", "bg-secondary", 4);
    public static readonly TypeItem Rejected = new(5, "Rejected", "VendorStatus.Rejected", "Reddedilmis", "bi-x-circle", "bg-danger", 5);

    public static IEnumerable<TypeItem> All => new[] { PendingProfile, PendingVerification, Active, Suspended, Rejected };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);
}

// ============================================================
// PRODUCT STATUSES
// ============================================================
public static class ProductStatuses
{
    public static readonly TypeItem Draft = new(1, "Draft", "ProductStatus.Draft", "Taslak", "bi-file-earmark", "bg-secondary", 1, isDefault: true);
    public static readonly TypeItem Active = new(2, "Active", "ProductStatus.Active", "Aktif", "bi-check-circle", "bg-success", 2);
    public static readonly TypeItem Inactive = new(3, "Inactive", "ProductStatus.Inactive", "Pasif", "bi-dash-circle", "bg-warning text-dark", 3);
    public static readonly TypeItem Discontinued = new(4, "Discontinued", "ProductStatus.Discontinued", "Uretimden kalkmis", "bi-archive", "bg-danger", 4);

    public static IEnumerable<TypeItem> All => new[] { Draft, Active, Inactive, Discontinued };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);
}

// ============================================================
// TEAM MEMBER TYPES
// ============================================================
public static class TeamMemberTypes
{
    public static readonly TypeItem Employee = new(0, "Employee", "TeamMemberType.Employee", "Calisan", "bi-person", "bg-secondary", 0, isDefault: true);
    public static readonly TypeItem Representative = new(1, "Representative", "TeamMemberType.Representative", "Temsilci", "bi-person-badge", "bg-primary", 1);
    public static readonly TypeItem Partner = new(2, "Partner", "TeamMemberType.Partner", "Ortak", "bi-people", "bg-info", 2);
    public static readonly TypeItem Shareholder = new(3, "Shareholder", "TeamMemberType.Shareholder", "Hissedar", "bi-pie-chart", "bg-info", 3);
    public static readonly TypeItem BoardMember = new(4, "BoardMember", "TeamMemberType.BoardMember", "Yonetim Kurulu Uyesi", "bi-briefcase", "bg-warning text-dark", 4);
    public static readonly TypeItem UBO = new(5, "UBO", "TeamMemberType.UBO", "UBO (Gercek Faydalanici)", "bi-person-fill-check", "bg-danger", 5);
    public static readonly TypeItem AuthorizedSignatory = new(6, "AuthorizedSignatory", "TeamMemberType.AuthorizedSignatory", "Imza Yetkili", "bi-pen", "bg-success", 6);
    public static readonly TypeItem Director = new(7, "Director", "TeamMemberType.Director", "Direktor", "bi-person-workspace", "bg-dark", 7);
    public static readonly TypeItem Founder = new(8, "Founder", "TeamMemberType.Founder", "Kurucu", "bi-star", "bg-dark", 8);

    public static IEnumerable<TypeItem> All => new[] { Employee, Representative, Partner, Shareholder, BoardMember, UBO, AuthorizedSignatory, Director, Founder };
    public static IEnumerable<TypeItem> CorporateTypes => new[] { Representative, Partner, Shareholder, BoardMember, UBO, AuthorizedSignatory, Director, Founder };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);
}

// ============================================================
// TEAM MEMBER STATUSES
// ============================================================
public static class TeamMemberStatuses
{
    public static readonly TypeItem Pending = new(1, "Pending", "TeamMemberStatus.Pending", "Beklemede", "bi-hourglass-split", "bg-warning text-dark", 1, isDefault: true);
    public static readonly TypeItem Active = new(2, "Active", "TeamMemberStatus.Active", "Aktif", "bi-person-check", "bg-success", 2);
    public static readonly TypeItem Rejected = new(3, "Rejected", "TeamMemberStatus.Rejected", "Reddedildi", "bi-person-x", "bg-danger", 3);
    public static readonly TypeItem Cancelled = new(4, "Cancelled", "TeamMemberStatus.Cancelled", "Iptal edildi", "bi-x-lg", "bg-secondary", 4);
    public static readonly TypeItem Expired = new(5, "Expired", "TeamMemberStatus.Expired", "Suresi doldu", "bi-clock-history", "bg-secondary", 5);
    public static readonly TypeItem Left = new(6, "Left", "TeamMemberStatus.Left", "Ayrildi", "bi-person-dash", "bg-info", 6);

    public static IEnumerable<TypeItem> All => new[] { Pending, Active, Rejected, Cancelled, Expired, Left };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);
}

// ============================================================
// MEMBER VERIFICATION STATUSES
// ============================================================
public static class MemberVerificationStatuses
{
    public static readonly TypeItem NotVerified = new(1, "NotVerified", "MemberVerificationStatus.NotVerified", "Dogrulanmadi", "bi-x-circle", "bg-secondary", 1, isDefault: true);
    public static readonly TypeItem Pending = new(2, "Pending", "MemberVerificationStatus.Pending", "Beklemede", "bi-hourglass-split", "bg-warning text-dark", 2);
    public static readonly TypeItem Verified = new(3, "Verified", "MemberVerificationStatus.Verified", "Dogrulandi", "bi-patch-check", "bg-success", 3);
    public static readonly TypeItem Rejected = new(4, "Rejected", "MemberVerificationStatus.Rejected", "Reddedildi", "bi-x-octagon", "bg-danger", 4);

    public static IEnumerable<TypeItem> All => new[] { NotVerified, Pending, Verified, Rejected };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);
}

// ============================================================
// CATEGORY REQUEST STATUSES
// ============================================================
public static class CategoryRequestStatuses
{
    public static readonly TypeItem Pending = new(1, "Pending", "CategoryRequestStatus.Pending", "Beklemede", "bi-hourglass-split", "bg-warning text-dark", 1, isDefault: true);
    public static readonly TypeItem Approved = new(2, "Approved", "CategoryRequestStatus.Approved", "Onaylandi", "bi-check-circle", "bg-success", 2);
    public static readonly TypeItem Rejected = new(3, "Rejected", "CategoryRequestStatus.Rejected", "Reddedildi", "bi-x-circle", "bg-danger", 3);
    public static readonly TypeItem Duplicate = new(4, "Duplicate", "CategoryRequestStatus.Duplicate", "Mevcut kategori", "bi-files", "bg-info", 4);

    public static IEnumerable<TypeItem> All => new[] { Pending, Approved, Rejected, Duplicate };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);
}

// ============================================================
// PRODUCT INQUIRY STATUSES
// ============================================================
public static class ProductInquiryStatuses
{
    public static readonly TypeItem Pending = new(1, "Pending", "ProductInquiryStatus.Pending", "Beklemede", "bi-hourglass-split", "bg-warning text-dark", 1, isDefault: true);
    public static readonly TypeItem Viewed = new(2, "Viewed", "ProductInquiryStatus.Viewed", "Goruldu", "bi-eye", "bg-info", 2);
    public static readonly TypeItem Responded = new(3, "Responded", "ProductInquiryStatus.Responded", "Yanitlandi", "bi-reply", "bg-primary", 3);
    public static readonly TypeItem Accepted = new(4, "Accepted", "ProductInquiryStatus.Accepted", "Kabul edildi", "bi-check-circle", "bg-success", 4);
    public static readonly TypeItem Rejected = new(5, "Rejected", "ProductInquiryStatus.Rejected", "Reddedildi", "bi-x-circle", "bg-danger", 5);
    public static readonly TypeItem Cancelled = new(6, "Cancelled", "ProductInquiryStatus.Cancelled", "Iptal", "bi-x-lg", "bg-secondary", 6);
    public static readonly TypeItem Expired = new(7, "Expired", "ProductInquiryStatus.Expired", "Suresi doldu", "bi-clock-history", "bg-dark", 7);

    public static IEnumerable<TypeItem> All => new[] { Pending, Viewed, Responded, Accepted, Rejected, Cancelled, Expired };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);
}

// ============================================================
// PRODUCT INQUIRY RESPONSE STATUSES
// ============================================================
public static class ProductInquiryResponseStatuses
{
    public static readonly TypeItem Pending = new(1, "Pending", "ProductInquiryResponseStatus.Pending", "Beklemede", "bi-hourglass-split", "bg-warning text-dark", 1, isDefault: true);
    public static readonly TypeItem Viewed = new(2, "Viewed", "ProductInquiryResponseStatus.Viewed", "Goruldu", "bi-eye", "bg-info", 2);
    public static readonly TypeItem Accepted = new(3, "Accepted", "ProductInquiryResponseStatus.Accepted", "Kabul edildi", "bi-check-circle", "bg-success", 3);
    public static readonly TypeItem Rejected = new(4, "Rejected", "ProductInquiryResponseStatus.Rejected", "Reddedildi", "bi-x-circle", "bg-danger", 4);
    public static readonly TypeItem CounterOffer = new(5, "CounterOffer", "ProductInquiryResponseStatus.CounterOffer", "Karsi teklif", "bi-arrow-left-right", "bg-primary", 5);
    public static readonly TypeItem Expired = new(6, "Expired", "ProductInquiryResponseStatus.Expired", "Suresi doldu", "bi-clock-history", "bg-dark", 6);

    public static IEnumerable<TypeItem> All => new[] { Pending, Viewed, Accepted, Rejected, CounterOffer, Expired };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);
}

// ============================================================
// ORDER STATUSES
// ============================================================
public static class OrderStatuses
{
    public static readonly TypeItem Draft = new(0, "Draft", "OrderStatus.Draft", "Taslak", "bi-file-earmark", "bg-secondary", 0, isDefault: true);
    public static readonly TypeItem PendingPayment = new(1, "PendingPayment", "OrderStatus.PendingPayment", "Odeme bekleniyor", "bi-credit-card", "bg-warning text-dark", 1);
    public static readonly TypeItem PaymentFailed = new(2, "PaymentFailed", "OrderStatus.PaymentFailed", "Odeme basarisiz", "bi-exclamation-triangle", "bg-danger", 2);
    public static readonly TypeItem PendingConfirm = new(3, "PendingConfirm", "OrderStatus.PendingConfirm", "Onay bekleniyor", "bi-hourglass-split", "bg-info", 3);
    public static readonly TypeItem Confirmed = new(4, "Confirmed", "OrderStatus.Confirmed", "Onaylandi", "bi-check-circle", "bg-primary", 4);
    public static readonly TypeItem Processing = new(5, "Processing", "OrderStatus.Processing", "Hazirlaniyor", "bi-gear", "bg-info", 5);
    public static readonly TypeItem PartiallyShipped = new(6, "PartiallyShipped", "OrderStatus.PartiallyShipped", "Kismen kargoda", "bi-box-seam", "bg-info", 6);
    public static readonly TypeItem Shipped = new(7, "Shipped", "OrderStatus.Shipped", "Kargoya verildi", "bi-truck", "bg-primary", 7);
    public static readonly TypeItem InTransit = new(8, "InTransit", "OrderStatus.InTransit", "Yolda", "bi-airplane", "bg-primary", 8);
    public static readonly TypeItem CustomsClearance = new(9, "CustomsClearance", "OrderStatus.CustomsClearance", "Gumrukte", "bi-flag", "bg-warning text-dark", 9);
    public static readonly TypeItem PartiallyDelivered = new(10, "PartiallyDelivered", "OrderStatus.PartiallyDelivered", "Kismen teslim", "bi-box-arrow-in-down", "bg-info", 10);
    public static readonly TypeItem Delivered = new(11, "Delivered", "OrderStatus.Delivered", "Teslim edildi", "bi-house-check", "bg-success", 11);
    public static readonly TypeItem Completed = new(12, "Completed", "OrderStatus.Completed", "Tamamlandi", "bi-check-all", "bg-success", 12);
    public static readonly TypeItem Cancelled = new(13, "Cancelled", "OrderStatus.Cancelled", "Iptal edildi", "bi-x-circle", "bg-danger", 13);
    public static readonly TypeItem Refunded = new(14, "Refunded", "OrderStatus.Refunded", "Iade edildi", "bi-arrow-counterclockwise", "bg-secondary", 14);
    public static readonly TypeItem Disputed = new(15, "Disputed", "OrderStatus.Disputed", "Anlaşmazlik", "bi-exclamation-diamond", "bg-danger", 15);

    public static IEnumerable<TypeItem> All => new[] { Draft, PendingPayment, PaymentFailed, PendingConfirm, Confirmed, Processing, PartiallyShipped, Shipped, InTransit, CustomsClearance, PartiallyDelivered, Delivered, Completed, Cancelled, Refunded, Disputed };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);

    // Active orders (for buyer)
    public static IEnumerable<TypeItem> Active => new[] { PendingPayment, PendingConfirm, Confirmed, Processing, PartiallyShipped, Shipped, InTransit, CustomsClearance, PartiallyDelivered };
    // Completed orders
    public static IEnumerable<TypeItem> CompletedOrders => new[] { Delivered, Completed };
    // Cancelled/Refunded
    public static IEnumerable<TypeItem> CancelledOrders => new[] { Cancelled, Refunded, Disputed };
}

// ============================================================
// PAYMENT STATUSES
// ============================================================
public static class PaymentStatuses
{
    public static readonly TypeItem Pending = new(0, "Pending", "PaymentStatus.Pending", "Beklemede", "bi-hourglass-split", "bg-warning text-dark", 0, isDefault: true);
    public static readonly TypeItem Processing = new(1, "Processing", "PaymentStatus.Processing", "Isleniyor", "bi-gear", "bg-info", 1);
    public static readonly TypeItem Succeeded = new(2, "Succeeded", "PaymentStatus.Succeeded", "Basarili", "bi-check-circle", "bg-success", 2);
    public static readonly TypeItem Failed = new(3, "Failed", "PaymentStatus.Failed", "Basarisiz", "bi-x-circle", "bg-danger", 3);
    public static readonly TypeItem Refunded = new(4, "Refunded", "PaymentStatus.Refunded", "Iade edildi", "bi-arrow-counterclockwise", "bg-secondary", 4);
    public static readonly TypeItem PartiallyRefunded = new(5, "PartiallyRefunded", "PaymentStatus.PartiallyRefunded", "Kismen iade", "bi-arrow-left-right", "bg-info", 5);

    public static IEnumerable<TypeItem> All => new[] { Pending, Processing, Succeeded, Failed, Refunded, PartiallyRefunded };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);
}

// ============================================================
// SHIPMENT STATUSES
// ============================================================
public static class ShipmentStatuses
{
    public static readonly TypeItem Preparing = new(0, "Preparing", "ShipmentStatus.Preparing", "Hazirlaniyor", "bi-box-seam", "bg-secondary", 0, isDefault: true);
    public static readonly TypeItem ReadyToShip = new(1, "ReadyToShip", "ShipmentStatus.ReadyToShip", "Kargoya hazir", "bi-box", "bg-info", 1);
    public static readonly TypeItem Shipped = new(2, "Shipped", "ShipmentStatus.Shipped", "Gonderildi", "bi-truck", "bg-primary", 2);
    public static readonly TypeItem InTransit = new(3, "InTransit", "ShipmentStatus.InTransit", "Yolda", "bi-airplane", "bg-primary", 3);
    public static readonly TypeItem OutForDelivery = new(4, "OutForDelivery", "ShipmentStatus.OutForDelivery", "Dagitimda", "bi-bicycle", "bg-info", 4);
    public static readonly TypeItem CustomsClearance = new(5, "CustomsClearance", "ShipmentStatus.CustomsClearance", "Gumrukte", "bi-flag", "bg-warning text-dark", 5);
    public static readonly TypeItem Delivered = new(6, "Delivered", "ShipmentStatus.Delivered", "Teslim edildi", "bi-house-check", "bg-success", 6);
    public static readonly TypeItem Failed = new(7, "Failed", "ShipmentStatus.Failed", "Teslim edilemedi", "bi-x-circle", "bg-danger", 7);
    public static readonly TypeItem Returned = new(8, "Returned", "ShipmentStatus.Returned", "Iade edildi", "bi-arrow-counterclockwise", "bg-secondary", 8);

    public static IEnumerable<TypeItem> All => new[] { Preparing, ReadyToShip, Shipped, InTransit, OutForDelivery, CustomsClearance, Delivered, Failed, Returned };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);
}

// ============================================================
// ORDER SOURCE TYPES
// ============================================================
public static class OrderSourceTypes
{
    public static readonly TypeItem Direct = new(0, "Direct", "OrderSourceType.Direct", "Dogrudan satin alma", "bi-cart-check", null, 0, isDefault: true);
    public static readonly TypeItem FromInquiry = new(1, "FromInquiry", "OrderSourceType.FromInquiry", "Urun isteginden", "bi-question-circle", null, 1);
    public static readonly TypeItem FromDemand = new(2, "FromDemand", "OrderSourceType.FromDemand", "Talepten", "bi-megaphone", null, 2);

    public static IEnumerable<TypeItem> All => new[] { Direct, FromInquiry, FromDemand };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);
}

// ============================================================
// DEMAND FULFILLMENT STATUSES
// ============================================================
public static class DemandFulfillmentStatuses
{
    public static readonly TypeItem Open = new(0, "Open", "DemandFulfillmentStatus.Open", "Acik", "bi-circle", "bg-primary", 0, isDefault: true);
    public static readonly TypeItem PartiallyFulfilled = new(1, "PartiallyFulfilled", "DemandFulfillmentStatus.PartiallyFulfilled", "Kismen karsilandi", "bi-pie-chart", "bg-info", 1);
    public static readonly TypeItem FullyFulfilled = new(2, "FullyFulfilled", "DemandFulfillmentStatus.FullyFulfilled", "Tamamen karsilandi", "bi-check-circle", "bg-success", 2);

    public static IEnumerable<TypeItem> All => new[] { Open, PartiallyFulfilled, FullyFulfilled };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);
}

// ============================================================
// SERVICE TYPES (Lojistik, Gumruk, Sigorta, Gozetim)
// ============================================================
public static class ServiceTypes
{
    public static readonly TypeItem Logistics = new(1, "Logistics", "ServiceType.Logistics", "Lojistik/Tasima", "bi-truck", "bg-primary", 1, isDefault: true);
    public static readonly TypeItem Customs = new(2, "Customs", "ServiceType.Customs", "Gumruk Islemleri", "bi-flag", "bg-warning text-dark", 2);
    public static readonly TypeItem Insurance = new(3, "Insurance", "ServiceType.Insurance", "Sigorta", "bi-shield-check", "bg-success", 3);
    public static readonly TypeItem Survey = new(4, "Survey", "ServiceType.Survey", "Gozetim/Ekspertiz", "bi-search", "bg-info", 4);

    public static IEnumerable<TypeItem> All => new[] { Logistics, Customs, Insurance, Survey };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);
}

// ============================================================
// SERVICE REQUEST STATUSES
// ============================================================
public static class ServiceRequestStatuses
{
    public static readonly TypeItem Open = new(1, "Open", "ServiceRequestStatus.Open", "Teklif bekleniyor", "bi-hourglass-split", "bg-warning text-dark", 1, isDefault: true);
    public static readonly TypeItem QuotesReceived = new(2, "QuotesReceived", "ServiceRequestStatus.QuotesReceived", "Teklifler geldi", "bi-inbox-fill", "bg-info", 2);
    public static readonly TypeItem QuoteSelected = new(3, "QuoteSelected", "ServiceRequestStatus.QuoteSelected", "Teklif secildi", "bi-check-circle", "bg-success", 3);
    public static readonly TypeItem Cancelled = new(4, "Cancelled", "ServiceRequestStatus.Cancelled", "Iptal edildi", "bi-x-circle", "bg-danger", 4);

    public static IEnumerable<TypeItem> All => new[] { Open, QuotesReceived, QuoteSelected, Cancelled };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);
}

// ============================================================
// SURVEY TRIGGER STATUSES (Siparis Gozetim Tetikleme Durumu)
// ============================================================
public static class SurveyTriggerStatuses
{
    /// <summary>Gozetim istenmedi veya gerekli degil</summary>
    public static readonly TypeItem NotRequired = new(0, "NotRequired", "SurveyTriggerStatus.NotRequired", "Gozetim istenmedi", "bi-dash-circle", "bg-secondary", 0, isDefault: true);
    /// <summary>Gozetim istendi ama lojistik secimi bekleniyor</summary>
    public static readonly TypeItem WaitingForLogistics = new(1, "WaitingForLogistics", "SurveyTriggerStatus.WaitingForLogistics", "Lojistik secimi bekleniyor", "bi-hourglass-split", "bg-warning text-dark", 1);
    /// <summary>Lojistik secildi, Survey request olusturulabilir</summary>
    public static readonly TypeItem Ready = new(2, "Ready", "SurveyTriggerStatus.Ready", "Gozetim talebi olusturulabilir", "bi-check-circle", "bg-info", 2);
    /// <summary>Survey request olusturuldu</summary>
    public static readonly TypeItem Created = new(3, "Created", "SurveyTriggerStatus.Created", "Gozetim talebi olusturuldu", "bi-check-circle-fill", "bg-success", 3);

    public static IEnumerable<TypeItem> All => new[] { NotRequired, WaitingForLogistics, Ready, Created };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);
}

// ============================================================
// CHECKOUT STEPS (Siparis Olusturma Adimlari)
// ============================================================
/// <summary>
/// Checkout sureci adimlari - servis teklif akisi takibi icin
/// </summary>
public static class CheckoutSteps
{
    /// <summary>1. Siparis olusturuldu, servis talepleri gonderildi</summary>
    public static readonly TypeItem ServiceRequested = new(1, "ServiceRequested", "CheckoutStep.ServiceRequested",
        "Hizmet talepleri gonderildi", "bi-send-check", "bg-info", 1, isDefault: true);

    /// <summary>2. Servis saglayicilardan teklif bekleniyor</summary>
    public static readonly TypeItem WaitingForQuotes = new(2, "WaitingForQuotes", "CheckoutStep.WaitingForQuotes",
        "Teklifler bekleniyor", "bi-hourglass-split", "bg-warning text-dark", 2);

    /// <summary>3. Teklifler geldi, secim bekleniyor</summary>
    public static readonly TypeItem QuotesReceived = new(3, "QuotesReceived", "CheckoutStep.QuotesReceived",
        "Teklifler geldi", "bi-clipboard-data", "bg-primary", 3);

    /// <summary>4. Tum teklifler secildi</summary>
    public static readonly TypeItem QuotesSelected = new(4, "QuotesSelected", "CheckoutStep.QuotesSelected",
        "Teklifler secildi", "bi-check-circle", "bg-success", 4);

    /// <summary>5. Finansman sureci (opsiyonel)</summary>
    public static readonly TypeItem FinancingPending = new(5, "FinancingPending", "CheckoutStep.FinancingPending",
        "Finansman bekleniyor", "bi-cash-coin", "bg-info", 5);

    /// <summary>6. Odeme bekleniyor</summary>
    public static readonly TypeItem PaymentPending = new(6, "PaymentPending", "CheckoutStep.PaymentPending",
        "Odeme bekleniyor", "bi-credit-card", "bg-warning text-dark", 6);

    /// <summary>7. Tamamlandi - Siparis onaylandi</summary>
    public static readonly TypeItem Completed = new(7, "Completed", "CheckoutStep.Completed",
        "Siparis onaylandi", "bi-check-all", "bg-success", 7);

    public static IEnumerable<TypeItem> All => new[] { ServiceRequested, WaitingForQuotes, QuotesReceived, QuotesSelected, FinancingPending, PaymentPending, Completed };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);
}

// ============================================================
// SERVICE QUOTE STATUSES
// ============================================================
public static class ServiceQuoteStatuses
{
    public static readonly TypeItem Pending = new(1, "Pending", "ServiceQuoteStatus.Pending", "Degerlendirilmeyi bekliyor", "bi-hourglass-split", "bg-warning text-dark", 1, isDefault: true);
    public static readonly TypeItem Accepted = new(2, "Accepted", "ServiceQuoteStatus.Accepted", "Kabul edildi", "bi-check-circle", "bg-success", 2);
    public static readonly TypeItem Rejected = new(3, "Rejected", "ServiceQuoteStatus.Rejected", "Reddedildi", "bi-x-circle", "bg-danger", 3);
    public static readonly TypeItem Expired = new(4, "Expired", "ServiceQuoteStatus.Expired", "Suresi doldu", "bi-clock-history", "bg-secondary", 4);
    public static readonly TypeItem Withdrawn = new(5, "Withdrawn", "ServiceQuoteStatus.Withdrawn", "Geri cekildi", "bi-arrow-counterclockwise", "bg-secondary", 5);
    public static readonly TypeItem Completed = new(6, "Completed", "ServiceQuoteStatus.Completed", "Tamamlandi", "bi-check-all", "bg-primary", 6);

    public static IEnumerable<TypeItem> All => new[] { Pending, Accepted, Rejected, Expired, Withdrawn, Completed };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);
}

// ============================================================
// TRANSPORT MODES
// ============================================================
public static class TransportModes
{
    public static readonly TypeItem Road = new(1, "Road", "TransportMode.Road", "Karayolu", "bi-truck", null, 1, isDefault: true);
    public static readonly TypeItem Sea = new(2, "Sea", "TransportMode.Sea", "Denizyolu", "bi-water", null, 2);
    public static readonly TypeItem Air = new(3, "Air", "TransportMode.Air", "Havayolu", "bi-airplane", null, 3);
    public static readonly TypeItem Rail = new(4, "Rail", "TransportMode.Rail", "Demiryolu", "bi-train-front", null, 4);
    public static readonly TypeItem Multimodal = new(5, "Multimodal", "TransportMode.Multimodal", "Coklu tasima", "bi-diagram-3", null, 5);

    public static IEnumerable<TypeItem> All => new[] { Road, Sea, Air, Rail, Multimodal };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);
}

// ============================================================
// CUSTOMS OPERATION TYPES
// ============================================================
public static class CustomsOperationTypes
{
    public static readonly TypeItem Export = new(1, "Export", "CustomsOperationType.Export", "Ihracat", "bi-box-arrow-up-right", null, 1, isDefault: true);
    public static readonly TypeItem Import = new(2, "Import", "CustomsOperationType.Import", "Ithalat", "bi-box-arrow-in-down-left", null, 2);
    public static readonly TypeItem Transit = new(3, "Transit", "CustomsOperationType.Transit", "Transit", "bi-arrow-left-right", null, 3);

    public static IEnumerable<TypeItem> All => new[] { Export, Import, Transit };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);
}

// ============================================================
// INSURANCE TYPES
// ============================================================
public static class InsuranceTypes
{
    public static readonly TypeItem Cargo = new(1, "Cargo", "InsuranceType.Cargo", "Yuk sigortasi", "bi-box", null, 1, isDefault: true);
    public static readonly TypeItem Liability = new(2, "Liability", "InsuranceType.Liability", "Sorumluluk sigortasi", "bi-shield-exclamation", null, 2);
    public static readonly TypeItem AllRisk = new(3, "AllRisk", "InsuranceType.AllRisk", "Tam koruma", "bi-shield-fill-check", null, 3);

    public static IEnumerable<TypeItem> All => new[] { Cargo, Liability, AllRisk };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);
}

// ============================================================
// SURVEY TYPES (Gozetim Tipleri)
// ============================================================
public static class SurveyTypes
{
    public static readonly TypeItem PreLoading = new(1, "PreLoading", "SurveyType.PreLoading", "Yukleme Oncesi", "bi-box-arrow-up", null, 1, isDefault: true);
    public static readonly TypeItem Loading = new(2, "Loading", "SurveyType.Loading", "Yukleme", "bi-box-seam", null, 2);
    public static readonly TypeItem Unloading = new(3, "Unloading", "SurveyType.Unloading", "Bosaltma", "bi-box-arrow-down", null, 3);
    public static readonly TypeItem DamageAssessment = new(4, "DamageAssessment", "SurveyType.DamageAssessment", "Hasar Tespiti", "bi-exclamation-triangle", null, 4);

    public static IEnumerable<TypeItem> All => new[] { PreLoading, Loading, Unloading, DamageAssessment };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);
}

// ============================================================
// PARTICIPANT ROLES (Siparis katilimcilari)
// ============================================================
public static class ParticipantRoles
{
    public static readonly TypeItem Seller = new(1, "Seller", "ParticipantRole.Seller", "Urun saticisi", "bi-shop", "bg-primary", 1, isDefault: true);
    public static readonly TypeItem LogisticsProvider = new(2, "LogisticsProvider", "ParticipantRole.LogisticsProvider", "Lojistik firmasi", "bi-truck", "bg-info", 2);
    public static readonly TypeItem CustomsBroker = new(3, "CustomsBroker", "ParticipantRole.CustomsBroker", "Gumruk musaviri", "bi-flag", "bg-warning text-dark", 3);
    public static readonly TypeItem InsuranceProvider = new(4, "InsuranceProvider", "ParticipantRole.InsuranceProvider", "Sigorta firmasi", "bi-shield-check", "bg-success", 4);
    public static readonly TypeItem SurveyProvider = new(5, "SurveyProvider", "ParticipantRole.SurveyProvider", "Gozetim/Ekspertiz firmasi", "bi-search", "bg-info", 5);
    public static readonly TypeItem Investor = new(6, "Investor", "ParticipantRole.Investor", "Yatirimci/Finansman", "bi-cash-stack", "bg-success", 6);

    public static IEnumerable<TypeItem> All => new[] { Seller, LogisticsProvider, CustomsBroker, InsuranceProvider, SurveyProvider, Investor };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);
}

// ============================================================
// PARTICIPANT STATUSES
// ============================================================
public static class ParticipantStatuses
{
    public static readonly TypeItem Pending = new(1, "Pending", "ParticipantStatus.Pending", "Odeme bekleniyor", "bi-hourglass-split", "bg-warning text-dark", 1, isDefault: true);
    public static readonly TypeItem Active = new(2, "Active", "ParticipantStatus.Active", "Gorev basladi", "bi-play-circle", "bg-primary", 2);
    public static readonly TypeItem Completed = new(3, "Completed", "ParticipantStatus.Completed", "Tamamlandi", "bi-check-circle", "bg-success", 3);
    public static readonly TypeItem Disputed = new(4, "Disputed", "ParticipantStatus.Disputed", "Anlasmazlik", "bi-exclamation-diamond", "bg-danger", 4);

    public static IEnumerable<TypeItem> All => new[] { Pending, Active, Completed, Disputed };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);
}

// ============================================================
// TASK TYPES
// ============================================================
public static class TaskTypes
{
    public static readonly TypeItem PrepareOrder = new(1, "PrepareOrder", "TaskType.PrepareOrder", "Siparis hazirla", "bi-box-seam", null, 1, isDefault: true);
    public static readonly TypeItem ShipOrder = new(2, "ShipOrder", "TaskType.ShipOrder", "Kargoya ver", "bi-truck", null, 2);
    public static readonly TypeItem PrepareDocuments = new(3, "PrepareDocuments", "TaskType.PrepareDocuments", "Evrak hazirla", "bi-file-earmark-text", null, 3);
    public static readonly TypeItem CustomsClearance = new(4, "CustomsClearance", "TaskType.CustomsClearance", "Gumruk islemi", "bi-flag", null, 4);
    public static readonly TypeItem ArrangeTransport = new(5, "ArrangeTransport", "TaskType.ArrangeTransport", "Tasimayi duzenle", "bi-map", null, 5);
    public static readonly TypeItem IssueInsurance = new(6, "IssueInsurance", "TaskType.IssueInsurance", "Sigorta policesi duzenle", "bi-shield-check", null, 6);
    public static readonly TypeItem ConfirmDelivery = new(7, "ConfirmDelivery", "TaskType.ConfirmDelivery", "Teslimi onayla", "bi-house-check", null, 7);
    public static readonly TypeItem ProvideTracking = new(8, "ProvideTracking", "TaskType.ProvideTracking", "Takip bilgisi ver", "bi-geo-alt", null, 8);
    public static readonly TypeItem Custom = new(99, "Custom", "TaskType.Custom", "Ozel gorev", "bi-pencil-square", null, 99);

    public static IEnumerable<TypeItem> All => new[] { PrepareOrder, ShipOrder, PrepareDocuments, CustomsClearance, ArrangeTransport, IssueInsurance, ConfirmDelivery, ProvideTracking, Custom };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);
}

// ============================================================
// TASK STATUSES
// ============================================================
public static class TaskStatuses
{
    public static readonly TypeItem Pending = new(1, "Pending", "TaskStatus.Pending", "Bekliyor", "bi-hourglass-split", "bg-warning text-dark", 1, isDefault: true);
    public static readonly TypeItem InProgress = new(2, "InProgress", "TaskStatus.InProgress", "Devam ediyor", "bi-play-circle", "bg-primary", 2);
    public static readonly TypeItem Completed = new(3, "Completed", "TaskStatus.Completed", "Tamamlandi", "bi-check-circle", "bg-success", 3);
    public static readonly TypeItem Blocked = new(4, "Blocked", "TaskStatus.Blocked", "Engellendi", "bi-slash-circle", "bg-danger", 4);
    public static readonly TypeItem Skipped = new(5, "Skipped", "TaskStatus.Skipped", "Gecildi", "bi-skip-forward", "bg-secondary", 5);

    public static IEnumerable<TypeItem> All => new[] { Pending, InProgress, Completed, Blocked, Skipped };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);
}

// ============================================================
// INVESTMENT TYPES
// ============================================================
public static class InvestmentTypes
{
    public static readonly TypeItem Equity = new(1, "Equity", "InvestmentType.Equity", "Kar ortakligi", "bi-pie-chart", null, 1, isDefault: true);
    public static readonly TypeItem Loan = new(2, "Loan", "InvestmentType.Loan", "Kredi (faizli)", "bi-percent", null, 2);
    public static readonly TypeItem PrePayment = new(3, "PrePayment", "InvestmentType.PrePayment", "On odeme (indirimli)", "bi-cash", null, 3);

    public static IEnumerable<TypeItem> All => new[] { Equity, Loan, PrePayment };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);
}

// ============================================================
// INVESTMENT STATUSES
// ============================================================
public static class InvestmentStatuses
{
    public static readonly TypeItem Pending = new(1, "Pending", "InvestmentStatus.Pending", "Onay bekliyor", "bi-hourglass-split", "bg-warning text-dark", 1, isDefault: true);
    public static readonly TypeItem Active = new(2, "Active", "InvestmentStatus.Active", "Aktif", "bi-check-circle", "bg-success", 2);
    public static readonly TypeItem Repaid = new(3, "Repaid", "InvestmentStatus.Repaid", "Geri odendi", "bi-cash-stack", "bg-info", 3);
    public static readonly TypeItem Defaulted = new(4, "Defaulted", "InvestmentStatus.Defaulted", "Temerrut", "bi-exclamation-triangle", "bg-danger", 4);
    public static readonly TypeItem Cancelled = new(5, "Cancelled", "InvestmentStatus.Cancelled", "Iptal", "bi-x-circle", "bg-secondary", 5);

    public static IEnumerable<TypeItem> All => new[] { Pending, Active, Repaid, Defaulted, Cancelled };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);
}

// ============================================================
// FINANCING TYPES (Fatura, Siparis, Proje Finansmani)
// ============================================================
public static class FinancingTypes
{
    public static readonly TypeItem Invoice = new(1, "Invoice", "FinancingType.Invoice", "Fatura Finansmani", "bi-receipt", "bg-primary", 1, isDefault: true);
    public static readonly TypeItem Order = new(2, "Order", "FinancingType.Order", "Siparis Finansmani", "bi-cart-check", "bg-info", 2);
    public static readonly TypeItem Project = new(3, "Project", "FinancingType.Project", "Proje Finansmani", "bi-kanban", "bg-success", 3);

    public static IEnumerable<TypeItem> All => new[] { Invoice, Order, Project };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);
}

// ============================================================
// FINANCING REQUEST STATUSES
// ============================================================
public static class FinancingRequestStatuses
{
    public static readonly TypeItem Draft = new(1, "Draft", "FinancingRequestStatus.Draft", "Taslak", "bi-file-earmark", "bg-secondary", 1, isDefault: true);
    public static readonly TypeItem Open = new(2, "Open", "FinancingRequestStatus.Open", "Teklif bekleniyor", "bi-hourglass-split", "bg-warning text-dark", 2);
    public static readonly TypeItem OffersReceived = new(3, "OffersReceived", "FinancingRequestStatus.OffersReceived", "Teklifler geldi", "bi-inbox-fill", "bg-info", 3);
    public static readonly TypeItem PartiallyFunded = new(4, "PartiallyFunded", "FinancingRequestStatus.PartiallyFunded", "Kismi finanse", "bi-pie-chart", "bg-info", 4);
    public static readonly TypeItem FullyFunded = new(5, "FullyFunded", "FinancingRequestStatus.FullyFunded", "Tam finanse", "bi-check-circle", "bg-success", 5);
    public static readonly TypeItem Repaid = new(6, "Repaid", "FinancingRequestStatus.Repaid", "Geri odendi", "bi-cash-stack", "bg-success", 6);
    public static readonly TypeItem Defaulted = new(7, "Defaulted", "FinancingRequestStatus.Defaulted", "Temerrut", "bi-exclamation-triangle", "bg-danger", 7);
    public static readonly TypeItem Cancelled = new(8, "Cancelled", "FinancingRequestStatus.Cancelled", "Iptal", "bi-x-circle", "bg-secondary", 8);

    public static IEnumerable<TypeItem> All => new[] { Draft, Open, OffersReceived, PartiallyFunded, FullyFunded, Repaid, Defaulted, Cancelled };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);

    // Aktif talepler (yatirimci gorebilir)
    public static IEnumerable<TypeItem> Active => new[] { Open, OffersReceived, PartiallyFunded };
}

// ============================================================
// COLLATERAL TYPES (Teminat Tipleri)
// ============================================================
public static class CollateralTypes
{
    public static readonly TypeItem Invoice = new(1, "Invoice", "CollateralType.Invoice", "Fatura", "bi-receipt", null, 1, isDefault: true);
    public static readonly TypeItem Order = new(2, "Order", "CollateralType.Order", "Siparis", "bi-cart-check", null, 2);
    public static readonly TypeItem Inventory = new(3, "Inventory", "CollateralType.Inventory", "Stok", "bi-box-seam", null, 3);
    public static readonly TypeItem Receivables = new(4, "Receivables", "CollateralType.Receivables", "Alacaklar", "bi-journal-text", null, 4);
    public static readonly TypeItem None = new(5, "None", "CollateralType.None", "Teminatsiz", "bi-slash-circle", null, 5);

    public static IEnumerable<TypeItem> All => new[] { Invoice, Order, Inventory, Receivables, None };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);
}

// ============================================================
// INVESTMENT OFFER STATUSES
// ============================================================
public static class InvestmentOfferStatuses
{
    public static readonly TypeItem Pending = new(1, "Pending", "InvestmentOfferStatus.Pending", "Beklemede", "bi-hourglass-split", "bg-warning text-dark", 1, isDefault: true);
    public static readonly TypeItem Accepted = new(2, "Accepted", "InvestmentOfferStatus.Accepted", "Kabul edildi", "bi-check-circle", "bg-success", 2);
    public static readonly TypeItem Rejected = new(3, "Rejected", "InvestmentOfferStatus.Rejected", "Reddedildi", "bi-x-circle", "bg-danger", 3);
    public static readonly TypeItem Withdrawn = new(4, "Withdrawn", "InvestmentOfferStatus.Withdrawn", "Geri cekildi", "bi-arrow-counterclockwise", "bg-secondary", 4);
    public static readonly TypeItem Expired = new(5, "Expired", "InvestmentOfferStatus.Expired", "Suresi doldu", "bi-clock-history", "bg-secondary", 5);
    public static readonly TypeItem Funded = new(6, "Funded", "InvestmentOfferStatus.Funded", "Finanse edildi", "bi-cash-stack", "bg-success", 6);

    public static IEnumerable<TypeItem> All => new[] { Pending, Accepted, Rejected, Withdrawn, Expired, Funded };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);
}

// ============================================================
// CAPABILITY REQUEST STATUSES
// ============================================================
public static class CapabilityRequestStatuses
{
    public static readonly TypeItem Pending = new(1, "Pending", "CapabilityRequestStatus.Pending", "Beklemede", "bi-hourglass-split", "bg-warning text-dark", 1, isDefault: true);
    public static readonly TypeItem Approved = new(2, "Approved", "CapabilityRequestStatus.Approved", "Onaylandi", "bi-check-circle", "bg-success", 2);
    public static readonly TypeItem Rejected = new(3, "Rejected", "CapabilityRequestStatus.Rejected", "Reddedildi", "bi-x-circle", "bg-danger", 3);

    public static IEnumerable<TypeItem> All => new[] { Pending, Approved, Rejected };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);
}

// ============================================================
// CAPABILITIES (Firma Yetkinlikleri)
// VendorCapabilities tablosu yerine - sabit degerler
// ============================================================
public static class Capabilities
{
    public static readonly TypeItem Seller = new(2, "Seller", "Capability.Seller", "Satici", "bi-shop", "bg-primary", 1);
    public static readonly TypeItem Buyer = new(3, "Buyer", "Capability.Buyer", "Alici", "bi-cart", "bg-info", 2);
    public static readonly TypeItem Carrier = new(4, "Carrier", "Capability.Carrier", "Tasimaci", "bi-truck", "bg-warning text-dark", 3);
    public static readonly TypeItem Insurance = new(5, "Insurance", "Capability.Insurance", "Sigorta", "bi-shield-check", "bg-success", 4);
    public static readonly TypeItem Customs = new(6, "Customs", "Capability.Customs", "Gumruk", "bi-building-check", "bg-secondary", 5);
    public static readonly TypeItem Survey = new(7, "Survey", "Capability.Survey", "Gozetim", "bi-clipboard-check", "bg-info", 6);
    public static readonly TypeItem Investor = new(8, "Investor", "Capability.Investor", "Yatirimci", "bi-cash-coin", "bg-success", 7);

    public static IEnumerable<TypeItem> All => new[] { Seller, Buyer, Carrier, Insurance, Customs, Survey, Investor };
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);

    // Servis saglayicilar (Lojistik, Sigorta, Gumruk, Gozetim)
    public static IEnumerable<TypeItem> Providers => new[] { Carrier, Insurance, Customs, Survey };

    // ID sabitleri - kod icerisinde direkt kullanim icin
    public static class Ids
    {
        public const int Seller = 2;
        public const int Buyer = 3;
        public const int Carrier = 4;
        public const int Insurance = 5;
        public const int Customs = 6;
        public const int Survey = 7;
        public const int Investor = 8;
    }
}

// ============================================================
// DOCUMENT TYPES
// ============================================================
public static class DocumentTypes
{
    public static readonly TypeItem Invoice = new(1, "Invoice", "DocumentType.Invoice", "Fatura", "bi-receipt", null, 1, isDefault: true);
    public static readonly TypeItem Contract = new(2, "Contract", "DocumentType.Contract", "Sozlesme", "bi-file-earmark-text", null, 2);
    public static readonly TypeItem Proposal = new(3, "Proposal", "DocumentType.Proposal", "Teklif", "bi-file-earmark-richtext", null, 3);
    public static readonly TypeItem Order = new(4, "Order", "DocumentType.Order", "Siparis Formu", "bi-cart", null, 4);
    public static readonly TypeItem Waybill = new(5, "Waybill", "DocumentType.Waybill", "Irsaliye", "bi-truck", null, 5);

    public static IEnumerable<TypeItem> All => new[] { Invoice, Contract, Proposal, Order, Waybill };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);

    /// <summary>
    /// Belge numarasi onek
    /// </summary>
    public static string GetPrefix(int documentTypeId) => documentTypeId switch
    {
        1 => "INV",   // Invoice
        2 => "CNT",   // Contract
        3 => "PRP",   // Proposal
        4 => "ORD",   // Order
        5 => "WBL",   // Waybill
        _ => "DOC"
    };

    public static class Ids
    {
        public const int Invoice = 1;
        public const int Contract = 2;
        public const int Proposal = 3;
        public const int Order = 4;
        public const int Waybill = 5;
    }
}

// ============================================================
// DOCUMENT STATUSES
// ============================================================
public static class DocumentStatuses
{
    public static readonly TypeItem Draft = new(1, "Draft", "DocumentStatus.Draft", "Taslak", "bi-file-earmark", "bg-secondary", 1, isDefault: true);
    public static readonly TypeItem Pending = new(2, "Pending", "DocumentStatus.Pending", "Onay Bekliyor", "bi-hourglass-split", "bg-warning text-dark", 2);
    public static readonly TypeItem Approved = new(3, "Approved", "DocumentStatus.Approved", "Onaylandi", "bi-check-circle", "bg-success", 3);
    public static readonly TypeItem Rejected = new(4, "Rejected", "DocumentStatus.Rejected", "Reddedildi", "bi-x-circle", "bg-danger", 4);
    public static readonly TypeItem Signed = new(5, "Signed", "DocumentStatus.Signed", "Imzalandi", "bi-pen", "bg-info", 5);
    public static readonly TypeItem Cancelled = new(6, "Cancelled", "DocumentStatus.Cancelled", "Iptal Edildi", "bi-x-lg", "bg-dark", 6);
    public static readonly TypeItem PartiallySigned = new(7, "PartiallySigned", "DocumentStatus.PartiallySigned", "Kismi Imzali", "bi-pen-fill", "bg-info", 7);
    public static readonly TypeItem NeedsSignature = new(8, "NeedsSignature", "DocumentStatus.NeedsSignature", "Imza Bekliyor", "bi-pencil-square", "bg-warning", 8);

    public static IEnumerable<TypeItem> All => new[] { Draft, Pending, Approved, Rejected, Signed, Cancelled, PartiallySigned, NeedsSignature };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);

    public static class Ids
    {
        public const int Draft = 1;
        public const int Pending = 2;
        public const int Approved = 3;
        public const int Rejected = 4;
        public const int Signed = 5;
        public const int Cancelled = 6;
        public const int PartiallySigned = 7;
        public const int NeedsSignature = 8;
    }
}

// ============================================================
// SIGNATURE TYPES
// ============================================================
public static class SignatureTypes
{
    public static readonly TypeItem Electronic = new(1, "Electronic", "SignatureType.Electronic", "Elektronik Imza", "bi-pen", null, 1, isDefault: true);
    public static readonly TypeItem Digital = new(2, "Digital", "SignatureType.Digital", "Dijital Imza", "bi-shield-check", null, 2);
    public static readonly TypeItem Handwritten = new(3, "Handwritten", "SignatureType.Handwritten", "El Yazisi Imza", "bi-pencil", null, 3);

    public static IEnumerable<TypeItem> All => new[] { Electronic, Digital, Handwritten };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);

    public static class Ids
    {
        public const int Electronic = 1;
        public const int Digital = 2;
        public const int Handwritten = 3;
    }
}

// ============================================================
// INCOTERMS 2020 (Uluslararasi Ticari Terimler)
// ============================================================
public static class Incoterms
{
    // Tum tasima modlari icin (7 terim)
    public static readonly TypeItem EXW = new(1, "EXW", "Incoterm.EXW", "Ex Works - Is Yerinde Teslim", "bi-building", "bg-secondary", 1);
    public static readonly TypeItem FCA = new(2, "FCA", "Incoterm.FCA", "Free Carrier - Tasiyiciya Teslim", "bi-box-seam", "bg-info", 2);
    public static readonly TypeItem CPT = new(3, "CPT", "Incoterm.CPT", "Carriage Paid To - Tasima Odenmis", "bi-truck", "bg-info", 3);
    public static readonly TypeItem CIP = new(4, "CIP", "Incoterm.CIP", "Carriage & Insurance Paid - Tasima ve Sigorta Odenmis", "bi-shield-check", "bg-primary", 4);
    public static readonly TypeItem DAP = new(5, "DAP", "Incoterm.DAP", "Delivered at Place - Belirlenen Yerde Teslim", "bi-geo-alt", "bg-success", 5);
    public static readonly TypeItem DPU = new(6, "DPU", "Incoterm.DPU", "Delivered at Place Unloaded - Bosaltilmis Teslim", "bi-box-arrow-down", "bg-success", 6);
    public static readonly TypeItem DDP = new(7, "DDP", "Incoterm.DDP", "Delivered Duty Paid - Gumruk Vergisi Odenmis Teslim", "bi-house-check", "bg-success", 7, isDefault: true);

    // Sadece deniz/ic su tasimaciligi icin (4 terim)
    public static readonly TypeItem FAS = new(8, "FAS", "Incoterm.FAS", "Free Alongside Ship - Gemi Yaninda Teslim", "bi-water", "bg-primary", 8);
    public static readonly TypeItem FOB = new(9, "FOB", "Incoterm.FOB", "Free on Board - Gemi Bordasinda Teslim", "bi-water", "bg-primary", 9);
    public static readonly TypeItem CFR = new(10, "CFR", "Incoterm.CFR", "Cost and Freight - Mal Bedeli ve Navlun", "bi-water", "bg-info", 10);
    public static readonly TypeItem CIF = new(11, "CIF", "Incoterm.CIF", "Cost, Insurance & Freight - Mal Bedeli, Sigorta ve Navlun", "bi-water", "bg-info", 11);

    // Gruplamalar
    public static IEnumerable<TypeItem> AllModes => new[] { EXW, FCA, CPT, CIP, DAP, DPU, DDP };
    public static IEnumerable<TypeItem> SeaOnly => new[] { FAS, FOB, CFR, CIF };
    public static IEnumerable<TypeItem> All => AllModes.Concat(SeaOnly);
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);

    public static class Ids
    {
        public const int EXW = 1;
        public const int FCA = 2;
        public const int CPT = 3;
        public const int CIP = 4;
        public const int DAP = 5;
        public const int DPU = 6;
        public const int DDP = 7;
        public const int FAS = 8;
        public const int FOB = 9;
        public const int CFR = 10;
        public const int CIF = 11;
    }
}

// ============================================================
// INCOTERM RESPONSIBILITY TYPES (Sorumluluk Tipleri)
// ============================================================
public static class IncotermResponsibilityTypes
{
    public static readonly TypeItem ExportCustoms = new(1, "ExportCustoms", "ResponsibilityType.ExportCustoms", "Ihracat Gumruk", "bi-flag", "bg-warning text-dark", 1);
    public static readonly TypeItem Loading = new(2, "Loading", "ResponsibilityType.Loading", "Yukleme", "bi-box-arrow-up", "bg-info", 2);
    public static readonly TypeItem MainCarriage = new(3, "MainCarriage", "ResponsibilityType.MainCarriage", "Ana Tasima (Navlun)", "bi-truck", "bg-primary", 3);
    public static readonly TypeItem Insurance = new(4, "Insurance", "ResponsibilityType.Insurance", "Sigorta", "bi-shield-check", "bg-success", 4);
    public static readonly TypeItem ImportCustoms = new(5, "ImportCustoms", "ResponsibilityType.ImportCustoms", "Ithalat Gumruk", "bi-flag-fill", "bg-danger", 5);
    public static readonly TypeItem Unloading = new(6, "Unloading", "ResponsibilityType.Unloading", "Bosaltma", "bi-box-arrow-down", "bg-secondary", 6);

    public static IEnumerable<TypeItem> All => new[] { ExportCustoms, Loading, MainCarriage, Insurance, ImportCustoms, Unloading };
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);

    public static class Ids
    {
        public const int ExportCustoms = 1;
        public const int Loading = 2;
        public const int MainCarriage = 3;
        public const int Insurance = 4;
        public const int ImportCustoms = 5;
        public const int Unloading = 6;
    }
}

// ============================================================
// INCOTERM RESPONSIBILITIES (Sorumluluk Matrisi)
// Kim hangi islemden sorumlu?
// ============================================================
public static class IncotermResponsibilities
{
    // Sorumlu taraf
    public const int Seller = 1;
    public const int Buyer = 2;
    public const int Optional = 3;

    // Her Incoterm icin sorumluluk matrisi
    // Key: IncotermId, Value: Dictionary<ResponsibilityTypeId, ResponsibleParty>
    private static readonly Dictionary<int, Dictionary<int, int>> _matrix = new()
    {
        // EXW: Tum sorumluluk alicida (satici sadece mali hazirlar)
        [Incoterms.Ids.EXW] = new() {
            { IncotermResponsibilityTypes.Ids.ExportCustoms, Buyer },
            { IncotermResponsibilityTypes.Ids.Loading, Buyer },
            { IncotermResponsibilityTypes.Ids.MainCarriage, Buyer },
            { IncotermResponsibilityTypes.Ids.Insurance, Buyer },
            { IncotermResponsibilityTypes.Ids.ImportCustoms, Buyer },
            { IncotermResponsibilityTypes.Ids.Unloading, Buyer }
        },
        // FCA: Satici tasiyiciya teslim eder, ihracat gumrugunu yapar
        [Incoterms.Ids.FCA] = new() {
            { IncotermResponsibilityTypes.Ids.ExportCustoms, Seller },
            { IncotermResponsibilityTypes.Ids.Loading, Seller },
            { IncotermResponsibilityTypes.Ids.MainCarriage, Buyer },
            { IncotermResponsibilityTypes.Ids.Insurance, Buyer },
            { IncotermResponsibilityTypes.Ids.ImportCustoms, Buyer },
            { IncotermResponsibilityTypes.Ids.Unloading, Buyer }
        },
        // CPT: Satici navlunu oder ama risk tasiyiciya teslimde gecer
        [Incoterms.Ids.CPT] = new() {
            { IncotermResponsibilityTypes.Ids.ExportCustoms, Seller },
            { IncotermResponsibilityTypes.Ids.Loading, Seller },
            { IncotermResponsibilityTypes.Ids.MainCarriage, Seller },
            { IncotermResponsibilityTypes.Ids.Insurance, Buyer },
            { IncotermResponsibilityTypes.Ids.ImportCustoms, Buyer },
            { IncotermResponsibilityTypes.Ids.Unloading, Buyer }
        },
        // CIP: CPT + Satici sigorta da yaptirir
        [Incoterms.Ids.CIP] = new() {
            { IncotermResponsibilityTypes.Ids.ExportCustoms, Seller },
            { IncotermResponsibilityTypes.Ids.Loading, Seller },
            { IncotermResponsibilityTypes.Ids.MainCarriage, Seller },
            { IncotermResponsibilityTypes.Ids.Insurance, Seller },
            { IncotermResponsibilityTypes.Ids.ImportCustoms, Buyer },
            { IncotermResponsibilityTypes.Ids.Unloading, Buyer }
        },
        // DAP: Satici varis noktasinda bosaltmaya hazir teslim eder
        [Incoterms.Ids.DAP] = new() {
            { IncotermResponsibilityTypes.Ids.ExportCustoms, Seller },
            { IncotermResponsibilityTypes.Ids.Loading, Seller },
            { IncotermResponsibilityTypes.Ids.MainCarriage, Seller },
            { IncotermResponsibilityTypes.Ids.Insurance, Optional },
            { IncotermResponsibilityTypes.Ids.ImportCustoms, Buyer },
            { IncotermResponsibilityTypes.Ids.Unloading, Buyer }
        },
        // DPU: Satici bosaltma dahil yapar
        [Incoterms.Ids.DPU] = new() {
            { IncotermResponsibilityTypes.Ids.ExportCustoms, Seller },
            { IncotermResponsibilityTypes.Ids.Loading, Seller },
            { IncotermResponsibilityTypes.Ids.MainCarriage, Seller },
            { IncotermResponsibilityTypes.Ids.Insurance, Optional },
            { IncotermResponsibilityTypes.Ids.ImportCustoms, Buyer },
            { IncotermResponsibilityTypes.Ids.Unloading, Seller }
        },
        // DDP: Tum sorumluluk saticida (kapiya teslim)
        [Incoterms.Ids.DDP] = new() {
            { IncotermResponsibilityTypes.Ids.ExportCustoms, Seller },
            { IncotermResponsibilityTypes.Ids.Loading, Seller },
            { IncotermResponsibilityTypes.Ids.MainCarriage, Seller },
            { IncotermResponsibilityTypes.Ids.Insurance, Seller },
            { IncotermResponsibilityTypes.Ids.ImportCustoms, Seller },
            { IncotermResponsibilityTypes.Ids.Unloading, Seller }
        },
        // FAS: Satici mali gemi yanina getirir
        [Incoterms.Ids.FAS] = new() {
            { IncotermResponsibilityTypes.Ids.ExportCustoms, Seller },
            { IncotermResponsibilityTypes.Ids.Loading, Buyer },
            { IncotermResponsibilityTypes.Ids.MainCarriage, Buyer },
            { IncotermResponsibilityTypes.Ids.Insurance, Buyer },
            { IncotermResponsibilityTypes.Ids.ImportCustoms, Buyer },
            { IncotermResponsibilityTypes.Ids.Unloading, Buyer }
        },
        // FOB: Satici mali gemiye yukler
        [Incoterms.Ids.FOB] = new() {
            { IncotermResponsibilityTypes.Ids.ExportCustoms, Seller },
            { IncotermResponsibilityTypes.Ids.Loading, Seller },
            { IncotermResponsibilityTypes.Ids.MainCarriage, Buyer },
            { IncotermResponsibilityTypes.Ids.Insurance, Buyer },
            { IncotermResponsibilityTypes.Ids.ImportCustoms, Buyer },
            { IncotermResponsibilityTypes.Ids.Unloading, Buyer }
        },
        // CFR: Satici navlunu oder ama risk gemiye yuklemede gecer
        [Incoterms.Ids.CFR] = new() {
            { IncotermResponsibilityTypes.Ids.ExportCustoms, Seller },
            { IncotermResponsibilityTypes.Ids.Loading, Seller },
            { IncotermResponsibilityTypes.Ids.MainCarriage, Seller },
            { IncotermResponsibilityTypes.Ids.Insurance, Buyer },
            { IncotermResponsibilityTypes.Ids.ImportCustoms, Buyer },
            { IncotermResponsibilityTypes.Ids.Unloading, Buyer }
        },
        // CIF: CFR + Satici sigorta da yaptirir
        [Incoterms.Ids.CIF] = new() {
            { IncotermResponsibilityTypes.Ids.ExportCustoms, Seller },
            { IncotermResponsibilityTypes.Ids.Loading, Seller },
            { IncotermResponsibilityTypes.Ids.MainCarriage, Seller },
            { IncotermResponsibilityTypes.Ids.Insurance, Seller },
            { IncotermResponsibilityTypes.Ids.ImportCustoms, Buyer },
            { IncotermResponsibilityTypes.Ids.Unloading, Buyer }
        }
    };

    /// <summary>
    /// Belirli bir Incoterm ve sorumluluk tipi icin sorumlu taraf
    /// </summary>
    public static int GetResponsibleParty(int incotermId, int responsibilityTypeId)
    {
        if (_matrix.TryGetValue(incotermId, out var responsibilities))
            if (responsibilities.TryGetValue(responsibilityTypeId, out var party))
                return party;
        return Buyer; // Default
    }

    /// <summary>
    /// Bir tarafin sorumlu oldugu tum kalemler
    /// </summary>
    public static List<int> GetResponsibilities(int incotermId, bool isSeller)
    {
        var target = isSeller ? Seller : Buyer;
        if (!_matrix.TryGetValue(incotermId, out var responsibilities))
            return new List<int>();

        return responsibilities
            .Where(r => r.Value == target)
            .Select(r => r.Key)
            .ToList();
    }

    /// <summary>
    /// Tum sorumluluklari al (UI icin)
    /// </summary>
    public static Dictionary<int, int> GetAllResponsibilities(int incotermId)
    {
        if (_matrix.TryGetValue(incotermId, out var responsibilities))
            return responsibilities;
        return new Dictionary<int, int>();
    }
}

// ============================================================
// FINANCING SUBJECTS (Finansman Konulari)
// Hangi masraf icin finansman isteniyor?
// ============================================================
public static class FinancingSubjects
{
    public static readonly TypeItem ProductCost = new(1, "ProductCost", "FinancingSubject.ProductCost", "Mal Bedeli", "bi-box", "bg-primary", 1, isDefault: true);
    public static readonly TypeItem Logistics = new(2, "Logistics", "FinancingSubject.Logistics", "Lojistik/Navlun", "bi-truck", "bg-info", 2);
    public static readonly TypeItem Insurance = new(3, "Insurance", "FinancingSubject.Insurance", "Sigorta", "bi-shield-check", "bg-success", 3);
    public static readonly TypeItem ExportCustoms = new(4, "ExportCustoms", "FinancingSubject.ExportCustoms", "Ihracat Gumruk", "bi-flag", "bg-warning text-dark", 4);
    public static readonly TypeItem ImportCustoms = new(5, "ImportCustoms", "FinancingSubject.ImportCustoms", "Ithalat Gumruk", "bi-flag-fill", "bg-danger", 5);

    public static IEnumerable<TypeItem> All => new[] { ProductCost, Logistics, Insurance, ExportCustoms, ImportCustoms };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);

    public static class Ids
    {
        public const int ProductCost = 1;
        public const int Logistics = 2;
        public const int Insurance = 3;
        public const int ExportCustoms = 4;
        public const int ImportCustoms = 5;
    }

    /// <summary>
    /// ResponsibilityType'dan FinancingSubject'e mapping
    /// </summary>
    public static int? FromResponsibilityType(int responsibilityTypeId) => responsibilityTypeId switch
    {
        IncotermResponsibilityTypes.Ids.MainCarriage => Ids.Logistics,
        IncotermResponsibilityTypes.Ids.Insurance => Ids.Insurance,
        IncotermResponsibilityTypes.Ids.ExportCustoms => Ids.ExportCustoms,
        IncotermResponsibilityTypes.Ids.ImportCustoms => Ids.ImportCustoms,
        _ => null
    };
}

// ============================================================
// UNIT TYPES (Olcu Birimleri) - GS1 Standardina Uygun
// ============================================================
public static class UnitTypes
{
    // === TEMEL BIRIMLER (Base Units / Each) ===
    // Urunun en kucuk olculebilir birimi - Product.SalesUnitId icin kullanilir
    public static readonly TypeItem Piece = new(1, "Piece", "UnitType.Piece", "Adet", "bi-box", null, 1, isDefault: true);
    public static readonly TypeItem Set = new(2, "Set", "UnitType.Set", "Takim/Set", "bi-boxes", null, 2);
    public static readonly TypeItem Pair = new(3, "Pair", "UnitType.Pair", "Cift", "bi-symmetry-horizontal", null, 3);
    public static readonly TypeItem Gram = new(10, "Gram", "UnitType.Gram", "Gram (g)", "bi-speedometer", null, 10);
    public static readonly TypeItem Kilogram = new(11, "Kilogram", "UnitType.Kilogram", "Kilogram (kg)", "bi-speedometer2", null, 11);
    public static readonly TypeItem Ton = new(12, "Ton", "UnitType.Ton", "Ton", "bi-truck-flatbed", null, 12);
    public static readonly TypeItem Meter = new(20, "Meter", "UnitType.Meter", "Metre (m)", "bi-rulers", null, 20);
    public static readonly TypeItem Centimeter = new(21, "Centimeter", "UnitType.Centimeter", "Santimetre (cm)", "bi-rulers", null, 21);
    public static readonly TypeItem Liter = new(30, "Liter", "UnitType.Liter", "Litre (L)", "bi-droplet", null, 30);
    public static readonly TypeItem CubicMeter = new(31, "CubicMeter", "UnitType.CubicMeter", "Metrekup (m3)", "bi-box", null, 31);
    public static readonly TypeItem SquareMeter = new(40, "SquareMeter", "UnitType.SquareMeter", "Metrekare (m2)", "bi-bounding-box", null, 40);

    // === PAKETLEME BIRIMLERI (Packaging Units) ===
    // Temel birimlerin gruplanmis hali - ProductPackaging.UnitId icin kullanilir
    public static readonly TypeItem Pack = new(4, "Pack", "UnitType.Pack", "Paket", "bi-box-seam", "bg-info", 4);
    public static readonly TypeItem Box = new(5, "Box", "UnitType.Box", "Kutu", "bi-box2", "bg-info", 5);
    public static readonly TypeItem Carton = new(6, "Carton", "UnitType.Carton", "Koli", "bi-archive", "bg-warning", 6);
    public static readonly TypeItem Pallet = new(7, "Pallet", "UnitType.Pallet", "Palet", "bi-grid-3x3", "bg-success", 7);
    public static readonly TypeItem Container = new(8, "Container", "UnitType.Container", "Konteyner", "bi-truck", "bg-dark", 8);
    public static readonly TypeItem Roll = new(22, "Roll", "UnitType.Roll", "Rulo", "bi-vinyl", "bg-info", 22);

    // Tum birimler
    public static IEnumerable<TypeItem> All => new[] {
        Piece, Set, Pair,
        Gram, Kilogram, Ton,
        Meter, Centimeter,
        Liter, CubicMeter, SquareMeter,
        Pack, Box, Carton, Pallet, Container, Roll
    };

    // Temel birimler (Product.SalesUnitId icin)
    public static IEnumerable<TypeItem> BaseUnits => new[] {
        Piece, Set, Pair,
        Gram, Kilogram, Ton,
        Meter, Centimeter,
        Liter, CubicMeter, SquareMeter
    };

    // Paketleme birimleri (ProductPackaging.UnitId icin)
    public static IEnumerable<TypeItem> PackagingUnits => new[] {
        Pack, Box, Carton, Pallet, Container, Roll
    };

    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);

    /// <summary>
    /// Birimin paketleme birimi olup olmadigini kontrol eder
    /// </summary>
    public static bool IsPackagingUnit(int id) => PackagingUnits.Any(x => x.Id == id);

    public static class Ids
    {
        // Temel birimler
        public const int Piece = 1;
        public const int Set = 2;
        public const int Pair = 3;
        public const int Gram = 10;
        public const int Kilogram = 11;
        public const int Ton = 12;
        public const int Meter = 20;
        public const int Centimeter = 21;
        public const int Liter = 30;
        public const int CubicMeter = 31;
        public const int SquareMeter = 40;
        // Paketleme birimleri
        public const int Pack = 4;
        public const int Box = 5;
        public const int Carton = 6;
        public const int Pallet = 7;
        public const int Container = 8;
        public const int Roll = 22;
    }
}

// PackagingLevels KALDIRILDI - UnitTypes.PackagingUnits kullanilmali
// GS1 standardina gore paketleme birimleri (Pack, Box, Carton, Pallet, Container, Roll)
// artik UnitTypes icerisinde tanimli

// ============================================================
// NEGOTIATION ROUND STATUSES (Pazarlik Turu Durumlari)
// ============================================================
public static class NegotiationRoundStatuses
{
    public static readonly TypeItem Active = new(1, "Active", "NegotiationRoundStatus.Active", "Yanit bekleniyor", "bi-hourglass-split", "bg-warning text-dark", 1, isDefault: true);
    public static readonly TypeItem Countered = new(2, "Countered", "NegotiationRoundStatus.Countered", "Karsi teklif verildi", "bi-arrow-left-right", "bg-info", 2);
    public static readonly TypeItem Accepted = new(3, "Accepted", "NegotiationRoundStatus.Accepted", "Kabul edildi", "bi-check-circle", "bg-success", 3);
    public static readonly TypeItem Rejected = new(4, "Rejected", "NegotiationRoundStatus.Rejected", "Reddedildi", "bi-x-circle", "bg-danger", 4);
    public static readonly TypeItem Expired = new(5, "Expired", "NegotiationRoundStatus.Expired", "Suresi doldu", "bi-clock-history", "bg-secondary", 5);
    public static readonly TypeItem Withdrawn = new(6, "Withdrawn", "NegotiationRoundStatus.Withdrawn", "Geri cekildi", "bi-arrow-counterclockwise", "bg-dark", 6);

    public static IEnumerable<TypeItem> All => new[] { Active, Countered, Accepted, Rejected, Expired, Withdrawn };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);

    public static class Ids
    {
        public const int Active = 1;
        public const int Countered = 2;
        public const int Accepted = 3;
        public const int Rejected = 4;
        public const int Expired = 5;
        public const int Withdrawn = 6;
    }
}

// ============================================================
// DEMAND RESPONSE STATUSES (Talep Yanit Durumlari)
// ============================================================
public static class DemandResponseStatuses
{
    public static readonly TypeItem Pending = new(1, "Pending", "DemandResponseStatus.Pending", "Beklemede", "bi-hourglass-split", "bg-warning text-dark", 1, isDefault: true);
    public static readonly TypeItem Viewed = new(2, "Viewed", "DemandResponseStatus.Viewed", "Goruldu", "bi-eye", "bg-info", 2);
    public static readonly TypeItem Shortlisted = new(3, "Shortlisted", "DemandResponseStatus.Shortlisted", "Kisa listede", "bi-star", "bg-primary", 3);
    public static readonly TypeItem Accepted = new(4, "Accepted", "DemandResponseStatus.Accepted", "Kabul edildi", "bi-check-circle", "bg-success", 4);
    public static readonly TypeItem Rejected = new(5, "Rejected", "DemandResponseStatus.Rejected", "Reddedildi", "bi-x-circle", "bg-danger", 5);
    public static readonly TypeItem Negotiating = new(6, "Negotiating", "DemandResponseStatus.Negotiating", "Pazarlik suruyor", "bi-arrow-left-right", "bg-purple", 6);

    public static IEnumerable<TypeItem> All => new[] { Pending, Viewed, Shortlisted, Accepted, Rejected, Negotiating };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);

    public static class Ids
    {
        public const int Pending = 1;
        public const int Viewed = 2;
        public const int Shortlisted = 3;
        public const int Accepted = 4;
        public const int Rejected = 5;
        public const int Negotiating = 6;
    }
}

// ============================================================
// CUSTOMS DECLARATION TYPES (Beyanname Tipleri)
// ============================================================
public static class CustomsDeclarationTypes
{
    public static readonly TypeItem Export = new(1, "Export", "CustomsDeclarationType.Export", "Ihracat Beyannamesi", "bi-box-arrow-up-right", "bg-primary", 1, isDefault: true);
    public static readonly TypeItem Import = new(2, "Import", "CustomsDeclarationType.Import", "Ithalat Beyannamesi", "bi-box-arrow-in-down-left", "bg-info", 2);
    public static readonly TypeItem ETGB = new(3, "ETGB", "CustomsDeclarationType.ETGB", "ETGB (Mikro Ihracat)", "bi-envelope", "bg-success", 3);

    public static IEnumerable<TypeItem> All => new[] { Export, Import, ETGB };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);

    /// <summary>
    /// Evrim dosya tipi kodu donusumu
    /// </summary>
    public static string ToEvrimDosyaTipi(int typeId) => typeId switch
    {
        1 => "IHR",
        2 => "ITH",
        3 => "ETGB",
        _ => "IHR"
    };

    /// <summary>
    /// Evrim dosya tipinden TypeId'ye donusum
    /// </summary>
    public static int FromEvrimDosyaTipi(string dosyaTipi) => dosyaTipi?.ToUpperInvariant() switch
    {
        "IHR" => 1,
        "ITH" => 2,
        "ETGB" => 3,
        _ => 1
    };

    public static class Ids
    {
        public const int Export = 1;
        public const int Import = 2;
        public const int ETGB = 3;
    }
}

// ============================================================
// CUSTOMS DECLARATION STATUSES (Beyanname Durumlari)
// ============================================================
public static class CustomsDeclarationStatuses
{
    public static readonly TypeItem Draft = new(1, "Draft", "CustomsDeclarationStatus.Draft", "Taslak", "bi-file-earmark", "bg-secondary", 1, isDefault: true);
    public static readonly TypeItem Pending = new(2, "Pending", "CustomsDeclarationStatus.Pending", "Beklemede", "bi-hourglass-split", "bg-warning text-dark", 2);
    public static readonly TypeItem Registered = new(3, "Registered", "CustomsDeclarationStatus.Registered", "Tescil Edildi", "bi-check-circle", "bg-primary", 3);
    public static readonly TypeItem InProgress = new(4, "InProgress", "CustomsDeclarationStatus.InProgress", "Islemde", "bi-gear", "bg-info", 4);
    public static readonly TypeItem Completed = new(5, "Completed", "CustomsDeclarationStatus.Completed", "Kapandi", "bi-check-all", "bg-success", 5);
    public static readonly TypeItem Rejected = new(6, "Rejected", "CustomsDeclarationStatus.Rejected", "Reddedildi", "bi-x-circle", "bg-danger", 6);
    public static readonly TypeItem Error = new(7, "Error", "CustomsDeclarationStatus.Error", "Hata", "bi-exclamation-triangle", "bg-danger", 7);

    public static IEnumerable<TypeItem> All => new[] { Draft, Pending, Registered, InProgress, Completed, Rejected, Error };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);

    /// <summary>
    /// Evrim durum kodundan StatusId'ye donusum
    /// </summary>
    public static int FromEvrimStatus(string? evrimDurum) => evrimDurum?.ToUpperInvariant() switch
    {
        "TASLAK" => Ids.Draft,
        "BEKLEMEDE" => Ids.Pending,
        "TESCILLI" => Ids.Registered,
        "ISLEMDE" => Ids.InProgress,
        "KAPALI" => Ids.Completed,
        "REDDEDILDI" => Ids.Rejected,
        "HATA" => Ids.Error,
        _ => Ids.Draft
    };

    /// <summary>
    /// Aktif durumlar (senkronizasyon yapilacak)
    /// </summary>
    public static IEnumerable<TypeItem> ActiveStatuses => new[] { Pending, Registered, InProgress };

    public static class Ids
    {
        public const int Draft = 1;
        public const int Pending = 2;
        public const int Registered = 3;
        public const int InProgress = 4;
        public const int Completed = 5;
        public const int Rejected = 6;
        public const int Error = 7;
    }
}

/// <summary>
/// Urun degerlendirme durumlari
/// </summary>
public static class ProductReviewStatuses
{
    public static readonly TypeItem Published = new(1, "Published", "ProductReviewStatus.Published", "Yayinda", "bi-check-circle", "bg-success", 1, isDefault: true);
    public static readonly TypeItem Pending = new(0, "Pending", "ProductReviewStatus.Pending", "Beklemede", "bi-hourglass-split", "bg-warning text-dark", 2);
    public static readonly TypeItem Rejected = new(2, "Rejected", "ProductReviewStatus.Rejected", "Reddedildi", "bi-x-circle", "bg-danger", 3);
    public static readonly TypeItem Hidden = new(3, "Hidden", "ProductReviewStatus.Hidden", "Gizlendi", "bi-eye-slash", "bg-secondary", 4);

    public static IEnumerable<TypeItem> All => new[] { Pending, Published, Rejected, Hidden };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);

    public static class Ids
    {
        public const int Pending = 0;
        public const int Published = 1;
        public const int Rejected = 2;
        public const int Hidden = 3;
    }
}

// ============================================================
// SOCIAL POST TYPES (Sosyal Paylasim Tipleri)
// ============================================================
public static class SocialPostTypes
{
    public static readonly TypeItem Text = new(1, "Text", "SocialPostType.Text", "Metin paylasimi", "bi-chat-text", "bg-primary", 1, isDefault: true);
    public static readonly TypeItem Article = new(2, "Article", "SocialPostType.Article", "Makale", "bi-newspaper", "bg-info", 2);
    public static readonly TypeItem Announcement = new(3, "Announcement", "SocialPostType.Announcement", "Duyuru", "bi-megaphone", "bg-warning text-dark", 3);
    public static readonly TypeItem ProductShowcase = new(4, "ProductShowcase", "SocialPostType.ProductShowcase", "Urun Tanitimi", "bi-box-seam", "bg-success", 4);

    public static IEnumerable<TypeItem> All => new[] { Text, Article, Announcement, ProductShowcase };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);

    public static class Ids
    {
        public const int Text = 1;
        public const int Article = 2;
        public const int Announcement = 3;
        public const int ProductShowcase = 4;
    }
}

// ============================================================
// SOCIAL POST STATUSES (Sosyal Paylasim Durumlari)
// ============================================================
public static class SocialPostStatuses
{
    public static readonly TypeItem Draft = new(1, "Draft", "SocialPostStatus.Draft", "Taslak", "bi-file-earmark", "bg-secondary", 1, isDefault: true);
    public static readonly TypeItem Published = new(2, "Published", "SocialPostStatus.Published", "Yayinda", "bi-check-circle", "bg-success", 2);
    public static readonly TypeItem Archived = new(3, "Archived", "SocialPostStatus.Archived", "Arsivlendi", "bi-archive", "bg-dark", 3);

    public static IEnumerable<TypeItem> All => new[] { Draft, Published, Archived };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);

    public static class Ids
    {
        public const int Draft = 1;
        public const int Published = 2;
        public const int Archived = 3;
    }
}

// ============================================================
// AUCTION STATUSES
// ============================================================
public static class AuctionStatuses
{
    public static readonly TypeItem Draft = new(0, "Draft", "AuctionStatus.Draft", "Taslak", "bi-file-earmark", "bg-secondary", 0, isDefault: true);
    public static readonly TypeItem Scheduled = new(1, "Scheduled", "AuctionStatus.Scheduled", "Planlanmis", "bi-calendar-event", "bg-info", 1);
    public static readonly TypeItem Active = new(2, "Active", "AuctionStatus.Active", "Aktif", "bi-hammer", "bg-success", 2);
    public static readonly TypeItem Ended = new(3, "Ended", "AuctionStatus.Ended", "Bitmis", "bi-clock-history", "bg-dark", 3);
    public static readonly TypeItem Cancelled = new(4, "Cancelled", "AuctionStatus.Cancelled", "Iptal", "bi-x-circle", "bg-danger", 4);
    public static readonly TypeItem Sold = new(5, "Sold", "AuctionStatus.Sold", "Satildi", "bi-check-circle", "bg-primary", 5);

    public static IEnumerable<TypeItem> All => new[] { Draft, Scheduled, Active, Ended, Cancelled, Sold };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);

    public static class Ids
    {
        public const int Draft = 0;
        public const int Scheduled = 1;
        public const int Active = 2;
        public const int Ended = 3;
        public const int Cancelled = 4;
        public const int Sold = 5;
    }
}

// ============================================================
// BID STATUSES
// ============================================================
public static class BidStatuses
{
    public static readonly TypeItem Valid = new(1, "Valid", "BidStatus.Valid", "Gecerli", "bi-check", "bg-success", 1, isDefault: true);
    public static readonly TypeItem Outbid = new(2, "Outbid", "BidStatus.Outbid", "Gecildi", "bi-arrow-down", "bg-warning text-dark", 2);
    public static readonly TypeItem Rejected = new(3, "Rejected", "BidStatus.Rejected", "Reddedildi", "bi-x-circle", "bg-danger", 3);
    public static readonly TypeItem Cancelled = new(4, "Cancelled", "BidStatus.Cancelled", "Iptal", "bi-slash-circle", "bg-secondary", 4);
    public static readonly TypeItem Winning = new(5, "Winning", "BidStatus.Winning", "Kazanan", "bi-trophy", "bg-primary", 5);

    public static IEnumerable<TypeItem> All => new[] { Valid, Outbid, Rejected, Cancelled, Winning };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);

    public static class Ids
    {
        public const int Valid = 1;
        public const int Outbid = 2;
        public const int Rejected = 3;
        public const int Cancelled = 4;
        public const int Winning = 5;
    }
}

// ============================================================
// REWARD POINT ACTION TYPES
// ============================================================
public static class RewardPointActionTypes
{
    public static readonly TypeItem Earned = new(1, "Earned", "RewardPointAction.Earned", "Kazanildi", "bi-plus-circle", "bg-success", 1, isDefault: true);
    public static readonly TypeItem Spent = new(2, "Spent", "RewardPointAction.Spent", "Harcandi", "bi-dash-circle", "bg-danger", 2);
    public static readonly TypeItem OrderBonus = new(3, "OrderBonus", "RewardPointAction.OrderBonus", "Siparis bonusu", "bi-cart-check", "bg-info", 3);
    public static readonly TypeItem Referral = new(4, "Referral", "RewardPointAction.Referral", "Referans bonusu", "bi-people", "bg-primary", 4);
    public static readonly TypeItem Expired = new(5, "Expired", "RewardPointAction.Expired", "Suresi doldu", "bi-clock-history", "bg-secondary", 5);
    public static readonly TypeItem AdminAdjustment = new(6, "AdminAdjustment", "RewardPointAction.AdminAdjustment", "Admin duzeltmesi", "bi-shield-check", "bg-warning text-dark", 6);

    public static IEnumerable<TypeItem> All => new[] { Earned, Spent, OrderBonus, Referral, Expired, AdminAdjustment };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);

    public static class Ids
    {
        public const int Earned = 1;
        public const int Spent = 2;
        public const int OrderBonus = 3;
        public const int Referral = 4;
        public const int Expired = 5;
        public const int AdminAdjustment = 6;
    }
}
