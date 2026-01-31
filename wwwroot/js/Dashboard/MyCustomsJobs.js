/**
 * MyCustomsJobs.js - Gumruk Islerim
 * Kabul edilen gumruk isleri + Evrim API entegrasyonu
 */

(function () {
    'use strict';

    function MyCustomsJobsViewModel() {
        var self = this;

        // Tab
        self.currentTab = ko.observable('active');

        // Data
        self.jobs = ko.observableArray([]);
        self.selectedJob = ko.observable(null);
        self.selectedJobForDeclaration = ko.observable(null);
        self.declarations = ko.observableArray([]);

        // State
        self.isLoading = ko.observable(false);
        self.isLoadingDeclarations = ko.observable(false);
        self.isCreatingDeclaration = ko.observable(false);
        self.isSyncing = ko.observable(false);

        // Service Type ID for customs
        var SERVICE_TYPE = 2;

        // Computed
        self.activeJobs = ko.computed(function () {
            return self.jobs().filter(function (j) {
                return !j.isTaskCompleted && j.status !== 3;
            });
        });

        self.completedJobs = ko.computed(function () {
            return self.jobs().filter(function (j) {
                return j.isTaskCompleted || j.status === 3;
            });
        });

        // Load jobs
        self.loadJobs = function () {
            self.isLoading(true);

            fetch('/api/provider/my-jobs?serviceType=' + SERVICE_TYPE)
                .then(function (response) { return response.json(); })
                .then(function (result) {
                    if (result.success) {
                        self.jobs(result.data || []);
                    } else {
                        toastr.error(result.message || 'Isler yuklenemedi');
                    }
                })
                .catch(function (error) {
                    console.error('Error:', error);
                    toastr.error('Bir hata olustu');
                })
                .finally(function () {
                    self.isLoading(false);
                });
        };

        // View detail
        self.viewDetail = function (job) {
            self.isLoading(true);

            fetch('/api/provider/my-jobs/' + job.id)
                .then(function (response) { return response.json(); })
                .then(function (result) {
                    if (result.success) {
                        self.selectedJob(result.data);
                        var modal = new bootstrap.Modal(document.getElementById('jobDetailModal'));
                        modal.show();
                    } else {
                        toastr.error(result.message || 'Detay yuklenemedi');
                    }
                })
                .catch(function (error) {
                    console.error('Error:', error);
                    toastr.error('Bir hata olustu');
                })
                .finally(function () {
                    self.isLoading(false);
                });
        };

        // Complete job
        self.completeJob = function (job) {
            if (!job) return;

            showConfirmModal({
                title: 'Isi Tamamla',
                message: 'Bu isi tamamlandi olarak isaretlemek istediginize emin misiniz?',
                type: 'success',
                confirmText: 'Tamamla',
                confirmIcon: 'bi bi-check-lg',
                onConfirm: function () {
                    fetch('/api/provider/my-jobs/' + job.id + '/complete', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' }
                    })
                        .then(function (response) { return response.json(); })
                        .then(function (result) {
                            if (result.success) {
                                toastr.success('Is tamamlandi olarak isaretlendi');
                                self.loadJobs();
                            } else {
                                toastr.error(result.message || 'Islem basarisiz');
                            }
                        })
                        .catch(function (error) {
                            console.error('Error:', error);
                            toastr.error('Bir hata olustu');
                        });
                }
            });
        };

        // ============================================================
        // EVRIM INTEGRATION
        // ============================================================

        // Open declaration modal
        self.openDeclarationModal = function (job) {
            self.selectedJobForDeclaration(job);
            self.declarations([]);
            self.loadDeclarations(job.orderId);

            var modal = new bootstrap.Modal(document.getElementById('declarationModal'));
            modal.show();
        };

        // Load declarations for order
        self.loadDeclarations = function (orderId) {
            self.isLoadingDeclarations(true);

            fetch('/api/customs/order/' + orderId)
                .then(function (response) { return response.json(); })
                .then(function (data) {
                    self.declarations(data || []);
                })
                .catch(function (error) {
                    console.error('Error:', error);
                    toastr.error('Beyannameler yuklenemedi');
                })
                .finally(function () {
                    self.isLoadingDeclarations(false);
                });
        };

        // Create declaration
        self.createDeclaration = function (type) {
            var job = self.selectedJobForDeclaration();
            if (!job) return;

            self.isCreatingDeclaration(true);

            var endpoint = '/api/customs/' + type;
            fetch(endpoint, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ orderId: job.orderId })
            })
                .then(function (response) { return response.json(); })
                .then(function (result) {
                    if (result.basarili) {
                        toastr.success('Beyanname olusturuldu: ' + (result.dosyaNo || ''));
                        self.loadDeclarations(job.orderId);
                    } else {
                        toastr.error(result.hataMesaji || 'Beyanname olusturulamadi');
                    }
                })
                .catch(function (error) {
                    console.error('Error:', error);
                    toastr.error('Bir hata olustu');
                })
                .finally(function () {
                    self.isCreatingDeclaration(false);
                });
        };

        // Sync declaration status
        self.syncDeclaration = function (declaration) {
            self.isSyncing(true);

            fetch('/api/customs/' + declaration.id + '/sync', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' }
            })
                .then(function (response) { return response.json(); })
                .then(function (result) {
                    if (result.id) {
                        toastr.success('Durum guncellendi');
                        // Update declaration in list
                        var index = self.declarations().findIndex(function(d) { return d.id === declaration.id; });
                        if (index >= 0) {
                            self.declarations.splice(index, 1, result);
                        }
                    } else {
                        toastr.error(result.message || 'Senkronizasyon basarisiz');
                    }
                })
                .catch(function (error) {
                    console.error('Error:', error);
                    toastr.error('Bir hata olustu');
                })
                .finally(function () {
                    self.isSyncing(false);
                });
        };

        // Helpers
        self.formatDate = function (dateStr) {
            if (!dateStr) return '-';
            var date = new Date(dateStr);
            return date.toLocaleDateString('tr-TR');
        };

        self.formatCurrency = function (amount, currency) {
            if (amount === null || amount === undefined) return '-';
            return new Intl.NumberFormat('tr-TR', {
                style: 'currency',
                currency: currency || 'TRY',
                minimumFractionDigits: 2
            }).format(amount);
        };

        // Init
        self.loadJobs();
    }

    // Initialize when DOM is ready
    document.addEventListener('DOMContentLoaded', function () {
        var container = document.getElementById('my-customs-jobs-app');
        if (container) {
            ko.applyBindings(new MyCustomsJobsViewModel(), container);
        }
    });
})();
