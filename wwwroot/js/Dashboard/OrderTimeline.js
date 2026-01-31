/**
 * OrderTimeline ViewModel - Siparis takip sayfasi
 */
function OrderTimelineViewModel(orderId) {
    var self = this;

    // State
    self.isLoading = ko.observable(true);
    self.isProcessing = ko.observable(false);
    self.errorMessage = ko.observable('');
    self.order = ko.observable(null);
    self.history = ko.observableArray([]);

    // Computed
    self.canTakeAction = ko.computed(function() {
        return self.canConfirmDelivery() || self.canReview();
    });

    self.canConfirmDelivery = ko.computed(function() {
        var order = self.order();
        if (!order) return false;
        // Can confirm if status is Delivered (11) but not yet Completed (12)
        return order.status === 11;
    });

    self.canReview = ko.computed(function() {
        var order = self.order();
        if (!order) return false;
        // Can review if completed and no review yet
        return order.status === 12 && !order.hasReview;
    });

    // Load order
    self.loadOrder = function() {
        self.isLoading(true);
        self.errorMessage('');

        $.ajax({
            url: '/api/orders/my/' + orderId,
            method: 'GET',
            success: function(result) {
                self.order(result);
                self.loadHistory();
            },
            error: function(xhr) {
                self.isLoading(false);
                if (xhr.status === 404) {
                    self.errorMessage('Siparis bulunamadi.');
                } else {
                    self.errorMessage('Siparis yuklenirken hata olustu.');
                }
            }
        });
    };

    // Load history
    self.loadHistory = function() {
        $.ajax({
            url: '/api/orders/' + orderId + '/history',
            method: 'GET',
            success: function(result) {
                // Mark current item (last one)
                if (result && result.length > 0) {
                    result.forEach(function(item, index) {
                        item.isCompleted = index < result.length - 1;
                        item.isCurrent = index === result.length - 1;
                        item.isPending = false;
                    });
                }
                self.history(result || []);
                self.isLoading(false);
            },
            error: function(xhr) {
                self.isLoading(false);
                console.error('History load error:', xhr);
            }
        });
    };

    // Confirm delivery
    self.confirmDelivery = function() {
        showConfirmModal({
            title: 'Teslimat Onayi',
            message: 'Siparisi teslim aldiginizi onayliyor musunuz?',
            type: 'success',
            confirmText: 'Teslim Aldim',
            confirmIcon: 'bi-check-lg',
            onConfirm: function() {
                self.isProcessing(true);
                $.ajax({
                    url: '/api/orders/' + orderId + '/confirm-delivery',
                    method: 'PUT',
                    success: function(result) {
                        self.isProcessing(false);
                        toastr.success('Teslimat onaylandi.');
                        self.loadOrder();
                    },
                    error: function(xhr) {
                        self.isProcessing(false);
                        var msg = xhr.responseJSON?.message || 'Bir hata olustu.';
                        toastr.error(msg);
                    }
                });
            }
        });
    };

    // Helper functions
    self.formatCurrency = function(amount) {
        if (!amount && amount !== 0) return '0';
        return new Intl.NumberFormat('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(amount);
    };

    self.formatDate = function(dateStr) {
        if (!dateStr) return '';
        return new Date(dateStr).toLocaleDateString('tr-TR');
    };

    self.formatDateTime = function(dateStr) {
        if (!dateStr) return '';
        return new Date(dateStr).toLocaleString('tr-TR');
    };

    self.getStatusBadgeClass = function(status) {
        var classes = {
            0: 'bg-secondary',  // Draft
            1: 'bg-warning text-dark', // PendingPayment
            2: 'bg-danger',     // PaymentFailed
            3: 'bg-info',       // PendingConfirm
            4: 'bg-primary',    // Confirmed
            5: 'bg-primary',    // Processing
            6: 'bg-info',       // PartiallyShipped
            7: 'bg-info',       // Shipped
            8: 'bg-info',       // InTransit
            9: 'bg-warning text-dark', // CustomsClearance
            10: 'bg-info',      // PartiallyDelivered
            11: 'bg-success',   // Delivered
            12: 'bg-success',   // Completed
            13: 'bg-secondary', // Cancelled
            14: 'bg-warning text-dark', // Refunded
            15: 'bg-danger'     // Disputed
        };
        return classes[status] || 'bg-secondary';
    };

    self.getPaymentBadgeClass = function(status) {
        var classes = {
            0: 'bg-warning text-dark', // Pending
            1: 'bg-info',       // Processing
            2: 'bg-success',    // Succeeded
            3: 'bg-danger',     // Failed
            4: 'bg-secondary',  // Refunded
            5: 'bg-warning text-dark' // PartiallyRefunded
        };
        return classes[status] || 'bg-secondary';
    };

    self.getShipmentBadgeClass = function(status) {
        var classes = {
            0: 'bg-secondary',  // Preparing
            1: 'bg-info',       // ReadyToShip
            2: 'bg-primary',    // Shipped
            3: 'bg-info',       // InTransit
            4: 'bg-primary',    // OutForDelivery
            5: 'bg-warning text-dark', // CustomsClearance
            6: 'bg-success',    // Delivered
            7: 'bg-danger',     // Failed
            8: 'bg-secondary'   // Returned
        };
        return classes[status] || 'bg-secondary';
    };

    self.getServiceStatusClass = function(status) {
        var classes = {
            1: 'bg-warning text-dark', // Open
            2: 'bg-info',       // QuotesReceived
            3: 'bg-success',    // QuoteSelected
            4: 'bg-danger'      // Cancelled
        };
        return classes[status] || 'bg-secondary';
    };

    self.getParticipantIcon = function(role) {
        var icons = {
            1: 'bi-shop',       // Seller
            2: 'bi-truck',      // LogisticsProvider
            3: 'bi-flag',       // CustomsBroker
            4: 'bi-shield-check', // InsuranceProvider
            5: 'bi-bank',       // Investor
            6: 'bi-search'      // Surveyor
        };
        return icons[role] || 'bi-person';
    };

    // Initialize
    self.loadOrder();
}
