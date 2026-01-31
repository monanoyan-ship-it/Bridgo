(function () {
    'use strict';

    var config = window.productConfig || {};
    var minQuantity = config.minOrderQuantity || 1;
    var currentUnitPrice = config.basePrice || 0;
    var priceTiers = config.priceTiers || [];
    var salesUnit = config.salesUnit || null;

    // Helper: Check if user is authenticated
    function isAuthenticated() {
        return config.isAuthenticated === true || config.isAuthenticated === 'true';
    }

    // Helper: Check if viewing own product
    function isOwnProduct() {
        return config.isOwnProduct === true || config.isOwnProduct === 'true';
    }

    // Helper: Satış birimi varsa girilen miktarı adete çevir
    function getActualQuantity(inputQuantity) {
        if (salesUnit && salesUnit.baseUnitQuantity > 1) {
            return inputQuantity * salesUnit.baseUnitQuantity;
        }
        return inputQuantity;
    }

    // Helper: Toplam adet bilgisini güncelle
    function updateTotalUnitsDisplay(inputQuantity) {
        if (salesUnit && salesUnit.baseUnitQuantity > 1) {
            var totalUnits = inputQuantity * salesUnit.baseUnitQuantity;
            $('#totalUnitsCount').text(totalUnits.toLocaleString('tr-TR'));
        }
    }

    // Select price tier (called when clicking on tier row)
    window.selectPriceTier = function (quantity, price) {
        // Update quantity input (giriş durumuna bakmaksızın)
        $('#cartQuantity').val(quantity);

        // Highlight selected tier
        $('.price-tier-row').removeClass('table-primary');
        $('.price-tier-row[data-min-qty="' + quantity + '"]').addClass('table-primary');

        // Update price display
        updatePriceDisplay(quantity);

        // Scroll to add to cart section
        $('html, body').animate({
            scrollTop: $('#addToCartSection').offset().top - 100
        }, 300);
    };

    // Highlight current tier based on quantity
    function highlightCurrentTier(quantity) {
        $('.price-tier-row').removeClass('table-primary');

        for (var i = priceTiers.length - 1; i >= 0; i--) {
            var tier = priceTiers[i];
            if (quantity >= tier.minQuantity) {
                $('.price-tier-row[data-min-qty="' + tier.minQuantity + '"]').addClass('table-primary');
                break;
            }
        }
    }

    // Change main image
    window.changeMainImage = function (url, alt) {
        var mainImage = document.getElementById('mainImage');
        if (mainImage) {
            mainImage.src = url;
            mainImage.alt = alt;
        }

        // Update thumbnail active state
        document.querySelectorAll('.thumbnail-item').forEach(function (item) {
            item.classList.remove('active');
            item.style.borderColor = '#dee2e6';
        });
        event.currentTarget.classList.add('active');
        event.currentTarget.style.borderColor = '#0d6efd';
    };

    // ===============================
    // CART FUNCTIONALITY
    // ===============================

    // Load user addresses
    function loadAddresses() {
        if (!isAuthenticated() || isOwnProduct()) return;

        $.get('/api/cart/addresses')
            .done(function (addresses) {
                var select = $('#cartDeliveryAddress');
                select.find('option:not(:first)').remove();

                addresses.forEach(function (addr) {
                    var text = addr.title + ' - ' + addr.fullAddress;
                    if (addr.isDefault) text += ' (Varsayilan)';
                    select.append($('<option>').val(addr.id).text(text));
                });

                // Select default if exists
                var defaultAddr = addresses.find(function (a) { return a.isDefault; });
                if (defaultAddr) {
                    select.val(defaultAddr.id);
                }
            })
            .fail(function () {
                console.log('Adresler yuklenemedi');
            });
    }

    // Load product cart info (min quantity, price)
    function loadProductCartInfo(quantity) {
        if (!isAuthenticated() || isOwnProduct()) return;

        $.get('/api/cart/product-info/' + config.productId, { quantity: quantity || 1 })
            .done(function (info) {
                minQuantity = info.minQuantity;
                currentUnitPrice = info.unitPrice;

                // Update min quantity info
                if (minQuantity > 1) {
                    $('#minQuantityInfo').text(T('Catalog.MinQuantity', 'Min. siparis: ') + minQuantity + ' ' + T('Catalog.Units', 'adet'));
                    $('#cartQuantity').attr('min', minQuantity);

                    // If current quantity is less than min, update it
                    var currentQty = parseInt($('#cartQuantity').val()) || 1;
                    if (currentQty < minQuantity) {
                        $('#cartQuantity').val(minQuantity);
                    }
                }

                updatePriceDisplay(parseInt($('#cartQuantity').val()) || minQuantity);
            });
    }

    // Update price display
    function updatePriceDisplay(quantity) {
        if (!isAuthenticated() || isOwnProduct()) return;

        $.get('/api/cart/product-info/' + config.productId, { quantity: quantity })
            .done(function (info) {
                currentUnitPrice = info.unitPrice;
                var totalPrice = info.totalPrice;

                $('#calculatedPrice').text(formatCurrency(totalPrice, config.currency));
                $('#unitPriceInfo').text(T('Catalog.UnitPrice', 'Birim fiyat: ') + formatCurrency(info.unitPrice, config.currency));
            });
    }

    // Format currency
    function formatCurrency(amount, currency) {
        return amount.toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + ' ' + (currency || 'TRY');
    }

    // Quantity increase/decrease
    $('#btnIncreaseQty').on('click', function () {
        var input = $('#cartQuantity');
        var val = parseInt(input.val()) || 1;
        input.val(val + 1);
        var actualQty = getActualQuantity(val + 1);
        updatePriceDisplay(actualQty);
        highlightCurrentTier(actualQty);
        updateTotalUnitsDisplay(val + 1);
    });

    $('#btnDecreaseQty').on('click', function () {
        var input = $('#cartQuantity');
        var val = parseInt(input.val()) || 1;
        if (val > 1) {
            input.val(val - 1);
            var actualQty = getActualQuantity(val - 1);
            updatePriceDisplay(actualQty);
            highlightCurrentTier(actualQty);
            updateTotalUnitsDisplay(val - 1);
        }
    });

    // Quantity input change
    $('#cartQuantity').on('change', function () {
        var val = parseInt($(this).val()) || 1;
        if (val < 1) {
            val = 1;
            $(this).val(val);
        }
        var actualQty = getActualQuantity(val);
        updatePriceDisplay(actualQty);
        highlightCurrentTier(actualQty);
        updateTotalUnitsDisplay(val);
    });

    // Add to cart
    $('#btnAddToCart').on('click', function () {
        var addressId = $('#cartDeliveryAddress').val();
        var inputQuantity = parseInt($('#cartQuantity').val()) || 1;
        var actualQuantity = getActualQuantity(inputQuantity); // Adet cinsinden gerçek miktar
        var note = $('#cartNote').val();

        // Validate address
        if (!addressId) {
            $('#cartDeliveryAddress').addClass('is-invalid');
            toastr.warning(T('Catalog.AddressRequired', 'Lutfen teslimat adresi secin'));
            return;
        }
        $('#cartDeliveryAddress').removeClass('is-invalid');

        // Validate quantity
        if (inputQuantity < 1) {
            var unitLabel = salesUnit ? salesUnit.name : T('Catalog.Units', 'adet');
            toastr.warning(T('Catalog.MinQuantityError', 'Minimum siparis miktari ') + '1 ' + unitLabel);
            return;
        }

        var btn = $(this);
        btn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm"></span> ' + T('Common.Processing', 'Ekleniyor...'));

        $.ajax({
            url: '/api/cart/items',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                productId: config.productId,
                quantity: actualQuantity, // Adet cinsinden gönder
                deliveryAddressId: parseInt(addressId),
                note: note
            }),
            success: function (result) {
                updateCartBadge();

                // Modal'i guncelle ve goster - satış birimi varsa o birimle göster
                var displayQty = salesUnit ? (inputQuantity + ' ' + salesUnit.name + ' (' + actualQuantity + ' adet)') : (actualQuantity + ' adet');
                $('#addedQuantity').text(displayQty);
                $('#addedPrice').text(formatCurrency(currentUnitPrice, config.currency));
                $('#addedTotal').text(formatCurrency(currentUnitPrice * actualQuantity, config.currency));

                // Cross-sell urunlerini goster
                if (result.hasSameWarehouseProducts && result.sameWarehouseProducts.length > 0) {
                    renderCrossSellProducts(result.sameWarehouseProducts, '#crossSellProducts');
                    $('#crossSellSection').show();
                } else {
                    $('#crossSellSection').hide();
                }

                // Modal'i ac
                var modal = new bootstrap.Modal(document.getElementById('addedToCartModal'));
                modal.show();
            },
            error: function (xhr) {
                var msg = xhr.responseJSON?.message || T('Catalog.AddToCartError', 'Sepete eklenirken hata olustu');
                toastr.error(msg);
            },
            complete: function () {
                btn.prop('disabled', false).html('<i class="bi bi-cart-plus"></i> ' + T('Catalog.AddToCart', 'Sepete Ekle'));
            }
        });
    });

    // Update cart badge in header
    function updateCartBadge() {
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
    }

    // ===============================
    // CROSS-SELL / SAME WAREHOUSE PRODUCTS
    // ===============================

    // Load same warehouse products
    function loadSameWarehouseProducts() {
        if (!config.productId) {
            console.log('productId bulunamadi, same warehouse products yuklenmiyor');
            return;
        }
        $.get('/api/cart/same-warehouse-products/' + config.productId, { limit: 6 })
            .done(function (products) {
                if (products && products.length > 0) {
                    renderCrossSellProducts(products, '#sameWarehouseProducts');
                    $('#sameWarehouseSection').show();
                }
            })
            .fail(function () {
                console.log('Same warehouse products yuklenemedi');
            });
    }

    // Render cross-sell products
    function renderCrossSellProducts(products, containerSelector) {
        var container = $(containerSelector);
        container.empty();

        products.forEach(function (product) {
            var imageUrl = product.imageUrl || '/images/no-image.png';
            var productUrl = '/Catalog/' + (product.slug || product.id);

            var html = '<div class="col">' +
                '<div class="card h-100 product-card-sm">' +
                '<a href="' + productUrl + '">' +
                '<div class="ratio ratio-1x1 bg-light">' +
                '<img src="' + imageUrl + '" alt="' + product.name + '" class="card-img-top object-fit-cover" loading="lazy" />' +
                '</div>' +
                '</a>' +
                '<div class="card-body p-2">' +
                '<h6 class="card-title small mb-1 text-truncate">' +
                '<a href="' + productUrl + '" class="text-decoration-none text-dark">' + product.name + '</a>' +
                '</h6>' +
                '<div class="fw-bold text-primary small">' + formatCurrency(product.price, product.currency) + '</div>' +
                '</div>' +
                '</div>' +
                '</div>';

            container.append(html);
        });
    }

    // ===============================
    // INQUIRY (TEKLIF ISTEME)
    // ===============================

    // Submit inquiry
    document.getElementById('submitInquiryBtn')?.addEventListener('click', function () {
        var form = document.getElementById('inquiryForm');
        if (!form.checkValidity()) {
            form.reportValidity();
            return;
        }

        var btn = this;
        btn.disabled = true;
        btn.innerHTML = '<span class="spinner-border spinner-border-sm"></span> Gonderiliyor...';

        var data = {
            productId: config.productId,
            quantity: parseInt(document.getElementById('inquiryQuantity').value),
            unit: document.getElementById('inquiryUnit').value,
            message: document.getElementById('inquiryMessage').value,
            deliveryCity: document.getElementById('inquiryDeliveryCity').value,
            contactName: document.getElementById('inquiryContactName').value,
            contactEmail: document.getElementById('inquiryContactEmail').value,
            contactPhone: document.getElementById('inquiryContactPhone').value
        };

        // Check if user is authenticated
        if (isAuthenticated()) {
            // Authenticated user - use API
            $.ajax({
                url: '/api/product-inquiries/my',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(data),
                success: function () {
                    toastr.success(T('Catalog.Inquiry.Success', 'Teklif talebiniz gonderildi. Satici en kisa surede size donecek.'));
                    var modal = bootstrap.Modal.getInstance(document.getElementById('inquiryModal'));
                    modal.hide();
                    form.reset();
                },
                error: function (xhr) {
                    var msg = xhr.responseJSON?.message || T('Catalog.Inquiry.Error', 'Teklif talebi gonderilemedi.');
                    toastr.error(msg);
                },
                complete: function () {
                    btn.disabled = false;
                    btn.innerHTML = '<i class="bi bi-send"></i> ' + T('Catalog.Inquiry.Send', 'Teklif Iste');
                }
            });
        } else {
            // Not authenticated - show login prompt
            toastr.warning(T('Catalog.Inquiry.LoginRequired', 'Teklif istemek icin giris yapmaniz gerekiyor.'));
            btn.disabled = false;
            btn.innerHTML = '<i class="bi bi-send"></i> ' + T('Catalog.Inquiry.Send', 'Teklif Iste');

            // Optional: redirect to login
            setTimeout(function () {
                showConfirmModal({
                    title: T('Common.LoginRequired', 'Giris Gerekli'),
                    message: T('Catalog.Inquiry.RedirectToLogin', 'Giris sayfasina yonlendirilmek ister misiniz?'),
                    type: 'info',
                    confirmText: T('Common.Login', 'Giris Yap'),
                    onConfirm: function () {
                        window.location.href = '/Account/Login?returnUrl=' + encodeURIComponent(window.location.pathname);
                    }
                });
            }, 500);
        }
    });

    // ===============================
    // IMAGE LIGHTBOX
    // ===============================

    document.getElementById('mainImage')?.addEventListener('click', function () {
        var imgSrc = this.src;
        var modal = document.createElement('div');
        modal.className = 'position-fixed top-0 start-0 w-100 h-100 d-flex align-items-center justify-content-center';
        modal.style.cssText = 'background: rgba(0,0,0,0.9); z-index: 9999; cursor: zoom-out;';
        modal.innerHTML = '<img src="' + imgSrc + '" style="max-width: 90%; max-height: 90%; object-fit: contain;" />';
        modal.onclick = function () { modal.remove(); };
        document.body.appendChild(modal);
    });

    // ===============================
    // INITIALIZATION
    // ===============================

    // T function fallback
    if (typeof T === 'undefined') {
        window.T = function (key, defaultValue) { return defaultValue; };
    }

    // showConfirmModal fallback
    if (typeof showConfirmModal === 'undefined') {
        window.showConfirmModal = function (options) {
            if (confirm(options.message)) {
                options.onConfirm && options.onConfirm();
            }
        };
    }

    // Initialize cart section
    if (isAuthenticated() && !isOwnProduct()) {
        loadAddresses();

        // Satış birimi varsa başlangıç miktarı 1 (birim), yoksa minOrderQuantity
        var initialInputQty = 1;
        var initialActualQty = getActualQuantity(initialInputQty);

        $('#cartQuantity').val(initialInputQty);
        $('#cartQuantity').attr('min', 1);
        loadProductCartInfo(initialActualQty);
        highlightCurrentTier(initialActualQty);
        updateTotalUnitsDisplay(initialInputQty);
    } else {
        // For non-authenticated users or own product, still highlight first tier
        if (priceTiers.length > 0) {
            highlightCurrentTier(priceTiers[0].minQuantity);
        }
        // Total units display için
        updateTotalUnitsDisplay(1);
    }

    // Load same warehouse products for all users
    loadSameWarehouseProducts();

})();
