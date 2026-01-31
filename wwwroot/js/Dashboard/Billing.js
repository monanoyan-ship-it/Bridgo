/**
 * Billing.js - Fatura ve Banka Bilgileri
 */

(function () {
    'use strict';

    function BillingViewModel() {
        var self = this;

        // ============================================
        // OBSERVABLES - Fatura Bilgileri
        // ============================================

        self.companyName = ko.observable('');
        self.taxOffice = ko.observable('');
        self.taxNumber = ko.observable('');
        self.billingAddress = ko.observable('');
        self.selectedAddressId = ko.observable(null);
        self.addresses = ko.observableArray([]);

        // ============================================
        // OBSERVABLES - Banka Hesaplari
        // ============================================

        self.bankAccounts = ko.observableArray([]);
        self.editingBankId = ko.observable(null);
        self.bankForm = {
            bankName: ko.observable(''),
            accountHolder: ko.observable(''),
            iban: ko.observable(''),
            isDefault: ko.observable(false)
        };

        // Eski referans icin (uyumluluk)
        self.newBankAccount = self.bankForm;

        // ============================================
        // OBSERVABLES - Odemeler
        // ============================================

        self.payments = ko.observableArray([]);

        // ============================================
        // STATE
        // ============================================

        self.isLoading = ko.observable(false);
        self.isSavingBilling = ko.observable(false);
        self.isAddingBank = ko.observable(false);

        // ============================================
        // API CALLS
        // ============================================

        // Adres secildiginde billingAddress'i guncelle
        self.selectedAddressId.subscribe(function (addressId) {
            if (!addressId) {
                self.useManualAddress(true);
                return;
            }

            var selectedAddress = self.addresses().find(function (a) {
                return a.id === addressId;
            });

            if (selectedAddress) {
                self.billingAddress(selectedAddress.fullAddress);
                self.useManualAddress(false);
            }
        });

        // Manuel adres girisi mi?
        self.useManualAddress = ko.observable(true);

        self.loadData = function () {
            self.isLoading(true);

            // Adresleri ayri yukle
            $.get('/api/company/addresses')
                .done(function (addresses) {
                    addresses = addresses || [];
                    self.addresses(addresses.map(function (a) {
                        return {
                            id: a.id,
                            displayText: a.title + ' - ' + (a.fullAddress || a.addressLine),
                            fullAddress: a.fullAddress || (a.addressLine + ', ' + a.district + ', ' + a.city + ', ' + a.country)
                        };
                    }));
                })
                .fail(function (xhr) {
                    console.error('Adresler yuklenemedi:', xhr);
                });

            // Fatura bilgilerini yukle
            $.get('/api/company/billing-info')
                .done(function (billing) {
                    billing = billing || {};
                    self.companyName(billing.companyName || '');
                    self.taxOffice(billing.taxOffice || '');
                    self.taxNumber(billing.taxNumber || '');
                    self.billingAddress(billing.billingAddress || '');
                    if (billing.billingAddressId) {
                        self.selectedAddressId(billing.billingAddressId);
                        self.useManualAddress(false);
                    }
                })
                .fail(function (xhr) {
                    console.error('Fatura bilgileri yuklenemedi:', xhr);
                });

            // Banka hesaplarini yukle
            $.get('/api/company/bank-accounts')
                .done(function (banks) {
                    self.bankAccounts(banks || []);
                })
                .fail(function (xhr) {
                    console.error('Banka hesaplari yuklenemedi:', xhr);
                });

            // Odemeleri yukle
            $.get('/api/company/payments')
                .done(function (payments) {
                    payments = payments || [];
                    self.payments(payments.map(function (p) {
                        return {
                            date: new Date(p.date).toLocaleDateString('tr-TR'),
                            description: p.description,
                            amount: p.amount.toLocaleString('tr-TR', { style: 'currency', currency: p.currency || 'TRY' }),
                            statusText: p.status === 'completed' ? 'Tamamlandi' : p.status === 'pending' ? 'Beklemede' : 'Iptal',
                            statusClass: p.status === 'completed' ? 'bg-success' : p.status === 'pending' ? 'bg-warning' : 'bg-danger'
                        };
                    }));
                })
                .fail(function (xhr) {
                    console.error('Odemeler yuklenemedi:', xhr);
                });

            // Loading durumunu kapat
            setTimeout(function () {
                self.isLoading(false);
            }, 500);
        };

        self.saveBillingInfo = function () {
            if (!self.companyName() || !self.taxOffice() || !self.taxNumber()) {
                toastr.warning('Tum zorunlu alanlari doldurun');
                return;
            }

            if (self.addresses().length > 0 && !self.selectedAddressId()) {
                toastr.warning('Lutfen fatura adresi secin');
                return;
            }

            self.isSavingBilling(true);

            $.ajax({
                url: '/api/company/billing-info',
                method: 'PUT',
                contentType: 'application/json',
                data: JSON.stringify({
                    companyName: self.companyName(),
                    taxOffice: self.taxOffice(),
                    taxNumber: self.taxNumber(),
                    billingAddressId: self.selectedAddressId(),
                    billingAddress: self.billingAddress()
                })
            }).done(function () {
                toastr.success('Fatura bilgileri kaydedildi');
            }).fail(function (xhr) {
                var msg = xhr.responseJSON?.message || 'Kaydetme hatasi';
                toastr.error(msg);
            }).always(function () {
                self.isSavingBilling(false);
            });
        };

        // ============================================
        // BANKA HESAPLARI
        // ============================================

        self.showAddBankModal = function () {
            self.editingBankId(null);
            self.bankForm.bankName('');
            self.bankForm.accountHolder('');
            self.bankForm.iban('');
            self.bankForm.isDefault(false);
            new bootstrap.Modal(document.getElementById('addBankModal')).show();
        };

        self.editBankAccount = function (account) {
            self.editingBankId(account.id);
            self.bankForm.bankName(account.bankName || '');
            self.bankForm.accountHolder(account.accountHolder || '');
            self.bankForm.iban(account.iban || '');
            self.bankForm.isDefault(account.isDefault || false);
            new bootstrap.Modal(document.getElementById('addBankModal')).show();
        };

        self.saveBankAccount = function () {
            var bank = self.bankForm;

            if (!bank.bankName() || !bank.accountHolder() || !bank.iban()) {
                toastr.warning('Tum alanlari doldurun');
                return;
            }

            self.isAddingBank(true);

            var isEdit = self.editingBankId() !== null;
            var url = isEdit
                ? '/api/company/bank-accounts/' + self.editingBankId()
                : '/api/company/bank-accounts';
            var method = isEdit ? 'PUT' : 'POST';

            $.ajax({
                url: url,
                method: method,
                contentType: 'application/json',
                data: JSON.stringify({
                    bankName: bank.bankName(),
                    accountHolder: bank.accountHolder(),
                    iban: bank.iban(),
                    isDefault: bank.isDefault()
                })
            }).done(function (savedAccount) {
                if (isEdit) {
                    // Mevcut hesabi guncelle
                    var accounts = self.bankAccounts();
                    for (var i = 0; i < accounts.length; i++) {
                        if (accounts[i].id === savedAccount.id) {
                            self.bankAccounts.splice(i, 1, savedAccount);
                            break;
                        }
                    }
                    toastr.success('Banka hesabi guncellendi');
                } else {
                    self.bankAccounts.push(savedAccount);
                    toastr.success('Banka hesabi eklendi');
                }
                bootstrap.Modal.getInstance(document.getElementById('addBankModal')).hide();
            }).fail(function (xhr) {
                var msg = xhr.responseJSON?.message || (isEdit ? 'Guncelleme hatasi' : 'Ekleme hatasi');
                toastr.error(msg);
            }).always(function () {
                self.isAddingBank(false);
            });
        };

        // Eski fonksiyon adi icin uyumluluk
        self.addBankAccount = self.saveBankAccount;

        self.deleteBankAccount = function (account) {
            if (!confirm('Bu banka hesabini silmek istediginize emin misiniz?')) return;

            $.ajax({
                url: '/api/company/bank-accounts/' + account.id,
                method: 'DELETE'
            }).done(function () {
                self.bankAccounts.remove(account);
                toastr.success('Banka hesabi silindi');
            }).fail(function (xhr) {
                var msg = xhr.responseJSON?.message || 'Silme hatasi';
                toastr.error(msg);
            });
        };

        // ============================================
        // INIT
        // ============================================

        self.loadData();
    }

    $(document).ready(function () {
        ko.applyBindings(new BillingViewModel(), document.getElementById('billing-app'));
    });

})();
