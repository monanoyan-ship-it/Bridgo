// Admin Dashboard ViewModel
function AdminDashboardViewModel() {
    var self = this;

    // Observables
    self.vendorCount = ko.observable(0);
    self.userCount = ko.observable(0);
    self.capabilityCount = ko.observable(0);
    self.roleCount = ko.observable(0);
    self.recentVendors = ko.observableArray([]);
    self.capabilityStats = ko.observableArray([]);

    // Load stats
    self.loadStats = function() {
        $.get('/api/admin/dashboard/stats')
            .done(function(data) {
                self.vendorCount(data.vendorCount);
                self.userCount(data.userCount);
                self.capabilityCount(data.capabilityCount);
                self.roleCount(data.roleCount);
            });
    };

    // Load recent vendors
    self.loadRecentVendors = function() {
        $.get('/api/admin/dashboard/recent-vendors')
            .done(function(data) {
                self.recentVendors(data);
            });
    };

    // Load capability stats
    self.loadCapabilityStats = function() {
        $.get('/api/admin/dashboard/capability-stats')
            .done(function(data) {
                self.capabilityStats(data);
            });
    };

    // Initialize
    self.init = function() {
        self.loadStats();
        self.loadRecentVendors();
        self.loadCapabilityStats();
    };

    self.init();
}

// Apply bindings
$(function() {
    ko.applyBindings(new AdminDashboardViewModel(), document.getElementById('admin-app'));
});
