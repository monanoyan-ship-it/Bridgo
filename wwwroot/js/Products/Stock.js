// Products/Stock.js - Stok Yonetimi Ozet Sayfasi

(function () {
    'use strict';

    function StockViewModel() {
        var self = this;

        // State
        self.isLoading = ko.observable(true);
        self.overview = ko.observable({
            totalProducts: 0,
            productsInStock: 0,
            productsOutOfStock: 0,
            lowStockProducts: 0,
            totalStockValue: 0,
            totalWarehouses: 0
        });
        self.lowStockProducts = ko.observableArray([]);
        self.warehouseStocks = ko.observableArray([]);

        // Format currency
        self.formatCurrency = function (value) {
            if (!value) return '0 TL';
            return new Intl.NumberFormat('tr-TR', {
                style: 'currency',
                currency: 'TRY',
                minimumFractionDigits: 0,
                maximumFractionDigits: 0
            }).format(value);
        };

        // Load all data
        self.loadData = function () {
            self.isLoading(true);

            // Load all three endpoints in parallel
            Promise.all([
                fetch('/api/products/stock/overview').then(r => r.json()),
                fetch('/api/products/stock/low-stock').then(r => r.json()),
                fetch('/api/products/stock/by-warehouse').then(r => r.json())
            ]).then(function (results) {
                self.overview(results[0]);
                self.lowStockProducts(results[1]);
                self.warehouseStocks(results[2]);
                self.isLoading(false);
            }).catch(function (error) {
                console.error('Stock data load error:', error);
                self.isLoading(false);
            });
        };

        // Initialize
        self.init = function () {
            self.loadData();
        };

        self.init();
    }

    // Apply bindings when DOM is ready
    document.addEventListener('DOMContentLoaded', function () {
        var app = document.getElementById('stock-app');
        if (app) {
            ko.applyBindings(new StockViewModel(), app);
        }
    });
})();
