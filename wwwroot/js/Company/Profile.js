/// <reference path="../knockout.min.js" />

/**
 * Company Profile ViewModel
 */
function ProfileViewModel() {
    var self = this;

    // Data
    self.vendor = ko.observable({
        companyName: '',
        email: '',
        phone: '',
        website: '',
        taxNumber: '',
        taxOffice: '',
        logoUrl: ''
    });

    // UI States
    self.isLoading = ko.observable(true);
    self.isSaving = ko.observable(false);
    self.errorMessage = ko.observable('');
    self.successMessage = ko.observable('');

    // Load vendor data
    self.load = function () {
        self.isLoading(true);
        self.errorMessage('');
        self.successMessage('');

        fetch('/api/company')
            .then(function (r) {
                if (!r.ok) throw new Error('Veri yuklenemedi');
                return r.json();
            })
            .then(function (data) {
                self.vendor({
                    companyName: data.companyName || '',
                    email: data.email || '',
                    phone: data.phone || '',
                    website: data.website || '',
                    taxNumber: data.taxNumber || '',
                    taxOffice: data.taxOffice || '',
                    logoUrl: data.logoUrl || ''
                });
            })
            .catch(function (error) {
                self.errorMessage(error.message || 'Veri yuklenemedi');
            })
            .finally(function () {
                self.isLoading(false);
            });
    };

    // Save vendor data
    self.save = function () {
        self.isSaving(true);
        self.errorMessage('');
        self.successMessage('');

        fetch('/api/company', {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(self.vendor())
        })
            .then(function (r) {
                if (!r.ok) return r.json().then(function (e) { throw e; });
                return r.json();
            })
            .then(function (data) {
                self.successMessage('Firma bilgileri guncellendi');
            })
            .catch(function (error) {
                self.errorMessage(error.message || 'Kaydetme basarisiz');
            })
            .finally(function () {
                self.isSaving(false);
            });
    };

    // Initialize
    self.init = function () {
        self.load();
    };
}

// Initialize on DOM ready
document.addEventListener('DOMContentLoaded', function () {
    var vm = new ProfileViewModel();
    ko.applyBindings(vm, document.getElementById('profile-app'));
    vm.init();
});
