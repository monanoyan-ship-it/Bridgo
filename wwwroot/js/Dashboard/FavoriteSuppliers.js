// Dashboard/FavoriteSuppliers.js - Favori Tedarikciler
(function () {
    'use strict';

    function FavoriteSuppliersViewModel() {
        var self = this;

        // State
        self.isLoading = ko.observable(false);
        self.isSaving = ko.observable(false);
        self.isSending = ko.observable(false);
        self.activeTab = ko.observable('orders');
        self.favorites = ko.observableArray([]);
        self.orderSuppliers = ko.observableArray([]);

        // Edit form
        self.editingId = ko.observable(null);
        self.editForm = {
            notes: ko.observable(''),
            tags: ko.observable('')
        };

        // Quote form
        self.quoteForm = {
            vendorId: ko.observable(null),
            companyName: ko.observable(''),
            subject: ko.observable(''),
            message: ko.observable('')
        };

        // Selected supplier for detail modal
        self.selectedSupplier = ko.observable(null);

        // Modals
        self.editModal = null;
        self.quoteModal = null;
        self.supplierModal = null;

        // Load favorites
        self.loadFavorites = function () {
            self.isLoading(true);

            fetch('/api/buyer/favorites')
                .then(function (response) { return response.json(); })
                .then(function (data) {
                    self.favorites(data || []);
                })
                .catch(function (err) {
                    console.error('Error loading favorites:', err);
                    toastr.error('Favoriler yuklenirken hata olustu');
                })
                .finally(function () {
                    self.isLoading(false);
                });
        };

        // Edit favorite
        self.editFavorite = function (favorite) {
            self.editingId(favorite.id);
            self.editForm.notes(favorite.notes || '');
            self.editForm.tags(favorite.tags || '');
            self.editModal.show();
        };

        // Save edit
        self.saveEdit = function () {
            if (!self.editingId()) return;

            self.isSaving(true);

            fetch('/api/buyer/favorites/' + self.editingId(), {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    notes: self.editForm.notes(),
                    tags: self.editForm.tags()
                })
            })
                .then(function (response) {
                    if (response.ok) {
                        // Update local data
                        var fav = self.favorites().find(function (f) { return f.id === self.editingId(); });
                        if (fav) {
                            fav.notes = self.editForm.notes();
                            fav.tags = self.editForm.tags();
                            self.favorites.valueHasMutated();
                        }
                        self.editModal.hide();
                        toastr.success('Kaydedildi');
                    } else {
                        return response.json().then(function (data) {
                            toastr.error(data.message || 'Islem basarisiz');
                        });
                    }
                })
                .catch(function (err) {
                    console.error('Error saving:', err);
                    toastr.error('Kayit sirasinda hata olustu');
                })
                .finally(function () {
                    self.isSaving(false);
                });
        };

        // Remove favorite
        self.removeFavorite = function (favorite) {
            showDeleteConfirm(favorite.companyName, function () {
                fetch('/api/buyer/favorites/' + favorite.id, { method: 'DELETE' })
                .then(function (response) {
                    if (response.ok) {
                        self.favorites.remove(favorite);
                        // Update order suppliers list
                        var supplier = self.orderSuppliers().find(function(s) { return s.vendorId === favorite.sellerVendorId; });
                        if (supplier) {
                            supplier.isFavorite = false;
                            self.orderSuppliers.valueHasMutated();
                        }
                        toastr.success('Favorilerden cikarildi');
                    } else {
                        return response.json().then(function (data) {
                            toastr.error(data.message || 'Islem basarisiz');
                        });
                    }
                })
                .catch(function (err) {
                    console.error('Error removing:', err);
                    toastr.error('Islem sirasinda hata olustu');
                });
            });
        };

        // Load order suppliers (suppliers from past orders)
        self.loadOrderSuppliers = function () {
            fetch('/api/buyer/order-suppliers')
                .then(function (response) { return response.json(); })
                .then(function (data) {
                    self.orderSuppliers(data || []);
                })
                .catch(function (err) {
                    console.error('Error loading order suppliers:', err);
                });
        };

        // Add to favorites from order suppliers
        self.addToFavorites = function (supplier) {
            fetch('/api/buyer/favorites', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ sellerVendorId: supplier.vendorId })
            })
            .then(function (response) {
                if (response.ok) {
                    return response.json();
                } else {
                    return response.json().then(function (data) {
                        throw new Error(data.message || 'Islem basarisiz');
                    });
                }
            })
            .then(function (data) {
                // Update supplier in list
                supplier.isFavorite = true;
                self.orderSuppliers.valueHasMutated();
                // Reload favorites
                self.loadFavorites();
                toastr.success('Favorilere eklendi');
            })
            .catch(function (err) {
                console.error('Error adding to favorites:', err);
                toastr.error(err.message || 'Favorilere eklerken hata olustu');
            });
        };

        // View supplier detail
        self.viewSupplier = function (item) {
            // Handle both favorites (sellerVendorId) and orderSuppliers (vendorId)
            var vendorId = item.sellerVendorId || item.vendorId;
            fetch('/api/buyer/suppliers/' + vendorId)
                .then(function (response) { return response.json(); })
                .then(function (data) {
                    self.selectedSupplier(data);
                    self.supplierModal.show();
                })
                .catch(function (err) {
                    console.error('Error loading supplier detail:', err);
                    toastr.error('Tedarikci bilgileri yuklenemedi');
                });
        };

        // Show quote modal
        self.showQuoteModal = function (favorite) {
            self.quoteForm.vendorId(favorite.sellerVendorId || favorite.id);
            self.quoteForm.companyName(favorite.companyName);
            self.quoteForm.subject('');
            self.quoteForm.message('');
            self.quoteModal.show();
        };

        // Send quote request
        self.sendQuoteRequest = function () {
            var subject = self.quoteForm.subject();
            var message = self.quoteForm.message();

            if (!subject || !message) {
                toastr.warning('Konu ve mesaj alanlari zorunludur');
                return;
            }

            self.isSending(true);

            fetch('/api/messages/threads', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    recipientVendorId: self.quoteForm.vendorId(),
                    subject: subject,
                    message: message,
                    threadType: 2 // Inquiry
                })
            })
                .then(function (response) {
                    if (response.ok) {
                        self.quoteModal.hide();
                        toastr.success('Teklif talebiniz gonderildi');
                    } else {
                        return response.json().then(function (data) {
                            toastr.error(data.message || 'Gonderim basarisiz');
                        });
                    }
                })
                .catch(function (err) {
                    console.error('Error sending quote:', err);
                    toastr.error('Gonderim sirasinda hata olustu');
                })
                .finally(function () {
                    self.isSending(false);
                });
        };

        // Init
        self.init = function () {
            var editModalEl = document.getElementById('editModal');
            if (editModalEl) {
                self.editModal = new bootstrap.Modal(editModalEl);
            }

            var quoteModalEl = document.getElementById('quoteModal');
            if (quoteModalEl) {
                self.quoteModal = new bootstrap.Modal(quoteModalEl);
            }

            var supplierModalEl = document.getElementById('supplierModal');
            if (supplierModalEl) {
                self.supplierModal = new bootstrap.Modal(supplierModalEl);
            }

            self.loadFavorites();
            self.loadOrderSuppliers();
        };

        self.init();
    }

    // Apply bindings
    $(function () {
        ko.applyBindings(new FavoriteSuppliersViewModel(), document.getElementById('favorite-suppliers-app'));
    });
})();
