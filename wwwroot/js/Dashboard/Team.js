/**
 * Team.js - Ekip Yonetimi
 */

(function () {
    'use strict';

    // Kaynak isimleri (TeamMemberSource - sabit)
    var sourceNames = {
        'Invitation': 'Davet',
        'JoinRequest': 'Istek',
        'DomainMatch': 'Domain',
        'OwnerCreated': 'Kurucu',
        'ManualEntry': 'Manuel'
    };

    function TeamViewModel() {
        var self = this;

        // ============================================
        // TYPE DEFINITIONS (API'den yuklenecek)
        // ============================================

        self.memberTypes = ko.observableArray([]);
        self.verificationStatuses = ko.observableArray([]);

        // ============================================
        // OBSERVABLES
        // ============================================

        self.members = ko.observableArray([]);
        self.isLoading = ko.observable(false);
        self.isProcessing = ko.observable(false);
        self.editingMemberId = ko.observable(null);

        // Invite form
        self.inviteForm = {
            email: ko.observable(''),
            name: ko.observable(''),
            role: ko.observable('4')
        };

        // Member form
        self.memberForm = {
            memberTypeId: ko.observable('0'),
            role: ko.observable('4'),
            name: ko.observable(''),
            title: ko.observable(''),
            email: ko.observable(''),
            phone: ko.observable(''),
            identityNumber: ko.observable(''),
            sharePercentage: ko.observable(null),
            isAuthorizedSignatory: ko.observable(false),
            isLegalRepresentative: ko.observable(false)
        };

        // ============================================
        // COMPUTED - Filtered Lists
        // ============================================

        self.activeMembers = ko.computed(function () {
            return self.members().filter(function (m) {
                return m.teamMemberStatusId === 2 && m.memberTypeId === 0; // Active + Employee type
            });
        });

        self.corporateMembers = ko.computed(function () {
            return self.members().filter(function (m) {
                return m.memberTypeId > 0; // Not Employee
            });
        });

        self.pendingMembers = ko.computed(function () {
            return self.members().filter(function (m) {
                return m.teamMemberStatusId === 1; // Pending
            });
        });

        // Sadece kurumsal tipler (Employee haric) - modal dropdown icin
        self.corporateMemberTypes = ko.computed(function () {
            return self.memberTypes().filter(function (t) {
                return t.id > 0;
            });
        });

        // ============================================
        // HELPER FUNCTIONS
        // ============================================

        function getMemberType(id) {
            return self.memberTypes().find(function (t) { return t.id === id; });
        }

        function getVerificationStatus(id) {
            return self.verificationStatuses().find(function (t) { return t.id === id; });
        }

        function getRoleBadgeClass(source) {
            // Source'a gore badge class
            return source === 'OwnerCreated' ? 'bg-danger' : 'bg-secondary';
        }

        function getRoleName(source) {
            // Source'a gore rol ismi: Owner veya Uye
            return source === 'OwnerCreated' ? 'Owner' : 'Uye';
        }

        function mapMember(m) {
            var initials = (m.name || m.email || '??').split(' ').map(function (n) {
                return n.charAt(0).toUpperCase();
            }).slice(0, 2).join('');

            var memberType = getMemberType(m.memberTypeId) || { name: 'Calisan', cssClass: 'bg-secondary' };
            var verificationStatus = getVerificationStatus(m.verificationStatusId) || { name: 'Dogrulanmadi', cssClass: 'bg-secondary' };

            return {
                id: m.id,
                userId: m.userId,
                hasUser: !!m.userId,
                name: m.name || m.email,
                email: m.email,
                phone: m.phone,
                title: m.title,
                initials: initials,
                memberTypeId: m.memberTypeId || 0,
                memberTypeName: memberType.name,
                memberTypeBadgeClass: memberType.cssClass,
                roleName: getRoleName(m.source),
                roleBadgeClass: getRoleBadgeClass(m.source),
                source: m.source,
                sourceName: sourceNames[m.source] || m.source,
                teamMemberStatusId: m.teamMemberStatusId,
                sharePercentage: m.sharePercentage,
                isAuthorizedSignatory: m.isAuthorizedSignatory,
                isLegalRepresentative: m.isLegalRepresentative,
                identityNumber: m.identityNumber,
                verificationStatusId: m.verificationStatusId || 1,
                verificationStatusName: verificationStatus.name,
                verificationBadgeClass: verificationStatus.cssClass,
                isVerified: m.verificationStatusId === 3,
                isExpired: m.isExpired,
                createdAt: m.createdAt,
                createdAtFormatted: m.createdAt ? new Date(m.createdAt).toLocaleDateString('tr-TR') : '-'
            };
        }

        // ============================================
        // API CALLS - Types
        // ============================================

        self.loadTypes = function () {
            return $.when(
                $.get('/api/types/team-member-type'),
                $.get('/api/types/member-verification-status')
            ).done(function (typesResult, statusesResult) {
                self.memberTypes(typesResult[0] || []);
                self.verificationStatuses(statusesResult[0] || []);
            });
        };

        // ============================================
        // API CALLS - Data
        // ============================================

        self.loadData = function () {
            self.isLoading(true);

            $.get('/api/team/members')
                .done(function (data) {
                    self.members((data || []).map(mapMember));
                })
                .fail(function (xhr) {
                    console.error('Ekip uyeleri yuklenemedi:', xhr);
                    toastr.error('Ekip uyeleri yuklenemedi');
                })
                .always(function () {
                    self.isLoading(false);
                });
        };

        // ============================================
        // INVITE
        // ============================================

        self.showInviteModal = function () {
            self.inviteForm.email('');
            self.inviteForm.name('');
            self.inviteForm.role('4');
            new bootstrap.Modal(document.getElementById('inviteModal')).show();
        };

        self.sendInvitation = function () {
            if (!self.inviteForm.email()) {
                toastr.warning('E-posta zorunludur');
                return;
            }

            self.isProcessing(true);

            $.ajax({
                url: '/api/team/invite',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({
                    email: self.inviteForm.email(),
                    name: self.inviteForm.name(),
                    role: parseInt(self.inviteForm.role())
                })
            }).done(function (result) {
                if (result.success) {
                    toastr.success('Davet gonderildi');
                    bootstrap.Modal.getInstance(document.getElementById('inviteModal')).hide();
                    self.loadData();
                } else {
                    toastr.error(result.message || 'Davet gonderilemedi');
                }
            }).fail(function (xhr) {
                var msg = xhr.responseJSON?.message || 'Davet gonderilemedi';
                toastr.error(msg);
            }).always(function () {
                self.isProcessing(false);
            });
        };

        self.resendInvitation = function (member) {
            self.isProcessing(true);

            $.post('/api/team/resend/' + member.id)
                .done(function (result) {
                    toastr.success('Davet tekrar gonderildi');
                    self.loadData();
                })
                .fail(function (xhr) {
                    toastr.error('Davet gonderilemedi');
                })
                .always(function () {
                    self.isProcessing(false);
                });
        };

        self.cancelInvitation = function (member) {
            showConfirmModal({
                title: 'Davet Iptali',
                message: '"' + (member.name || member.email) + '" davetini iptal etmek istediginizden emin misiniz?',
                type: 'warning',
                confirmText: 'Iptal Et',
                confirmIcon: 'bi bi-x-lg',
                onConfirm: function () {
                    self.isProcessing(true);

                    $.ajax({
                        url: '/api/team/cancel/' + member.id,
                        method: 'DELETE'
                    }).done(function () {
                        toastr.success('Davet iptal edildi');
                        self.loadData();
                    }).fail(function () {
                        toastr.error('Davet iptal edilemedi');
                    }).always(function () {
                        self.isProcessing(false);
                    });
                }
            });
        };

        // ============================================
        // JOIN REQUESTS
        // ============================================

        self.approveRequest = function (member) {
            self.isProcessing(true);

            $.ajax({
                url: '/api/team/process/' + member.id,
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({
                    approve: true,
                    assignedRole: 4 // Employee
                })
            }).done(function () {
                toastr.success('Istek onaylandi');
                self.loadData();
            }).fail(function () {
                toastr.error('Istek onaylanamadi');
            }).always(function () {
                self.isProcessing(false);
            });
        };

        self.rejectRequest = function (member) {
            var reason = prompt('Red nedeni (istege bagli):');

            self.isProcessing(true);

            $.ajax({
                url: '/api/team/process/' + member.id,
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({
                    approve: false,
                    rejectionReason: reason
                })
            }).done(function () {
                toastr.success('Istek reddedildi');
                self.loadData();
            }).fail(function () {
                toastr.error('Istek reddedilemedi');
            }).always(function () {
                self.isProcessing(false);
            });
        };

        // ============================================
        // MEMBER CRUD
        // ============================================

        self.showAddMemberModal = function () {
            self.editingMemberId(null);
            self.memberForm.memberTypeId('1'); // Default: Representative
            self.memberForm.role('4');
            self.memberForm.name('');
            self.memberForm.title('');
            self.memberForm.email('');
            self.memberForm.phone('');
            self.memberForm.identityNumber('');
            self.memberForm.sharePercentage(null);
            self.memberForm.isAuthorizedSignatory(false);
            self.memberForm.isLegalRepresentative(false);
            new bootstrap.Modal(document.getElementById('memberModal')).show();
        };

        self.editMember = function (member) {
            self.editingMemberId(member.id);
            self.memberForm.memberTypeId(String(member.memberTypeId));
            self.memberForm.role(String(member.role));
            self.memberForm.name(member.name || '');
            self.memberForm.title(member.title || '');
            self.memberForm.email(member.email || '');
            self.memberForm.phone(member.phone || '');
            self.memberForm.identityNumber(member.identityNumber || '');
            self.memberForm.sharePercentage(member.sharePercentage);
            self.memberForm.isAuthorizedSignatory(member.isAuthorizedSignatory);
            self.memberForm.isLegalRepresentative(member.isLegalRepresentative);
            new bootstrap.Modal(document.getElementById('memberModal')).show();
        };

        self.saveMember = function () {
            if (!self.memberForm.name() || !self.memberForm.email()) {
                toastr.warning('Ad ve E-posta zorunludur');
                return;
            }

            self.isProcessing(true);

            var isEdit = self.editingMemberId() !== null;
            var url = isEdit
                ? '/api/team/members/' + self.editingMemberId()
                : '/api/team/members';
            var method = isEdit ? 'PUT' : 'POST';

            $.ajax({
                url: url,
                method: method,
                contentType: 'application/json',
                data: JSON.stringify({
                    memberTypeId: parseInt(self.memberForm.memberTypeId()),
                    role: parseInt(self.memberForm.role()),
                    name: self.memberForm.name(),
                    title: self.memberForm.title(),
                    email: self.memberForm.email(),
                    phone: self.memberForm.phone(),
                    identityNumber: self.memberForm.identityNumber(),
                    sharePercentage: self.memberForm.sharePercentage() || null,
                    isAuthorizedSignatory: self.memberForm.isAuthorizedSignatory(),
                    isLegalRepresentative: self.memberForm.isLegalRepresentative()
                })
            }).done(function () {
                toastr.success(isEdit ? 'Uye guncellendi' : 'Uye eklendi');
                bootstrap.Modal.getInstance(document.getElementById('memberModal')).hide();
                self.loadData();
            }).fail(function (xhr) {
                var msg = xhr.responseJSON?.message || (isEdit ? 'Guncelleme hatasi' : 'Ekleme hatasi');
                toastr.error(msg);
            }).always(function () {
                self.isProcessing(false);
            });
        };

        self.removeMember = function (member) {
            showDeleteConfirm(member.name, function () {
                self.isProcessing(true);

                $.ajax({
                    url: '/api/team/members/' + member.id,
                    method: 'DELETE'
                }).done(function () {
                    toastr.success('Uye cikarildi');
                    self.loadData();
                }).fail(function (xhr) {
                    var msg = xhr.responseJSON?.message || 'Cikarma hatasi';
                    toastr.error(msg);
                }).always(function () {
                    self.isProcessing(false);
                });
            });
        };

        // ============================================
        // INIT
        // ============================================

        self.loadTypes().done(function () {
            self.loadData();
        });
    }

    $(document).ready(function () {
        ko.applyBindings(new TeamViewModel(), document.getElementById('team-app'));
    });

})();
