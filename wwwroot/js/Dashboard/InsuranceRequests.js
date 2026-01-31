/**
 * InsuranceRequests.js - Sigorta Talepleri
 * Sigorta sirketleri icin acik sigorta taleplerini listeler
 */

(function () {
    'use strict';

    function InsuranceRequestsViewModel() {
        var self = this;

        // Tab
        self.currentTab = ko.observable('open');

        // Data
        self.requests = ko.observableArray([]);
        self.myQuotes = ko.observableArray([]);
        self.selectedRequest = ko.observable(null);

        // Filters
        self.searchTerm = ko.observable('');
        self.insuranceTypeFilter = ko.observable('');
        self.quoteStatusFilter = ko.observable('');
        self.quoteStatuses = ko.observableArray([]);

        // State
        self.isLoading = ko.observable(false);
        self.isSubmitting = ko.observable(false);

        // Quote form
        self.quoteForm = ko.observable({
            quoteAmount: null,
            currency: 'TRY',
            coverageAmount: null,
            deductiblePercent: null,
            validUntil: '',
            includedServices: '',
            additionalCosts: '',
            notes: ''
        });

        // Computed
        self.openRequests = ko.computed(function () {
            return self.requests().filter(function (r) {
                return r.status === 1 || r.status === 2;
            });
        });

        self.filteredRequests = ko.computed(function () {
            var search = self.searchTerm().toLowerCase();
            var insuranceType = self.insuranceTypeFilter();

            return self.requests().filter(function (r) {
                var matchSearch = !search ||
                    r.title.toLowerCase().indexOf(search) >= 0 ||
                    r.orderNumber.toLowerCase().indexOf(search) >= 0 ||
                    r.buyerName.toLowerCase().indexOf(search) >= 0 ||
                    (r.origin && r.origin.toLowerCase().indexOf(search) >= 0) ||
                    (r.destination && r.destination.toLowerCase().indexOf(search) >= 0);

                var matchType = !insuranceType ||
                    r.insuranceType == insuranceType;

                return matchSearch && matchType;
            });
        });

        self.filteredQuotes = ko.computed(function () {
            var statusFilter = self.quoteStatusFilter();
            return self.myQuotes().filter(function (q) {
                return !statusFilter || q.status == statusFilter;
            });
        });

        // Load quote statuses
        self.loadQuoteStatuses = function () {
            $.get('/api/service-requests/quote-statuses')
                .done(function (data) {
                    self.quoteStatuses(data || []);
                });
        };

        // Tab change handler
        self.currentTab.subscribe(function (tab) {
            if (tab === 'open') {
                self.loadRequests();
            } else if (tab === 'quotes') {
                self.loadMyQuotes();
            }
        });

        // Load requests
        self.loadRequests = function () {
            self.isLoading(true);

            $.get('/api/service-requests/insurance')
                .done(function (data) {
                    self.requests(data || []);
                })
                .fail(function (xhr) {
                    console.error('Talepler yuklenemedi:', xhr);
                    toastr.error('Talepler yuklenemedi');
                })
                .always(function () {
                    self.isLoading(false);
                });
        };

        // Load my quotes
        self.loadMyQuotes = function () {
            self.isLoading(true);

            $.get('/api/service-requests/my-quotes', { serviceType: 3 })
                .done(function (data) {
                    self.myQuotes(data || []);
                })
                .fail(function (xhr) {
                    console.error('Teklifler yuklenemedi:', xhr);
                })
                .always(function () {
                    self.isLoading(false);
                });
        };

        // Open quote modal
        self.openQuoteModal = function (request) {
            self.selectedRequest(request);

            // Form her zaman bos baslar (birden fazla teklif verilebilir)
            // Teminat tutari olarak mal degerini default olarak koy
            self.quoteForm({
                quoteAmount: null,
                currency: 'TRY',
                coverageAmount: request.cargoValue || null,
                deductiblePercent: null,
                validUntil: '',
                includedServices: '',
                additionalCosts: '',
                notes: ''
            });

            new bootstrap.Modal(document.getElementById('quoteModal')).show();
        };

        // Submit quote
        self.submitQuote = function () {
            var form = self.quoteForm();
            var request = self.selectedRequest();

            if (!form.quoteAmount || form.quoteAmount <= 0) {
                toastr.warning('Lutfen prim tutari girin');
                return;
            }

            self.isSubmitting(true);

            $.ajax({
                url: '/api/service-requests/insurance/' + request.id + '/quote',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({
                    quoteAmount: parseFloat(form.quoteAmount),
                    currency: form.currency,
                    coverageAmount: form.coverageAmount ? parseFloat(form.coverageAmount) : null,
                    deductiblePercent: form.deductiblePercent ? parseFloat(form.deductiblePercent) : null,
                    validUntil: form.validUntil || null,
                    includedServices: form.includedServices,
                    additionalCosts: form.additionalCosts,
                    notes: form.notes
                })
            })
                .done(function (response) {
                    toastr.success(response.message || 'Teklif gonderildi');
                    bootstrap.Modal.getInstance(document.getElementById('quoteModal')).hide();
                    self.loadRequests();
                })
                .fail(function (xhr) {
                    var msg = xhr.responseJSON?.message || 'Teklif gonderilemedi';
                    toastr.error(msg);
                })
                .always(function () {
                    self.isSubmitting(false);
                });
        };

        // Withdraw quote
        self.withdrawQuote = function (quote) {
            showConfirmModal({
                title: 'Teklif Geri Cekme',
                message: 'Bu teklifi geri cekmek istediginize emin misiniz?',
                type: 'warning',
                confirmText: 'Geri Cek',
                confirmIcon: 'bi bi-x-circle',
                onConfirm: function () {
                    $.ajax({
                        url: '/api/service-requests/my-quotes/' + quote.id + '/withdraw',
                        method: 'POST'
                    })
                        .done(function () {
                            toastr.success('Teklif geri cekildi');
                            self.loadMyQuotes();
                        })
                        .fail(function (xhr) {
                            var msg = xhr.responseJSON?.message || 'Islem basarisiz';
                            toastr.error(msg);
                        });
                }
            });
        };

        // Init
        self.loadQuoteStatuses();
        self.loadRequests();
    }

    $(document).ready(function () {
        ko.applyBindings(new InsuranceRequestsViewModel(), document.getElementById('insurance-app'));
    });

})();
