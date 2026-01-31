// Provider My Quotes ViewModel
function ProviderMyQuotesViewModel() {
    var self = this;

    // State
    self.isLoading = ko.observable(true);
    self.isSaving = ko.observable(false);
    self.isWithdrawing = ko.observable(false);
    self.quotes = ko.observableArray([]);
    self.selectedQuote = ko.observable(null);
    self.selectedStatus = ko.observable('');
    self.quoteToWithdraw = ko.observable(null);

    // Quote statuses
    self.quoteStatuses = ko.observableArray([
        { id: 1, name: 'Bekliyor' },
        { id: 2, name: 'Kabul Edildi' },
        { id: 3, name: 'Reddedildi' },
        { id: 4, name: 'Suresi Doldu' },
        { id: 5, name: 'Geri Cekildi' }
    ]);

    // Edit form
    self.editForm = {
        quoteId: ko.observable(null),
        quoteAmount: ko.observable(null),
        currency: ko.observable('TRY'),
        estimatedDays: ko.observable(null),
        validUntil: ko.observable(null),
        includedServices: ko.observable(''),
        notes: ko.observable('')
    };

    // Modals
    self.editModal = null;
    self.viewModal = null;
    self.withdrawModal = null;

    // Computed - summary counts
    self.totalCount = ko.computed(function() {
        return self.quotes().length;
    });

    self.pendingCount = ko.computed(function() {
        return self.quotes().filter(function(q) { return q.status === 1; }).length;
    });

    self.acceptedCount = ko.computed(function() {
        return self.quotes().filter(function(q) { return q.status === 2 || q.isSelected; }).length;
    });

    self.rejectedCount = ko.computed(function() {
        return self.quotes().filter(function(q) { return q.status === 3; }).length;
    });

    // Format currency
    self.formatCurrency = function(amount, currency) {
        if (amount === null || amount === undefined) return '-';
        currency = currency || 'TRY';
        return new Intl.NumberFormat('tr-TR', {
            style: 'currency',
            currency: currency,
            minimumFractionDigits: 2
        }).format(amount);
    };

    // Format date
    self.formatDate = function(dateStr) {
        if (!dateStr) return '-';
        var date = new Date(dateStr);
        return date.toLocaleDateString('tr-TR');
    };

    // Format date time
    self.formatDateTime = function(dateStr) {
        if (!dateStr) return '-';
        var date = new Date(dateStr);
        return date.toLocaleDateString('tr-TR') + ' ' + date.toLocaleTimeString('tr-TR', { hour: '2-digit', minute: '2-digit' });
    };

    // Check if expiring soon (within 2 days)
    self.isExpiringSoon = function(dateStr) {
        if (!dateStr) return false;
        var date = new Date(dateStr);
        var now = new Date();
        var diff = (date - now) / (1000 * 60 * 60 * 24);
        return diff <= 2 && diff > 0;
    };

    // Status class
    self.getStatusClass = function(status) {
        switch (status) {
            case 1: return 'bg-warning text-dark'; // Pending
            case 2: return 'bg-success'; // Accepted
            case 3: return 'bg-danger'; // Rejected
            case 4: return 'bg-secondary'; // Expired
            case 5: return 'bg-secondary'; // Withdrawn
            default: return 'bg-secondary';
        }
    };

    // Load quotes
    self.loadQuotes = function() {
        self.isLoading(true);
        var url = '/api/provider/my-quotes';
        var status = self.selectedStatus();
        if (status) {
            url += '?status=' + status;
        }

        $.get(url)
            .done(function(response) {
                if (response.success) {
                    self.quotes(response.data || []);
                } else {
                    toastr.error(response.message || 'Teklifler yuklenemedi');
                }
            })
            .fail(function(xhr) {
                toastr.error('Teklifler yuklenirken hata olustu');
            })
            .always(function() {
                self.isLoading(false);
            });
    };

    // Watch status filter change
    self.selectedStatus.subscribe(function() {
        self.loadQuotes();
    });

    // Edit quote
    self.editQuote = function(quote) {
        self.selectedQuote(quote);

        // Load form
        self.editForm.quoteId(quote.id);
        self.editForm.quoteAmount(quote.quoteAmount);
        self.editForm.currency(quote.currency || 'TRY');
        self.editForm.estimatedDays(quote.estimatedDays);
        self.editForm.validUntil(quote.validUntil ? quote.validUntil.split('T')[0] : null);
        self.editForm.includedServices(quote.includedServices || '');
        self.editForm.notes(quote.notes || '');

        self.editModal.show();
    };

    // Save quote
    self.saveQuote = function() {
        if (self.isSaving()) return;

        var amount = self.editForm.quoteAmount();
        if (!amount || amount <= 0) {
            toastr.warning('Lutfen gecerli bir teklif tutari girin');
            return;
        }

        var dto = {
            quoteAmount: parseFloat(amount),
            currency: self.editForm.currency() || 'TRY',
            estimatedDays: self.editForm.estimatedDays() ? parseInt(self.editForm.estimatedDays()) : null,
            validUntil: self.editForm.validUntil() || null,
            includedServices: self.editForm.includedServices() || null,
            notes: self.editForm.notes() || null
        };

        self.isSaving(true);
        $.ajax({
            url: '/api/provider/quotes/' + self.editForm.quoteId(),
            method: 'PUT',
            contentType: 'application/json',
            data: JSON.stringify(dto)
        })
        .done(function(response) {
            if (response.success) {
                toastr.success('Teklif guncellendi');
                self.editModal.hide();
                self.loadQuotes();
            } else {
                toastr.error(response.message || 'Teklif guncellenemedi');
            }
        })
        .fail(function(xhr) {
            var error = xhr.responseJSON?.message || 'Teklif guncellenirken hata olustu';
            toastr.error(error);
        })
        .always(function() {
            self.isSaving(false);
        });
    };

    // View quote
    self.viewQuote = function(quote) {
        self.selectedQuote(quote);
        self.viewModal.show();
    };

    // Withdraw quote - show confirmation
    self.withdrawQuote = function(quote) {
        self.quoteToWithdraw(quote);
        self.withdrawModal.show();
    };

    // Confirm withdraw
    self.confirmWithdraw = function() {
        if (self.isWithdrawing()) return;

        var quote = self.quoteToWithdraw();
        if (!quote) return;

        self.isWithdrawing(true);
        $.ajax({
            url: '/api/provider/quotes/' + quote.id + '/withdraw',
            method: 'POST'
        })
        .done(function(response) {
            if (response.success) {
                toastr.success('Teklif geri cekildi');
                self.withdrawModal.hide();
                self.loadQuotes();
            } else {
                toastr.error(response.message || 'Teklif geri cekilemedi');
            }
        })
        .fail(function(xhr) {
            var error = xhr.responseJSON?.message || 'Teklif geri cekilirken hata olustu';
            toastr.error(error);
        })
        .always(function() {
            self.isWithdrawing(false);
        });
    };

    // Init
    self.editModal = new bootstrap.Modal(document.getElementById('editQuoteModal'));
    self.viewModal = new bootstrap.Modal(document.getElementById('viewQuoteModal'));
    self.withdrawModal = new bootstrap.Modal(document.getElementById('withdrawModal'));
    self.loadQuotes();
}

$(function() {
    ko.applyBindings(new ProviderMyQuotesViewModel(), document.getElementById('provider-myquotes-app'));
});
