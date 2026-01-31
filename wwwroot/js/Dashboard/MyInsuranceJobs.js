/**
 * MyInsuranceJobs.js - Sigorta Islerim
 * Kabul edilen ve tamamlanan sigorta isleri
 */

(function () {
    'use strict';

    function MyInsuranceJobsViewModel() {
        var self = this;

        self.currentTab = ko.observable('active');
        self.jobs = ko.observableArray([]);
        self.selectedJob = ko.observable(null);
        self.isLoading = ko.observable(false);

        var SERVICE_TYPE = 3; // Insurance

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
                                var modalEl = document.getElementById('jobDetailModal');
                                var modal = bootstrap.Modal.getInstance(modalEl);
                                if (modal) modal.hide();
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

        self.formatDate = function (dateStr) {
            if (!dateStr) return '-';
            return new Date(dateStr).toLocaleDateString('tr-TR');
        };

        self.formatCurrency = function (amount, currency) {
            if (amount === null || amount === undefined) return '-';
            return new Intl.NumberFormat('tr-TR', {
                style: 'currency',
                currency: currency || 'TRY',
                minimumFractionDigits: 2
            }).format(amount);
        };

        self.loadJobs();
    }

    document.addEventListener('DOMContentLoaded', function () {
        var container = document.getElementById('my-insurance-jobs-app');
        if (container) {
            ko.applyBindings(new MyInsuranceJobsViewModel(), container);
        }
    });
})();
