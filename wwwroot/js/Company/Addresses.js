/// <reference path="../knockout.min.js" />

/**
 * Company Addresses ViewModel
 */
function AddressesViewModel() {
    var self = this;

    // Data
    self.addresses = ko.observableArray([]);
    self.addressTypes = ko.observableArray([]);
    self.countries = ko.observableArray([]);
    self.states = ko.observableArray([]);

    // UI States
    self.isLoading = ko.observable(true);
    self.isSaving = ko.observable(false);
    self.errorMessage = ko.observable('');
    self.successMessage = ko.observable('');
    self.showModal = ko.observable(false);
    self.isEditing = ko.observable(false);

    // Get default type id
    self.getDefaultTypeId = function () {
        var defaultType = self.addressTypes().find(function (t) { return t.isDefault; });
        return defaultType ? defaultType.id : (self.addressTypes()[0]?.id || 1);
    };

    // Form with observable properties for cascading
    self.form = ko.observable({
        id: null,
        title: '',
        type: null, // Will be set from API (isDefault)
        countryId: ko.observable(null),
        stateId: ko.observable(null),
        city: '',
        district: '',
        addressLine1: '',
        postalCode: '',
        contactName: '',
        contactPhone: '',
        isDefault: false
    });

    // Load address types from API
    self.loadAddressTypes = function () {
        return fetch('/api/types/address')
            .then(function (r) {
                if (!r.ok) throw new Error('Adres tipleri yuklenemedi');
                return r.json();
            })
            .then(function (data) {
                self.addressTypes(data);
            })
            .catch(function (error) {
                console.error('Address types load error:', error);
            });
    };

    // Load countries
    self.loadCountries = function () {
        return fetch('/api/company/geography/countries')
            .then(function (r) {
                if (!r.ok) throw new Error('Ulkeler yuklenemedi');
                return r.json();
            })
            .then(function (data) {
                self.countries(data);
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
                if (!r.ok) throw new Error('Eyaletler yuklenemedi');
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
        var countryId = self.form().countryId();
        self.form().stateId(null);
        self.loadStates(countryId);
    };

    // Get type item by id
    self.getTypeById = function (typeId) {
        return self.addressTypes().find(function (t) { return t.id === typeId; });
    };

    // Get type icon by id
    self.getTypeIcon = function (typeId) {
        var typeItem = self.getTypeById(typeId);
        return typeItem ? typeItem.icon : 'bi-geo-alt';
    };

    // Get type name by id
    self.getTypeName = function (typeId) {
        var typeItem = self.getTypeById(typeId);
        return typeItem ? typeItem.name : '';
    };

    // Load addresses
    self.load = function () {
        self.isLoading(true);
        self.errorMessage('');

        fetch('/api/company/addresses')
            .then(function (r) {
                if (!r.ok) throw new Error('Adresler yuklenemedi');
                return r.json();
            })
            .then(function (data) {
                self.addresses(data.map(function (a) {
                    return {
                        id: a.id,
                        title: a.title,
                        type: a.addressTypeId,
                        typeIcon: self.getTypeIcon(a.addressTypeId),
                        typeName: self.getTypeName(a.addressTypeId),
                        countryId: a.countryId,
                        country: a.country,
                        stateId: a.stateId,
                        state: a.state,
                        city: a.city,
                        district: a.district,
                        addressLine: a.addressLine,
                        postalCode: a.postalCode,
                        contactName: a.contactName,
                        contactPhone: a.contactPhone,
                        isDefault: a.isDefault,
                        fullAddress: a.fullAddress || (a.addressLine + ', ' + a.district + ', ' + a.city)
                    };
                }));
            })
            .catch(function (error) {
                self.errorMessage(error.message);
            })
            .finally(function () {
                self.isLoading(false);
            });
    };

    // Open add modal
    self.openAddModal = function () {
        self.isEditing(false);
        self.states([]);
        self.form({
            id: null,
            title: '',
            type: self.getDefaultTypeId(),
            countryId: ko.observable(null),
            stateId: ko.observable(null),
            city: '',
            district: '',
            addressLine: '',
            postalCode: '',
            contactName: '',
            contactPhone: '',
            isDefault: false
        });
        self.showModal(true);
    };

    // Edit address
    self.editAddress = function (address) {
        self.isEditing(true);

        self.form({
            id: address.id,
            title: address.title,
            type: address.type,
            countryId: ko.observable(address.countryId || null),
            stateId: ko.observable(null),
            city: address.city || '',
            district: address.district || '',
            addressLine: address.addressLine || '',
            postalCode: address.postalCode || '',
            contactName: address.contactName || '',
            contactPhone: address.contactPhone || '',
            isDefault: address.isDefault
        });

        // Load states for this country, then set stateId
        if (address.countryId) {
            self.loadStates(address.countryId).then(function () {
                if (address.stateId) {
                    self.form().stateId(address.stateId);
                }
            });
        }

        self.showModal(true);
    };

    // Close modal
    self.closeModal = function () {
        self.showModal(false);
    };

    // Save address
    self.saveAddress = function () {
        // Validation
        var form = self.form();
        if (!form.title || !form.countryId() || !form.city || !form.district || !form.addressLine || !form.addressLine.trim()) {
            self.errorMessage('Lutfen zorunlu alanlari doldurun');
            return;
        }

        // State required if states exist
        if (self.states().length > 0 && !form.stateId()) {
            self.errorMessage('Lutfen eyalet secin');
            return;
        }

        self.isSaving(true);
        self.errorMessage('');
        self.successMessage('');

        var method = self.isEditing() ? 'PUT' : 'POST';
        var url = self.isEditing()
            ? '/api/company/addresses/' + form.id
            : '/api/company/addresses';

        fetch(url, {
            method: method,
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                title: form.title,
                addressTypeId: form.type,
                countryId: form.countryId(),
                stateId: form.stateId(),
                city: form.city,
                district: form.district,
                addressLine: form.addressLine,
                postalCode: form.postalCode,
                contactName: form.contactName,
                contactPhone: form.contactPhone,
                isDefault: form.isDefault
            })
        })
            .then(function (r) {
                if (!r.ok) return r.json().then(function (e) { throw e; });
                return r.json();
            })
            .then(function (data) {
                self.successMessage(self.isEditing() ? 'Adres guncellendi' : 'Adres eklendi');
                self.closeModal();
                self.load();
            })
            .catch(function (error) {
                self.errorMessage(error.message || 'Kaydetme basarisiz');
            })
            .finally(function () {
                self.isSaving(false);
            });
    };

    // Delete address
    self.deleteAddress = function (address) {
        showConfirmModal({
            title: 'Adres Sil',
            message: 'Bu adresi silmek istediginizden emin misiniz?',
            confirmText: 'Sil',
            type: 'danger',
            onConfirm: function () {
                self.isSaving(true);
                self.errorMessage('');

                fetch('/api/company/addresses/' + address.id, {
                    method: 'DELETE'
                })
                    .then(function (r) {
                        if (!r.ok) return r.json().then(function (e) { throw e; });
                        return r.json();
                    })
                    .then(function (data) {
                        self.successMessage('Adres silindi');
                        self.load();
                    })
                    .catch(function (error) {
                        self.errorMessage(error.message || 'Silme basarisiz');
                    })
                    .finally(function () {
                        self.isSaving(false);
                    });
            }
        });
    };

    // Initialize
    self.init = function () {
        // Load types and countries first, then load addresses
        Promise.all([
            self.loadAddressTypes(),
            self.loadCountries()
        ]).then(function () {
            self.load();
        });
    };
}

// Initialize on DOM ready
document.addEventListener('DOMContentLoaded', function () {
    var vm = new AddressesViewModel();
    ko.applyBindings(vm, document.getElementById('addresses-app'));
    vm.init();
});
