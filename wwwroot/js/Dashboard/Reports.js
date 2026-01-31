// Reports.js - Raporlama ve Analytics
(function () {
    'use strict';

    function ReportsViewModel() {
        var self = this;

        // Tab State
        self.activeTab = ko.observable('seller');

        // Loading State
        self.isLoading = ko.observable(false);

        // Filter
        self.filter = {
            fromDate: ko.observable(self.getDefaultFromDate()),
            toDate: ko.observable(self.getDefaultToDate()),
            period: ko.observable('month')
        };

        // Report Data
        self.sellerReport = ko.observable({});
        self.buyerReport = ko.observable({});

        // Charts
        self.salesChart = null;
        self.purchasesChart = null;

        // ============================================
        // HELPER METHODS
        // ============================================

        self.getDefaultFromDate = function () {
            var date = new Date();
            date.setMonth(date.getMonth() - 12);
            return date.toISOString().split('T')[0];
        };

        self.getDefaultToDate = function () {
            return new Date().toISOString().split('T')[0];
        };

        self.formatCurrency = function (amount) {
            if (!amount) return '₺0,00';
            return new Intl.NumberFormat('tr-TR', {
                style: 'currency',
                currency: 'TRY'
            }).format(amount);
        };

        self.formatDate = function (dateStr) {
            if (!dateStr) return '-';
            var date = new Date(dateStr);
            return date.toLocaleDateString('tr-TR');
        };

        // ============================================
        // API METHODS
        // ============================================

        self.loadReport = function () {
            if (self.activeTab() === 'seller') {
                self.loadSellerReport();
            } else {
                self.loadBuyerReport();
            }
        };

        self.loadSellerReport = function () {
            self.isLoading(true);

            var params = new URLSearchParams({
                fromDate: self.filter.fromDate() || '',
                toDate: self.filter.toDate() || '',
                period: self.filter.period() || 'month'
            });

            fetch('/api/reports/seller?' + params.toString())
                .then(function (response) {
                    if (!response.ok) throw new Error('Rapor yuklenemedi');
                    return response.json();
                })
                .then(function (data) {
                    self.sellerReport(data);
                    self.renderSalesChart(data.salesByPeriod || []);
                })
                .catch(function (error) {
                    console.error('Seller report error:', error);
                    self.showToast('Rapor yuklenirken hata olustu', 'error');
                })
                .finally(function () {
                    self.isLoading(false);
                });
        };

        self.loadBuyerReport = function () {
            self.isLoading(true);

            var params = new URLSearchParams({
                fromDate: self.filter.fromDate() || '',
                toDate: self.filter.toDate() || '',
                period: self.filter.period() || 'month'
            });

            fetch('/api/reports/buyer?' + params.toString())
                .then(function (response) {
                    if (!response.ok) throw new Error('Rapor yuklenemedi');
                    return response.json();
                })
                .then(function (data) {
                    self.buyerReport(data);
                    self.renderPurchasesChart(data.purchasesByPeriod || []);
                })
                .catch(function (error) {
                    console.error('Buyer report error:', error);
                    self.showToast('Rapor yuklenirken hata olustu', 'error');
                })
                .finally(function () {
                    self.isLoading(false);
                });
        };

        // ============================================
        // CHART METHODS
        // ============================================

        self.renderSalesChart = function (data) {
            var ctx = document.getElementById('salesChart');
            if (!ctx) return;

            // Destroy existing chart
            if (self.salesChart) {
                self.salesChart.destroy();
            }

            var labels = data.map(function (d) { return d.period; });
            var revenues = data.map(function (d) { return d.revenue; });
            var orders = data.map(function (d) { return d.orderCount; });

            self.salesChart = new Chart(ctx, {
                type: 'bar',
                data: {
                    labels: labels,
                    datasets: [
                        {
                            label: 'Gelir (₺)',
                            data: revenues,
                            backgroundColor: 'rgba(25, 135, 84, 0.7)',
                            borderColor: 'rgba(25, 135, 84, 1)',
                            borderWidth: 1,
                            yAxisID: 'y'
                        },
                        {
                            label: 'Siparis Sayisi',
                            data: orders,
                            type: 'line',
                            borderColor: 'rgba(13, 110, 253, 1)',
                            backgroundColor: 'rgba(13, 110, 253, 0.1)',
                            borderWidth: 2,
                            fill: true,
                            yAxisID: 'y1'
                        }
                    ]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    interaction: {
                        mode: 'index',
                        intersect: false
                    },
                    scales: {
                        y: {
                            type: 'linear',
                            display: true,
                            position: 'left',
                            title: {
                                display: true,
                                text: 'Gelir (₺)'
                            },
                            ticks: {
                                callback: function (value) {
                                    return '₺' + value.toLocaleString('tr-TR');
                                }
                            }
                        },
                        y1: {
                            type: 'linear',
                            display: true,
                            position: 'right',
                            title: {
                                display: true,
                                text: 'Siparis'
                            },
                            grid: {
                                drawOnChartArea: false
                            }
                        }
                    },
                    plugins: {
                        tooltip: {
                            callbacks: {
                                label: function (context) {
                                    var label = context.dataset.label || '';
                                    if (context.datasetIndex === 0) {
                                        return label + ': ₺' + context.raw.toLocaleString('tr-TR');
                                    }
                                    return label + ': ' + context.raw;
                                }
                            }
                        }
                    }
                }
            });
        };

        self.renderPurchasesChart = function (data) {
            var ctx = document.getElementById('purchasesChart');
            if (!ctx) return;

            // Destroy existing chart
            if (self.purchasesChart) {
                self.purchasesChart.destroy();
            }

            var labels = data.map(function (d) { return d.period; });
            var spent = data.map(function (d) { return d.spent; });
            var orders = data.map(function (d) { return d.orderCount; });

            self.purchasesChart = new Chart(ctx, {
                type: 'bar',
                data: {
                    labels: labels,
                    datasets: [
                        {
                            label: 'Harcama (₺)',
                            data: spent,
                            backgroundColor: 'rgba(220, 53, 69, 0.7)',
                            borderColor: 'rgba(220, 53, 69, 1)',
                            borderWidth: 1,
                            yAxisID: 'y'
                        },
                        {
                            label: 'Siparis Sayisi',
                            data: orders,
                            type: 'line',
                            borderColor: 'rgba(13, 110, 253, 1)',
                            backgroundColor: 'rgba(13, 110, 253, 0.1)',
                            borderWidth: 2,
                            fill: true,
                            yAxisID: 'y1'
                        }
                    ]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    interaction: {
                        mode: 'index',
                        intersect: false
                    },
                    scales: {
                        y: {
                            type: 'linear',
                            display: true,
                            position: 'left',
                            title: {
                                display: true,
                                text: 'Harcama (₺)'
                            },
                            ticks: {
                                callback: function (value) {
                                    return '₺' + value.toLocaleString('tr-TR');
                                }
                            }
                        },
                        y1: {
                            type: 'linear',
                            display: true,
                            position: 'right',
                            title: {
                                display: true,
                                text: 'Siparis'
                            },
                            grid: {
                                drawOnChartArea: false
                            }
                        }
                    },
                    plugins: {
                        tooltip: {
                            callbacks: {
                                label: function (context) {
                                    var label = context.dataset.label || '';
                                    if (context.datasetIndex === 0) {
                                        return label + ': ₺' + context.raw.toLocaleString('tr-TR');
                                    }
                                    return label + ': ' + context.raw;
                                }
                            }
                        }
                    }
                }
            });
        };

        // ============================================
        // UI METHODS
        // ============================================

        self.showToast = function (message, type) {
            type = type || 'info';
            var bgClass = type === 'error' ? 'bg-danger' : type === 'success' ? 'bg-success' : 'bg-primary';

            var toastHtml = '<div class="toast align-items-center text-white ' + bgClass + ' border-0" role="alert">' +
                '<div class="d-flex">' +
                '<div class="toast-body">' + message + '</div>' +
                '<button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>' +
                '</div></div>';

            var container = document.querySelector('.toast-container');
            if (!container) {
                container = document.createElement('div');
                container.className = 'toast-container position-fixed bottom-0 end-0 p-3';
                document.body.appendChild(container);
            }

            container.insertAdjacentHTML('beforeend', toastHtml);
            var toastEl = container.lastElementChild;
            var toast = new bootstrap.Toast(toastEl);
            toast.show();

            toastEl.addEventListener('hidden.bs.toast', function () {
                toastEl.remove();
            });
        };

        // ============================================
        // SUBSCRIPTIONS
        // ============================================

        self.activeTab.subscribe(function (newTab) {
            // Clear previous report when switching tabs
            if (newTab === 'seller') {
                self.sellerReport({});
            } else {
                self.buyerReport({});
            }
            // Load new report
            self.loadReport();
        });

        // ============================================
        // INITIALIZATION
        // ============================================

        self.init = function () {
            // Load initial report
            self.loadReport();
        };

        // Initialize on page load
        self.init();
    }

    // Make formatCurrency and formatDate available globally for bindings
    window.formatCurrency = function (amount) {
        if (!amount) return '₺0,00';
        return new Intl.NumberFormat('tr-TR', {
            style: 'currency',
            currency: 'TRY'
        }).format(amount);
    };

    window.formatDate = function (dateStr) {
        if (!dateStr) return '-';
        var date = new Date(dateStr);
        return date.toLocaleDateString('tr-TR');
    };

    // Initialize when DOM is ready
    document.addEventListener('DOMContentLoaded', function () {
        var app = document.getElementById('reports-app');
        if (app) {
            ko.applyBindings(new ReportsViewModel(), app);
        }
    });
})();
