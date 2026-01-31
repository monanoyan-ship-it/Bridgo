// Products Module - KnockoutJS ViewModel
(function () {
    'use strict';

    function ProductsViewModel() {
        const self = this;

        // Product Statuses (API'den yuklenir)
        self.productStatuses = ko.observableArray([]);

        // =====================================
        // FORM HELPERS (must be defined first)
        // =====================================

        // Varsayilan status ID (API'den yuklenir)
        self.defaultStatusId = 1;

        self.getEmptyProductForm = function () {
            return {
                name: ko.observable(''),
                sku: ko.observable(''),
                barcode: ko.observable(''),
                description: ko.observable(''),
                shortDescription: ko.observable(''),
                price: ko.observable(0),
                compareAtPrice: ko.observable(null),
                costPrice: ko.observable(null),
                currency: ko.observable('TRY'),
                stockQuantity: ko.observable(0),
                minStockLevel: ko.observable(null),
                trackStock: ko.observable(true),
                allowBackorder: ko.observable(false),
                weight: ko.observable(null),
                length: ko.observable(null),
                width: ko.observable(null),
                height: ko.observable(null),
                categoryId: ko.observable(''),
                tags: ko.observable(''),
                displayOrder: ko.observable(0),
                status: ko.observable(self.defaultStatusId),
                isFeatured: ko.observable(false),
                metaTitle: ko.observable(''),
                metaDescription: ko.observable(''),
                packagings: ko.observableArray([])
            };
        };

        // Packaging Levels (API'den yuklenir)
        self.packagingUnits = ko.observableArray([]);

        // State
        self.isLoading = ko.observable(false);
        self.isSaving = ko.observable(false);
        self.errorMessage = ko.observable('');

        // Products
        self.products = ko.observableArray([]);
        self.totalProductCount = ko.observable(0);
        self.selectedProducts = ko.observableArray([]);

        // Filters
        self.searchQuery = ko.observable('');
        self.filterCategory = ko.observable('');
        self.filterStatus = ko.observable('');
        self.filterInStock = ko.observable('');

        // Categories (for dropdown)
        self.categoriesForSelect = ko.observableArray([]);

        // Product Modal
        self.showProductModal = ko.observable(false);
        self.editingProductId = ko.observable(null);
        self.productForm = ko.observable(self.getEmptyProductForm());

        // Editing state (for packaging section visibility)
        self.isEditing = ko.computed(function () {
            return self.editingProductId() !== null;
        });

        // Image Modal
        self.showImageModal = ko.observable(false);
        self.currentProductId = ko.observable(null);
        self.currentProductImages = ko.observableArray([]);
        self.newImageUrl = ko.observable('');
        self.selectedFile = ko.observable(null);
        self.isUploading = ko.observable(false);

        // Price Tiers
        self.priceTiers = ko.observableArray([]);
        self.isSavingTier = ko.observable(false);
        self.editingTierId = ko.observable(null);
        self.newPriceTier = ko.observable({
            minQuantity: ko.observable(1),
            maxQuantity: ko.observable(null),
            price: ko.observable(0),
            description: ko.observable('')
        });

        // Stock Modal
        self.showStockModal = ko.observable(false);
        self.stockProductId = ko.observable(null);
        self.stockProductName = ko.observable('');
        self.isLoadingStock = ko.observable(false);
        self.isSavingStock = ko.observable(false);
        self.warehouses = ko.observableArray([]);
        self.warehouseStocks = ko.observableArray([]);

        // Category Subscription Suggestion Modal
        self.showSubscriptionModal = ko.observable(false);
        self.subscriptionCategoryId = ko.observable(null);
        self.subscriptionCategoryName = ko.observable('');
        self.isSubscribing = ko.observable(false);

        // Stock Computed Totals
        self.totalStockQuantity = ko.computed(function () {
            return self.warehouseStocks().reduce(function (sum, s) {
                return sum + (parseFloat(s.quantity()) || 0);
            }, 0);
        });

        self.totalReservedQuantity = ko.computed(function () {
            return self.warehouseStocks().reduce(function (sum, s) {
                return sum + (parseFloat(s.reservedQuantity()) || 0);
            }, 0);
        });

        self.totalAvailableQuantity = ko.computed(function () {
            return self.totalStockQuantity() - self.totalReservedQuantity();
        });

        // Computed
        self.allSelected = ko.computed({
            read: function () {
                return self.products().length > 0 &&
                    self.selectedProducts().length === self.products().length;
            },
            write: function (value) {
                if (value) {
                    self.selectedProducts(self.products().map(p => p.id));
                } else {
                    self.selectedProducts([]);
                }
            }
        });

        // Initialize
        self.init = function () {
            // URL parametrelerini oku
            var urlParams = new URLSearchParams(window.location.search);
            var categoryParam = urlParams.get('category') || urlParams.get('categoryId');
            if (categoryParam) {
                self.filterCategory(categoryParam);
            }

            // edit parametresi varsa urunu duzenleme icin ac
            var editParam = urlParams.get('edit');

            // Once statuses ve packaging levels yukle, sonra products (timing icin)
            Promise.all([
                self.loadProductStatuses(),
                self.loadPackagingLevels()
            ]).then(function() {
                self.loadProducts();

                // Eger edit parametresi varsa urunu yukle ve modali ac
                if (editParam) {
                    // URL'den edit parametresini temizle (yenilemede tekrar acmasin)
                    window.history.replaceState({}, document.title, window.location.pathname);
                    // Urunu duzenle
                    self.editProductById(parseInt(editParam));
                }
            });
            self.loadCategoriesForSelect();
        };

        // Edit product by ID (URL'den acmak icin)
        self.editProductById = function (productId) {
            if (!productId) return;
            self.editProduct({ id: productId });
        };

        // Load Product Statuses from API
        self.loadProductStatuses = function () {
            return fetch('/api/products/types/statuses')
                .then(response => response.json())
                .then(data => {
                    self.productStatuses(data || []);
                    // Varsayilan status'u bul
                    var defaultStatus = data.find(s => s.isDefault);
                    if (defaultStatus) {
                        self.defaultStatusId = defaultStatus.id;
                    }
                })
                .catch(err => {
                    console.error('Error loading product statuses:', err);
                });
        };

        // Get status by ID
        self.getStatusById = function (statusId) {
            return self.productStatuses().find(s => s.id === statusId) || { name: 'Bilinmiyor', cssClass: 'bg-secondary' };
        };

        // Load Packaging Levels from API
        self.loadPackagingLevels = function () {
            return fetch('/api/types/units/packaging')
                .then(response => response.json())
                .then(data => {
                    self.packagingUnits(data || []);
                })
                .catch(err => {
                    console.error('Error loading packaging levels:', err);
                });
        };

        // Get packaging level name by ID
        self.getPackagingUnitName = function (levelId) {
            var level = self.packagingUnits().find(l => l.id === levelId);
            return level ? level.name : '';
        };

        // =====================================
        // PACKAGING MANAGEMENT
        // =====================================

        // Add new packaging
        self.addPackaging = function () {
            // İlk paketleme varsayılan olsun
            var isFirst = self.productForm().packagings().length === 0;
            self.productForm().packagings.push({
                id: ko.observable(null),
                unitId: ko.observable(1),
                quantity: ko.observable(1),
                containsCount: ko.observable(1),
                barcode: ko.observable(''),
                sku: ko.observable(''),
                grossWeight: ko.observable(null),
                netWeight: ko.observable(null),
                length: ko.observable(null),
                width: ko.observable(null),
                height: ko.observable(null),
                minOrderQuantity: ko.observable(null),
                orderMultiple: ko.observable(null),
                isActive: ko.observable(true),
                isDefault: ko.observable(isFirst),
                displayOrder: ko.observable(self.productForm().packagings().length)
            });
        };

        // Set packaging as default sales unit
        self.setAsDefault = function (packaging) {
            // Tüm paketlemelerin isDefault'unu false yap
            self.productForm().packagings().forEach(function (p) {
                p.isDefault(false);
            });
            // Seçileni varsayılan yap
            packaging.isDefault(true);
        };

        // Remove packaging
        self.removePackaging = function (packaging) {
            self.productForm().packagings.remove(packaging);
        };

        // Load product packagings
        self.loadProductPackagings = function (productId) {
            return fetch('/api/products/' + productId + '/packaging')
                .then(response => response.json())
                .then(data => {
                    var packagings = (data || []).map(function(p) {
                        return {
                            id: ko.observable(p.id),
                            unitId: ko.observable(p.unitId),
                            quantity: ko.observable(p.quantity),
                            containsCount: ko.observable(p.containsCount),
                            barcode: ko.observable(p.barcode || ''),
                            sku: ko.observable(p.sku || ''),
                            grossWeight: ko.observable(p.grossWeight),
                            netWeight: ko.observable(p.netWeight),
                            length: ko.observable(p.length),
                            width: ko.observable(p.width),
                            height: ko.observable(p.height),
                            minOrderQuantity: ko.observable(p.minOrderQuantity),
                            orderMultiple: ko.observable(p.orderMultiple),
                            isActive: ko.observable(p.isActive),
                            isDefault: ko.observable(p.isDefault),
                            displayOrder: ko.observable(p.displayOrder)
                        };
                    });
                    self.productForm().packagings(packagings);
                })
                .catch(err => {
                    console.error('Error loading product packagings:', err);
                });
        };

        // Save product packagings
        self.saveProductPackagings = function (productId) {
            var packagings = self.productForm().packagings().map(function(p) {
                return {
                    id: p.id(),
                    unitId: p.unitId(),
                    quantity: parseInt(p.quantity()) || 1,
                    containsCount: parseInt(p.containsCount()) || 1,
                    barcode: p.barcode() || null,
                    sku: p.sku() || null,
                    grossWeight: p.grossWeight() ? parseFloat(p.grossWeight()) : null,
                    netWeight: p.netWeight() ? parseFloat(p.netWeight()) : null,
                    length: p.length() ? parseFloat(p.length()) : null,
                    width: p.width() ? parseFloat(p.width()) : null,
                    height: p.height() ? parseFloat(p.height()) : null,
                    minOrderQuantity: p.minOrderQuantity() ? parseInt(p.minOrderQuantity()) : null,
                    orderMultiple: p.orderMultiple() ? parseInt(p.orderMultiple()) : null,
                    isActive: p.isActive(),
                    isDefault: p.isDefault(),
                    displayOrder: p.displayOrder()
                };
            });

            return fetch('/api/products/' + productId + '/packaging', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ packagings: packagings })
            });
        };

        // =====================================
        // PRODUCTS
        // =====================================

        self.loadProducts = function () {
            self.isLoading(true);
            self.errorMessage('');
            self.selectedProducts([]);

            const params = new URLSearchParams();
            if (self.searchQuery()) params.append('search', self.searchQuery());
            if (self.filterCategory()) params.append('categoryId', self.filterCategory());
            if (self.filterStatus()) params.append('status', self.filterStatus());
            if (self.filterInStock()) params.append('inStock', self.filterInStock());

            fetch('/api/products?' + params.toString())
                .then(response => response.json())
                .then(data => {
                    const products = data.map(p => self.mapProduct(p));
                    self.products(products);
                    self.totalProductCount(products.length);
                })
                .catch(err => {
                    console.error('Error loading products:', err);
                    self.errorMessage(T('Common.Error.Load', 'Veriler yuklenirken hata olustu'));
                })
                .finally(() => self.isLoading(false));
        };

        self.mapProduct = function (p) {
            const status = self.getStatusById(p.productStatusId);
            return {
                id: p.id,
                name: p.name,
                sku: p.sku,
                barcode: p.barcode,
                price: p.price,
                currency: p.currency,
                stockQuantity: p.stockQuantity,
                status: p.productStatusId,
                statusText: status.name,
                statusBadgeClass: status.cssClass,
                isFeatured: p.isFeatured,
                categoryId: p.categoryId,
                categoryName: p.categoryName,
                mainImageUrl: p.mainImageUrl,
                priceFormatted: self.formatPrice(p.price, p.currency),
                stockBadgeClass: p.stockQuantity > 0 ? 'bg-success' : 'bg-danger',
                createdAt: p.createdAt
            };
        };

        self.formatPrice = function (price, currency) {
            return new Intl.NumberFormat('tr-TR', {
                style: 'currency',
                currency: currency || 'TRY'
            }).format(price);
        };

        self.resetFilters = function () {
            self.searchQuery('');
            self.filterCategory('');
            self.filterStatus('');
            self.filterInStock('');
            self.loadProducts();
        };

        self.toggleSelectAll = function () {
            return true; // Allow checkbox to toggle
        };

        // Product Modal
        self.openProductModal = function () {
            self.editingProductId(null);
            self.productForm(self.getEmptyProductForm());
            self.priceTiers([]); // Clear price tiers for new product
            self.resetNewPriceTier();
            self.showProductModal(true);
        };

        self.closeProductModal = function () {
            self.showProductModal(false);
            self.editingProductId(null);
        };

        self.editProduct = function (product) {
            self.isLoading(true);

            fetch('/api/products/' + product.id)
                .then(response => response.json())
                .then(data => {
                    self.editingProductId(data.id);
                    self.productForm({
                        name: ko.observable(data.name || ''),
                        sku: ko.observable(data.sku || ''),
                        barcode: ko.observable(data.barcode || ''),
                        description: ko.observable(data.description || ''),
                        shortDescription: ko.observable(data.shortDescription || ''),
                        price: ko.observable(data.price || 0),
                        compareAtPrice: ko.observable(data.compareAtPrice),
                        costPrice: ko.observable(data.costPrice),
                        currency: ko.observable(data.currency || 'TRY'),
                        stockQuantity: ko.observable(data.stockQuantity || 0),
                        minStockLevel: ko.observable(data.minStockLevel),
                        trackStock: ko.observable(data.trackStock),
                        allowBackorder: ko.observable(data.allowBackorder),
                        weight: ko.observable(data.weight),
                        length: ko.observable(data.length),
                        width: ko.observable(data.width),
                        height: ko.observable(data.height),
                        categoryId: ko.observable(data.categoryId || ''),
                        tags: ko.observable(data.tags || ''),
                        displayOrder: ko.observable(data.displayOrder || 0),
                        status: ko.observable(data.productStatusId),
                        isFeatured: ko.observable(data.isFeatured),
                        metaTitle: ko.observable(data.metaTitle || ''),
                        metaDescription: ko.observable(data.metaDescription || ''),
                        packagings: ko.observableArray([])
                    });
                    // Load price tiers for editing
                    self.priceTiers(data.priceTiers || []);
                    self.resetNewPriceTier();
                    // Load packagings for editing
                    self.loadProductPackagings(data.id);
                    self.showProductModal(true);
                })
                .catch(err => {
                    console.error('Error loading product:', err);
                    self.errorMessage(T('Common.Error.Load', 'Veriler yuklenirken hata olustu'));
                })
                .finally(() => self.isLoading(false));
        };

        self.saveProduct = function () {
            const form = self.productForm();
            const data = {
                name: form.name(),
                sku: form.sku() || null,
                barcode: form.barcode() || null,
                description: form.description() || null,
                shortDescription: form.shortDescription() || null,
                price: parseFloat(form.price()) || 0,
                compareAtPrice: form.compareAtPrice() ? parseFloat(form.compareAtPrice()) : null,
                costPrice: form.costPrice() ? parseFloat(form.costPrice()) : null,
                currency: form.currency() || 'TRY',
                stockQuantity: parseInt(form.stockQuantity()) || 0,
                minStockLevel: form.minStockLevel() ? parseInt(form.minStockLevel()) : null,
                trackStock: form.trackStock(),
                allowBackorder: form.allowBackorder(),
                weight: form.weight() ? parseFloat(form.weight()) : null,
                length: form.length() ? parseFloat(form.length()) : null,
                width: form.width() ? parseFloat(form.width()) : null,
                height: form.height() ? parseFloat(form.height()) : null,
                categoryId: form.categoryId() ? parseInt(form.categoryId()) : null,
                tags: form.tags() || null,
                displayOrder: parseInt(form.displayOrder()) || 0,
                productStatusId: parseInt(form.status()),
                isFeatured: form.isFeatured(),
                metaTitle: form.metaTitle() || null,
                metaDescription: form.metaDescription() || null
            };

            if (!data.name) {
                toastr.error(T('Products.Error.NameRequired', 'Urun adi zorunludur'));
                return;
            }

            if (!data.categoryId) {
                toastr.error(T('Products.Error.CategoryRequired', 'Kategori secimi zorunludur'));
                return;
            }

            self.isSaving(true);

            const url = self.editingProductId()
                ? '/api/products/' + self.editingProductId()
                : '/api/products';
            const method = self.editingProductId() ? 'PUT' : 'POST';
            const categoryId = data.categoryId;

            fetch(url, {
                method: method,
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(data)
            })
                .then(response => response.json())
                .then(result => {
                    // Urun kaydedildikten sonra paketleme bilgilerini de kaydet
                    var productId = result.id || self.editingProductId();
                    if (productId && self.productForm().packagings().length > 0) {
                        return self.saveProductPackagings(productId).then(function() {
                            return result;
                        });
                    }
                    return result;
                })
                .then(result => {
                    if (result.message) {
                        toastr.success(result.message);
                    }
                    self.closeProductModal();
                    self.loadProducts();

                    // Kategori secildiyse takip onerisi goster (hem yeni urun hem update icin)
                    if (categoryId) {
                        self.checkCategorySubscription(categoryId);
                    }
                })
                .catch(err => {
                    console.error('Error saving product:', err);
                    toastr.error(T('Common.Error.Save', 'Kaydedilirken hata olustu'));
                })
                .finally(() => self.isSaving(false));
        };

        self.deleteProduct = function (product) {
            showConfirmModal(
                T('Products.Delete.Title', 'Urunu Sil'),
                T('Products.Delete.Message', '{0} adli urunu silmek istediginizden emin misiniz?').replace('{0}', product.name),
                function () {
                    self.isSaving(true);
                    fetch('/api/products/' + product.id, { method: 'DELETE' })
                        .then(response => response.json())
                        .then(result => {
                            toastr.success(result.message || T('Common.Deleted', 'Silindi'));
                            self.loadProducts();
                        })
                        .catch(err => {
                            console.error('Error deleting product:', err);
                            toastr.error(T('Common.Error.Delete', 'Silinirken hata olustu'));
                        })
                        .finally(() => self.isSaving(false));
                }
            );
        };

        // Bulk Actions
        self.bulkUpdateStatus = function (status) {
            if (self.selectedProducts().length === 0) return;

            self.isSaving(true);
            fetch('/api/products/bulk-status', {
                method: 'PATCH',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    productIds: self.selectedProducts(),
                    status: status
                })
            })
                .then(response => response.json())
                .then(result => {
                    toastr.success(result.message);
                    self.loadProducts();
                })
                .catch(err => {
                    console.error('Error bulk updating:', err);
                    toastr.error(T('Common.Error.Update', 'Guncellenirken hata olustu'));
                })
                .finally(() => self.isSaving(false));
        };

        self.bulkDelete = function () {
            if (self.selectedProducts().length === 0) return;

            showConfirmModal(
                T('Products.BulkDelete.Title', 'Urunleri Sil'),
                T('Products.BulkDelete.Message', '{0} urun silinecek. Emin misiniz?').replace('{0}', self.selectedProducts().length),
                function () {
                    self.isSaving(true);
                    fetch('/api/products/bulk', {
                        method: 'DELETE',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify({ productIds: self.selectedProducts() })
                    })
                        .then(response => response.json())
                        .then(result => {
                            toastr.success(result.message);
                            self.loadProducts();
                        })
                        .catch(err => {
                            console.error('Error bulk deleting:', err);
                            toastr.error(T('Common.Error.Delete', 'Silinirken hata olustu'));
                        })
                        .finally(() => self.isSaving(false));
                }
            );
        };

        // Load categories for dropdown filter
        self.loadCategoriesForSelect = function () {
            fetch('/api/products/categories/select')
                .then(response => response.json())
                .then(data => {
                    self.categoriesForSelect(data || []);
                })
                .catch(err => {
                    console.error('Error loading categories for select:', err);
                });
        };

        // =====================================
        // IMAGES
        // =====================================

        self.openImageModal = function (product) {
            self.currentProductId(product.id);
            self.newImageUrl('');
            self.loadProductImages(product.id);
            self.showImageModal(true);
        };

        self.closeImageModal = function () {
            self.showImageModal(false);
            self.currentProductId(null);
            self.currentProductImages([]);
            self.selectedFile(null);
            self.newImageUrl('');
            var fileInput = document.getElementById('imageFileInput');
            if (fileInput) fileInput.value = '';
        };

        self.loadProductImages = function (productId) {
            fetch('/api/products/' + productId)
                .then(response => response.json())
                .then(data => {
                    self.currentProductImages(data.images || []);
                })
                .catch(err => {
                    console.error('Error loading product images:', err);
                });
        };

        self.addImage = function () {
            const url = self.newImageUrl();
            if (!url) return;

            self.isSaving(true);
            fetch('/api/products/' + self.currentProductId() + '/images', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ url: url })
            })
                .then(response => response.json())
                .then(result => {
                    if (result.id) {
                        toastr.success(result.message || T('Products.Image.Added', 'Gorsel eklendi'));
                        self.newImageUrl('');
                        self.loadProductImages(self.currentProductId());
                        self.loadProducts(); // Update main image
                    } else {
                        toastr.error(result.message || T('Common.Error.Save', 'Kaydedilirken hata olustu'));
                    }
                })
                .catch(err => {
                    console.error('Error adding image:', err);
                    toastr.error(T('Common.Error.Save', 'Kaydedilirken hata olustu'));
                })
                .finally(() => self.isSaving(false));
        };

        // File upload
        self.onFileSelected = function (data, event) {
            var files = event.target.files;
            if (files && files.length > 0) {
                self.selectedFile(files[0]);
            } else {
                self.selectedFile(null);
            }
        };

        self.uploadImage = function () {
            var file = self.selectedFile();
            if (!file) return;

            // Dosya boyutu kontrolu (5MB)
            if (file.size > 5 * 1024 * 1024) {
                toastr.error(T('Products.Upload.TooLarge', 'Dosya boyutu 5MB\'dan buyuk olamaz'));
                return;
            }

            self.isUploading(true);

            var formData = new FormData();
            formData.append('file', file);

            fetch('/api/products/' + self.currentProductId() + '/images/upload', {
                method: 'POST',
                body: formData
            })
                .then(response => response.json())
                .then(result => {
                    if (result.id) {
                        toastr.success(result.message || T('Products.Image.Uploaded', 'Gorsel yuklendi'));
                        self.selectedFile(null);
                        document.getElementById('imageFileInput').value = '';
                        self.loadProductImages(self.currentProductId());
                        self.loadProducts(); // Update main image
                    } else {
                        toastr.error(result.message || T('Common.Error.Upload', 'Yuklenirken hata olustu'));
                    }
                })
                .catch(err => {
                    console.error('Error uploading image:', err);
                    toastr.error(T('Common.Error.Upload', 'Yuklenirken hata olustu'));
                })
                .finally(() => self.isUploading(false));
        };

        self.deleteImage = function (imageId) {
            self.isSaving(true);
            fetch('/api/products/images/' + imageId, { method: 'DELETE' })
                .then(response => response.json())
                .then(result => {
                    toastr.success(result.message || T('Products.Image.Deleted', 'Gorsel silindi'));
                    self.loadProductImages(self.currentProductId());
                    self.loadProducts(); // Update main image
                })
                .catch(err => {
                    console.error('Error deleting image:', err);
                    toastr.error(T('Common.Error.Delete', 'Silinirken hata olustu'));
                })
                .finally(() => self.isSaving(false));
        };

        self.setMainImage = function (imageId) {
            self.isSaving(true);
            fetch('/api/products/images/' + imageId + '/main', { method: 'PATCH' })
                .then(response => response.json())
                .then(result => {
                    toastr.success(result.message || T('Products.Image.SetAsMain', 'Ana gorsel ayarlandi'));
                    self.loadProductImages(self.currentProductId());
                    self.loadProducts(); // Update main image
                })
                .catch(err => {
                    console.error('Error setting main image:', err);
                    toastr.error(T('Common.Error.Update', 'Guncellenirken hata olustu'));
                })
                .finally(() => self.isSaving(false));
        };

        // =====================================
        // PRICE TIERS
        // =====================================

        self.resetNewPriceTier = function () {
            self.editingTierId(null);
            self.newPriceTier({
                minQuantity: ko.observable(1),
                maxQuantity: ko.observable(null),
                price: ko.observable(0),
                description: ko.observable('')
            });
        };

        self.loadPriceTiers = function (productId) {
            fetch('/api/products/' + productId + '/price-tiers')
                .then(response => response.json())
                .then(data => {
                    self.priceTiers(data || []);
                })
                .catch(err => {
                    console.error('Error loading price tiers:', err);
                });
        };

        // Edit tier - form'a yukle
        self.editTier = function (tier) {
            self.editingTierId(tier.id);
            self.newPriceTier({
                minQuantity: ko.observable(tier.minQuantity),
                maxQuantity: ko.observable(tier.maxQuantity),
                price: ko.observable(tier.price),
                description: ko.observable(tier.description || '')
            });
        };

        // Cancel edit
        self.cancelEditTier = function () {
            self.resetNewPriceTier();
        };

        // Save tier (add or update)
        self.savePriceTier = function () {
            const tier = self.newPriceTier();
            const minQty = parseInt(tier.minQuantity()) || 0;
            const maxQty = tier.maxQuantity() ? parseInt(tier.maxQuantity()) : null;
            const price = parseFloat(tier.price()) || 0;

            // Validation
            if (minQty < 1) {
                toastr.error(T('Products.PriceTiers.Error.MinQty', 'Minimum miktar 1\'den kucuk olamaz'));
                return;
            }
            if (maxQty && maxQty <= minQty) {
                toastr.error(T('Products.PriceTiers.Error.MaxQty', 'Maksimum miktar, minimum miktardan buyuk olmalidir'));
                return;
            }
            if (price <= 0) {
                toastr.error(T('Products.PriceTiers.Error.Price', 'Fiyat 0\'dan buyuk olmalidir'));
                return;
            }

            const data = {
                minQuantity: minQty,
                maxQuantity: maxQty,
                price: price,
                description: tier.description() || null
            };

            self.isSavingTier(true);

            const isEditing = self.editingTierId() !== null;
            const url = isEditing
                ? '/api/products/price-tiers/' + self.editingTierId()
                : '/api/products/' + self.editingProductId() + '/price-tiers';
            const method = isEditing ? 'PUT' : 'POST';

            fetch(url, {
                method: method,
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(data)
            })
                .then(response => response.json())
                .then(result => {
                    if (result.id || result.message) {
                        const msg = isEditing
                            ? T('Products.PriceTiers.Updated', 'Fiyat esigi guncellendi')
                            : T('Products.PriceTiers.Added', 'Fiyat esigi eklendi');
                        toastr.success(result.message || msg);
                        self.resetNewPriceTier();
                        self.loadPriceTiers(self.editingProductId());
                    } else {
                        toastr.error(result.message || T('Common.Error.Save', 'Kaydedilirken hata olustu'));
                    }
                })
                .catch(err => {
                    console.error('Error saving price tier:', err);
                    toastr.error(T('Common.Error.Save', 'Kaydedilirken hata olustu'));
                })
                .finally(() => self.isSavingTier(false));
        };

        self.deletePriceTier = function (tier) {
            self.isSavingTier(true);
            fetch('/api/products/price-tiers/' + tier.id, { method: 'DELETE' })
                .then(response => response.json())
                .then(result => {
                    toastr.success(result.message || T('Products.PriceTiers.Deleted', 'Fiyat esigi silindi'));
                    self.loadPriceTiers(self.editingProductId());
                })
                .catch(err => {
                    console.error('Error deleting price tier:', err);
                    toastr.error(T('Common.Error.Delete', 'Silinirken hata olustu'));
                })
                .finally(() => self.isSavingTier(false));
        };

        // =====================================
        // STOCK MANAGEMENT
        // =====================================

        self.openStockModal = function (product) {
            self.stockProductId(product.id);
            self.stockProductName(product.name);
            self.warehouseStocks([]);
            self.showStockModal(true);
            self.loadProductStock(product.id);
        };

        self.closeStockModal = function () {
            self.showStockModal(false);
            self.stockProductId(null);
            self.stockProductName('');
            self.warehouseStocks([]);
        };

        self.loadProductStock = function (productId) {
            self.isLoadingStock(true);
            fetch('/api/products/' + productId + '/stock')
                .then(response => response.json())
                .then(data => {
                    // Her depo icin observable'lar olustur
                    const stocks = (data || []).map(s => ({
                        warehouseId: s.warehouseId,
                        warehouseName: s.warehouseName,
                        warehouseCode: s.warehouseCode,
                        isDefault: s.isDefault,
                        quantity: ko.observable(s.quantity || 0),
                        reservedQuantity: ko.observable(s.reservedQuantity || 0),
                        availableQuantity: ko.computed(function () {
                            // Bu closure icinde quantity ve reservedQuantity'ye erisemiyoruz
                            // Bu yuzden asagida tekrar tanimlayacagiz
                            return 0;
                        })
                    }));

                    // availableQuantity'yi her stock icin yeniden tanimla
                    stocks.forEach(s => {
                        s.availableQuantity = ko.computed(function () {
                            return (parseFloat(s.quantity()) || 0) - (parseFloat(s.reservedQuantity()) || 0);
                        });
                    });

                    self.warehouseStocks(stocks);
                    self.warehouses(stocks); // warehouses arrayini de doldur
                })
                .catch(err => {
                    console.error('Error loading product stock:', err);
                    toastr.error(T('Products.Stock.Error.Load', 'Stok bilgileri yuklenemedi'));
                })
                .finally(() => self.isLoadingStock(false));
        };

        self.saveStock = function () {
            const stocks = self.warehouseStocks().map(s => ({
                warehouseId: s.warehouseId,
                quantity: parseFloat(s.quantity()) || 0,
                reservedQuantity: parseFloat(s.reservedQuantity()) || 0
            }));

            self.isSavingStock(true);
            fetch('/api/products/' + self.stockProductId() + '/stock', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ stocks: stocks })
            })
                .then(response => response.json())
                .then(result => {
                    if (result.message) {
                        toastr.success(result.message);
                        self.closeStockModal();
                        self.loadProducts(); // Listeyi yenile
                    } else {
                        toastr.error(result.message || T('Common.Error.Save', 'Kaydedilirken hata olustu'));
                    }
                })
                .catch(err => {
                    console.error('Error saving stock:', err);
                    toastr.error(T('Common.Error.Save', 'Kaydedilirken hata olustu'));
                })
                .finally(() => self.isSavingStock(false));
        };

        // =====================================
        // CATEGORY SUBSCRIPTION SUGGESTION
        // =====================================

        self.checkCategorySubscription = function (categoryId) {
            // Kategori takip ediliyor mu kontrol et
            fetch('/api/suppliers/subscriptions/check/' + categoryId)
                .then(response => response.json())
                .then(result => {
                    if (!result.isSubscribed) {
                        // Kategori adini bul
                        var category = self.categoriesForSelect().find(c => c.id == categoryId);
                        if (category) {
                            self.subscriptionCategoryId(categoryId);
                            self.subscriptionCategoryName(category.name);
                            self.showSubscriptionModal(true);
                            // Bootstrap modal ac
                            var modal = new bootstrap.Modal(document.getElementById('subscriptionModal'));
                            modal.show();
                        }
                    }
                })
                .catch(err => {
                    console.log('Subscription check skipped:', err);
                });
        };

        self.subscribeToCategory = function () {
            var categoryId = self.subscriptionCategoryId();
            if (!categoryId) return;

            self.isSubscribing(true);
            fetch('/api/suppliers/subscriptions/quick', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ categoryId: categoryId })
            })
                .then(response => response.json())
                .then(result => {
                    toastr.success(result.message || T('Products.Subscription.Success', 'Kategori takibe alindi'));
                    self.closeSubscriptionModal();
                })
                .catch(err => {
                    console.error('Error subscribing:', err);
                    toastr.error(T('Common.Error.Save', 'Kaydedilirken hata olustu'));
                })
                .finally(() => self.isSubscribing(false));
        };

        self.closeSubscriptionModal = function () {
            self.showSubscriptionModal(false);
            self.subscriptionCategoryId(null);
            self.subscriptionCategoryName('');
            // Bootstrap modal kapat
            var modalEl = document.getElementById('subscriptionModal');
            if (modalEl) {
                var modal = bootstrap.Modal.getInstance(modalEl);
                if (modal) modal.hide();
            }
        };

        // Initialize
        self.init();
    }

    // Apply bindings when DOM is ready
    document.addEventListener('DOMContentLoaded', function () {
        const container = document.getElementById('products-app');
        if (container) {
            ko.applyBindings(new ProductsViewModel(), container);
        }
    });
})();
