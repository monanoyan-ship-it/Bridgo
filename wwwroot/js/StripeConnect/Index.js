(function () {
    'use strict';

    function StripeConnectViewModel() {
        var self = this;

        self.isLoading = ko.observable(true);
        self.isConnecting = ko.observable(false);
        self.isConnected = ko.observable(false);
        self.connection = ko.observable({});
        self.payments = ko.observableArray([]);

        self.loadStatus = function () {
            self.isLoading(true);
            $.get('/api/services/connections')
                .done(function (data) {
                    var stripe = (data || []).find(function(c) { return c.serviceCode === 'stripe'; });
                    if (stripe && stripe.status === 'connected') {
                        self.isConnected(true);
                        self.connection(stripe);
                    } else {
                        self.isConnected(false);
                        self.connection(stripe || {});
                    }
                })
                .fail(function () {
                    self.isConnected(false);
                })
                .always(function () {
                    self.isLoading(false);
                });
        };

        self.loadPayments = function () {
            $.get('/api/services/stripe/payments')
                .done(function (data) {
                    self.payments(data || []);
                });
        };

        self.connectStripe = function () {
            self.isConnecting(true);
            $.post('/api/services/stripe/connect')
                .done(function (data) {
                    if (data.redirectUrl) {
                        window.location.href = data.redirectUrl;
                    } else {
                        toastr.success(data.message || 'Baglanti olusturuldu.');
                        self.loadStatus();
                    }
                })
                .fail(function (xhr) {
                    var msg = xhr.responseJSON ? xhr.responseJSON.message : 'Baglanti olusturulamadi.';
                    toastr.error(msg);
                })
                .always(function () {
                    self.isConnecting(false);
                });
        };

        self.disconnectStripe = function () {
            showConfirmModal({
                title: 'Stripe Baglantisini Kes',
                message: 'Stripe hesap baglantinizi kesmek istediginize emin misiniz? Odeme alamayacaksiniz.',
                type: 'danger',
                confirmText: 'Baglantiyi Kes',
                confirmIcon: 'bi-x-circle',
                onConfirm: function () {
                    $.post('/api/services/stripe/disconnect')
                        .done(function (data) {
                            toastr.success(data.message || 'Baglanti kesildi.');
                            self.isConnected(false);
                            self.connection({});
                            self.payments([]);
                        })
                        .fail(function (xhr) {
                            var msg = xhr.responseJSON ? xhr.responseJSON.message : 'Hata olustu.';
                            toastr.error(msg);
                        });
                }
            });
        };

        self.refreshStatus = function () {
            self.loadStatus();
            self.loadPayments();
            toastr.info('Durum yenilendi.');
        };

        self.openStripeDashboard = function () {
            window.open('https://dashboard.stripe.com', '_blank');
        };

        self.formatPrice = function (amount, currency) {
            if (!amount) return '-';
            return parseFloat(amount).toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + ' ' + (currency || 'USD');
        };

        self.formatDateTime = function (dateStr) {
            if (!dateStr) return '';
            return new Date(dateStr).toLocaleDateString('tr-TR', { day: 'numeric', month: 'short', year: 'numeric', hour: '2-digit', minute: '2-digit' });
        };

        // Init
        self.loadStatus();
        self.loadPayments();
    }

    ko.applyBindings(new StripeConnectViewModel(), document.getElementById('stripe-connect-app'));
})();
