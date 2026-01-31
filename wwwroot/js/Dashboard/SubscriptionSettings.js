/**
 * SubscriptionSettings.js - Abonelik Ayarlari
 * Her capability icin farkli metrikler ve planlar
 */

(function () {
    'use strict';

    // ============================================
    // CAPABILITY TANIMLARI
    // ============================================

    var CapabilityMetrics = {
        // Seller (Satici)
        2: {
            name: 'Satici',
            metrics: [
                { key: 'productLimit', label: 'Urun Limiti', icon: 'bi-box-seam' },
                { key: 'orderLimit', label: 'Aylik Siparis', icon: 'bi-cart-check' },
                { key: 'teamMemberLimit', label: 'Takim Uyesi', icon: 'bi-people' },
                { key: 'storageLimit', label: 'Depolama', icon: 'bi-hdd' }
            ],
            defaultPlans: [
                {
                    id: 1, name: 'Baslangic', price: 0, isPopular: false,
                    limits: { productLimit: '10', orderLimit: '50', teamMemberLimit: '2', storageLimit: '100 MB' },
                    features: ['10 Urun', '50 Aylik Siparis', '2 Takim Uyesi', '100 MB Depolama', 'Temel Destek']
                },
                {
                    id: 2, name: 'Profesyonel', price: 499, isPopular: true,
                    limits: { productLimit: '100', orderLimit: '500', teamMemberLimit: '10', storageLimit: '1 GB' },
                    features: ['100 Urun', '500 Aylik Siparis', '10 Takim Uyesi', '1 GB Depolama', 'Oncelikli Destek', 'Gelismis Raporlar']
                },
                {
                    id: 3, name: 'Kurumsal', price: 1499, isPopular: false,
                    limits: { productLimit: 'Sinirsiz', orderLimit: 'Sinirsiz', teamMemberLimit: 'Sinirsiz', storageLimit: '10 GB' },
                    features: ['Sinirsiz Urun', 'Sinirsiz Siparis', 'Sinirsiz Takim Uyesi', '10 GB Depolama', '7/24 Destek', 'API Erisimi']
                }
            ]
        },

        // Buyer (Alici)
        3: {
            name: 'Alici',
            metrics: [
                { key: 'demandLimit', label: 'Aylik Talep', icon: 'bi-megaphone' },
                { key: 'supplierLimit', label: 'Tedarikci Takibi', icon: 'bi-building' },
                { key: 'teamMemberLimit', label: 'Takim Uyesi', icon: 'bi-people' },
                { key: 'rfqLimit', label: 'Teklif Isteme', icon: 'bi-file-earmark-text' }
            ],
            defaultPlans: [
                {
                    id: 1, name: 'Baslangic', price: 0, isPopular: false,
                    limits: { demandLimit: '5', supplierLimit: '10', teamMemberLimit: '2', rfqLimit: '20' },
                    features: ['5 Aylik Talep', '10 Tedarikci Takibi', '2 Takim Uyesi', '20 Teklif Isteme', 'Temel Destek']
                },
                {
                    id: 2, name: 'Profesyonel', price: 399, isPopular: true,
                    limits: { demandLimit: '50', supplierLimit: '100', teamMemberLimit: '10', rfqLimit: '200' },
                    features: ['50 Aylik Talep', '100 Tedarikci Takibi', '10 Takim Uyesi', '200 Teklif Isteme', 'Oncelikli Destek', 'Tedarikci Analizi']
                },
                {
                    id: 3, name: 'Kurumsal', price: 999, isPopular: false,
                    limits: { demandLimit: 'Sinirsiz', supplierLimit: 'Sinirsiz', teamMemberLimit: 'Sinirsiz', rfqLimit: 'Sinirsiz' },
                    features: ['Sinirsiz Talep', 'Sinirsiz Tedarikci', 'Sinirsiz Takim Uyesi', 'Sinirsiz Teklif', '7/24 Destek', 'API Erisimi']
                }
            ]
        },

        // Carrier (Tasimaci)
        4: {
            name: 'Tasimaci',
            metrics: [
                { key: 'quoteLimit', label: 'Aylik Teklif', icon: 'bi-cash-coin' },
                { key: 'shipmentLimit', label: 'Aktif Tasima', icon: 'bi-truck' },
                { key: 'vehicleLimit', label: 'Arac/Filo', icon: 'bi-truck-front' },
                { key: 'routeLimit', label: 'Rota Kapsami', icon: 'bi-signpost-split' }
            ],
            defaultPlans: [
                {
                    id: 1, name: 'Baslangic', price: 0, isPopular: false,
                    limits: { quoteLimit: '10', shipmentLimit: '5', vehicleLimit: '3', routeLimit: 'Yerel' },
                    features: ['10 Aylik Teklif', '5 Aktif Tasima', '3 Arac', 'Yerel Rotalar', 'Temel Destek']
                },
                {
                    id: 2, name: 'Profesyonel', price: 599, isPopular: true,
                    limits: { quoteLimit: '100', shipmentLimit: '50', vehicleLimit: '20', routeLimit: 'Ulusal' },
                    features: ['100 Aylik Teklif', '50 Aktif Tasima', '20 Arac', 'Ulusal Rotalar', 'Oncelikli Destek', 'Filo Yonetimi']
                },
                {
                    id: 3, name: 'Kurumsal', price: 1299, isPopular: false,
                    limits: { quoteLimit: 'Sinirsiz', shipmentLimit: 'Sinirsiz', vehicleLimit: 'Sinirsiz', routeLimit: 'Uluslararasi' },
                    features: ['Sinirsiz Teklif', 'Sinirsiz Tasima', 'Sinirsiz Arac', 'Uluslararasi Rotalar', '7/24 Destek', 'API Erisimi']
                }
            ]
        },

        // Insurance (Sigorta)
        5: {
            name: 'Sigorta',
            metrics: [
                { key: 'policyLimit', label: 'Aylik Police', icon: 'bi-shield-check' },
                { key: 'coverageLimit', label: 'Teminat Limiti', icon: 'bi-currency-dollar' },
                { key: 'sectorLimit', label: 'Sektor Kapsami', icon: 'bi-diagram-3' },
                { key: 'teamMemberLimit', label: 'Takim Uyesi', icon: 'bi-people' }
            ],
            defaultPlans: [
                {
                    id: 1, name: 'Baslangic', price: 0, isPopular: false,
                    limits: { policyLimit: '10', coverageLimit: '100K USD', sectorLimit: '3 Sektor', teamMemberLimit: '2' },
                    features: ['10 Aylik Police', '100K USD Teminat', '3 Sektor', '2 Takim Uyesi', 'Temel Destek']
                },
                {
                    id: 2, name: 'Profesyonel', price: 699, isPopular: true,
                    limits: { policyLimit: '100', coverageLimit: '1M USD', sectorLimit: '10 Sektor', teamMemberLimit: '10' },
                    features: ['100 Aylik Police', '1M USD Teminat', '10 Sektor', '10 Takim Uyesi', 'Oncelikli Destek', 'Risk Analizi']
                },
                {
                    id: 3, name: 'Kurumsal', price: 1499, isPopular: false,
                    limits: { policyLimit: 'Sinirsiz', coverageLimit: 'Sinirsiz', sectorLimit: 'Tum Sektorler', teamMemberLimit: 'Sinirsiz' },
                    features: ['Sinirsiz Police', 'Sinirsiz Teminat', 'Tum Sektorler', 'Sinirsiz Takim', '7/24 Destek', 'API Erisimi']
                }
            ]
        },

        // Customs (Gumruk)
        6: {
            name: 'Gumruk',
            metrics: [
                { key: 'declarationLimit', label: 'Aylik Beyanname', icon: 'bi-file-earmark-ruled' },
                { key: 'portLimit', label: 'Gumruk Noktasi', icon: 'bi-geo-alt' },
                { key: 'hsCodeLimit', label: 'HS Kodu Kapsami', icon: 'bi-tags' },
                { key: 'teamMemberLimit', label: 'Takim Uyesi', icon: 'bi-people' }
            ],
            defaultPlans: [
                {
                    id: 1, name: 'Baslangic', price: 0, isPopular: false,
                    limits: { declarationLimit: '10', portLimit: '3 Nokta', hsCodeLimit: '50 Kod', teamMemberLimit: '2' },
                    features: ['10 Aylik Beyanname', '3 Gumruk Noktasi', '50 HS Kodu', '2 Takim Uyesi', 'Temel Destek']
                },
                {
                    id: 2, name: 'Profesyonel', price: 599, isPopular: true,
                    limits: { declarationLimit: '100', portLimit: '10 Nokta', hsCodeLimit: '500 Kod', teamMemberLimit: '10' },
                    features: ['100 Aylik Beyanname', '10 Gumruk Noktasi', '500 HS Kodu', '10 Takim Uyesi', 'Oncelikli Destek', 'Mevzuat Takibi']
                },
                {
                    id: 3, name: 'Kurumsal', price: 1199, isPopular: false,
                    limits: { declarationLimit: 'Sinirsiz', portLimit: 'Tum Noktalar', hsCodeLimit: 'Tum Kodlar', teamMemberLimit: 'Sinirsiz' },
                    features: ['Sinirsiz Beyanname', 'Tum Gumruk Noktalari', 'Tum HS Kodlari', 'Sinirsiz Takim', '7/24 Destek', 'API Erisimi']
                }
            ]
        },

        // Survey (Gozetim)
        7: {
            name: 'Gozetim',
            metrics: [
                { key: 'inspectionLimit', label: 'Aylik Gozetim', icon: 'bi-search' },
                { key: 'fieldTeamLimit', label: 'Saha Ekibi', icon: 'bi-person-badge' },
                { key: 'geoLimit', label: 'Cografi Kapsam', icon: 'bi-globe' },
                { key: 'reportLimit', label: 'Rapor Limiti', icon: 'bi-file-earmark-bar-graph' }
            ],
            defaultPlans: [
                {
                    id: 1, name: 'Baslangic', price: 0, isPopular: false,
                    limits: { inspectionLimit: '5', fieldTeamLimit: '2', geoLimit: 'Yerel', reportLimit: '10' },
                    features: ['5 Aylik Gozetim', '2 Saha Ekibi', 'Yerel Kapsam', '10 Rapor', 'Temel Destek']
                },
                {
                    id: 2, name: 'Profesyonel', price: 499, isPopular: true,
                    limits: { inspectionLimit: '50', fieldTeamLimit: '10', geoLimit: 'Ulusal', reportLimit: '100' },
                    features: ['50 Aylik Gozetim', '10 Saha Ekibi', 'Ulusal Kapsam', '100 Rapor', 'Oncelikli Destek', 'Detayli Analiz']
                },
                {
                    id: 3, name: 'Kurumsal', price: 999, isPopular: false,
                    limits: { inspectionLimit: 'Sinirsiz', fieldTeamLimit: 'Sinirsiz', geoLimit: 'Uluslararasi', reportLimit: 'Sinirsiz' },
                    features: ['Sinirsiz Gozetim', 'Sinirsiz Saha Ekibi', 'Uluslararasi Kapsam', 'Sinirsiz Rapor', '7/24 Destek', 'API Erisimi']
                }
            ]
        },

        // Investor (Yatirimci)
        8: {
            name: 'Yatirimci',
            metrics: [
                { key: 'investmentLimit', label: 'Aylik Yatirim', icon: 'bi-graph-up-arrow' },
                { key: 'portfolioLimit', label: 'Portfoy Limiti', icon: 'bi-briefcase' },
                { key: 'sectorLimit', label: 'Sektor Kapsami', icon: 'bi-diagram-3' },
                { key: 'analysisLimit', label: 'Analiz Raporu', icon: 'bi-bar-chart-line' }
            ],
            defaultPlans: [
                {
                    id: 1, name: 'Baslangic', price: 0, isPopular: false,
                    limits: { investmentLimit: '5', portfolioLimit: '100K USD', sectorLimit: '3 Sektor', analysisLimit: '10' },
                    features: ['5 Aylik Yatirim', '100K USD Portfoy', '3 Sektor', '10 Analiz Raporu', 'Temel Destek']
                },
                {
                    id: 2, name: 'Profesyonel', price: 799, isPopular: true,
                    limits: { investmentLimit: '50', portfolioLimit: '1M USD', sectorLimit: '10 Sektor', analysisLimit: '100' },
                    features: ['50 Aylik Yatirim', '1M USD Portfoy', '10 Sektor', '100 Analiz Raporu', 'Oncelikli Destek', 'Risk Skorlama']
                },
                {
                    id: 3, name: 'Kurumsal', price: 1999, isPopular: false,
                    limits: { investmentLimit: 'Sinirsiz', portfolioLimit: 'Sinirsiz', sectorLimit: 'Tum Sektorler', analysisLimit: 'Sinirsiz' },
                    features: ['Sinirsiz Yatirim', 'Sinirsiz Portfoy', 'Tum Sektorler', 'Sinirsiz Analiz', '7/24 Destek', 'API Erisimi']
                }
            ]
        }
    };

    // Default fallback (Platform/Genel)
    var DefaultCapability = {
        name: 'Genel',
        metrics: [
            { key: 'teamMemberLimit', label: 'Takim Uyesi', icon: 'bi-people' },
            { key: 'storageLimit', label: 'Depolama', icon: 'bi-hdd' },
            { key: 'apiLimit', label: 'API Cagrisi', icon: 'bi-code-slash' },
            { key: 'supportLevel', label: 'Destek Seviyesi', icon: 'bi-headset' }
        ],
        defaultPlans: [
            {
                id: 1, name: 'Baslangic', price: 0, isPopular: false,
                limits: { teamMemberLimit: '2', storageLimit: '100 MB', apiLimit: '1000/ay', supportLevel: 'Email' },
                features: ['2 Takim Uyesi', '100 MB Depolama', '1000 API/ay', 'Email Destek']
            },
            {
                id: 2, name: 'Profesyonel', price: 299, isPopular: true,
                limits: { teamMemberLimit: '10', storageLimit: '1 GB', apiLimit: '10000/ay', supportLevel: 'Oncelikli' },
                features: ['10 Takim Uyesi', '1 GB Depolama', '10000 API/ay', 'Oncelikli Destek']
            },
            {
                id: 3, name: 'Kurumsal', price: 799, isPopular: false,
                limits: { teamMemberLimit: 'Sinirsiz', storageLimit: '10 GB', apiLimit: 'Sinirsiz', supportLevel: '7/24' },
                features: ['Sinirsiz Takim', '10 GB Depolama', 'Sinirsiz API', '7/24 Destek']
            }
        ]
    };

    function SubscriptionSettingsViewModel() {
        var self = this;

        // ============================================
        // OBSERVABLES
        // ============================================

        self.capabilityId = ko.observable(null);
        self.capabilityName = ko.observable('');

        self.currentPlan = ko.observable({
            name: 'Ucretsiz Plan',
            price: '0 TL',
            isActive: true,
            isFree: true,
            renewalDate: '-',
            limits: {}
        });

        self.currentPlanMetrics = ko.observableArray([]);
        self.plans = ko.observableArray([]);
        self.invoices = ko.observableArray([]);

        // ============================================
        // STATE
        // ============================================

        self.isLoading = ko.observable(false);

        // ============================================
        // HELPERS
        // ============================================

        self.getCapabilityConfig = function (capabilityId) {
            return CapabilityMetrics[capabilityId] || DefaultCapability;
        };

        self.buildMetrics = function (limits, capabilityId) {
            var config = self.getCapabilityConfig(capabilityId);
            var metrics = [];

            config.metrics.forEach(function (m) {
                metrics.push({
                    key: m.key,
                    label: m.label,
                    icon: m.icon,
                    value: limits[m.key] || 'Sinirsiz'
                });
            });

            return metrics;
        };

        self.formatPrice = function (price) {
            if (price === 0) return '0 TL';
            return price?.toLocaleString('tr-TR', { style: 'currency', currency: 'TRY' }) || '0 TL';
        };

        // ============================================
        // API CALLS
        // ============================================

        self.loadData = function () {
            self.isLoading(true);

            $.when(
                $.get('/api/subscription/current'),
                $.get('/api/subscription/plans'),
                $.get('/api/subscription/invoices')
            ).done(function (currentResult, plansResult, invoicesResult) {
                var current = currentResult[0] || {};
                var plans = plansResult[0] || [];
                var invoices = invoicesResult[0] || [];

                // Capability bilgisini al
                var capId = current.capabilityId || 2; // Default: Seller
                self.capabilityId(capId);
                self.capabilityName(self.getCapabilityConfig(capId).name);

                // Mevcut plan
                var limits = current.limits || {};
                if (current.name) {
                    self.currentPlan({
                        name: current.name,
                        price: self.formatPrice(current.price),
                        isActive: current.isActive ?? true,
                        isFree: current.isFree ?? (current.price === 0),
                        renewalDate: current.renewalDate ? new Date(current.renewalDate).toLocaleDateString('tr-TR') : '-',
                        limits: limits
                    });

                    // Metrikleri olustur
                    self.currentPlanMetrics(self.buildMetrics(limits, capId));
                } else {
                    // Default metrikleri goster
                    var config = self.getCapabilityConfig(capId);
                    var defaultLimits = config.defaultPlans[0].limits;
                    self.currentPlanMetrics(self.buildMetrics(defaultLimits, capId));
                }

                // Planlar
                if (plans.length > 0) {
                    self.plans(plans.map(function (p) {
                        return {
                            id: p.id,
                            name: p.name,
                            price: self.formatPrice(p.price),
                            features: p.features || [],
                            isPopular: p.isPopular || false,
                            isCurrentPlan: p.id === current.planId,
                            isUpgrade: p.price > (current.price || 0)
                        };
                    }));
                } else {
                    self.loadDefaultPlans();
                }

                // Faturalar
                self.invoices(invoices.map(function (i) {
                    return {
                        invoiceNumber: i.invoiceNumber,
                        date: new Date(i.date).toLocaleDateString('tr-TR'),
                        amount: self.formatPrice(i.amount),
                        statusText: i.status === 'paid' ? 'Odendi' : i.status === 'pending' ? 'Beklemede' : 'Iptal',
                        statusClass: i.status === 'paid' ? 'bg-success' : i.status === 'pending' ? 'bg-warning' : 'bg-danger',
                        downloadUrl: i.downloadUrl || '#'
                    };
                }));
            }).fail(function (xhr) {
                console.error('Abonelik bilgileri yuklenemedi:', xhr);
                self.loadDefaultPlans();
            }).always(function () {
                self.isLoading(false);
            });
        };

        self.loadDefaultPlans = function () {
            var capId = self.capabilityId() || 2;
            var config = self.getCapabilityConfig(capId);

            self.capabilityName(config.name);

            // Ilk plani default olarak ayarla
            var defaultLimits = config.defaultPlans[0].limits;
            self.currentPlan({
                name: config.defaultPlans[0].name,
                price: self.formatPrice(config.defaultPlans[0].price),
                isActive: true,
                isFree: config.defaultPlans[0].price === 0,
                renewalDate: '-',
                limits: defaultLimits
            });
            self.currentPlanMetrics(self.buildMetrics(defaultLimits, capId));

            // Planlari yukle
            self.plans(config.defaultPlans.map(function (p, index) {
                return {
                    id: p.id,
                    name: p.name,
                    price: self.formatPrice(p.price),
                    features: p.features,
                    isPopular: p.isPopular,
                    isCurrentPlan: index === 0,
                    isUpgrade: index > 0
                };
            }));
        };

        // ============================================
        // ACTIONS
        // ============================================

        self.selectPlan = function (plan) {
            if (plan.isCurrentPlan) return;

            showConfirmModal({
                title: 'Plan Degisikligi',
                message: plan.name + ' planina gecmek istediginize emin misiniz?',
                type: plan.isUpgrade ? 'success' : 'warning',
                confirmText: plan.isUpgrade ? 'Yukselt' : 'Degistir',
                confirmIcon: plan.isUpgrade ? 'bi bi-arrow-up-circle' : 'bi bi-arrow-left-right',
                onConfirm: function () {
                    $.ajax({
                        url: '/api/subscription/change-plan',
                        method: 'POST',
                        contentType: 'application/json',
                        data: JSON.stringify({ planId: plan.id })
                    }).done(function (data) {
                        if (data.checkoutUrl) {
                            window.location.href = data.checkoutUrl;
                        } else {
                            toastr.success('Plan degistirildi');
                            self.loadData();
                        }
                    }).fail(function (xhr) {
                        var msg = xhr.responseJSON?.message || 'Plan degistirilemedi';
                        toastr.error(msg);
                    });
                }
            });
        };

        self.cancelSubscription = function () {
            showConfirmModal({
                title: 'Abonelik Iptali',
                message: 'Aboneligi iptal etmek istediginize emin misiniz? Donem sonuna kadar erisim devam edecek.',
                type: 'danger',
                confirmText: 'Iptal Et',
                confirmIcon: 'bi bi-x-circle',
                onConfirm: function () {
                    $.ajax({
                        url: '/api/subscription/cancel',
                        method: 'POST'
                    }).done(function () {
                        toastr.success('Abonelik iptal edildi. Donem sonuna kadar erisim devam edecek.');
                        self.loadData();
                    }).fail(function (xhr) {
                        var msg = xhr.responseJSON?.message || 'Abonelik iptal edilemedi';
                        toastr.error(msg);
                    });
                }
            });
        };

        // ============================================
        // INIT
        // ============================================

        // Sayfa yuklendiginde capability bilgisini al
        var pageCapability = document.getElementById('subscriptions-app')?.dataset?.capability;
        if (pageCapability) {
            self.capabilityId(parseInt(pageCapability));
        }

        self.loadData();
    }

    $(document).ready(function () {
        ko.applyBindings(new SubscriptionSettingsViewModel(), document.getElementById('subscriptions-app'));
    });

})();
