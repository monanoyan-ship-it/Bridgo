/**
 * NotificationSettings.js - Bildirim Ayarlari
 */

(function () {
    'use strict';

    function NotificationSettingsViewModel() {
        var self = this;

        // ============================================
        // OBSERVABLES - Email Notifications
        // ============================================

        self.emailOnNewOrder = ko.observable(true);
        self.emailOnOrderStatusChange = ko.observable(true);
        self.emailOnNewInquiry = ko.observable(true);
        self.emailOnNewDemand = ko.observable(true);
        self.emailOnNewOffer = ko.observable(true);
        self.emailOnNewMessage = ko.observable(true);
        self.emailMarketing = ko.observable(false);
        self.emailWeeklyDigest = ko.observable(true);

        // ============================================
        // OBSERVABLES - In-App Notifications
        // ============================================

        self.inAppOrders = ko.observable(true);
        self.inAppMessages = ko.observable(true);
        self.inAppSystem = ko.observable(true);

        // ============================================
        // STATE
        // ============================================

        self.isLoading = ko.observable(false);
        self.isSaving = ko.observable(false);

        // ============================================
        // API CALLS
        // ============================================

        self.loadSettings = function () {
            self.isLoading(true);

            $.get('/api/account/notifications')
                .done(function (data) {
                    // Email settings
                    self.emailOnNewOrder(data.emailOnNewOrder ?? true);
                    self.emailOnOrderStatusChange(data.emailOnOrderStatusChange ?? true);
                    self.emailOnNewInquiry(data.emailOnNewInquiry ?? true);
                    self.emailOnNewDemand(data.emailOnNewDemand ?? true);
                    self.emailOnNewOffer(data.emailOnNewOffer ?? true);
                    self.emailOnNewMessage(data.emailOnNewMessage ?? true);
                    self.emailMarketing(data.emailMarketing ?? false);
                    self.emailWeeklyDigest(data.emailWeeklyDigest ?? true);

                    // In-app settings
                    self.inAppOrders(data.inAppOrders ?? true);
                    self.inAppMessages(data.inAppMessages ?? true);
                    self.inAppSystem(data.inAppSystem ?? true);
                })
                .fail(function (xhr) {
                    console.error('Bildirim ayarlari yuklenemedi:', xhr);
                    toastr.error('Ayarlar yuklenirken hata olustu');
                })
                .always(function () {
                    self.isLoading(false);
                });
        };

        self.saveSettings = function () {
            self.isSaving(true);

            $.ajax({
                url: '/api/account/notifications',
                method: 'PUT',
                contentType: 'application/json',
                data: JSON.stringify({
                    emailOnNewOrder: self.emailOnNewOrder(),
                    emailOnOrderStatusChange: self.emailOnOrderStatusChange(),
                    emailOnNewInquiry: self.emailOnNewInquiry(),
                    emailOnNewDemand: self.emailOnNewDemand(),
                    emailOnNewOffer: self.emailOnNewOffer(),
                    emailOnNewMessage: self.emailOnNewMessage(),
                    emailMarketing: self.emailMarketing(),
                    emailWeeklyDigest: self.emailWeeklyDigest(),
                    inAppOrders: self.inAppOrders(),
                    inAppMessages: self.inAppMessages(),
                    inAppSystem: self.inAppSystem()
                })
            }).done(function () {
                toastr.success('Bildirim ayarlari kaydedildi');
            }).fail(function (xhr) {
                var msg = xhr.responseJSON?.message || 'Kaydetme hatasi';
                toastr.error(msg);
            }).always(function () {
                self.isSaving(false);
            });
        };

        // ============================================
        // INIT
        // ============================================

        self.loadSettings();
    }

    $(document).ready(function () {
        ko.applyBindings(new NotificationSettingsViewModel(), document.getElementById('notifications-app'));
    });

})();
