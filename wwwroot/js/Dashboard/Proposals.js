/**
 * Proposals.js - Teklif Yonetimi (Birlesik Dashboard)
 * ProductInquiryResponse + DemandResponse
 */

(function () {
    'use strict';

    function ProposalsViewModel() {
        var self = this;

        // ============================================
        // OBSERVABLES
        // ============================================

        // Tab
        self.selectedTab = ko.observable('all'); // 'all', 'inquiry', 'demand'

        // Filtreler
        self.searchText = ko.observable('');
        self.selectedStatus = ko.observable('');

        // Data
        self.proposals = ko.observableArray([]);
        self.stats = ko.observable({
            totalProposals: 0,
            pendingProposals: 0,
            acceptedProposals: 0,
            rejectedProposals: 0,
            inquiryProposals: 0,
            demandProposals: 0,
            thisMonthProposals: 0,
            thisMonthValue: 0
        });

        // Pagination
        self.currentPage = ko.observable(1);
        self.pageSize = ko.observable(20);
        self.totalCount = ko.observable(0);

        // UI State
        self.isLoading = ko.observable(false);

        // ============================================
        // COMPUTED
        // ============================================

        self.totalPages = ko.computed(function () {
            return Math.ceil(self.totalCount() / self.pageSize()) || 1;
        });

        self.pageNumbers = ko.computed(function () {
            var total = self.totalPages();
            var current = self.currentPage();
            var pages = [];

            var start = Math.max(1, current - 2);
            var end = Math.min(total, current + 2);

            for (var i = start; i <= end; i++) {
                pages.push(i);
            }

            return pages;
        });

        // ============================================
        // API CALLS
        // ============================================

        self.loadStats = function () {
            $.get('/api/proposals/stats')
                .done(function (data) {
                    self.stats(data);
                })
                .fail(function (xhr) {
                    console.error('Stats yukleme hatasi:', xhr);
                });
        };

        self.loadProposals = function () {
            self.isLoading(true);

            var params = {
                page: self.currentPage(),
                pageSize: self.pageSize(),
                search: self.searchText() || null,
                status: self.selectedStatus() || null,
                sourceType: self.selectedTab() === 'all' ? null : self.selectedTab()
            };

            $.get('/api/proposals', params)
                .done(function (data) {
                    self.proposals(data.items || []);
                    self.totalCount(data.totalCount || 0);
                })
                .fail(function (xhr) {
                    console.error('Proposals yukleme hatasi:', xhr);
                    toastr.error(T('Common.Error.Loading', 'Veriler yuklenirken hata olustu'));
                })
                .always(function () {
                    self.isLoading(false);
                });
        };

        // ============================================
        // ACTIONS
        // ============================================

        self.selectTab = function (tab) {
            self.selectedTab(tab);
            self.currentPage(1);
            self.loadProposals();
        };

        self.goToPage = function (page) {
            if (page < 1 || page > self.totalPages()) return;
            self.currentPage(page);
            self.loadProposals();
        };

        self.resetFilters = function () {
            self.searchText('');
            self.selectedStatus('');
            self.currentPage(1);
            self.loadProposals();
        };

        // ============================================
        // HELPERS
        // ============================================

        self.formatMoney = function (amount, currency) {
            if (!amount) return '-';
            var formatted = parseFloat(amount).toLocaleString('tr-TR', {
                minimumFractionDigits: 2,
                maximumFractionDigits: 2
            });
            return formatted + ' ' + (currency || 'TRY');
        };

        self.formatDate = function (dateStr) {
            if (!dateStr) return '-';
            var date = new Date(dateStr);
            return date.toLocaleDateString('tr-TR', {
                day: '2-digit',
                month: '2-digit',
                year: 'numeric'
            });
        };

        self.getStatusBadgeClass = function (status) {
            switch (status) {
                case 1: return 'bg-warning text-dark'; // Pending
                case 2: return 'bg-info';              // Viewed
                case 3: return 'bg-success';           // Accepted / Shortlisted
                case 4: return 'bg-success';           // Accepted (demand)
                case 5: return 'bg-danger';            // Rejected
                case 6: return 'bg-secondary';         // Expired
                default: return 'bg-secondary';
            }
        };

        // ============================================
        // SUBSCRIPTIONS
        // ============================================

        // Arama debounce
        var searchTimeout;
        self.searchText.subscribe(function () {
            clearTimeout(searchTimeout);
            searchTimeout = setTimeout(function () {
                self.currentPage(1);
                self.loadProposals();
            }, 300);
        });

        self.selectedStatus.subscribe(function () {
            self.currentPage(1);
            self.loadProposals();
        });

        // ============================================
        // INIT
        // ============================================

        self.init = function () {
            self.loadStats();
            self.loadProposals();
        };

        self.init();
    }

    // ============================================
    // APPLY BINDINGS
    // ============================================

    $(document).ready(function () {
        var vm = new ProposalsViewModel();
        window.vm = vm; // Debug icin
        ko.applyBindings({ vm: vm }, document.querySelector('.container-fluid'));
    });

})();
