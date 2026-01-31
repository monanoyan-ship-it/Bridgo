$(function () {
    function InvestmentViewModel() {
        var self = this;

        // Tab state
        self.currentTab = ko.observable('opportunities');

        // Data
        self.opportunities = ko.observableArray([]);
        self.myOffers = ko.observableArray([]);

        // Filters
        self.searchTerm = ko.observable('');
        self.financingTypeFilter = ko.observable('');

        // UI State
        self.isLoading = ko.observable(false);
        self.isSubmitting = ko.observable(false);

        // Selected item
        self.selectedOpportunity = ko.observable(null);
        self.detailData = ko.observable(null);

        // Offer form
        self.offerForm = ko.observable({
            offeredAmount: 0,
            interestRate: 0,
            notes: '',
            validUntil: ''
        });

        // Computed: Filtered opportunities
        self.filteredOpportunities = ko.computed(function () {
            var items = self.opportunities();
            var search = (self.searchTerm() || '').toLowerCase();
            var type = self.financingTypeFilter();

            if (search) {
                items = items.filter(function (item) {
                    return (item.title || '').toLowerCase().indexOf(search) > -1 ||
                           (item.description || '').toLowerCase().indexOf(search) > -1 ||
                           (item.requesterCompanyName || '').toLowerCase().indexOf(search) > -1;
                });
            }

            if (type) {
                items = items.filter(function (item) {
                    return item.financingType == type;
                });
            }

            return items;
        });

        // Computed: Calculate repayment
        self.calculatedRepayment = ko.computed(function () {
            var form = self.offerForm();
            var opportunity = self.selectedOpportunity();
            if (!form || !opportunity || !form.offeredAmount || !form.interestRate) return 0;

            var monthlyRate = form.interestRate / 100;
            var months = opportunity.durationDays / 30;
            var interest = form.offeredAmount * monthlyRate * months;
            return Math.round((parseFloat(form.offeredAmount) + interest) * 100) / 100;
        });

        self.calculatedInterest = ko.computed(function () {
            var form = self.offerForm();
            var opportunity = self.selectedOpportunity();
            if (!form || !opportunity || !form.offeredAmount || !form.interestRate) return 0;

            var monthlyRate = form.interestRate / 100;
            var months = opportunity.durationDays / 30;
            return Math.round(form.offeredAmount * monthlyRate * months * 100) / 100;
        });

        // Tab change handler
        self.currentTab.subscribe(function (tab) {
            if (tab === 'opportunities') {
                self.loadOpportunities();
            } else if (tab === 'offers') {
                self.loadMyOffers();
            }
        });

        // Load opportunities
        self.loadOpportunities = function () {
            self.isLoading(true);
            $.get('/api/investment/opportunities')
                .done(function (data) {
                    self.opportunities(data || []);
                })
                .fail(function (xhr) {
                    toastr.error('Firsatlar yuklenemedi');
                })
                .always(function () {
                    self.isLoading(false);
                });
        };

        // Load my offers
        self.loadMyOffers = function () {
            self.isLoading(true);
            $.get('/api/investment/my-offers')
                .done(function (data) {
                    self.myOffers(data || []);
                })
                .fail(function (xhr) {
                    toastr.error('Teklifler yuklenemedi');
                })
                .always(function () {
                    self.isLoading(false);
                });
        };

        // Open offer modal
        self.openOfferModal = function (opportunity) {
            self.selectedOpportunity(opportunity);
            self.offerForm({
                offeredAmount: opportunity.remainingAmount,
                interestRate: opportunity.maxInterestRate || 2,
                notes: '',
                validUntil: ''
            });
            new bootstrap.Modal('#offerModal').show();
        };

        // Open offer modal from detail
        self.openOfferModalFromDetail = function () {
            var detail = self.detailData();
            if (detail) {
                bootstrap.Modal.getInstance(document.getElementById('detailModal')).hide();
                setTimeout(function () {
                    self.openOfferModal(detail);
                }, 300);
            }
        };

        // View details
        self.viewDetails = function (opportunity) {
            $.get('/api/investment/opportunities/' + opportunity.id)
                .done(function (data) {
                    self.detailData(data);
                    new bootstrap.Modal('#detailModal').show();
                })
                .fail(function (xhr) {
                    toastr.error('Detay yuklenemedi');
                });
        };

        // Submit offer
        self.submitOffer = function () {
            var opportunity = self.selectedOpportunity();
            var form = self.offerForm();

            if (!form.offeredAmount || form.offeredAmount <= 0) {
                toastr.warning('Teklif tutari girin');
                return;
            }

            if (!form.interestRate || form.interestRate < 0) {
                toastr.warning('Faiz orani girin');
                return;
            }

            if (opportunity.maxInterestRate && form.interestRate > opportunity.maxInterestRate) {
                toastr.warning('Faiz orani maksimum %' + opportunity.maxInterestRate + ' olabilir');
                return;
            }

            if (form.offeredAmount > opportunity.remainingAmount) {
                toastr.warning('Teklif tutari kalan tutardan fazla olamaz');
                return;
            }

            self.isSubmitting(true);
            $.ajax({
                url: '/api/investment/opportunities/' + opportunity.id + '/offer',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({
                    offeredAmount: parseFloat(form.offeredAmount),
                    interestRate: parseFloat(form.interestRate),
                    notes: form.notes,
                    validUntil: form.validUntil || null
                })
            })
            .done(function (result) {
                toastr.success(result.message || 'Teklif gonderildi');
                bootstrap.Modal.getInstance(document.getElementById('offerModal')).hide();
                self.loadOpportunities();
            })
            .fail(function (xhr) {
                var error = xhr.responseJSON?.error || 'Teklif gonderilemedi';
                toastr.error(error);
            })
            .always(function () {
                self.isSubmitting(false);
            });
        };

        // Withdraw offer
        self.withdrawOffer = function (offer) {
            showConfirmModal({
                title: 'Teklifi Geri Cek',
                message: 'Bu teklifi geri cekmek istediginizden emin misiniz?',
                type: 'warning',
                confirmText: 'Geri Cek',
                confirmIcon: 'bi bi-x-circle',
                onConfirm: function () {
                    $.post('/api/investment/offers/' + offer.id + '/withdraw')
                        .done(function (result) {
                            toastr.success(result.message || 'Teklif geri cekildi');
                            self.loadMyOffers();
                        })
                        .fail(function (xhr) {
                            var error = xhr.responseJSON?.error || 'Islem basarisiz';
                            toastr.error(error);
                        });
                }
            });
        };

        // Helper: Get financing type CSS class
        self.getFinancingTypeCss = function (type) {
            switch (type) {
                case 1: return 'bg-primary'; // Invoice
                case 2: return 'bg-info'; // Order
                case 3: return 'bg-success'; // Project
                default: return 'bg-secondary';
            }
        };

        // Helper: Get risk score CSS class
        self.getRiskScoreCss = function (score) {
            if (!score) return '';
            if (score <= 30) return 'text-success';
            if (score <= 60) return 'text-warning';
            return 'text-danger';
        };

        // Initial load
        self.loadOpportunities();
    }

    ko.applyBindings(new InvestmentViewModel(), document.getElementById('investment-app'));
});
