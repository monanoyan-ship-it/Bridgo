$(function () {
    function MyInvestmentsViewModel() {
        var self = this;

        // Tab state
        self.currentTab = ko.observable('active');

        // Data
        self.activeInvestments = ko.observableArray([]);
        self.completedInvestments = ko.observableArray([]);
        self.stats = ko.observable({
            activeCount: 0,
            activeTotalAmount: ko.observable(0),
            activeExpectedReturn: ko.observable(0),
            completedCount: 0
        });

        // UI State
        self.isLoading = ko.observable(false);
        self.selectedInvestment = ko.observable(null);

        // Tab change handler
        self.currentTab.subscribe(function (tab) {
            if (tab === 'active') {
                self.loadActiveInvestments();
            } else if (tab === 'completed') {
                self.loadCompletedInvestments();
            }
        });

        // Load active investments
        self.loadActiveInvestments = function () {
            self.isLoading(true);
            $.get('/api/investment/my-investments', { completed: false })
                .done(function (data) {
                    self.activeInvestments(data || []);
                })
                .fail(function (xhr) {
                    toastr.error('Aktif yatirimlar yuklenemedi');
                })
                .always(function () {
                    self.isLoading(false);
                });
        };

        // Load completed investments
        self.loadCompletedInvestments = function () {
            self.isLoading(true);
            $.get('/api/investment/my-investments', { completed: true })
                .done(function (data) {
                    self.completedInvestments(data || []);
                })
                .fail(function (xhr) {
                    toastr.error('Tamamlanan yatirimlar yuklenemedi');
                })
                .always(function () {
                    self.isLoading(false);
                });
        };

        // Load stats
        self.loadStats = function () {
            $.get('/api/investment/my-investments/stats')
                .done(function (data) {
                    self.stats({
                        activeCount: data.activeCount || 0,
                        activeTotalAmount: ko.observable(data.activeTotalAmount || 0),
                        activeExpectedReturn: ko.observable(data.activeExpectedReturn || 0),
                        completedCount: data.completedCount || 0
                    });
                })
                .fail(function (xhr) {
                    console.error('Stats yuklenemedi');
                });
        };

        // View investment detail
        self.viewDetail = function (investment) {
            self.selectedInvestment(investment);
            new bootstrap.Modal('#detailModal').show();
        };

        // Helper: Get risk score CSS class
        self.getRiskScoreCss = function (score) {
            if (!score) return '';
            if (score <= 30) return 'text-success';
            if (score <= 60) return 'text-warning';
            return 'text-danger';
        };

        // Helper: Format currency
        self.formatCurrency = function (amount, currency) {
            return (amount || 0).toLocaleString('tr-TR') + ' ' + (currency || 'TRY');
        };

        // Helper: Format date
        self.formatDate = function (dateStr) {
            if (!dateStr) return '-';
            return new Date(dateStr).toLocaleDateString('tr-TR');
        };

        // Helper: Calculate days remaining
        self.calculateDaysRemaining = function (repaymentDate) {
            if (!repaymentDate) return 0;
            var now = new Date();
            var target = new Date(repaymentDate);
            var diff = Math.ceil((target - now) / (1000 * 60 * 60 * 24));
            return Math.max(0, diff);
        };

        // Initial load
        self.loadStats();
        self.loadActiveInvestments();
    }

    ko.applyBindings(new MyInvestmentsViewModel(), document.getElementById('my-investments-app'));
});
