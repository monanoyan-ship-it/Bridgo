(function () {
    'use strict';

    function CatalogViewModel() {
        var self = this;

        // Config
        var config = window.catalogConfig || {};

        // Observables
        self.isLoading = ko.observable(false);
        self.viewMode = ko.observable('grid'); // grid, list
        self.pageTitle = ko.observable(T('Catalog.Title', 'Urun Katalogu'));

        // Filter
        self.filter = {
            search: ko.observable(config.initialSearch || ''),
            categoryId: ko.observable(config.initialCategoryId || null),
            sortBy: ko.observable(config.initialSort || 'newest'),
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

        // Categories (flat for mobile dropdown)
        self.flatCategories = ko.observableArray([]);

        // Category Search
        self.categorySearch = ko.observable('');
        self.allCategories = ko.observableArray([]);

        // Filtered categories based on search
        self.filteredCategories = ko.computed(function () {
            var search = (self.categorySearch() || '').toLowerCase().trim();
            if (!search) return [];

            var all = self.allCategories();
            return all.filter(function (cat) {
                return cat.name.toLowerCase().indexOf(search) > -1;
            });
        });

        // Computed
        self.hasActiveFilters = ko.computed(function () {
            return self.filter.search() ||
                self.filter.categoryId() ||
                self.filter.inStock() ||
                self.filter.minPrice() ||
                self.filter.maxPrice() ||
                self.selectedAttributeValues().length > 0;
        });

        self.activeFilterTags = ko.computed(function () {
            var tags = [];
            if (self.filter.search()) {
                tags.push({ key: 'search', label: '"' + self.filter.search() + '"' });
            }
            if (self.filter.inStock()) {
                tags.push({ key: 'inStock', label: T('Catalog.Filter.InStock', 'Stokta Var') });
            }
            if (self.filter.minPrice()) {
                tags.push({ key: 'minPrice', label: 'Min: ' + self.filter.minPrice() });
            }
            if (self.filter.maxPrice()) {
                tags.push({ key: 'maxPrice', label: 'Max: ' + self.filter.maxPrice() });
            }
            // Attribute filters
            var selectedValues = self.selectedAttributeValues();
            if (selectedValues.length > 0) {
                // Find labels from facets
                var facets = self.facets();
                selectedValues.forEach(function (valueId) {
                    facets.forEach(function (facet) {
                        var foundValue = facet.values.find(function (v) { return v.valueId === valueId; });
                        if (foundValue) {
                            tags.push({
                                key: 'attr_' + valueId,
                                label: facet.attributeName + ': ' + foundValue.value
                            });
                        }
                    });
                });
            }
            return tags;
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

        // Category change - also clear attribute filters
        self.filter.categoryId.subscribe(function () {
            self.filter.page(1);
            self.selectedAttributeValues([]); // Clear attribute filters on category change
            self.loadProducts();
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
                categoryId: self.filter.categoryId() || undefined,
                sortBy: self.filter.sortBy(),
                inStock: self.filter.inStock() || undefined,
                minPrice: self.filter.minPrice() || undefined,
                maxPrice: self.filter.maxPrice() || undefined,
                page: self.filter.page(),
                pageSize: self.filter.pageSize()
            };

            // Vendor slug varsa
            if (config.vendorSlug) {
                params.vendorId = config.vendorSlug;
            }

            // Attribute value filters
            var selectedValues = self.selectedAttributeValues();
            if (selectedValues.length > 0) {
                params.attributeValueIds = selectedValues.join(',');
            }

            var url = '/api/catalog/products?' + $.param(params);

            $.get(url)
                .done(function (data) {
                    self.result(data);
                    // Update facets from response
                    self.facets(data.facets || []);
                    self.updateUrl();
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

        self.loadCategories = function () {
            $.get('/api/catalog/categories')
                .done(function (data) {
                    // Flat list for dropdown
                    var flat = [];
                    // All categories with path for search
                    var all = [];

                    function flatten(cats, level, parentPath) {
                        cats.forEach(function (cat) {
                            flat.push({ id: cat.id, name: cat.name, level: level });

                            // For search - include full details
                            all.push({
                                id: cat.id,
                                name: cat.name,
                                slug: cat.slug,
                                icon: cat.icon,
                                imageUrl: cat.imageUrl,
                                productCount: cat.productCount,
                                level: level,
                                parentPath: parentPath
                            });

                            if (cat.children && cat.children.length > 0) {
                                var newPath = parentPath ? parentPath + ' > ' + cat.name : cat.name;
                                flatten(cat.children, level + 1, newPath);
                            }
                        });
                    }
                    flatten(data, 0, '');
                    self.flatCategories(flat);
                    self.allCategories(all);
                });
        };

        self.resetFilters = function () {
            self.filter.search('');
            self.filter.categoryId(null);
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

        self.removeFilter = function (key) {
            // Handle attribute filters
            if (key.startsWith('attr_')) {
                var valueId = parseInt(key.replace('attr_', ''));
                var currentValues = self.selectedAttributeValues();
                var newValues = currentValues.filter(function (v) { return v !== valueId; });
                self.selectedAttributeValues(newValues);
                return;
            }

            switch (key) {
                case 'search':
                    self.filter.search('');
                    break;
                case 'inStock':
                    self.filter.inStock(false);
                    break;
                case 'minPrice':
                    self.filter.minPrice(null);
                    break;
                case 'maxPrice':
                    self.filter.maxPrice(null);
                    break;
            }
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

        self.updateUrl = function () {
            var params = new URLSearchParams();
            if (self.filter.search()) params.set('search', self.filter.search());
            if (self.filter.categoryId()) params.set('categoryId', self.filter.categoryId());
            if (self.filter.sortBy() !== 'newest') params.set('sort', self.filter.sortBy());
            if (self.filter.page() > 1) params.set('page', self.filter.page());
            if (self.selectedAttributeValues().length > 0) {
                params.set('attrs', self.selectedAttributeValues().join(','));
            }

            var newUrl = window.location.pathname;
            if (params.toString()) {
                newUrl += '?' + params.toString();
            }
            window.history.replaceState({}, '', newUrl);
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
            self.loadCategories();
            self.loadProducts();

            // Category sidebar click handler
            $(document).on('click', '.category-item a', function (e) {
                e.preventDefault();
                var categoryId = $(this).data('category-id');
                self.filter.categoryId(categoryId);

                // Active class
                $('.category-item a').removeClass('active');
                $(this).addClass('active');
            });
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
            ko.applyBindings(new CatalogViewModel(), app);
        }
    });
})();
