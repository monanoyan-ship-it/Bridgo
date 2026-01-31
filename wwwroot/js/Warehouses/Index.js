function WarehousesViewModel() {
    var self = this;

    // State
    self.isLoading = ko.observable(false);
    self.isSaving = ko.observable(false);
    self.isEditing = ko.observable(false);
    self.modalError = ko.observable('');

    // Data
    self.warehouses = ko.observableArray([]);
    self.warehouseTypes = ko.observableArray([]);
    self.addresses = ko.observableArray([]);
    self.teamMembers = ko.observableArray([]);

    // Filters
    self.searchQuery = ko.observable('');
    self.filterType = ko.observable('');
    self.filterStatus = ko.observable('');

    // Delete
    self.deleteWarehouseId = ko.observable(null);
    self.deleteWarehouseName = ko.observable('');

    // Form
    self.editingId = ko.observable(null);
    self.form = {
        code: ko.observable(''),
        name: ko.observable(''),
        warehouseTypeId: ko.observable(1),
        description: ko.observable(''),
        addressId: ko.observable(null),
        totalCapacity: ko.observable(null),
        capacityUnit: ko.observable(''),
        managerUserId: ko.observable(null),
        contactPhone: ko.observable(''),
        contactEmail: ko.observable(''),
        priority: ko.observable(0),
        isDefault: ko.observable(false),
        isActive: ko.observable(true)
    };

    self.errors = {
        code: ko.observable(''),
        name: ko.observable('')
    };

    // Filtered warehouses
    self.filteredWarehouses = ko.computed(function() {
        var result = self.warehouses();
        var query = self.searchQuery().toLowerCase().trim();
        var typeFilter = self.filterType();
        var statusFilter = self.filterStatus();

        if (query) {
            result = result.filter(function(w) {
                return w.code.toLowerCase().indexOf(query) > -1 ||
                       w.name.toLowerCase().indexOf(query) > -1 ||
                       (w.city && w.city.toLowerCase().indexOf(query) > -1);
            });
        }

        if (typeFilter) {
            result = result.filter(function(w) {
                return w.warehouseTypeId == typeFilter;
            });
        }

        if (statusFilter === 'active') {
            result = result.filter(function(w) { return w.isActive; });
        } else if (statusFilter === 'inactive') {
            result = result.filter(function(w) { return !w.isActive; });
        }

        return result;
    });

    // Load data
    self.loadWarehouses = function() {
        self.isLoading(true);
        fetch('/api/warehouses')
            .then(function(r) { return r.json(); })
            .then(function(data) {
                self.warehouses(data);
            })
            .catch(function(err) {
                console.error('Warehouses load error:', err);
                toastr.error(T('Common.Error.Load', 'Veriler yuklenirken hata olustu'));
            })
            .finally(function() {
                self.isLoading(false);
            });
    };

    self.loadWarehouseTypes = function() {
        fetch('/api/warehouses/types')
            .then(function(r) { return r.json(); })
            .then(function(data) {
                self.warehouseTypes(data);
            });
    };

    self.loadAddresses = function() {
        fetch('/api/company/addresses')
            .then(function(r) { return r.json(); })
            .then(function(data) {
                self.addresses(data);
            });
    };

    self.loadTeamMembers = function() {
        fetch('/api/TeamApi/members')
            .then(function(r) { return r.json(); })
            .then(function(data) {
                // Sadece aktif uyeler
                var activeMembers = data.filter(function(m) { return m.status === 'Active'; });
                self.teamMembers(activeMembers.map(function(m) {
                    return { id: m.userId, name: m.fullName };
                }));
            })
            .catch(function() {
                self.teamMembers([]);
            });
    };

    // Modal operations
    self.resetForm = function() {
        self.editingId(null);
        self.isEditing(false);
        self.modalError('');
        self.form.code('');
        self.form.name('');
        self.form.warehouseTypeId(1);
        self.form.description('');
        self.form.addressId(null);
        self.form.totalCapacity(null);
        self.form.capacityUnit('');
        self.form.managerUserId(null);
        self.form.contactPhone('');
        self.form.contactEmail('');
        self.form.priority(0);
        self.form.isDefault(false);
        self.form.isActive(true);
        self.errors.code('');
        self.errors.name('');
    };

    self.openCreateModal = function() {
        self.resetForm();
        var modal = new bootstrap.Modal(document.getElementById('warehouseModal'));
        modal.show();
    };

    self.openEditModal = function(warehouse) {
        self.resetForm();
        self.editingId(warehouse.id);
        self.isEditing(true);
        self.form.code(warehouse.code);
        self.form.name(warehouse.name);
        self.form.warehouseTypeId(warehouse.warehouseTypeId);
        self.form.description(warehouse.description || '');
        self.form.addressId(warehouse.addressId);
        self.form.totalCapacity(warehouse.totalCapacity);
        self.form.capacityUnit(warehouse.capacityUnit || '');
        self.form.managerUserId(warehouse.managerUserId);
        self.form.contactPhone(warehouse.contactPhone || '');
        self.form.contactEmail(warehouse.contactEmail || '');
        self.form.priority(warehouse.priority);
        self.form.isDefault(warehouse.isDefault);
        self.form.isActive(warehouse.isActive);

        var modal = new bootstrap.Modal(document.getElementById('warehouseModal'));
        modal.show();
    };

    // Validation
    self.validate = function() {
        var isValid = true;
        self.errors.code('');
        self.errors.name('');

        if (!self.form.code().trim()) {
            self.errors.code(T('Warehouse.Error.CodeRequired', 'Depo kodu zorunludur'));
            isValid = false;
        }

        if (!self.form.name().trim()) {
            self.errors.name(T('Warehouse.Error.NameRequired', 'Depo adi zorunludur'));
            isValid = false;
        }

        return isValid;
    };

    // Save
    self.save = function() {
        if (!self.validate()) return;

        self.modalError('');
        self.isSaving(true);

        var url = self.isEditing() ? '/api/warehouses/' + self.editingId() : '/api/warehouses';
        var method = self.isEditing() ? 'PUT' : 'POST';

        var data = {
            code: self.form.code().trim(),
            name: self.form.name().trim(),
            warehouseTypeId: parseInt(self.form.warehouseTypeId()),
            description: self.form.description().trim() || null,
            addressId: self.form.addressId() ? parseInt(self.form.addressId()) : null,
            totalCapacity: self.form.totalCapacity() ? parseFloat(self.form.totalCapacity()) : null,
            capacityUnit: self.form.capacityUnit() || null,
            managerUserId: self.form.managerUserId() ? parseInt(self.form.managerUserId()) : null,
            contactPhone: self.form.contactPhone().trim() || null,
            contactEmail: self.form.contactEmail().trim() || null,
            priority: parseInt(self.form.priority()) || 0,
            isDefault: self.form.isDefault(),
            isActive: self.form.isActive()
        };

        fetch(url, {
            method: method,
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        })
        .then(function(r) {
            if (r.ok) {
                bootstrap.Modal.getInstance(document.getElementById('warehouseModal')).hide();
                self.loadWarehouses();
                toastr.success(self.isEditing()
                    ? T('Warehouse.Success.Updated', 'Depo guncellendi')
                    : T('Warehouse.Success.Created', 'Depo olusturuldu'));
            } else {
                return r.json().then(function(err) {
                    self.modalError(err.message || T('Common.Error.Save', 'Kayit sirasinda hata olustu'));
                });
            }
        })
        .catch(function(err) {
            console.error('Save error:', err);
            self.modalError(T('Common.Error.Save', 'Kayit sirasinda hata olustu'));
        })
        .finally(function() {
            self.isSaving(false);
        });
    };

    // Delete
    self.confirmDelete = function(warehouse) {
        self.deleteWarehouseId(warehouse.id);
        self.deleteWarehouseName(warehouse.code + ' - ' + warehouse.name);
        var modal = new bootstrap.Modal(document.getElementById('deleteModal'));
        modal.show();
    };

    self.deleteWarehouse = function() {
        fetch('/api/warehouses/' + self.deleteWarehouseId(), { method: 'DELETE' })
            .then(function(r) {
                if (r.ok) {
                    bootstrap.Modal.getInstance(document.getElementById('deleteModal')).hide();
                    self.loadWarehouses();
                    toastr.success(T('Warehouse.Success.Deleted', 'Depo silindi'));
                } else {
                    return r.json().then(function(err) {
                        toastr.error(err.message || T('Common.Error.Delete', 'Silme sirasinda hata olustu'));
                    });
                }
            })
            .catch(function(err) {
                console.error('Delete error:', err);
                toastr.error(T('Common.Error.Delete', 'Silme sirasinda hata olustu'));
            });
    };

    // Set as default
    self.setAsDefault = function(warehouse) {
        fetch('/api/warehouses/' + warehouse.id + '/set-default', { method: 'POST' })
            .then(function(r) {
                if (r.ok) {
                    self.loadWarehouses();
                    toastr.success(T('Warehouse.Success.SetDefault', 'Varsayilan depo degistirildi'));
                } else {
                    return r.json().then(function(err) {
                        toastr.error(err.message);
                    });
                }
            });
    };

    // Get type icon
    self.getTypeIcon = function(typeId) {
        var type = self.warehouseTypes().find(function(t) { return t.id === typeId; });
        return type ? type.icon : 'bi-box';
    };

    // Initialize
    self.loadWarehouseTypes();
    self.loadAddresses();
    self.loadTeamMembers();
    self.loadWarehouses();
}

// Apply bindings
$(document).ready(function() {
    ko.applyBindings(new WarehousesViewModel(), document.getElementById('warehouses-app'));
});
