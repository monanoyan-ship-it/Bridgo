// Admin Vendors ViewModel
function VendorsViewModel() {
    var self = this;

    // Observables
    self.vendors = ko.observableArray([]);
    self.isLoading = ko.observable(false);
    self.searchQuery = ko.observable('');
    self.statusFilter = ko.observable('');

    // Debounced search
    self.searchQuery.subscribe(function () {
        self.loadVendors();
    });

    self.statusFilter.subscribe(function () {
        self.loadVendors();
    });

    // Load vendors
    self.loadVendors = function () {
        self.isLoading(true);

        var params = new URLSearchParams();
        if (self.searchQuery()) params.append('search', self.searchQuery());
        if (self.statusFilter()) params.append('status', self.statusFilter());

        $.get('/api/admin/vendors?' + params.toString())
            .done(function (data) {
                self.vendors(data);
            })
            .fail(function () {
                toastr.error('Firmalar yuklenemedi');
            })
            .always(function () {
                self.isLoading(false);
            });
    };

    // View vendor details
    self.viewVendor = function (vendor) {
        $.get('/api/admin/vendors/' + vendor.id)
            .done(function (data) {
                var content = '<div class="row">';
                content += '<div class="col-md-6"><strong>Firma Adi:</strong> ' + data.companyName + '</div>';
                content += '<div class="col-md-6"><strong>E-posta:</strong> ' + data.email + '</div>';
                content += '<div class="col-md-6 mt-2"><strong>Telefon:</strong> ' + (data.phone || '-') + '</div>';
                content += '<div class="col-md-6 mt-2"><strong>Website:</strong> ' + (data.website || '-') + '</div>';
                content += '<div class="col-md-6 mt-2"><strong>Vergi No:</strong> ' + (data.taxNumber || '-') + '</div>';
                content += '<div class="col-md-6 mt-2"><strong>Vergi Dairesi:</strong> ' + (data.taxOffice || '-') + '</div>';
                content += '<div class="col-md-6 mt-2"><strong>Yetenekler:</strong> ' + (data.capabilities.join(', ') || '-') + '</div>';
                content += '<div class="col-md-6 mt-2"><strong>Kullanici Sayisi:</strong> ' + data.userCount + '</div>';
                content += '<div class="col-md-6 mt-2"><strong>Durum:</strong> <span class="badge ' + data.statusClass + '">' + data.statusText + '</span></div>';
                content += '<div class="col-md-6 mt-2"><strong>Dogrulanmis:</strong> ' + (data.isVerified ? 'Evet' : 'Hayir') + '</div>';
                content += '<div class="col-md-6 mt-2"><strong>Profil Tamamlandi:</strong> ' + (data.isProfileComplete ? 'Evet' : 'Hayir') + '</div>';
                content += '<div class="col-md-6 mt-2"><strong>Kayit Tarihi:</strong> ' + data.createdAt + '</div>';
                content += '</div>';

                if (data.users && data.users.length > 0) {
                    content += '<hr><h6>Kullanicilar</h6>';
                    content += '<table class="table table-sm"><thead><tr><th>Ad Soyad</th><th>E-posta</th><th>Rol</th><th>Durum</th></tr></thead><tbody>';
                    data.users.forEach(function (u) {
                        content += '<tr>';
                        content += '<td>' + u.fullName + '</td>';
                        content += '<td>' + u.email + '</td>';
                        content += '<td>' + (u.vendorRole || '-') + '</td>';
                        content += '<td>' + (u.isActive ? '<span class="badge bg-success">Aktif</span>' : '<span class="badge bg-secondary">Pasif</span>') + '</td>';
                        content += '</tr>';
                    });
                    content += '</tbody></table>';
                }

                showInfoModal({
                    title: 'Firma Detayi',
                    content: content,
                    size: 'lg'
                });
            })
            .fail(function () {
                toastr.error('Firma detayi yuklenemedi');
            });
    };

    // Approve vendor
    self.approveVendor = function (vendor) {
        showConfirmModal({
            title: 'Firma Onayla',
            message: '"' + vendor.companyName + '" firmasini onaylamak istiyor musunuz?',
            confirmText: 'Onayla',
            type: 'success',
            onConfirm: function () {
                $.post('/api/admin/vendors/' + vendor.id + '/approve')
                    .done(function (response) {
                        toastr.success(response.message);
                        self.loadVendors();
                    })
                    .fail(function (xhr) {
                        var msg = xhr.responseJSON?.message || 'Onaylama basarisiz';
                        toastr.error(msg);
                    });
            }
        });
    };

    // Suspend vendor
    self.suspendVendor = function (vendor) {
        showConfirmModal({
            title: 'Firma Askiya Al',
            message: '"' + vendor.companyName + '" firmasini askiya almak istiyor musunuz?',
            confirmText: 'Askiya Al',
            type: 'warning',
            onConfirm: function () {
                $.post('/api/admin/vendors/' + vendor.id + '/suspend')
                    .done(function (response) {
                        toastr.success(response.message);
                        self.loadVendors();
                    })
                    .fail(function (xhr) {
                        var msg = xhr.responseJSON?.message || 'Islem basarisiz';
                        toastr.error(msg);
                    });
            }
        });
    };

    // Reactivate vendor
    self.reactivateVendor = function (vendor) {
        showConfirmModal({
            title: 'Firma Aktif Et',
            message: '"' + vendor.companyName + '" firmasini yeniden aktif etmek istiyor musunuz?',
            confirmText: 'Aktif Et',
            type: 'success',
            onConfirm: function () {
                $.post('/api/admin/vendors/' + vendor.id + '/reactivate')
                    .done(function (response) {
                        toastr.success(response.message);
                        self.loadVendors();
                    })
                    .fail(function (xhr) {
                        var msg = xhr.responseJSON?.message || 'Islem basarisiz';
                        toastr.error(msg);
                    });
            }
        });
    };

    // Initialize
    self.init = function () {
        self.loadVendors();
    };

    self.init();
}

// Apply bindings
$(function () {
    ko.applyBindings(new VendorsViewModel(), document.getElementById('vendors-app'));
});
