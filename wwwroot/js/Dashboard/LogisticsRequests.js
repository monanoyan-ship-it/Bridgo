/**
 * LogisticsRequests.js - Lojistik Talepleri
 * Servis saglayicilar icin acik lojistik taleplerini listeler
 */

(function () {
    'use strict';

    function LogisticsRequestsViewModel() {
        var self = this;

        // Tab
        self.currentTab = ko.observable('open');

        // Data
        self.requests = ko.observableArray([]);
        self.myQuotes = ko.observableArray([]);
        self.selectedRequest = ko.observable(null);

        // Filters
        self.searchTerm = ko.observable('');
        self.statusFilter = ko.observable('');
        self.quoteStatusFilter = ko.observable('');
        self.quoteStatuses = ko.observableArray([]);

        // State
        self.isLoading = ko.observable(false);
        self.isSubmitting = ko.observable(false);

        // Quote form
        self.quoteForm = ko.observable({
            quoteAmount: null,
            currency: 'TRY',
            estimatedDays: null,
            carrierName: '',
            transitStops: null,
            validUntil: '',
            includedServices: '',
            additionalCosts: '',
            notes: ''
        });

        // Transport modes (multiple selection)
        self.transportModeOptions = [
            { id: '1', name: 'Karayolu' },
            { id: '2', name: 'Denizyolu' },
            { id: '3', name: 'Havayolu' },
            { id: '4', name: 'Demiryolu' },
            { id: '5', name: 'Multimodal' }
        ];
        self.selectedTransportMode = ko.observable('');
        self.selectedTransportModes = ko.observableArray([]);

        // Add transport mode
        self.addTransportMode = function () {
            var modeId = self.selectedTransportMode();
            if (!modeId) return;

            var mode = self.transportModeOptions.find(function (m) { return m.id === modeId; });
            if (mode) {
                // Aynısından eklenmesini engelleme - her seferinde ekle
                self.selectedTransportModes.push({ id: mode.id, name: mode.name });
                self.selectedTransportMode(''); // Reset combobox
                self.updateTransitStopsMin();
            }
        };

        // Remove transport mode
        self.removeTransportMode = function (mode) {
            self.selectedTransportModes.remove(mode);
            self.updateTransitStopsMin();
        };

        // Transit stops minimum = tag sayisi
        self.minTransitStops = ko.computed(function () {
            return self.selectedTransportModes().length;
        });

        // Transit stops değerini güncelle (min değerin altına düşmesin)
        self.updateTransitStopsMin = function () {
            var form = self.quoteForm();
            var min = self.selectedTransportModes().length;
            var current = parseInt(form.transitStops) || 0;
            if (current < min) {
                form.transitStops = min;
                self.quoteForm(form);
            }
        };

        // Computed
        self.openRequests = ko.computed(function () {
            return self.requests().filter(function (r) {
                return r.status === 1 || r.status === 2;
            });
        });

        self.filteredRequests = ko.computed(function () {
            var search = self.searchTerm().toLowerCase();
            var status = self.statusFilter();

            return self.requests().filter(function (r) {
                var matchSearch = !search ||
                    r.title.toLowerCase().indexOf(search) >= 0 ||
                    r.orderNumber.toLowerCase().indexOf(search) >= 0 ||
                    r.buyerName.toLowerCase().indexOf(search) >= 0 ||
                    (r.origin && r.origin.toLowerCase().indexOf(search) >= 0) ||
                    (r.destination && r.destination.toLowerCase().indexOf(search) >= 0);

                var matchStatus = !status ||
                    (status === 'open' && r.status === 1) ||
                    (status === 'quoted' && r.status === 2);

                return matchSearch && matchStatus;
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

            $.get('/api/service-requests/logistics')
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

            $.get('/api/service-requests/my-quotes', { serviceType: 1 })
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
                quoteAmount: null,
                currency: 'TRY',
                estimatedDays: null,
                carrierName: '',
                transitStops: null,
                validUntil: '',
                includedServices: '',
                additionalCosts: '',
                notes: ''
            });

            // Transport modes'u temizle
            self.selectedTransportMode('');
            self.selectedTransportModes([]);

            new bootstrap.Modal(document.getElementById('quoteModal')).show();
        };

        // Submit quote
        self.submitQuote = function () {
            var form = self.quoteForm();
            var request = self.selectedRequest();

            if (!form.quoteAmount || form.quoteAmount <= 0) {
                toastr.warning('Lutfen teklif tutari girin');
                return;
            }

            self.isSubmitting(true);

            // Transport modes'u array olarak al
            var transportModes = self.selectedTransportModes().map(function (m) { return parseInt(m.id); });

            $.ajax({
                url: '/api/service-requests/logistics/' + request.id + '/quote',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({
                    quoteAmount: parseFloat(form.quoteAmount),
                    currency: form.currency,
                    estimatedDays: form.estimatedDays ? parseInt(form.estimatedDays) : null,
                    transportModes: transportModes.length > 0 ? transportModes : null,
                    carrierName: form.carrierName,
                    transitStops: form.transitStops ? parseInt(form.transitStops) : null,
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
        ko.applyBindings(new LogisticsRequestsViewModel(), document.getElementById('logistics-app'));
    });

})();
