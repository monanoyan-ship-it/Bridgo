// Public Profile ViewModel
(function () {
    'use strict';

    function PublicProfileViewModel() {
        var self = this;

        // State
        self.isLoading = ko.observable(true);
        self.error = ko.observable('');
        self.profile = ko.observable(null);

        // Get slug from data attribute
        var appElement = document.getElementById('public-profile-app');
        var slug = appElement ? appElement.getAttribute('data-slug') : null;

        // Load profile
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
                        document.title = (data.displayName || data.companyName) + ' - Bridgo';
                    }
                })
                .catch(function (err) {
                    self.error(err.message);
                })
                .finally(function () {
                    self.isLoading(false);
                });
        };

        // Initialize
        self.loadProfile();
    }

    // Apply bindings when DOM is ready
    $(function () {
        var appEl = document.getElementById('public-profile-app');
        if (appEl) {
            ko.applyBindings(new PublicProfileViewModel(), appEl);
        }
    });
})();
