/// <reference path="../knockout.min.js" />

/**
 * Invitation Accept ViewModel
 * Davet kabul ve kayit formu
 */
function AcceptInvitationViewModel() {
    var self = this;

    // Form fields
    self.firstName = ko.observable('');
    self.lastName = ko.observable('');
    self.password = ko.observable('');
    self.confirmPassword = ko.observable('');
    self.showPassword = ko.observable(false);

    // UI states
    self.isLoading = ko.observable(false);
    self.isSuccess = ko.observable(false);
    self.errorMessage = ko.observable('');

    // Toggle password visibility
    self.togglePassword = function () {
        self.showPassword(!self.showPassword());
    };

    // Validate form
    self.validate = function () {
        if (!self.firstName().trim()) {
            self.errorMessage('Ad zorunludur');
            return false;
        }
        if (!self.lastName().trim()) {
            self.errorMessage('Soyad zorunludur');
            return false;
        }
        if (self.password().length < 6) {
            self.errorMessage('Sifre en az 6 karakter olmali');
            return false;
        }
        if (!/[A-Z]/.test(self.password())) {
            self.errorMessage('Sifre en az bir buyuk harf icermeli');
            return false;
        }
        if (!/[a-z]/.test(self.password())) {
            self.errorMessage('Sifre en az bir kucuk harf icermeli');
            return false;
        }
        if (!/[0-9]/.test(self.password())) {
            self.errorMessage('Sifre en az bir rakam icermeli');
            return false;
        }
        if (self.password() !== self.confirmPassword()) {
            self.errorMessage('Sifreler eslesmeli');
            return false;
        }
        return true;
    };

    // Submit registration
    self.register = function () {
        self.errorMessage('');

        if (!self.validate()) {
            return;
        }

        self.isLoading(true);

        fetch('/api/TeamApi/invitations/accept', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                token: invitationToken,
                firstName: self.firstName().trim(),
                lastName: self.lastName().trim(),
                password: self.password(),
                confirmPassword: self.confirmPassword()
            })
        })
            .then(function (response) {
                if (!response.ok) {
                    return response.json().then(function (data) {
                        throw new Error(data.message || 'Kayit basarisiz');
                    });
                }
                return response.json();
            })
            .then(function (data) {
                self.isSuccess(true);
            })
            .catch(function (error) {
                self.errorMessage(error.message || 'Bir hata olustu');
            })
            .finally(function () {
                self.isLoading(false);
            });
    };
}

// Initialize on DOM ready
document.addEventListener('DOMContentLoaded', function () {
    if (typeof invitationToken !== 'undefined') {
        var vm = new AcceptInvitationViewModel();
        ko.applyBindings(vm, document.getElementById('accept-app'));
    }
});
