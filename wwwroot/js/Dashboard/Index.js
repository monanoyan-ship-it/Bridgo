/**
 * Dashboard Index.js - Capability-based Dashboard ViewModel
 */

(function () {
    'use strict';

    function DashboardViewModel() {
        var self = this;

        // ============================================
        // OBSERVABLES
        // ============================================

        self.capabilities = ko.observableArray(serverCapabilities || []);
        self.activeCapability = ko.observable(null);
        self.isLoading = ko.observable(false);

        // Company Dashboard (Firma Bilgileri)
        self.isCompanyDashboard = ko.observable(false);
        self.companyData = ko.observable({});

        // Stats and data based on active capability
        self.stats = ko.observable({});

        // Seller specific
        self.recentOrders = ko.observableArray([]);
        self.recentInquiries = ko.observableArray([]);
        self.topProducts = ko.observableArray([]);

        // Buyer specific
        self.recentDemands = ko.observableArray([]);
        self.pendingProposals = ko.observableArray([]);

        // Service provider specific
        self.recentRequests = ko.observableArray([]);

        // Investor specific
        self.recentOpportunities = ko.observableArray([]);
        self.activeInvestments = ko.observableArray([]);

        // ============================================
        // CAPABILITY SLUG MAP
        // ============================================

        var capabilitySlugMap = {
            'seller': 'seller',
            'buyer': 'buyer',
            'carrier': 'carrier',
            'insurance': 'insurance',
            'customs': 'customs',
            'survey': 'survey',
            'investor': 'investor'
        };

        // ============================================
        // METHODS
        // ============================================

        // Company Dashboard'a gec
        self.switchToCompany = function () {
            if (self.isCompanyDashboard()) {
                return; // Already active
            }

            self.activeCapability(null);
            self.isCompanyDashboard(true);
            self.loadCompanyData();

            // Store in localStorage
            localStorage.setItem('lastDashboardCapability', 'company');
        };

        // Company data yukle
        self.loadCompanyData = function () {
            self.isLoading(true);
            self.clearData();

            $.get('/api/dashboard/company')
                .done(function (data) {
                    self.companyData(data);
                })
                .fail(function (xhr) {
                    console.error('Company dashboard data load failed:', xhr);
                    toastr.error('Firma bilgileri yuklenemedi');
                })
                .always(function () {
                    self.isLoading(false);
                });
        };

        self.switchCapability = function (capability) {
            if (self.activeCapability() && self.activeCapability().code === capability.code) {
                return; // Already active
            }

            self.isCompanyDashboard(false);
            self.activeCapability(capability);
            self.loadDashboardData(capability.code);

            // Store in localStorage
            localStorage.setItem('lastDashboardCapability', capability.code);
        };

        self.loadDashboardData = function (capabilityCode) {
            var slug = capabilitySlugMap[capabilityCode];
            if (!slug) {
                console.error('Unknown capability code:', capabilityCode);
                return;
            }

            self.isLoading(true);
            self.clearData();

            $.get('/api/dashboard/' + slug)
                .done(function (data) {
                    self.mapDashboardData(capabilityCode, data);
                })
                .fail(function (xhr) {
                    console.error('Dashboard data load failed:', xhr);
                    toastr.error('Dashboard verileri yuklenemedi');
                })
                .always(function () {
                    self.isLoading(false);
                });
        };

        self.clearData = function () {
            self.stats({});
            self.companyData({});
            self.recentOrders([]);
            self.recentInquiries([]);
            self.topProducts([]);
            self.recentDemands([]);
            self.pendingProposals([]);
            self.recentRequests([]);
            self.recentOpportunities([]);
            self.activeInvestments([]);
        };

        self.mapDashboardData = function (capabilityCode, data) {
            switch (capabilityCode) {
                case 'seller':
                    self.stats({
                        pendingOrders: data.pendingOrders,
                        processingOrders: data.processingOrders,
                        monthlyRevenue: data.monthlyRevenue,
                        revenueCurrency: data.revenueCurrency || 'TRY',
                        activeProducts: data.activeProducts,
                        lowStockProducts: data.lowStockProducts,
                        newInquiries: data.newInquiries,
                        pendingDemandResponses: data.pendingDemandResponses
                    });
                    self.recentOrders(data.recentOrders || []);
                    self.recentInquiries(data.recentInquiries || []);
                    self.topProducts(data.topProducts || []);
                    break;

                case 'buyer':
                    self.stats({
                        activeOrders: data.activeOrders,
                        pendingDeliveries: data.pendingDeliveries,
                        activeDemands: data.activeDemands,
                        pendingProposals: data.pendingProposals,
                        monthlySpending: data.monthlySpending,
                        spendingCurrency: data.spendingCurrency || 'TRY'
                    });
                    self.recentOrders(data.recentOrders || []);
                    self.recentDemands(data.recentDemands || []);
                    self.pendingProposals(data.pendingProposalsList || []);
                    break;

                case 'carrier':
                    self.stats({
                        openRequests: data.openRequests,
                        activeShipments: data.activeShipments,
                        pendingQuotes: data.pendingQuotes,
                        completedThisMonth: data.completedThisMonth,
                        monthlyEarnings: data.monthlyEarnings,
                        earningsCurrency: data.earningsCurrency || 'TRY'
                    });
                    self.recentRequests(data.recentRequests || []);
                    break;

                case 'insurance':
                    self.stats({
                        openRequests: data.openRequests,
                        activePolicies: data.activePolicies,
                        pendingQuotes: data.pendingQuotes,
                        pendingClaims: data.pendingClaims,
                        monthlyPremium: data.monthlyPremium,
                        premiumCurrency: data.premiumCurrency || 'TRY'
                    });
                    self.recentRequests(data.recentRequests || []);
                    break;

                case 'customs':
                    self.stats({
                        openRequests: data.openRequests,
                        activeDeclarations: data.activeDeclarations,
                        pendingQuotes: data.pendingQuotes,
                        completedThisMonth: data.completedThisMonth,
                        monthlyEarnings: data.monthlyEarnings,
                        earningsCurrency: data.earningsCurrency || 'TRY'
                    });
                    self.recentRequests(data.recentRequests || []);
                    break;

                case 'survey':
                    self.stats({
                        openRequests: data.openRequests,
                        scheduledInspections: data.scheduledInspections,
                        pendingReports: data.pendingReports,
                        completedThisMonth: data.completedThisMonth,
                        monthlyEarnings: data.monthlyEarnings,
                        earningsCurrency: data.earningsCurrency || 'TRY'
                    });
                    self.recentRequests(data.recentRequests || []);
                    break;

                case 'investor':
                    self.stats({
                        availableOpportunities: data.availableOpportunities,
                        activeInvestments: data.activeInvestments,
                        pendingOffers: data.pendingOffers,
                        totalPortfolioValue: data.totalPortfolioValue,
                        expectedMonthlyReturn: data.expectedMonthlyReturn,
                        currency: data.currency || 'TRY'
                    });
                    self.recentOpportunities(data.recentOpportunities || []);
                    self.activeInvestments(data.activeInvestmentsList || []);
                    break;
            }
        };

        // ============================================
        // FORMATTERS
        // ============================================

        self.formatCurrency = function (amount, currency) {
            if (amount === null || amount === undefined) return '-';
            currency = currency || 'TRY';

            var symbol = currency === 'TRY' ? '₺' :
                         currency === 'USD' ? '$' :
                         currency === 'EUR' ? '€' : currency + ' ';

            return symbol + self.formatNumber(amount);
        };

        self.formatNumber = function (num) {
            if (num === null || num === undefined) return '0';
            if (num >= 1000000) {
                return (num / 1000000).toFixed(1) + 'M';
            } else if (num >= 1000) {
                return (num / 1000).toFixed(1) + 'K';
            }
            return num.toLocaleString('tr-TR', { minimumFractionDigits: 0, maximumFractionDigits: 2 });
        };

        self.formatDate = function (dateStr) {
            if (!dateStr) return '-';
            var date = new Date(dateStr);
            return date.toLocaleDateString('tr-TR');
        };

        // ============================================
        // INIT
        // ============================================

        self.init = function () {
            if (self.capabilities().length === 0) {
                return;
            }

            // 1. Check URL query parameter first (e.g., ?capability=seller)
            var urlParams = new URLSearchParams(window.location.search);
            var urlCapability = urlParams.get('capability');

            // URL'de capability varsa ona git
            if (urlCapability) {
                var capability = self.capabilities().find(function (c) {
                    return c.code === urlCapability;
                });
                if (capability) {
                    self.switchCapability(capability);
                    return;
                }
            }

            // 2. URL'de capability yoksa -> Company Dashboard goster
            // (Ust menuden Dashboard'a tiklandiginda bu durum olusur)
            self.switchToCompany();
        };

        self.init();
    }

    $(document).ready(function () {
        var appElement = document.getElementById('dashboard-app');
        if (appElement) {
            ko.applyBindings(new DashboardViewModel(), appElement);
        }
    });

})();
