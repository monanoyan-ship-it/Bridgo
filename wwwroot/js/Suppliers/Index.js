function SuppliersViewModel() {
    var self = this;

    // State
    self.isLoading = ko.observable(false);
    self.suppliers = ko.observableArray([]);
    self.categories = ko.observableArray([]);
    self.countries = ko.observableArray([]);

    // Pagination
    self.currentPage = ko.observable(1);
    self.pageSize = ko.observable(12);
    self.totalCount = ko.observable(0);
    self.totalPages = ko.computed(function() {
        return Math.ceil(self.totalCount() / self.pageSize());
    });

    // Filters
    self.filter = {
        search: ko.observable(''),
        categoryId: ko.observable(null),
        countryId: ko.observable(null),
        sortBy: ko.observable('')
    };

    // Debounced search
    var searchTimeout;
    self.filter.search.subscribe(function() {
        clearTimeout(searchTimeout);
        searchTimeout = setTimeout(function() {
            self.currentPage(1);
            self.loadSuppliers();
        }, 300);
    });

    // Filter subscriptions
    self.filter.categoryId.subscribe(function() {
        self.currentPage(1);
        self.loadSuppliers();
    });
    self.filter.countryId.subscribe(function() {
        self.currentPage(1);
        self.loadSuppliers();
    });
    self.filter.sortBy.subscribe(function() {
        self.currentPage(1);
        self.loadSuppliers();
    });

    // Visible pages for pagination
    self.visiblePages = ko.computed(function() {
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

    // Load suppliers
    self.loadSuppliers = function() {
        self.isLoading(true);

        var params = new URLSearchParams();
        params.append('page', self.currentPage());
        params.append('pageSize', self.pageSize());

        if (self.filter.search()) params.append('search', self.filter.search());
        if (self.filter.categoryId()) params.append('categoryId', self.filter.categoryId());
        if (self.filter.countryId()) params.append('countryId', self.filter.countryId());
        if (self.filter.sortBy()) params.append('sortBy', self.filter.sortBy());

        fetch('/api/suppliers/public?' + params.toString())
            .then(function(r) { return r.json(); })
            .then(function(data) {
                // Process category names
                var items = data.items || [];
                items.forEach(function(item) {
                    if (item.categoryIds) {
                        // Convert comma-separated category IDs to names
                        var catIds = item.categoryIds.split(',').map(function(id) { return parseInt(id.trim()); });
                        item.categoryNames = self.categories().filter(function(c) {
                            return catIds.indexOf(c.id) !== -1;
                        }).map(function(c) { return c.name; });
                    } else {
                        item.categoryNames = [];
                    }
                });
                self.suppliers(items);
                self.totalCount(data.totalCount || 0);
            })
            .catch(function(err) {
                console.error('Error loading suppliers:', err);
                self.suppliers([]);
            })
            .finally(function() {
                self.isLoading(false);
            });
    };

    // Load categories
    self.loadCategories = function() {
        fetch('/api/products/categories/select')
            .then(function(r) { return r.json(); })
            .then(function(data) {
                self.categories(data || []);
            })
            .catch(function(err) {
                console.error('Error loading categories:', err);
            });
    };

    // Load countries
    self.loadCountries = function() {
        fetch('/api/localization/countries')
            .then(function(r) { return r.json(); })
            .then(function(data) {
                self.countries(data || []);
            })
            .catch(function(err) {
                console.error('Error loading countries:', err);
            });
    };

    // Pagination
    self.goToPage = function(page) {
        if (page < 1 || page > self.totalPages()) return;
        self.currentPage(page);
        self.loadSuppliers();
        window.scrollTo({ top: 0, behavior: 'smooth' });
    };

    // Reset filters
    self.resetFilters = function() {
        self.filter.search('');
        self.filter.categoryId(null);
        self.filter.countryId(null);
        self.filter.sortBy('');
    };

    // Initialize
    self.loadCategories();
    self.loadCountries();
    self.loadSuppliers();
}

$(document).ready(function() {
    ko.applyBindings(new SuppliersViewModel(), document.getElementById('suppliers-app'));
});
