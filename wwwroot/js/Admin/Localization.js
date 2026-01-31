// Admin Localization ViewModel
function LocalizationViewModel() {
    var self = this;

    // Observables
    self.languages = ko.observableArray([]);
    self.selectedLanguageId = ko.observable(null);
    self.resources = ko.observableArray([]);
    self.isLoading = ko.observable(false);
    self.isSaving = ko.observable(false);
    self.isImporting = ko.observable(false);

    // Pagination
    self.currentPage = ko.observable(1);
    self.pageSize = ko.observable(50);
    self.totalCount = ko.observable(0);
    self.totalPages = ko.computed(function () {
        return Math.ceil(self.totalCount() / self.pageSize()) || 1;
    });

    // Search
    self.searchQuery = ko.observable('');
    self.searchTimeout = null;

    // Inline editing
    self.editingResourceId = ko.observable(null);
    self.editingValue = ko.observable('');

    // New resource
    self.newResourceName = ko.observable('');
    self.newResourceValue = ko.observable('');

    // Modals
    self.resourceModal = null;

    // Visible pages for pagination
    self.visiblePages = ko.computed(function () {
        var pages = [];
        var total = self.totalPages();
        var current = self.currentPage();

        var start = Math.max(1, current - 2);
        var end = Math.min(total, current + 2);

        for (var i = start; i <= end; i++) {
            pages.push(i);
        }
        return pages;
    });

    // Watch language selection
    self.selectedLanguageId.subscribe(function (languageId) {
        if (languageId) {
            self.currentPage(1);
            self.loadResources();
            // Update URL
            var url = new URL(window.location.href);
            url.searchParams.set('languageId', languageId);
            window.history.replaceState({}, '', url);
        }
    });

    // Watch search query with debounce
    self.searchQuery.subscribe(function () {
        clearTimeout(self.searchTimeout);
        self.searchTimeout = setTimeout(function () {
            self.currentPage(1);
            self.loadResources();
        }, 300);
    });

    // Load languages (only active)
    self.loadLanguages = function () {
        $.get('/api/admin/languages?onlyActive=true')
            .done(function (data) {
                self.languages(data);

                // Check URL for languageId
                var urlParams = new URLSearchParams(window.location.search);
                var languageId = urlParams.get('languageId');

                if (languageId) {
                    self.selectedLanguageId(parseInt(languageId));
                } else if (data.length > 0) {
                    // Select default or first language
                    var defaultLang = data.find(function (l) { return l.isDefault; });
                    self.selectedLanguageId(defaultLang ? defaultLang.id : data[0].id);
                }
            })
            .fail(function () {
                toastr.error('Diller yuklenemedi');
            });
    };

    // Load resources
    self.loadResources = function () {
        if (!self.selectedLanguageId()) return;

        self.isLoading(true);

        var params = new URLSearchParams();
        params.append('page', self.currentPage());
        params.append('pageSize', self.pageSize());
        if (self.searchQuery()) params.append('search', self.searchQuery());

        $.get('/api/admin/languages/' + self.selectedLanguageId() + '/resources?' + params.toString())
            .done(function (data) {
                self.resources(data.items);
                self.totalCount(data.totalCount);
            })
            .fail(function () {
                toastr.error('Kaynaklar yuklenemedi');
            })
            .always(function () {
                self.isLoading(false);
            });
    };

    // Pagination
    self.goToPage = function (page) {
        if (page >= 1 && page <= self.totalPages()) {
            self.currentPage(page);
            self.loadResources();
        }
    };

    self.previousPage = function () {
        self.goToPage(self.currentPage() - 1);
    };

    self.nextPage = function () {
        self.goToPage(self.currentPage() + 1);
    };

    // Inline editing
    self.startInlineEdit = function (resource) {
        self.editingResourceId(resource.id);
        self.editingValue(resource.resourceValue);
    };

    self.cancelInlineEdit = function () {
        self.editingResourceId(null);
        self.editingValue('');
    };

    self.saveInlineEdit = function (resource) {
        if (!self.editingValue()) {
            toastr.warning('Deger bos olamaz');
            return;
        }

        $.ajax({
            url: '/api/admin/localization/resources/' + resource.id,
            method: 'PUT',
            contentType: 'application/json',
            data: JSON.stringify({ resourceValue: self.editingValue() })
        })
        .done(function (response) {
            toastr.success(response.message);
            resource.resourceValue = self.editingValue();
            self.cancelInlineEdit();
            self.loadResources();
        })
        .fail(function (xhr) {
            var msg = xhr.responseJSON?.message || 'Guncelleme basarisiz';
            toastr.error(msg);
        });
    };

    // Show add modal
    self.showAddModal = function () {
        self.newResourceName('');
        self.newResourceValue('');
        self.resourceModal.show();
    };

    // Save new resource
    self.saveResource = function () {
        if (!self.newResourceName() || !self.newResourceValue()) {
            toastr.warning('Lutfen zorunlu alanlari doldurun');
            return;
        }

        self.isSaving(true);

        var data = {
            languageId: self.selectedLanguageId(),
            resourceName: self.newResourceName(),
            resourceValue: self.newResourceValue()
        };

        $.ajax({
            url: '/api/admin/localization/resources',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data)
        })
        .done(function (response) {
            toastr.success(response.message);
            self.resourceModal.hide();
            self.loadResources();
        })
        .fail(function (xhr) {
            var msg = xhr.responseJSON?.message || 'Kaydetme basarisiz';
            toastr.error(msg);
        })
        .always(function () {
            self.isSaving(false);
        });
    };

    // Delete resource
    self.deleteResource = function (resource) {
        showConfirmModal({
            title: 'Kaynak Sil',
            message: '"' + resource.resourceName + '" kaynagini silmek istiyor musunuz?',
            confirmText: 'Sil',
            type: 'danger',
            onConfirm: function () {
                $.ajax({
                    url: '/api/admin/localization/resources/' + resource.id,
                    method: 'DELETE'
                })
                .done(function (response) {
                    toastr.success(response.message);
                    self.loadResources();
                })
                .fail(function (xhr) {
                    var msg = xhr.responseJSON?.message || 'Silme basarisiz';
                    toastr.error(msg);
                });
            }
        });
    };

    // Export resources as XML
    self.exportResources = function () {
        if (!self.selectedLanguageId()) return;

        $.get('/api/admin/languages/' + self.selectedLanguageId() + '/resources/export')
            .done(function (data) {
                // Build XML
                var xml = '<?xml version="1.0" encoding="utf-8"?>\n<resources>\n';
                data.forEach(function (item) {
                    // Escape XML special characters
                    var value = item.resourceValue
                        .replace(/&/g, '&amp;')
                        .replace(/</g, '&lt;')
                        .replace(/>/g, '&gt;')
                        .replace(/"/g, '&quot;')
                        .replace(/'/g, '&apos;');
                    xml += '  <resource name="' + item.resourceName + '">' + value + '</resource>\n';
                });
                xml += '</resources>';

                var blob = new Blob([xml], { type: 'application/xml' });
                var url = window.URL.createObjectURL(blob);
                var a = document.createElement('a');
                a.href = url;

                var lang = self.languages().find(function (l) { return l.id === self.selectedLanguageId(); });
                var filename = 'localization_' + (lang ? lang.uniqueSeoCode : 'export') + '.xml';
                a.download = filename;
                document.body.appendChild(a);
                a.click();
                window.URL.revokeObjectURL(url);
                document.body.removeChild(a);

                toastr.success(data.length + ' kaynak disa aktarildi');
            })
            .fail(function () {
                toastr.error('Disa aktarma basarisiz');
            });
    };

    // Import resources from file (wwwroot/data/localization/resources.{seoCode}.xml)
    self.importFromFile = function () {
        if (!self.selectedLanguageId()) {
            toastr.warning('Lutfen once bir dil secin');
            return;
        }

        var lang = self.languages().find(function (l) { return l.id === self.selectedLanguageId(); });
        var fileName = 'resources.' + (lang ? lang.uniqueSeoCode : '') + '.xml';

        showConfirmModal({
            title: 'Dosyadan Yukle',
            message: '"' + fileName + '" dosyasindan kaynaklari yuklemek istiyor musunuz? Mevcut kaynaklar guncellenecek, yenileri eklenecek.',
            confirmText: 'Yukle',
            type: 'success',
            onConfirm: function () {
                self.isImporting(true);

                $.ajax({
                    url: '/api/admin/languages/' + self.selectedLanguageId() + '/resources/import-from-file',
                    method: 'POST'
                })
                .done(function (response) {
                    toastr.success(response.message);
                    self.loadResources();
                })
                .fail(function (xhr) {
                    var msg = xhr.responseJSON?.message || 'Ice aktarma basarisiz';
                    toastr.error(msg);
                })
                .always(function () {
                    self.isImporting(false);
                });
            }
        });
    };

    // Initialize
    self.init = function () {
        self.resourceModal = new bootstrap.Modal(document.getElementById('resourceModal'));
        self.loadLanguages();
    };

    self.init();
}

// Apply bindings
$(function () {
    ko.applyBindings(new LocalizationViewModel(), document.getElementById('localization-app'));
});
