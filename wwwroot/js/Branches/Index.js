function BranchesViewModel() {
    var self = this;

    // State
    self.isLoading = ko.observable(false);
    self.isSaving = ko.observable(false);
    self.errorMessage = ko.observable('');

    // Modal state
    self.isModalOpen = ko.observable(false);
    self.editingItem = ko.observable(null);

    // Data
    self.items = ko.observableArray([]);

    // CRUD operations
    self.loadItems = function () {
        self.isLoading(true);
        self.errorMessage('');

        fetch('/api/branches')
            .then(function (response) {
                if (!response.ok) {
                    throw new Error('Veriler yuklenemedi');
                }
                return response.json();
            })
            .then(function (data) {
                self.items(data);
            })
            .catch(function (error) {
                self.errorMessage(error.message || 'Bir hata olustu');
                toastr.error('Veriler yuklenirken hata olustu');
            })
            .finally(function () {
                self.isLoading(false);
            });
    };

    self.createNew = function () {
        self.editingItem({
            id: ko.observable(0),  // int ID - yeni kayit icin 0
            code: ko.observable(''),
            name: ko.observable(''),
            address: ko.observable(''),
            phone: ko.observable(''),
            email: ko.observable(''),
            city: ko.observable(''),
            district: ko.observable(''),
            isActive: ko.observable(true),
            sortOrder: ko.observable(0)
        });
        self.isModalOpen(true);
    };

    self.editItem = function (item) {
        self.editingItem({
            id: ko.observable(item.id),
            code: ko.observable(item.code),
            name: ko.observable(item.name),
            address: ko.observable(item.address || ''),
            phone: ko.observable(item.phone || ''),
            email: ko.observable(item.email || ''),
            city: ko.observable(item.city || ''),
            district: ko.observable(item.district || ''),
            isActive: ko.observable(item.isActive),
            sortOrder: ko.observable(item.sortOrder)
        });
        self.isModalOpen(true);
    };

    self.closeModal = function () {
        self.isModalOpen(false);
        self.editingItem(null);
    };

    self.save = function () {
        var item = self.editingItem();

        // Validation
        if (!item.code() || !item.code().trim()) {
            toastr.warning('Sube kodu zorunludur');
            return;
        }
        if (!item.name() || !item.name().trim()) {
            toastr.warning('Sube adi zorunludur');
            return;
        }

        var isNew = item.id() === 0;  // int ID kontrolu
        var url = isNew ? '/api/branches' : '/api/branches/' + item.id();
        var method = isNew ? 'POST' : 'PUT';

        var payload = {
            code: item.code().trim(),
            name: item.name().trim(),
            address: item.address() ? item.address().trim() : null,
            phone: item.phone() ? item.phone().trim() : null,
            email: item.email() ? item.email().trim() : null,
            city: item.city() ? item.city().trim() : null,
            district: item.district() ? item.district().trim() : null,
            isActive: item.isActive(),
            sortOrder: parseInt(item.sortOrder()) || 0
        };

        self.isSaving(true);

        fetch(url, {
            method: method,
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        })
            .then(function (response) {
                if (response.ok) {
                    self.closeModal();
                    self.loadItems();
                    toastr.success(isNew ? 'Sube eklendi' : 'Sube guncellendi');
                } else {
                    return response.json().then(function (err) {
                        throw new Error(err.message || 'Bir hata olustu');
                    });
                }
            })
            .catch(function (error) {
                toastr.error(error.message || 'Bir hata olustu');
            })
            .finally(function () {
                self.isSaving(false);
            });
    };

    self.deleteItem = function (item) {
        showDeleteConfirm(item.name, function () {
            fetch('/api/branches/' + item.id, { method: 'DELETE' })
                .then(function (response) {
                    if (response.ok) {
                        self.loadItems();
                        toastr.success('Sube silindi');
                    } else {
                        return response.json().then(function (err) {
                            throw new Error(err.message || 'Silme islemi basarisiz');
                        });
                    }
                })
                .catch(function (error) {
                    toastr.error(error.message || 'Bir hata olustu');
                });
        });
    };

    // Initialize
    self.loadItems();
}

// DOGRU BINDING - Spesifik element'e
$(document).ready(function () {
    ko.applyBindings(new BranchesViewModel(), document.getElementById('branches-app'));
});
