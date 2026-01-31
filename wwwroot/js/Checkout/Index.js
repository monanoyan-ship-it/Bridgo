// Checkout ViewModel
function CheckoutViewModel() {
    var self = this;

    // State
    self.isLoading = ko.observable(true);
    self.isSubmitting = ko.observable(false);
    self.step = ko.observable(1);
    self.summary = ko.observable(null);
    self.selectedAddressId = ko.observable(null);
    self.requestFinancing = ko.observable(false);
    self.notes = ko.observable('');

    // Incoterm state
    self.incoterms = ko.observableArray([]);
    self.selectedIncotermId = ko.observable(null);
    self.incotermLocation = ko.observable('');
    self.incotermResponsibilities = ko.observable(null);

    // Financing form
    self.financingForm = {
        requestedAmount: ko.observable(null),
        durationDays: ko.observable('30'),
        maxInterestRate: ko.observable(null),
        offerDeadline: ko.observable(null),
        description: ko.observable('')
    };

    // Set default financing amount when summary loads
    self.summary.subscribe(function(sum) {
        if (sum && sum.totalProductAmount) {
            self.financingForm.requestedAmount(sum.totalProductAmount);
        }
    });

    // Computed
    self.selectedAddress = ko.computed(function() {
        var sum = self.summary();
        var addressId = self.selectedAddressId();
        if (!sum || !addressId) return null;
        return sum.availableAddresses.find(function(a) { return a.id === addressId; });
    });

    self.selectedServices = ko.computed(function() {
        var sum = self.summary();
        if (!sum) return [];
        return sum.serviceOptions.filter(function(s) { return s.isSelected; });
    });

    self.allItems = ko.computed(function() {
        var sum = self.summary();
        if (!sum) return [];
        var items = [];
        sum.vendorGroups.forEach(function(g) {
            items = items.concat(g.items);
        });
        return items;
    });

    // Selected Incoterm object
    self.selectedIncoterm = ko.computed(function() {
        var incotermId = self.selectedIncotermId();
        if (!incotermId) return null;
        return self.incoterms().find(function(i) { return i.id === parseInt(incotermId); });
    });

    // Can proceed from Incoterm step
    self.canProceedFromIncoterm = ko.computed(function() {
        return self.selectedIncotermId() && self.incotermLocation() && self.incotermLocation().trim().length > 0;
    });

    // Load incoterm responsibilities when selection changes
    self.selectedIncotermId.subscribe(function(incotermId) {
        if (!incotermId) {
            self.incotermResponsibilities(null);
            return;
        }

        $.get('/api/types/incoterms/' + incotermId + '/responsibilities')
            .done(function(data) {
                self.incotermResponsibilities(data);
            })
            .fail(function() {
                self.incotermResponsibilities(null);
            });
    });

    // Format currency
    self.formatCurrency = function(amount, currency) {
        if (amount === null || amount === undefined) return '-';
        currency = currency || 'TRY';
        return new Intl.NumberFormat('tr-TR', {
            style: 'currency',
            currency: currency,
            minimumFractionDigits: 2
        }).format(amount);
    };

    // Load incoterms
    self.loadIncoterms = function() {
        $.get('/api/types/incoterms')
            .done(function(data) {
                self.incoterms(data);
                // Set default (DDP)
                var defaultIncoterm = data.find(function(i) { return i.isDefault; });
                if (defaultIncoterm) {
                    self.selectedIncotermId(defaultIncoterm.id);
                }
            })
            .fail(function() {
                toastr.error('Incoterms yuklenemedi');
            });
    };

    // Load checkout summary
    self.loadSummary = function() {
        self.isLoading(true);
        $.get('/api/checkout/summary')
            .done(function(response) {
                if (response.success && response.data) {
                    // Make service options observable
                    response.data.serviceOptions.forEach(function(s) {
                        s.isSelected = ko.observable(s.isSelected);
                    });
                    self.summary(response.data);

                    // Set default address
                    if (response.data.shippingAddress) {
                        self.selectedAddressId(response.data.shippingAddress.id);
                    }
                } else {
                    self.summary(null);
                }
            })
            .fail(function(xhr) {
                toastr.error('Checkout bilgileri yuklenemedi');
            })
            .always(function() {
                self.isLoading(false);
            });
    };

    // Step navigation (4 steps now)
    self.nextStep = function() {
        if (self.step() === 1) {
            if (!self.selectedAddressId()) {
                toastr.warning('Lutfen bir teslimat adresi secin');
                return;
            }
        }
        if (self.step() === 2) {
            if (!self.canProceedFromIncoterm()) {
                toastr.warning('Lutfen Incoterm ve lokasyon bilgilerini girin');
                return;
            }
        }
        if (self.step() < 4) {
            self.step(self.step() + 1);
        }
    };

    self.prevStep = function() {
        if (self.step() > 1) {
            self.step(self.step() - 1);
        }
    };

    // Submit order
    self.submitOrder = function() {
        if (self.isSubmitting()) return;

        var selectedServices = self.selectedServices().map(function(s) {
            return {
                serviceType: s.serviceType
            };
        });

        var dto = {
            shippingAddressId: self.selectedAddressId(),
            notes: self.notes(),
            // Incoterm bilgileri
            incotermId: parseInt(self.selectedIncotermId()) || null,
            incotermLocation: self.incotermLocation(),
            // Hizmetler
            services: selectedServices,
            // Finansman
            requestFinancing: self.requestFinancing(),
            financingDetails: self.requestFinancing() ? {
                financingSubject: 1, // ProductCost by default
                requestedAmount: parseFloat(self.financingForm.requestedAmount()) || null,
                durationDays: parseInt(self.financingForm.durationDays()) || 30,
                maxInterestRate: self.financingForm.maxInterestRate() ? parseFloat(self.financingForm.maxInterestRate()) : null,
                offerDeadline: self.financingForm.offerDeadline() || null,
                description: self.financingForm.description() || null
            } : null
        };

        self.isSubmitting(true);
        $.ajax({
            url: '/api/checkout/initiate',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(dto)
        })
        .done(function(response) {
            if (response.success) {
                if (response.nextStep === 'AwaitingQuotes') {
                    toastr.success('Siparisiniz olusturuldu! Servis saglayicilardan teklifler gelince bildirim alacaksiniz.');
                } else {
                    toastr.success('Siparis olusturuldu!');
                }
                // Redirect to MyOrders after short delay
                setTimeout(function() {
                    window.location.href = '/Orders/MyOrders';
                }, 1500);
            } else {
                toastr.error(response.message || 'Siparis olusturulamadi');
            }
        })
        .fail(function(xhr) {
            toastr.error('Siparis olusturulurken hata olustu');
        })
        .always(function() {
            self.isSubmitting(false);
        });
    };

    // Init
    self.loadSummary();
    self.loadIncoterms();
}

$(function() {
    ko.applyBindings(new CheckoutViewModel(), document.getElementById('checkout-app'));
});
