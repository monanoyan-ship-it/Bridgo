// Admin Categories Management
(function () {
    'use strict';

    function CategoriesViewModel() {
        var self = this;

        // State
        self.categories = ko.observableArray([]);
        self.flatCategories = ko.observableArray([]);
        self.deletedCategories = ko.observableArray([]);
        self.pendingCategories = ko.observableArray([]);
        self.isLoading = ko.observable(false);
        self.isLoadingTrash = ko.observable(false);
        self.isLoadingPending = ko.observable(false);
        self.isSaving = ko.observable(false);
        self.isEditing = ko.observable(false);
        self.editingId = ko.observable(null);

        // Deletion review state
        self.selectedDeletion = ko.observable(null);
        self.reviewNote = ko.observable('');

        // Search
        self.searchQuery = ko.observable('');

        // Tree version (for forcing re-render on expand/collapse)
        self.treeVersion = ko.observable(0);

        // Form data
        self.formData = {
            name: ko.observable(''),
            displayName: ko.observable(''),
            nameResourceKey: ko.observable(''),
            description: ko.observable(''),
            descriptionResourceKey: ko.observable(''),
            icon: ko.observable(''),
            imageUrl: ko.observable(''),
            parentId: ko.observable(null),
            displayOrder: ko.observable(0),
            isActive: ko.observable(true),
            slug: ko.observable(''),
            metaTitle: ko.observable(''),
            metaDescription: ko.observable('')
        };

        // Modal references
        self.modal = null;
        self.productMigrationModal = null;
        self.deletionDetailsModal = null;
        self.approveModal = null;
        self.rejectModal = null;

        // Product migration state
        self.deletingCategory = ko.observable(null);
        self.productMigrationCount = ko.observable(0);
        self.targetCategoryId = ko.observable(null);
        self.availableTargetCategories = ko.observableArray([]);

        // Count all categories recursively
        self.countCategories = function (cats) {
            var count = 0;
            cats.forEach(function (cat) {
                count++;
                if (cat.children && cat.children.length > 0) {
                    count += self.countCategories(cat.children);
                }
            });
            return count;
        };

        // Total count
        self.totalCount = ko.computed(function () {
            return self.countCategories(self.categories());
        });

        // Filtered categories (for search)
        // Note: We must return a new array reference for Knockout to detect changes
        self.filteredCategories = ko.computed(function () {
            var cats = self.categories();
            var query = self.searchQuery().toLowerCase().trim();
            if (!query) {
                // Return a shallow copy to ensure Knockout detects changes
                return cats.slice();
            }

            // Filter and mark matching categories
            return self.filterCategories(cats, query);
        });

        // Filter categories recursively
        self.filterCategories = function (cats, query) {
            var result = [];

            cats.forEach(function (cat) {
                var nameMatch = cat.name.toLowerCase().indexOf(query) !== -1;
                var childMatches = [];

                if (cat.children && cat.children.length > 0) {
                    childMatches = self.filterCategories(cat.children, query);
                }

                if (nameMatch || childMatches.length > 0) {
                    // Use original category directly, just update UI properties
                    cat._isMatch = nameMatch;
                    result.push(cat);
                }
            });

            return result;
        };

        // Initialize
        self.init = function () {
            self.modal = new bootstrap.Modal(document.getElementById('categoryModal'));
            self.productMigrationModal = new bootstrap.Modal(document.getElementById('productMigrationModal'));
            self.deletionDetailsModal = new bootstrap.Modal(document.getElementById('deletionDetailsModal'));
            self.approveModal = new bootstrap.Modal(document.getElementById('approveModal'));
            self.rejectModal = new bootstrap.Modal(document.getElementById('rejectModal'));
            self.loadCategories();
        };

        // Load categories
        self.loadCategories = function () {
            self.isLoading(true);
            fetch('/api/admin/categories')
                .then(function (response) { return response.json(); })
                .then(function (data) {
                    // Add _expanded property to all categories (collapsed by default for root)
                    self.addExpandedProperty(data, 0);
                    self.categories(data);
                    self.buildFlatCategories(data);
                })
                .catch(function (error) {
                    console.error('Error loading categories:', error);
                    toastr.error('Kategoriler yuklenemedi');
                })
                .finally(function () {
                    self.isLoading(false);
                });
        };

        // Add _expanded property recursively
        self.addExpandedProperty = function (categories, level) {
            categories.forEach(function (cat) {
                cat._expanded = false; // All collapsed by default
                cat._isMatch = false;
                if (cat.children && cat.children.length > 0) {
                    self.addExpandedProperty(cat.children, level + 1);
                }
            });
        };

        // Check if category is expanded (references treeVersion for reactivity)
        self.isExpanded = function (category) {
            self.treeVersion(); // Subscribe to changes
            return category._expanded;
        };

        // Count all descendants (children, grandchildren, etc.)
        self.countDescendants = function (category) {
            if (!category.children || category.children.length === 0) {
                return 0;
            }
            var count = category.children.length;
            category.children.forEach(function (child) {
                count += self.countDescendants(child);
            });
            return count;
        };

        // Toggle expand/collapse - find original by ID
        self.toggleExpand = function (category) {
            self.toggleExpandById(self.categories(), category.id);
            self.treeVersion(self.treeVersion() + 1); // Increment to trigger re-render
            self.categories.valueHasMutated();
        };

        // Find and toggle category by ID recursively
        self.toggleExpandById = function (cats, id) {
            for (var i = 0; i < cats.length; i++) {
                if (cats[i].id === id) {
                    cats[i]._expanded = !cats[i]._expanded;
                    return true;
                }
                if (cats[i].children && cats[i].children.length > 0) {
                    if (self.toggleExpandById(cats[i].children, id)) {
                        return true;
                    }
                }
            }
            return false;
        };

        // Expand all
        self.expandAll = function () {
            self.setExpandedAll(self.categories(), true);
            self.treeVersion(self.treeVersion() + 1);
            self.categories.valueHasMutated();
        };

        // Collapse all
        self.collapseAll = function () {
            self.setExpandedAll(self.categories(), false);
            self.treeVersion(self.treeVersion() + 1);
            self.categories.valueHasMutated();
        };

        // Set expanded recursively
        self.setExpandedAll = function (cats, expanded) {
            cats.forEach(function (cat) {
                cat._expanded = expanded;
                if (cat.children && cat.children.length > 0) {
                    self.setExpandedAll(cat.children, expanded);
                }
            });
        };

        // Clear search
        self.clearSearch = function () {
            self.searchQuery('');
            // Reset _isMatch on all categories
            self.resetIsMatch(self.categories());
            self.categories.valueHasMutated();
        };

        // Reset _isMatch recursively
        self.resetIsMatch = function (cats) {
            cats.forEach(function (cat) {
                cat._isMatch = false;
                if (cat.children && cat.children.length > 0) {
                    self.resetIsMatch(cat.children);
                }
            });
        };

        // Build flat list for dropdown
        self.buildFlatCategories = function (categories, result, level) {
            result = result || [];
            level = level || 0;

            categories.forEach(function (cat) {
                result.push({
                    id: cat.id,
                    name: cat.name,
                    displayName: cat.displayName,
                    level: level
                });
                if (cat.children && cat.children.length > 0) {
                    self.buildFlatCategories(cat.children, result, level + 1);
                }
            });

            if (level === 0) {
                self.flatCategories(result);
            }
            return result;
        };

        // Reset form
        self.resetForm = function () {
            self.formData.name('');
            self.formData.displayName('');
            self.formData.nameResourceKey('');
            self.formData.description('');
            self.formData.descriptionResourceKey('');
            self.formData.icon('');
            self.formData.imageUrl('');
            self.formData.parentId(null);
            self.formData.displayOrder(0);
            self.formData.isActive(true);
            self.formData.slug('');
            self.formData.metaTitle('');
            self.formData.metaDescription('');
            self.isEditing(false);
            self.editingId(null);
        };

        // Show add modal
        self.showAddModal = function () {
            self.resetForm();
            self.modal.show();
        };

        // Show add child modal
        self.showAddChildModal = function (parentCategory) {
            self.resetForm();
            self.formData.parentId(parentCategory.id);
            self.modal.show();
        };

        // Show edit modal
        self.showEditModal = function (category) {
            self.resetForm();
            self.isEditing(true);
            self.editingId(category.id);

            self.formData.name(category.name);
            self.formData.displayName(category.displayName || '');
            self.formData.nameResourceKey(category.nameResourceKey || '');
            self.formData.description(category.description || '');
            self.formData.descriptionResourceKey(category.descriptionResourceKey || '');
            self.formData.icon(category.icon || '');
            self.formData.imageUrl(category.imageUrl || '');
            self.formData.parentId(category.parentId);
            self.formData.displayOrder(category.displayOrder);
            self.formData.isActive(category.isActive);
            self.formData.slug(category.slug || '');
            self.formData.metaTitle(category.metaTitle || '');
            self.formData.metaDescription(category.metaDescription || '');

            self.modal.show();
        };

        // Save category
        self.saveCategory = function () {
            if (!self.formData.name()) {
                toastr.warning('Kategori adi zorunludur');
                return;
            }

            self.isSaving(true);

            var data = {
                name: self.formData.name(),
                displayName: self.formData.displayName() || null,
                nameResourceKey: self.formData.nameResourceKey() || null,
                description: self.formData.description() || null,
                descriptionResourceKey: self.formData.descriptionResourceKey() || null,
                icon: self.formData.icon() || null,
                imageUrl: self.formData.imageUrl() || null,
                parentId: self.formData.parentId() || null,
                displayOrder: parseInt(self.formData.displayOrder()) || 0,
                isActive: self.formData.isActive(),
                slug: self.formData.slug() || null,
                metaTitle: self.formData.metaTitle() || null,
                metaDescription: self.formData.metaDescription() || null
            };

            var url = self.isEditing()
                ? '/api/admin/categories/' + self.editingId()
                : '/api/admin/categories';

            var method = self.isEditing() ? 'PUT' : 'POST';

            fetch(url, {
                method: method,
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(data)
            })
                .then(function (response) { return response.json().then(function (d) { return { ok: response.ok, data: d }; }); })
                .then(function (result) {
                    if (result.ok) {
                        toastr.success(result.data.message || 'Islem basarili');
                        self.modal.hide();
                        self.loadCategories();
                    } else {
                        toastr.error(result.data.message || 'Bir hata olustu');
                    }
                })
                .catch(function (error) {
                    console.error('Error saving category:', error);
                    toastr.error('Kategori kaydedilemedi');
                })
                .finally(function () {
                    self.isSaving(false);
                });
        };

        // Delete category
        self.deleteCategory = function (category) {
            if (category.children && category.children.length > 0) {
                toastr.warning('Alt kategorileri olan kategori silinemez');
                return;
            }

            // Urun varsa direkt migration modal'i goster
            if (category.productCount > 0) {
                self.showProductMigrationModal(category, category.productCount);
                return;
            }

            if (typeof showConfirmModal === 'function') {
                showConfirmModal({
                    title: 'Kategori Sil',
                    message: '"' + category.name + '" kategorisini silmek istediginize emin misiniz?',
                    type: 'danger',
                    confirmText: 'Sil',
                    confirmIcon: 'bi bi-trash',
                    onConfirm: function () { self.doDeleteCategory(category.id); }
                });
            } else {
                toastr.error('Silme islemi yapilamiyor');
            }
        };

        // Show product migration modal
        self.showProductMigrationModal = function (category, productCount) {
            self.deletingCategory(category);
            self.productMigrationCount(productCount);
            self.targetCategoryId(null);

            // Build available target categories (exclude the category being deleted and its children)
            var excludeIds = self.getAllDescendantIds(category.id);
            excludeIds.push(category.id);

            var available = self.flatCategories().filter(function (cat) {
                return excludeIds.indexOf(cat.id) === -1;
            });
            self.availableTargetCategories(available);

            self.productMigrationModal.show();
        };

        // Get all descendant IDs recursively
        self.getAllDescendantIds = function (categoryId) {
            var ids = [];
            var category = self.findCategoryById(self.categories(), categoryId);
            if (category && category.children) {
                category.children.forEach(function (child) {
                    ids.push(child.id);
                    ids = ids.concat(self.getAllDescendantIds(child.id));
                });
            }
            return ids;
        };

        // Find category by ID
        self.findCategoryById = function (cats, id) {
            for (var i = 0; i < cats.length; i++) {
                if (cats[i].id === id) {
                    return cats[i];
                }
                if (cats[i].children && cats[i].children.length > 0) {
                    var found = self.findCategoryById(cats[i].children, id);
                    if (found) return found;
                }
            }
            return null;
        };

        // Confirm delete with product migration
        self.confirmDeleteWithMigration = function () {
            var categoryId = self.deletingCategory().id;
            var targetId = self.targetCategoryId();

            if (!targetId) {
                toastr.warning('Lutfen hedef kategori secin');
                return;
            }

            self.productMigrationModal.hide();
            self.doDeleteCategory(categoryId, targetId);
        };

        self.doDeleteCategory = function (id, targetCategoryId) {
            var url = '/api/admin/categories/' + id;
            if (targetCategoryId) {
                url += '?targetCategoryId=' + targetCategoryId;
            }

            fetch(url, { method: 'DELETE' })
                .then(function (response) { return response.json().then(function (d) { return { ok: response.ok, data: d }; }); })
                .then(function (result) {
                    if (result.ok) {
                        toastr.success(result.data.message || 'Kategori silindi');
                        self.loadCategories();
                    } else {
                        // Eger urun migration gerekiyorsa modal goster
                        if (result.data.requiresProductMigration) {
                            var category = self.findCategoryById(self.categories(), id);
                            if (category) {
                                self.showProductMigrationModal(category, result.data.productCount);
                            }
                        } else {
                            toastr.error(result.data.message || 'Kategori silinemedi');
                        }
                    }
                })
                .catch(function (error) {
                    console.error('Error deleting category:', error);
                    toastr.error('Kategori silinemedi');
                });
        };

        // Seed categories
        self.seedCategories = function () {
            if (typeof showConfirmModal === 'function') {
                showConfirmModal({
                    title: 'Ornek Kategoriler',
                    message: 'Ornek kategorileri yuklemek istediginize emin misiniz? Bu islem sadece bos veritabaninda calisir.',
                    type: 'success',
                    confirmText: 'Yukle',
                    confirmIcon: 'bi bi-database-add',
                    onConfirm: function () { self.doSeedCategories(); }
                });
            } else {
                toastr.error('Modal yuklenemedi');
            }
        };

        self.doSeedCategories = function () {
            self.isLoading(true);
            fetch('/api/admin/categories/seed', { method: 'POST' })
                .then(function (response) { return response.json().then(function (d) { return { ok: response.ok, data: d }; }); })
                .then(function (result) {
                    if (result.ok) {
                        toastr.success(result.data.message || 'Kategoriler yuklendi');
                        self.loadCategories();
                    } else {
                        toastr.error(result.data.message || 'Kategoriler yuklenemedi');
                    }
                })
                .catch(function (error) {
                    console.error('Error seeding categories:', error);
                    toastr.error('Kategoriler yuklenemedi');
                })
                .finally(function () {
                    self.isLoading(false);
                });
        };

        // === TRASH BIN ===

        // Load deleted categories
        self.loadDeletedCategories = function () {
            self.isLoadingTrash(true);
            fetch('/api/admin/categories/deleted')
                .then(function (response) {
                    if (!response.ok) {
                        throw new Error('HTTP ' + response.status + ': ' + response.statusText);
                    }
                    return response.json();
                })
                .then(function (data) {
                    console.log('Deleted categories loaded:', data);
                    self.deletedCategories(data);
                })
                .catch(function (error) {
                    console.error('Error loading deleted categories:', error);
                    toastr.error('Silinen kategoriler yuklenemedi: ' + error.message);
                })
                .finally(function () {
                    self.isLoadingTrash(false);
                });
        };

        // Restore category
        self.restoreCategory = function (category) {
            if (typeof showConfirmModal === 'function') {
                showConfirmModal({
                    title: 'Kategori Geri Yukle',
                    message: '"' + category.name + '" kategorisini geri yuklemek istediginize emin misiniz?',
                    type: 'success',
                    confirmText: 'Geri Yukle',
                    confirmIcon: 'bi bi-arrow-counterclockwise',
                    onConfirm: function () { self.doRestoreCategory(category.id); }
                });
            }
        };

        self.doRestoreCategory = function (id) {
            fetch('/api/admin/categories/' + id + '/restore', { method: 'POST' })
                .then(function (response) { return response.json().then(function (d) { return { ok: response.ok, data: d }; }); })
                .then(function (result) {
                    if (result.ok) {
                        toastr.success(result.data.message || 'Kategori geri yuklendi');
                        self.loadDeletedCategories();
                        self.loadCategories();
                    } else {
                        toastr.error(result.data.message || 'Kategori geri yuklenemedi');
                    }
                })
                .catch(function (error) {
                    console.error('Error restoring category:', error);
                    toastr.error('Kategori geri yuklenemedi');
                });
        };

        // Permanent delete category
        self.permanentDeleteCategory = function (category) {
            if (typeof showConfirmModal === 'function') {
                showConfirmModal({
                    title: 'Kalici Silme',
                    message: '"' + category.name + '" kategorisini kalici olarak silmek istediginize emin misiniz? Bu islem geri alinamaz!',
                    type: 'danger',
                    confirmText: 'Kalici Sil',
                    confirmIcon: 'bi bi-x-circle',
                    onConfirm: function () { self.doPermanentDeleteCategory(category.id); }
                });
            }
        };

        self.doPermanentDeleteCategory = function (id) {
            fetch('/api/admin/categories/' + id + '/permanent', { method: 'DELETE' })
                .then(function (response) { return response.json().then(function (d) { return { ok: response.ok, data: d }; }); })
                .then(function (result) {
                    if (result.ok) {
                        toastr.success(result.data.message || 'Kategori kalici olarak silindi');
                        self.loadDeletedCategories();
                    } else {
                        toastr.error(result.data.message || 'Kategori silinemedi');
                    }
                })
                .catch(function (error) {
                    console.error('Error permanently deleting category:', error);
                    toastr.error('Kategori silinemedi');
                });
        };

        // Empty trash
        self.emptyTrash = function () {
            if (typeof showConfirmModal === 'function') {
                showConfirmModal({
                    title: 'Cop Kutusunu Bosalt',
                    message: 'Cop kutusundaki tum kategorileri kalici olarak silmek istediginize emin misiniz? Bu islem geri alinamaz!',
                    type: 'danger',
                    confirmText: 'Tumunu Sil',
                    confirmIcon: 'bi bi-trash3',
                    onConfirm: function () { self.doEmptyTrash(); }
                });
            }
        };

        self.doEmptyTrash = function () {
            var deletedIds = self.deletedCategories().map(function (c) { return c.id; });
            var promises = deletedIds.map(function (id) {
                return fetch('/api/admin/categories/' + id + '/permanent', { method: 'DELETE' });
            });

            Promise.all(promises)
                .then(function () {
                    toastr.success('Cop kutusu bosaltildi');
                    self.loadDeletedCategories();
                })
                .catch(function (error) {
                    console.error('Error emptying trash:', error);
                    toastr.error('Cop kutusu bosaltilamadi');
                    self.loadDeletedCategories();
                });
        };

        // === PENDING APPROVAL ===

        // Load pending categories
        self.loadPendingCategories = function () {
            self.isLoadingPending(true);
            fetch('/api/admin/categories/pending')
                .then(function (response) {
                    if (!response.ok) {
                        throw new Error('HTTP ' + response.status);
                    }
                    return response.json();
                })
                .then(function (data) {
                    self.pendingCategories(data);
                })
                .catch(function (error) {
                    console.error('Error loading pending categories:', error);
                    toastr.error('Onay bekleyen kategoriler yuklenemedi');
                })
                .finally(function () {
                    self.isLoadingPending(false);
                });
        };

        // Show deletion details modal
        self.showDeletionDetails = function (category) {
            self.selectedDeletion(category);
            self.deletionDetailsModal.show();
        };

        // Show approve modal
        self.showApproveModal = function (category) {
            self.selectedDeletion(category);
            self.reviewNote('');
            self.approveModal.show();
        };

        // Show reject modal
        self.showRejectModal = function (category) {
            self.selectedDeletion(category);
            self.reviewNote('');
            self.rejectModal.show();
        };

        // Confirm approve
        self.confirmApprove = function () {
            var categoryId = self.selectedDeletion().id;

            fetch('/api/admin/categories/' + categoryId + '/approve', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ reviewNote: self.reviewNote() })
            })
                .then(function (response) {
                    return response.json().then(function (d) { return { ok: response.ok, data: d }; });
                })
                .then(function (result) {
                    if (result.ok) {
                        toastr.success(result.data.message || 'Silme islemi onaylandi');
                        self.approveModal.hide();
                        self.loadPendingCategories();
                        self.loadDeletedCategories();
                    } else {
                        toastr.error(result.data.message || 'Onay basarisiz');
                    }
                })
                .catch(function (error) {
                    console.error('Error approving deletion:', error);
                    toastr.error('Onay islemi basarisiz');
                });
        };

        // Confirm reject
        self.confirmReject = function () {
            var categoryId = self.selectedDeletion().id;

            fetch('/api/admin/categories/' + categoryId + '/reject', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ reviewNote: self.reviewNote() })
            })
                .then(function (response) {
                    return response.json().then(function (d) { return { ok: response.ok, data: d }; });
                })
                .then(function (result) {
                    if (result.ok) {
                        toastr.success(result.data.message || 'Silme islemi reddedildi');
                        self.rejectModal.hide();
                        self.loadPendingCategories();
                        self.loadCategories();
                    } else {
                        toastr.error(result.data.message || 'Red islemi basarisiz');
                    }
                })
                .catch(function (error) {
                    console.error('Error rejecting deletion:', error);
                    toastr.error('Red islemi basarisiz');
                });
        };

        // Initialize
        self.init();
    }

    // Start
    document.addEventListener('DOMContentLoaded', function () {
        ko.applyBindings(new CategoriesViewModel(), document.getElementById('categories-app'));
    });
})();
