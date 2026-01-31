// Dashboard/StockMovements.js - Stok Hareketleri
(function () {
    'use strict';

    function StockMovementsViewModel() {
        var self = this;

        // State
        self.isLoading = ko.observable(false);
        self.isProcessing = ko.observable(false);
        self.movements = ko.observableArray([]);
        self.stats = ko.observable({});
        self.lowStockAlerts = ko.observableArray([]);
        self.warehouses = ko.observableArray([]);

        // Product search for Stock In/Out
        self.productSearchResults = ko.observableArray([]);
        self.showProductResults = ko.observable(false);

        // Product search for Transfer
        self.transferProductSearchResults = ko.observableArray([]);
        self.showTransferProductResults = ko.observable(false);

        // Product search for Adjustment
        self.adjustProductSearchResults = ko.observableArray([]);
        self.showAdjustProductResults = ko.observable(false);

        // Pagination
        self.currentPage = ko.observable(1);
        self.totalPages = ko.observable(1);
        self.pageSize = ko.observable(20);

        // Filters
        self.filter = {
            search: ko.observable(''),
            warehouseId: ko.observable(''),
            movementType: ko.observable(''),
            fromDate: ko.observable(''),
            toDate: ko.observable('')
        };

        // Form for add/remove stock
        self.form = {
            productId: ko.observable(''),
            productSearch: ko.observable(''),
            selectedProduct: ko.observable(null),
            warehouseId: ko.observable(''),
            quantity: ko.observable(''),
            notes: ko.observable('')
        };

        // Transfer form
        self.transferForm = {
            productId: ko.observable(''),
            productSearch: ko.observable(''),
            selectedProduct: ko.observable(null),
            sourceWarehouseId: ko.observable(''),
            targetWarehouseId: ko.observable(''),
            quantity: ko.observable(''),
            notes: ko.observable('')
        };

        // Adjustment form
        self.adjustForm = {
            productId: ko.observable(''),
            productSearch: ko.observable(''),
            selectedProduct: ko.observable(null),
            warehouseId: ko.observable(''),
            newQuantity: ko.observable(''),
            reason: ko.observable('')
        };

        // Modals
        self.stockInModal = null;
        self.stockOutModal = null;
        self.transferModal = null;
        self.adjustmentModal = null;

        // Computed
        self.canSave = ko.computed(function () {
            return self.form.productId() && self.form.warehouseId() && parseFloat(self.form.quantity()) > 0;
        });

        self.canSaveTransfer = ko.computed(function () {
            return self.transferForm.productId() &&
                self.transferForm.sourceWarehouseId() &&
                self.transferForm.targetWarehouseId() &&
                self.transferForm.sourceWarehouseId() !== self.transferForm.targetWarehouseId() &&
                parseFloat(self.transferForm.quantity()) > 0;
        });

        self.canSaveAdjustment = ko.computed(function () {
            return self.adjustForm.productId() &&
                self.adjustForm.warehouseId() &&
                self.adjustForm.newQuantity() !== '' &&
                self.adjustForm.reason();
        });

        self.pageNumbers = ko.computed(function () {
            var pages = [];
            var total = self.totalPages();
            var current = self.currentPage();
            var start = Math.max(1, current - 2);
            var end = Math.min(total, current + 2);

            for (var i = start; i <= end; i++) {
                pages.push(i);
            }
            return pages;
        });

        // Product search subscriptions
        var productSearchTimeout;
        self.form.productSearch.subscribe(function (val) {
            clearTimeout(productSearchTimeout);
            if (!val || val.length < 2) {
                self.productSearchResults([]);
                return;
            }
            productSearchTimeout = setTimeout(function () {
                self.searchProducts(val, self.productSearchResults);
            }, 300);
        });

        var transferSearchTimeout;
        self.transferForm.productSearch.subscribe(function (val) {
            clearTimeout(transferSearchTimeout);
            if (!val || val.length < 2) {
                self.transferProductSearchResults([]);
                return;
            }
            transferSearchTimeout = setTimeout(function () {
                self.searchProducts(val, self.transferProductSearchResults);
            }, 300);
        });

        var adjustSearchTimeout;
        self.adjustForm.productSearch.subscribe(function (val) {
            clearTimeout(adjustSearchTimeout);
            if (!val || val.length < 2) {
                self.adjustProductSearchResults([]);
                return;
            }
            adjustSearchTimeout = setTimeout(function () {
                self.searchProducts(val, self.adjustProductSearchResults);
            }, 300);
        });

        // Search products API call
        self.searchProducts = function (query, resultsObservable) {
            $.ajax({
                url: '/api/products/search',
                data: { search: query, pageSize: 10 },
                success: function (result) {
                    resultsObservable(result || []);
                },
                error: function () {
                    resultsObservable([]);
                }
            });
        };

        // Show/hide dropdown handlers for Stock In/Out
        self.showProductDropdown = function () {
            self.showProductResults(true);
        };

        self.hideProductDropdownDelayed = function () {
            setTimeout(function () {
                self.showProductResults(false);
            }, 200);
        };

        self.selectProduct = function (product) {
            self.form.productId(product.id);
            self.form.selectedProduct(product);
            self.form.productSearch('');
            self.productSearchResults([]);
            self.showProductResults(false);
        };

        self.clearSelectedProduct = function () {
            self.form.productId('');
            self.form.selectedProduct(null);
            self.form.productSearch('');
        };

        // Show/hide dropdown handlers for Transfer
        self.showTransferProductDropdown = function () {
            self.showTransferProductResults(true);
        };

        self.hideTransferProductDropdownDelayed = function () {
            setTimeout(function () {
                self.showTransferProductResults(false);
            }, 200);
        };

        self.selectTransferProduct = function (product) {
            self.transferForm.productId(product.id);
            self.transferForm.selectedProduct(product);
            self.transferForm.productSearch('');
            self.transferProductSearchResults([]);
            self.showTransferProductResults(false);
        };

        self.clearTransferSelectedProduct = function () {
            self.transferForm.productId('');
            self.transferForm.selectedProduct(null);
            self.transferForm.productSearch('');
        };

        // Show/hide dropdown handlers for Adjustment
        self.showAdjustProductDropdown = function () {
            self.showAdjustProductResults(true);
        };

        self.hideAdjustProductDropdownDelayed = function () {
            setTimeout(function () {
                self.showAdjustProductResults(false);
            }, 200);
        };

        self.selectAdjustProduct = function (product) {
            self.adjustForm.productId(product.id);
            self.adjustForm.selectedProduct(product);
            self.adjustForm.productSearch('');
            self.adjustProductSearchResults([]);
            self.showAdjustProductResults(false);
        };

        self.clearAdjustSelectedProduct = function () {
            self.adjustForm.productId('');
            self.adjustForm.selectedProduct(null);
            self.adjustForm.productSearch('');
        };

        // Filter subscriptions
        var filterTimeout;
        function onFilterChange() {
            clearTimeout(filterTimeout);
            filterTimeout = setTimeout(function () {
                self.currentPage(1);
                self.loadMovements();
            }, 300);
        }

        self.filter.search.subscribe(onFilterChange);
        self.filter.warehouseId.subscribe(onFilterChange);
        self.filter.movementType.subscribe(onFilterChange);
        self.filter.fromDate.subscribe(onFilterChange);
        self.filter.toDate.subscribe(onFilterChange);

        self.resetFilters = function () {
            self.filter.search('');
            self.filter.warehouseId('');
            self.filter.movementType('');
            self.filter.fromDate('');
            self.filter.toDate('');
        };

        // Load data
        self.loadMovements = function () {
            self.isLoading(true);

            var params = {
                page: self.currentPage(),
                pageSize: self.pageSize()
            };

            if (self.filter.search()) params.search = self.filter.search();
            if (self.filter.warehouseId()) params.warehouseId = self.filter.warehouseId();
            if (self.filter.movementType()) params.movementType = self.filter.movementType();
            if (self.filter.fromDate()) params.fromDate = self.filter.fromDate();
            if (self.filter.toDate()) params.toDate = self.filter.toDate();

            $.ajax({
                url: '/api/stock/movements',
                data: params,
                success: function (result) {
                    self.movements(result.items || []);
                    self.totalPages(result.totalPages || 1);
                },
                error: function () {
                    toastr.error('Stok hareketleri yuklenirken hata olustu.');
                },
                complete: function () {
                    self.isLoading(false);
                }
            });
        };

        self.loadStats = function () {
            $.ajax({
                url: '/api/stock/stats',
                success: function (result) {
                    self.stats(result || {});
                }
            });
        };

        self.loadLowStockAlerts = function () {
            $.ajax({
                url: '/api/stock/alerts/low-stock',
                success: function (result) {
                    self.lowStockAlerts(result || []);
                }
            });
        };

        self.loadWarehouses = function () {
            $.ajax({
                url: '/api/warehouses',
                success: function (result) {
                    self.warehouses((result || []).map(function (w) {
                        return { id: w.id, name: w.name };
                    }));
                }
            });
        };

        // Pagination
        self.prevPage = function () {
            if (self.currentPage() > 1) {
                self.currentPage(self.currentPage() - 1);
                self.loadMovements();
            }
        };

        self.nextPage = function () {
            if (self.currentPage() < self.totalPages()) {
                self.currentPage(self.currentPage() + 1);
                self.loadMovements();
            }
        };

        self.goToPage = function (page) {
            self.currentPage(page);
            self.loadMovements();
        };

        // Show modals
        self.resetForm = function () {
            self.form.productId('');
            self.form.productSearch('');
            self.form.selectedProduct(null);
            self.form.warehouseId('');
            self.form.quantity('');
            self.form.notes('');
            self.productSearchResults([]);
            self.showProductResults(false);
        };

        self.showAddStock = function () {
            self.resetForm();
            self.stockInModal.show();
        };

        self.showRemoveStock = function () {
            self.resetForm();
            self.stockOutModal.show();
        };

        self.showTransfer = function () {
            self.transferForm.productId('');
            self.transferForm.productSearch('');
            self.transferForm.selectedProduct(null);
            self.transferForm.sourceWarehouseId('');
            self.transferForm.targetWarehouseId('');
            self.transferForm.quantity('');
            self.transferForm.notes('');
            self.transferProductSearchResults([]);
            self.showTransferProductResults(false);
            self.transferModal.show();
        };

        self.showAdjustment = function () {
            self.adjustForm.productId('');
            self.adjustForm.productSearch('');
            self.adjustForm.selectedProduct(null);
            self.adjustForm.warehouseId('');
            self.adjustForm.newQuantity('');
            self.adjustForm.reason('');
            self.adjustProductSearchResults([]);
            self.showAdjustProductResults(false);
            self.adjustmentModal.show();
        };

        // Save actions
        self.saveStockIn = function () {
            self.isProcessing(true);

            $.ajax({
                url: '/api/stock/in',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({
                    productId: parseInt(self.form.productId()),
                    warehouseId: parseInt(self.form.warehouseId()),
                    quantity: parseFloat(self.form.quantity()),
                    notes: self.form.notes()
                }),
                success: function () {
                    self.stockInModal.hide();
                    toastr.success('Stok girisi yapildi.');
                    self.loadMovements();
                    self.loadStats();
                    self.loadLowStockAlerts();
                },
                error: function (xhr) {
                    var msg = xhr.responseJSON?.message || 'Bir hata olustu.';
                    toastr.error(msg);
                },
                complete: function () {
                    self.isProcessing(false);
                }
            });
        };

        self.saveStockOut = function () {
            self.isProcessing(true);

            $.ajax({
                url: '/api/stock/out',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({
                    productId: parseInt(self.form.productId()),
                    warehouseId: parseInt(self.form.warehouseId()),
                    quantity: parseFloat(self.form.quantity()),
                    notes: self.form.notes()
                }),
                success: function () {
                    self.stockOutModal.hide();
                    toastr.success('Stok cikisi yapildi.');
                    self.loadMovements();
                    self.loadStats();
                    self.loadLowStockAlerts();
                },
                error: function (xhr) {
                    var msg = xhr.responseJSON?.message || 'Bir hata olustu.';
                    toastr.error(msg);
                },
                complete: function () {
                    self.isProcessing(false);
                }
            });
        };

        self.saveTransfer = function () {
            self.isProcessing(true);

            $.ajax({
                url: '/api/stock/transfer',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({
                    productId: parseInt(self.transferForm.productId()),
                    sourceWarehouseId: parseInt(self.transferForm.sourceWarehouseId()),
                    targetWarehouseId: parseInt(self.transferForm.targetWarehouseId()),
                    quantity: parseFloat(self.transferForm.quantity()),
                    notes: self.transferForm.notes()
                }),
                success: function () {
                    self.transferModal.hide();
                    toastr.success('Transfer tamamlandi.');
                    self.loadMovements();
                    self.loadStats();
                },
                error: function (xhr) {
                    var msg = xhr.responseJSON?.message || 'Bir hata olustu.';
                    toastr.error(msg);
                },
                complete: function () {
                    self.isProcessing(false);
                }
            });
        };

        self.saveAdjustment = function () {
            self.isProcessing(true);

            $.ajax({
                url: '/api/stock/adjust',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({
                    productId: parseInt(self.adjustForm.productId()),
                    warehouseId: parseInt(self.adjustForm.warehouseId()),
                    newQuantity: parseFloat(self.adjustForm.newQuantity()),
                    reason: self.adjustForm.reason()
                }),
                success: function () {
                    self.adjustmentModal.hide();
                    toastr.success('Stok duzeltmesi yapildi.');
                    self.loadMovements();
                    self.loadStats();
                    self.loadLowStockAlerts();
                },
                error: function (xhr) {
                    var msg = xhr.responseJSON?.message || 'Bir hata olustu.';
                    toastr.error(msg);
                },
                complete: function () {
                    self.isProcessing(false);
                }
            });
        };

        // Helper functions
        self.formatCurrency = function (amount) {
            if (!amount) return '0';
            return new Intl.NumberFormat('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(amount);
        };

        self.formatDateTime = function (dateStr) {
            if (!dateStr) return '';
            return new Date(dateStr).toLocaleString('tr-TR');
        };

        self.getMovementTypeBadge = function (type) {
            var badges = {
                1: 'bg-success',      // In
                2: 'bg-danger',       // Out
                3: 'bg-primary',      // Transfer
                4: 'bg-warning',      // Adjustment
                5: 'bg-info',         // Reserve
                6: 'bg-secondary'     // Unreserve
            };
            return badges[type] || 'bg-secondary';
        };

        // Make helpers available globally
        window.formatDateTime = self.formatDateTime;
        window.getMovementTypeBadge = self.getMovementTypeBadge;

        // Init
        self.init = function () {
            var stockInEl = document.getElementById('stockInModal');
            var stockOutEl = document.getElementById('stockOutModal');
            var transferEl = document.getElementById('transferModal');
            var adjustmentEl = document.getElementById('adjustmentModal');

            if (stockInEl) self.stockInModal = new bootstrap.Modal(stockInEl);
            if (stockOutEl) self.stockOutModal = new bootstrap.Modal(stockOutEl);
            if (transferEl) self.transferModal = new bootstrap.Modal(transferEl);
            if (adjustmentEl) self.adjustmentModal = new bootstrap.Modal(adjustmentEl);

            self.loadWarehouses();
            self.loadStats();
            self.loadLowStockAlerts();
            self.loadMovements();
        };

        self.init();
    }

    // Apply bindings
    $(function () {
        ko.applyBindings(new StockMovementsViewModel(), document.getElementById('stock-movements-app'));
    });
})();
