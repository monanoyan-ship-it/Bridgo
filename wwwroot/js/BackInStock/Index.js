(function () {
    'use strict';

    function BackInStockViewModel() {
        var self = this;

        self.isLoading = ko.observable(true);
        self.subscriptions = ko.observableArray([]);

        self.loadSubscriptions = function () {
            self.isLoading(true);
            $.get('/api/back-in-stock')
                .done(function (data) {
                    self.subscriptions(data || []);
                })
                .fail(function () {
                    toastr.error('Abonelikler yuklenemedi.');
                })
                .always(function () {
                    self.isLoading(false);
                });
        };

        self.unsubscribe = function (item) {
            showConfirmModal({
                title: 'Aboneligi Iptal Et',
                message: '"' + item.productName + '" icin stok bildirim aboneligini iptal etmek istiyor musunuz?',
                type: 'danger',
                confirmText: 'Iptal Et',
                confirmIcon: 'bi-trash',
                onConfirm: function () {
                    $.ajax({
                        url: '/api/back-in-stock/' + item.id,
                        type: 'DELETE'
                    })
                    .done(function (data) {
                        toastr.success(data.message || 'Abonelik iptal edildi.');
                        self.loadSubscriptions();
                    })
                    .fail(function (xhr) {
                        var msg = xhr.responseJSON ? xhr.responseJSON.message : 'Hata olustu.';
                        toastr.error(msg);
                    });
                }
            });
        };

        self.refresh = function () {
            self.loadSubscriptions();
            toastr.info('Yenilendi.');
        };

        self.formatDateTime = function (dateStr) {
            if (!dateStr) return '';
            return new Date(dateStr).toLocaleDateString('tr-TR', {
                day: 'numeric', month: 'short', year: 'numeric',
                hour: '2-digit', minute: '2-digit'
            });
        };

        // Init
        self.loadSubscriptions();
    }

    ko.applyBindings(new BackInStockViewModel(), document.getElementById('back-in-stock-app'));
})();
