(function () {
    'use strict';

    function RewardPointsViewModel() {
        var self = this;

        self.isLoading = ko.observable(true);
        self.summary = ko.observable({});
        self.history = ko.observableArray([]);
        self.actionTypes = ko.observableArray([]);

        // Filter & Pagination
        self.filterActionTypeId = ko.observable('');
        self.currentPage = ko.observable(1);
        self.totalCount = ko.observable(0);
        self.pageSize = 20;

        self.totalPages = ko.computed(function () {
            return Math.ceil(self.totalCount() / self.pageSize) || 1;
        });

        self.pageNumbers = ko.computed(function () {
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

        // Watch filter changes
        self.filterActionTypeId.subscribe(function () {
            self.currentPage(1);
            self.loadHistory();
        });

        self.loadSummary = function () {
            $.get('/api/reward-points/summary')
                .done(function (data) {
                    self.summary(data || {});
                });
        };

        self.loadHistory = function () {
            var params = {
                page: self.currentPage(),
                pageSize: self.pageSize
            };

            var actionTypeId = self.filterActionTypeId();
            if (actionTypeId) {
                params.actionTypeId = actionTypeId;
            }

            $.get('/api/reward-points/history', params)
                .done(function (data) {
                    self.history(data.items || []);
                    self.totalCount(data.totalCount || 0);
                })
                .always(function () {
                    self.isLoading(false);
                });
        };

        self.loadActionTypes = function () {
            $.get('/api/reward-points/action-types')
                .done(function (data) {
                    self.actionTypes(data || []);
                });
        };

        self.refresh = function () {
            self.loadSummary();
            self.loadHistory();
            toastr.info('Yenilendi.');
        };

        self.prevPage = function () {
            if (self.currentPage() > 1) {
                self.currentPage(self.currentPage() - 1);
                self.loadHistory();
            }
        };

        self.nextPage = function () {
            if (self.currentPage() < self.totalPages()) {
                self.currentPage(self.currentPage() + 1);
                self.loadHistory();
            }
        };

        self.goToPage = function (page) {
            self.currentPage(page);
            self.loadHistory();
        };

        self.formatDateTime = function (dateStr) {
            if (!dateStr) return '';
            return new Date(dateStr).toLocaleDateString('tr-TR', {
                day: 'numeric', month: 'short', year: 'numeric',
                hour: '2-digit', minute: '2-digit'
            });
        };

        // Init
        self.loadSummary();
        self.loadHistory();
        self.loadActionTypes();
    }

    ko.applyBindings(new RewardPointsViewModel(), document.getElementById('reward-points-app'));
})();
