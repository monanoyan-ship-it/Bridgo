# Bridgo B2B Development Patterns

Bu dosya projedeki standart pattern'leri tanımlar. **YENİ BİR ÖZELLİK EKLENİRKEN BU DOSYA MUTLAKA OKUNMALIDIR.**

---

## 0. Temel Kurallar

### ID Kullanimi
- Tum entity'lerde `int` ID kullanilir (auto-increment)
- GUID KULLANILMAZ (iletisim kolayligi icin int tercih edilir)

```csharp
// DOGRU
public class Branch : BaseEntity
{
    // Id int olarak BaseEntity'den gelir
}

// YANLIS - GUID kullanma
public Guid Id { get; set; }
```

### BaseEntity
Tum entity'ler BaseEntity'den turetilir:
```csharp
public abstract class BaseEntity
{
    public int Id { get; set; }  // Auto-increment
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }  // Soft delete
}
```

---

## 1. View/JavaScript Yapısı (SPA Modal Pattern)

### DOĞRU Pattern (Branches, Users, Checklists gibi)
```
Views/
  ModuleName/
    Index.cshtml          # TEK DOSYA - Liste + Create Modal + Edit Modal + Detail Modal
wwwroot/js/
  ModuleName/
    Index.js              # TEK DOSYA - Tüm ViewModel mantığı
```

### YANLIŞ Pattern (KULLANILMAMALI)
```
Views/
  ModuleName/
    Index.cshtml          # YANLIŞ - Ayrı sayfalara bölünmüş
    Create.cshtml         # YANLIŞ
    Edit.cshtml           # YANLIŞ
    Detail.cshtml         # YANLIŞ
wwwroot/js/
  ModuleName/
    index.js              # YANLIŞ - Ayrı JS dosyalarına bölünmüş
    create.js             # YANLIŞ
    edit.js               # YANLIŞ
    detail.js             # YANLIŞ
```

---

## 2. KnockoutJS Binding Pattern

### DOĞRU - Spesifik element'e bind et
```javascript
$(document).ready(function() {
    ko.applyBindings(new ModuleViewModel(), document.getElementById('module-app'));
});
```

### YANLIŞ - Tüm document'a bind etme
```javascript
ko.applyBindings(new ModuleViewModel());  // YANLIŞ!
```

### Observable Kullanımı (KRİTİK!)

KnockoutJS'de tüm değişken değerler `ko.observable()` veya `ko.observableArray()` olmalı!

#### YANLIŞ - Düz değişken
```javascript
self.name = '';                    // YANLIŞ - binding çalışmaz!
self.items = [];                   // YANLIŞ - liste güncellenince UI güncellenmez!
self.isLoading = false;            // YANLIŞ!
```

#### DOĞRU - Observable
```javascript
self.name = ko.observable('');              // DOĞRU
self.items = ko.observableArray([]);        // DOĞRU
self.isLoading = ko.observable(false);      // DOĞRU
```

#### Form Nesneleri
```javascript
// DOĞRU - Form alanları observable
self.memberForm = {
    name: ko.observable(''),
    email: ko.observable(''),
    phone: ko.observable(''),
    role: ko.observable('4'),
    isActive: ko.observable(true)
};
```

#### Değer Okuma/Yazma
```javascript
// Okuma - parantezle çağır
var currentName = self.name();

// Yazma - parantez içine değer ver
self.name('Yeni değer');

// Array işlemleri
self.items.push(newItem);
self.items.remove(item);
self.items([]);  // Temizle
```

---

## 3. View Yapısı Template

### Index.cshtml Şablonu:
```html
@{
    ViewData["Title"] = "Modül Başlığı";
}

<div id="module-app" class="container-fluid">
    <!-- Header with Create Button -->
    <div class="d-flex justify-content-between align-items-center mb-4">
        <h2>Modül Başlığı</h2>
        <button class="btn btn-primary" data-bind="click: createNew">
            <i class="bi bi-plus-circle"></i> Yeni Ekle
        </button>
    </div>

    <!-- Loading -->
    <div data-bind="visible: isLoading" class="text-center py-5">
        <div class="spinner-border text-primary"></div>
    </div>

    <!-- Error/Success Messages -->
    <div data-bind="visible: errorMessage" class="alert alert-danger alert-dismissible fade show">
        <span data-bind="text: errorMessage"></span>
        <button type="button" class="btn-close" data-bind="click: function() { errorMessage(''); }"></button>
    </div>

    <!-- Table/List -->
    <div data-bind="visible: !isLoading()">
        <!-- Empty State -->
        <div data-bind="visible: items().length === 0">
            <div class="alert alert-info">
                <i class="bi bi-info-circle"></i> Henüz kayıt bulunmamaktadır.
            </div>
        </div>

        <!-- Data Table -->
        <div data-bind="visible: items().length > 0" class="card shadow">
            <div class="card-body">
                <table class="table table-hover">
                    <!-- ... -->
                </table>
            </div>
        </div>
    </div>

    <!-- Create/Edit Modal (TEK MODAL) -->
    <div class="modal fade" data-bind="css: { show: isModalOpen }, style: { display: isModalOpen() ? 'block' : 'none' }" tabindex="-1">
        <div class="modal-dialog modal-lg">
            <div class="modal-content" data-bind="with: editingItem">
                <div class="modal-header">
                    <h5 class="modal-title">
                        <span data-bind="visible: !id">Yeni Kayıt</span>
                        <span data-bind="visible: id">Kayıt Düzenle</span>
                    </h5>
                    <button type="button" class="btn-close" data-bind="click: $parent.closeModal"></button>
                </div>
                <div class="modal-body">
                    <!-- Form fields -->
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-bind="click: $parent.closeModal">İptal</button>
                    <button type="button" class="btn btn-primary" data-bind="click: $parent.save, disable: $parent.isSaving">
                        Kaydet
                    </button>
                </div>
            </div>
        </div>
    </div>
    <div class="modal-backdrop fade" data-bind="css: { show: isModalOpen }, style: { display: isModalOpen() ? 'block' : 'none' }"></div>

    <!-- Delete Confirmation (Shared Partial) -->
    <partial name="_DeleteConfirmationModal" />

</div>

@section Scripts {
    <script src="~/js/Shared/delete-confirmation.js"></script>
    <script src="~/js/ModuleName/Index.js"></script>
}
```

---

## 4. JavaScript ViewModel Şablonu

### Index.js Şablonu:
```javascript
function ModuleViewModel() {
    var self = this;

    // State
    self.isLoading = ko.observable(false);
    self.isSaving = ko.observable(false);
    self.errorMessage = ko.observable('');
    self.successMessage = ko.observable('');

    // Modal state
    self.isModalOpen = ko.observable(false);
    self.editingItem = ko.observable(null);

    // Data
    self.items = ko.observableArray([]);

    // CRUD operations
    self.loadItems = function() {
        self.isLoading(true);
        fetch('/api/module')
            .then(function(r) { return r.json(); })
            .then(function(data) {
                self.items(data);
            })
            .finally(function() {
                self.isLoading(false);
            });
    };

    self.createNew = function() {
        self.editingItem({
            id: null,
            name: ko.observable(''),
            // ... other fields
        });
        self.isModalOpen(true);
    };

    self.editItem = function(item) {
        self.editingItem({
            id: item.id,
            name: ko.observable(item.name),
            // ... other fields
        });
        self.isModalOpen(true);
    };

    self.closeModal = function() {
        self.isModalOpen(false);
        self.editingItem(null);
    };

    self.save = function() {
        var item = self.editingItem();
        var isNew = !item.id;
        var url = isNew ? '/api/module' : '/api/module/' + item.id;
        var method = isNew ? 'POST' : 'PUT';

        self.isSaving(true);
        fetch(url, {
            method: method,
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                name: item.name(),
                // ... other fields
            })
        })
        .then(function(r) {
            if (r.ok) {
                self.closeModal();
                self.loadItems();
                self.successMessage(isNew ? 'Kayıt eklendi.' : 'Kayıt güncellendi.');
            } else {
                return r.json().then(function(err) {
                    self.errorMessage(err.message || 'Bir hata oluştu.');
                });
            }
        })
        .finally(function() {
            self.isSaving(false);
        });
    };

    self.deleteItem = function(item) {
        if (confirm('Silmek istediğinizden emin misiniz?')) {
            fetch('/api/module/' + item.id, { method: 'DELETE' })
                .then(function(r) {
                    if (r.ok) {
                        self.loadItems();
                        self.successMessage('Kayıt silindi.');
                    }
                });
        }
    };

    // Initialize
    self.loadItems();
}

// DOĞRU BINDING - Spesifik element'e
$(document).ready(function() {
    ko.applyBindings(new ModuleViewModel(), document.getElementById('module-app'));
});
```

---

## 5. Repository + Service Pattern

### CONTROLLER'DA SQL/DBCONTEXT KULLANILMAZ!

Controller -> Service -> Repository -> DbContext

### Repository Interface
```csharp
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T> AddAsync(T entity);
    void Update(T entity);
    Task SoftDeleteAsync(int id);
    IQueryable<T> Query();
}
```

### Unit of Work
```csharp
public interface IUnitOfWork : IDisposable
{
    IRepository<Branch> Branches { get; }
    IRepository<Vendor> Vendors { get; }
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitAsync();
}
```

### Service Layer
```csharp
// Interface
public interface IBranchService
{
    Task<IEnumerable<BranchDto>> GetAllAsync(bool? isActive = null);
    Task<BranchDto?> GetByIdAsync(int id);
    Task<BranchDto> CreateAsync(CreateBranchDto dto, string? createdBy);
    Task<bool> UpdateAsync(int id, UpdateBranchDto dto, string? updatedBy);
    Task<bool> DeleteAsync(int id, string? deletedBy);
}

// Implementation
public class BranchService : IBranchService
{
    private readonly IUnitOfWork _unitOfWork;

    public BranchService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    // ... metodlar
}
```

### DI Registration (Program.cs)
```csharp
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IBranchService, BranchService>();
```

---

## 6. API Controller Pattern

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ModuleApiController : ControllerBase
{
    private readonly IModuleService _service;

    public ModuleApiController(IModuleService service)
    {
        _service = service;
    }

    // GET /api/module - Liste
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? isActive)
    {
        var items = await _service.GetAllAsync(isActive);
        return Ok(items);
    }

    // GET /api/module/{id} - Tekil
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)

    // POST /api/module - Create
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDto dto)

    // PUT /api/module/{id} - Update
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDto dto)

    // DELETE /api/module/{id} - Delete
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
}
```

---

## 6. MVC Controller Pattern (View Controller)

### Dashboard Modül Controller'ları

Tüm dashboard modülleri `DashboardBaseController`'dan inherit eder:

```csharp
// Controllers/DashboardBaseController.cs
[Authorize]
public abstract class DashboardBaseController : Controller
{
    protected readonly IVendorService _vendorService;
    protected readonly ICompanyService _companyService;
    protected readonly UserManager<ApplicationUser> _userManager;

    protected DashboardBaseController(
        IVendorService vendorService,
        ICompanyService companyService,
        UserManager<ApplicationUser> userManager)
    {
        _vendorService = vendorService;
        _companyService = companyService;
        _userManager = userManager;
    }

    protected async Task<ApplicationUser?> GetCurrentUserAsync() { ... }
    protected async Task<bool> LoadViewDataAsync(ApplicationUser user, int? activeCapabilityId = null) { ... }
    protected async Task<IActionResult> ExecuteWithViewDataAsync(string? viewName = null) { ... }
}
```

### Modül Controller Örneği

```csharp
// Controllers/Modules/OrdersController.cs
namespace Bridgo.Controllers.Modules;

public class OrdersController : DashboardBaseController
{
    public OrdersController(
        IVendorService vendorService,
        ICompanyService companyService,
        UserManager<ApplicationUser> userManager)
        : base(vendorService, companyService, userManager)
    {
    }

    public Task<IActionResult> MyOrders() => ExecuteWithViewDataAsync();
    public Task<IActionResult> SellerOrders() => ExecuteWithViewDataAsync();
}
```

### View Klasör Yapısı

```
Views/
  Orders/
    MyOrders.cshtml           # /Orders/MyOrders route'u
    SellerOrders.cshtml       # /Orders/SellerOrders route'u
  Settings/
    PersonalInfo.cshtml       # /Settings/PersonalInfo
    Documents.cshtml          # /Settings/Documents
    Billing.cshtml            # /Settings/Billing
  ...
```

### Controller/Modules Klasörü

```
Controllers/
  Modules/
    OrdersController.cs       # Sipariş modülü
    DemandsController.cs      # Talep modülü
    SuppliersController.cs    # Tedarikçi modülü
    ProposalsController.cs    # Teklif modülü
    MessagesController.cs     # Mesajlaşma modülü
    ReportsController.cs      # Raporlar modülü
    ServicesController.cs     # Hizmet talepleri
    InvestmentController.cs   # Yatırım modülü
    StockController.cs        # Stok modülü
    SettingsController.cs     # Ayarlar modülü
```

### ExecuteWithViewDataAsync Kullanımı

```csharp
// Basit action - view adı action adıyla aynı
public Task<IActionResult> MyOrders() => ExecuteWithViewDataAsync();

// Farklı view adı
public Task<IActionResult> Index() => ExecuteWithViewDataAsync("Proposals");

// Özel kontrol gerektiren action
public async Task<IActionResult> Settings()
{
    var user = await GetCurrentUserAsync();
    if (user?.VendorId == null)
        return RedirectToAction("Index", "VendorSetup");

    // Sadece Owner ve Admin erişebilir
    if (user.VendorRole != VendorUserRole.Owner && user.VendorRole != VendorUserRole.Admin)
        return RedirectToAction("Index", "Dashboard");

    if (!await LoadViewDataAsync(user))
        return RedirectToAction("Index", "VendorSetup");

    return View();
}
```

### Route Kuralları

| Controller | Action | Route | View |
|------------|--------|-------|------|
| OrdersController | MyOrders | /Orders/MyOrders | Views/Orders/MyOrders.cshtml |
| SettingsController | PersonalInfo | /Settings/PersonalInfo | Views/Settings/PersonalInfo.cshtml |
| ProposalsController | Index | /Proposals | Views/Proposals/Proposals.cshtml |
| ProposalsController | Compare | /Proposals/Compare | Views/Proposals/CompareProposals.cshtml |

**NOT:** Create, Edit, Detail gibi ayrı action'lar OLMAMALI. Her şey Index içinde modal ile yapılmalı.

---

## 7. Rapor Sayfaları İçin Pattern

Rapor sayfaları Index.cshtml pattern'i izler ama modal yerine filtre ve tablo içerir:

```html
<div id="report-app">
    <!-- Filters -->
    <div class="card mb-4">
        <div class="card-header">Filtreler</div>
        <div class="card-body">
            <!-- Filter controls -->
        </div>
    </div>

    <!-- Summary Cards (optional) -->
    <!-- Data Table -->
    <!-- Pagination (if needed) -->
</div>
```

---

## 8. Düzeltilmiş Modüller

Bu modüller doğru SPA Modal Pattern'e dönüştürüldü:

- [x] Calls (tek Index.cshtml + Index.js)
- [x] Trainings (tek Index.cshtml + Index.js)
- [x] Meetings (tek Index.cshtml + Index.js)
- [x] Approvals (tek Index.cshtml + Index.js)
- [x] Evaluations (tek Index.cshtml + Index.js, modal-fullscreen kullanıyor)
- [ ] Notifications (Settings ayrı kalabilir)

---

## 9. Doğru Yapılmış Modüller (Referans)

Bu modüller doğru pattern kullanıyor:

- Branches - Tek Index.cshtml + modal
- Users - Tek Index.cshtml + modal
- Checklists - Tek Index.cshtml + modal
- Customers - Tek Index.cshtml + modal
- FieldWorkers - Tek Index.cshtml + modal
- Personnel - Tek Index.cshtml + modal
- VisitDetails - Tek Index.cshtml + tab yapısı + modal (Sektör ve Alan Tanımları)

---

## 10. Visit Details - Dinamik Alan Sistemi

Ziyaret detayları için dinamik alan sistemi (EAV - Entity-Attribute-Value pattern).

### Tablo Yapısı

```
VisitSectors (Sektör Tanımları)
├── Id, Code, Name, Description, IconClass, SortOrder, IsActive
│
VisitFieldDefinitions (Alan Tanımları)
├── Id, SectorId (nullable = ortak alan), Code, Name
├── FieldType (Int, Decimal, Bool, String, DateTime, Rating)
├── Category (Time, Staff, Facility, General, Sector)
├── IsRequired, MaxRating, MaxLength, MinValue, MaxValue
├── Placeholder, HelpText, SortOrder, IsActive
│
VisitDetailValues (Değerler - EAV)
├── Id, CustomerVisitId, FieldDefinitionId
├── IntValue, DecimalValue, BoolValue, StringValue, DateTimeValue
```

### Kullanım

**Sektör Oluşturma:**
```
POST /api/visit-details/sectors
{ "code": "BANK", "name": "Banka", "iconClass": "bi-bank", "isActive": true }
```

**Alan Tanımı Oluşturma:**
```
POST /api/visit-details/fields
{
  "sectorId": null,  // null = tüm sektörlerde geçerli ortak alan
  "code": "wait_time",
  "name": "Bekleme Süresi (dk)",
  "fieldType": 0,    // Int
  "category": 0,     // Time
  "isRequired": true
}
```

**Ziyaret Detayı Kaydetme:**
```
POST /api/visit-details/values
{
  "customerVisitId": "...",
  "values": [
    { "fieldDefinitionId": "...", "value": 15 },
    { "fieldDefinitionId": "...", "value": true }
  ]
}
```

### API Endpoints

| Endpoint | Açıklama |
|----------|----------|
| `GET /api/visit-details/sectors` | Tüm sektörler |
| `GET /api/visit-details/fields` | Tüm alan tanımları |
| `GET /api/visit-details/fields/sector/{id}` | Sektöre özel alanlar |
| `GET /api/visit-details/fields/for-visit?sectorId=` | Ziyaret için geçerli alanlar (ortak + sektöre özel) |
| `GET /api/visit-details/values/{customerVisitId}` | Ziyaret detayları |
| `POST /api/visit-details/values` | Toplu değer kaydetme |
| `GET /api/visit-details/statistics/{fieldId}` | Alan istatistikleri |

### Yönetim UI

`/VisitDetails/Index` - Admin only
- **Sektörler Tab:** Sektör CRUD
- **Alan Tanımları Tab:** Alan tanımı CRUD, sektöre göre filtreleme

---

## 11. Kütüphane Kullanımı (Offline Uyumluluk)

### KESİNLİKLE CDN KULLANILMAZ!

Uygulama offline çalışmalıdır. Tüm kütüphaneler `wwwroot/lib/` altında yerel olarak bulunmalıdır.

### YANLIŞ - CDN Link Kullanımı
```html
<link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet">
<script src="https://cdnjs.cloudflare.com/ajax/libs/toastr.js/latest/toastr.min.js"></script>
```

### DOĞRU - Yerel Dosya Kullanımı
```html
<link href="~/lib/bootstrap/bootstrap.min.css" rel="stylesheet">
<script src="~/lib/toastr/toastr.min.js"></script>
```

### Mevcut Yerel Kütüphaneler
```
wwwroot/lib/
  bootstrap/           # CSS ve JS
  bootstrap-icons/     # CSS ve font dosyaları
  chartjs/             # Chart.js
  jquery/              # jQuery
  knockout/            # KnockoutJS
  toastr/              # Toastr notifications
```

### Yeni Kütüphane Ekleme
1. Kütüphane dosyalarını `wwwroot/lib/{library-name}/` altına indir
2. `_Layout.cshtml`'de yerel path kullan
3. CDN referansı KULLANMA

---

## 12. Onay Modalları (Confirmation)

### Native confirm() KULLANILMAZ!

Browser'ın native `confirm()` popup'ı yerine Bootstrap modal kullanılmalıdır.

### YANLIŞ
```javascript
if (confirm('Silmek istediğinize emin misiniz?')) {
    // işlem
}
```

### DOĞRU - Shared Modal Kullanımı
```javascript
// Silme onayı için
showDeleteConfirm('Kayıt adı', function() {
    // silme işlemi
});

// Genel onay için
showConfirmModal({
    title: 'Onay Başlığı',
    message: 'Onay mesajı',
    type: 'warning',  // warning, danger, info, success
    confirmText: 'Onayla',
    confirmIcon: 'bi-check',
    onConfirm: function() {
        // onaylanan işlem
    }
});
```

### Shared Modal Dosyaları
- **Modal HTML:** `_Layout.cshtml` içinde `#sharedConfirmModal`
- **JS Helper:** `wwwroot/js/shared/confirm-modal.js`

---

## 13. Cok Dilli Destek (Localization)

### T() Helper Metodu
Tum text'ler T() ile cekilir. nopCommerce benzeri yapı.

### View'da Kullanim
```html
@using Bridgo.Extensions

<h2>@Html.T("Branch.Title")</h2>
<button>@Html.T("Common.Save")</button>
```

### Controller'da Kullanim
```csharp
public class BranchesController : BaseController
{
    public IActionResult Index()
    {
        ViewBag.Message = T("Branch.Welcome");
        return View();
    }
}
```

### API Mesajlari
API'den donen mesajlar resource key olarak doner, frontend cevirir:
```csharp
return BadRequest(new { message = "Branch.Code.AlreadyExists" });
```

### Resource Key Formati
```
Common.Save          -> Kaydet
Common.Cancel        -> Iptal
Common.Delete        -> Sil
Branch.Title         -> Subeler
Branch.Code.Required -> Sube kodu zorunludur
Vendor.CompanyName   -> Sirket Adi
```

### Dil Tabloları
- `Languages` - Dil tanimlari (tr-TR, en-US)
- `LocaleStringResources` - Key-Value ciftleri

---

## 14. Vendor Sistemi

### Vendor-User Iliskisi
- Vendor = Cati sirket
- User'lar Vendor altinda calisir
- Her User'in bir VendorId'si var
- VendorUserRole: Owner, Admin, Manager, Employee

### VendorStatus
```csharp
public enum VendorStatus
{
    PendingProfile = 0,      // Profil tamamlanmayi bekliyor
    PendingVerification = 1, // Admin onay bekliyor
    Active = 2,              // Aktif
    Suspended = 3,           // Askiya alindi
    Rejected = 4             // Reddedildi
}
```

### VendorId Ownership Pattern (KRITIK!)

**Is verileri VendorId'ye aittir, UserId sadece audit icindir.**

```csharp
// DOGRU - Sepet VendorId'ye ait
public class ShoppingCart : BaseEntity
{
    public int VendorId { get; set; }      // SAHIPLIK - Sepet hangi firmanin
    public Vendor Vendor { get; set; }

    // CreatedBy zaten BaseEntity'de var (kim olusturdu - audit)
}

// DOGRU - Siparis VendorId'ye ait
public class Order : BaseEntity
{
    public int VendorId { get; set; }      // SAHIPLIK - Siparis hangi firmanin
    // NOT: CreatedBy (UserId) kim siparisi verdi bilgisi BaseEntity'de
}

// YANLIS - UserId ile sahiplik belirleme
public class ShoppingCart
{
    public int UserId { get; set; }  // YANLIS! User degisebilir, firma degismez
}
```

### Neden VendorId?
- Bir firmanin birden fazla kullanicisi olabilir
- Tum kullanicilar ayni firmanin sepetini/siparislerini gorebilir
- Kullanici ayrilsa bile firmaya ait veriler kalir
- UserId sadece "kim yapti" bilgisi icin (audit trail)

### Middleware: VendorRequired
```csharp
app.UseVendorRequired();
```
- Admin haric tum kullanicilar VendorId olmadan sisteme giremez
- VendorId yoksa `/VendorSetup` sayfasina yonlendirilir
- API isteklerinde 403 + JSON response doner

---

## 15. VendorSetup Akisi

### Akis Senaryolari

**Senaryo 1: Kurumsal E-posta (Domain Match)**
```
1. User register olur (ornek: ali@abcsirket.com)
2. Login yapar -> Middleware VendorSetup'a yonlendirir
3. Sistem e-posta domain'ini kontrol eder (abcsirket.com)
4. Eger www.abcsirket.com website'li vendor varsa -> "Firmaniz bulundu!" gosterir
5. User "Katil" tiklar -> JoinRequest olusturulur
6. Vendor Owner onaylar -> User firmaya eklenir
```

**Senaryo 2: Kisisel E-posta (gmail vb.)**
```
1. User register olur (ornek: ali@gmail.com)
2. Login yapar -> VendorSetup'a yonlendirilir
3. Domain eslesmesi olmaz (gmail kisisel)
4. User ya mevcut firmayi arar ya yeni firma olusturur
```

**Senaryo 3: Yeni Firma Olusturma**
```
1. User VendorSetup'ta "Yeni Firma Olustur" secer
2. Firma bilgilerini + ilk adresi girer
3. Kaydeder -> User otomatik Owner rolunu alir
```

### Domain Matching
```csharp
// Kisisel e-posta domain'leri (esleme yapilmaz)
private static readonly HashSet<string> PersonalEmailDomains = new[]
{
    "gmail.com", "hotmail.com", "outlook.com", "yahoo.com",
    "icloud.com", "yandex.com", "mail.ru", ...
};

// Kurumsal domain -> Vendor.Website ile eslestirilir
```

### JoinRequest Sistemi
```csharp
public class VendorJoinRequest
{
    public int UserId { get; set; }
    public int VendorId { get; set; }
    public JoinRequestSource Source { get; set; }  // Manual, EmailDomainMatch, InviteLink
    public JoinRequestStatus Status { get; set; }  // Pending, Approved, Rejected, Cancelled
    public VendorUserRole RequestedRole { get; set; }
}
```

Vendor Owner/Admin:
- `/JoinRequests` sayfasindan bekleyen istekleri gorur
- Onaylar veya reddeder
- Onaylarken kullaniciya rol atar (Employee, Manager, vb.)

---

## 16. Tip Tanimlari Pattern (Enum Yerine - Kod Bazli)

### NEDEN ENUM KULLANILMAZ?

Kullanicinin gorebilecegi degerler (status, type vb.) enum yerine **kod icinde static class** olarak tanimlanir:
- **Cok dilli destek:** NameResourceKey ile localization
- **Type-safe erisim:** `AddressTypes.Billing.Id` gibi
- **Helper metodlar:** `GetById()`, `GetBySystemName()`, `All`, `Default`
- **Veritabani tablosu GEREKMEZ!**

### TypeItem Base Class

```csharp
public class TypeItem
{
    public int Id { get; }
    public string SystemName { get; }
    public string NameResourceKey { get; }
    public string? Description { get; }
    public string? Icon { get; }
    public int DisplayOrder { get; }
    public bool IsDefault { get; }
    public bool IsActive { get; }
    public bool IsSystem { get; }

    public TypeItem(int id, string systemName, string nameResourceKey,
        string? description = null, string? icon = null, int displayOrder = 0,
        bool isDefault = false, bool isActive = true, bool isSystem = true)
    {
        // ... constructor
    }
}
```

### Tip Tanimlama Ornegi

```csharp
public static class AddressTypes
{
    public static readonly TypeItem Billing = new(1, "Billing", "AddressType.Billing", "Fatura adresi", "bi-receipt", 1);
    public static readonly TypeItem Shipping = new(2, "Shipping", "AddressType.Shipping", "Teslimat adresi", "bi-truck", 2);
    public static readonly TypeItem Headquarters = new(3, "Headquarters", "AddressType.Headquarters", "Merkez ofis", "bi-building", 3, isDefault: true);
    public static readonly TypeItem Warehouse = new(4, "Warehouse", "AddressType.Warehouse", "Depo adresi", "bi-box-seam", 4);
    public static readonly TypeItem Branch = new(5, "Branch", "AddressType.Branch", "Sube adresi", "bi-shop", 5);
    public static readonly TypeItem Return = new(6, "Return", "AddressType.Return", "Iade adresi", "bi-arrow-return-left", 6);

    // Helper metodlar
    public static IEnumerable<TypeItem> All => new[] { Billing, Shipping, Headquarters, Warehouse, Branch, Return };
    public static TypeItem Default => All.First(x => x.IsDefault);
    public static TypeItem? GetById(int id) => All.FirstOrDefault(x => x.Id == id);
    public static TypeItem? GetBySystemName(string systemName) => All.FirstOrDefault(x => x.SystemName == systemName);
}
```

### Kullanim

```csharp
// Entity'de int ID ile sakla
public class Address : BaseEntity
{
    public int AddressTypeId { get; set; }  // FK degil, sadece int
}

// Service'de kullan
var address = new Address { AddressTypeId = AddressTypes.Billing.Id };

// Kontrol
if (address.AddressTypeId == AddressTypes.Headquarters.Id) { ... }

// Tip bilgisi al
var typeInfo = AddressTypes.GetById(address.AddressTypeId);
var localizedName = T(typeInfo.NameResourceKey);

// Dropdown icin liste
var types = AddressTypes.All.Where(t => t.IsActive);
```

### Frontend'de Kullanim

```javascript
// API'den tip listesi al
fetch('/api/types/address-types')
    .then(r => r.json())
    .then(data => self.addressTypes(data));
```

```html
<select data-bind="options: addressTypes,
                   optionsText: function(item) { return T(item.nameResourceKey); },
                   optionsValue: 'id',
                   value: selectedAddressTypeId">
</select>
```

### Mevcut Tip Tanimlari (Models/Enums/TypeDefinitions.cs)

| Static Class | Aciklama |
|--------------|----------|
| `AddressTypes` | Billing, Shipping, Headquarters, Warehouse, Branch, Return |
| `WarehouseTypes` | Main, Distribution, Returns, Temporary, Virtual, Consignment |
| `VendorStatuses` | PendingProfile, PendingVerification, Active, Suspended, Rejected |
| `ProductStatuses` | Draft, Active, Inactive, Discontinued |
| `TeamMemberStatuses` | Pending, Active, Rejected, Cancelled, Expired, Left |

### AVANTAJLAR

- ✅ Veritabani tablosu yok - migration gerekmez
- ✅ Compile-time type safety
- ✅ IntelliSense desteği (`AddressTypes.` yazinca tum tipler gorunur)
- ✅ Kolay test edilir
- ✅ Localization key'leri kod icerisinde

### NE ZAMAN KULLANILIR?

| Senaryo | Kod Bazli (TypeItem) | Veritabani Tablosu |
|---------|---------------------|-------------------|
| Sabit degerler (status, type) | ✅ | ❌ |
| Admin tarafindan yeni eklenmeyecek | ✅ | ❌ |
| Sadece sistem tarafindan kullanilacak | ✅ | ❌ |
| Admin UI'dan CRUD gerekli | ❌ | ✅ |
| Runtime'da yeni deger eklenebilmeli | ❌ | ✅ |

---

## 17. Liste/Tablo CRUD Butonları (ZORUNLU)

### HER LİSTEDE CRUD BUTONLARI OLMALI!

Bir liste veya tablo gösterildiğinde, her satırda mutlaka şu butonlar olmalıdır:

| Buton | Açıklama | Her zaman |
|-------|----------|-----------|
| **Düzenle** | Kaydı düzenle | ✅ ZORUNLU |
| **Sil** | Kaydı sil | ✅ ZORUNLU |
| **Context Butonlar** | Duruma göre (Doğrula, Onayla, Reddet vb.) | Gerekirse |

### ÖRNEK - Satır Butonları
```html
<td class="text-end">
    <button class="btn btn-sm btn-outline-primary me-1" data-bind="click: $parent.editItem">
        <i class="bi bi-pencil"></i>
    </button>
    <button class="btn btn-sm btn-outline-danger" data-bind="click: $parent.deleteItem">
        <i class="bi bi-trash"></i>
    </button>
    <!-- Context butonlar (opsiyonel) -->
    <button class="btn btn-sm btn-outline-success" data-bind="visible: !isVerified, click: $parent.verifyItem">
        <i class="bi bi-patch-check"></i>
    </button>
</td>
```

### NOT
Bu butonlar BAŞTAN eklenmeli. "Düzenle/Sil butonu yok" demek için kullanıcı uyarması BEKLENMEZ!

---

## 18. JSON camelCase Convention (API Response)

### .NET API'den dönen JSON HER ZAMAN camelCase!

.NET'in System.Text.Json default olarak camelCase kullanır. JavaScript tarafında buna uyulmalı.

### YANLIŞ - PascalCase kullanım
```javascript
// YANLIŞ - API PascalCase döndürmez!
optionsValue: 'Id',
optionsText: 'Name'
t.CssClass
```

### DOĞRU - camelCase kullanım
```javascript
// DOĞRU - API camelCase döndürür
optionsValue: 'id',
optionsText: 'name'
t.cssClass
```

### TypeItem API Response Örneği
```json
[
  { "id": 1, "name": "Temsilci", "cssClass": "bg-info" },
  { "id": 2, "name": "Ortak", "cssClass": "bg-success" }
]
```

---

## 19. API Controller Route Pattern

### Explicit Route KULLAN - [controller] KULLANMA!

`[Route("api/[controller]")]` beklenmedik sonuçlar üretebilir (örn: `TeamApiController` → `api/TeamApi`).

### YANLIŞ
```csharp
[Route("api/[controller]")]  // api/TeamApi olur, api/team değil!
public class TeamApiController : ControllerBase
```

### DOĞRU
```csharp
[Route("api/team")]  // Explicit ve net
public class TeamApiController : ControllerBase
```

---

## 20. Kayıt Sonrası Partial Refresh

### Kayıt sonrası SADECE ilgili listeyi yenile!

Modal kapatıldığında tüm sayfayı yenilemek yerine, sadece etkilenen listeyi yeniden yükle. Bu:
- Tab yapısını korur
- Kullanıcı konumunu korur
- Daha hızlı çalışır

### YANLIŞ
```javascript
// Kayıt sonrası tüm veriyi yenile
self.saveMember = function() {
    $.ajax({...}).done(function() {
        self.loadAllData();  // YANLIŞ - Tab state kaybolur!
    });
};
```

### DOĞRU
```javascript
// Kayıt sonrası sadece ilgili listeyi yenile
self.saveMember = function() {
    $.ajax({...}).done(function() {
        self.loadMembers();  // DOĞRU - Sadece üye listesi
    });
};
```

### Tab Yapısı Varsa
Eğer sayfada tab yapısı varsa, her tab için ayrı load fonksiyonu olmalı:
```javascript
self.loadPersonalData = function() { ... };
self.loadCorporateMembers = function() { ... };
self.loadDocuments = function() { ... };
```

---

## 12. Authorization Pattern (Capability-Based)

### Capability Kontrolü
Vendor'ların erişim yetkisi capability bazlı kontrol edilir. Her vendor bir veya daha fazla capability'ye sahip olabilir:

| Capability | Açıklama |
|------------|----------|
| Platform | Platform yöneticisi |
| Seller | Satıcı/Tedarikçi |
| Buyer | Alıcı |
| Carrier | Taşıyıcı |
| Insurance | Sigorta |
| Customs | Gümrük |

### RequireCapability Attribute Kullanımı

```csharp
using Bridgo.Authorization;

// Sadece Seller erişebilir
[RequireCapability(VendorCapabilities.Seller)]
public IActionResult Products() { ... }

// Sadece Buyer erişebilir
[RequireCapability(VendorCapabilities.Buyer)]
public IActionResult MyOrders() { ... }

// Seller VEYA Buyer erişebilir
[RequireCapability(VendorCapabilities.Seller, VendorCapabilities.Buyer)]
public IActionResult Messages() { ... }

// String olarak da kullanılabilir
[RequireCapability("Seller")]
public IActionResult Stock() { ... }
```

### Controller Seviyesinde Kullanım
```csharp
[Route("api/products")]
[RequireCapability(VendorCapabilities.Seller)]
public class ProductsApiController : ControllerBase
{
    // Tüm action'lar Seller gerektirir

    [HttpGet]
    public IActionResult GetAll() { ... }

    [HttpPost]
    public IActionResult Create([FromBody] ProductDto dto) { ... }
}
```

### Action Seviyesinde Override
```csharp
[RequireCapability(VendorCapabilities.Seller)]
public class ProductsApiController : ControllerBase
{
    // Controller'dan Seller gerekli

    [HttpGet]
    public IActionResult GetAll() { ... }

    // Bu action için Buyer da erişebilir
    [HttpGet("{id}")]
    [RequireCapability(VendorCapabilities.Seller, VendorCapabilities.Buyer)]
    public IActionResult GetById(int id) { ... }
}
```

### Nasıl Çalışır?
1. Kullanıcı giriş yaptığında `VendorCapability` claim'leri eklenir
2. `CapabilityAuthorizationHandler` her request'te claim'leri kontrol eder
3. Gerekli capability yoksa 403 Forbidden döner
4. System Admin tüm capability kontrollerinden muaftır

### Dosya Yapısı
```
Authorization/
  RequireCapabilityAttribute.cs   # Attribute + VendorCapabilities sabitleri
  CapabilityAuthorizationHandler.cs # Authorization handler
Services/
  CustomUserClaimsPrincipalFactory.cs # Claim'lere capability ekler
```

### Önemli Notlar
- Capability claim'leri login sırasında bir kez yüklenir
- Vendor'ın capability'si değişirse yeniden login gerekir
- System Admin (`IsSystemAdmin = true`) tüm kontrollerden muaftır
- VendorId olmayan kullanıcılar capability kontrolünden geçemez

---

## OZET

1. **ID'ler int (auto-increment) - GUID KULLANILMAZ**
2. **Her modul TEK Index.cshtml ile calisir**
3. **Create/Edit/Detail islemleri MODAL ile yapilir**
4. **ko.applyBindings MUTLAKA spesifik div'e baglanir**
5. **Ayri sayfa (Create.cshtml, Edit.cshtml, Detail.cshtml) OLMAZ**
6. **CDN KULLANILMAZ - Tum kutuphaneler yerel olmali**
7. **Native confirm() KULLANILMAZ - showConfirmModal() kullan**
8. **Controller'da DbContext KULLANILMAZ - Service layer kullan**
9. **Tum text'ler T() ile cekilir (cok dilli destek)**
10. **Tum entity'ler BaseEntity'den turetilir (soft delete, audit)**
11. **Is verileri VendorId'ye aittir - UserId sadece audit icindir**
12. **VendorId olmayan kullanici sisteme giremez (Admin haric)**
13. **Liste varsa CRUD butonları ŞART (Düzenle, Sil + context)**
14. **JSON camelCase: API `id`, `name`, `cssClass` döndürür**
15. **API Route: `[Route("api/team")]` kullan, `[Route("api/[controller]")]` KULLANMA**
16. **Kayıt sonrası: Sadece ilgili listeyi yenile (tab state korunur)**
17. **KnockoutJS: Tüm değişkenler ko.observable() veya ko.observableArray() olmalı**
18. **Capability Kontrolü: `[RequireCapability(VendorCapabilities.Seller)]` ile yetki kontrolü**
