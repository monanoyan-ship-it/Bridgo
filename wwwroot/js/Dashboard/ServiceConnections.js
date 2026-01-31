/**
 * ServiceConnections.js - Servis Baglantilari
 */

(function () {
    'use strict';

    // Servis tanimlari
    var serviceDefinitions = {
        // Odeme
        stripe: { name: 'Stripe', category: 'payment', categoryName: 'Odeme' },
        iyzico: { name: 'iyzico', category: 'payment', categoryName: 'Odeme' },
        paypal: { name: 'PayPal', category: 'payment', categoryName: 'Odeme' },

        // Lojistik
        aras: { name: 'Aras Kargo', category: 'logistics', categoryName: 'Lojistik' },
        yurtici: { name: 'Yurtici Kargo', category: 'logistics', categoryName: 'Lojistik' },
        dhl: { name: 'DHL Express', category: 'logistics', categoryName: 'Lojistik' },
        ups: { name: 'UPS', category: 'logistics', categoryName: 'Lojistik' },

        // Muhasebe
        parasut: { name: 'Parasut', category: 'accounting', categoryName: 'Muhasebe' },
        logo: { name: 'Logo Yazilim', category: 'accounting', categoryName: 'ERP' },
        efatura: { name: 'E-Fatura', category: 'invoice', categoryName: 'E-Fatura' },

        // Pazar Yerleri
        trendyol: { name: 'Trendyol', category: 'marketplace', categoryName: 'Pazar Yeri' },
        hepsiburada: { name: 'Hepsiburada', category: 'marketplace', categoryName: 'Pazar Yeri' },
        n11: { name: 'n11', category: 'marketplace', categoryName: 'Pazar Yeri' }
    };

    function ServiceConnectionsViewModel() {
        var self = this;

        // ============================================
        // OBSERVABLES
        // ============================================

        // Her servis icin ayri observable
        self.services = {};
        Object.keys(serviceDefinitions).forEach(function (code) {
            self.services[code] = {
                serviceCode: code,
                serviceName: serviceDefinitions[code].name,
                category: serviceDefinitions[code].category,
                categoryName: serviceDefinitions[code].categoryName,
                status: ko.observable('disconnected'),
                externalAccountId: ko.observable(''),
                chargesEnabled: ko.observable(false),
                payoutsEnabled: ko.observable(false),
                connectedAt: ko.observable(null),
                lastSyncAt: ko.observable(null),
                lastError: ko.observable(null)
            };
        });

        // ============================================
        // STATE
        // ============================================

        self.isLoading = ko.observable(false);
        self.isConnecting = ko.observable(false);
        self.connectingService = ko.observable('');

        // ============================================
        // COMPUTED
        // ============================================

        self.connectedServices = ko.computed(function () {
            var connected = [];
            Object.keys(self.services).forEach(function (code) {
                var svc = self.services[code];
                if (svc.status() === 'connected') {
                    connected.push({
                        serviceCode: code,
                        serviceName: svc.serviceName,
                        categoryName: svc.categoryName,
                        status: svc.status(),
                        statusText: 'Bagli',
                        statusClass: 'bg-success',
                        connectedAt: svc.connectedAt() ? new Date(svc.connectedAt()).toLocaleDateString('tr-TR') : null,
                        lastSyncAt: svc.lastSyncAt() ? new Date(svc.lastSyncAt()).toLocaleDateString('tr-TR') : null
                    });
                }
            });
            return connected;
        });

        // ============================================
        // HELPERS
        // ============================================

        self.getService = function (code) {
            return self.services[code] || {
                status: ko.observable('disconnected'),
                externalAccountId: ko.observable('')
            };
        };

        // ============================================
        // API CALLS
        // ============================================

        self.loadData = function () {
            self.isLoading(true);

            $.get('/api/services/connections')
                .done(function (data) {
                    var connections = data || [];
                    connections.forEach(function (conn) {
                        if (self.services[conn.serviceCode]) {
                            var svc = self.services[conn.serviceCode];
                            svc.status(conn.status || 'disconnected');
                            svc.externalAccountId(conn.externalAccountId || '');
                            svc.chargesEnabled(conn.chargesEnabled || false);
                            svc.payoutsEnabled(conn.payoutsEnabled || false);
                            svc.connectedAt(conn.connectedAt);
                            svc.lastSyncAt(conn.lastSyncAt);
                            svc.lastError(conn.lastError);
                        }
                    });
                })
                .fail(function (xhr) {
                    console.error('Servis baglantilari yuklenemedi:', xhr);
                })
                .always(function () {
                    self.isLoading(false);
                });
        };

        self.connect = function (serviceCode) {
            self.isConnecting(true);
            self.connectingService(serviceCode);

            $.post('/api/services/' + serviceCode + '/connect')
                .done(function (data) {
                    if (data.redirectUrl) {
                        // OAuth akisi icin yonlendir
                        window.location.href = data.redirectUrl;
                    } else if (data.success) {
                        // Direkt baglanti
                        self.services[serviceCode].status('connected');
                        toastr.success('Servis basariyla baglandi');
                        self.loadData();
                    }
                })
                .fail(function (xhr) {
                    var msg = xhr.responseJSON?.message || 'Baglanti olusturulamadi';
                    toastr.error(msg);
                })
                .always(function () {
                    self.isConnecting(false);
                    self.connectingService('');
                });
        };

        self.disconnect = function (serviceCode) {
            if (!confirm(serviceDefinitions[serviceCode].name + ' baglantisini kesmek istediginize emin misiniz?')) {
                return;
            }

            $.ajax({
                url: '/api/services/' + serviceCode + '/disconnect',
                method: 'DELETE'
            }).done(function () {
                self.services[serviceCode].status('disconnected');
                self.services[serviceCode].externalAccountId('');
                toastr.success('Baglanti kesildi');
            }).fail(function (xhr) {
                var msg = xhr.responseJSON?.message || 'Baglanti kesilemedi';
                toastr.error(msg);
            });
        };

        self.openDashboard = function (serviceCode) {
            $.post('/api/services/' + serviceCode + '/dashboard-link')
                .done(function (data) {
                    if (data.url) {
                        window.open(data.url, '_blank');
                    } else {
                        toastr.error('Dashboard linki alinamadi');
                    }
                })
                .fail(function (xhr) {
                    var msg = xhr.responseJSON?.message || 'Dashboard acilamadi';
                    toastr.error(msg);
                });
        };

        // ============================================
        // INIT
        // ============================================

        self.loadData();
    }

    $(document).ready(function () {
        ko.applyBindings(new ServiceConnectionsViewModel(), document.getElementById('services-app'));
    });

})();
