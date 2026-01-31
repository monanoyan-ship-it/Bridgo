/**
 * MyOrders ViewModel - Alici siparisleri
 */
function MyOrdersViewModel() {
    var self = this;

    // State
    self.isLoading = ko.observable(true);
    self.isProcessing = ko.observable(false);
    self.isSubmittingReview = ko.observable(false);
    self.isLoadingQuotes = ko.observable(false);
    self.isCompletingCheckout = ko.observable(false);
    self.isRequestingFinancing = ko.observable(false);
    self.activeTab = ko.observable('active');
    self.orders = ko.observableArray([]);
    self.selectedOrder = ko.observable(null);
    self.cancelReason = ko.observable('');
    self.orderToCancel = ko.observable(null);
    self.orderToReview = ko.observable(null);
    self.quotesData = ko.observable(null);
    self.quotesOrderId = ko.observable(null);

    // Financing state
    self.financingStatus = ko.observable({});
    self.unselectedServices = ko.observableArray([]);

    // Checkout progress state
    self.checkoutProgress = ko.observable({ steps: [] });

    // Wizard state
    self.wizardStep = ko.observable(1);
    self.wizardData = ko.observable({});
    self.selectedShippingAddressId = ko.observable(null);
    self.wantFinancing = ko.observable(false);
    self.financingDays = ko.observable('30');
    self.financingMaxRate = ko.observable('');
    self.acceptTerms = ko.observable(false);

    // Review form
    self.reviewForm = {
        orderId: ko.observable(null),
        sellerVendorId: ko.observable(null),
        sellerCompanyName: ko.observable(''),
        overallRating: ko.observable(0),
        qualityRating: ko.observable(0),
        deliveryRating: ko.observable(0),
        communicationRating: ko.observable(0),
        valueRating: ko.observable(0),
        title: ko.observable(''),
        comment: ko.observable(''),
        pros: ko.observable(''),
        cons: ko.observable(''),
        wouldRecommend: ko.observable(true)
    };

    // Stats
    self.stats = {
        activeCount: ko.observable(0),
        completedCount: ko.observable(0),
        cancelledCount: ko.observable(0),
        totalSpent: ko.observable(0)
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
        paymentStatus: ko.observable('')
    };

    // Computed
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

    self.filter.paymentStatus.subscribe(function() {
        self.currentPage(1);
        self.loadOrders();
    });

    self.resetFilters = function() {
        self.filter.search('');
        self.filter.status('');
        self.filter.paymentStatus('');
    };

    // Load orders
    self.loadOrders = function() {
        self.isLoading(true);

        var params = {
            page: self.currentPage(),
            pageSize: self.pageSize(),
            search: self.filter.search() || undefined,
            status: self.filter.status() || undefined,
            paymentStatus: self.filter.paymentStatus() || undefined
        };

        // Filter by tab
        if (self.activeTab() === 'active') {
            params.statusCategory = 'active';
        } else if (self.activeTab() === 'completed') {
            params.statusCategory = 'completed';
        } else if (self.activeTab() === 'cancelled') {
            params.statusCategory = 'cancelled';
        }

        $.ajax({
            url: '/api/orders/my',
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
            url: '/api/orders/my/stats',
            method: 'GET',
            success: function(result) {
                self.stats.activeCount(result.activeCount || 0);
                self.stats.completedCount(result.completedCount || 0);
                self.stats.cancelledCount(result.cancelledCount || 0);
                self.stats.totalSpent(result.totalSpent || 0);
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
            url: '/api/orders/my/' + order.id,
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

    // Confirm delivery
    self.confirmDelivery = function(order) {
        showConfirmModal({
            title: 'Teslimat Onayi',
            message: 'Siparisi teslim aldiginizi onayliyor musunuz?',
            type: 'success',
            confirmText: 'Teslim Aldim',
            confirmIcon: 'bi-check-lg',
            onConfirm: function() {
                self.isProcessing(true);
                $.ajax({
                    url: '/api/orders/' + order.id + '/confirm-delivery',
                    method: 'PUT',
                    success: function(result) {
                        self.isProcessing(false);
                        toastr.success('Teslimat onaylandi.');
                        self.loadOrders();
                        self.loadStats();
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

    // Show review modal
    self.showReviewModal = function(order) {
        self.orderToReview(order);
        self.reviewForm.orderId(order.id);
        self.reviewForm.sellerVendorId(order.sellerVendorId);
        self.reviewForm.sellerCompanyName(order.sellerCompanyName);
        self.reviewForm.overallRating(0);
        self.reviewForm.qualityRating(0);
        self.reviewForm.deliveryRating(0);
        self.reviewForm.communicationRating(0);
        self.reviewForm.valueRating(0);
        self.reviewForm.title('');
        self.reviewForm.comment('');
        self.reviewForm.pros('');
        self.reviewForm.cons('');
        self.reviewForm.wouldRecommend(true);
        var modal = new bootstrap.Modal(document.getElementById('reviewModal'));
        modal.show();
    };

    // Submit review
    self.submitReview = function() {
        if (self.reviewForm.overallRating() < 1) {
            toastr.warning('Lutfen genel puan verin.');
            return;
        }

        self.isSubmittingReview(true);
        $.ajax({
            url: '/api/buyer/reviews',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                reviewedVendorId: self.reviewForm.sellerVendorId(),
                orderId: self.reviewForm.orderId(),
                overallRating: self.reviewForm.overallRating(),
                qualityRating: self.reviewForm.qualityRating() || null,
                deliveryRating: self.reviewForm.deliveryRating() || null,
                communicationRating: self.reviewForm.communicationRating() || null,
                valueRating: self.reviewForm.valueRating() || null,
                title: self.reviewForm.title(),
                comment: self.reviewForm.comment(),
                pros: self.reviewForm.pros(),
                cons: self.reviewForm.cons(),
                wouldRecommend: self.reviewForm.wouldRecommend()
            }),
            success: function(result) {
                self.isSubmittingReview(false);
                bootstrap.Modal.getInstance(document.getElementById('reviewModal')).hide();
                toastr.success('Degerlendirmeniz kaydedildi. Tesekkurler!');
                // Update order to show it has been reviewed
                var order = self.orderToReview();
                if (order) {
                    order.hasReview = true;
                    self.orders.valueHasMutated();
                }
            },
            error: function(xhr) {
                self.isSubmittingReview(false);
                var msg = xhr.responseJSON?.message || 'Bir hata olustu.';
                toastr.error(msg);
            }
        });
    };

    // ============================================
    // QUOTES MODAL
    // ============================================

    // Quotes computed
    self.quotesTotalCount = ko.computed(function() {
        var data = self.quotesData();
        if (!data || !data.serviceRequests) return 0;
        return data.serviceRequests.length;
    });

    self.quotesSelectedCount = ko.computed(function() {
        var data = self.quotesData();
        if (!data || !data.serviceRequests) return 0;
        return data.serviceRequests.filter(function(sr) { return sr.selectedQuoteId; }).length;
    });

    self.quotesProgress = ko.computed(function() {
        var total = self.quotesTotalCount();
        if (total === 0) return 100;
        return (self.quotesSelectedCount() / total) * 100;
    });

    self.quotesAllSelected = ko.computed(function() {
        var total = self.quotesTotalCount();
        return total > 0 && self.quotesSelectedCount() === total;
    });

    self.quotesTotalWithServices = ko.computed(function() {
        var data = self.quotesData();
        if (!data) return 0;
        var productTotal = data.productTotal || 0;
        var serviceTotal = 0;
        if (data.serviceRequests) {
            data.serviceRequests.forEach(function(sr) {
                if (sr.selectedQuoteAmount) {
                    serviceTotal += sr.selectedQuoteAmount;
                }
            });
        }
        return productTotal + serviceTotal;
    });

    // Show checkout wizard modal
    self.showQuotesModal = function(order) {
        self.quotesOrderId(order.id);
        self.wizardStep(1);
        self.wantFinancing(false);
        self.financingDays('30');
        self.financingMaxRate('');
        self.acceptTerms(false);
        self.loadOrderQuotes(order.id);
        self.loadWizardData(order.id);
        var modal = new bootstrap.Modal(document.getElementById('checkoutWizardModal'));
        modal.show();
    };

    // Load wizard data (addresses, etc.)
    self.loadWizardData = function(orderId) {
        // Get order details for shipping address
        $.get('/api/orders/my/' + orderId)
            .done(function(order) {
                self.wizardData({
                    shippingAddress: order.shippingAddress ? {
                        id: order.shippingAddressId,
                        title: order.shippingAddress.title || 'Teslimat Adresi',
                        fullAddress: order.shippingAddress.addressLine,
                        city: order.shippingAddress.city,
                        countryName: order.shippingAddress.countryName
                    } : null,
                    availableAddresses: [] // Could load from /api/addresses if needed
                });
                self.selectedShippingAddressId(order.shippingAddressId);
            });
    };

    // Select shipping address
    self.selectShippingAddress = function(address) {
        self.selectedShippingAddressId(address.id);
        self.wizardData().shippingAddress = address;
        self.wizardData.valueHasMutated();
    };

    // Wizard navigation
    self.wizardNextStep = function() {
        var current = self.wizardStep();
        if (current < 4) {
            self.wizardStep(current + 1);
        }
    };

    self.wizardPrevStep = function() {
        var current = self.wizardStep();
        if (current > 1) {
            self.wizardStep(current - 1);
        }
    };

    // Step 2 next with validation
    self.wizardStep2Next = function() {
        var data = self.quotesData();
        if (!data || !data.serviceRequests) {
            self.wizardNextStep();
            return;
        }

        // Check for unselected services with available quotes
        var unselected = [];
        data.serviceRequests.forEach(function(sr) {
            if (!sr.selectedQuoteId && sr.quotes && sr.quotes.length > 0) {
                unselected.push({
                    name: sr.serviceTypeName,
                    icon: sr.serviceTypeIcon,
                    quoteCount: sr.quotes.length
                });
            }
        });

        if (unselected.length > 0) {
            // Show warning modal
            self.unselectedServices(unselected);
            var modal = new bootstrap.Modal(document.getElementById('unselectedQuotesModal'));
            modal.show();
        } else {
            // Check financing status and proceed
            self.checkFinancingAndProceed();
        }
    };

    // Check financing and proceed to step 3
    self.checkFinancingAndProceed = function() {
        var orderId = self.quotesOrderId();
        $.get('/api/orders/' + orderId + '/financing-status')
            .done(function(response) {
                self.financingStatus(response);
                self.wizardNextStep();
            })
            .fail(function() {
                self.wizardNextStep();
            });
    };

    // Complete wizard
    self.completeWizard = function() {
        var orderId = self.quotesOrderId();
        if (!orderId || self.isCompletingCheckout()) return;

        self.isCompletingCheckout(true);

        // If financing requested, create financing request first
        var financingPromise = $.Deferred().resolve();
        if (self.wantFinancing() && !self.financingStatus().existingFinancingRequestId) {
            financingPromise = $.ajax({
                url: '/api/orders/' + orderId + '/request-financing',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({
                    durationDays: parseInt(self.financingDays()),
                    maxInterestRate: self.financingMaxRate() ? parseFloat(self.financingMaxRate()) : null
                })
            });
        }

        financingPromise.always(function() {
            // Complete checkout
            $.ajax({
                url: '/api/checkout/orders/' + orderId + '/complete',
                method: 'POST'
            })
            .done(function(response) {
                if (response.success) {
                    toastr.success('Siparis onaylandi!');
                    bootstrap.Modal.getInstance(document.getElementById('checkoutWizardModal')).hide();
                    self.loadOrders();
                    self.loadStats();
                } else {
                    toastr.error(response.message || 'Siparis onaylanamadi');
                }
            })
            .fail(function(xhr) {
                toastr.error('Siparis onaylanirken hata olustu');
            })
            .always(function() {
                self.isCompletingCheckout(false);
            });
        });
    };

    // Load order quotes
    self.loadOrderQuotes = function(orderId) {
        self.isLoadingQuotes(true);
        self.quotesData(null);
        self.checkoutProgress({ steps: [] });

        // Load quotes and checkout progress in parallel
        $.when(
            $.get('/api/checkout/orders/' + orderId + '/quotes'),
            $.get('/api/orders/' + orderId + '/checkout-progress')
        ).done(function(quotesResponse, progressResponse) {
            var quotes = quotesResponse[0];
            var progress = progressResponse[0];

            if (quotes.success && quotes.data) {
                self.quotesData(quotes.data);
            } else {
                toastr.error(quotes.message || 'Teklifler yuklenemedi');
            }

            if (progress) {
                self.checkoutProgress(progress);
            }
        }).fail(function(xhr) {
            toastr.error('Teklifler yuklenirken hata olustu');
        }).always(function() {
            self.isLoadingQuotes(false);
        });
    };

    // Select quote
    self.selectQuote = function(serviceRequestId, quoteId) {
        var orderId = self.quotesOrderId();
        if (!orderId) return;

        $.ajax({
            url: '/api/checkout/orders/' + orderId + '/quotes/' + quoteId + '/select',
            method: 'POST'
        })
        .done(function(response) {
            if (response.success) {
                toastr.success('Teklif secildi');
                self.loadOrderQuotes(orderId);
            } else {
                toastr.error(response.message || 'Teklif secilemedi');
            }
        })
        .fail(function(xhr) {
            toastr.error('Teklif secilirken hata olustu');
        });
    };

    // Proceed to checkout - deprecated, use wizard instead
    // Kept for backwards compatibility
    self.proceedToCheckout = function() {
        self.wizardStep2Next();
    };

    // Confirm proceed without all quotes
    self.confirmProceedWithoutAllQuotes = function() {
        bootstrap.Modal.getInstance(document.getElementById('unselectedQuotesModal')).hide();
        // Continue to financing step
        self.checkFinancingAndProceed();
    };

    // Complete checkout
    self.completeCheckout = function() {
        var orderId = self.quotesOrderId();
        if (!orderId || self.isCompletingCheckout()) return;

        self.isCompletingCheckout(true);
        $.ajax({
            url: '/api/checkout/orders/' + orderId + '/complete',
            method: 'POST'
        })
        .done(function(response) {
            if (response.success) {
                toastr.success('Siparis onaylandi!');
                bootstrap.Modal.getInstance(document.getElementById('quotesModal')).hide();
                self.loadOrders();
                self.loadStats();

                // Check if financing is available
                self.checkFinancingAvailability(orderId);
            } else {
                toastr.error(response.message || 'Siparis onaylanamadi');
            }
        })
        .fail(function(xhr) {
            toastr.error('Siparis onaylanirken hata olustu');
        })
        .always(function() {
            self.isCompletingCheckout(false);
        });
    };

    // ============================================
    // FINANCING
    // ============================================

    // Check if financing is available for the order
    self.checkFinancingAvailability = function(orderId) {
        $.get('/api/orders/' + orderId + '/financing-status')
            .done(function(response) {
                self.financingStatus(response);

                // If financing can be triggered and not already exists
                if (response.canTriggerFinancing && !response.existingFinancingRequestId) {
                    // Show financing option modal
                    var modal = new bootstrap.Modal(document.getElementById('financingOptionModal'));
                    modal.show();
                }
            })
            .fail(function(xhr) {
                console.log('Financing status check failed:', xhr);
            });
    };

    // Skip financing
    self.skipFinancing = function() {
        bootstrap.Modal.getInstance(document.getElementById('financingOptionModal')).hide();
    };

    // Request financing
    self.requestFinancing = function() {
        var orderId = self.quotesOrderId();
        if (!orderId || self.isRequestingFinancing()) return;

        self.isRequestingFinancing(true);
        $.ajax({
            url: '/api/orders/' + orderId + '/request-financing',
            method: 'POST'
        })
        .done(function(response) {
            bootstrap.Modal.getInstance(document.getElementById('financingOptionModal')).hide();

            if (response.financingRequestId) {
                // Show success modal
                var successModal = new bootstrap.Modal(document.getElementById('financingSuccessModal'));
                successModal.show();
            } else {
                toastr.info('Finansman talebi olusturulamadi. Lutfen daha sonra tekrar deneyin.');
            }
        })
        .fail(function(xhr) {
            var msg = xhr.responseJSON?.error || 'Finansman talebi olusturulurken hata olustu';
            toastr.error(msg);
        })
        .always(function() {
            self.isRequestingFinancing(false);
        });
    };

    // Service status class
    self.getServiceStatusClass = function(status) {
        switch (status) {
            case 1: return 'bg-warning text-dark'; // Open
            case 2: return 'bg-info'; // QuotesReceived
            case 3: return 'bg-success'; // QuoteSelected
            case 4: return 'bg-danger'; // Cancelled
            default: return 'bg-secondary';
        }
    };

    // Quote status class
    self.getQuoteStatusClass = function(status) {
        switch (status) {
            case 1: return 'bg-warning text-dark'; // Pending
            case 2: return 'bg-success'; // Accepted
            case 3: return 'bg-danger'; // Rejected
            case 4: return 'bg-secondary'; // Expired
            case 5: return 'bg-secondary'; // Withdrawn
            default: return 'bg-secondary';
        }
    };

    // Cancel order
    self.cancelOrder = function(order) {
        self.orderToCancel(order);
        self.cancelReason('');
        var modal = new bootstrap.Modal(document.getElementById('cancelOrderModal'));
        modal.show();
    };

    self.confirmCancelOrder = function() {
        var order = self.orderToCancel();
        if (!order) return;

        self.isProcessing(true);
        $.ajax({
            url: '/api/orders/' + order.id + '/cancel',
            method: 'PUT',
            contentType: 'application/json',
            data: JSON.stringify({ reason: self.cancelReason() }),
            success: function(result) {
                self.isProcessing(false);
                bootstrap.Modal.getInstance(document.getElementById('cancelOrderModal')).hide();
                toastr.success('Siparis iptal edildi.');
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
            0: 'bg-secondary',  // Draft
            1: 'bg-warning',    // PendingPayment
            2: 'bg-danger',     // PaymentFailed
            3: 'bg-info',       // PendingConfirm
            4: 'bg-primary',    // Confirmed
            5: 'bg-primary',    // Processing
            6: 'bg-info',       // PartiallyShipped
            7: 'bg-info',       // Shipped
            8: 'bg-info',       // InTransit
            9: 'bg-warning',    // CustomsClearance
            10: 'bg-info',      // PartiallyDelivered
            11: 'bg-success',   // Delivered
            12: 'bg-success',   // Completed
            13: 'bg-secondary', // Cancelled
            14: 'bg-warning',   // Refunded
            15: 'bg-danger'     // Disputed
        };
        return classes[status] || 'bg-secondary';
    };

    self.getPaymentBadgeClass = function(status) {
        var classes = {
            0: 'bg-warning',    // Pending
            1: 'bg-info',       // Processing
            2: 'bg-success',    // Succeeded
            3: 'bg-danger',     // Failed
            4: 'bg-secondary',  // Refunded
            5: 'bg-warning'     // PartiallyRefunded
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
            5: 'bg-warning',    // CustomsClearance
            6: 'bg-success',    // Delivered
            7: 'bg-danger',     // Failed
            8: 'bg-secondary'   // Returned
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
