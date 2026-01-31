// Admin/Geography.js - Cografi Veriler Yonetimi
(function () {
    'use strict';

    function GeographyViewModel() {
        var self = this;

        // Data
        self.stats = ko.observable({});
        self.countries = ko.observableArray([]);
        self.states = ko.observableArray([]);

        // Filters
        self.countrySearch = ko.observable('');
        self.countryStatusFilter = ko.observable('');
        self.stateSearch = ko.observable('');
        self.selectedCountryId = ko.observable(null);

        // Edit state - Country
        self.editingCountry = ko.observable({});
        self.editingTranslations = ko.observableArray([]);
        self.saving = ko.observable(false);

        // Edit state - State
        self.editingState = ko.observable({});
        self.editingStateTranslations = ko.observableArray([]);
        self.savingState = ko.observable(false);

        // Modals
        self.countryModal = null;
        self.stateModal = null;

        // Computed: Countries with states (for dropdown)
        self.countriesWithStates = ko.computed(function () {
            return self.countries().filter(function (c) {
                return c.stateCount > 0;
            });
        });

        // Computed: Filtered countries
        self.filteredCountries = ko.computed(function () {
            var search = (self.countrySearch() || '').toLowerCase();
            var statusFilter = self.countryStatusFilter();
            var result = self.countries();

            // Status filter
            if (statusFilter === 'active') {
                result = result.filter(function (c) { return c.isActive; });
            } else if (statusFilter === 'inactive') {
                result = result.filter(function (c) { return !c.isActive; });
            }

            // Search filter
            if (search) {
                result = result.filter(function (c) {
                    return c.name.toLowerCase().indexOf(search) >= 0 ||
                        (c.officialName && c.officialName.toLowerCase().indexOf(search) >= 0) ||
                        c.iso2Code.toLowerCase().indexOf(search) >= 0 ||
                        c.iso3Code.toLowerCase().indexOf(search) >= 0;
                });
            }

            return result;
        });

        // Computed: Filtered states
        self.filteredStates = ko.computed(function () {
            var countryId = self.selectedCountryId();
            var search = (self.stateSearch() || '').toLowerCase();

            var filtered = self.states();

            if (countryId) {
                filtered = filtered.filter(function (s) {
                    return s.countryId === countryId;
                });
            }

            if (search) {
                filtered = filtered.filter(function (s) {
                    return s.name.toLowerCase().indexOf(search) >= 0 ||
                        (s.code && s.code.toLowerCase().indexOf(search) >= 0);
                });
            }

            return filtered;
        });

        // Select country (from countries table click)
        self.selectCountry = function (country) {
            if (country.stateCount > 0) {
                self.selectedCountryId(country.id);
                // Switch to states tab
                var statesTab = document.querySelector('[data-bs-target="#statesTab"]');
                if (statesTab) {
                    var tab = new bootstrap.Tab(statesTab);
                    tab.show();
                }
            }
        };

        // Toggle country active status
        self.toggleCountryActive = function (country) {
            $.post('/api/admin/countries/' + country.id + '/toggle-active')
                .done(function (response) {
                    toastr.success(response.message || 'Durum guncellendi');
                    self.loadCountries();
                })
                .fail(function (xhr) {
                    var msg = xhr.responseJSON?.message || 'Islem basarisiz';
                    toastr.error(msg);
                });
        };

        // Edit country translations
        self.editCountry = function (country) {
            $.get('/api/admin/countries/' + country.id + '/detail')
                .done(function (data) {
                    self.editingCountry(data);
                    // Make translations observable
                    var translations = (data.translations || []).map(function (t) {
                        return {
                            languageCode: t.languageCode,
                            languageName: t.languageName,
                            name: ko.observable(t.name),
                            officialName: ko.observable(t.officialName || '')
                        };
                    });
                    self.editingTranslations(translations);
                    self.countryModal.show();
                })
                .fail(function () {
                    toastr.error('Ulke bilgileri yuklenemedi');
                });
        };

        // Save translations
        self.saveTranslations = function () {
            var countryId = self.editingCountry().id;
            var translations = self.editingTranslations().map(function (t) {
                return {
                    languageCode: t.languageCode,
                    name: t.name(),
                    officialName: t.officialName() || null
                };
            });

            self.saving(true);
            $.ajax({
                url: '/api/admin/countries/' + countryId + '/translations',
                method: 'PUT',
                contentType: 'application/json',
                data: JSON.stringify(translations)
            })
                .done(function (response) {
                    toastr.success(response.message || 'Ceviriler kaydedildi');
                    self.countryModal.hide();
                    self.loadStats();
                })
                .fail(function (xhr) {
                    var msg = xhr.responseJSON?.message || 'Kayit sirasinda hata olustu';
                    toastr.error(msg);
                })
                .always(function () {
                    self.saving(false);
                });
        };

        // Toggle state active status
        self.toggleStateActive = function (state) {
            $.post('/api/admin/states/' + state.id + '/toggle-active')
                .done(function (response) {
                    toastr.success(response.message || 'Durum guncellendi');
                    self.loadStates();
                })
                .fail(function (xhr) {
                    var msg = xhr.responseJSON?.message || 'Islem basarisiz';
                    toastr.error(msg);
                });
        };

        // Edit state translations
        self.editState = function (state) {
            $.get('/api/admin/states/' + state.id + '/detail')
                .done(function (data) {
                    self.editingState(data);
                    var translations = (data.translations || []).map(function (t) {
                        return {
                            languageCode: t.languageCode,
                            languageName: t.languageName,
                            name: ko.observable(t.name)
                        };
                    });
                    self.editingStateTranslations(translations);
                    self.stateModal.show();
                })
                .fail(function () {
                    toastr.error('Eyalet bilgileri yuklenemedi');
                });
        };

        // Save state translations
        self.saveStateTranslations = function () {
            var stateId = self.editingState().id;
            var translations = self.editingStateTranslations().map(function (t) {
                return {
                    languageCode: t.languageCode,
                    name: t.name()
                };
            });

            self.savingState(true);
            $.ajax({
                url: '/api/admin/states/' + stateId + '/translations',
                method: 'PUT',
                contentType: 'application/json',
                data: JSON.stringify(translations)
            })
                .done(function (response) {
                    toastr.success(response.message || 'Ceviriler kaydedildi');
                    self.stateModal.hide();
                })
                .fail(function (xhr) {
                    var msg = xhr.responseJSON?.message || 'Kayit sirasinda hata olustu';
                    toastr.error(msg);
                })
                .always(function () {
                    self.savingState(false);
                });
        };

        // Load stats
        self.loadStats = function () {
            $.get('/api/admin/geography/stats')
                .done(function (data) {
                    self.stats(data);
                });
        };

        // Load countries
        self.loadCountries = function () {
            $.get('/api/admin/countries')
                .done(function (data) {
                    self.countries(data || []);
                });
        };

        // Load states
        self.loadStates = function () {
            $.get('/api/admin/states')
                .done(function (data) {
                    self.states(data || []);
                });
        };

        // Init
        self.init = function () {
            self.loadStats();
            self.loadCountries();
            self.loadStates();

            // Init modals
            var countryModalEl = document.getElementById('countryModal');
            if (countryModalEl) {
                self.countryModal = new bootstrap.Modal(countryModalEl);
            }

            var stateModalEl = document.getElementById('stateModal');
            if (stateModalEl) {
                self.stateModal = new bootstrap.Modal(stateModalEl);
            }
        };

        self.init();
    }

    // Apply bindings
    $(function () {
        ko.applyBindings(new GeographyViewModel(), document.getElementById('geography-app'));
    });
})();
