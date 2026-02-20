(function () {
    'use strict';

    function DownloadableProductsViewModel() {
        var self = this;

        self.isLoading = ko.observable(true);
        self.products = ko.observableArray([]);

        self.loadProducts = function () {
            self.isLoading(true);
            $.get('/api/products', { isDownloadable: true })
                .done(function (data) {
                    var items = (data.items || data || []);
                    self.products(items.filter(function(p) { return p.isDownloadable; }));
                })
                .fail(function () {
                    self.products([]);
                })
                .always(function () {
                    self.isLoading(false);
                });
        };

        self.editProduct = function (item) {
            toastr.info('Urun duzenleme sayfasina yonlendiriliyorsunuz...');
        };

        // Init
        self.loadProducts();
    }

    ko.applyBindings(new DownloadableProductsViewModel(), document.getElementById('downloadable-products-app'));
})();
