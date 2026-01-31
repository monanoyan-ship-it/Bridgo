function SupplierProfileViewModel() {
    var self = this;

    // State
    self.isLoading = ko.observable(true);
    self.isSaving = ko.observable(false);
    self.loadError = ko.observable('');
    self.successMessage = ko.observable('');

    // Data
    self.profile = ko.observable(null);
    self.categories = ko.observableArray([]);
    self.countries = ko.observableArray([]);
    self.categorySearch = ko.observable('');

    // Form
    self.form = {
        isPublic: ko.observable(false),
        tagline: ko.observable(''),
        description: ko.observable(''),
        countryId: ko.observable(null),
        city: ko.observable(''),
        address: ko.observable(''),
        phone: ko.observable(''),
        email: ko.observable(''),
        website: ko.observable(''),
        foundedYear: ko.observable(null),
        employeeCountRange: ko.observable(''),
        minimumOrderValue: ko.observable(null),
        minimumOrderCurrency: ko.observable('USD'),
        leadTimeDays: ko.observable(null),
        acceptsCustomOrders: ko.observable(false),
        certifications: ko.observable(''),
        capabilities: ko.observable(''),
        exportCountries: ko.observable(''),
        paymentTerms: ko.observable(''),
        categoryIds: ko.observableArray([]),
        galleryImages: ko.observableArray([]),
        slug: ko.observable(''),
        metaTitle: ko.observable(''),
        metaDescription: ko.observable(''),
        isIndexable: ko.observable(true)
    };

    // Computed: Filtered Categories
    self.filteredCategories = ko.computed(function() {
        var search = self.categorySearch().toLowerCase();
        var categories = self.categories();
        if (!search) return categories;
        return categories.filter(function(c) {
            return c.name.toLowerCase().indexOf(search) > -1;
        });
    });

    // Computed: Selected Category Objects
    self.selectedCategoryObjects = ko.computed(function() {
        var ids = self.form.categoryIds();
        var categories = self.categories();
        return categories.filter(function(c) {
            return ids.indexOf(c.id) > -1;
        });
    });

    // Toggle category selection
    self.toggleCategory = function(categoryId) {
        var ids = self.form.categoryIds();
        var index = ids.indexOf(categoryId);
        if (index > -1) {
            self.form.categoryIds.splice(index, 1);
        } else {
            self.form.categoryIds.push(categoryId);
        }
    };

    // Load profile data
    self.loadProfile = function() {
        self.isLoading(true);
        self.loadError('');

        fetch('/api/suppliers/my-profile')
            .then(function(r) {
                if (r.status === 404) return null;
                if (!r.ok) throw new Error('Profil yuklenemedi');
                // Check if response has content before parsing
                return r.text().then(function(text) {
                    return text ? JSON.parse(text) : null;
                });
            })
            .then(function(data) {
                self.profile(data);
                if (data) {
                    self.populateForm(data);
                }
            })
            .catch(function(err) {
                self.loadError(err.message);
            })
            .finally(function() {
                self.isLoading(false);
            });
    };

    // Populate form from profile data
    self.populateForm = function(data) {
        self.form.isPublic(data.isPublic || false);
        self.form.tagline(data.tagline || '');
        self.form.description(data.description || '');
        self.form.countryId(data.countryId || null);
        self.form.city(data.city || '');
        self.form.address(data.address || '');
        self.form.phone(data.phone || '');
        self.form.email(data.email || '');
        self.form.website(data.website || '');
        self.form.foundedYear(data.foundedYear || null);
        self.form.employeeCountRange(data.employeeCountRange || '');
        self.form.minimumOrderValue(data.minimumOrderValue || null);
        self.form.minimumOrderCurrency(data.minimumOrderCurrency || 'USD');
        self.form.leadTimeDays(data.leadTimeDays || null);
        self.form.acceptsCustomOrders(data.acceptsCustomOrders || false);
        self.form.certifications(data.certifications || '');
        self.form.capabilities(data.capabilities || '');
        self.form.exportCountries(data.exportCountries || '');
        self.form.paymentTerms(data.paymentTerms || '');
        self.form.categoryIds(data.categoryIds || []);
        self.form.slug(data.slug || '');
        self.form.metaTitle(data.metaTitle || '');
        self.form.metaDescription(data.metaDescription || '');
        self.form.isIndexable(data.isIndexable !== false);

        // Gallery images
        var images = (data.galleryImages || []).map(function(img) {
            return {
                url: ko.observable(img.url),
                caption: ko.observable(img.caption || '')
            };
        });
        self.form.galleryImages(images);
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

    // Save profile
    self.saveProfile = function() {
        self.isSaving(true);
        self.successMessage('');

        var data = {
            isPublic: self.form.isPublic(),
            isPubliclyVisible: self.form.isPublic(),
            tagline: self.form.tagline(),
            description: self.form.description(),
            countryId: self.form.countryId() ? parseInt(self.form.countryId()) : null,
            city: self.form.city(),
            address: self.form.address(),
            phone: self.form.phone(),
            publicPhone: self.form.phone(),
            email: self.form.email(),
            publicEmail: self.form.email(),
            website: self.form.website(),
            publicWebsite: self.form.website(),
            foundedYear: self.form.foundedYear() ? parseInt(self.form.foundedYear()) : null,
            employeeCountRange: self.form.employeeCountRange(),
            minimumOrderValue: self.form.minimumOrderValue() ? parseFloat(self.form.minimumOrderValue()) : null,
            minimumOrderCurrency: self.form.minimumOrderCurrency(),
            leadTimeDays: self.form.leadTimeDays() ? parseInt(self.form.leadTimeDays()) : null,
            acceptsCustomOrders: self.form.acceptsCustomOrders(),
            certifications: self.form.certifications(),
            capabilities: self.form.capabilities(),
            exportCountries: self.form.exportCountries(),
            paymentTerms: self.form.paymentTerms(),
            categoryIdList: self.form.categoryIds(),
            categoryIds: self.form.categoryIds().join(','),
            slug: self.form.slug(),
            metaTitle: self.form.metaTitle(),
            metaDescription: self.form.metaDescription(),
            isIndexable: self.form.isIndexable(),
            galleryImageList: self.form.galleryImages().map(function(img) {
                return {
                    url: img.url(),
                    caption: img.caption()
                };
            })
        };

        var url = '/api/suppliers/my-profile';
        var method = self.profile() ? 'PUT' : 'POST';

        fetch(url, {
            method: method,
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        })
        .then(function(r) {
            if (r.ok) return r.json();
            return r.json().then(function(err) { throw new Error(err.message || 'Kaydetme basarisiz'); });
        })
        .then(function(result) {
            self.successMessage('Profil basariyla kaydedildi');
            self.loadProfile(); // Reload to get updated data
        })
        .catch(function(err) {
            alert(err.message);
        })
        .finally(function() {
            self.isSaving(false);
        });
    };

    // Upload gallery images
    self.uploadGalleryImages = function(vm, event) {
        var files = event.target.files;
        if (!files || files.length === 0) return;

        for (var i = 0; i < files.length; i++) {
            self.uploadGalleryImage(files[i]);
        }

        // Reset input
        event.target.value = '';
    };

    self.uploadGalleryImage = function(file) {
        var formData = new FormData();
        formData.append('file', file);

        fetch('/api/suppliers/my-profile/gallery', {
            method: 'POST',
            body: formData
        })
        .then(function(r) {
            if (r.ok) return r.json();
            throw new Error('Gorsel yuklenemedi');
        })
        .then(function(result) {
            self.form.galleryImages.push({
                url: ko.observable(result.url),
                caption: ko.observable('')
            });
        })
        .catch(function(err) {
            alert(err.message);
        });
    };

    // Remove gallery image
    self.removeGalleryImage = function(index) {
        self.form.galleryImages.splice(index, 1);
    };

    // =========================================
    // CATEGORY SUBSCRIPTIONS
    // =========================================

    // Subscription State
    self.subscriptions = ko.observableArray([]);
    self.isLoadingSubscriptions = ko.observable(false);
    self.isSavingSubscription = ko.observable(false);
    self.isDeletingSubscription = ko.observable(false);
    self.subscriptionError = ko.observable('');
    self.editingSubscriptionId = ko.observable(null);
    self.subscriptionToDelete = null;

    // Subscription Form
    self.subscriptionForm = {
        categoryId: ko.observable(null),
        categoryName: ko.observable(''),
        notifyByEmail: ko.observable(true),
        notifyInApp: ko.observable(true),
        minQuantity: ko.observable(null),
        keywordFilter: ko.observable(''),
        countryFilter: ko.observable('')
    };

    // Computed: Available categories (not already subscribed)
    self.availableCategories = ko.computed(function() {
        var subscribedIds = self.subscriptions().map(function(s) { return s.categoryId; });
        return self.categories().filter(function(c) {
            return subscribedIds.indexOf(c.id) === -1;
        });
    });

    // Load subscriptions
    self.loadSubscriptions = function() {
        self.isLoadingSubscriptions(true);

        fetch('/api/suppliers/subscriptions')
            .then(function(r) { return r.json(); })
            .then(function(data) {
                self.subscriptions(data || []);
            })
            .catch(function(err) {
                console.error('Error loading subscriptions:', err);
            })
            .finally(function() {
                self.isLoadingSubscriptions(false);
            });
    };

    // Open subscription modal
    self.openSubscriptionModal = function() {
        self.editingSubscriptionId(null);
        self.subscriptionError('');
        self.resetSubscriptionForm();
        var modal = new bootstrap.Modal(document.getElementById('subscriptionModal'));
        modal.show();
    };

    // Reset subscription form
    self.resetSubscriptionForm = function() {
        self.subscriptionForm.categoryId(null);
        self.subscriptionForm.categoryName('');
        self.subscriptionForm.notifyByEmail(true);
        self.subscriptionForm.notifyInApp(true);
        self.subscriptionForm.minQuantity(null);
        self.subscriptionForm.keywordFilter('');
        self.subscriptionForm.countryFilter('');
    };

    // Edit subscription
    self.editSubscription = function(subscription) {
        self.editingSubscriptionId(subscription.id);
        self.subscriptionError('');
        self.subscriptionForm.categoryId(subscription.categoryId);
        self.subscriptionForm.categoryName(subscription.categoryName);
        self.subscriptionForm.notifyByEmail(subscription.notifyByEmail);
        self.subscriptionForm.notifyInApp(subscription.notifyInApp);
        self.subscriptionForm.minQuantity(subscription.minQuantity);
        self.subscriptionForm.keywordFilter(subscription.keywordFilter || '');
        self.subscriptionForm.countryFilter(subscription.countryFilter || '');
        var modal = new bootstrap.Modal(document.getElementById('subscriptionModal'));
        modal.show();
    };

    // Save subscription
    self.saveSubscription = function() {
        self.subscriptionError('');

        var isEditing = !!self.editingSubscriptionId();

        if (!isEditing && !self.subscriptionForm.categoryId()) {
            self.subscriptionError('Kategori secmelisiniz.');
            return;
        }

        self.isSavingSubscription(true);

        var data = {
            categoryId: self.subscriptionForm.categoryId(),
            notifyByEmail: self.subscriptionForm.notifyByEmail(),
            notifyInApp: self.subscriptionForm.notifyInApp(),
            minQuantity: self.subscriptionForm.minQuantity() ? parseInt(self.subscriptionForm.minQuantity()) : null,
            keywordFilter: self.subscriptionForm.keywordFilter(),
            countryFilter: self.subscriptionForm.countryFilter()
        };

        var url = '/api/suppliers/subscriptions';
        var method = 'POST';

        if (isEditing) {
            url = '/api/suppliers/subscriptions/' + self.editingSubscriptionId();
            method = 'PUT';
        }

        fetch(url, {
            method: method,
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        })
        .then(function(r) {
            if (r.ok) return r.json();
            return r.json().then(function(err) { throw new Error(err.message || 'Kaydetme basarisiz'); });
        })
        .then(function(result) {
            bootstrap.Modal.getInstance(document.getElementById('subscriptionModal')).hide();
            self.loadSubscriptions();
        })
        .catch(function(err) {
            self.subscriptionError(err.message);
        })
        .finally(function() {
            self.isSavingSubscription(false);
        });
    };

    // Delete subscription
    self.deleteSubscription = function(subscription) {
        self.subscriptionToDelete = subscription;
        var modal = new bootstrap.Modal(document.getElementById('deleteSubscriptionModal'));
        modal.show();
    };

    // Confirm delete
    self.confirmDeleteSubscription = function() {
        if (!self.subscriptionToDelete) return;

        self.isDeletingSubscription(true);

        fetch('/api/suppliers/subscriptions/' + self.subscriptionToDelete.id, {
            method: 'DELETE'
        })
        .then(function(r) {
            if (r.ok) {
                bootstrap.Modal.getInstance(document.getElementById('deleteSubscriptionModal')).hide();
                self.loadSubscriptions();
            } else {
                return r.json().then(function(err) { throw new Error(err.message || 'Silme basarisiz'); });
            }
        })
        .catch(function(err) {
            alert(err.message);
        })
        .finally(function() {
            self.isDeletingSubscription(false);
            self.subscriptionToDelete = null;
        });
    };

    // Initialize
    self.loadCategories();
    self.loadCountries();
    self.loadProfile();
    self.loadSubscriptions();
}
