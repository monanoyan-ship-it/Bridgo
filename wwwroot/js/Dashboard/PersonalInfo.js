/**
 * PersonalInfo.js - Kisisel Bilgiler
 */

(function () {
    'use strict';

    function PersonalInfoViewModel() {
        var self = this;

        // ============================================
        // OBSERVABLES
        // ============================================

        self.userId = ko.observable(0);
        self.firstName = ko.observable('');
        self.lastName = ko.observable('');
        self.email = ko.observable('');
        self.phoneNumber = ko.observable('');
        self.profileImageUrl = ko.observable('');
        self.languageId = ko.observable(null);
        self.createdAt = ko.observable(null);
        self.lastLoginAt = ko.observable(null);

        self.languages = ko.observableArray([]);
        self.isSaving = ko.observable(false);
        self.isLoading = ko.observable(false);

        // ============================================
        // COMPUTED
        // ============================================

        self.fullName = ko.computed(function () {
            return (self.firstName() + ' ' + self.lastName()).trim() || 'Kullanici';
        });

        self.initials = ko.computed(function () {
            var f = self.firstName() ? self.firstName().charAt(0).toUpperCase() : '';
            var l = self.lastName() ? self.lastName().charAt(0).toUpperCase() : '';
            return f + l || 'U';
        });

        // ============================================
        // API CALLS
        // ============================================

        self.loadData = function () {
            self.isLoading(true);

            // Load personal info and languages in parallel
            $.when(
                $.get('/api/account/personal-info'),
                $.get('/api/account/languages')
            ).done(function (infoResult, langResult) {
                var info = infoResult[0];
                var langs = langResult[0];

                self.userId(info.userId);
                self.firstName(info.firstName || '');
                self.lastName(info.lastName || '');
                self.email(info.email || '');
                self.phoneNumber(info.phoneNumber || '');
                self.profileImageUrl(info.profileImageUrl || '');
                self.languageId(info.languageId);
                self.createdAt(info.createdAt);
                self.lastLoginAt(info.lastLoginAt);

                self.languages(langs || []);
            }).fail(function (xhr) {
                console.error('Veri yukleme hatasi:', xhr);
                toastr.error('Bilgiler yuklenirken hata olustu');
            }).always(function () {
                self.isLoading(false);
            });
        };

        self.savePersonalInfo = function () {
            if (!self.firstName() || !self.lastName()) {
                toastr.warning('Ad ve Soyad gereklidir');
                return;
            }

            self.isSaving(true);

            $.ajax({
                url: '/api/account/personal-info',
                method: 'PUT',
                contentType: 'application/json',
                data: JSON.stringify({
                    firstName: self.firstName(),
                    lastName: self.lastName(),
                    phoneNumber: self.phoneNumber() || null,
                    languageId: self.languageId() || null
                })
            }).done(function () {
                toastr.success('Bilgiler kaydedildi');
            }).fail(function (xhr) {
                var msg = xhr.responseJSON?.message || 'Kaydetme hatasi';
                toastr.error(msg);
            }).always(function () {
                self.isSaving(false);
            });
        };

        // ============================================
        // PROFILE IMAGE
        // ============================================

        self.selectImage = function () {
            $('#profileImageInput').click();
        };

        self.uploadImage = function (vm, event) {
            var file = event.target.files[0];
            if (!file) return;

            // Validate file
            if (!file.type.startsWith('image/')) {
                toastr.error('Sadece resim dosyalari yuklenebilir');
                return;
            }

            if (file.size > 5 * 1024 * 1024) {
                toastr.error('Dosya boyutu 5MB\'dan kucuk olmalidir');
                return;
            }

            // For now, convert to base64 and save
            // In production, you'd upload to a file server
            var reader = new FileReader();
            reader.onload = function (e) {
                var imageUrl = e.target.result;

                $.ajax({
                    url: '/api/account/profile-image',
                    method: 'PUT',
                    contentType: 'application/json',
                    data: JSON.stringify({ profileImageUrl: imageUrl })
                }).done(function () {
                    self.profileImageUrl(imageUrl);
                    toastr.success('Profil resmi guncellendi');
                }).fail(function (xhr) {
                    toastr.error('Resim yuklenemedi');
                });
            };
            reader.readAsDataURL(file);

            // Reset input
            event.target.value = '';
        };

        self.deleteImage = function () {
            if (!confirm('Profil resmini silmek istediginize emin misiniz?')) return;

            $.ajax({
                url: '/api/account/profile-image',
                method: 'DELETE'
            }).done(function () {
                self.profileImageUrl('');
                toastr.success('Profil resmi silindi');
            }).fail(function () {
                toastr.error('Silme hatasi');
            });
        };

        // ============================================
        // HELPERS
        // ============================================

        self.formatDate = function (dateStr) {
            if (!dateStr) return '-';
            var date = new Date(dateStr);
            return date.toLocaleDateString('tr-TR', {
                day: '2-digit',
                month: 'long',
                year: 'numeric'
            });
        };

        // ============================================
        // INIT
        // ============================================

        self.loadData();
    }

    $(document).ready(function () {
        ko.applyBindings(new PersonalInfoViewModel(), document.getElementById('personal-info-app'));
    });

})();
