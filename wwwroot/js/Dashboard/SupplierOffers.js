function SupplierOffersViewModel() {
    var self = this;

    // State
    self.isLoading = ko.observable(false);
    self.activeTab = ko.observable('incoming');

    // ========================================
    // TAB 1: GELEN TALEPLER (Product Inquiries)
    // ========================================
    self.incomingInquiries = ko.observableArray([]);
    self.unreadInquiryCount = ko.observable(0);
    self.incomingPage = ko.observable(1);
    self.incomingTotalPages = ko.observable(1);
    self.incomingFilter = {
        search: ko.observable(''),
        status: ko.observable(''),
        isRead: ko.observable('')
    };

    // Selected inquiry for detail modal
    self.selectedInquiry = ko.observable(null);
    self.respondingInquiry = ko.observable(null);

    // Response form
    self.responseForm = {
        unitPrice: ko.observable(''),
        currency: ko.observable('TRY'),
        offeredQuantity: ko.observable(''),
        unitId: ko.observable(null),
        leadTimeDays: ko.observable(''),
        validUntil: ko.observable(''),
        notes: ko.observable('')
    };
    self.responseError = ko.observable('');
    self.isSubmittingResponse = ko.observable(false);
    self.units = ko.observableArray([]);

    // ========================================
    // TAB 2: MY OFFERS (DemandResponses)
    // ========================================
    self.myOffers = ko.observableArray([]);
    self.isLoadingMyOffers = ko.observable(false);
    self.myOffersFilter = ko.observable('all'); // 'all', 'negotiating', 'myturn'
    self.activeNegotiationsCount = ko.observable(0);

    // Seller negotiation state
    self.currentSellerNegotiationResponseId = ko.observable(null);
    self.sellerNegotiationHistory = ko.observable(null);
    self.sellerNegotiationSummary = ko.observable(null);
    self.sellerNegotiationRejectReason = ko.observable('');
    self.isSubmittingSellerCounter = ko.observable(false);

    // Seller counter offer form
    self.sellerCounterOfferForm = {
        unitPrice: ko.observable(null),
        totalPrice: ko.observable(null),
        currency: ko.observable('TRY'),
        quantity: ko.observable(null),
        leadTimeDays: ko.observable(null),
        notes: ko.observable('')
    };

    // ========================================
    // TAB 3: MUHTEMEL TALEPLER (Subscribed Demands)
    // ========================================
    self.potentialDemands = ko.observableArray([]);
    self.potentialDemandsCount = ko.observable(0);
    self.isLoadingPotential = ko.observable(false);

    // ========================================
    // TAB 3: TAKIP ETTIGIM KATEGORILER
    // ========================================
    self.subscriptions = ko.observableArray([]);
    self.allCategories = ko.observableArray([]);
    self.isLoadingSubscriptions = ko.observable(false);
    self.newSubscription = {
        categoryId: ko.observable(null),
        keywordFilter: ko.observable(''),
        notifyByEmail: ko.observable(true),
        notifyInApp: ko.observable(true)
    };

    // Debounced search for incoming inquiries
    var searchTimeout;
    self.incomingFilter.search.subscribe(function() {
        clearTimeout(searchTimeout);
        searchTimeout = setTimeout(function() {
            self.incomingPage(1);
            self.loadIncomingInquiries();
        }, 300);
    });

    self.incomingFilter.status.subscribe(function() {
        self.incomingPage(1);
        self.loadIncomingInquiries();
    });

    self.incomingFilter.isRead.subscribe(function() {
        self.incomingPage(1);
        self.loadIncomingInquiries();
    });

    // Tab switching
    self.setTab = function(tab) {
        self.activeTab(tab);
        self.loadData();
    };

    // Status helpers for Inquiries
    self.getInquiryStatusClass = function(status) {
        switch(status) {
            case 0: return 'bg-danger';      // New
            case 1: return 'bg-info';        // Read
            case 2: return 'bg-success';     // Responded
            case 3: return 'bg-secondary';   // Closed
            default: return 'bg-secondary';
        }
    };

    self.getInquiryStatusText = function(status) {
        switch(status) {
            case 0: return 'Yeni';
            case 1: return 'Okundu';
            case 2: return 'Yanit Verildi';
            case 3: return 'Kapandi';
            default: return 'Bilinmiyor';
        }
    };

    // Load data based on active tab
    self.loadData = function() {
        switch(self.activeTab()) {
            case 'incoming':
                self.loadIncomingInquiries();
                break;
            case 'myoffers':
                self.loadMyOffers();
                break;
            case 'potential':
                self.loadPotentialDemands();
                break;
            case 'subscriptions':
                self.loadSubscriptions();
                self.loadAllCategories();
                break;
        }
    };

    // Set my offers filter
    self.setMyOffersFilter = function(filter) {
        self.myOffersFilter(filter);
        self.loadMyOffers();
    };

    // Format remaining time helper
    self.formatTimeRemaining = function(expiresAt) {
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

    // ========================================
    // LOAD FUNCTIONS
    // ========================================

    // Load incoming product inquiries
    self.loadIncomingInquiries = function() {
        self.isLoading(true);

        var params = new URLSearchParams();
        params.append('page', self.incomingPage());
        params.append('pageSize', 10);
        if (self.incomingFilter.search()) params.append('search', self.incomingFilter.search());
        if (self.incomingFilter.status()) params.append('status', self.incomingFilter.status());
        if (self.incomingFilter.isRead()) params.append('isRead', self.incomingFilter.isRead());

        fetch('/api/product-inquiries/incoming?' + params.toString())
            .then(function(r) { return r.json(); })
            .then(function(data) {
                self.incomingInquiries(data.items || []);
                self.incomingTotalPages(data.totalPages || 1);
            })
            .catch(function(err) {
                console.error('Error loading incoming inquiries:', err);
                self.incomingInquiries([]);
            })
            .finally(function() {
                self.isLoading(false);
            });

        // Also load unread count
        self.loadUnreadCount();
    };

    self.loadUnreadCount = function() {
        fetch('/api/product-inquiries/incoming/unread-count')
            .then(function(r) { return r.json(); })
            .then(function(data) {
                self.unreadInquiryCount(data.count || 0);
            })
            .catch(function(err) {
                console.error('Error loading unread count:', err);
            });
    };

    // Load potential demands (from subscribed categories)
    self.loadPotentialDemands = function() {
        self.isLoadingPotential(true);

        fetch('/api/demands/subscribed')
            .then(function(r) { return r.json(); })
            .then(function(data) {
                self.potentialDemands(data.items || data || []);
                self.potentialDemandsCount(data.totalCount || data.length || 0);
            })
            .catch(function(err) {
                console.error('Error loading potential demands:', err);
                self.potentialDemands([]);
                self.potentialDemandsCount(0);
            })
            .finally(function() {
                self.isLoadingPotential(false);
            });
    };

    // Load subscriptions
    self.loadSubscriptions = function() {
        self.isLoadingSubscriptions(true);

        fetch('/api/suppliers/subscriptions')
            .then(function(r) { return r.json(); })
            .then(function(data) {
                self.subscriptions(data || []);
            })
            .catch(function(err) {
                console.error('Error loading subscriptions:', err);
                self.subscriptions([]);
            })
            .finally(function() {
                self.isLoadingSubscriptions(false);
            });
    };

    // Load all categories for dropdown
    self.loadAllCategories = function() {
        fetch('/api/products/categories/select')
            .then(function(r) { return r.json(); })
            .then(function(data) {
                self.allCategories(data || []);
            })
            .catch(function(err) {
                console.error('Error loading categories:', err);
            });
    };

    // Load units for dropdown
    self.loadUnits = function() {
        fetch('/api/types/units')
            .then(function(r) { return r.json(); })
            .then(function(data) {
                self.units(data || []);
            })
            .catch(function(err) {
                console.error('Error loading units:', err);
            });
    };

    // Load my demand responses (offers I've sent)
    self.loadMyOffers = function() {
        self.isLoadingMyOffers(true);

        var params = new URLSearchParams();
        if (self.myOffersFilter() === 'negotiating') {
            params.append('status', '6'); // Negotiating status
        } else if (self.myOffersFilter() === 'myturn') {
            params.append('isMyTurn', 'true');
        }

        fetch('/api/demands/my-responses?' + params.toString())
            .then(function(r) { return r.json(); })
            .then(function(data) {
                self.myOffers(data.items || data || []);
                // Count active negotiations where it's my turn
                var myTurnCount = (data.items || data || []).filter(function(o) {
                    return o.status === 6 && o.isMyTurn;
                }).length;
                self.activeNegotiationsCount(myTurnCount);
            })
            .catch(function(err) {
                console.error('Error loading my offers:', err);
                self.myOffers([]);
            })
            .finally(function() {
                self.isLoadingMyOffers(false);
            });
    };

    // ========================================
    // SELLER NEGOTIATION FUNCTIONS
    // ========================================

    // Open seller negotiation modal
    self.openSellerNegotiationModal = function(offer) {
        self.currentSellerNegotiationResponseId(offer.id);
        self.loadSellerNegotiationData(offer.id);
        var modal = new bootstrap.Modal(document.getElementById('sellerNegotiationModal'));
        modal.show();
    };

    // Load seller negotiation data
    self.loadSellerNegotiationData = function(responseId) {
        Promise.all([
            fetch('/api/negotiations/' + responseId + '/history').then(function(r) { return r.ok ? r.json() : null; }),
            fetch('/api/negotiations/' + responseId + '/summary').then(function(r) { return r.ok ? r.json() : null; })
        ])
            .then(function(results) {
                self.sellerNegotiationHistory(results[0]);
                self.sellerNegotiationSummary(results[1]);
            })
            .catch(function(err) {
                console.error('Error loading negotiation data:', err);
                toastr.error('Pazarlik bilgileri yuklenemedi');
            });
    };

    // Open seller counter offer modal
    self.openSellerCounterOfferModal = function() {
        var summary = self.sellerNegotiationSummary();
        if (!summary) return;

        // Pre-fill with current values
        self.sellerCounterOfferForm.unitPrice(summary.currentPrice || null);
        self.sellerCounterOfferForm.totalPrice(null);
        self.sellerCounterOfferForm.currency(summary.currency || 'TRY');
        self.sellerCounterOfferForm.quantity(summary.quantity || null);
        self.sellerCounterOfferForm.leadTimeDays(summary.leadTimeDays || null);
        self.sellerCounterOfferForm.notes('');

        var modal = new bootstrap.Modal(document.getElementById('sellerCounterOfferModal'));
        modal.show();
    };

    // Submit seller counter offer
    self.submitSellerCounterOffer = function() {
        var responseId = self.currentSellerNegotiationResponseId();
        if (!responseId) return;

        if (!self.sellerCounterOfferForm.unitPrice()) {
            toastr.warning('Lutfen birim fiyat girin.');
            return;
        }

        self.isSubmittingSellerCounter(true);

        var dto = {
            demandResponseId: responseId,
            unitPrice: parseFloat(self.sellerCounterOfferForm.unitPrice()),
            totalPrice: self.sellerCounterOfferForm.totalPrice() ? parseFloat(self.sellerCounterOfferForm.totalPrice()) : null,
            currency: self.sellerCounterOfferForm.currency(),
            quantity: self.sellerCounterOfferForm.quantity() ? parseInt(self.sellerCounterOfferForm.quantity()) : null,
            leadTimeDays: self.sellerCounterOfferForm.leadTimeDays() ? parseInt(self.sellerCounterOfferForm.leadTimeDays()) : null,
            notes: self.sellerCounterOfferForm.notes()
        };

        fetch('/api/negotiations/counter', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(dto)
        })
            .then(function(response) {
                if (response.ok) {
                    var counterModal = bootstrap.Modal.getInstance(document.getElementById('sellerCounterOfferModal'));
                    if (counterModal) counterModal.hide();
                    toastr.success('Karsi teklif gonderildi!');
                    self.loadSellerNegotiationData(responseId);
                    self.loadMyOffers(); // Refresh list
                } else {
                    return response.json().then(function(data) {
                        toastr.error(data.message || 'Teklif gonderilemedi');
                    });
                }
            })
            .catch(function(err) {
                console.error('Error submitting counter offer:', err);
                toastr.error('Islem sirasinda hata olustu');
            })
            .finally(function() {
                self.isSubmittingSellerCounter(false);
            });
    };

    // Accept seller negotiation
    self.acceptSellerNegotiation = function() {
        var summary = self.sellerNegotiationSummary();
        if (!summary || !summary.lastRoundId) {
            toastr.error('Kabul edilecek tur bulunamadi');
            return;
        }

        self.isSubmittingSellerCounter(true);

        fetch('/api/negotiations/rounds/' + summary.lastRoundId + '/accept', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' }
        })
            .then(function(response) {
                if (response.ok) {
                    var modal = bootstrap.Modal.getInstance(document.getElementById('sellerNegotiationModal'));
                    if (modal) modal.hide();
                    toastr.success('Teklif kabul edildi!');
                    self.loadMyOffers();
                } else {
                    return response.json().then(function(data) {
                        toastr.error(data.message || 'Kabul islemi basarisiz');
                    });
                }
            })
            .catch(function(err) {
                console.error('Error accepting negotiation:', err);
                toastr.error('Islem sirasinda hata olustu');
            })
            .finally(function() {
                self.isSubmittingSellerCounter(false);
            });
    };

    // Reject seller negotiation - show confirm modal
    self.rejectSellerNegotiation = function() {
        self.sellerNegotiationRejectReason('');
        var modal = new bootstrap.Modal(document.getElementById('sellerNegotiationRejectModal'));
        modal.show();
    };

    // Confirm reject seller negotiation
    self.confirmSellerRejectNegotiation = function() {
        var summary = self.sellerNegotiationSummary();
        if (!summary || !summary.lastRoundId) {
            toastr.error('Reddedilecek tur bulunamadi');
            return;
        }

        self.isSubmittingSellerCounter(true);

        fetch('/api/negotiations/rounds/' + summary.lastRoundId + '/reject', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ reason: self.sellerNegotiationRejectReason() })
        })
            .then(function(response) {
                if (response.ok) {
                    var rejectModal = bootstrap.Modal.getInstance(document.getElementById('sellerNegotiationRejectModal'));
                    if (rejectModal) rejectModal.hide();
                    var modal = bootstrap.Modal.getInstance(document.getElementById('sellerNegotiationModal'));
                    if (modal) modal.hide();
                    toastr.success('Pazarlik reddedildi');
                    self.loadMyOffers();
                } else {
                    return response.json().then(function(data) {
                        toastr.error(data.message || 'Red islemi basarisiz');
                    });
                }
            })
            .catch(function(err) {
                console.error('Error rejecting negotiation:', err);
                toastr.error('Islem sirasinda hata olustu');
            })
            .finally(function() {
                self.isSubmittingSellerCounter(false);
            });
    };

    // View my offer detail
    self.viewMyOfferDetail = function(offer) {
        // Open the demand detail in a new tab
        window.open('/Demands/' + offer.demandSlug, '_blank');
    };

    // ========================================
    // ACTIONS
    // ========================================

    self.resetIncomingFilters = function() {
        self.incomingFilter.search('');
        self.incomingFilter.status('');
        self.incomingFilter.isRead('');
    };

    // View inquiry detail
    self.viewInquiryDetail = function(inquiry) {
        self.selectedInquiry(inquiry);
        var modal = new bootstrap.Modal(document.getElementById('inquiryDetailModal'));
        modal.show();
    };

    // Respond to inquiry
    self.respondToInquiry = function(inquiry) {
        self.respondingInquiry(inquiry);
        self.resetResponseForm();

        // Pre-fill with inquiry data
        if (inquiry.quantity) self.responseForm.offeredQuantity(inquiry.quantity);
        if (inquiry.unit) {
            // Find unitId from unit name
            var matchedUnit = self.units().find(function(u) { return u.name === inquiry.unit || u.systemName === inquiry.unit; });
            self.responseForm.unitId(matchedUnit ? matchedUnit.id : null);
        }
        if (inquiry.currency) self.responseForm.currency(inquiry.currency);

        // Close detail modal if open
        var detailModal = bootstrap.Modal.getInstance(document.getElementById('inquiryDetailModal'));
        if (detailModal) detailModal.hide();

        var modal = new bootstrap.Modal(document.getElementById('respondModal'));
        modal.show();
    };

    self.resetResponseForm = function() {
        self.responseForm.unitPrice('');
        self.responseForm.currency('TRY');
        self.responseForm.offeredQuantity('');
        self.responseForm.unitId(null);
        self.responseForm.leadTimeDays('');
        self.responseForm.validUntil('');
        self.responseForm.notes('');
        self.responseError('');
    };

    self.submitResponse = function() {
        if (!self.respondingInquiry()) return;

        // Validation
        if (!self.responseForm.unitPrice()) {
            self.responseError('Birim fiyat zorunludur');
            return;
        }
        if (!self.responseForm.offeredQuantity()) {
            self.responseError('Miktar zorunludur');
            return;
        }

        self.isSubmittingResponse(true);
        self.responseError('');

        // Get unit name from selected unit
        var selectedUnit = self.units().find(function(u) { return u.id === self.responseForm.unitId(); });
        var unitName = selectedUnit ? selectedUnit.name : 'Adet';

        var data = {
            unitPrice: parseFloat(self.responseForm.unitPrice()),
            currency: self.responseForm.currency(),
            offeredQuantity: parseInt(self.responseForm.offeredQuantity()),
            offeredUnit: unitName,
            leadTimeDays: self.responseForm.leadTimeDays() ? parseInt(self.responseForm.leadTimeDays()) : null,
            validUntil: self.responseForm.validUntil() || null,
            notes: self.responseForm.notes() || null
        };

        fetch('/api/product-inquiries/incoming/' + self.respondingInquiry().id + '/respond', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        })
            .then(function(r) {
                if (!r.ok) throw new Error('Teklif gonderilemedi');
                return r.json();
            })
            .then(function(result) {
                if (typeof toastr !== 'undefined') {
                    toastr.success('Teklif gonderildi');
                }
                var modal = bootstrap.Modal.getInstance(document.getElementById('respondModal'));
                if (modal) modal.hide();
                self.loadIncomingInquiries();
            })
            .catch(function(err) {
                self.responseError(err.message || 'Bir hata olustu');
            })
            .finally(function() {
                self.isSubmittingResponse(false);
            });
    };

    // Add subscription
    self.addSubscription = function() {
        if (!self.newSubscription.categoryId()) return;

        var data = {
            categoryId: self.newSubscription.categoryId(),
            keywordFilter: self.newSubscription.keywordFilter() || null,
            notifyByEmail: self.newSubscription.notifyByEmail(),
            notifyInApp: self.newSubscription.notifyInApp()
        };

        fetch('/api/suppliers/subscriptions', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        })
            .then(function(r) {
                if (!r.ok) throw new Error('Kategori eklenemedi');
                return r.json();
            })
            .then(function(result) {
                if (typeof toastr !== 'undefined') {
                    toastr.success('Kategori takibe alindi');
                }
                self.newSubscription.categoryId(null);
                self.newSubscription.keywordFilter('');
                self.loadSubscriptions();
                self.loadPotentialDemands(); // Refresh potential demands count
            })
            .catch(function(err) {
                if (typeof toastr !== 'undefined') {
                    toastr.error(err.message || 'Bir hata olustu');
                }
            });
    };

    // Remove subscription
    self.removeSubscription = function(subscription) {
        if (!confirm('Bu kategoriyi takipten cikmak istediginizden emin misiniz?')) return;

        fetch('/api/suppliers/subscriptions/' + subscription.id, {
            method: 'DELETE'
        })
            .then(function(r) {
                if (!r.ok) throw new Error('Kategori kaldirilamadi');
                return r.json();
            })
            .then(function(result) {
                if (typeof toastr !== 'undefined') {
                    toastr.success('Kategori takipten cikarildi');
                }
                self.loadSubscriptions();
                self.loadPotentialDemands(); // Refresh potential demands count
            })
            .catch(function(err) {
                if (typeof toastr !== 'undefined') {
                    toastr.error(err.message || 'Bir hata olustu');
                }
            });
    };

    // ========================================
    // INITIALIZE
    // ========================================
    self.init = function() {
        // Load units for response form
        self.loadUnits();

        // Load initial data for first tab
        self.loadIncomingInquiries();

        // Also preload potential demands count for badge
        fetch('/api/demands/subscribed')
            .then(function(r) { return r.json(); })
            .then(function(data) {
                self.potentialDemandsCount(data.totalCount || data.length || 0);
            })
            .catch(function() {});

        // Also preload subscriptions count for badge
        fetch('/api/suppliers/subscriptions')
            .then(function(r) { return r.json(); })
            .then(function(data) {
                self.subscriptions(data || []);
            })
            .catch(function() {});
    };

    self.init();
}

// Initialize on document ready
document.addEventListener('DOMContentLoaded', function() {
    ko.applyBindings(new SupplierOffersViewModel(), document.getElementById('supplier-offers-app'));
});
