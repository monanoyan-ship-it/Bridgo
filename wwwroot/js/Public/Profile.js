// Public Profile Mini Website ViewModel
(function () {
    'use strict';

    function PublicProfileViewModel() {
        var self = this;

        // === STATE ===
        self.isLoading = ko.observable(true);
        self.error = ko.observable('');
        self.profile = ko.observable(null);
        self.activeTab = ko.observable('home');

        // Products tab
        self.products = ko.observableArray([]);
        self.productsLoaded = ko.observable(false);
        self.productsLoading = ko.observable(false);
        self.productPage = ko.observable(1);
        self.productTotalPages = ko.observable(1);
        self.productSearch = ko.observable('');
        self.featuredProducts = ko.observableArray([]);

        // Contact form
        self.contactForm = {
            senderName: ko.observable(''),
            senderEmail: ko.observable(''),
            senderPhone: ko.observable(''),
            subject: ko.observable(''),
            message: ko.observable('')
        };
        self.contactSending = ko.observable(false);
        self.contactSent = ko.observable(false);

        // === COMPUTED ===
        self.productPageNumbers = ko.computed(function () {
            var current = self.productPage();
            var total = self.productTotalPages();
            var pages = [];
            var start = Math.max(1, current - 2);
            var end = Math.min(total, current + 2);
            for (var i = start; i <= end; i++) {
                pages.push(i);
            }
            return pages;
        });

        // === SLUG ===
        var appElement = document.getElementById('public-profile-app');
        var slug = appElement ? appElement.getAttribute('data-slug') : null;

        // === LOAD PROFILE ===
        self.loadProfile = function () {
            if (!slug) {
                self.error('Profil bulunamadi.');
                self.isLoading(false);
                return;
            }

            fetch('/api/capability-profile/public/' + encodeURIComponent(slug))
                .then(function (r) {
                    if (r.ok) return r.json();
                    if (r.status === 404) throw new Error('Bu profil bulunamadi veya yayinda degil.');
                    throw new Error('Profil yuklenirken bir hata olustu.');
                })
                .then(function (data) {
                    self.profile(data);

                    // Update page title
                    if (data.displayName || data.companyName) {
                        document.title = (data.displayName || data.companyName) + ' - Corplynk';
                    }

                    // Load featured products if configured
                    if (data.featuredProductIds) {
                        self.loadFeaturedProducts(data.featuredProductIds, data.vendorId);
                    } else if (data.productCount > 0) {
                        // No featured products configured, load first 4 products
                        self.loadFeaturedProducts(null, data.vendorId);
                    }
                })
                .catch(function (err) {
                    self.error(err.message);
                })
                .finally(function () {
                    self.isLoading(false);
                });
        };

        // === LOAD FEATURED PRODUCTS ===
        self.loadFeaturedProducts = function (featuredIds, vendorId) {
            var url = '/api/catalog/products?vendorId=' + vendorId + '&pageSize=8&page=1';
            fetch(url)
                .then(function (r) { return r.ok ? r.json() : null; })
                .then(function (data) {
                    if (data && data.items) {
                        var items = data.items;
                        // If featuredIds specified, filter and reorder
                        if (featuredIds) {
                            var ids = featuredIds.split(',').map(function (s) { return parseInt(s.trim()); });
                            var featured = [];
                            ids.forEach(function (id) {
                                var found = items.find(function (p) { return p.id === id; });
                                if (found) featured.push(found);
                            });
                            // If not enough from featured, add more from list
                            if (featured.length < 4) {
                                items.forEach(function (p) {
                                    if (featured.length < 8 && !featured.find(function (f) { return f.id === p.id; })) {
                                        featured.push(p);
                                    }
                                });
                            }
                            self.featuredProducts(featured.slice(0, 8));
                        } else {
                            self.featuredProducts(items.slice(0, 4));
                        }
                    }
                })
                .catch(function () { /* silent fail */ });
        };

        // === LOAD PRODUCTS (Products Tab) ===
        self.loadProducts = function () {
            var p = self.profile();
            if (!p) return;

            self.productsLoading(true);
            var url = '/api/catalog/products?vendorId=' + p.vendorId
                + '&page=' + self.productPage()
                + '&pageSize=12';

            var search = self.productSearch().trim();
            if (search) {
                url += '&search=' + encodeURIComponent(search);
            }

            fetch(url)
                .then(function (r) { return r.ok ? r.json() : null; })
                .then(function (data) {
                    if (data) {
                        self.products(data.items || []);
                        self.productTotalPages(data.totalPages || 1);
                        self.productsLoaded(true);
                    }
                })
                .catch(function () {
                    self.products([]);
                })
                .finally(function () {
                    self.productsLoading(false);
                });
        };

        // === PRODUCT PAGINATION ===
        self.nextProductPage = function () {
            if (self.productPage() < self.productTotalPages()) {
                self.productPage(self.productPage() + 1);
                self.loadProducts();
            }
        };

        self.prevProductPage = function () {
            if (self.productPage() > 1) {
                self.productPage(self.productPage() - 1);
                self.loadProducts();
            }
        };

        self.goToProductPage = function (page) {
            self.productPage(page);
            self.loadProducts();
        };

        // Product search debounce
        var searchTimeout;
        self.productSearch.subscribe(function () {
            clearTimeout(searchTimeout);
            searchTimeout = setTimeout(function () {
                self.productPage(1);
                self.loadProducts();
            }, 400);
        });

        // === TAB MANAGEMENT ===
        self.setTab = function (tab) {
            self.activeTab(tab);

            // Lazy load products
            if (tab === 'products' && !self.productsLoaded()) {
                self.loadProducts();
            }

            // Update URL hash
            history.replaceState(null, null, '#' + tab);

            // Scroll to top of content
            window.scrollTo({ top: 0, behavior: 'smooth' });
        };

        // === CONTACT FORM ===
        self.submitContact = function () {
            var form = self.contactForm;
            if (!form.senderName() || !form.senderEmail() || !form.subject() || !form.message()) {
                toastr.warning('Lutfen tum zorunlu alanlari doldurun.');
                return;
            }

            self.contactSending(true);
            fetch('/api/capability-profile/public/' + encodeURIComponent(slug) + '/contact', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    senderName: form.senderName(),
                    senderEmail: form.senderEmail(),
                    senderPhone: form.senderPhone(),
                    subject: form.subject(),
                    message: form.message()
                })
            })
                .then(function (r) {
                    if (r.ok) return r.json();
                    throw new Error('Mesaj gonderilemedi.');
                })
                .then(function () {
                    self.contactSent(true);
                    toastr.success('Mesajiniz basariyla iletildi!');
                })
                .catch(function (err) {
                    toastr.error(err.message || 'Bir hata olustu.');
                })
                .finally(function () {
                    self.contactSending(false);
                });
        };

        self.resetContactForm = function () {
            self.contactForm.senderName('');
            self.contactForm.senderEmail('');
            self.contactForm.senderPhone('');
            self.contactForm.subject('');
            self.contactForm.message('');
            self.contactSent(false);
        };

        // === INIT ===
        // Read hash for deep linking
        var hash = window.location.hash.replace('#', '');
        if (['home', 'about', 'products', 'services', 'contact'].indexOf(hash) >= 0) {
            self.activeTab(hash);
        }

        // Load profile data
        self.loadProfile();
    }

    // Apply bindings when DOM ready
    $(function () {
        var appEl = document.getElementById('public-profile-app');
        if (appEl) {
            ko.applyBindings(new PublicProfileViewModel(), appEl);
        }
    });
})();
