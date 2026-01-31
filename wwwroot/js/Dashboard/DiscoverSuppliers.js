// Dashboard/DiscoverSuppliers.js - Tedarikci Kesfi
(function () {
    'use strict';

    function DiscoverSuppliersViewModel() {
        var self = this;

        // State
        self.isLoading = ko.observable(false);
        self.suppliers = ko.observableArray([]);
        self.recommended = ko.observableArray([]);
        self.categories = ko.observableArray([]);
        self.selectedSupplier = ko.observable(null);

        // Stats
        self.totalSuppliers = ko.observable(0);
        self.verifiedCount = ko.observable(0);
        self.favoriteCount = ko.observable(0);
        self.recommendedCount = ko.observable(0);

        // Pagination
        self.currentPage = ko.observable(1);
        self.pageSize = ko.observable(12);
        self.totalPages = ko.observable(0);

        // Filters
        self.filter = {
            search: ko.observable(''),
            categoryId: ko.observable(null),
            sortBy: ko.observable(''),
            hasProducts: ko.observable(false)
        };

        // Show recommended only when no filters
        self.showRecommended = ko.computed(function () {
            return !self.filter.search() &&
                !self.filter.categoryId() &&
                !self.filter.sortBy() &&
                !self.filter.hasProducts() &&
                self.currentPage() === 1;
        });

        // Page numbers for pagination
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

        // Modal
        self.modal = null;

        // Debounced search
        var searchTimeout;
        self.filter.search.subscribe(function () {
            clearTimeout(searchTimeout);
            searchTimeout = setTimeout(function () {
                self.currentPage(1);
                self.loadSuppliers();
            }, 300);
        });

        self.filter.categoryId.subscribe(function () {
            self.currentPage(1);
            self.loadSuppliers();
        });

        self.filter.sortBy.subscribe(function () {
            self.currentPage(1);
            self.loadSuppliers();
        });

        self.filter.hasProducts.subscribe(function () {
            self.currentPage(1);
            self.loadSuppliers();
        });

        // Load suppliers
        self.loadSuppliers = function () {
            self.isLoading(true);

            var params = new URLSearchParams({
                page: self.currentPage(),
                pageSize: self.pageSize()
            });

            if (self.filter.search()) params.set('search', self.filter.search());
            if (self.filter.categoryId()) params.set('categoryId', self.filter.categoryId());
            if (self.filter.sortBy()) params.set('sortBy', self.filter.sortBy());
            if (self.filter.hasProducts()) params.set('hasProducts', 'true');

            fetch('/api/buyer/suppliers?' + params.toString())
                .then(function (response) { return response.json(); })
                .then(function (data) {
                    self.suppliers(data.items || []);
                    self.totalSuppliers(data.totalCount || 0);
                    self.totalPages(data.totalPages || 0);
                    self.verifiedCount(data.items ? data.items.filter(function (s) { return s.isVerified; }).length : 0);
                })
                .catch(function (err) {
                    console.error('Error loading suppliers:', err);
                    toastr.error('Tedarikciler yuklenirken hata olustu');
                })
                .finally(function () {
                    self.isLoading(false);
                });
        };

        // Load recommended suppliers
        self.loadRecommended = function () {
            fetch('/api/buyer/suppliers/recommended?count=6')
                .then(function (response) { return response.json(); })
                .then(function (data) {
                    self.recommended(data || []);
                    self.recommendedCount(data ? data.length : 0);
                })
                .catch(function (err) {
                    console.error('Error loading recommended:', err);
                });
        };

        // Load categories
        self.loadCategories = function () {
            fetch('/api/catalog/categories')
                .then(function (response) { return response.json(); })
                .then(function (data) {
                    // Flatten the tree
                    var flat = [];
                    function flatten(items, prefix) {
                        items.forEach(function (item) {
                            flat.push({
                                id: item.id,
                                name: prefix + item.name
                            });
                            if (item.children && item.children.length > 0) {
                                flatten(item.children, prefix + '  ');
                            }
                        });
                    }
                    flatten(data || [], '');
                    self.categories(flat);
                })
                .catch(function (err) {
                    console.error('Error loading categories:', err);
                });
        };

        // Load favorites count
        self.loadFavoriteCount = function () {
            fetch('/api/buyer/favorites')
                .then(function (response) { return response.json(); })
                .then(function (data) {
                    self.favoriteCount(data ? data.length : 0);
                })
                .catch(function (err) {
                    console.error('Error loading favorites:', err);
                });
        };

        // View supplier detail
        self.viewSupplier = function (supplier) {
            fetch('/api/buyer/suppliers/' + supplier.id)
                .then(function (response) { return response.json(); })
                .then(function (data) {
                    self.selectedSupplier(data);
                    self.modal.show();
                })
                .catch(function (err) {
                    console.error('Error loading supplier detail:', err);
                    toastr.error('Tedarikci bilgileri yuklenemedi');
                });
        };

        // Toggle favorite
        self.toggleFavorite = function (supplier) {
            if (supplier.isFavorite) {
                // Remove from favorites
                fetch('/api/buyer/favorites/' + supplier.favoriteId, { method: 'DELETE' })
                    .then(function (response) {
                        if (response.ok) {
                            supplier.isFavorite = false;
                            supplier.favoriteId = null;
                            self.favoriteCount(self.favoriteCount() - 1);
                            self.suppliers.valueHasMutated();
                            self.recommended.valueHasMutated();
                            if (self.selectedSupplier()) {
                                self.selectedSupplier().isFavorite = false;
                                self.selectedSupplier.valueHasMutated();
                            }
                            toastr.success('Favorilerden cikarildi');
                        }
                    })
                    .catch(function (err) {
                        console.error('Error removing favorite:', err);
                        toastr.error('Islem basarisiz');
                    });
            } else {
                // Add to favorites
                fetch('/api/buyer/favorites', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ sellerVendorId: supplier.id })
                })
                    .then(function (response) { return response.json(); })
                    .then(function (data) {
                        if (data.id) {
                            supplier.isFavorite = true;
                            supplier.favoriteId = data.id;
                            self.favoriteCount(self.favoriteCount() + 1);
                            self.suppliers.valueHasMutated();
                            self.recommended.valueHasMutated();
                            if (self.selectedSupplier()) {
                                self.selectedSupplier().isFavorite = true;
                                self.selectedSupplier().favoriteId = data.id;
                                self.selectedSupplier.valueHasMutated();
                            }
                            toastr.success('Favorilere eklendi');
                        } else if (data.message) {
                            toastr.warning(data.message);
                        }
                    })
                    .catch(function (err) {
                        console.error('Error adding favorite:', err);
                        toastr.error('Islem basarisiz');
                    });
            }
        };

        // Reset filters
        self.resetFilters = function () {
            self.filter.search('');
            self.filter.categoryId(null);
            self.filter.sortBy('');
            self.filter.hasProducts(false);
            self.currentPage(1);
        };

        // Go to page
        self.goToPage = function (page) {
            if (page >= 1 && page <= self.totalPages()) {
                self.currentPage(page);
                self.loadSuppliers();
            }
        };

        // Init
        self.init = function () {
            var modalEl = document.getElementById('supplierModal');
            if (modalEl) {
                self.modal = new bootstrap.Modal(modalEl);
            }

            self.loadCategories();
            self.loadSuppliers();
            self.loadRecommended();
            self.loadFavoriteCount();
        };

        self.init();
    }

    // Apply bindings
    $(function () {
        ko.applyBindings(new DiscoverSuppliersViewModel(), document.getElementById('discover-suppliers-app'));
    });
})();
