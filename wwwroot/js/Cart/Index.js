(function () {
    'use strict';

    // Format currency helper (global)
    window.formatCurrency = function (amount, currency) {
        return (amount || 0).toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + ' ' + (currency || 'TRY');
    };

    // Cart Item ViewModel
    function CartItemViewModel(data, parent) {
        var self = this;
        self.id = data.id;
        self.productId = data.productId;
        self.productName = data.productName;
        self.productSlug = data.productSlug || data.productId;
        self.productSku = data.productSku;
        self.productImageUrl = data.productImageUrl;
        self.quantity = ko.observable(data.quantity);
        self.unitPrice = ko.observable(data.unitPrice);
        self.currency = data.currency || 'TRY';
        self.note = data.note;
        self.minQuantity = data.minQuantity || 1;
        self.inStock = data.inStock;
        self.availableStock = data.availableStock;

        self.totalPrice = ko.computed(function () {
            return self.quantity() * self.unitPrice();
        });

        self.parent = parent;
    }

    // Cart ViewModel (single cart = single seller + warehouse)
    function CartViewModel(data, root) {
        var self = this;
        self.id = data.id;
        self.sellerVendorId = data.sellerVendorId;
        self.sellerVendorName = data.sellerVendorName;
        self.sellerIsVerified = data.sellerIsVerified;
        self.sourceWarehouseId = data.sourceWarehouseId;
        self.sourceWarehouseName = data.sourceWarehouseName;
        self.sourceWarehouseCity = data.sourceWarehouseCity;
        self.deliveryAddressId = data.deliveryAddressId;
        self.deliveryAddressTitle = data.deliveryAddressTitle;
        self.deliveryAddressFull = data.deliveryAddressFull;
        self.lastUpdatedAt = data.lastUpdatedAt;

        self.items = ko.observableArray(
            (data.items || []).map(function (item) {
                return new CartItemViewModel(item, self);
            })
        );

        self.currency = ko.observable(data.currency || 'TRY');

        self.totalItemCount = ko.computed(function () {
            return self.items().reduce(function (sum, item) {
                return sum + item.quantity();
            }, 0);
        });

        self.totalAmount = ko.computed(function () {
            return self.items().reduce(function (sum, item) {
                return sum + item.totalPrice();
            }, 0);
        });

        self.root = root;

        // Increase quantity
        self.increaseQuantity = function (item) {
            item.quantity(item.quantity() + 1);
            self.updateQuantity(item);
        };

        // Decrease quantity
        self.decreaseQuantity = function (item) {
            if (item.quantity() > item.minQuantity) {
                item.quantity(item.quantity() - 1);
                self.updateQuantity(item);
            }
        };

        // Update quantity
        self.updateQuantity = function (item) {
            var qty = item.quantity();
            if (qty < item.minQuantity) {
                item.quantity(item.minQuantity);
                qty = item.minQuantity;
            }

            $.ajax({
                url: '/api/cart/items/' + item.id,
                type: 'PUT',
                contentType: 'application/json',
                data: JSON.stringify({ quantity: qty }),
                success: function (result) {
                    // Fiyati API'den yeniden al
                    $.get('/api/cart/product-info/' + item.productId, { quantity: qty })
                        .done(function (info) {
                            item.unitPrice(info.unitPrice);
                        });
                    root.updateCartBadge();
                },
                error: function (xhr) {
                    var msg = xhr.responseJSON?.message || 'Guncelleme basarisiz';
                    toastr.error(msg);
                    root.loadCarts(); // Reload to get correct data
                }
            });
        };

        // Remove item
        self.removeItem = function (item) {
            showDeleteConfirm(item.productName, function () {
                $.ajax({
                    url: '/api/cart/items/' + item.id,
                    type: 'DELETE',
                    success: function () {
                        self.items.remove(item);
                        toastr.success('Urun sepetten cikarildi');
                        root.updateCartBadge();

                        // Sepet bos kaldiysa listeden cikar
                        if (self.items().length === 0) {
                            root.carts.remove(self);
                        }
                    },
                    error: function (xhr) {
                        var msg = xhr.responseJSON?.message || 'Silme basarisiz';
                        toastr.error(msg);
                    }
                });
            });
        };
    }

    // Main ViewModel
    function CartPageViewModel() {
        var self = this;

        self.loading = ko.observable(true);
        self.carts = ko.observableArray([]);

        self.totalItemCount = ko.computed(function () {
            return self.carts().reduce(function (sum, cart) {
                return sum + cart.totalItemCount();
            }, 0);
        });

        self.grandTotal = ko.computed(function () {
            return self.carts().reduce(function (sum, cart) {
                return sum + cart.totalAmount();
            }, 0);
        });

        // Load carts
        self.loadCarts = function () {
            self.loading(true);

            $.get('/api/cart')
                .done(function (data) {
                    var cartViewModels = (data.carts || []).map(function (cart) {
                        return new CartViewModel(cart, self);
                    });
                    self.carts(cartViewModels);
                })
                .fail(function (xhr) {
                    console.error('Sepetler yuklenemedi', xhr);
                    toastr.error('Sepetler yuklenirken hata olustu');
                })
                .always(function () {
                    self.loading(false);
                });
        };

        // Clear cart
        self.clearCart = function (cart) {
            showConfirmModal({
                title: 'Sepeti Temizle',
                message: cart.sellerVendorName + ' saticisindan alinan tum urunler sepetten cikarilacak. Emin misiniz?',
                type: 'danger',
                confirmText: 'Temizle',
                confirmIcon: 'bi bi-trash',
                onConfirm: function () {
                    $.ajax({
                        url: '/api/cart/' + cart.id,
                        type: 'DELETE',
                        success: function () {
                            self.carts.remove(cart);
                            toastr.success('Sepet temizlendi');
                            self.updateCartBadge();
                        },
                        error: function (xhr) {
                            var msg = xhr.responseJSON?.message || 'Sepet temizlenemedi';
                            toastr.error(msg);
                        }
                    });
                }
            });
        };

        // Update cart badge
        self.updateCartBadge = function () {
            $.get('/api/cart/count')
                .done(function (data) {
                    var badge = $('#cartBadge');
                    if (badge.length) {
                        if (data.count > 0) {
                            badge.text(data.count).show();
                        } else {
                            badge.hide();
                        }
                    }
                });
        };

        // Initialize
        self.loadCarts();
    }

    // Fallback for showDeleteConfirm
    if (typeof showDeleteConfirm === 'undefined') {
        window.showDeleteConfirm = function (itemName, callback) {
            if (confirm('"' + itemName + '" silinecek. Emin misiniz?')) {
                callback();
            }
        };
    }

    // Fallback for showConfirmModal
    if (typeof showConfirmModal === 'undefined') {
        window.showConfirmModal = function (options) {
            if (confirm(options.message)) {
                options.onConfirm && options.onConfirm();
            }
        };
    }

    // Apply bindings
    $(function () {
        var container = document.getElementById('cart-app');
        if (container) {
            ko.applyBindings(new CartPageViewModel(), container);
            // Show content after binding
            $(container).find('[data-bind]').show();
        }
    });

})();
