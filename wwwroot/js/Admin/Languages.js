// Admin Languages ViewModel
function LanguagesViewModel() {
    var self = this;

    // Observables
    self.languages = ko.observableArray([]);
    self.isLoading = ko.observable(false);
    self.isSaving = ko.observable(false);
    self.isEditing = ko.observable(false);
    self.editingId = ko.observable(null);

    // Form data
    self.formData = {
        name: ko.observable(''),
        nativeName: ko.observable(''),
        languageCulture: ko.observable(''),
        uniqueSeoCode: ko.observable(''),
        iso3Code: ko.observable(''),
        flagEmoji: ko.observable(''),
        flagImageFileName: ko.observable(''),
        rtl: ko.observable(false),
        isGeoTranslationEnabled: ko.observable(false),
        isActive: ko.observable(true),
        displayOrder: ko.observable(0)
    };

    // Modal
    self.languageModal = null;

    // Load languages
    self.loadLanguages = function () {
        self.isLoading(true);

        $.get('/api/admin/languages')
            .done(function (data) {
                self.languages(data);
            })
            .fail(function () {
                toastr.error('Diller yuklenemedi');
            })
            .always(function () {
                self.isLoading(false);
            });
    };

    // Reset form
    self.resetForm = function () {
        self.formData.name('');
        self.formData.nativeName('');
        self.formData.languageCulture('');
        self.formData.uniqueSeoCode('');
        self.formData.iso3Code('');
        self.formData.flagEmoji('');
        self.formData.flagImageFileName('');
        self.formData.rtl(false);
        self.formData.isGeoTranslationEnabled(false);
        self.formData.isActive(true);
        self.formData.displayOrder(0);
        self.isEditing(false);
        self.editingId(null);
    };

    // Show add modal
    self.showAddModal = function () {
        self.resetForm();
        self.languageModal.show();
    };

    // Show edit modal
    self.showEditModal = function (language) {
        self.isEditing(true);
        self.editingId(language.id);
        self.formData.name(language.name);
        self.formData.nativeName(language.nativeName || '');
        self.formData.languageCulture(language.languageCulture);
        self.formData.uniqueSeoCode(language.uniqueSeoCode);
        self.formData.iso3Code(language.iso3Code || '');
        self.formData.flagEmoji(language.flagEmoji || '');
        self.formData.flagImageFileName(language.flagImageFileName || '');
        self.formData.rtl(language.rtl);
        self.formData.isGeoTranslationEnabled(language.isGeoTranslationEnabled);
        self.formData.isActive(language.isActive);
        self.formData.displayOrder(language.displayOrder);
        self.languageModal.show();
    };

    // Save language
    self.saveLanguage = function () {
        // Validation
        if (!self.formData.name() || !self.formData.languageCulture() || !self.formData.uniqueSeoCode()) {
            toastr.warning('Lutfen zorunlu alanlari doldurun');
            return;
        }

        self.isSaving(true);

        var data = {
            name: self.formData.name(),
            nativeName: self.formData.nativeName() || null,
            languageCulture: self.formData.languageCulture(),
            uniqueSeoCode: self.formData.uniqueSeoCode(),
            iso3Code: self.formData.iso3Code() || null,
            flagEmoji: self.formData.flagEmoji() || null,
            flagImageFileName: self.formData.flagImageFileName() || null,
            rtl: self.formData.rtl(),
            isGeoTranslationEnabled: self.formData.isGeoTranslationEnabled(),
            isActive: self.formData.isActive(),
            displayOrder: parseInt(self.formData.displayOrder()) || 0
        };

        var url = self.isEditing()
            ? '/api/admin/languages/' + self.editingId()
            : '/api/admin/languages';

        var method = self.isEditing() ? 'PUT' : 'POST';

        $.ajax({
            url: url,
            method: method,
            contentType: 'application/json',
            data: JSON.stringify(data)
        })
        .done(function (response) {
            toastr.success(response.message);
            self.languageModal.hide();
            self.loadLanguages();
        })
        .fail(function (xhr) {
            var msg = xhr.responseJSON?.message || 'Islem basarisiz';
            toastr.error(msg);
        })
        .always(function () {
            self.isSaving(false);
        });
    };

    // Delete language
    self.deleteLanguage = function (language) {
        showConfirmModal({
            title: 'Dil Sil',
            message: '"' + language.name + '" dilini ve tum kaynaklarini silmek istiyor musunuz?',
            confirmText: 'Sil',
            type: 'danger',
            onConfirm: function () {
                $.ajax({
                    url: '/api/admin/languages/' + language.id,
                    method: 'DELETE'
                })
                .done(function (response) {
                    toastr.success(response.message);
                    self.loadLanguages();
                })
                .fail(function (xhr) {
                    var msg = xhr.responseJSON?.message || 'Silme basarisiz';
                    toastr.error(msg);
                });
            }
        });
    };

    // Set default language
    self.setDefault = function (language) {
        showConfirmModal({
            title: 'Varsayilan Dil',
            message: '"' + language.name + '" dilini varsayilan yapmak istiyor musunuz?',
            confirmText: 'Varsayilan Yap',
            type: 'warning',
            onConfirm: function () {
                $.post('/api/admin/languages/' + language.id + '/set-default')
                    .done(function (response) {
                        toastr.success(response.message);
                        self.loadLanguages();
                    })
                    .fail(function (xhr) {
                        var msg = xhr.responseJSON?.message || 'Islem basarisiz';
                        toastr.error(msg);
                    });
            }
        });
    };

    // Toggle active/inactive
    self.toggleActive = function (language) {
        var action = language.isActive ? 'pasif' : 'aktif';
        showConfirmModal({
            title: 'Dil Durumu',
            message: '"' + language.name + '" dilini ' + action + ' yapmak istiyor musunuz?',
            confirmText: action.charAt(0).toUpperCase() + action.slice(1) + ' Yap',
            type: language.isActive ? 'warning' : 'success',
            onConfirm: function () {
                $.post('/api/admin/languages/' + language.id + '/toggle-active')
                    .done(function (response) {
                        toastr.success(response.message);
                        self.loadLanguages();
                    })
                    .fail(function (xhr) {
                        var msg = xhr.responseJSON?.message || 'Islem basarisiz';
                        toastr.error(msg);
                    });
            }
        });
    };

    // Go to resources page
    self.goToResources = function (language) {
        window.location.href = '/Admin/Localization?languageId=' + language.id;
    };

    // Initialize
    self.init = function () {
        self.languageModal = new bootstrap.Modal(document.getElementById('languageModal'));
        self.loadLanguages();
    };

    self.init();
}

// Apply bindings
$(function () {
    ko.applyBindings(new LanguagesViewModel(), document.getElementById('languages-app'));
});
