/**
 * SellerOrders ViewModel - Satici siparisleri
 */
function SellerOrdersViewModel() {
    var self = this;

    // State
    self.isLoading = ko.observable(true);
    self.isProcessing = ko.observable(false);
    self.activeTab = ko.observable('pending');
    self.orders = ko.observableArray([]);
    self.selectedOrder = ko.observable(null);

    // Stats
    self.stats = {
        pendingCount: ko.observable(0),
        activeCount: ko.observable(0),
        completedCount: ko.observable(0),
        cancelledCount: ko.observable(0),
        totalRevenue: ko.observable(0)
    };

    // Pagination
    self.currentPage = ko.observable(1);
    self.totalPages = ko.observable(1);
    self.pageSize = ko.observable(10);
    self.totalCount = ko.observable(0);

    // Filter
    self.filter = {
        search: ko.observable(''),
        status: ko.observable(''),
        fromDate: ko.observable('')
    };

    // Modal data
    self.orderToProcess = ko.observable(null);
    self.confirmNotes = ko.observable('');
    self.rejectReason = ko.observable('');
    self.newStatus = ko.observable('');
    self.statusNotes = ko.observable('');

    // Shipment form
    self.shipmentForm = {
        carrierCode: ko.observable(''),
        carrierName: ko.observable(''),
        trackingNumber: ko.observable(''),
        trackingUrl: ko.observable(''),
        estimatedDeliveryDate: ko.observable(''),
        notes: ko.observable(''),
        items: ko.observableArray([])
    };

    // Computed
    self.pendingOrderCount = ko.computed(function() {
        return self.stats.pendingCount();
    });

    self.activeOrderCount = ko.computed(function() {
        return self.stats.activeCount();
    });

    self.completedOrderCount = ko.computed(function() {
        return self.stats.completedCount();
    });

    self.cancelledOrderCount = ko.computed(function() {
        return self.stats.cancelledCount();
    });

    self.pageNumbers = ko.computed(function() {
        var pages = [];
        var total = self.totalPages();
        var current = self.currentPage();
        var start = Math.max(1, current - 2);
        var end = Math.min(total, current + 2);

        for (var i = start; i <= end; i++) {
            pages.push(i);
        }
        return pages;
    });

    // Tab change
    self.setTab = function(tab) {
        self.activeTab(tab);
        self.currentPage(1);
        self.loadOrders();
    };

    // Filter debounce
    var filterTimeout;
    self.filter.search.subscribe(function() {
        clearTimeout(filterTimeout);
        filterTimeout = setTimeout(function() {
            self.currentPage(1);
            self.loadOrders();
        }, 300);
    });

    self.filter.status.subscribe(function() {
        self.currentPage(1);
        self.loadOrders();
    });

    self.filter.fromDate.subscribe(function() {
        self.currentPage(1);
        self.loadOrders();
    });

    self.resetFilters = function() {
        self.filter.search('');
        self.filter.status('');
        self.filter.fromDate('');
    };

    // Load orders
    self.loadOrders = function() {
        self.isLoading(true);

        var params = {
            page: self.currentPage(),
            pageSize: self.pageSize(),
            search: self.filter.search() || undefined,
            status: self.filter.status() || undefined,
            fromDate: self.filter.fromDate() || undefined
        };

        // Filter by tab
        if (self.activeTab() === 'pending') {
            params.statusCategory = 'pending';
        } else if (self.activeTab() === 'active') {
            params.statusCategory = 'active';
        } else if (self.activeTab() === 'completed') {
            params.statusCategory = 'completed';
        } else if (self.activeTab() === 'cancelled') {
            params.statusCategory = 'cancelled';
        }

        $.ajax({
            url: '/api/orders/seller',
            method: 'GET',
            data: params,
            success: function(result) {
                self.orders(result.items || []);
                self.totalCount(result.totalCount || 0);
                self.totalPages(result.totalPages || 1);
                self.isLoading(false);
            },
            error: function(xhr) {
                console.error('Orders load error:', xhr);
                self.isLoading(false);
                toastr.error('Siparisler yuklenirken hata olustu.');
            }
        });
    };

    // Load stats
    self.loadStats = function() {
        $.ajax({
            url: '/api/orders/seller/stats',
            method: 'GET',
            success: function(result) {
                self.stats.pendingCount(result.pendingCount || 0);
                self.stats.activeCount(result.activeCount || 0);
                self.stats.completedCount(result.completedCount || 0);
                self.stats.cancelledCount(result.cancelledCount || 0);
                self.stats.totalRevenue(result.totalRevenue || 0);
            },
            error: function(xhr) {
                console.error('Stats load error:', xhr);
            }
        });
    };

    // Pagination
    self.prevPage = function() {
        if (self.currentPage() > 1) {
            self.currentPage(self.currentPage() - 1);
            self.loadOrders();
        }
    };

    self.nextPage = function() {
        if (self.currentPage() < self.totalPages()) {
            self.currentPage(self.currentPage() + 1);
            self.loadOrders();
        }
    };

    self.goToPage = function(page) {
        self.currentPage(page);
        self.loadOrders();
    };

    // View order detail
    self.viewOrderDetail = function(order) {
        self.isLoading(true);
        $.ajax({
            url: '/api/orders/seller/' + order.id,
            method: 'GET',
            success: function(result) {
                self.selectedOrder(result);
                self.isLoading(false);
                var modal = new bootstrap.Modal(document.getElementById('orderDetailModal'));
                modal.show();
            },
            error: function(xhr) {
                console.error('Order detail error:', xhr);
                self.isLoading(false);
                toastr.error('Siparis detayi yuklenirken hata olustu.');
            }
        });
    };

    // Confirm order
    self.confirmOrder = function(order) {
        self.orderToProcess(order);
        self.confirmNotes('');
        var modal = new bootstrap.Modal(document.getElementById('confirmOrderModal'));
        modal.show();
    };

    self.doConfirmOrder = function() {
        var order = self.orderToProcess();
        if (!order) return;

        self.isProcessing(true);
        $.ajax({
            url: '/api/orders/' + order.id + '/confirm',
            method: 'PUT',
            contentType: 'application/json',
            data: JSON.stringify({ notes: self.confirmNotes() }),
            success: function(result) {
                self.isProcessing(false);
                bootstrap.Modal.getInstance(document.getElementById('confirmOrderModal')).hide();
                toastr.success('Siparis onaylandi.');
                self.loadOrders();
                self.loadStats();
            },
            error: function(xhr) {
                self.isProcessing(false);
                var msg = xhr.responseJSON?.message || 'Bir hata olustu.';
                toastr.error(msg);
            }
        });
    };

    // Reject order
    self.rejectOrder = function(order) {
        self.orderToProcess(order);
        self.rejectReason('');
        var modal = new bootstrap.Modal(document.getElementById('rejectOrderModal'));
        modal.show();
    };

    self.doRejectOrder = function() {
        var order = self.orderToProcess();
        if (!order) return;

        self.isProcessing(true);
        $.ajax({
            url: '/api/orders/' + order.id + '/reject',
            method: 'PUT',
            contentType: 'application/json',
            data: JSON.stringify({ reason: self.rejectReason() }),
            success: function(result) {
                self.isProcessing(false);
                bootstrap.Modal.getInstance(document.getElementById('rejectOrderModal')).hide();
                toastr.success('Siparis reddedildi.');
                self.loadOrders();
                self.loadStats();
            },
            error: function(xhr) {
                self.isProcessing(false);
                var msg = xhr.responseJSON?.message || 'Bir hata olustu.';
                toastr.error(msg);
            }
        });
    };

    // Add shipment
    self.addShipment = function(order) {
        self.orderToProcess(order);

        // Reset form
        self.shipmentForm.carrierCode('');
        self.shipmentForm.carrierName('');
        self.shipmentForm.trackingNumber('');
        self.shipmentForm.trackingUrl('');
        self.shipmentForm.estimatedDeliveryDate('');
        self.shipmentForm.notes('');

        // Prepare items
        var items = (order.items || []).map(function(item) {
            return {
                orderItemId: item.id,
                productName: item.productName,
                orderQuantity: item.quantity,
                shippedQuantity: item.shippedQuantity || 0,
                remainingQuantity: item.quantity - (item.shippedQuantity || 0),
                quantity: ko.observable(item.quantity - (item.shippedQuantity || 0))
            };
        });
        self.shipmentForm.items(items);

        var modal = new bootstrap.Modal(document.getElementById('addShipmentModal'));
        modal.show();
    };

    self.saveShipment = function() {
        var order = self.orderToProcess();
        if (!order) return;

        var items = self.shipmentForm.items().filter(function(item) {
            return parseInt(item.quantity()) > 0;
        }).map(function(item) {
            return {
                orderItemId: item.orderItemId,
                quantity: parseInt(item.quantity())
            };
        });

        if (items.length === 0) {
            toastr.warning('En az bir urun secmelisiniz.');
            return;
        }

        var data = {
            carrierCode: self.shipmentForm.carrierCode(),
            carrierName: self.shipmentForm.carrierCode() === 'OTHER' ? self.shipmentForm.carrierName() : self.shipmentForm.carrierCode(),
            trackingNumber: self.shipmentForm.trackingNumber() || null,
            trackingUrl: self.shipmentForm.trackingUrl() || null,
            estimatedDeliveryDate: self.shipmentForm.estimatedDeliveryDate() || null,
            notes: self.shipmentForm.notes() || null,
            items: items
        };

        self.isProcessing(true);
        $.ajax({
            url: '/api/orders/' + order.id + '/shipments',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data),
            success: function(result) {
                self.isProcessing(false);
                bootstrap.Modal.getInstance(document.getElementById('addShipmentModal')).hide();
                toastr.success('Kargo eklendi.');
                self.loadOrders();
            },
            error: function(xhr) {
                self.isProcessing(false);
                var msg = xhr.responseJSON?.message || 'Bir hata olustu.';
                toastr.error(msg);
            }
        });
    };

    // Update status
    self.updateStatus = function(order) {
        self.orderToProcess(order);
        self.newStatus('');
        self.statusNotes('');
        var modal = new bootstrap.Modal(document.getElementById('updateStatusModal'));
        modal.show();
    };

    self.doUpdateStatus = function() {
        var order = self.orderToProcess();
        if (!order) return;

        self.isProcessing(true);
        $.ajax({
            url: '/api/orders/' + order.id + '/status',
            method: 'PUT',
            contentType: 'application/json',
            data: JSON.stringify({
                status: parseInt(self.newStatus()),
                notes: self.statusNotes()
            }),
            success: function(result) {
                self.isProcessing(false);
                bootstrap.Modal.getInstance(document.getElementById('updateStatusModal')).hide();
                toastr.success('Durum guncellendi.');
                self.loadOrders();
            },
            error: function(xhr) {
                self.isProcessing(false);
                var msg = xhr.responseJSON?.message || 'Bir hata olustu.';
                toastr.error(msg);
            }
        });
    };

    // Helper functions
    self.formatCurrency = function(amount) {
        if (!amount) return '0';
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
            0: 'bg-secondary',
            1: 'bg-warning',
            2: 'bg-danger',
            3: 'bg-warning',
            4: 'bg-primary',
            5: 'bg-primary',
            6: 'bg-info',
            7: 'bg-info',
            8: 'bg-info',
            9: 'bg-warning',
            10: 'bg-info',
            11: 'bg-success',
            12: 'bg-success',
            13: 'bg-secondary',
            14: 'bg-warning',
            15: 'bg-danger'
        };
        return classes[status] || 'bg-secondary';
    };

    self.getPaymentBadgeClass = function(status) {
        var classes = {
            0: 'bg-warning',
            1: 'bg-info',
            2: 'bg-success',
            3: 'bg-danger',
            4: 'bg-secondary',
            5: 'bg-warning'
        };
        return classes[status] || 'bg-secondary';
    };

    self.getShipmentBadgeClass = function(status) {
        var classes = {
            0: 'bg-secondary',
            1: 'bg-info',
            2: 'bg-primary',
            3: 'bg-info',
            4: 'bg-primary',
            5: 'bg-warning',
            6: 'bg-success',
            7: 'bg-danger',
            8: 'bg-secondary'
        };
        return classes[status] || 'bg-secondary';
    };

    // Make helper functions available to bindings
    window.formatCurrency = self.formatCurrency;
    window.formatDate = self.formatDate;
    window.formatDateTime = self.formatDateTime;
    window.getStatusBadgeClass = self.getStatusBadgeClass;
    window.getPaymentBadgeClass = self.getPaymentBadgeClass;
    window.getShipmentBadgeClass = self.getShipmentBadgeClass;

    // Initialize
    self.loadStats();
    self.loadOrders();
}
