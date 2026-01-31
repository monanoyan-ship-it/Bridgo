// Provider/MyProfile.js - Capability Profile Yonetimi
(function () {
    'use strict';

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
                    // Profil olusturulabilecek capability'ler: Satici(2), Alici(3), Tasimaci(4), Sigorta(5), Gumruk(6), Gozetim(7), Yatirimci(8)
                    var profileCaps = (data || []).filter(function (c) {
                        return c.id >= 2 && c.id <= 8;
                    });
                    self.capabilities(profileCaps);

                    // Ilk capability'yi sec
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
        // SAVE
        // ============================================

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
                self.successMessage('Profil basariyla kaydedildi');
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
