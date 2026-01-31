// Products/Categories - Kategori Goruntuleyici + Talep Sistemi
(function () {
    'use strict';

    function CategoriesViewModel() {
        var self = this;

        // =====================
        // CATEGORIES STATE
        // =====================
        self.categories = ko.observableArray([]);
        self.isLoading = ko.observable(false);

        // Filters
        self.searchQuery = ko.observable('');
        self.filterLevel = ko.observable('');

        // Stats
        self.stats = ko.observable({
            totalCategories: 0,
            mainCategories: 0,
            subCategories: 0,
            totalProducts: 0
        });

        // =====================
        // REQUESTS STATE
        // =====================
        self.requests = ko.observableArray([]);
        self.isLoadingRequests = ko.observable(false);
        self.isSaving = ko.observable(false);
        self.requestStatusFilter = ko.observable('');

        // New request form
        self.newRequest = {
            requestedName: ko.observable(''),
            description: ko.observable(''),
            suggestedParentCategoryId: ko.observable(null)
        };

        // Similar categories warning
        self.similarCategories = ko.observableArray([]);
        self.parentCategoryOptions = ko.observableArray([]);

        // Selected request for detail
        self.selectedRequest = ko.observable(null);

        // Modal states
        self.showCreateModal = ko.observable(false);
        self.showDetailModalFlag = ko.observable(false);

        // Pending request count
        self.pendingRequestCount = ko.computed(function () {
            return self.requests().filter(function (r) { return r.statusId === 1; }).length;
        });

        // =====================
        // CATEGORIES - Filtered
        // =====================
        self.filteredCategories = ko.computed(function () {
            var query = (self.searchQuery() || '').toLowerCase().trim();
            var level = self.filterLevel();
            var cats = self.categories();

            if (!query && !level) {
                return cats;
            }

            // Filter by level first
            if (level === '0') {
                cats = cats.filter(function (c) { return c.level === 0; });
            }

            // If searching, do recursive search
            if (query) {
                return self.filterBySearch(cats, query);
            }

            return cats;
        });

        // Recursive search filter
        self.filterBySearch = function (categories, query) {
            var results = [];

            categories.forEach(function (cat) {
                var matches = cat.name.toLowerCase().indexOf(query) !== -1;
                var childMatches = [];

                if (cat.children && cat.children.length > 0) {
                    childMatches = self.filterBySearch(cat.children, query);
                }

                if (matches || childMatches.length > 0) {
                    var catCopy = Object.assign({}, cat);
                    catCopy.children = childMatches.length > 0 ? childMatches : cat.children;
                    catCopy.expanded = true;
                    results.push(catCopy);
                }
            });

            return results;
        };

        // =====================
        // CATEGORIES - Load
        // =====================
        self.loadCategories = function () {
            self.isLoading(true);
            fetch('/api/products/categories/tree')
                .then(function (response) { return response.json(); })
                .then(function (data) {
                    self.addExpandedProperty(data, true);
                    self.categories(data);
                    self.calculateStats(data);
                })
                .catch(function (error) {
                    console.error('Error loading categories:', error);
                    toastr.error(T('Categories.Error.Load', 'Kategoriler yuklenemedi'));
                })
                .finally(function () {
                    self.isLoading(false);
                });
        };

        // Load parent categories for dropdown
        self.loadParentCategories = function () {
            fetch('/api/products/categories/select')
                .then(function (response) { return response.json(); })
                .then(function (data) {
                    self.parentCategoryOptions(data);
                })
                .catch(function (error) {
                    console.error('Error loading parent categories:', error);
                });
        };

        // Add expanded property recursively
        self.addExpandedProperty = function (categories, defaultExpanded) {
            categories.forEach(function (cat) {
                cat.expanded = defaultExpanded;
                if (cat.children && cat.children.length > 0) {
                    self.addExpandedProperty(cat.children, false);
                }
            });
        };

        // Calculate statistics
        self.calculateStats = function (categories) {
            var total = 0;
            var main = 0;
            var sub = 0;
            var products = 0;

            var countRecursive = function (cats, level) {
                cats.forEach(function (cat) {
                    total++;
                    products += cat.productCount || 0;
                    if (level === 0) {
                        main++;
                    } else {
                        sub++;
                    }
                    if (cat.children && cat.children.length > 0) {
                        countRecursive(cat.children, level + 1);
                    }
                });
            };

            countRecursive(categories, 0);

            self.stats({
                totalCategories: total,
                mainCategories: main,
                subCategories: sub,
                totalProducts: products
            });
        };

        // Toggle expand/collapse
        self.toggleExpand = function (category) {
            category.expanded = !category.expanded;
            var cats = self.categories();
            self.categories([]);
            self.categories(cats);
        };

        // Expand all
        self.expandAll = function () {
            self.setExpandedRecursive(self.categories(), true);
            var cats = self.categories();
            self.categories([]);
            self.categories(cats);
        };

        // Collapse all
        self.collapseAll = function () {
            self.setExpandedRecursive(self.categories(), false);
            var cats = self.categories();
            self.categories([]);
            self.categories(cats);
        };

        // Set expanded recursively
        self.setExpandedRecursive = function (categories, expanded) {
            categories.forEach(function (cat) {
                cat.expanded = expanded;
                if (cat.children && cat.children.length > 0) {
                    self.setExpandedRecursive(cat.children, expanded);
                }
            });
        };

        // Reset filters
        self.resetFilters = function () {
            self.searchQuery('');
            self.filterLevel('');
        };

        // =====================
        // REQUESTS - Load
        // =====================
        self.loadRequests = function () {
            self.isLoadingRequests(true);
            var url = '/api/category-requests';
            var status = self.requestStatusFilter();
            if (status) {
                url += '?status=' + status;
            }

            fetch(url)
                .then(function (response) { return response.json(); })
                .then(function (data) {
                    var mapped = data.map(function (item) {
                        return self.mapRequest(item);
                    });
                    self.requests(mapped);
                })
                .catch(function (error) {
                    console.error('Error loading requests:', error);
                    toastr.error(T('CategoryRequest.Error.Load', 'Talepler yuklenirken hata olustu'));
                })
                .finally(function () {
                    self.isLoadingRequests(false);
                });
        };

        // Map request from API
        self.mapRequest = function (item) {
            item.formattedDate = new Date(item.createdAt).toLocaleDateString('tr-TR');
            item.formattedReviewDate = item.reviewedAt ? new Date(item.reviewedAt).toLocaleDateString('tr-TR') : null;
            item.statusBadgeClass = item.statusCssClass || 'bg-secondary';
            return item;
        };

        // Status filter change
        self.requestStatusFilter.subscribe(function () {
            self.loadRequests();
        });

        // =====================
        // REQUESTS - Similar Check
        // =====================
        var similarCheckTimeout;
        self.newRequest.requestedName.subscribe(function (name) {
            clearTimeout(similarCheckTimeout);
            if (name && name.length >= 3) {
                similarCheckTimeout = setTimeout(function () {
                    self.checkSimilarCategories(name);
                }, 300);
            } else {
                self.similarCategories([]);
            }
        });

        self.checkSimilarCategories = function (name) {
            fetch('/api/category-requests/check-similar?name=' + encodeURIComponent(name))
                .then(function (response) { return response.json(); })
                .then(function (data) {
                    self.similarCategories(data);
                })
                .catch(function (error) {
                    console.error('Error checking similar:', error);
                });
        };

        // =====================
        // REQUESTS - Create Modal
        // =====================
        self.showRequestModal = function () {
            self.newRequest.requestedName('');
            self.newRequest.description('');
            self.newRequest.suggestedParentCategoryId(null);
            self.similarCategories([]);
            self.loadParentCategories();
            self.showCreateModal(true);
        };

        self.closeRequestModal = function () {
            self.showCreateModal(false);
        };

        // Create request
        self.createRequest = function () {
            var name = self.newRequest.requestedName().trim();
            if (!name) {
                toastr.warning(T('CategoryRequest.Error.NameRequired', 'Kategori adi zorunludur'));
                return;
            }

            self.isSaving(true);
            fetch('/api/category-requests', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    requestedName: name,
                    description: self.newRequest.description(),
                    suggestedParentCategoryId: self.newRequest.suggestedParentCategoryId()
                })
            })
                .then(function (response) {
                    if (!response.ok) {
                        return response.json().then(function (err) { throw new Error(err.message); });
                    }
                    return response.json();
                })
                .then(function (data) {
                    toastr.success(T('CategoryRequest.Success.Created', 'Kategori talebi olusturuldu'));
                    self.closeRequestModal();
                    self.loadRequests();
                    // Switch to requests tab
                    var requestsTab = document.querySelector('[data-bs-target="#tab-requests"]');
                    if (requestsTab) {
                        bootstrap.Tab.getOrCreateInstance(requestsTab).show();
                    }
                })
                .catch(function (error) {
                    toastr.error(error.message || T('CategoryRequest.Error.Create', 'Talep olusturulurken hata olustu'));
                })
                .finally(function () {
                    self.isSaving(false);
                });
        };

        // =====================
        // REQUESTS - Cancel
        // =====================
        self.cancelRequest = function (request) {
            if (typeof showConfirmModal === 'function') {
                showConfirmModal(
                    T('CategoryRequest.Cancel.Title', 'Talep Iptal'),
                    T('CategoryRequest.Cancel.Message', 'Bu talebi iptal etmek istediginizden emin misiniz?'),
                    function () { self.doCancelRequest(request.id); }
                );
            } else if (confirm(T('CategoryRequest.Cancel.Message', 'Bu talebi iptal etmek istediginizden emin misiniz?'))) {
                self.doCancelRequest(request.id);
            }
        };

        self.doCancelRequest = function (id) {
            fetch('/api/category-requests/' + id, { method: 'DELETE' })
                .then(function (response) {
                    if (!response.ok) {
                        return response.json().then(function (err) { throw new Error(err.message); });
                    }
                    return response.json();
                })
                .then(function () {
                    toastr.success(T('CategoryRequest.Success.Cancelled', 'Talep iptal edildi'));
                    self.loadRequests();
                })
                .catch(function (error) {
                    toastr.error(error.message || T('CategoryRequest.Error.Cancel', 'Talep iptal edilirken hata olustu'));
                });
        };

        // =====================
        // REQUESTS - Detail Modal
        // =====================
        self.showDetailModal = function (request) {
            self.selectedRequest(request);
            self.showDetailModalFlag(true);
        };

        self.closeDetailModal = function () {
            self.showDetailModalFlag(false);
            self.selectedRequest(null);
        };

        // =====================
        // INITIALIZE
        // =====================
        self.loadCategories();
        self.loadRequests();
    }

    // Start
    document.addEventListener('DOMContentLoaded', function () {
        ko.applyBindings(new CategoriesViewModel(), document.getElementById('categories-app'));
    });
})();
