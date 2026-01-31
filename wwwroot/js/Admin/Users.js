// Admin Users ViewModel
function UsersViewModel() {
    var self = this;

    // Observables
    self.users = ko.observableArray([]);
    self.vendors = ko.observableArray([]);
    self.isLoading = ko.observable(false);
    self.isSaving = ko.observable(false);
    self.searchQuery = ko.observable('');
    self.roleFilter = ko.observable('');

    // Form data
    self.formData = {
        firstName: ko.observable(''),
        lastName: ko.observable(''),
        email: ko.observable(''),
        phone: ko.observable(''),
        password: ko.observable(''),
        vendorId: ko.observable(null),
        vendorRole: ko.observable('Employee'),
        isSystemAdmin: ko.observable(false),
        systemRole: ko.observable('')
    };

    // Modal
    self.userModal = null;

    // Debounced search
    self.searchQuery.subscribe(function () {
        self.loadUsers();
    });

    self.roleFilter.subscribe(function () {
        self.loadUsers();
    });

    // Load vendors for dropdown
    self.loadVendors = function () {
        $.get('/api/admin/vendors/dropdown')
            .done(function (data) {
                self.vendors(data);
            });
    };

    // Load users
    self.loadUsers = function () {
        self.isLoading(true);

        var params = new URLSearchParams();
        if (self.searchQuery()) params.append('search', self.searchQuery());
        if (self.roleFilter()) params.append('roleFilter', self.roleFilter());

        $.get('/api/admin/users?' + params.toString())
            .done(function (data) {
                self.users(data);
            })
            .fail(function () {
                toastr.error('Kullanicilar yuklenemedi');
            })
            .always(function () {
                self.isLoading(false);
            });
    };

    // Reset form
    self.resetForm = function () {
        self.formData.firstName('');
        self.formData.lastName('');
        self.formData.email('');
        self.formData.phone('');
        self.formData.password('');
        self.formData.vendorId(null);
        self.formData.vendorRole('Employee');
        self.formData.isSystemAdmin(false);
        self.formData.systemRole('');
    };

    // Show add modal
    self.showAddModal = function () {
        self.resetForm();
        self.userModal.show();
    };

    // Save user
    self.saveUser = function () {
        // Validation
        if (!self.formData.firstName() || !self.formData.lastName() || !self.formData.email() || !self.formData.password()) {
            toastr.warning('Lutfen zorunlu alanlari doldurun');
            return;
        }

        self.isSaving(true);

        var data = {
            firstName: self.formData.firstName(),
            lastName: self.formData.lastName(),
            email: self.formData.email(),
            phone: self.formData.phone() || null,
            password: self.formData.password(),
            vendorId: self.formData.vendorId() ? parseInt(self.formData.vendorId()) : null,
            vendorRole: self.formData.vendorId() ? self.formData.vendorRole() : null,
            isSystemAdmin: !self.formData.vendorId() && self.formData.isSystemAdmin(),
            systemRole: self.formData.isSystemAdmin() ? self.formData.systemRole() : null
        };

        $.ajax({
            url: '/api/admin/users',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data)
        })
        .done(function (response) {
            toastr.success(response.message);
            self.userModal.hide();
            self.loadUsers();
        })
        .fail(function (xhr) {
            var msg = xhr.responseJSON?.message || 'Kullanici olusturulamadi';
            toastr.error(msg);
        })
        .always(function () {
            self.isSaving(false);
        });
    };

    // View user details
    self.viewUser = function (user) {
        $.get('/api/admin/users/' + user.id)
            .done(function (data) {
                var content = '<div class="row">';
                content += '<div class="col-md-6"><strong>Ad Soyad:</strong> ' + data.fullName + '</div>';
                content += '<div class="col-md-6"><strong>E-posta:</strong> ' + data.email + '</div>';
                content += '<div class="col-md-6 mt-2"><strong>Telefon:</strong> ' + (data.phone || '-') + '</div>';
                content += '<div class="col-md-6 mt-2"><strong>Firma:</strong> ' + (data.companyName || 'Yok') + '</div>';
                content += '<div class="col-md-6 mt-2"><strong>Firma Rolu:</strong> ' + (data.vendorRole || '-') + '</div>';
                content += '<div class="col-md-6 mt-2"><strong>Sistem Rolleri:</strong> ' + (data.systemRoles.join(', ') || '-') + '</div>';
                content += '<div class="col-md-6 mt-2"><strong>Durum:</strong> ' + (data.isActive ? '<span class="badge bg-success">Aktif</span>' : '<span class="badge bg-secondary">Pasif</span>') + '</div>';
                content += '<div class="col-md-6 mt-2"><strong>E-posta Dogrulandi:</strong> ' + (data.emailConfirmed ? 'Evet' : 'Hayir') + '</div>';
                content += '<div class="col-md-6 mt-2"><strong>Son Giris:</strong> ' + (data.lastLoginAt || '-') + '</div>';
                content += '<div class="col-md-6 mt-2"><strong>Kayit Tarihi:</strong> ' + data.createdAt + '</div>';
                content += '</div>';

                showInfoModal({
                    title: 'Kullanici Detayi',
                    content: content
                });
            })
            .fail(function () {
                toastr.error('Kullanici detayi yuklenemedi');
            });
    };

    // Toggle user status
    self.toggleUserStatus = function (user) {
        var action = user.isActive ? 'pasif' : 'aktif';
        showConfirmModal({
            title: 'Kullanici Durumu Degistir',
            message: '"' + user.fullName + '" kullanicisini ' + action + ' yapmak istiyor musunuz?',
            confirmText: action.charAt(0).toUpperCase() + action.slice(1) + ' Yap',
            type: user.isActive ? 'warning' : 'success',
            onConfirm: function () {
                $.post('/api/admin/users/' + user.id + '/toggle-status')
                    .done(function (response) {
                        toastr.success(response.message);
                        self.loadUsers();
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
        self.userModal = new bootstrap.Modal(document.getElementById('userModal'));
        self.loadVendors();
        self.loadUsers();
    };

    self.init();
}

// Apply bindings
$(function () {
    ko.applyBindings(new UsersViewModel(), document.getElementById('users-app'));
});
