// Provider/MyProfile.js - Capability Profile Yonetimi
(function () {
    'use strict';

    // Gunlerin label'lari
    var DAY_LABELS = {
        'mon': 'Pzt', 'tue': 'Sal', 'wed': 'Car',
        'thu': 'Per', 'fri': 'Cum', 'sat': 'Cmt', 'sun': 'Paz'
    };
    var DAY_ORDER = ['mon', 'tue', 'wed', 'thu', 'fri', 'sat', 'sun'];

    function createWorkingHourItem(day, hours, isClosed) {
        var parts = (hours || '09:00-18:00').split('-');
        return {
            day: day,
            dayLabel: DAY_LABELS[day] || day,
            openTime: ko.observable(parts[0] || '09:00'),
            closeTime: ko.observable(parts[1] || '18:00'),
            isClosed: ko.observable(isClosed || false)
        };
    }

    function createHighlightItem(icon, value, label) {
        return {
            icon: ko.observable(icon || 'bi-globe'),
            value: ko.observable(value || ''),
            label: ko.observable(label || '')
        };
    }

    function ProviderProfileViewModel() {
        var self = this;

        // State
        self.isLoading = ko.observable(true);
        self.isSaving = ko.observable(false);
        self.loadError = ko.observable('');
        self.successMessage = ko.observable('');

        // Data
        self.capabilities = ko.observableArray([]);
        self.selectedCapabilityId = ko.observable(null);
        self.profile = ko.observable(null);

        // Featured Products
        self.featuredProductSearch = ko.observable('');
        self.featuredProductsLoading = ko.observable(false);
        self.availableProducts = ko.observableArray([]);
        self.allVendorProducts = []; // Cache

        // Form
        self.form = {
            displayName: ko.observable(''),
            tagline: ko.observable(''),
            shortDescription: ko.observable(''),
            description: ko.observable(''),
            services: ko.observable(''),
            certifications: ko.observable(''),
            serviceRegions: ko.observable(''),
            isPubliclyVisible: ko.observable(false),
            acceptingNewRequests: ko.observable(true),
            // Mini Website - Social Media
            socialLinkedin: ko.observable(''),
            socialTwitter: ko.observable(''),
            socialFacebook: ko.observable(''),
            socialInstagram: ko.observable(''),
            socialYoutube: ko.observable(''),
            // Mini Website - Working Hours
            workingHoursList: ko.observableArray([]),
            // Mini Website - Highlights
            highlightsList: ko.observableArray([]),
            // Mini Website - Featured Products
            selectedFeaturedProducts: ko.observableArray([]),
            // Satici (2)
            categoryIds: ko.observable(''),
            productionCapacity: ko.observable(''),
            minimumOrderValue: ko.observable(''),
            leadTime: ko.observable(''),
            // Alici (3)
            industry: ko.observable(''),
            businessType: ko.observable(''),
            preferredCategories: ko.observable(''),
            annualPurchaseVolume: ko.observable(null),
            purchaseVolumeCurrency: ko.observable('USD'),
            // Tasimaci (4)
            fleetInfo: ko.observable(''),
            transportModes: ko.observable(''),
            routes: ko.observable(''),
            // Sigorta (5)
            insuranceTypes: ko.observable(''),
            coverageTypes: ko.observable(''),
            maxCoverageAmount: ko.observable(null),
            maxCoverageCurrency: ko.observable('USD'),
            // Gumruk (6)
            customsServices: ko.observable(''),
            licenseNumbers: ko.observable(''),
            customsOffices: ko.observable(''),
            // Gozetim (7)
            inspectionTypes: ko.observable(''),
            inspectionStandards: ko.observable(''),
            // Yatirimci (8)
            investmentTypes: ko.observable(''),
            investmentFocus: ko.observable(''),
            minInvestmentAmount: ko.observable(null),
            maxInvestmentAmount: ko.observable(null),
            investmentCurrency: ko.observable('USD'),
            interestRateRange: ko.observable('')
        };

        // ============================================
        // DATA LOADING
        // ============================================

        self.loadCapabilities = function () {
            return fetch('/api/capability-profile/my-capabilities')
                .then(function (r) { return r.json(); })
                .then(function (data) {
                    var profileCaps = (data || []).filter(function (c) {
                        return c.id >= 2 && c.id <= 8;
                    });
                    self.capabilities(profileCaps);

                    if (profileCaps.length > 0) {
                        self.selectedCapabilityId(profileCaps[0].id);
                        return self.loadProfile(profileCaps[0].id);
                    }
                })
                .catch(function (err) {
                    console.error('Error loading capabilities:', err);
                    self.loadError('Capability bilgileri yuklenemedi.');
                });
        };

        self.loadProfile = function (capabilityId) {
            return fetch('/api/capability-profile/my-profile/' + capabilityId)
                .then(function (r) {
                    if (r.ok) return r.json();
                    throw new Error('Profil yuklenemedi');
                })
                .then(function (data) {
                    self.profile(data);
                    self.populateForm(data);
                })
                .catch(function (err) {
                    console.error('Error loading profile:', err);
                    self.loadError(err.message);
                });
        };

        self.populateForm = function (data) {
            if (!data) return;

            self.form.displayName(data.displayName || '');
            self.form.tagline(data.tagline || '');
            self.form.shortDescription(data.shortDescription || '');
            self.form.description(data.description || '');
            self.form.services(data.services || '');
            self.form.certifications(data.certifications || '');
            self.form.serviceRegions(data.serviceRegions || '');
            self.form.isPubliclyVisible(data.isPubliclyVisible || false);
            self.form.acceptingNewRequests(data.acceptingNewRequests !== false);

            // ---- Mini Website: Social Media ----
            self.form.socialLinkedin('');
            self.form.socialTwitter('');
            self.form.socialFacebook('');
            self.form.socialInstagram('');
            self.form.socialYoutube('');
            if (data.socialLinkList && data.socialLinkList.length > 0) {
                data.socialLinkList.forEach(function (link) {
                    var platform = (link.platform || '').toLowerCase();
                    if (platform === 'linkedin') self.form.socialLinkedin(link.url || '');
                    else if (platform === 'twitter') self.form.socialTwitter(link.url || '');
                    else if (platform === 'facebook') self.form.socialFacebook(link.url || '');
                    else if (platform === 'instagram') self.form.socialInstagram(link.url || '');
                    else if (platform === 'youtube') self.form.socialYoutube(link.url || '');
                });
            }

            // ---- Mini Website: Working Hours ----
            var hoursMap = {};
            if (data.workingHourList && data.workingHourList.length > 0) {
                data.workingHourList.forEach(function (wh) {
                    hoursMap[wh.day] = wh;
                });
            }
            var hourItems = DAY_ORDER.map(function (day) {
                var existing = hoursMap[day];
                if (existing) {
                    return createWorkingHourItem(day, existing.hours, existing.isClosed);
                }
                return createWorkingHourItem(day, '09:00-18:00', (day === 'sat' || day === 'sun'));
            });
            self.form.workingHoursList(hourItems);

            // ---- Mini Website: Highlights ----
            var highlightItems = [];
            if (data.highlightList && data.highlightList.length > 0) {
                highlightItems = data.highlightList.map(function (h) {
                    return createHighlightItem(h.icon, h.value, h.label);
                });
            }
            self.form.highlightsList(highlightItems);

            // ---- Mini Website: Featured Products ----
            self.form.selectedFeaturedProducts([]);
            if (data.featuredProductIds) {
                var ids = data.featuredProductIds.split(',').map(function (id) { return parseInt(id.trim()); }).filter(function (id) { return !isNaN(id); });
                if (ids.length > 0) {
                    self.loadFeaturedProductDetails(ids);
                }
            }

            // Satici
            self.form.categoryIds(data.categoryIds || '');
            self.form.productionCapacity(data.productionCapacity || '');
            self.form.minimumOrderValue(data.minimumOrderValue || '');
            self.form.leadTime(data.leadTime || '');

            // Alici
            self.form.industry(data.industry || '');
            self.form.businessType(data.businessType || '');
            self.form.preferredCategories(data.preferredCategories || '');
            self.form.annualPurchaseVolume(data.annualPurchaseVolume || null);
            self.form.purchaseVolumeCurrency(data.purchaseVolumeCurrency || 'USD');

            // Tasimaci
            self.form.fleetInfo(data.fleetInfo || '');
            self.form.transportModes(data.transportModes || '');
            self.form.routes(data.routes || '');

            // Sigorta
            self.form.insuranceTypes(data.insuranceTypes || '');
            self.form.coverageTypes(data.coverageTypes || '');
            self.form.maxCoverageAmount(data.maxCoverageAmount || null);
            self.form.maxCoverageCurrency(data.maxCoverageCurrency || 'USD');

            // Gumruk
            self.form.customsServices(data.customsServices || '');
            self.form.licenseNumbers(data.licenseNumbers || '');
            self.form.customsOffices(data.customsOffices || '');

            // Gozetim
            self.form.inspectionTypes(data.inspectionTypes || '');
            self.form.inspectionStandards(data.inspectionStandards || '');

            // Yatirimci
            self.form.investmentTypes(data.investmentTypes || '');
            self.form.investmentFocus(data.investmentFocus || '');
            self.form.minInvestmentAmount(data.minInvestmentAmount || null);
            self.form.maxInvestmentAmount(data.maxInvestmentAmount || null);
            self.form.investmentCurrency(data.investmentCurrency || 'USD');
            self.form.interestRateRange(data.interestRateRange || '');
        };

        // ============================================
        // CAPABILITY SELECTION
        // ============================================

        self.selectCapability = function (capability) {
            if (self.selectedCapabilityId() === capability.id) return;

            self.selectedCapabilityId(capability.id);
            self.isLoading(true);
            self.loadProfile(capability.id)
                .finally(function () {
                    self.isLoading(false);
                });
        };

        // ============================================
        // HIGHLIGHTS MANAGEMENT
        // ============================================

        self.addHighlight = function () {
            if (self.form.highlightsList().length >= 6) return;
            self.form.highlightsList.push(createHighlightItem('bi-globe', '', ''));
        };

        self.removeHighlight = function (item) {
            self.form.highlightsList.remove(item);
        };

        // ============================================
        // FEATURED PRODUCTS MANAGEMENT
        // ============================================

        self.loadFeaturedProductDetails = function (ids) {
            // Urun detaylarini yukle
            fetch('/api/catalog/products?pageSize=100')
                .then(function (r) { return r.json(); })
                .then(function (result) {
                    var products = result.items || result.data || result || [];
                    self.allVendorProducts = products;

                    // Secili urunleri bul
                    var selected = [];
                    ids.forEach(function (id) {
                        var found = products.find(function (p) { return p.id === id; });
                        if (found) {
                            selected.push({ id: found.id, name: found.name, sku: found.sku || '' });
                        }
                    });
                    self.form.selectedFeaturedProducts(selected);
                    self.filterAvailableProducts();
                })
                .catch(function (err) {
                    console.error('Error loading products:', err);
                });
        };

        self.loadAllProducts = function () {
            if (self.allVendorProducts.length > 0) {
                self.filterAvailableProducts();
                return;
            }
            self.featuredProductsLoading(true);
            fetch('/api/catalog/products?pageSize=100')
                .then(function (r) { return r.json(); })
                .then(function (result) {
                    self.allVendorProducts = result.items || result.data || result || [];
                    self.filterAvailableProducts();
                })
                .catch(function (err) {
                    console.error('Error loading products:', err);
                })
                .finally(function () {
                    self.featuredProductsLoading(false);
                });
        };

        self.filterAvailableProducts = function () {
            var search = (self.featuredProductSearch() || '').toLowerCase().trim();
            var selectedIds = self.form.selectedFeaturedProducts().map(function (p) { return p.id; });

            var filtered = self.allVendorProducts.filter(function (p) {
                if (selectedIds.indexOf(p.id) >= 0) return false;
                if (search && (p.name || '').toLowerCase().indexOf(search) < 0 &&
                    (p.sku || '').toLowerCase().indexOf(search) < 0) return false;
                return true;
            });
            self.availableProducts(filtered.slice(0, 20));
        };

        self.addFeaturedProduct = function (product) {
            if (self.form.selectedFeaturedProducts().length >= 8) return;
            self.form.selectedFeaturedProducts.push({
                id: product.id,
                name: product.name,
                sku: product.sku || ''
            });
            self.filterAvailableProducts();
        };

        self.removeFeaturedProduct = function (product) {
            self.form.selectedFeaturedProducts.remove(product);
            self.filterAvailableProducts();
        };

        // Urun arama debounce
        var searchTimeout = null;
        self.featuredProductSearch.subscribe(function () {
            clearTimeout(searchTimeout);
            searchTimeout = setTimeout(function () {
                if (self.allVendorProducts.length === 0) {
                    self.loadAllProducts();
                } else {
                    self.filterAvailableProducts();
                }
            }, 300);
        });

        // ============================================
        // SAVE
        // ============================================

        self.buildSocialLinkList = function () {
            var links = [];
            if (self.form.socialLinkedin()) links.push({ platform: 'linkedin', url: self.form.socialLinkedin() });
            if (self.form.socialTwitter()) links.push({ platform: 'twitter', url: self.form.socialTwitter() });
            if (self.form.socialFacebook()) links.push({ platform: 'facebook', url: self.form.socialFacebook() });
            if (self.form.socialInstagram()) links.push({ platform: 'instagram', url: self.form.socialInstagram() });
            if (self.form.socialYoutube()) links.push({ platform: 'youtube', url: self.form.socialYoutube() });
            return links;
        };

        self.buildWorkingHourList = function () {
            return self.form.workingHoursList().map(function (wh) {
                return {
                    day: wh.day,
                    hours: wh.isClosed() ? null : (wh.openTime() + '-' + wh.closeTime()),
                    isClosed: wh.isClosed()
                };
            });
        };

        self.buildHighlightList = function () {
            return self.form.highlightsList().filter(function (h) {
                return h.value() && h.label();
            }).map(function (h) {
                return {
                    icon: h.icon(),
                    value: h.value(),
                    label: h.label()
                };
            });
        };

        self.saveProfile = function () {
            var capabilityId = self.selectedCapabilityId();
            if (!capabilityId) return;

            self.isSaving(true);
            self.successMessage('');

            var data = {
                displayName: self.form.displayName(),
                tagline: self.form.tagline(),
                shortDescription: self.form.shortDescription(),
                description: self.form.description(),
                services: self.form.services(),
                certifications: self.form.certifications(),
                serviceRegions: self.form.serviceRegions(),
                isPubliclyVisible: self.form.isPubliclyVisible(),
                acceptingNewRequests: self.form.acceptingNewRequests(),
                // Mini Website
                socialLinkList: self.buildSocialLinkList(),
                workingHourList: self.buildWorkingHourList(),
                highlightList: self.buildHighlightList(),
                featuredProductIds: self.form.selectedFeaturedProducts().map(function (p) { return p.id; }).join(','),
                // Satici (2)
                categoryIds: self.form.categoryIds(),
                productionCapacity: self.form.productionCapacity(),
                minimumOrderValue: self.form.minimumOrderValue(),
                leadTime: self.form.leadTime(),
                // Alici (3)
                industry: self.form.industry(),
                businessType: self.form.businessType(),
                preferredCategories: self.form.preferredCategories(),
                annualPurchaseVolume: self.form.annualPurchaseVolume() ? parseFloat(self.form.annualPurchaseVolume()) : null,
                purchaseVolumeCurrency: self.form.purchaseVolumeCurrency(),
                // Tasimaci (4)
                fleetInfo: self.form.fleetInfo(),
                transportModes: self.form.transportModes(),
                routes: self.form.routes(),
                // Sigorta (5)
                insuranceTypes: self.form.insuranceTypes(),
                coverageTypes: self.form.coverageTypes(),
                maxCoverageAmount: self.form.maxCoverageAmount() ? parseFloat(self.form.maxCoverageAmount()) : null,
                maxCoverageCurrency: self.form.maxCoverageCurrency(),
                // Gumruk (6)
                customsServices: self.form.customsServices(),
                licenseNumbers: self.form.licenseNumbers(),
                customsOffices: self.form.customsOffices(),
                // Gozetim (7)
                inspectionTypes: self.form.inspectionTypes(),
                inspectionStandards: self.form.inspectionStandards(),
                // Yatirimci (8)
                investmentTypes: self.form.investmentTypes(),
                investmentFocus: self.form.investmentFocus(),
                minInvestmentAmount: self.form.minInvestmentAmount() ? parseFloat(self.form.minInvestmentAmount()) : null,
                maxInvestmentAmount: self.form.maxInvestmentAmount() ? parseFloat(self.form.maxInvestmentAmount()) : null,
                investmentCurrency: self.form.investmentCurrency(),
                interestRateRange: self.form.interestRateRange()
            };

            fetch('/api/capability-profile/my-profile/' + capabilityId, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(data)
            })
            .then(function (r) {
                if (r.ok) return r.json();
                return r.json().then(function (err) { throw new Error(err.message || 'Kaydetme basarisiz'); });
            })
            .then(function (result) {
                self.profile(result);
                self.populateForm(result);
                if (typeof toastr !== 'undefined') {
                    toastr.success('Profil kaydedildi');
                }
            })
            .catch(function (err) {
                if (typeof toastr !== 'undefined') {
                    toastr.error(err.message);
                } else {
                    alert(err.message);
                }
            })
            .finally(function () {
                self.isSaving(false);
            });
        };

        // ============================================
        // INIT
        // ============================================

        self.init = function () {
            self.loadCapabilities()
                .then(function () {
                    // Satici ise urunleri on-yukle
                    if (self.selectedCapabilityId() === 2) {
                        self.loadAllProducts();
                    }
                })
                .finally(function () {
                    self.isLoading(false);
                });
        };

        self.init();
    }

    // Apply bindings when DOM is ready
    $(function () {
        var appEl = document.getElementById('provider-profile-app');
        if (appEl) {
            ko.applyBindings(new ProviderProfileViewModel(), appEl);
        }
    });
})();
