/**
 * Documents.js - Belgelerim (KYC)
 */

(function () {
    'use strict';

    function DocumentsViewModel() {
        var self = this;

        // ============================================
        // DOCUMENT TYPES
        // ============================================

        var allDocumentTypes = {
            identity: [
                { id: 'tc_id', name: 'TC Kimlik Karti' },
                { id: 'passport', name: 'Pasaport' },
                { id: 'driver_license', name: 'Ehliyet' }
            ],
            company: [
                { id: 'tax_plate', name: 'Vergi Levhasi' },
                { id: 'trade_registry', name: 'Ticaret Sicil Gazetesi' },
                { id: 'signature_circular', name: 'Imza Sirkuleri' },
                { id: 'activity_certificate', name: 'Faaliyet Belgesi' }
            ],
            bank: [
                { id: 'account_statement', name: 'Hesap Cuzdani' },
                { id: 'iban_document', name: 'IBAN Belgesi' }
            ],
            auth: [
                { id: 'power_of_attorney', name: 'Vekaletname' },
                { id: 'authorization', name: 'Yetki Belgesi' }
            ]
        };

        // ============================================
        // OBSERVABLES
        // ============================================

        self.identityDocs = ko.observableArray([]);
        self.companyDocs = ko.observableArray([]);
        self.bankDocs = ko.observableArray([]);
        self.authDocs = ko.observableArray([]);

        // Upload modal
        self.uploadCategory = ko.observable('');
        self.uploadType = ko.observable('');
        self.documentTypes = ko.observableArray([]);

        // ============================================
        // COMPUTED
        // ============================================

        self.identityDocCount = ko.computed(function () {
            return self.identityDocs().length;
        });

        self.companyDocCount = ko.computed(function () {
            return self.companyDocs().length;
        });

        self.bankDocCount = ko.computed(function () {
            return self.bankDocs().length;
        });

        self.authDocCount = ko.computed(function () {
            return self.authDocs().length;
        });

        // All documents combined for table view
        self.allDocuments = ko.computed(function () {
            var categoryInfo = {
                identity: { name: 'Kimlik', icon: 'bi-person-badge' },
                company: { name: 'Firma', icon: 'bi-building' },
                bank: { name: 'Banka', icon: 'bi-bank' },
                auth: { name: 'Yetki', icon: 'bi-file-earmark-check' }
            };

            var addCategoryInfo = function (docs, category) {
                return docs.map(function (d) {
                    d.categoryName = categoryInfo[category].name;
                    d.categoryIcon = categoryInfo[category].icon;
                    d.typeName = (allDocumentTypes[category] || []).find(function (t) { return t.id === d.type; })?.name || d.type;
                    return d;
                });
            };

            return [].concat(
                addCategoryInfo(self.identityDocs(), 'identity'),
                addCategoryInfo(self.companyDocs(), 'company'),
                addCategoryInfo(self.bankDocs(), 'bank'),
                addCategoryInfo(self.authDocs(), 'auth')
            );
        });

        self.canUpload = ko.computed(function () {
            return self.uploadCategory() && self.uploadType();
        });

        // Update document types when category changes
        self.uploadCategory.subscribe(function (category) {
            self.uploadType('');
            self.documentTypes(allDocumentTypes[category] || []);
        });

        // ============================================
        // STATE
        // ============================================

        self.isLoading = ko.observable(false);
        self.isUploading = ko.observable(false);

        // ============================================
        // API CALLS
        // ============================================

        self.loadData = function () {
            self.isLoading(true);

            $.get('/api/documents')
                .done(function (data) {
                    var docs = data || [];

                    var mapDoc = function (d) {
                        return {
                            id: d.id,
                            fileName: d.fileName,
                            category: d.category,
                            type: d.type,
                            uploadDate: new Date(d.uploadedAt).toLocaleDateString('tr-TR'),
                            status: d.status,
                            statusText: d.status === 'approved' ? 'Onaylandi' :
                                d.status === 'pending' ? 'Beklemede' : 'Reddedildi',
                            statusClass: d.status === 'approved' ? 'bg-success' :
                                d.status === 'pending' ? 'bg-warning' : 'bg-danger'
                        };
                    };

                    self.identityDocs(docs.filter(function (d) { return d.category === 'identity'; }).map(mapDoc));
                    self.companyDocs(docs.filter(function (d) { return d.category === 'company'; }).map(mapDoc));
                    self.bankDocs(docs.filter(function (d) { return d.category === 'bank'; }).map(mapDoc));
                    self.authDocs(docs.filter(function (d) { return d.category === 'auth'; }).map(mapDoc));
                })
                .fail(function (xhr) {
                    console.error('Belgeler yuklenemedi:', xhr);
                })
                .always(function () {
                    self.isLoading(false);
                });
        };

        self.showUploadModal = function () {
            self.uploadCategory('');
            self.uploadType('');
            document.getElementById('documentFile').value = '';
            new bootstrap.Modal(document.getElementById('uploadModal')).show();
        };

        self.uploadDocument = function () {
            var fileInput = document.getElementById('documentFile');
            var file = fileInput.files[0];

            if (!file) {
                toastr.warning('Lutfen dosya secin');
                return;
            }

            if (file.size > 5 * 1024 * 1024) {
                toastr.error('Dosya boyutu 5MB\'dan kucuk olmalidir');
                return;
            }

            self.isUploading(true);

            var formData = new FormData();
            formData.append('file', file);
            formData.append('category', self.uploadCategory());
            formData.append('type', self.uploadType());

            $.ajax({
                url: '/api/documents/upload',
                method: 'POST',
                data: formData,
                processData: false,
                contentType: false
            }).done(function () {
                bootstrap.Modal.getInstance(document.getElementById('uploadModal')).hide();
                toastr.success('Belge yuklendi');
                self.loadData();
            }).fail(function (xhr) {
                var msg = xhr.responseJSON?.message || 'Yukleme hatasi';
                toastr.error(msg);
            }).always(function () {
                self.isUploading(false);
            });
        };

        self.deleteDocument = function (doc) {
            showDeleteConfirm(doc.fileName, function () {
                $.ajax({
                    url: '/api/documents/' + doc.id,
                    method: 'DELETE'
                }).done(function () {
                    // Remove from appropriate array
                    self.identityDocs.remove(doc);
                    self.companyDocs.remove(doc);
                    self.bankDocs.remove(doc);
                    self.authDocs.remove(doc);
                    toastr.success('Belge silindi');
                }).fail(function (xhr) {
                    var msg = xhr.responseJSON?.message || 'Silme hatasi';
                    toastr.error(msg);
                });
            });
        };

        // ============================================
        // INIT
        // ============================================

        self.loadData();
    }

    $(document).ready(function () {
        ko.applyBindings(new DocumentsViewModel(), document.getElementById('documents-app'));
    });

})();
