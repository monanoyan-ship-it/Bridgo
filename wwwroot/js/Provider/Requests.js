// Provider Requests ViewModel
function ProviderRequestsViewModel() {
    var self = this;

    // State
    self.isLoading = ko.observable(true);
    self.isSubmitting = ko.observable(false);
    self.isEditing = ko.observable(false);
    self.requests = ko.observableArray([]);
    self.selectedRequest = ko.observable(null);
    self.selectedServiceType = ko.observable('');

    // Service types
    self.serviceTypes = ko.observableArray([
        { id: 1, name: 'Lojistik' },
        { id: 2, name: 'Gumruk' },
        { id: 3, name: 'Sigorta' },
        { id: 4, name: 'Gozetim' }
    ]);

    // Quote form
    self.quoteForm = {
        serviceRequestId: ko.observable(null),
        quoteId: ko.observable(null),
        quoteAmount: ko.observable(null),
        currency: ko.observable('TRY'),
        estimatedDays: ko.observable(null),
        validUntil: ko.observable(null),
        includedServices: ko.observable(''),
        notes: ko.observable('')
    };

    // Modal
    self.quoteModal = null;

    // Initialize service type from URL
    var urlServiceType = $('#provider-requests-app').data('service-type');
    if (urlServiceType) {
        self.selectedServiceType(urlServiceType.toString());
    }

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

    // Status class
    self.getStatusClass = function(status) {
        switch (status) {
            case 1: return 'bg-warning text-dark'; // Open
            case 2: return 'bg-info'; // QuotesReceived
            case 3: return 'bg-success'; // QuoteSelected
            case 4: return 'bg-danger'; // Cancelled
            default: return 'bg-secondary';
        }
    };

    // Load requests
    self.loadRequests = function() {
        self.isLoading(true);
        var url = '/api/provider/requests';
        var serviceType = self.selectedServiceType();
        if (serviceType) {
            url += '?serviceType=' + serviceType;
        }

        $.get(url)
            .done(function(response) {
                if (response.success) {
                    self.requests(response.data || []);
                } else {
                    toastr.error(response.message || 'Talepler yuklenemedi');
                }
            })
            .fail(function(xhr) {
                toastr.error('Talepler yuklenirken hata olustu');
            })
            .always(function() {
                self.isLoading(false);
            });
    };

    // Watch service type change
    self.selectedServiceType.subscribe(function() {
        self.loadRequests();
    });

    // Open quote modal
    self.openQuoteModal = function(request) {
        self.selectedRequest(request);
        self.isEditing(false);

        // Reset form
        self.quoteForm.serviceRequestId(request.id);
        self.quoteForm.quoteId(null);
        self.quoteForm.quoteAmount(null);
        self.quoteForm.currency(request.currency || 'TRY');
        self.quoteForm.estimatedDays(null);
        self.quoteForm.validUntil(null);
        self.quoteForm.includedServices('');
        self.quoteForm.notes('');

        self.quoteModal.show();
    };

    // Edit quote
    self.editQuote = function(request) {
        self.selectedRequest(request);
        self.isEditing(true);

        // Load existing quote data
        self.quoteForm.serviceRequestId(request.id);
        self.quoteForm.quoteId(request.myQuoteId);
        self.quoteForm.quoteAmount(request.myQuoteAmount);
        self.quoteForm.currency(request.currency || 'TRY');
        self.quoteForm.estimatedDays(null);
        self.quoteForm.validUntil(null);
        self.quoteForm.includedServices('');
        self.quoteForm.notes('');

        self.quoteModal.show();
    };

    // Submit quote
    self.submitQuote = function() {
        if (self.isSubmitting()) return;

        var amount = self.quoteForm.quoteAmount();
        if (!amount || amount <= 0) {
            toastr.warning('Lutfen gecerli bir teklif tutari girin');
            return;
        }

        var dto = {
            serviceRequestId: self.quoteForm.serviceRequestId(),
            quoteAmount: parseFloat(amount),
            currency: self.quoteForm.currency() || 'TRY',
            estimatedDays: self.quoteForm.estimatedDays() ? parseInt(self.quoteForm.estimatedDays()) : null,
            validUntil: self.quoteForm.validUntil() || null,
            includedServices: self.quoteForm.includedServices() || null,
            notes: self.quoteForm.notes() || null
        };

        var url, method;
        if (self.isEditing() && self.quoteForm.quoteId()) {
            url = '/api/provider/quotes/' + self.quoteForm.quoteId();
            method = 'PUT';
        } else {
            url = '/api/provider/quotes';
            method = 'POST';
        }

        self.isSubmitting(true);
        $.ajax({
            url: url,
            method: method,
            contentType: 'application/json',
            data: JSON.stringify(dto)
        })
        .done(function(response) {
            if (response.success) {
                toastr.success(self.isEditing() ? 'Teklif guncellendi' : 'Teklif gonderildi');
                self.quoteModal.hide();
                self.loadRequests();
            } else {
                toastr.error(response.message || 'Teklif gonderilemedi');
            }
        })
        .fail(function(xhr) {
            var error = xhr.responseJSON?.message || 'Teklif gonderilirken hata olustu';
            toastr.error(error);
        })
        .always(function() {
            self.isSubmitting(false);
        });
    };

    // Init
    self.quoteModal = new bootstrap.Modal(document.getElementById('quoteModal'));
    self.loadRequests();
}

$(function() {
    ko.applyBindings(new ProviderRequestsViewModel(), document.getElementById('provider-requests-app'));
});
