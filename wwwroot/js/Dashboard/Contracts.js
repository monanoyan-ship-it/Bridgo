/**
 * Contracts.js - Sozlesmelerim (Order-based)
 */

(function () {
    'use strict';

    function ContractsViewModel() {
        var self = this;

        // ============================================
        // OBSERVABLES
        // ============================================

        self.contracts = ko.observableArray([]);
        self.invoices = ko.observableArray([]);
        self.selectedContract = ko.observable(null);

        // Filters
        self.contractSearch = ko.observable('');
        self.contractStatusFilter = ko.observable('');
        self.invoiceSearch = ko.observable('');
        self.invoiceTypeFilter = ko.observable('');

        // ============================================
        // COMPUTED
        // ============================================

        self.filteredContracts = ko.computed(function () {
            var search = self.contractSearch().toLowerCase();
            var status = self.contractStatusFilter();

            return self.contracts().filter(function (c) {
                var matchSearch = !search ||
                    c.contractNumber.toLowerCase().indexOf(search) >= 0 ||
                    c.orderNumber.toLowerCase().indexOf(search) >= 0;

                var matchStatus = !status || c.status === status;

                return matchSearch && matchStatus;
            });
        });

        self.filteredInvoices = ko.computed(function () {
            var search = self.invoiceSearch().toLowerCase();
            var type = self.invoiceTypeFilter();

            return self.invoices().filter(function (i) {
                var matchSearch = !search ||
                    i.invoiceNumber.toLowerCase().indexOf(search) >= 0 ||
                    i.orderNumber.toLowerCase().indexOf(search) >= 0;

                var matchType = !type || i.type === type;

                return matchSearch && matchType;
            });
        });

        // ============================================
        // STATE
        // ============================================

        self.isLoading = ko.observable(false);

        // ============================================
        // API CALLS
        // ============================================

        self.loadData = function () {
            self.isLoading(true);

            $.when(
                $.get('/api/contracts'),
                $.get('/api/contracts/invoices')
            ).done(function (contractsResult, invoicesResult) {
                var contracts = contractsResult[0] || [];
                var invoices = invoicesResult[0] || [];

                self.contracts(contracts.map(function (c) {
                    return {
                        id: c.id,
                        contractNumber: c.contractNumber || 'SZL-' + c.id,
                        orderNumber: c.orderNumber || 'SIP-' + c.orderId,
                        orderId: c.orderId,
                        counterparty: c.counterparty || c.vendorName || c.buyerName,
                        date: new Date(c.createdAt).toLocaleDateString('tr-TR'),
                        amount: c.amount?.toLocaleString('tr-TR', { style: 'currency', currency: c.currency || 'TRY' }) || '-',
                        status: c.status || 'pending',
                        statusText: c.status === 'signed' ? 'Imzalandi' :
                            c.status === 'completed' ? 'Tamamlandi' :
                                c.status === 'cancelled' ? 'Iptal' : 'Beklemede',
                        statusClass: c.status === 'signed' ? 'bg-success' :
                            c.status === 'completed' ? 'bg-info' :
                                c.status === 'cancelled' ? 'bg-danger' : 'bg-warning',
                        summary: c.summary,
                        downloadUrl: c.downloadUrl || '/api/contracts/' + c.id + '/download'
                    };
                }));

                self.invoices(invoices.map(function (i) {
                    return {
                        id: i.id,
                        invoiceNumber: i.invoiceNumber || 'FTR-' + i.id,
                        orderNumber: i.orderNumber || 'SIP-' + i.orderId,
                        orderId: i.orderId,
                        type: i.type || 'sales',
                        typeText: i.type === 'purchase' ? 'Alis' : 'Satis',
                        typeClass: i.type === 'purchase' ? 'bg-info' : 'bg-primary',
                        date: new Date(i.createdAt).toLocaleDateString('tr-TR'),
                        amount: i.amount?.toLocaleString('tr-TR', { style: 'currency', currency: i.currency || 'TRY' }) || '-',
                        status: i.status || 'issued',
                        statusText: i.status === 'paid' ? 'Odendi' :
                            i.status === 'issued' ? 'Kesildi' :
                                i.status === 'cancelled' ? 'Iptal' : 'Beklemede',
                        statusClass: i.status === 'paid' ? 'bg-success' :
                            i.status === 'issued' ? 'bg-info' :
                                i.status === 'cancelled' ? 'bg-danger' : 'bg-warning',
                        downloadUrl: i.downloadUrl || '/api/contracts/invoices/' + i.id + '/download'
                    };
                }));
            }).fail(function (xhr) {
                console.error('Sozlesmeler yuklenemedi:', xhr);
            }).always(function () {
                self.isLoading(false);
            });
        };

        // ============================================
        // CONTRACT ACTIONS
        // ============================================

        self.viewContract = function (contract) {
            self.selectedContract(contract);
            new bootstrap.Modal(document.getElementById('contractModal')).show();
        };

        self.downloadContract = function (contract) {
            window.open(contract.downloadUrl, '_blank');
        };

        self.downloadSelectedContract = function () {
            var contract = self.selectedContract();
            if (contract) {
                window.open(contract.downloadUrl, '_blank');
            }
        };

        self.signContract = function (contract) {
            if (!confirm('Bu sozlesmeyi imzalamak istediginize emin misiniz?')) return;

            $.ajax({
                url: '/api/contracts/' + contract.id + '/sign',
                method: 'POST'
            }).done(function () {
                toastr.success('Sozlesme imzalandi');
                self.loadData();
            }).fail(function (xhr) {
                var msg = xhr.responseJSON?.message || 'Imzalama hatasi';
                toastr.error(msg);
            });
        };

        // ============================================
        // INVOICE ACTIONS
        // ============================================

        self.viewInvoice = function (invoice) {
            window.open(invoice.downloadUrl, '_blank');
        };

        self.downloadInvoice = function (invoice) {
            window.open(invoice.downloadUrl, '_blank');
        };

        self.printInvoice = function (invoice) {
            var printWindow = window.open(invoice.downloadUrl, '_blank');
            printWindow.onload = function () {
                printWindow.print();
            };
        };

        // ============================================
        // INIT
        // ============================================

        self.loadData();
    }

    $(document).ready(function () {
        ko.applyBindings(new ContractsViewModel(), document.getElementById('contracts-app'));
    });

})();
