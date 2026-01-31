/**
 * Bridgo Landing Page - JavaScript
 * Animations, Counter, Carousel, Tabs
 */

(function() {
    'use strict';

    // Initialize when DOM is ready
    document.addEventListener('DOMContentLoaded', function() {
        initAOS();
        initCounters();
        initValueTabs();
        initProductCarousel();
        initVideoModal();
        initSmoothScroll();
    });

    /**
     * Initialize AOS (Animate On Scroll)
     */
    function initAOS() {
        if (typeof AOS !== 'undefined') {
            AOS.init({
                duration: 800,
                easing: 'ease-out-cubic',
                once: true,
                offset: 50,
                disable: 'mobile'
            });
        }
    }

    /**
     * Counter Animation - Numbers count up when visible
     */
    function initCounters() {
        var counters = document.querySelectorAll('.counter-value');
        if (counters.length === 0) return;

        var options = {
            root: null,
            rootMargin: '0px',
            threshold: 0.5
        };

        var observer = new IntersectionObserver(function(entries) {
            entries.forEach(function(entry) {
                if (entry.isIntersecting) {
                    var counter = entry.target;
                    var target = parseInt(counter.getAttribute('data-target'), 10);
                    var suffix = counter.getAttribute('data-suffix') || '';
                    var prefix = counter.getAttribute('data-prefix') || '';

                    animateCounter(counter, target, suffix, prefix);
                    observer.unobserve(counter);
                }
            });
        }, options);

        counters.forEach(function(counter) {
            observer.observe(counter);
        });
    }

    /**
     * Animate a single counter
     */
    function animateCounter(element, target, suffix, prefix) {
        var duration = 2000;
        var startTime = null;
        var startValue = 0;

        function easeOutQuart(t) {
            return 1 - Math.pow(1 - t, 4);
        }

        function animate(currentTime) {
            if (!startTime) startTime = currentTime;
            var elapsed = currentTime - startTime;
            var progress = Math.min(elapsed / duration, 1);
            var easedProgress = easeOutQuart(progress);
            var currentValue = Math.floor(startValue + (target - startValue) * easedProgress);

            element.textContent = prefix + formatNumber(currentValue) + suffix;

            if (progress < 1) {
                requestAnimationFrame(animate);
            } else {
                element.textContent = prefix + formatNumber(target) + suffix;
            }
        }

        requestAnimationFrame(animate);
    }

    /**
     * Format number with thousand separators
     */
    function formatNumber(num) {
        return num.toString().replace(/\B(?=(\d{3})+(?!\d))/g, '.');
    }

    /**
     * Value Proposition Tabs (Buyer/Seller)
     */
    function initValueTabs() {
        var tabs = document.querySelectorAll('.value-tab');
        var contents = document.querySelectorAll('.value-content');

        tabs.forEach(function(tab) {
            tab.addEventListener('click', function() {
                var target = this.getAttribute('data-target');

                // Update active tab
                tabs.forEach(function(t) {
                    t.classList.remove('active');
                });
                this.classList.add('active');

                // Show corresponding content
                contents.forEach(function(content) {
                    if (content.id === target) {
                        content.classList.remove('d-none');
                        content.classList.add('fade-in');
                    } else {
                        content.classList.add('d-none');
                        content.classList.remove('fade-in');
                    }
                });
            });
        });
    }

    /**
     * Product Carousel
     */
    function initProductCarousel() {
        var carousel = document.querySelector('.product-carousel-inner');
        var prevBtn = document.querySelector('.carousel-nav.prev');
        var nextBtn = document.querySelector('.carousel-nav.next');

        if (!carousel || !prevBtn || !nextBtn) return;

        var scrollAmount = 300;

        prevBtn.addEventListener('click', function() {
            carousel.scrollBy({
                left: -scrollAmount,
                behavior: 'smooth'
            });
        });

        nextBtn.addEventListener('click', function() {
            carousel.scrollBy({
                left: scrollAmount,
                behavior: 'smooth'
            });
        });

        // Auto-scroll (optional)
        var autoScrollInterval = null;

        function startAutoScroll() {
            autoScrollInterval = setInterval(function() {
                if (carousel.scrollLeft + carousel.clientWidth >= carousel.scrollWidth) {
                    carousel.scrollTo({ left: 0, behavior: 'smooth' });
                } else {
                    carousel.scrollBy({ left: scrollAmount, behavior: 'smooth' });
                }
            }, 5000);
        }

        function stopAutoScroll() {
            if (autoScrollInterval) {
                clearInterval(autoScrollInterval);
            }
        }

        // Start auto-scroll
        startAutoScroll();

        // Pause on hover
        carousel.addEventListener('mouseenter', stopAutoScroll);
        carousel.addEventListener('mouseleave', startAutoScroll);
    }

    /**
     * Video Modal
     */
    function initVideoModal() {
        var playButton = document.querySelector('.play-button');
        var videoModal = document.getElementById('videoModal');

        if (!playButton || !videoModal) return;

        playButton.addEventListener('click', function() {
            var modal = new bootstrap.Modal(videoModal);
            modal.show();
        });

        // Stop video when modal closes
        videoModal.addEventListener('hidden.bs.modal', function() {
            var iframe = videoModal.querySelector('iframe');
            if (iframe) {
                var src = iframe.src;
                iframe.src = '';
                iframe.src = src;
            }
        });
    }

    /**
     * Smooth scroll for anchor links
     */
    function initSmoothScroll() {
        document.querySelectorAll('a[href^="#"]').forEach(function(anchor) {
            anchor.addEventListener('click', function(e) {
                var href = this.getAttribute('href');
                if (href === '#') return;

                var target = document.querySelector(href);
                if (target) {
                    e.preventDefault();
                    target.scrollIntoView({
                        behavior: 'smooth',
                        block: 'start'
                    });
                }
            });
        });
    }

    /**
     * API Data Loading Functions
     * These are called from the page
     */
    window.LandingPage = {
        /**
         * Load and display stats
         */
        loadStats: function(callback) {
            $.get('/api/catalog/stats', function(data) {
                if (callback) callback(data);
            }).fail(function() {
                console.warn('Failed to load stats');
            });
        },

        /**
         * Load and display featured data (categories, products)
         */
        loadFeaturedData: function(callbacks) {
            $.get('/api/catalog/featured', function(data) {
                if (callbacks.categories) callbacks.categories(data.popularCategories);
                if (callbacks.featured) callbacks.featured(data.featuredProducts);
                if (callbacks.newProducts) callbacks.newProducts(data.newProducts);
            }).fail(function() {
                console.warn('Failed to load featured data');
            });
        },

        /**
         * Format price with currency symbol
         */
        formatPrice: function(price, currency) {
            var formatted = new Intl.NumberFormat('tr-TR', {
                minimumFractionDigits: 2,
                maximumFractionDigits: 2
            }).format(price);

            var symbols = {
                'TRY': '\u20BA',
                'USD': '$',
                'EUR': '\u20AC',
                'GBP': '\u00A3'
            };

            return formatted + ' ' + (symbols[currency] || currency);
        },

        /**
         * Render category card HTML
         */
        renderCategoryCard: function(category, delay) {
            var icon = category.icon || 'bi-folder';
            return '<div class="col" data-aos="fade-up" data-aos-delay="' + (delay || 0) + '">' +
                '<a href="/Catalog/Category/' + category.slug + '" class="category-card-modern">' +
                '<div class="category-icon"><i class="bi ' + icon + '"></i></div>' +
                '<div class="category-name">' + category.name + '</div>' +
                '<div class="category-count">' + category.productCount + ' urun</div>' +
                '</a></div>';
        },

        /**
         * Render product card HTML
         */
        renderProductCard: function(product, delay) {
            var imgHtml = product.mainImageUrl
                ? '<img src="' + product.mainImageUrl + '" alt="' + product.name + '" loading="lazy">'
                : '<div class="no-image"><i class="bi bi-image"></i></div>';

            return '<div class="col" data-aos="fade-up" data-aos-delay="' + (delay || 0) + '">' +
                '<div class="product-card-modern">' +
                '<a href="/Catalog/' + product.slug + '" class="text-decoration-none">' +
                '<div class="product-image">' + imgHtml + '</div>' +
                '</a>' +
                '<div class="product-body">' +
                '<div class="product-category">' + (product.categoryName || '') + '</div>' +
                '<h6 class="product-title">' +
                '<a href="/Catalog/' + product.slug + '" class="text-decoration-none text-dark">' + product.name + '</a>' +
                '</h6>' +
                '<div class="product-price">' + this.formatPrice(product.price, product.currency) + '</div>' +
                '<div class="product-vendor"><i class="bi bi-shop"></i> ' + product.vendorName + '</div>' +
                '</div></div></div>';
        },

        /**
         * Hero search function
         */
        search: function(query) {
            if (query && query.trim()) {
                window.location.href = '/Catalog?search=' + encodeURIComponent(query.trim());
            } else {
                window.location.href = '/Catalog';
            }
        }
    };

})();
