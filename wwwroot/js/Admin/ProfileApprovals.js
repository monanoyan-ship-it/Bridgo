// Admin ProfileApprovals ViewModel
function ProfileApprovalsViewModel() {
    var self = this;

    // Observables
    self.profiles = ko.observableArray([]);
    self.isLoading = ko.observable(false);
    self.selectedProfile = ko.observable(null);
    self.rejectingProfileId = ko.observable(null);
    self.rejectReason = ko.observable('');

    // Modals
    self.detailModal = null;
    self.rejectModal = null;

    // Load pending profiles
    self.loadProfiles = function() {
        self.isLoading(true);
        $.get('/api/admin/profiles/pending')
            .done(function(data) {
                self.profiles(data || []);
            })
            .fail(function() {
                toastr.error('Profiller yuklenemedi');
            })
            .always(function() {
                self.isLoading(false);
            });
    };

    // Show profile details
    self.showDetails = function(profile) {
        $.get('/api/admin/profiles/' + profile.id)
            .done(function(data) {
                self.selectedProfile(data);
                self.detailModal.show();
            })
            .fail(function() {
                toastr.error('Profil detaylari yuklenemedi');
            });
    };

    // Approve profile
    self.approveProfile = function(profile) {
        showConfirmModal({
            title: 'Profili Onayla',
            message: profile.companyName + ' firmasinin profilini onaylamak istediginizden emin misiniz?',
            type: 'success',
            confirmText: 'Onayla',
            confirmIcon: 'bi bi-check-lg',
            onConfirm: function() {
                $.ajax({
                    url: '/api/admin/profiles/' + profile.id + '/approve',
                    method: 'POST'
                })
                .done(function(response) {
                    toastr.success(response.message || 'Profil onaylandi');
                    self.loadProfiles();
                })
                .fail(function(xhr) {
                    var msg = xhr.responseJSON?.message || 'Profil onaylanamadi';
                    toastr.error(msg);
                });
            }
        });
    };

    // Approve from detail modal
    self.approveFromDetail = function() {
        var profile = self.selectedProfile();
        if (!profile) return;

        $.ajax({
            url: '/api/admin/profiles/' + profile.id + '/approve',
            method: 'POST'
        })
        .done(function(response) {
            toastr.success(response.message || 'Profil onaylandi');
            self.detailModal.hide();
            self.loadProfiles();
        })
        .fail(function(xhr) {
            var msg = xhr.responseJSON?.message || 'Profil onaylanamadi';
            toastr.error(msg);
        });
    };

    // Show reject modal
    self.showRejectModal = function(profile) {
        self.rejectingProfileId(profile.id);
        self.rejectReason('');
        self.rejectModal.show();
    };

    // Reject from detail modal
    self.rejectFromDetail = function() {
        var profile = self.selectedProfile();
        if (!profile) return;

        self.detailModal.hide();
        self.rejectingProfileId(profile.id);
        self.rejectReason('');
        self.rejectModal.show();
    };

    // Execute reject
    self.executeReject = function() {
        var profileId = self.rejectingProfileId();
        var reason = self.rejectReason();

        if (!reason || !reason.trim()) {
            toastr.warning('Red sebebi zorunludur');
            return;
        }

        $.ajax({
            url: '/api/admin/profiles/' + profileId + '/reject',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ reason: reason })
        })
        .done(function(response) {
            toastr.success(response.message || 'Profil reddedildi');
            self.rejectModal.hide();
            self.loadProfiles();
        })
        .fail(function(xhr) {
            var msg = xhr.responseJSON?.message || 'Profil reddedilemedi';
            toastr.error(msg);
        });
    };

    // Initialize
    self.init = function() {
        self.detailModal = new bootstrap.Modal(document.getElementById('detailModal'));
        self.rejectModal = new bootstrap.Modal(document.getElementById('rejectModal'));
        self.loadProfiles();
    };

    self.init();
}

// Apply bindings
$(function() {
    ko.applyBindings(new ProfileApprovalsViewModel(), document.getElementById('profile-approvals-app'));
});
