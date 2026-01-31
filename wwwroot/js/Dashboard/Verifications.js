/**
 * Verifications.js - Dogrulamalar (Kisisel + Kurumsal)
 */

(function () {
    'use strict';

    function VerificationsViewModel() {
        var self = this;

        // ============================================
        // TYPE DEFINITIONS (API'den yuklenecek)
        // ============================================

        self.memberTypes = ko.observableArray([]);
        self.verificationStatuses = ko.observableArray([]);

        // ============================================
        // OBSERVABLES - Kisisel
        // ============================================

        self.emailVerified = ko.observable(false);
        self.phoneVerified = ko.observable(false);
        self.identityVerified = ko.observable(false);

        // ============================================
        // OBSERVABLES - Kurumsal
        // ============================================

        self.companyVerified = ko.observable(false);
        self.companyDocsVerified = ko.observable(false);
        self.hasVerifiedRepresentative = ko.observable(false);
        self.shareholdersVerified = ko.observable(false);

        self.corporateMembers = ko.observableArray([]);
        self.companyDocuments = ko.observableArray([]);

        // ============================================
        // STATE
        // ============================================

        self.isLoading = ko.observable(false);
        self.isSendingEmail = ko.observable(false);
        self.isProcessing = ko.observable(false);
        self.editingMemberId = ko.observable(null);

        // Member form
        self.memberForm = {
            memberTypeId: ko.observable('1'),
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
        // COMPUTED
        // ============================================

        self.verificationProgress = ko.computed(function () {
            var count = 0;
            if (self.emailVerified()) count++;
            if (self.phoneVerified()) count++;
            if (self.identityVerified()) count++;
            if (self.companyVerified()) count++;
            return count;
        });

        self.verificationPercent = ko.computed(function () {
            return (self.verificationProgress() / 4) * 100;
        });

        // Sadece kurumsal tipler (Employee haric)
        self.corporateMemberTypes = ko.computed(function () {
            return self.memberTypes().filter(function (t) {
                return t.id > 0; // Employee = 0 haric
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

        function mapMember(m) {
            var memberType = getMemberType(m.memberTypeId) || { name: 'Calisan', cssClass: 'bg-secondary' };
            var verificationStatus = getVerificationStatus(m.verificationStatusId) || { name: 'Dogrulanmadi', cssClass: 'bg-secondary' };

            return {
                id: m.id,
                hasUser: !!m.userId,
                name: m.name || m.email,
                email: m.email,
                title: m.title,
                phone: m.phone,
                identityNumber: m.identityNumber,
                sharePercentage: m.sharePercentage,
                memberTypeId: m.memberTypeId || 0,
                memberTypeName: memberType.name,
                memberTypeBadgeClass: memberType.cssClass,
                isAuthorizedSignatory: m.isAuthorizedSignatory,
                isLegalRepresentative: m.isLegalRepresentative,
                verificationStatusId: m.verificationStatusId || 1,
                verificationStatusName: verificationStatus.name,
                verificationBadgeClass: verificationStatus.cssClass
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

        self.loadCorporateMembers = function () {
            $.get('/api/team/members')
                .done(function (data) {
                    var members = (data || []).filter(function (m) {
                        return m.memberTypeId > 0; // Sadece kurumsal tipler
                    }).map(mapMember);

                    self.corporateMembers(members);

                    // Dogrulanmis temsilci var mi?
                    var hasVerified = members.some(function (m) {
                        return m.verificationStatusId === 3;
                    });
                    self.hasVerifiedRepresentative(hasVerified);

                    // Hissedar bilgileri tam mi?
                    var shareholders = members.filter(function (m) {
                        return m.memberTypeId === 3 || m.memberTypeId === 5; // Shareholder or UBO
                    });
                    self.shareholdersVerified(shareholders.length > 0);
                })
                .fail(function (xhr) {
                    console.error('Ekip uyeleri yuklenemedi:', xhr);
                });
        };

        self.loadVerifications = function () {
            self.isLoading(true);

            // Kisisel dogrulama
            $.get('/api/account/verifications')
                .done(function (data) {
                    self.emailVerified(data.emailVerified || false);
                    self.phoneVerified(data.phoneVerified || false);
                    self.identityVerified(data.identityVerified || false);
                    self.companyVerified(data.companyVerified || false);
                })
                .fail(function (xhr) {
                    console.error('Dogrulama bilgileri yuklenemedi:', xhr);
                });

            // Kurumsal uyeler
            self.loadCorporateMembers();

            // Firma belgeleri
            $.get('/api/documents')
                .done(function (data) {
                    self.companyDocuments((data || []).map(function (d) {
                        return {
                            id: d.id,
                            name: d.fileName || d.name,
                            typeName: d.documentType || 'Belge',
                            uploadedAtFormatted: d.createdAt ? new Date(d.createdAt).toLocaleDateString('tr-TR') : '-',
                            statusName: d.isVerified ? 'Onaylandi' : 'Beklemede',
                            statusBadgeClass: d.isVerified ? 'bg-success' : 'bg-warning text-dark'
                        };
                    }));

                    // Belgeler dogrulanmis mi?
                    var hasVerifiedDocs = (data || []).some(function (d) {
                        return d.isVerified;
                    });
                    self.companyDocsVerified(hasVerifiedDocs);
                })
                .fail(function (xhr) {
                    console.error('Belgeler yuklenemedi:', xhr);
                });

            setTimeout(function () {
                self.isLoading(false);
            }, 500);
        };

        self.sendEmailVerification = function () {
            self.isSendingEmail(true);

            $.post('/api/account/send-email-verification')
                .done(function () {
                    toastr.success('Dogrulama e-postasi gonderildi');
                })
                .fail(function (xhr) {
                    var msg = xhr.responseJSON?.message || 'E-posta gonderilemedi';
                    toastr.error(msg);
                })
                .always(function () {
                    self.isSendingEmail(false);
                });
        };

        self.showUploadDocModal = function () {
            // TODO: Belge yukleme modali
            toastr.info('Belge yukleme ozelligi yakin zamanda eklenecek');
        };

        self.deleteDocument = function (doc) {
            showDeleteConfirm(doc.name, function () {
                $.ajax({
                    url: '/api/documents/' + doc.id,
                    method: 'DELETE'
                }).done(function () {
                    self.companyDocuments.remove(doc);
                    toastr.success('Belge silindi');
                }).fail(function () {
                    toastr.error('Belge silinemedi');
                });
            });
        };

        // ============================================
        // MEMBER CRUD
        // ============================================

        self.showAddMemberModal = function () {
            self.editingMemberId(null);
            self.memberForm.memberTypeId('1'); // Default: Representative
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
                    role: 4, // Employee
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
                toastr.success(isEdit ? 'Temsilci guncellendi' : 'Temsilci eklendi');
                bootstrap.Modal.getInstance(document.getElementById('memberModal')).hide();
                self.loadCorporateMembers();
            }).fail(function (xhr) {
                var msg = xhr.responseJSON?.message || (isEdit ? 'Guncelleme hatasi' : 'Ekleme hatasi');
                toastr.error(msg);
            }).always(function () {
                self.isProcessing(false);
            });
        };

        self.removeMember = function (member) {
            showDeleteConfirm(member.name, function () {
                $.ajax({
                    url: '/api/team/members/' + member.id,
                    method: 'DELETE'
                }).done(function () {
                    toastr.success('Temsilci silindi');
                    self.loadCorporateMembers();
                }).fail(function (xhr) {
                    var msg = xhr.responseJSON?.message || 'Silme hatasi';
                    toastr.error(msg);
                });
            });
        };

        self.verifyMember = function (member) {
            showConfirmModal({
                title: 'Dogrulama Onayi',
                message: '"' + member.name + '" temsilcisini dogrulamak istediginizden emin misiniz?',
                type: 'success',
                confirmText: 'Dogrula',
                confirmIcon: 'bi bi-patch-check',
                onConfirm: function () {
                    $.ajax({
                        url: '/api/team/members/' + member.id + '/verify',
                        method: 'POST'
                    }).done(function () {
                        toastr.success('Temsilci dogrulandi');
                        self.loadCorporateMembers();
                    }).fail(function (xhr) {
                        var msg = xhr.responseJSON?.message || 'Dogrulama hatasi';
                        toastr.error(msg);
                    });
                }
            });
        };

        // ============================================
        // INIT
        // ============================================

        self.loadTypes().done(function () {
            self.loadVerifications();
        });
    }

    $(document).ready(function () {
        ko.applyBindings(new VerificationsViewModel(), document.getElementById('verifications-app'));
    });

})();
