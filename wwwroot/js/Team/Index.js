/// <reference path="../knockout.min.js" />

/**
 * Team Management ViewModel
 * Ekip yonetimi islemleri
 */
function TeamViewModel() {
    var self = this;

    // UI States
    self.isLoading = ko.observable(true);
    self.isSaving = ko.observable(false);
    self.errorMessage = ko.observable('');
    self.successMessage = ko.observable('');
    self.activeTab = ko.observable('members');

    // Data
    self.activeMembers = ko.observableArray([]);
    self.pendingInvitations = ko.observableArray([]);
    self.pendingRequests = ko.observableArray([]);

    // Invite Modal
    self.showInviteModal = ko.observable(false);
    self.inviteEmail = ko.observable('');
    self.inviteName = ko.observable('');
    self.inviteRole = ko.observable('2'); // Employee default

    // Approve Modal
    self.showApproveModal = ko.observable(false);
    self.selectedRequest = ko.observable(null);
    self.approveRole = ko.observable('4'); // Employee default
    self.companyRoles = ko.observableArray([]);
    self.selectedRoleIds = ko.observableArray([]);
    self.isLoadingRoles = ko.observable(false);

    // Capability'lere gore gruplanmis roller
    self.groupedRoles = ko.computed(function () {
        var roles = self.companyRoles();
        var grouped = {};

        roles.forEach(function (role) {
            var capId = role.capabilityId;
            if (!grouped[capId]) {
                grouped[capId] = {
                    capabilityId: capId,
                    capabilityName: role.capabilityName,
                    capabilityCode: role.capabilityCode,
                    roles: []
                };
            }
            grouped[capId].roles.push(role);
        });

        return Object.values(grouped);
    });

    // Reject Modal
    self.showRejectModal = ko.observable(false);
    self.rejectReason = ko.observable('');

    // ========================================
    // DATA LOADING
    // ========================================

    self.loadData = function () {
        self.isLoading(true);
        self.errorMessage('');

        // Load all data in parallel
        Promise.all([
            self.loadActiveMembers(),
            self.loadPendingInvitations(),
            self.loadPendingRequests()
        ]).then(function () {
            self.isLoading(false);
        }).catch(function (error) {
            self.errorMessage('Veriler yuklenirken hata olustu');
            self.isLoading(false);
        });
    };

    self.loadActiveMembers = function () {
        return fetch('/api/TeamApi/active')
            .then(function (r) { return r.json(); })
            .then(function (data) {
                self.activeMembers(data.map(self.mapMember));
            });
    };

    self.loadPendingInvitations = function () {
        return fetch('/api/TeamApi/invitations/pending')
            .then(function (r) { return r.json(); })
            .then(function (data) {
                self.pendingInvitations(data.map(self.mapMember));
            });
    };

    self.loadPendingRequests = function () {
        return fetch('/api/TeamApi/requests/pending')
            .then(function (r) { return r.json(); })
            .then(function (data) {
                self.pendingRequests(data.map(self.mapMember));
            });
    };

    self.mapMember = function (m) {
        var roleTexts = { 0: 'Sahip', 1: 'Yonetici', 2: 'Calisan' };
        var roleBadgeClasses = { 0: 'bg-danger', 1: 'bg-warning text-dark', 2: 'bg-secondary' };
        var sourceTexts = { 0: 'Davet', 1: 'Talep', 2: 'Domain Eslesmesi', 3: 'Kurucu' };

        return {
            id: m.id,
            vendorId: m.vendorId,
            userId: m.userId,
            email: m.email,
            name: m.name,
            role: m.role,
            roleText: roleTexts[m.role] || 'Bilinmiyor',
            roleBadgeClass: roleBadgeClasses[m.role] || 'bg-secondary',
            source: m.source,
            sourceText: sourceTexts[m.source] || 'Bilinmiyor',
            status: m.status,
            message: m.message,
            createdAt: m.createdAt,
            createdAtFormatted: self.formatDate(m.createdAt),
            expiresAt: m.expiresAt,
            expiresAtFormatted: m.expiresAt ? self.formatDate(m.expiresAt) : '-',
            isExpired: m.isExpired
        };
    };

    self.formatDate = function (dateStr) {
        if (!dateStr) return '-';
        var d = new Date(dateStr);
        return d.toLocaleDateString('tr-TR') + ' ' + d.toLocaleTimeString('tr-TR', { hour: '2-digit', minute: '2-digit' });
    };

    // ========================================
    // INVITE OPERATIONS
    // ========================================

    self.openInviteModal = function () {
        self.inviteEmail('');
        self.inviteName('');
        self.inviteRole('2');
        self.showInviteModal(true);
    };

    self.closeInviteModal = function () {
        self.showInviteModal(false);
    };

    self.sendInvitation = function () {
        if (!self.inviteEmail()) {
            self.errorMessage('E-posta zorunludur');
            return;
        }

        self.isSaving(true);
        self.errorMessage('');

        fetch('/api/TeamApi/invitations', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                email: self.inviteEmail(),
                name: self.inviteName(),
                role: parseInt(self.inviteRole())
            })
        })
            .then(function (r) {
                if (!r.ok) return r.json().then(function (e) { throw e; });
                return r.json();
            })
            .then(function (data) {
                self.successMessage('Davet gonderildi');
                self.closeInviteModal();
                self.loadPendingInvitations();
            })
            .catch(function (error) {
                self.errorMessage(error.message || 'Davet gonderilemedi');
            })
            .finally(function () {
                self.isSaving(false);
            });
    };

    self.resendInvitation = function (member) {
        self.isSaving(true);
        self.errorMessage('');

        fetch('/api/TeamApi/invitations/' + member.id + '/resend', {
            method: 'POST'
        })
            .then(function (r) {
                if (!r.ok) return r.json().then(function (e) { throw e; });
                return r.json();
            })
            .then(function (data) {
                self.successMessage('Davet yeniden gonderildi');
                self.loadPendingInvitations();
            })
            .catch(function (error) {
                self.errorMessage(error.message || 'Davet yenilenemedi');
            })
            .finally(function () {
                self.isSaving(false);
            });
    };

    self.cancelInvitation = function (member) {
        if (!confirm('Bu daveti iptal etmek istediginizden emin misiniz?')) return;

        self.isSaving(true);
        self.errorMessage('');

        fetch('/api/TeamApi/invitations/' + member.id, {
            method: 'DELETE'
        })
            .then(function (r) {
                if (!r.ok) return r.json().then(function (e) { throw e; });
                return r.json();
            })
            .then(function (data) {
                self.successMessage('Davet iptal edildi');
                self.loadPendingInvitations();
            })
            .catch(function (error) {
                self.errorMessage(error.message || 'Davet iptal edilemedi');
            })
            .finally(function () {
                self.isSaving(false);
            });
    };

    // ========================================
    // REQUEST OPERATIONS
    // ========================================

    // Approve Modal
    self.openApproveModal = function (member) {
        self.selectedRequest(member);
        self.approveRole('4'); // Employee default
        self.selectedRoleIds([]);
        self.showApproveModal(true);
        self.loadCompanyRoles();
    };

    self.closeApproveModal = function () {
        self.showApproveModal(false);
        self.selectedRequest(null);
    };

    self.loadCompanyRoles = function () {
        self.isLoadingRoles(true);
        self.companyRoles([]);

        fetch('/api/TeamApi/roles')
            .then(function (r) { return r.json(); })
            .then(function (data) {
                self.companyRoles(data);
            })
            .catch(function (error) {
                console.error('Roller yuklenemedi:', error);
            })
            .finally(function () {
                self.isLoadingRoles(false);
            });
    };

    self.submitApprove = function () {
        if (!self.selectedRequest()) return;

        self.isSaving(true);
        self.errorMessage('');

        fetch('/api/TeamApi/requests/process', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                memberId: self.selectedRequest().id,
                approve: true,
                assignedRole: parseInt(self.approveRole()),
                companyRoleIds: self.selectedRoleIds()
            })
        })
            .then(function (r) {
                if (!r.ok) return r.json().then(function (e) { throw e; });
                return r.json();
            })
            .then(function (data) {
                self.successMessage('Istek onaylandi');
                self.closeApproveModal();
                self.loadPendingRequests();
                self.loadActiveMembers();
            })
            .catch(function (error) {
                self.errorMessage(error.message || 'Istek onaylanamadi');
            })
            .finally(function () {
                self.isSaving(false);
            });
    };

    // Reject Modal
    self.openRejectModal = function (member) {
        self.selectedRequest(member);
        self.rejectReason('');
        self.showRejectModal(true);
    };

    self.closeRejectModal = function () {
        self.showRejectModal(false);
        self.selectedRequest(null);
    };

    self.submitReject = function () {
        if (!self.selectedRequest()) return;

        self.isSaving(true);
        self.errorMessage('');

        fetch('/api/TeamApi/requests/process', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                memberId: self.selectedRequest().id,
                approve: false,
                rejectionReason: self.rejectReason()
            })
        })
            .then(function (r) {
                if (!r.ok) return r.json().then(function (e) { throw e; });
                return r.json();
            })
            .then(function (data) {
                self.successMessage('Istek reddedildi');
                self.closeRejectModal();
                self.loadPendingRequests();
            })
            .catch(function (error) {
                self.errorMessage(error.message || 'Istek reddedilemedi');
            })
            .finally(function () {
                self.isSaving(false);
            });
    };

    // ========================================
    // INIT
    // ========================================

    self.init = function () {
        self.loadData();
    };
}

// Initialize on DOM ready
document.addEventListener('DOMContentLoaded', function () {
    var vm = new TeamViewModel();
    ko.applyBindings(vm, document.getElementById('team-app'));
    vm.init();
});
