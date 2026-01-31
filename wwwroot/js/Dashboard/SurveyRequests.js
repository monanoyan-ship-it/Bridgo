/**
 * SurveyRequests.js - Gozetim Talepleri
 * Gozetim sirketleri icin acik gozetim taleplerini listeler
 */

(function () {
    'use strict';

    function SurveyRequestsViewModel() {
        var self = this;

        // Tab
        self.currentTab = ko.observable('open');

        // Data
        self.requests = ko.observableArray([]);
        self.myQuotes = ko.observableArray([]);
        self.selectedRequest = ko.observable(null);

        // Filters
        self.searchTerm = ko.observable('');
        self.surveyTypeFilter = ko.observable('');
        self.quoteStatusFilter = ko.observable('');
        self.quoteStatuses = ko.observableArray([]);

        // State
        self.isLoading = ko.observable(false);
        self.isSubmitting = ko.observable(false);

        // Survey type selection
        self.selectedSurveyType = ko.observable('');
        self.surveyTypeNames = {
            '1': 'Yukleme Oncesi',
            '2': 'Yukleme',
            '3': 'Bosaltma',
            '4': 'Hasar Tespiti'
        };

        // Quote form
        self.quoteForm = ko.observable({
            surveyTypes: ko.observableArray([]),
            quoteAmount: null,
            currency: 'TRY',
            validUntil: '',
            includedServices: '',
            additionalCosts: '',
            notes: ''
        });

        // Add survey type
        self.addSurveyType = function () {
            var typeId = self.selectedSurveyType();
            if (!typeId) return;

            var form = self.quoteForm();
            var exists = form.surveyTypes().some(function (t) { return t.id === typeId; });
            if (exists) {
                toastr.warning('Bu tip zaten eklendi');
                return;
            }

            form.surveyTypes.push({
                id: typeId,
                name: self.surveyTypeNames[typeId]
            });
            self.selectedSurveyType('');
        };

        // Remove survey type
        self.removeSurveyType = function (item) {
            self.quoteForm().surveyTypes.remove(item);
        };

        // Computed
        self.openRequests = ko.computed(function () {
            return self.requests().filter(function (r) {
                return r.status === 1 || r.status === 2;
            });
        });

        self.filteredRequests = ko.computed(function () {
            var search = self.searchTerm().toLowerCase();
            var surveyType = self.surveyTypeFilter();

            return self.requests().filter(function (r) {
                var matchSearch = !search ||
                    r.title.toLowerCase().indexOf(search) >= 0 ||
                    r.orderNumber.toLowerCase().indexOf(search) >= 0 ||
                    r.buyerName.toLowerCase().indexOf(search) >= 0 ||
                    (r.surveyLocation && r.surveyLocation.toLowerCase().indexOf(search) >= 0) ||
                    (r.origin && r.origin.toLowerCase().indexOf(search) >= 0);

                // SurveyTypes is comma-separated string (e.g. "1,2,3")
                var matchType = !surveyType ||
                    (r.surveyTypes && r.surveyTypes.split(',').indexOf(surveyType) >= 0);

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

            $.get('/api/service-requests/survey')
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

            $.get('/api/service-requests/my-quotes', { serviceType: 4 })
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
            self.quoteForm({
                surveyTypes: ko.observableArray([]),
                quoteAmount: null,
                currency: 'TRY',
                validUntil: '',
                includedServices: '',
                additionalCosts: '',
                notes: ''
            });
            self.selectedSurveyType('');

            new bootstrap.Modal(document.getElementById('quoteModal')).show();
        };

        // Submit quote
        self.submitQuote = function () {
            var form = self.quoteForm();
            var request = self.selectedRequest();

            if (form.surveyTypes().length === 0) {
                toastr.warning('Lutfen en az bir gozetim tipi secin');
                return;
            }

            if (!form.quoteAmount || form.quoteAmount <= 0) {
                toastr.warning('Lutfen teklif tutari girin');
                return;
            }

            self.isSubmitting(true);

            // Survey type ID'lerini array olarak al
            var surveyTypeIds = form.surveyTypes().map(function (t) { return parseInt(t.id); });

            $.ajax({
                url: '/api/service-requests/survey/' + request.id + '/quote',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({
                    surveyTypes: surveyTypeIds,
                    quoteAmount: parseFloat(form.quoteAmount),
                    currency: form.currency,
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
        ko.applyBindings(new SurveyRequestsViewModel(), document.getElementById('survey-app'));
    });

})();
