(function () {
    'use strict';

    function AuctionsViewModel() {
        var self = this;

        // State
        self.activeTab = ko.observable('explore');
        self.isLoading = ko.observable(false);
        self.isSaving = ko.observable(false);
        self.auctions = ko.observableArray([]);
        self.categories = ko.observableArray([]);
        self.currentPage = ko.observable(1);
        self.totalPages = ko.observable(0);
        self.totalCount = ko.observable(0);

        // Filter
        self.filter = {
            search: ko.observable(''),
            sortBy: ko.observable('newest'),
            categoryId: ko.observable(null)
        };

        // Form (Create/Edit)
        self.editingId = ko.observable(null);
        self.form = {
            title: ko.observable(''),
            description: ko.observable(''),
            categoryId: ko.observable(null),
            currency: ko.observable('USD'),
            startingPrice: ko.observable(null),
            reservePrice: ko.observable(null),
            buyNowPrice: ko.observable(null),
            startAt: ko.observable(''),
            endAt: ko.observable(''),
            autoExtendMinutes: ko.observable(null)
        };

        // Bid
        self.bidAuction = ko.observable(null);
        self.bidAmount = ko.observable(null);
        self.bidMessage = ko.observable('');

        // Detail
        self.detailAuction = ko.observable(null);

        // Pagination
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

        // ============================================
        // TAB MANAGEMENT
        // ============================================

        self.setTab = function (tab) {
            if (self.activeTab() === tab) return;
            self.activeTab(tab);
            self.currentPage(1);
            self.loadAuctions();
        };

        // ============================================
        // LOAD AUCTIONS
        // ============================================

        self.loadAuctions = function () {
            self.isLoading(true);
            var tab = self.activeTab();
            var url;
            var params = {
                page: self.currentPage(),
                pageSize: 12,
                search: self.filter.search() || '',
                sortBy: self.filter.sortBy() || 'newest',
                categoryId: self.filter.categoryId() || ''
            };

            if (tab === 'explore') {
                url = '/api/auctions/public';
            } else if (tab === 'myAuctions') {
                url = '/api/auctions';
            } else if (tab === 'watchlist') {
                url = '/api/auctions/watchlist';
            }

            var queryString = Object.keys(params)
                .filter(function (k) { return params[k] !== ''; })
                .map(function (k) { return k + '=' + encodeURIComponent(params[k]); })
                .join('&');

            $.get(url + '?' + queryString)
                .done(function (data) {
                    self.auctions(data.items || []);
                    self.totalCount(data.totalCount || 0);
                    self.totalPages(data.totalPages || 0);
                })
                .fail(function () {
                    toastr.error('Veriler yuklenirken hata olustu.');
                })
                .always(function () {
                    self.isLoading(false);
                });
        };

        self.goToPage = function (page) {
            self.currentPage(page);
            self.loadAuctions();
        };

        // ============================================
        // CREATE / EDIT
        // ============================================

        self.openCreateModal = function () {
            self.editingId(null);
            self.form.title('');
            self.form.description('');
            self.form.categoryId(null);
            self.form.currency('USD');
            self.form.startingPrice(null);
            self.form.reservePrice(null);
            self.form.buyNowPrice(null);
            self.form.startAt('');
            self.form.endAt('');
            self.form.autoExtendMinutes(null);
            new bootstrap.Modal(document.getElementById('auctionFormModal')).show();
        };

        self.editAuction = function (auction) {
            self.editingId(auction.id);
            // Load detail to get all fields
            $.get('/api/auctions/' + auction.id)
                .done(function (data) {
                    self.form.title(data.title);
                    self.form.description(data.description || '');
                    self.form.categoryId(data.categoryId);
                    self.form.currency(data.currency);
                    self.form.startingPrice(data.startingPrice);
                    self.form.reservePrice(data.reservePrice);
                    self.form.buyNowPrice(data.buyNowPrice);
                    self.form.startAt(self.toLocalDateTimeInput(data.startAt));
                    self.form.endAt(self.toLocalDateTimeInput(data.endAt));
                    self.form.autoExtendMinutes(data.autoExtendMinutes);
                    new bootstrap.Modal(document.getElementById('auctionFormModal')).show();
                })
                .fail(function () {
                    toastr.error('Detay yuklenemedi.');
                });
        };

        self.saveAuction = function () {
            if (!self.form.title()) {
                toastr.warning('Baslik zorunludur.');
                return;
            }
            if (!self.form.startingPrice() || self.form.startingPrice() <= 0) {
                toastr.warning('Baslangic fiyati zorunludur.');
                return;
            }
            if (!self.form.startAt() || !self.form.endAt()) {
                toastr.warning('Baslangic ve bitis tarihleri zorunludur.');
                return;
            }

            self.isSaving(true);
            var payload = {
                title: self.form.title(),
                description: self.form.description() || null,
                categoryId: self.form.categoryId() ? parseInt(self.form.categoryId()) : null,
                currency: self.form.currency(),
                startingPrice: parseFloat(self.form.startingPrice()),
                reservePrice: self.form.reservePrice() ? parseFloat(self.form.reservePrice()) : null,
                buyNowPrice: self.form.buyNowPrice() ? parseFloat(self.form.buyNowPrice()) : null,
                startAt: new Date(self.form.startAt()).toISOString(),
                endAt: new Date(self.form.endAt()).toISOString(),
                autoExtendMinutes: self.form.autoExtendMinutes() ? parseInt(self.form.autoExtendMinutes()) : null
            };

            var url = self.editingId() ? '/api/auctions/' + self.editingId() : '/api/auctions';
            var method = self.editingId() ? 'PUT' : 'POST';

            $.ajax({ url: url, method: method, contentType: 'application/json', data: JSON.stringify(payload) })
                .done(function (data) {
                    toastr.success(data.message);
                    bootstrap.Modal.getInstance(document.getElementById('auctionFormModal')).hide();
                    self.loadAuctions();
                })
                .fail(function (xhr) {
                    var msg = xhr.responseJSON ? xhr.responseJSON.message : 'Hata olustu.';
                    toastr.error(msg);
                })
                .always(function () {
                    self.isSaving(false);
                });
        };

        // ============================================
        // ACTIVATE / CANCEL
        // ============================================

        self.activateAuction = function (auction) {
            showConfirmModal({
                title: 'Acik Artirmayi Aktiflestir',
                message: '"' + auction.title + '" acik artirmasini aktiflestirmek istediginize emin misiniz?',
                type: 'success',
                confirmText: 'Aktiflestir',
                confirmIcon: 'bi-play-circle',
                onConfirm: function () {
                    $.post('/api/auctions/' + auction.id + '/activate')
                        .done(function (data) {
                            toastr.success(data.message);
                            self.loadAuctions();
                        })
                        .fail(function (xhr) {
                            var msg = xhr.responseJSON ? xhr.responseJSON.message : 'Hata olustu.';
                            toastr.error(msg);
                        });
                }
            });
        };

        self.cancelAuction = function (auction) {
            showConfirmModal({
                title: 'Acik Artirmayi Iptal Et',
                message: '"' + auction.title + '" acik artirmasini iptal etmek istediginize emin misiniz?',
                type: 'danger',
                confirmText: 'Iptal Et',
                confirmIcon: 'bi-x-circle',
                onConfirm: function () {
                    $.post('/api/auctions/' + auction.id + '/cancel')
                        .done(function (data) {
                            toastr.success(data.message);
                            self.loadAuctions();
                        })
                        .fail(function (xhr) {
                            var msg = xhr.responseJSON ? xhr.responseJSON.message : 'Hata olustu.';
                            toastr.error(msg);
                        });
                }
            });
        };

        // ============================================
        // BIDDING
        // ============================================

        self.openBidModal = function (auction) {
            self.bidAuction(auction);
            var minBid = auction.currentHighestBid > 0 ? auction.currentHighestBid + 0.01 : auction.startingPrice + 0.01;
            self.bidAmount(Math.ceil(minBid * 100) / 100);
            self.bidMessage('');
            // Close detail modal if open
            var detailModalEl = document.getElementById('detailModal');
            var detailInstance = bootstrap.Modal.getInstance(detailModalEl);
            if (detailInstance) detailInstance.hide();

            setTimeout(function () {
                new bootstrap.Modal(document.getElementById('bidModal')).show();
            }, 300);
        };

        self.submitBid = function () {
            if (!self.bidAmount() || self.bidAmount() <= 0) {
                toastr.warning('Teklif tutari giriniz.');
                return;
            }

            self.isSaving(true);
            var payload = {
                bidAmount: parseFloat(self.bidAmount()),
                bidderMessage: self.bidMessage() || null
            };

            $.ajax({
                url: '/api/auctions/' + self.bidAuction().id + '/bid',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(payload)
            })
                .done(function (data) {
                    toastr.success(data.message);
                    bootstrap.Modal.getInstance(document.getElementById('bidModal')).hide();
                    self.loadAuctions();
                })
                .fail(function (xhr) {
                    var msg = xhr.responseJSON ? xhr.responseJSON.message : 'Hata olustu.';
                    toastr.error(msg);
                })
                .always(function () {
                    self.isSaving(false);
                });
        };

        // ============================================
        // DETAIL
        // ============================================

        self.openDetail = function (auction) {
            $.get('/api/auctions/' + auction.id)
                .done(function (data) {
                    self.detailAuction(data);
                    new bootstrap.Modal(document.getElementById('detailModal')).show();
                })
                .fail(function () {
                    toastr.error('Detay yuklenemedi.');
                });
        };

        // ============================================
        // WATCHLIST
        // ============================================

        self.toggleWatch = function (auction) {
            $.post('/api/auctions/' + auction.id + '/watch')
                .done(function (data) {
                    toastr.success(data.message);
                    self.loadAuctions();
                })
                .fail(function (xhr) {
                    var msg = xhr.responseJSON ? xhr.responseJSON.message : 'Hata olustu.';
                    toastr.error(msg);
                });
        };

        // ============================================
        // HELPERS
        // ============================================

        self.formatPrice = function (amount, currency) {
            if (amount === null || amount === undefined) return '-';
            return parseFloat(amount).toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + ' ' + (currency || 'USD');
        };

        self.formatDateTime = function (dateStr) {
            if (!dateStr) return '';
            var date = new Date(dateStr);
            return date.toLocaleDateString('tr-TR', { day: 'numeric', month: 'short', year: 'numeric', hour: '2-digit', minute: '2-digit' });
        };

        self.toLocalDateTimeInput = function (dateStr) {
            if (!dateStr) return '';
            var date = new Date(dateStr);
            var offset = date.getTimezoneOffset();
            var local = new Date(date.getTime() - offset * 60 * 1000);
            return local.toISOString().slice(0, 16);
        };

        self.getCountdown = function (endAt, statusId) {
            // Not active statuses
            if (statusId === 0) return 'Taslak';
            if (statusId === 1) return 'Planlanmis';
            if (statusId === 3) return 'Bitmis';
            if (statusId === 4) return 'Iptal';
            if (statusId === 5) return 'Satildi';

            if (!endAt) return '';
            var now = new Date();
            var end = new Date(endAt);
            var diff = end - now;

            if (diff <= 0) return 'Bitmis';

            var days = Math.floor(diff / (1000 * 60 * 60 * 24));
            var hours = Math.floor((diff % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
            var minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));

            if (days > 0) return days + 'g ' + hours + 'sa';
            if (hours > 0) return hours + 'sa ' + minutes + 'dk';
            return minutes + 'dk';
        };

        // ============================================
        // INIT
        // ============================================

        self.loadCategories = function () {
            $.get('/api/catalog/categories')
                .done(function (data) {
                    self.categories(data || []);
                });
        };

        // Check for auctionId in query string (deep link)
        var urlParams = new URLSearchParams(window.location.search);
        var auctionId = urlParams.get('auctionId');

        self.loadCategories();
        self.loadAuctions();

        if (auctionId) {
            $.get('/api/auctions/' + auctionId)
                .done(function (data) {
                    self.detailAuction(data);
                    new bootstrap.Modal(document.getElementById('detailModal')).show();
                });
        }
    }

    // Apply bindings
    ko.applyBindings(new AuctionsViewModel(), document.getElementById('auctions-app'));
})();
