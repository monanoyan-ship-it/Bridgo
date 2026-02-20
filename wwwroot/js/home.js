/**
 * Bridgo Home Page - LandingPage module
 */
var LandingPage = (function () {
    'use strict';

    // Init AOS
    if (typeof AOS !== 'undefined') {
        AOS.init({
            duration: 600,
            once: true,
            offset: 50
        });
    }

    // Value tabs
    document.querySelectorAll('.value-tab').forEach(function (tab) {
        tab.addEventListener('click', function () {
            document.querySelectorAll('.value-tab').forEach(function (t) { t.classList.remove('active'); });
            document.querySelectorAll('.value-content').forEach(function (c) { c.classList.add('d-none'); });
            tab.classList.add('active');
            var target = document.getElementById(tab.getAttribute('data-target'));
            if (target) target.classList.remove('d-none');
        });
    });

    // Counter animation
    function animateCounter(el, target) {
        var current = 0;
        var duration = 1500;
        var step = Math.max(1, Math.ceil(target / (duration / 16)));
        var timer = setInterval(function () {
            current += step;
            if (current >= target) {
                current = target;
                clearInterval(timer);
            }
            el.textContent = current.toLocaleString();
        }, 16);
    }

    function startCounters() {
        document.querySelectorAll('.counter-value').forEach(function (el) {
            var target = parseInt(el.getAttribute('data-target'), 10);
            if (target > 0) {
                animateCounter(el, target);
            }
        });
    }

    return {
        search: function (query) {
            if (query && query.trim()) {
                window.location.href = '/Catalog?search=' + encodeURIComponent(query.trim());
            }
        },

        loadStats: function (callback) {
            $.get('/api/catalog/stats', function (data) {
                if (callback) callback(data);
                setTimeout(startCounters, 300);
            }).fail(function () {
                // Set zeros on error
                if (callback) callback({ totalProducts: 0, totalVendors: 0, totalCategories: 0, featuredProducts: 0 });
            });
        },

        loadFeaturedData: function (callbacks) {
            // Categories
            $.get('/api/catalog/categories', function (cats) {
                if (callbacks.categories) {
                    var topCats = (cats || []).slice(0, 12);
                    callbacks.categories(topCats);
                }
            }).fail(function () {
                if (callbacks.categories) callbacks.categories([]);
            });

            // Featured products
            $.get('/api/catalog/featured', function (data) {
                if (callbacks.featured) {
                    callbacks.featured(data.featuredProducts || data.featured || []);
                }
                if (callbacks.newProducts) {
                    callbacks.newProducts(data.newProducts || data.latest || []);
                }
            }).fail(function () {
                if (callbacks.featured) callbacks.featured([]);
                if (callbacks.newProducts) callbacks.newProducts([]);
            });
        },

        renderCategoryCard: function (cat, delay) {
            var icon = cat.icon || 'bi-grid';
            var count = cat.productCount || 0;
            var slug = cat.slug || '';
            var name = cat.name || '';

            return '<div class="col" data-aos="fade-up" data-aos-delay="' + (delay || 0) + '">' +
                '<a href="/Catalog?category=' + slug + '" class="category-card">' +
                '<div class="category-icon"><i class="bi ' + icon + '"></i></div>' +
                '<div class="category-name">' + name + '</div>' +
                '<div class="category-count">' + count + ' urun</div>' +
                '</a></div>';
        },

        renderProductCard: function (p, delay) {
            var imgHtml = '';
            if (p.mainImageUrl) {
                imgHtml = '<img src="' + p.mainImageUrl + '" alt="' + (p.name || '') + '" loading="lazy" />';
            } else {
                imgHtml = '<i class="bi bi-image no-image"></i>';
            }

            var priceHtml = '';
            if (p.price && p.price > 0) {
                var currency = p.currency || 'TRY';
                var symbol = currency === 'USD' ? '$' : (currency === 'EUR' ? '€' : '₺');
                priceHtml = symbol + p.price.toLocaleString();
            }

            var slug = p.slug || p.id;
            var category = p.categoryName || '';
            var vendor = p.vendorName || '';

            return '<div class="col" data-aos="fade-up" data-aos-delay="' + (delay || 0) + '">' +
                '<div class="product-card">' +
                '<div class="product-card-img">' + imgHtml + '</div>' +
                '<div class="product-card-body">' +
                (category ? '<div class="product-card-category">' + category + '</div>' : '') +
                '<div class="product-card-title"><a href="/Catalog/Product/' + slug + '">' + (p.name || '') + '</a></div>' +
                (vendor ? '<div class="product-card-vendor"><i class="bi bi-shop me-1"></i>' + vendor + '</div>' : '') +
                (priceHtml ? '<div class="product-card-price">' + priceHtml + '</div>' : '') +
                '</div></div></div>';
        }
    };
})();
