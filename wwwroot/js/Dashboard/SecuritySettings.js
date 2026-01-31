/**
 * SecuritySettings.js - Sifre ve Guvenlik
 */

(function () {
    'use strict';

    function SecuritySettingsViewModel() {
        var self = this;

        // ============================================
        // OBSERVABLES - Password Change
        // ============================================

        self.currentPassword = ko.observable('');
        self.newPassword = ko.observable('');
        self.confirmPassword = ko.observable('');
        self.isChangingPassword = ko.observable(false);

        // Password visibility toggles
        self.showCurrentPassword = ko.observable(false);
        self.showNewPassword = ko.observable(false);

        // ============================================
        // OBSERVABLES - Security Info
        // ============================================

        self.twoFactorEnabled = ko.observable(false);
        self.activeSessionsCount = ko.observable(1);
        self.isLoading = ko.observable(false);

        // ============================================
        // COMPUTED
        // ============================================

        self.passwordMismatch = ko.computed(function () {
            return self.confirmPassword() && self.newPassword() !== self.confirmPassword();
        });

        self.canChangePassword = ko.computed(function () {
            return self.currentPassword() &&
                self.newPassword() &&
                self.newPassword().length >= 6 &&
                self.confirmPassword() &&
                !self.passwordMismatch();
        });

        // ============================================
        // TOGGLE FUNCTIONS
        // ============================================

        self.toggleCurrentPassword = function () {
            self.showCurrentPassword(!self.showCurrentPassword());
        };

        self.toggleNewPassword = function () {
            self.showNewPassword(!self.showNewPassword());
        };

        // ============================================
        // API CALLS
        // ============================================

        self.loadSecurityInfo = function () {
            self.isLoading(true);

            $.get('/api/account/security')
                .done(function (data) {
                    self.twoFactorEnabled(data.twoFactorEnabled || false);
                    self.activeSessionsCount(data.activeSessionsCount || 1);
                })
                .fail(function (xhr) {
                    console.error('Guvenlik bilgileri yuklenemedi:', xhr);
                })
                .always(function () {
                    self.isLoading(false);
                });
        };

        self.changePassword = function () {
            if (!self.canChangePassword()) {
                if (self.passwordMismatch()) {
                    toastr.warning('Sifreler eslesmiyor');
                } else if (!self.newPassword() || self.newPassword().length < 6) {
                    toastr.warning('Yeni sifre en az 6 karakter olmalidir');
                } else {
                    toastr.warning('Tum alanlari doldurun');
                }
                return;
            }

            self.isChangingPassword(true);

            $.ajax({
                url: '/api/account/change-password',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({
                    currentPassword: self.currentPassword(),
                    newPassword: self.newPassword(),
                    confirmPassword: self.confirmPassword()
                })
            }).done(function () {
                toastr.success('Sifre basariyla degistirildi');
                self.clearPasswordFields();
            }).fail(function (xhr) {
                var msg = xhr.responseJSON?.message || 'Sifre degistirme hatasi';
                toastr.error(msg);
            }).always(function () {
                self.isChangingPassword(false);
            });
        };

        // ============================================
        // HELPERS
        // ============================================

        self.clearPasswordFields = function () {
            self.currentPassword('');
            self.newPassword('');
            self.confirmPassword('');
            self.showCurrentPassword(false);
            self.showNewPassword(false);
        };

        // ============================================
        // INIT
        // ============================================

        self.loadSecurityInfo();
    }

    $(document).ready(function () {
        ko.applyBindings(new SecuritySettingsViewModel(), document.getElementById('security-app'));
    });

})();
