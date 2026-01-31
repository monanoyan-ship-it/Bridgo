(function () {
    'use strict';

    function CategoryViewModel() {
        var self = this;

        // Config
        var config = window.catalogConfig || {};

        // Observables
        self.isLoading = ko.observable(false);
        self.viewMode = ko.observable('grid'); // grid, list

        // Filter
        self.filter = {
            search: ko.observable(''),
            sortBy: ko.observable('newest'),
            inStock: ko.observable(false),
            minPrice: ko.observable(null),
            maxPrice: ko.observable(null),
            page: ko.observable(1),
            pageSize: ko.observable(24)
        };

        // Results
        self.result = ko.observable({
            products: [],
            totalCount: 0,
            page: 1,
            pageSize: 24,
            totalPages: 0
        });

        // Facets
        self.facets = ko.observableArray([]);
        self.selectedAttributeValues = ko.observableArray([]);

        // Stats
        self.stats = ko.observable({
            totalProducts: 0,
            totalCategories: 0,
            totalVendors: 0
        });

        // Computed
        self.hasActiveFilters = ko.computed(function () {
            return self.filter.search() ||
                self.filter.inStock() ||
                self.filter.minPrice() ||
                self.filter.maxPrice() ||
                self.selectedAttributeValues().length > 0;
        });

        self.pageNumbers = ko.computed(function () {
            var current = self.filter.page();
            var total = self.result().totalPages;
            var pages = [];
            var start = Math.max(1, current - 2);
            var end = Math.min(total, current + 2);

            for (var i = start; i <= end; i++) {
                pages.push(i);
            }
            return pages;
        });

        // Debounced search
        var searchTimeout = null;
        self.filter.search.subscribe(function () {
            clearTimeout(searchTimeout);
            searchTimeout = setTimeout(function () {
                self.filter.page(1);
                self.loadProducts();
            }, 400);
        });

        // Sort change
        self.filter.sortBy.subscribe(function () {
            self.filter.page(1);
            self.loadProducts();
        });

        // In stock filter change
        self.filter.inStock.subscribe(function () {
            self.filter.page(1);
            self.loadProducts();
        });

        // Attribute filter change
        self.selectedAttributeValues.subscribe(function () {
            self.filter.page(1);
            self.loadProducts();
        });

        // Functions
        self.loadProducts = function () {
            self.isLoading(true);

            var params = {
                search: self.filter.search() || undefined,
                sortBy: self.filter.sortBy(),
                inStock: self.filter.inStock() || undefined,
                minPrice: self.filter.minPrice() || undefined,
                maxPrice: self.filter.maxPrice() || undefined,
                page: self.filter.page(),
                pageSize: self.filter.pageSize()
            };

            // Attribute value filters
            var selectedValues = self.selectedAttributeValues();
            if (selectedValues.length > 0) {
                params.attributeValueIds = selectedValues.join(',');
            }

            var url = '/api/catalog/products/category/' + config.categorySlug + '?' + $.param(params);

            $.get(url)
                .done(function (data) {
                    self.result(data);
                    // Update facets from response
                    self.facets(data.facets || []);
                })
                .fail(function (xhr) {
                    console.error('Urunler yuklenemedi:', xhr.responseText);
                    toastr.error(T('Catalog.Error.LoadProducts', 'Urunler yuklenirken hata olustu.'));
                })
                .always(function () {
                    self.isLoading(false);
                });
        };

        self.loadStats = function () {
            $.get('/api/catalog/stats')
                .done(function (data) {
                    self.stats(data);
                })
                .fail(function (xhr) {
                    console.error('Istatistikler yuklenemedi:', xhr.responseText);
                });
        };

        self.resetFilters = function () {
            self.filter.search('');
            self.filter.inStock(false);
            self.filter.minPrice(null);
            self.filter.maxPrice(null);
            self.selectedAttributeValues([]);
            self.filter.page(1);
            self.loadProducts();
        };

        self.clearAttributeFilters = function () {
            self.selectedAttributeValues([]);
        };

        self.goToPage = function (page) {
            self.filter.page(page);
            self.loadProducts();
            window.scrollTo({ top: 0, behavior: 'smooth' });
        };

        self.prevPage = function () {
            if (self.filter.page() > 1) {
                self.goToPage(self.filter.page() - 1);
            }
        };

        self.nextPage = function () {
            if (self.filter.page() < self.result().totalPages) {
                self.goToPage(self.filter.page() + 1);
            }
        };

        self.formatPrice = function (price, currency) {
            if (!price) return '';
            var formatted = new Intl.NumberFormat('tr-TR', {
                minimumFractionDigits: 2,
                maximumFractionDigits: 2
            }).format(price);

            var currencySymbols = {
                'TRY': '\u20BA',
                'USD': '$',
                'EUR': '\u20AC',
                'GBP': '\u00A3'
            };
            var symbol = currencySymbols[currency] || currency;
            return formatted + ' ' + symbol;
        };

        // Init
        self.init = function () {
            self.loadStats();
            self.loadProducts();
        };

        self.init();
    }

    // T function fallback
    if (typeof T === 'undefined') {
        window.T = function (key, defaultValue) { return defaultValue; };
    }

    // Apply bindings
    $(function () {
        var app = document.getElementById('catalog-app');
        if (app) {
            ko.applyBindings(new CategoryViewModel(), app);
        }
    });
})();
