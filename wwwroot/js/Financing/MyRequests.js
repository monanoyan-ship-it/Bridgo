(function () {
    'use strict';

    function FinancingViewModel() {
        var self = this;

        // State
        self.isLoading = ko.observable(false);
        self.isSubmitting = ko.observable(false);
        self.requests = ko.observableArray([]);
        self.orders = ko.observableArray([]);

        // Filters
        self.searchTerm = ko.observable('');
        self.statusFilter = ko.observable('');
        self.typeFilter = ko.observable('');

        // Form
        self.isEditing = ko.observable(false);
        self.editingId = ko.observable(null);
        self.form = ko.observable({
            financingType: '1',
            title: '',
            description: '',
            requestedAmount: '',
            currency: 'TRY',
            totalValue: '',
            maxInterestRate: '',
            durationDays: 30,
            offerDeadline: '',
            collateralType: '',
            collateralDescription: '',
            invoiceNumber: '',
            invoiceDate: '',
            debtorName: '',
            debtorTaxNumber: '',
            relatedOrderId: ''
        });

        // Detail
        self.detailData = ko.observable(null);

        // Offers
        self.selectedRequest = ko.observable(null);
        self.offers = ko.observableArray([]);
        self.offersLoading = ko.observable(false);

        // Offer actions
        self.selectedOffer = ko.observable(null);
        self.rejectReason = ko.observable('');
        self.transferReference = ko.observable('');
        self.repaymentReference = ko.observable('');

        // Computed
        self.filteredRequests = ko.computed(function () {
            var list = self.requests();
            var search = (self.searchTerm() || '').toLowerCase();
            var status = self.statusFilter();
            var type = self.typeFilter();

            return list.filter(function (r) {
                if (search && r.title.toLowerCase().indexOf(search) === -1) return false;
                if (status && r.status !== parseInt(status)) return false;
                if (type && r.financingType !== parseInt(type)) return false;
                return true;
            });
        });

        self.activeCount = ko.computed(function () {
            return self.requests().filter(function (r) {
                return r.status >= 2 && r.status <= 4;
            }).length;
        });

        self.pendingOfferCount = ko.computed(function () {
            return self.requests().reduce(function (sum, r) {
                return sum + (r.pendingOfferCount || 0);
            }, 0);
        });

        self.fundedTotal = ko.computed(function () {
            return self.requests().reduce(function (sum, r) {
                return sum + (r.fundedAmount || 0);
            }, 0);
        });

        // Helpers
        self.formatCurrency = function (amount) {
            return amount.toLocaleString('tr-TR');
        };

        // Load data
        self.loadRequests = function () {
            self.isLoading(true);
            $.get('/api/financing/my-requests')
                .done(function (data) {
                    self.requests(data || []);
                })
                .fail(function (xhr) {
                    toastr.error(xhr.responseJSON?.error || 'Talepler yuklenemedi');
                })
                .always(function () {
                    self.isLoading(false);
                });
        };

        self.loadOrders = function () {
            $.get('/api/financing/orders')
                .done(function (data) {
                    self.orders(data || []);
                });
        };

        // Create/Edit Modal
        self.openCreateModal = function () {
            self.isEditing(false);
            self.editingId(null);
            self.resetForm();
            self.loadOrders();
            $('#requestModal').modal('show');
        };

        self.editRequest = function (request) {
            self.isEditing(true);
            self.editingId(request.id);
            self.loadOrders();

            // Load detail first
            $.get('/api/financing/my-requests/' + request.id)
                .done(function (data) {
                    self.form({
                        financingType: String(data.financingType),
                        title: data.title || '',
                        description: data.description || '',
                        requestedAmount: data.requestedAmount || '',
                        currency: data.currency || 'TRY',
                        totalValue: data.totalValue || '',
                        maxInterestRate: data.maxInterestRate || '',
                        durationDays: data.durationDays || 30,
                        offerDeadline: data.offerDeadline ? data.offerDeadline.split('T')[0] : '',
                        collateralType: data.collateralType ? String(data.collateralType) : '',
                        collateralDescription: data.collateralDescription || '',
                        invoiceNumber: data.invoiceNumber || '',
                        invoiceDate: data.invoiceDate ? data.invoiceDate.split('T')[0] : '',
                        debtorName: data.debtorName || '',
                        debtorTaxNumber: data.debtorTaxNumber || '',
                        relatedOrderId: data.relatedOrderId ? String(data.relatedOrderId) : ''
                    });
                    $('#requestModal').modal('show');
                })
                .fail(function (xhr) {
                    toastr.error(xhr.responseJSON?.error || 'Talep yuklenemedi');
                });
        };

        self.resetForm = function () {
            self.form({
                financingType: '1',
                title: '',
                description: '',
                requestedAmount: '',
                currency: 'TRY',
                totalValue: '',
                maxInterestRate: '',
                durationDays: 30,
                offerDeadline: '',
                collateralType: '',
                collateralDescription: '',
                invoiceNumber: '',
                invoiceDate: '',
                debtorName: '',
                debtorTaxNumber: '',
                relatedOrderId: ''
            });
        };

        self.saveDraft = function () {
            self.saveRequest(true);
        };

        self.saveAndPublish = function () {
            self.saveRequest(false);
        };

        self.saveRequest = function (saveAsDraft) {
            var f = self.form();

            // Validation
            if (!f.title || !f.title.trim()) {
                toastr.warning('Baslik zorunludur');
                return;
            }
            if (!f.requestedAmount || parseFloat(f.requestedAmount) <= 0) {
                toastr.warning('Talep edilen tutar zorunludur');
                return;
            }
            if (!f.durationDays || parseInt(f.durationDays) <= 0) {
                toastr.warning('Vade suresi zorunludur');
                return;
            }

            var dto = {
                financingType: parseInt(f.financingType),
                title: f.title.trim(),
                description: f.description || null,
                requestedAmount: parseFloat(f.requestedAmount),
                currency: f.currency || 'TRY',
                totalValue: f.totalValue ? parseFloat(f.totalValue) : null,
                maxInterestRate: f.maxInterestRate ? parseFloat(f.maxInterestRate) : null,
                durationDays: parseInt(f.durationDays),
                offerDeadline: f.offerDeadline || null,
                collateralType: f.collateralType ? parseInt(f.collateralType) : null,
                collateralDescription: f.collateralDescription || null,
                invoiceNumber: f.invoiceNumber || null,
                invoiceDate: f.invoiceDate || null,
                debtorName: f.debtorName || null,
                debtorTaxNumber: f.debtorTaxNumber || null,
                relatedOrderId: f.relatedOrderId ? parseInt(f.relatedOrderId) : null,
                saveAsDraft: saveAsDraft
            };

            self.isSubmitting(true);

            var url = self.isEditing() ? '/api/financing/requests/' + self.editingId() : '/api/financing/requests';
            var method = self.isEditing() ? 'PUT' : 'POST';

            $.ajax({
                url: url,
                method: method,
                contentType: 'application/json',
                data: JSON.stringify(dto)
            })
                .done(function (result) {
                    toastr.success(result.message || 'Talep kaydedildi');
                    $('#requestModal').modal('hide');
                    self.loadRequests();
                })
                .fail(function (xhr) {
                    toastr.error(xhr.responseJSON?.error || 'Islem basarisiz');
                })
                .always(function () {
                    self.isSubmitting(false);
                });
        };

        // Publish draft
        self.publishRequest = function (request) {
            showConfirmModal({
                title: 'Talebi Yayinla',
                message: '"' + request.title + '" talebini yayinlamak istediginizden emin misiniz?',
                type: 'success',
                confirmText: 'Yayinla',
                confirmIcon: 'bi bi-broadcast',
                onConfirm: function () {
                    $.post('/api/financing/requests/' + request.id + '/publish')
                        .done(function (result) {
                            toastr.success(result.message || 'Talep yayinlandi');
                            self.loadRequests();
                        })
                        .fail(function (xhr) {
                            toastr.error(xhr.responseJSON?.error || 'Islem basarisiz');
                        });
                }
            });
        };

        // Cancel request
        self.cancelRequest = function (request) {
            var actionText = request.status === 1 ? 'silmek' : 'iptal etmek';
            showDeleteConfirm(request.title, function () {
                $.post('/api/financing/requests/' + request.id + '/cancel')
                    .done(function (result) {
                        toastr.success(result.message || 'Talep iptal edildi');
                        self.loadRequests();
                    })
                    .fail(function (xhr) {
                        toastr.error(xhr.responseJSON?.error || 'Islem basarisiz');
                    });
            });
        };

        // View detail
        self.viewDetail = function (request) {
            $.get('/api/financing/my-requests/' + request.id)
                .done(function (data) {
                    self.detailData(data);
                    $('#detailModal').modal('show');
                })
                .fail(function (xhr) {
                    toastr.error(xhr.responseJSON?.error || 'Detay yuklenemedi');
                });
        };

        // View offers
        self.viewOffers = function (request) {
            self.selectedRequest(request);
            self.offers([]);
            self.offersLoading(true);
            $('#offersModal').modal('show');

            $.get('/api/financing/my-requests/' + request.id)
                .done(function (data) {
                    self.offers(data.offers || []);
                    // Update selected request with fresh data
                    self.selectedRequest(data);
                })
                .fail(function (xhr) {
                    toastr.error(xhr.responseJSON?.error || 'Teklifler yuklenemedi');
                })
                .always(function () {
                    self.offersLoading(false);
                });
        };

        // Accept offer
        self.acceptOffer = function (offer) {
            showConfirmModal({
                title: 'Teklifi Kabul Et',
                message: offer.investorCompanyName + ' firmasinin ' +
                    offer.offeredAmount.toLocaleString('tr-TR') + ' tutarli teklifini kabul etmek istediginizden emin misiniz?',
                type: 'success',
                confirmText: 'Kabul Et',
                confirmIcon: 'bi bi-check-lg',
                onConfirm: function () {
                    $.post('/api/financing/offers/' + offer.id + '/accept')
                        .done(function (result) {
                            toastr.success(result.message || 'Teklif kabul edildi');
                            self.refreshOffers();
                            self.loadRequests();
                        })
                        .fail(function (xhr) {
                            toastr.error(xhr.responseJSON?.error || 'Islem basarisiz');
                        });
                }
            });
        };

        // Reject offer
        self.rejectOffer = function (offer) {
            self.selectedOffer(offer);
            self.rejectReason('');
            $('#rejectModal').modal('show');
        };

        self.confirmReject = function () {
            var offer = self.selectedOffer();
            if (!offer) return;

            $.ajax({
                url: '/api/financing/offers/' + offer.id + '/reject',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({ reason: self.rejectReason() || null })
            })
                .done(function (result) {
                    toastr.success(result.message || 'Teklif reddedildi');
                    $('#rejectModal').modal('hide');
                    self.refreshOffers();
                    self.loadRequests();
                })
                .fail(function (xhr) {
                    toastr.error(xhr.responseJSON?.error || 'Islem basarisiz');
                });
        };

        // Confirm transfer
        self.confirmTransfer = function (offer) {
            self.selectedOffer(offer);
            self.transferReference('');
            $('#transferModal').modal('show');
        };

        self.confirmTransferSubmit = function () {
            var offer = self.selectedOffer();
            if (!offer) return;

            $.ajax({
                url: '/api/financing/offers/' + offer.id + '/confirm-transfer',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({ transferReference: self.transferReference() || null })
            })
                .done(function (result) {
                    toastr.success(result.message || 'Transfer onayi kaydedildi');
                    $('#transferModal').modal('hide');
                    self.refreshOffers();
                    self.loadRequests();
                })
                .fail(function (xhr) {
                    toastr.error(xhr.responseJSON?.error || 'Islem basarisiz');
                });
        };

        // Confirm repayment
        self.confirmRepayment = function (offer) {
            self.selectedOffer(offer);
            self.repaymentReference('');
            $('#repaymentModal').modal('show');
        };

        self.confirmRepaymentSubmit = function () {
            var offer = self.selectedOffer();
            if (!offer) return;

            $.ajax({
                url: '/api/financing/offers/' + offer.id + '/confirm-repayment',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({ repaymentReference: self.repaymentReference() || null })
            })
                .done(function (result) {
                    toastr.success(result.message || 'Geri odeme kaydedildi');
                    $('#repaymentModal').modal('hide');
                    self.refreshOffers();
                    self.loadRequests();
                })
                .fail(function (xhr) {
                    toastr.error(xhr.responseJSON?.error || 'Islem basarisiz');
                });
        };

        // Refresh offers
        self.refreshOffers = function () {
            var request = self.selectedRequest();
            if (!request) return;

            $.get('/api/financing/my-requests/' + request.id)
                .done(function (data) {
                    self.offers(data.offers || []);
                    self.selectedRequest(data);
                });
        };

        // Init
        self.init = function () {
            self.loadRequests();
        };

        self.init();
    }

    // Apply bindings
    $(function () {
        ko.applyBindings(new FinancingViewModel(), document.getElementById('financing-app'));
    });
})();
