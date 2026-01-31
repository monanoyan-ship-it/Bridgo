// Dashboard/CompareProposals.js - Teklif Karsilastirma
(function () {
    'use strict';

    function CompareProposalsViewModel() {
        var self = this;

        // State
        self.isLoading = ko.observable(false);
        self.isProcessing = ko.observable(false);
        self.demands = ko.observableArray([]);
        self.inquiries = ko.observableArray([]);
        self.stats = ko.observable({});

        // Selected proposal for actions
        self.selectedProposal = ko.observable(null);
        self.proposalToReject = ko.observable(null);
        self.proposalToAccept = ko.observable(null);
        self.rejectReason = ko.observable('');
        self.rejectType = ko.observable('demand'); // 'demand' or 'inquiry'

        // Order creation
        self.orderProposal = ko.observable(null);
        self.addresses = ko.observableArray([]);
        self.selectedAddressId = ko.observable(null);
        self.orderNotes = ko.observable('');
        self.acceptTerms = ko.observable(false);

        // Negotiation state
        self.negotiationHistory = ko.observable(null);
        self.negotiationSummary = ko.observable(null);
        self.currentNegotiationResponseId = ko.observable(null);
        self.proposalToNegotiate = ko.observable(null);
        self.negotiationRejectReason = ko.observable('');

        // Start negotiation form
        self.startNegotiationForm = {
            unitPrice: ko.observable(null),
            totalPrice: ko.observable(null),
            currency: ko.observable('TRY'),
            quantity: ko.observable(null),
            leadTimeDays: ko.observable(null),
            notes: ko.observable('')
        };

        // Counter offer form
        self.counterOfferForm = {
            unitPrice: ko.observable(null),
            totalPrice: ko.observable(null),
            currency: ko.observable('TRY'),
            quantity: ko.observable(null),
            leadTimeDays: ko.observable(null),
            validUntil: ko.observable(null),
            notes: ko.observable('')
        };

        // Demand creation
        self.categories = ko.observableArray([]);
        self.countries = ko.observableArray([]);
        self.units = ko.observableArray([]);
        self.isSavingDemand = ko.observable(false);
        self.demandFormError = ko.observable('');
        self.demandForm = {
            title: ko.observable(''),
            description: ko.observable(''),
            categoryId: ko.observable(null),
            quantity: ko.observable(null),
            unitId: ko.observable(null),
            budgetMin: ko.observable(null),
            budgetMax: ko.observable(null),
            budgetCurrency: ko.observable('TRY'),
            desiredLeadTimeDays: ko.observable(null),
            expiresAt: ko.observable(null),
            countryId: ko.observable(null)
        };

        // Modals
        self.proposalModal = null;
        self.rejectModal = null;
        self.acceptModal = null;
        self.createOrderModal = null;
        self.createDemandModal = null;
        self.negotiationModal = null;
        self.counterOfferModal = null;
        self.startNegotiationModal = null;
        self.negotiationRejectModal = null;

        // Load all data
        self.loadData = function () {
            self.isLoading(true);

            Promise.all([
                fetch('/api/buyer/proposals/demands').then(function (r) { return r.json(); }),
                fetch('/api/buyer/proposals/inquiries').then(function (r) { return r.json(); }),
                fetch('/api/buyer/proposals/stats').then(function (r) { return r.json(); })
            ])
                .then(function (results) {
                    self.demands(results[0] || []);
                    self.inquiries(results[1] || []);
                    self.stats(results[2] || {});
                })
                .catch(function (err) {
                    console.error('Error loading data:', err);
                    toastr.error('Veriler yuklenirken hata olustu');
                })
                .finally(function () {
                    self.isLoading(false);
                });
        };

        // View proposal detail
        self.viewProposal = function (proposal) {
            // Mark as viewed if pending
            if (proposal.status === 1) {
                fetch('/api/buyer/proposals/demands/' + proposal.id + '/action', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ action: 'view' })
                }).then(function () {
                    proposal.status = 2;
                    proposal.statusName = 'Goruldu';
                    self.demands.valueHasMutated();
                });
            }

            self.selectedProposal(proposal);
            self.proposalModal.show();
        };

        // Shortlist proposal
        self.shortlistProposal = function (proposal) {
            self.isProcessing(true);

            fetch('/api/buyer/proposals/demands/' + proposal.id + '/action', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ action: 'shortlist' })
            })
                .then(function (response) {
                    if (response.ok) {
                        proposal.status = 3;
                        proposal.statusName = 'Kisa Liste';
                        self.demands.valueHasMutated();
                        self.loadStats();
                        toastr.success('Teklif kisa listeye eklendi');
                    } else {
                        return response.json().then(function (data) {
                            toastr.error(data.message || 'Islem basarisiz');
                        });
                    }
                })
                .catch(function (err) {
                    console.error('Error:', err);
                    toastr.error('Islem sirasinda hata olustu');
                })
                .finally(function () {
                    self.isProcessing(false);
                });
        };

        // Accept proposal - show confirmation
        self.acceptProposal = function (proposal) {
            self.proposalToAccept(proposal);
            self.rejectType('demand');
            self.acceptModal.show();
        };

        // Confirm accept
        self.confirmAccept = function () {
            var proposal = self.proposalToAccept();
            if (!proposal) return;

            self.isProcessing(true);

            var url = self.rejectType() === 'demand'
                ? '/api/buyer/proposals/demands/' + proposal.id + '/action'
                : '/api/buyer/proposals/inquiries/' + proposal.id + '/action';

            fetch(url, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ action: 'accept' })
            })
                .then(function (response) {
                    if (response.ok) {
                        self.acceptModal.hide();
                        toastr.success('Teklif kabul edildi');
                        self.loadData(); // Reload all data
                    } else {
                        return response.json().then(function (data) {
                            toastr.error(data.message || 'Islem basarisiz');
                        });
                    }
                })
                .catch(function (err) {
                    console.error('Error:', err);
                    toastr.error('Islem sirasinda hata olustu');
                })
                .finally(function () {
                    self.isProcessing(false);
                });
        };

        // Reject proposal - show modal
        self.rejectProposal = function (proposal) {
            self.proposalToReject(proposal);
            self.rejectType('demand');
            self.rejectReason('');
            self.rejectModal.show();
        };

        // Confirm reject
        self.confirmReject = function () {
            var proposal = self.proposalToReject();
            if (!proposal) return;

            self.isProcessing(true);

            var url = self.rejectType() === 'demand'
                ? '/api/buyer/proposals/demands/' + proposal.id + '/action'
                : '/api/buyer/proposals/inquiries/' + proposal.id + '/action';

            fetch(url, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    action: 'reject',
                    rejectionReason: self.rejectReason()
                })
            })
                .then(function (response) {
                    if (response.ok) {
                        self.rejectModal.hide();
                        toastr.success('Teklif reddedildi');
                        self.loadData(); // Reload all data
                    } else {
                        return response.json().then(function (data) {
                            toastr.error(data.message || 'Islem basarisiz');
                        });
                    }
                })
                .catch(function (err) {
                    console.error('Error:', err);
                    toastr.error('Islem sirasinda hata olustu');
                })
                .finally(function () {
                    self.isProcessing(false);
                });
        };

        // Accept inquiry proposal
        self.acceptInquiryProposal = function (proposal) {
            self.proposalToAccept(proposal);
            self.rejectType('inquiry');
            self.acceptModal.show();
        };

        // Reject inquiry proposal
        self.rejectInquiryProposal = function (proposal) {
            self.proposalToReject(proposal);
            self.rejectType('inquiry');
            self.rejectReason('');
            self.rejectModal.show();
        };

        // Reload stats only
        self.loadStats = function () {
            fetch('/api/buyer/proposals/stats')
                .then(function (r) { return r.json(); })
                .then(function (data) {
                    self.stats(data || {});
                });
        };

        // ============================================
        // ORDER CREATION
        // ============================================

        // Load addresses for order
        self.loadAddresses = function () {
            fetch('/api/company/addresses')
                .then(function (r) { return r.json(); })
                .then(function (data) {
                    self.addresses(data || []);
                })
                .catch(function (err) {
                    console.error('Error loading addresses:', err);
                });
        };

        // Show create order modal
        self.showCreateOrder = function (proposal) {
            self.orderProposal(proposal);
            self.selectedAddressId(null);
            self.orderNotes('');
            self.acceptTerms(false);
            self.loadAddresses();
            self.createOrderModal.show();
        };

        // Create order from accepted proposal
        self.createOrder = function () {
            var proposal = self.orderProposal();
            if (!proposal) return;

            if (!self.acceptTerms()) {
                toastr.warning('Lutfen siparis kosullarini kabul edin.');
                return;
            }

            self.isProcessing(true);

            var dto = {
                demandId: proposal.demandId,
                responseId: proposal.id,
                shippingAddressId: self.selectedAddressId(),
                billingAddressId: self.selectedAddressId(), // Same as shipping for now
                notes: self.orderNotes()
            };

            fetch('/api/orders/from-demand', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(dto)
            })
                .then(function (response) {
                    if (response.ok) {
                        return response.json().then(function (order) {
                            self.createOrderModal.hide();
                            toastr.success('Siparis olusturuldu: ' + order.orderNumber);
                            // Redirect to orders page or show order details
                            setTimeout(function () {
                                window.location.href = '/Dashboard/MyOrders';
                            }, 1500);
                        });
                    } else {
                        return response.json().then(function (data) {
                            toastr.error(data.message || 'Siparis olusturulamadi');
                        });
                    }
                })
                .catch(function (err) {
                    console.error('Error creating order:', err);
                    toastr.error('Siparis olusturulurken hata olustu');
                })
                .finally(function () {
                    self.isProcessing(false);
                });
        };

        // ============================================
        // DEMAND CREATION
        // ============================================

        self.loadCategories = function () {
            fetch('/api/catalog/categories')
                .then(function (r) { return r.json(); })
                .then(function (data) {
                    // Flatten tree structure for select
                    var flat = [];
                    function flatten(items, prefix) {
                        items.forEach(function (item) {
                            flat.push({ id: item.id, name: prefix + item.name });
                            if (item.children && item.children.length > 0) {
                                flatten(item.children, prefix + item.name + ' > ');
                            }
                        });
                    }
                    flatten(data || [], '');
                    self.categories(flat);
                })
                .catch(function (err) {
                    console.error('Error loading categories:', err);
                });
        };

        self.loadCountries = function () {
            fetch('/api/localization/countries')
                .then(function (r) { return r.json(); })
                .then(function (data) {
                    self.countries(data || []);
                })
                .catch(function (err) {
                    console.error('Error loading countries:', err);
                });
        };

        self.loadUnits = function () {
            fetch('/api/types/units')
                .then(function (r) { return r.json(); })
                .then(function (data) {
                    self.units(data || []);
                })
                .catch(function (err) {
                    console.error('Error loading units:', err);
                });
        };

        self.openCreateDemandModal = function () {
            self.resetDemandForm();
            self.demandFormError('');
            self.createDemandModal.show();
        };

        self.resetDemandForm = function () {
            self.demandForm.title('');
            self.demandForm.description('');
            self.demandForm.categoryId(null);
            self.demandForm.quantity(null);
            self.demandForm.unitId(null);
            self.demandForm.budgetMin(null);
            self.demandForm.budgetMax(null);
            self.demandForm.budgetCurrency('TRY');
            self.demandForm.desiredLeadTimeDays(null);
            self.demandForm.expiresAt(null);
            self.demandForm.countryId(null);
        };

        self.saveDraftDemand = function () {
            self.saveDemand(1); // Draft status
        };

        self.publishDemand = function () {
            self.saveDemand(2); // Active status
        };

        self.saveDemand = function (status) {
            self.demandFormError('');

            if (!self.demandForm.title()) {
                self.demandFormError('Baslik zorunludur.');
                return;
            }
            if (!self.demandForm.description()) {
                self.demandFormError('Aciklama zorunludur.');
                return;
            }

            self.isSavingDemand(true);

            // Get unit name from selected unit
            var selectedUnit = self.units().find(function(u) { return u.id === self.demandForm.unitId(); });
            var unitName = selectedUnit ? selectedUnit.name : '';

            var data = {
                title: self.demandForm.title(),
                description: self.demandForm.description(),
                categoryId: self.demandForm.categoryId(),
                quantity: self.demandForm.quantity() ? parseInt(self.demandForm.quantity()) : null,
                unit: unitName,
                budgetMin: self.demandForm.budgetMin() ? parseFloat(self.demandForm.budgetMin()) : null,
                budgetMax: self.demandForm.budgetMax() ? parseFloat(self.demandForm.budgetMax()) : null,
                budgetCurrency: self.demandForm.budgetCurrency(),
                desiredLeadTimeDays: self.demandForm.desiredLeadTimeDays() ? parseInt(self.demandForm.desiredLeadTimeDays()) : null,
                expiresAt: self.demandForm.expiresAt() || null,
                countryId: self.demandForm.countryId(),
                status: status
            };

            fetch('/api/demands', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(data)
            })
                .then(function (response) {
                    if (response.ok) {
                        self.createDemandModal.hide();
                        toastr.success(status === 1 ? 'Talep taslak olarak kaydedildi.' : 'Talep yayinlandi.');
                        self.loadData(); // Reload to see new demand
                    } else {
                        return response.json().then(function (err) {
                            self.demandFormError(err.message || 'Talep kaydedilemedi.');
                        });
                    }
                })
                .catch(function (err) {
                    console.error('Error saving demand:', err);
                    self.demandFormError('Talep kaydedilirken hata olustu.');
                })
                .finally(function () {
                    self.isSavingDemand(false);
                });
        };

        // ============================================
        // NEGOTIATION FUNCTIONS
        // ============================================

        // Format remaining time
        self.formatTimeRemaining = function (expiresAt) {
            if (!expiresAt) return '-';

            var now = new Date();
            var expires = new Date(expiresAt);
            var diff = expires - now;

            if (diff <= 0) return 'Suresi doldu';

            var hours = Math.floor(diff / (1000 * 60 * 60));
            var minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));

            if (hours > 24) {
                var days = Math.floor(hours / 24);
                return days + ' gun ' + (hours % 24) + ' saat';
            }
            return hours + ' saat ' + minutes + ' dk';
        };

        // Start negotiation - show modal
        self.startNegotiation = function (proposal) {
            self.proposalToNegotiate(proposal);
            // Pre-fill with slightly lower values
            self.startNegotiationForm.unitPrice(proposal.unitPrice ? (proposal.unitPrice * 0.9).toFixed(2) : null);
            self.startNegotiationForm.totalPrice(proposal.totalPrice ? (proposal.totalPrice * 0.9).toFixed(2) : null);
            self.startNegotiationForm.currency(proposal.currency || 'TRY');
            self.startNegotiationForm.quantity(proposal.quantity || null);
            self.startNegotiationForm.leadTimeDays(proposal.leadTimeDays || null);
            self.startNegotiationForm.notes('');
            self.startNegotiationModal.show();
        };

        // Confirm start negotiation
        self.confirmStartNegotiation = function () {
            var proposal = self.proposalToNegotiate();
            if (!proposal) return;

            if (!self.startNegotiationForm.unitPrice()) {
                toastr.warning('Lutfen birim fiyat girin.');
                return;
            }

            self.isProcessing(true);

            var dto = {
                demandResponseId: proposal.id,
                unitPrice: parseFloat(self.startNegotiationForm.unitPrice()),
                totalPrice: self.startNegotiationForm.totalPrice() ? parseFloat(self.startNegotiationForm.totalPrice()) : null,
                currency: self.startNegotiationForm.currency(),
                quantity: self.startNegotiationForm.quantity() ? parseInt(self.startNegotiationForm.quantity()) : null,
                leadTimeDays: self.startNegotiationForm.leadTimeDays() ? parseInt(self.startNegotiationForm.leadTimeDays()) : null,
                notes: self.startNegotiationForm.notes()
            };

            fetch('/api/negotiations/start', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(dto)
            })
                .then(function (response) {
                    if (response.ok) {
                        self.startNegotiationModal.hide();
                        toastr.success('Pazarlik baslatildi!');
                        self.loadData(); // Reload to see updated status
                    } else {
                        return response.json().then(function (data) {
                            toastr.error(data.message || 'Pazarlik baslatilamadi');
                        });
                    }
                })
                .catch(function (err) {
                    console.error('Error starting negotiation:', err);
                    toastr.error('Islem sirasinda hata olustu');
                })
                .finally(function () {
                    self.isProcessing(false);
                });
        };

        // Open negotiation modal (for ongoing negotiations)
        self.openNegotiationModal = function (proposal) {
            self.currentNegotiationResponseId(proposal.id);
            self.loadNegotiationData(proposal.id);
            self.negotiationModal.show();
        };

        // Load negotiation history and summary
        self.loadNegotiationData = function (responseId) {
            Promise.all([
                fetch('/api/negotiations/' + responseId + '/history').then(function (r) { return r.ok ? r.json() : null; }),
                fetch('/api/negotiations/' + responseId + '/summary').then(function (r) { return r.ok ? r.json() : null; })
            ])
                .then(function (results) {
                    self.negotiationHistory(results[0]);
                    self.negotiationSummary(results[1]);
                })
                .catch(function (err) {
                    console.error('Error loading negotiation data:', err);
                    toastr.error('Pazarlik bilgileri yuklenemedi');
                });
        };

        // Open counter offer modal
        self.openCounterOfferModal = function () {
            var summary = self.negotiationSummary();
            if (!summary) return;

            // Pre-fill with current values
            self.counterOfferForm.unitPrice(summary.currentPrice || null);
            self.counterOfferForm.totalPrice(null);
            self.counterOfferForm.currency(summary.currency || 'TRY');
            self.counterOfferForm.quantity(summary.quantity || null);
            self.counterOfferForm.leadTimeDays(summary.leadTimeDays || null);
            self.counterOfferForm.validUntil(null);
            self.counterOfferForm.notes('');

            self.counterOfferModal.show();
        };

        // Submit counter offer
        self.submitCounterOffer = function () {
            var responseId = self.currentNegotiationResponseId();
            if (!responseId) return;

            if (!self.counterOfferForm.unitPrice()) {
                toastr.warning('Lutfen birim fiyat girin.');
                return;
            }

            self.isProcessing(true);

            var dto = {
                demandResponseId: responseId,
                unitPrice: parseFloat(self.counterOfferForm.unitPrice()),
                totalPrice: self.counterOfferForm.totalPrice() ? parseFloat(self.counterOfferForm.totalPrice()) : null,
                currency: self.counterOfferForm.currency(),
                quantity: self.counterOfferForm.quantity() ? parseInt(self.counterOfferForm.quantity()) : null,
                leadTimeDays: self.counterOfferForm.leadTimeDays() ? parseInt(self.counterOfferForm.leadTimeDays()) : null,
                validUntil: self.counterOfferForm.validUntil() || null,
                notes: self.counterOfferForm.notes()
            };

            fetch('/api/negotiations/counter', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(dto)
            })
                .then(function (response) {
                    if (response.ok) {
                        self.counterOfferModal.hide();
                        toastr.success('Karsi teklif gonderildi!');
                        self.loadNegotiationData(responseId); // Reload negotiation data
                    } else {
                        return response.json().then(function (data) {
                            toastr.error(data.message || 'Teklif gonderilemedi');
                        });
                    }
                })
                .catch(function (err) {
                    console.error('Error submitting counter offer:', err);
                    toastr.error('Islem sirasinda hata olustu');
                })
                .finally(function () {
                    self.isProcessing(false);
                });
        };

        // Accept negotiation (accept current round)
        self.acceptNegotiation = function () {
            var summary = self.negotiationSummary();
            if (!summary || !summary.lastRoundId) {
                toastr.error('Kabul edilecek tur bulunamadi');
                return;
            }

            self.isProcessing(true);

            fetch('/api/negotiations/rounds/' + summary.lastRoundId + '/accept', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' }
            })
                .then(function (response) {
                    if (response.ok) {
                        self.negotiationModal.hide();
                        toastr.success('Teklif kabul edildi!');
                        self.loadData(); // Reload all data
                    } else {
                        return response.json().then(function (data) {
                            toastr.error(data.message || 'Kabul islemi basarisiz');
                        });
                    }
                })
                .catch(function (err) {
                    console.error('Error accepting negotiation:', err);
                    toastr.error('Islem sirasinda hata olustu');
                })
                .finally(function () {
                    self.isProcessing(false);
                });
        };

        // Reject negotiation - show confirm modal
        self.rejectNegotiation = function () {
            self.negotiationRejectReason('');
            self.negotiationRejectModal.show();
        };

        // Confirm reject negotiation
        self.confirmRejectNegotiation = function () {
            var summary = self.negotiationSummary();
            if (!summary || !summary.lastRoundId) {
                toastr.error('Reddedilecek tur bulunamadi');
                return;
            }

            self.isProcessing(true);

            fetch('/api/negotiations/rounds/' + summary.lastRoundId + '/reject', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ reason: self.negotiationRejectReason() })
            })
                .then(function (response) {
                    if (response.ok) {
                        self.negotiationRejectModal.hide();
                        self.negotiationModal.hide();
                        toastr.success('Pazarlik reddedildi');
                        self.loadData(); // Reload all data
                    } else {
                        return response.json().then(function (data) {
                            toastr.error(data.message || 'Red islemi basarisiz');
                        });
                    }
                })
                .catch(function (err) {
                    console.error('Error rejecting negotiation:', err);
                    toastr.error('Islem sirasinda hata olustu');
                })
                .finally(function () {
                    self.isProcessing(false);
                });
        };

        // Init
        self.init = function () {
            var proposalModalEl = document.getElementById('proposalModal');
            var rejectModalEl = document.getElementById('rejectModal');
            var acceptModalEl = document.getElementById('acceptModal');
            var createOrderModalEl = document.getElementById('createOrderModal');
            var createDemandModalEl = document.getElementById('createDemandModal');

            if (proposalModalEl) {
                self.proposalModal = new bootstrap.Modal(proposalModalEl);
            }
            if (rejectModalEl) {
                self.rejectModal = new bootstrap.Modal(rejectModalEl);
            }
            if (acceptModalEl) {
                self.acceptModal = new bootstrap.Modal(acceptModalEl);
            }
            if (createOrderModalEl) {
                self.createOrderModal = new bootstrap.Modal(createOrderModalEl);
            }
            if (createDemandModalEl) {
                self.createDemandModal = new bootstrap.Modal(createDemandModalEl);
            }

            // Negotiation modals
            var negotiationModalEl = document.getElementById('negotiationModal');
            var counterOfferModalEl = document.getElementById('counterOfferModal');
            var startNegotiationModalEl = document.getElementById('startNegotiationModal');
            var negotiationRejectModalEl = document.getElementById('negotiationRejectModal');

            if (negotiationModalEl) {
                self.negotiationModal = new bootstrap.Modal(negotiationModalEl);
            }
            if (counterOfferModalEl) {
                self.counterOfferModal = new bootstrap.Modal(counterOfferModalEl);
            }
            if (startNegotiationModalEl) {
                self.startNegotiationModal = new bootstrap.Modal(startNegotiationModalEl);
            }
            if (negotiationRejectModalEl) {
                self.negotiationRejectModal = new bootstrap.Modal(negotiationRejectModalEl);
            }

            // Load categories, countries and units for demand creation
            self.loadCategories();
            self.loadCountries();
            self.loadUnits();

            self.loadData();
        };

        self.init();
    }

    // Apply bindings
    $(function () {
        ko.applyBindings(new CompareProposalsViewModel(), document.getElementById('compare-proposals-app'));
    });
})();
