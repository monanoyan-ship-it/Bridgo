function MyDemandsViewModel() {
    var self = this;

    // State
    self.isLoading = ko.observable(false);
    self.isSaving = ko.observable(false);
    self.isDeleting = ko.observable(false);
    self.activeTab = ko.observable('demands');

    // Data
    self.demands = ko.observableArray([]);
    self.inquiries = ko.observableArray([]);
    self.categories = ko.observableArray([]);
    self.countries = ko.observableArray([]);
    self.units = ko.observableArray([]);

    // Counts for tabs
    self.demandCount = ko.observable(0);
    self.inquiryCount = ko.observable(0);
    self.demandResponseCount = ko.observable(0);
    self.inquiryResponseCount = ko.observable(0);

    // Responses for detail modals
    self.demandResponses = ko.observableArray([]);
    self.inquiryResponses = ko.observableArray([]);

    // Selected items
    self.selectedDemand = ko.observable(null);
    self.selectedInquiry = ko.observable(null);
    self.demandToDelete = null;

    // Pagination (for demands)
    self.currentPage = ko.observable(1);
    self.pageSize = ko.observable(10);
    self.totalCount = ko.observable(0);
    self.totalPages = ko.computed(function() {
        return Math.ceil(self.totalCount() / self.pageSize());
    });

    // Demand Filters
    self.demandFilter = {
        search: ko.observable(''),
        status: ko.observable(''),
        categoryId: ko.observable(null)
    };

    // Product Search (for reference product in demand)
    self.productSearch = ko.observable('');
    self.productSearchResults = ko.observableArray([]);
    self.isSearchingProducts = ko.observable(false);
    self.selectedProduct = ko.observable(null);

    // Form
    self.isEditing = ko.observable(false);
    self.editingId = ko.observable(null);
    self.formError = ko.observable('');
    self.form = {
        title: ko.observable(''),
        description: ko.observable(''),
        categoryId: ko.observable(null),
        quantity: ko.observable(null),
        unitId: ko.observable(null),
        budgetMin: ko.observable(null),
        budgetMax: ko.observable(null),
        budgetCurrency: ko.observable('TRY'),
        desiredLeadTimeDays: ko.observable(null),
        expiresAt: ko.observable(null),
        countryId: ko.observable(null),
        city: ko.observable(''),
        tags: ko.observable(''),
        isIndexable: ko.observable(true),
        status: ko.observable(1),
        // Reference Product fields
        hasReferenceProduct: ko.observable(false),
        referenceProductId: ko.observable(null),
        modificationNotes: ko.observable(''),
        modifications: ko.observableArray([])
    };

    // Debounced search
    var searchTimeout;
    self.demandFilter.search.subscribe(function() {
        clearTimeout(searchTimeout);
        searchTimeout = setTimeout(function() {
            self.currentPage(1);
            self.loadDemands();
        }, 300);
    });

    self.demandFilter.status.subscribe(function() {
        self.currentPage(1);
        self.loadDemands();
    });

    self.demandFilter.categoryId.subscribe(function() {
        self.currentPage(1);
        self.loadDemands();
    });

    // Clear reference when hasReferenceProduct is unchecked
    self.form.hasReferenceProduct.subscribe(function(value) {
        if (!value) {
            self.clearReferenceProduct();
        }
    });

    // Visible pages
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

    // ========================================
    // TAB SWITCHING
    // ========================================
    self.setTab = function(tab) {
        self.activeTab(tab);
        self.loadData();
    };

    self.loadData = function() {
        if (self.activeTab() === 'demands') {
            self.loadDemands();
        } else if (self.activeTab() === 'inquiries') {
            self.loadInquiries();
        }
    };

    // ========================================
    // DEMAND STATUS HELPERS
    // ========================================
    self.getDemandStatusClass = function(status) {
        switch (status) {
            case 1: return 'bg-secondary'; // Draft
            case 2: return 'bg-success'; // Active
            case 3: return 'bg-dark'; // Closed
            case 4: return 'bg-primary'; // Awarded
            case 5: return 'bg-warning text-dark'; // Expired
            case 6: return 'bg-danger'; // Cancelled
            default: return 'bg-light text-dark';
        }
    };

    self.getDemandStatusText = function(status) {
        switch (status) {
            case 1: return 'Taslak';
            case 2: return 'Aktif';
            case 3: return 'Kapandi';
            case 4: return 'Teklif Secildi';
            case 5: return 'Suresi Doldu';
            case 6: return 'Iptal';
            default: return 'Bilinmiyor';
        }
    };

    self.getResponseStatusClass = function(status) {
        switch (status) {
            case 1: return 'bg-warning text-dark'; // Pending
            case 2: return 'bg-info'; // Viewed
            case 3: return 'bg-primary'; // Shortlisted
            case 4: return 'bg-success'; // Accepted
            case 5: return 'bg-danger'; // Rejected
            default: return 'bg-light text-dark';
        }
    };

    self.getResponseStatusText = function(status) {
        switch (status) {
            case 1: return 'Beklemede';
            case 2: return 'Goruldu';
            case 3: return 'Degerlendiriliyor';
            case 4: return 'Kabul Edildi';
            case 5: return 'Reddedildi';
            default: return 'Bilinmiyor';
        }
    };

    // ========================================
    // INQUIRY STATUS HELPERS
    // ========================================
    self.getInquiryStatusClass = function(status) {
        switch (status) {
            case 0: return 'bg-warning text-dark'; // Pending
            case 1: return 'bg-info'; // Read
            case 2: return 'bg-primary'; // Responded
            case 3: return 'bg-success'; // Accepted
            case 4: return 'bg-danger'; // Rejected
            case 5: return 'bg-secondary'; // Cancelled
            default: return 'bg-light text-dark';
        }
    };

    self.getInquiryStatusText = function(status) {
        switch (status) {
            case 0: return 'Beklemede';
            case 1: return 'Goruldu';
            case 2: return 'Teklif Geldi';
            case 3: return 'Kabul Edildi';
            case 4: return 'Reddedildi';
            case 5: return 'Iptal';
            default: return 'Bilinmiyor';
        }
    };

    self.getInquiryResponseStatusClass = function(status) {
        switch (status) {
            case 0: return 'bg-warning text-dark'; // Pending
            case 1: return 'bg-success'; // Accepted
            case 2: return 'bg-danger'; // Rejected
            default: return 'bg-light text-dark';
        }
    };

    self.getInquiryResponseStatusText = function(status) {
        switch (status) {
            case 0: return 'Beklemede';
            case 1: return 'Kabul Edildi';
            case 2: return 'Reddedildi';
            default: return 'Bilinmiyor';
        }
    };

    // ========================================
    // LOAD DEMANDS (Tab 1)
    // ========================================
    self.loadDemands = function() {
        self.isLoading(true);

        var params = new URLSearchParams();
        params.append('page', self.currentPage());
        params.append('pageSize', self.pageSize());
        if (self.demandFilter.search()) params.append('search', self.demandFilter.search());
        if (self.demandFilter.status()) params.append('status', self.demandFilter.status());
        if (self.demandFilter.categoryId()) params.append('categoryId', self.demandFilter.categoryId());

        fetch('/api/demands/my?' + params.toString())
            .then(function(r) { return r.json(); })
            .then(function(data) {
                self.demands(data.items || []);
                self.totalCount(data.totalCount || 0);
                self.demandCount(data.totalCount || 0);

                // Calculate total response count for demands
                var totalResponses = 0;
                (data.items || []).forEach(function(d) {
                    totalResponses += d.responseCount || 0;
                });
                self.demandResponseCount(totalResponses);
            })
            .catch(function(err) {
                console.error('Error loading demands:', err);
                self.demands([]);
            })
            .finally(function() {
                self.isLoading(false);
            });
    };

    // ========================================
    // LOAD INQUIRIES (Tab 2)
    // ========================================
    self.loadInquiries = function() {
        self.isLoading(true);

        fetch('/api/product-inquiries/my')
            .then(function(r) { return r.json(); })
            .then(function(data) {
                self.inquiries(data || []);
                self.inquiryCount(data?.length || 0);

                // Calculate total response count for inquiries
                var totalResponses = 0;
                (data || []).forEach(function(i) {
                    totalResponses += i.responseCount || 0;
                });
                self.inquiryResponseCount(totalResponses);
            })
            .catch(function(err) {
                console.error('Error loading inquiries:', err);
                self.inquiries([]);
            })
            .finally(function() {
                self.isLoading(false);
            });
    };

    // ========================================
    // LOAD HELPERS
    // ========================================
    self.loadCategories = function() {
        fetch('/api/catalog/categories')
            .then(function(r) { return r.json(); })
            .then(function(data) {
                // Flatten tree structure for select
                var flat = [];
                function flatten(items, prefix) {
                    items.forEach(function(item) {
                        flat.push({ id: item.id, name: prefix + item.name });
                        if (item.children && item.children.length > 0) {
                            flatten(item.children, prefix + '  ');
                        }
                    });
                }
                flatten(data || [], '');
                self.categories(flat);
            })
            .catch(function(err) {
                console.error('Error loading categories:', err);
            });
    };

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

    self.loadUnits = function() {
        fetch('/api/types/units')
            .then(function(r) { return r.json(); })
            .then(function(data) {
                self.units(data || []);
            })
            .catch(function(err) {
                console.error('Error loading units:', err);
            });
    };

    // ========================================
    // DEMAND DETAIL MODAL
    // ========================================
    self.viewDemandDetail = function(demand) {
        self.selectedDemand(demand);
        self.demandResponses([]);

        // Load responses for this demand
        if (demand.responseCount > 0) {
            fetch('/api/demands/' + demand.id + '/responses')
                .then(function(r) { return r.json(); })
                .then(function(data) {
                    self.demandResponses(data || []);
                })
                .catch(function(err) {
                    console.error('Error loading demand responses:', err);
                });
        }

        var modal = new bootstrap.Modal(document.getElementById('demandDetailModal'));
        modal.show();
    };

    self.acceptDemandResponse = function(response) {
        fetch('/api/demands/responses/' + response.id + '/status', {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ status: 4 }) // Accepted
        })
        .then(function(r) {
            if (r.ok) {
                // Reload responses
                var demandId = self.selectedDemand().id;
                fetch('/api/demands/' + demandId + '/responses')
                    .then(function(r) { return r.json(); })
                    .then(function(data) {
                        self.demandResponses(data || []);
                    });
                self.loadDemands();
            } else {
                return r.json().then(function(err) { throw new Error(err.message || 'Islem basarisiz.'); });
            }
        })
        .catch(function(err) {
            alert(err.message);
        });
    };

    self.rejectDemandResponse = function(response) {
        fetch('/api/demands/responses/' + response.id + '/status', {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ status: 5 }) // Rejected
        })
        .then(function(r) {
            if (r.ok) {
                var demandId = self.selectedDemand().id;
                fetch('/api/demands/' + demandId + '/responses')
                    .then(function(r) { return r.json(); })
                    .then(function(data) {
                        self.demandResponses(data || []);
                    });
                self.loadDemands();
            } else {
                return r.json().then(function(err) { throw new Error(err.message || 'Islem basarisiz.'); });
            }
        })
        .catch(function(err) {
            alert(err.message);
        });
    };

    // ========================================
    // INQUIRY DETAIL MODAL
    // ========================================
    self.viewInquiryDetail = function(inquiry) {
        self.selectedInquiry(inquiry);
        self.inquiryResponses([]);

        // Load responses for this inquiry
        if (inquiry.responseCount > 0) {
            fetch('/api/product-inquiries/' + inquiry.id + '/responses')
                .then(function(r) { return r.json(); })
                .then(function(data) {
                    self.inquiryResponses(data || []);
                })
                .catch(function(err) {
                    console.error('Error loading inquiry responses:', err);
                });
        }

        var modal = new bootstrap.Modal(document.getElementById('inquiryDetailModal'));
        modal.show();
    };

    self.acceptInquiryResponse = function(response) {
        fetch('/api/product-inquiries/responses/' + response.id + '/status', {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ status: 1 }) // Accepted
        })
        .then(function(r) {
            if (r.ok) {
                var inquiryId = self.selectedInquiry().id;
                fetch('/api/product-inquiries/' + inquiryId + '/responses')
                    .then(function(r) { return r.json(); })
                    .then(function(data) {
                        self.inquiryResponses(data || []);
                    });
                self.loadInquiries();
            } else {
                return r.json().then(function(err) { throw new Error(err.message || 'Islem basarisiz.'); });
            }
        })
        .catch(function(err) {
            alert(err.message);
        });
    };

    self.rejectInquiryResponse = function(response) {
        fetch('/api/product-inquiries/responses/' + response.id + '/status', {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ status: 2 }) // Rejected
        })
        .then(function(r) {
            if (r.ok) {
                var inquiryId = self.selectedInquiry().id;
                fetch('/api/product-inquiries/' + inquiryId + '/responses')
                    .then(function(r) { return r.json(); })
                    .then(function(data) {
                        self.inquiryResponses(data || []);
                    });
                self.loadInquiries();
            } else {
                return r.json().then(function(err) { throw new Error(err.message || 'Islem basarisiz.'); });
            }
        })
        .catch(function(err) {
            alert(err.message);
        });
    };

    self.cancelInquiry = function(inquiry) {
        if (!confirm('Bu urun istegini iptal etmek istediginize emin misiniz?')) return;

        fetch('/api/product-inquiries/' + inquiry.id + '/cancel', {
            method: 'PUT'
        })
        .then(function(r) {
            if (r.ok) {
                self.loadInquiries();
            } else {
                return r.json().then(function(err) { throw new Error(err.message || 'Islem basarisiz.'); });
            }
        })
        .catch(function(err) {
            alert(err.message);
        });
    };

    // ========================================
    // PRODUCT SEARCH (for reference product)
    // ========================================
    self.searchProducts = function() {
        var query = self.productSearch();
        if (!query || query.length < 2) {
            self.productSearchResults([]);
            return;
        }

        self.isSearchingProducts(true);

        fetch('/api/products/search?search=' + encodeURIComponent(query) + '&pageSize=10')
            .then(function(r) { return r.json(); })
            .then(function(data) {
                self.productSearchResults(data || []);
            })
            .catch(function(err) {
                console.error('Error searching products:', err);
                self.productSearchResults([]);
            })
            .finally(function() {
                self.isSearchingProducts(false);
            });
    };

    self.selectReferenceProduct = function(product) {
        self.selectedProduct(product);
        self.form.referenceProductId(product.id);
        self.productSearchResults([]);
        self.productSearch('');

        if (!self.form.categoryId() && product.categoryId) {
            self.form.categoryId(product.categoryId);
        }

        if (self.form.modifications().length === 0) {
            self.addModification();
        }
    };

    self.clearReferenceProduct = function() {
        self.selectedProduct(null);
        self.form.referenceProductId(null);
        self.form.modifications([]);
        self.form.modificationNotes('');
        self.productSearchResults([]);
        self.productSearch('');
    };

    self.addModification = function() {
        self.form.modifications.push({
            propertyName: ko.observable(''),
            originalValue: ko.observable(''),
            desiredValue: ko.observable(''),
            notes: ko.observable(''),
            displayOrder: ko.observable(self.form.modifications().length)
        });
    };

    self.removeModification = function(index) {
        self.form.modifications.splice(index, 1);
    };

    // ========================================
    // PAGINATION & FILTERS
    // ========================================
    self.goToPage = function(page) {
        if (page < 1 || page > self.totalPages()) return;
        self.currentPage(page);
        self.loadData();
    };

    self.resetDemandFilters = function() {
        self.demandFilter.search('');
        self.demandFilter.status('');
        self.demandFilter.categoryId(null);
    };

    // ========================================
    // DEMAND CRUD OPERATIONS
    // ========================================
    self.openCreateModal = function() {
        self.resetForm();
        self.isEditing(false);
        self.editingId(null);
        self.formError('');
        var modal = new bootstrap.Modal(document.getElementById('demandModal'));
        modal.show();
    };

    self.resetForm = function() {
        self.form.title('');
        self.form.description('');
        self.form.categoryId(null);
        self.form.quantity(null);
        self.form.unitId(null);
        self.form.budgetMin(null);
        self.form.budgetMax(null);
        self.form.budgetCurrency('TRY');
        self.form.desiredLeadTimeDays(null);
        self.form.expiresAt(null);
        self.form.countryId(null);
        self.form.city('');
        self.form.tags('');
        self.form.isIndexable(true);
        self.form.status(1);
        self.form.hasReferenceProduct(false);
        self.form.referenceProductId(null);
        self.form.modificationNotes('');
        self.form.modifications([]);
        self.selectedProduct(null);
        self.productSearch('');
        self.productSearchResults([]);
    };

    self.editDemand = function(demand) {
        self.isEditing(true);
        self.editingId(demand.id);
        self.formError('');

        self.form.title(demand.title);
        self.form.description(demand.description);
        self.form.categoryId(demand.categoryId);
        self.form.quantity(demand.quantity);
        // Find unitId from unit name
        var matchedUnit = self.units().find(function(u) { return u.name === demand.unit || u.systemName === demand.unit; });
        self.form.unitId(matchedUnit ? matchedUnit.id : null);
        self.form.budgetMin(demand.budgetMin);
        self.form.budgetMax(demand.budgetMax);
        self.form.budgetCurrency(demand.budgetCurrency || 'TRY');
        self.form.desiredLeadTimeDays(demand.desiredLeadTimeDays);
        self.form.expiresAt(demand.expiresAt ? demand.expiresAt.split('T')[0] : null);
        self.form.countryId(demand.countryId);
        self.form.city(demand.city);
        self.form.tags(demand.tags);
        self.form.isIndexable(demand.isIndexable);
        self.form.status(demand.status);

        if (demand.referenceProductId) {
            self.form.hasReferenceProduct(true);
            self.form.referenceProductId(demand.referenceProductId);
            self.form.modificationNotes(demand.modificationNotes || '');
            self.selectedProduct({
                id: demand.referenceProductId,
                name: demand.referenceProductName,
                mainImageUrl: demand.referenceProductImage,
                categoryName: demand.categoryName
            });
            if (demand.modifications && demand.modifications.length > 0) {
                var mods = demand.modifications.map(function(m) {
                    return {
                        propertyName: ko.observable(m.propertyName),
                        originalValue: ko.observable(m.originalValue),
                        desiredValue: ko.observable(m.desiredValue),
                        notes: ko.observable(m.notes),
                        displayOrder: ko.observable(m.displayOrder)
                    };
                });
                self.form.modifications(mods);
            }
        } else {
            self.form.hasReferenceProduct(false);
            self.form.referenceProductId(null);
            self.form.modificationNotes('');
            self.form.modifications([]);
            self.selectedProduct(null);
        }

        var modal = new bootstrap.Modal(document.getElementById('demandModal'));
        modal.show();
    };

    self.deleteDemand = function(demand) {
        self.demandToDelete = demand;
        var modal = new bootstrap.Modal(document.getElementById('deleteModal'));
        modal.show();
    };

    self.confirmDelete = function() {
        if (!self.demandToDelete) return;

        self.isDeleting(true);

        fetch('/api/demands/' + self.demandToDelete.id, {
            method: 'DELETE'
        })
        .then(function(r) {
            if (r.ok) {
                bootstrap.Modal.getInstance(document.getElementById('deleteModal')).hide();
                self.loadDemands();
            } else {
                return r.json().then(function(err) { throw new Error(err.message || 'Silme islemi basarisiz.'); });
            }
        })
        .catch(function(err) {
            alert(err.message);
        })
        .finally(function() {
            self.isDeleting(false);
            self.demandToDelete = null;
        });
    };

    self.saveDraft = function() {
        self.saveDemand(1);
    };

    self.publishDemand = function() {
        self.saveDemand(2);
    };

    self.saveDemand = function(status) {
        self.formError('');

        if (!self.form.title()) {
            self.formError('Baslik zorunludur.');
            return;
        }
        if (!self.form.description()) {
            self.formError('Aciklama zorunludur.');
            return;
        }

        if (self.form.hasReferenceProduct() && self.form.referenceProductId()) {
            var mods = self.form.modifications();
            for (var i = 0; i < mods.length; i++) {
                if (!mods[i].propertyName() || !mods[i].desiredValue()) {
                    self.formError('Tum modifikasyonlarda ozellik ve istenen deger zorunludur.');
                    return;
                }
            }
        }

        self.isSaving(true);

        // Get unit name from selected unit
        var selectedUnit = self.units().find(function(u) { return u.id === self.form.unitId(); });
        var unitName = selectedUnit ? selectedUnit.name : '';

        var data = {
            title: self.form.title(),
            description: self.form.description(),
            categoryId: self.form.categoryId(),
            quantity: self.form.quantity() ? parseInt(self.form.quantity()) : null,
            unit: unitName,
            budgetMin: self.form.budgetMin() ? parseFloat(self.form.budgetMin()) : null,
            budgetMax: self.form.budgetMax() ? parseFloat(self.form.budgetMax()) : null,
            budgetCurrency: self.form.budgetCurrency(),
            desiredLeadTimeDays: self.form.desiredLeadTimeDays() ? parseInt(self.form.desiredLeadTimeDays()) : null,
            expiresAt: self.form.expiresAt() || null,
            countryId: self.form.countryId(),
            city: self.form.city(),
            tags: self.form.tags(),
            isIndexable: self.form.isIndexable(),
            status: status
        };

        if (self.form.hasReferenceProduct() && self.form.referenceProductId()) {
            data.referenceProductId = self.form.referenceProductId();
            data.modificationNotes = self.form.modificationNotes();
            data.modifications = self.form.modifications().map(function(m, index) {
                return {
                    propertyName: m.propertyName(),
                    originalValue: m.originalValue(),
                    desiredValue: m.desiredValue(),
                    notes: m.notes(),
                    displayOrder: index
                };
            });
        }

        var url = '/api/demands';
        var method = 'POST';

        if (self.isEditing() && self.editingId()) {
            url = '/api/demands/' + self.editingId();
            method = 'PUT';
        }

        fetch(url, {
            method: method,
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        })
        .then(function(r) {
            if (r.ok) return r.json();
            return r.json().then(function(err) { throw new Error(err.message || 'Kaydetme islemi basarisiz.'); });
        })
        .then(function(result) {
            bootstrap.Modal.getInstance(document.getElementById('demandModal')).hide();
            self.loadDemands();
        })
        .catch(function(err) {
            self.formError(err.message);
        })
        .finally(function() {
            self.isSaving(false);
        });
    };

    // ========================================
    // INITIALIZE
    // ========================================
    self.loadCategories();
    self.loadCountries();
    self.loadUnits();
    self.loadDemands();

    // Check URL params - open create modal if ?create=true
    var urlParams = new URLSearchParams(window.location.search);
    if (urlParams.get('create') === 'true') {
        // Wait for categories to load, then open modal
        setTimeout(function() {
            self.openCreateModal();
            // Clean URL
            window.history.replaceState({}, document.title, window.location.pathname);
        }, 500);
    }

    // Also load inquiry count for badge
    fetch('/api/product-inquiries/my')
        .then(function(r) { return r.json(); })
        .then(function(data) {
            self.inquiryCount(data?.length || 0);
            var totalResponses = 0;
            (data || []).forEach(function(i) {
                totalResponses += i.responseCount || 0;
            });
            self.inquiryResponseCount(totalResponses);
        })
        .catch(function() {});
}

$(document).ready(function() {
    ko.applyBindings(new MyDemandsViewModel(), document.getElementById('my-demands-app'));
});
