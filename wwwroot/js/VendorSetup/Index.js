// VendorSetup ViewModel
(function () {
    'use strict';

    function VendorSetupViewModel() {
        var self = this;

        // State
        self.isLoading = ko.observable(true);
        self.errorMessage = ko.observable('');
        self.activeTab = ko.observable('search');

        // Domain Match & Pending Request
        self.hasDomainMatch = ko.observable(false);
        self.matchedVendor = ko.observable(null);
        self.hasPendingRequest = ko.observable(false);
        self.pendingRequest = ko.observable(null);
        self.isCorporateEmail = ko.observable(false);

        // Search
        self.searchTerm = ko.observable('');
        self.searchResults = ko.observableArray([]);
        self.isSearching = ko.observable(false);
        self.searchTimeout = null;

        // Join Modal
        self.showJoinModal = ko.observable(false);
        self.selectedVendorToJoin = ko.observable(null);
        self.joinMessage = ko.observable('');

        // Geography data
        self.countries = ko.observableArray([]);
        self.states = ko.observableArray([]);

        // Create Vendor Form
        self.isSaving = ko.observable(false);
        self.newVendor = ko.observable({
            companyName: ko.observable(''),
            email: ko.observable(''),
            phone: ko.observable(''),
            addressTitle: ko.observable('Merkez'),
            countryId: ko.observable(null),
            stateId: ko.observable(null),
            city: ko.observable(''),
            district: ko.observable(''),
            addressLine: ko.observable(''),
            postalCode: ko.observable(''),
            addressType: ko.observable(null) // Will be set from API (isDefault)
        });

        // Load countries
        self.loadCountries = function () {
            return fetch('/api/company/geography/countries')
                .then(function (r) {
                    if (!r.ok) throw new Error('Ulkeler yuklenemedi');
                    return r.json();
                })
                .then(function (data) {
                    self.countries(data);
                    // Default olarak Turkiye secili gelsin (varsa)
                    var turkey = data.find(function (c) { return c.name === 'Türkiye' || c.name === 'Turkey'; });
                    if (turkey) {
                        self.newVendor().countryId(turkey.id);
                        self.loadStates(turkey.id);
                    }
                })
                .catch(function (error) {
                    console.error('Countries load error:', error);
                });
        };

        // Load states for selected country
        self.loadStates = function (countryId) {
            if (!countryId) {
                self.states([]);
                return Promise.resolve();
            }

            return fetch('/api/company/geography/countries/' + countryId + '/states')
                .then(function (r) {
                    if (!r.ok) throw new Error('Iller yuklenemedi');
                    return r.json();
                })
                .then(function (data) {
                    self.states(data);
                })
                .catch(function (error) {
                    console.error('States load error:', error);
                    self.states([]);
                });
        };

        // Country change handler
        self.onCountryChange = function () {
            var countryId = self.newVendor().countryId();
            self.newVendor().stateId(null);
            self.loadStates(countryId);
        };

        // Load address types
        self.addressTypes = ko.observableArray([]);
        self.loadAddressTypes = function () {
            return fetch('/api/types/address')
                .then(function (r) {
                    if (!r.ok) throw new Error('Adres tipleri yuklenemedi');
                    return r.json();
                })
                .then(function (data) {
                    self.addressTypes(data);
                    // Set default address type
                    var defaultType = data.find(function (t) { return t.isDefault; });
                    if (defaultType) {
                        self.newVendor().addressType(defaultType.id);
                    }
                })
                .catch(function (error) {
                    console.error('Address types load error:', error);
                });
        };

        // Initialize
        self.init = function () {
            // Load data in parallel
            Promise.all([
                self.loadCountries(),
                self.loadAddressTypes()
            ]).then(function () {
                self.checkStatus();
            });
        };

        // Check user status (domain match, pending request)
        self.checkStatus = function () {
            self.isLoading(true);

            fetch('/api/VendorSetupApi/status')
                .then(function (response) {
                    if (!response.ok) {
                        if (response.status === 401) {
                            window.location.href = '/Account/Login';
                            return;
                        }
                        throw new Error('Durum kontrol edilemedi');
                    }
                    return response.json();
                })
                .then(function (data) {
                    if (data.hasVendor) {
                        // Zaten vendor'a bagli, ana sayfaya yonlendir
                        window.location.href = data.redirectUrl || '/';
                        return;
                    }

                    self.isCorporateEmail(data.isCorporateEmail || false);

                    if (data.hasPendingRequest) {
                        self.hasPendingRequest(true);
                        self.pendingRequest(data.pendingRequest);
                    } else if (data.hasMatch && data.matchedVendor) {
                        self.hasDomainMatch(true);
                        self.matchedVendor(data.matchedVendor);
                    }

                    self.isLoading(false);
                })
                .catch(function (error) {
                    self.errorMessage(error.message || 'Bir hata olustu');
                    self.isLoading(false);
                });
        };

        // Search vendors
        self.searchVendors = function () {
            var term = self.searchTerm().trim();
            if (term.length < 2) {
                self.searchResults([]);
                return;
            }

            self.isSearching(true);

            fetch('/api/VendorSetupApi/search?q=' + encodeURIComponent(term))
                .then(function (response) {
                    if (!response.ok) throw new Error('Arama yapilamadi');
                    return response.json();
                })
                .then(function (data) {
                    self.searchResults(data);
                    self.isSearching(false);
                })
                .catch(function (error) {
                    self.errorMessage(error.message || 'Arama sirasinda hata olustu');
                    self.isSearching(false);
                });
        };

        // Search on keyup with debounce
        self.onSearchKeyUp = function (data, event) {
            if (self.searchTimeout) {
                clearTimeout(self.searchTimeout);
            }

            self.searchTimeout = setTimeout(function () {
                if (self.searchTerm().trim().length >= 2) {
                    self.searchVendors();
                } else {
                    self.searchResults([]);
                }
            }, 300);

            return true;
        };

        // Select vendor to join
        self.selectVendorToJoin = function (vendor) {
            self.selectedVendorToJoin(vendor);
            self.joinMessage('');
            self.showJoinModal(true);
        };

        // Join matched vendor (from domain match)
        self.joinMatchedVendor = function () {
            var vendor = self.matchedVendor();
            if (vendor) {
                self.selectVendorToJoin(vendor);
            }
        };

        // Close join modal
        self.closeJoinModal = function () {
            self.showJoinModal(false);
            self.selectedVendorToJoin(null);
            self.joinMessage('');
        };

        // Submit join request
        self.submitJoinRequest = function () {
            var vendor = self.selectedVendorToJoin();
            if (!vendor) return;

            self.isSaving(true);

            var endpoint = self.hasDomainMatch() && vendor.id === self.matchedVendor()?.id
                ? '/api/VendorSetupApi/join-by-domain'
                : '/api/VendorSetupApi/join';

            fetch(endpoint, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    vendorId: vendor.id,
                    message: self.joinMessage()
                })
            })
                .then(function (response) {
                    if (!response.ok) {
                        return response.json().then(function (data) {
                            throw new Error(data.message || 'Istek gonderilemedi');
                        });
                    }
                    return response.json();
                })
                .then(function (data) {
                    self.closeJoinModal();
                    self.hasPendingRequest(true);
                    self.pendingRequest(data);
                    self.hasDomainMatch(false);
                    self.matchedVendor(null);
                    toastr.success('Katilma isteginiz gonderildi. Firma yetkililerinin onayini bekleyin.');
                    self.isSaving(false);
                })
                .catch(function (error) {
                    toastr.error(error.message || 'Istek gonderilemedi');
                    self.isSaving(false);
                });
        };

        // Cancel pending request
        self.cancelPendingRequest = function () {
            var request = self.pendingRequest();
            if (!request) return;

            if (!confirm('Katilma isteginizi iptal etmek istediginize emin misiniz?')) {
                return;
            }

            fetch('/api/VendorSetupApi/join/' + request.id, {
                method: 'DELETE'
            })
                .then(function (response) {
                    if (!response.ok) throw new Error('Istek iptal edilemedi');
                    return response.json();
                })
                .then(function () {
                    self.hasPendingRequest(false);
                    self.pendingRequest(null);
                    toastr.success('Katilma isteginiz iptal edildi');
                    // Re-check status (maybe domain match exists)
                    self.checkStatus();
                })
                .catch(function (error) {
                    toastr.error(error.message || 'Istek iptal edilemedi');
                });
        };

        // Create new vendor
        self.createVendor = function () {
            var vendor = self.newVendor();

            // Validation
            if (!vendor.companyName() || !vendor.email() || !vendor.phone()) {
                toastr.warning('Lutfen zorunlu alanlari doldurun');
                return;
            }

            if (!vendor.countryId() || !vendor.city() || !vendor.district() || !vendor.addressLine()) {
                toastr.warning('Lutfen adres bilgilerini doldurun');
                return;
            }

            // State required if states exist for this country
            if (self.states().length > 0 && !vendor.stateId()) {
                toastr.warning('Lutfen eyalet secin');
                return;
            }

            self.isSaving(true);

            var payload = {
                companyName: vendor.companyName(),
                email: vendor.email(),
                phone: vendor.phone(),
                addressTitle: vendor.addressTitle(),
                countryId: vendor.countryId(),
                stateId: vendor.stateId(),
                city: vendor.city(),
                district: vendor.district(),
                addressLine: vendor.addressLine(),
                postalCode: vendor.postalCode(),
                addressTypeId: vendor.addressType()
            };

            fetch('/api/VendorSetupApi/create', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            })
                .then(function (response) {
                    if (!response.ok) {
                        return response.json().then(function (data) {
                            throw new Error(data.message || 'Firma olusturulamadi');
                        });
                    }
                    return response.json();
                })
                .then(function (data) {
                    toastr.success('Firma basariyla olusturuldu: ' + data.companyName);
                    // Ana sayfaya yonlendir
                    setTimeout(function () {
                        window.location.href = '/';
                    }, 1500);
                })
                .catch(function (error) {
                    toastr.error(error.message || 'Firma olusturulamadi');
                    self.isSaving(false);
                });
        };

        // Init
        self.init();
    }

    // Apply bindings when DOM ready
    document.addEventListener('DOMContentLoaded', function () {
        var appElement = document.getElementById('vendor-setup-app');
        if (appElement) {
            ko.applyBindings(new VendorSetupViewModel(), appElement);
        }
    });
})();
