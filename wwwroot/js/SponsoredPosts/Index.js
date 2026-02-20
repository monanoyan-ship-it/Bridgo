(function () {
    'use strict';

    function SponsoredPostsViewModel() {
        var self = this;

        self.isLoading = ko.observable(false);
        self.items = ko.observableArray([]);
        self.editingId = ko.observable(null);

        self.form = {
            socialPostId: ko.observable(''),
            budgetAmount: ko.observable(''),
            currency: ko.observable('USD'),
            targetImpressions: ko.observable(''),
            startDate: ko.observable(''),
            endDate: ko.observable('')
        };

        self.loadItems = function () {
            self.isLoading(true);
            $.get('/api/sponsored-posts')
                .done(function (data) {
                    self.items(data || []);
                })
                .fail(function () {
                    toastr.error('Sponsorlu icerikler yuklenemedi');
                })
                .always(function () {
                    self.isLoading(false);
                });
        };

        self.resetForm = function () {
            self.form.socialPostId('');
            self.form.budgetAmount('');
            self.form.currency('USD');
            self.form.targetImpressions('');
            self.form.startDate('');
            self.form.endDate('');
            self.editingId(null);
        };

        self.showCreateModal = function () {
            self.resetForm();
            var modal = new bootstrap.Modal(document.getElementById('sponsoredModal'));
            modal.show();
        };

        self.showEditModal = function (item) {
            self.editingId(item.id);
            self.form.socialPostId(item.socialPostId);
            self.form.budgetAmount(item.budgetAmount);
            self.form.currency(item.currency);
            self.form.targetImpressions(item.targetImpressions);
            self.form.startDate(item.startDate ? item.startDate.substring(0, 10) : '');
            self.form.endDate(item.endDate ? item.endDate.substring(0, 10) : '');
            var modal = new bootstrap.Modal(document.getElementById('sponsoredModal'));
            modal.show();
        };

        self.saveSponsored = function () {
            var data = {
                budgetAmount: parseFloat(self.form.budgetAmount()) || 0,
                currency: self.form.currency(),
                targetImpressions: parseInt(self.form.targetImpressions()) || 0,
                startDate: self.form.startDate() || null,
                endDate: self.form.endDate() || null
            };

            var url, method;

            if (self.editingId()) {
                url = '/api/sponsored-posts/' + self.editingId();
                method = 'PUT';
            } else {
                data.socialPostId = parseInt(self.form.socialPostId()) || 0;
                url = '/api/sponsored-posts';
                method = 'POST';
            }

            $.ajax({
                url: url,
                type: method,
                contentType: 'application/json',
                data: JSON.stringify(data)
            })
                .done(function () {
                    toastr.success(self.editingId() ? 'Sponsorluk guncellendi' : 'Sponsorluk olusturuldu');
                    bootstrap.Modal.getInstance(document.getElementById('sponsoredModal')).hide();
                    self.loadItems();
                })
                .fail(function (xhr) {
                    var msg = xhr.responseJSON?.message || 'Hata olustu';
                    toastr.error(msg);
                });
        };

        self.activate = function (item) {
            $.ajax({
                url: '/api/sponsored-posts/' + item.id + '/activate',
                type: 'POST'
            })
                .done(function () {
                    toastr.success('Sponsorluk aktif edildi');
                    self.loadItems();
                })
                .fail(function (xhr) {
                    var msg = xhr.responseJSON?.message || 'Hata olustu';
                    toastr.error(msg);
                });
        };

        self.pause = function (item) {
            $.ajax({
                url: '/api/sponsored-posts/' + item.id + '/pause',
                type: 'POST'
            })
                .done(function () {
                    toastr.success('Sponsorluk duraklatildi');
                    self.loadItems();
                })
                .fail(function (xhr) {
                    var msg = xhr.responseJSON?.message || 'Hata olustu';
                    toastr.error(msg);
                });
        };

        // Init
        self.loadItems();
    }

    $(function () {
        var app = document.getElementById('sponsored-app');
        if (app) {
            ko.applyBindings(new SponsoredPostsViewModel(), app);
        }
    });
})();
