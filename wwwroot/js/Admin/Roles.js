// Admin Roles ViewModel
function RolesViewModel() {
    var self = this;

    // Observables
    self.capabilities = ko.observableArray([]);
    self.roles = ko.observableArray([]);
    self.selectedCapabilityId = ko.observable(null);
    self.selectedCapabilityName = ko.observable(null);
    self.isLoading = ko.observable(false);
    self.isSaving = ko.observable(false);
    self.isEditing = ko.observable(false);
    self.editingId = ko.observable(null);

    // Permissions
    self.selectedRole = ko.observable(null);
    self.modulePermissions = ko.observableArray([]); // Flat list (tum moduller)
    self.capabilityGroups = ko.observableArray([]); // Capability bazli gruplar
    self.isSavingPermissions = ko.observable(false);

    // Permission tree (hiyerarsik yapi) - capability bazli
    self.permissionTree = ko.computed(function() {
        return self.capabilityGroups();
    });

    // Tree olustur (recursive)
    function buildPermissionTree(flatList, parentId) {
        var children = flatList.filter(function(m) {
            // null ve undefined icin guvenli karsilastirma
            var mParent = m.parentId == null ? null : m.parentId;
            var target = parentId == null ? null : parentId;
            return mParent === target;
        });
        children.forEach(function(child) {
            child.children = buildPermissionTree(flatList, child.moduleId);
        });
        return children;
    }

    // Flat list'e geri cevir (kayit icin)
    function flattenPermissionTree(tree, result) {
        result = result || [];
        tree.forEach(function(node) {
            result.push(node);
            if (node.children && node.children.length > 0) {
                flattenPermissionTree(node.children, result);
            }
        });
        return result;
    }

    // Secili modul sayisi
    self.selectedModuleCount = ko.computed(function() {
        return self.modulePermissions().filter(function(m) {
            return m.canView();
        }).length;
    });

    // Toplam modul sayisi
    self.totalModuleCount = ko.computed(function() {
        return self.modulePermissions().length;
    });

    // Tum gorulenler secili mi?
    self.allViewSelected = ko.computed(function() {
        var perms = self.modulePermissions();
        if (!perms.length) return false;
        return perms.every(function(m) { return m.canView(); });
    });

    // Tum eklenenler secili mi?
    self.allCreateSelected = ko.computed(function() {
        var perms = self.modulePermissions();
        if (!perms.length) return false;
        return perms.every(function(m) { return m.canCreate(); });
    });

    // Tum duzenlenenler secili mi?
    self.allEditSelected = ko.computed(function() {
        var perms = self.modulePermissions();
        if (!perms.length) return false;
        return perms.every(function(m) { return m.canEdit(); });
    });

    // Tum silinenler secili mi?
    self.allDeleteSelected = ko.computed(function() {
        var perms = self.modulePermissions();
        if (!perms.length) return false;
        return perms.every(function(m) { return m.canDelete(); });
    });

    // Tum moduller secili mi? (view bazinda)
    self.allModulesSelected = ko.computed(function() {
        return self.allViewSelected();
    });

    // Izin tipine gore tumunu sec/kaldir
    self.toggleSelectAllByType = function(type) {
        var perms = self.modulePermissions();
        var allSelected = false;

        switch(type) {
            case 'view':
                allSelected = self.allViewSelected();
                perms.forEach(function(m) {
                    m.canView(!allSelected);
                    if (allSelected) {
                        m.canCreate(false);
                        m.canEdit(false);
                        m.canDelete(false);
                    }
                });
                break;
            case 'create':
                allSelected = self.allCreateSelected();
                perms.forEach(function(m) {
                    if (m.canView()) {
                        m.canCreate(!allSelected);
                    }
                });
                break;
            case 'edit':
                allSelected = self.allEditSelected();
                perms.forEach(function(m) {
                    if (m.canView()) {
                        m.canEdit(!allSelected);
                    }
                });
                break;
            case 'delete':
                allSelected = self.allDeleteSelected();
                perms.forEach(function(m) {
                    if (m.canView()) {
                        m.canDelete(!allSelected);
                    }
                });
                break;
        }
    };

    // Tumunu sec/kaldir (view bazinda)
    self.toggleSelectAll = function() {
        self.toggleSelectAllByType('view');
        return true;
    };

    // Modul iznini toggle et (section ise alt modulleri de etkiler)
    self.toggleModulePermission = function(module, type) {
        // Checkbox zaten toggle edildi, yeni degeri al
        var propName = 'can' + type.charAt(0).toUpperCase() + type.slice(1);
        var newValue = module[propName]();

        // Debug
        console.log('toggleModulePermission:', module.name, 'type:', type, 'newValue:', newValue, 'isSection:', module.isSection, 'children:', module.children?.length);

        // Section ise alt modulleri de guncelle
        if (module.isSection && module.children && module.children.length > 0) {
            console.log('Section children guncelleniyor:', module.children.map(function(c) { return c.name; }));
            setChildrenPermission(module.children, type, newValue);
        }

        // View kapatilirsa diger izinler de kapanir
        if (type === 'view' && !newValue) {
            module.canCreate(false);
            module.canEdit(false);
            module.canDelete(false);
            // Alt modullerin tum izinlerini kapat
            if (module.children && module.children.length > 0) {
                setChildrenAllPermissions(module.children, false);
            }
        }

        return true; // checkbox'in kendi davranisini devam ettir
    };

    // Alt modullerin belirli iznini ayarla (recursive)
    function setChildrenPermission(children, type, value) {
        children.forEach(function(child) {
            var propName = 'can' + type.charAt(0).toUpperCase() + type.slice(1);
            if (type === 'view' || child.canView()) {
                child[propName](value);
            }
            // View acilirsa sadece view ac, diger izinler ayni kalsin
            if (type === 'view' && value) {
                // sadece view ac
            }
            // View kapatilirsa diger izinleri de kapat
            if (type === 'view' && !value) {
                child.canCreate(false);
                child.canEdit(false);
                child.canDelete(false);
            }
            if (child.children && child.children.length > 0) {
                setChildrenPermission(child.children, type, value);
            }
        });
    }

    // Alt modullerin tum izinlerini ayarla
    function setChildrenAllPermissions(children, value) {
        children.forEach(function(child) {
            child.canView(value);
            child.canCreate(value);
            child.canEdit(value);
            child.canDelete(value);
            if (child.children && child.children.length > 0) {
                setChildrenAllPermissions(child.children, value);
            }
        });
    }

    // Filtered roles
    self.filteredRoles = ko.computed(function() {
        if (!self.selectedCapabilityId()) {
            return self.roles();
        }
        return self.roles().filter(function(r) {
            return r.capabilityId === self.selectedCapabilityId();
        });
    });

    // Total role count
    self.totalRoleCount = ko.computed(function() {
        return self.roles().length;
    });

    // Form data
    self.formData = {
        name: ko.observable(''),
        nameResourceKey: ko.observable(''),
        description: ko.observable(''),
        capabilityId: ko.observable(null),
        isDefault: ko.observable(false),
        isActive: ko.observable(true)
    };

    // Delete role
    self.deleteRoleData = ko.observable({ id: null, name: '', userCount: 0, capabilityId: null });
    self.targetRoleId = ko.observable(null);
    self.isDeleting = ko.observable(false);

    // Hedef rol listesi (ayni capability, silinecek rol haric)
    self.availableTargetRoles = ko.computed(function() {
        var data = self.deleteRoleData();
        if (!data.capabilityId) return [];
        return self.roles().filter(function(r) {
            return r.capabilityId === data.capabilityId && r.id !== data.id && r.isActive;
        });
    });

    // Silme butonu aktif mi?
    self.canDelete = ko.computed(function() {
        if (self.isDeleting()) return false;
        var data = self.deleteRoleData();
        // Kullanici yoksa direkt silebilir
        if (data.userCount === 0) return true;
        // Kullanici varsa hedef rol secilmeli
        return self.targetRoleId() != null;
    });

    // Modals
    self.roleModal = null;
    self.permissionsModal = null;
    self.deleteRoleModal = null;

    // Load capabilities
    self.loadCapabilities = function() {
        $.get('/api/admin/capabilities')
            .done(function(data) {
                data.forEach(function(cap) {
                    cap.roleCount = cap.roleCount || 0;
                });
                self.capabilities(data);
            });
    };

    // Load roles
    self.loadRoles = function() {
        self.isLoading(true);
        $.get('/api/admin/roles')
            .done(function(data) {
                self.roles(data);
                // Capability role counts guncelle
                self.capabilities().forEach(function(cap) {
                    cap.roleCount = data.filter(function(r) { return r.capabilityId === cap.id; }).length;
                });
                self.capabilities.valueHasMutated();
            })
            .fail(function() {
                toastr.error('Roller yuklenemedi');
            })
            .always(function() {
                self.isLoading(false);
            });
    };

    // Select capability
    self.selectCapability = function(capability) {
        if (capability) {
            self.selectedCapabilityId(capability.id);
            self.selectedCapabilityName(capability.name);
        } else {
            self.selectedCapabilityId(null);
            self.selectedCapabilityName(null);
        }
    };

    // Reset form
    self.resetForm = function() {
        self.formData.name('');
        self.formData.nameResourceKey('');
        self.formData.description('');
        self.formData.capabilityId(self.selectedCapabilityId() || (self.capabilities().length ? self.capabilities()[0].id : null));
        self.formData.isDefault(false);
        self.formData.isActive(true);
        self.isEditing(false);
        self.editingId(null);
    };

    // Show add modal
    self.showAddModal = function() {
        self.resetForm();
        self.roleModal.show();
    };

    // Show edit modal
    self.showEditModal = function(role) {
        self.isEditing(true);
        self.editingId(role.id);
        self.formData.name(role.name);
        self.formData.nameResourceKey(role.nameResourceKey || '');
        self.formData.description(role.description || '');
        self.formData.capabilityId(role.capabilityId);
        self.formData.isDefault(role.isDefault || false);
        self.formData.isActive(role.isActive);
        self.roleModal.show();
    };

    // Save role
    self.saveRole = function() {
        var data = {
            name: self.formData.name(),
            nameResourceKey: self.formData.nameResourceKey() || null,
            description: self.formData.description(),
            capabilityId: parseInt(self.formData.capabilityId()),
            isDefault: self.formData.isDefault(),
            isActive: self.formData.isActive()
        };

        if (!data.name || !data.capabilityId) {
            toastr.warning('Ad ve capability zorunludur');
            return;
        }

        self.isSaving(true);
        var url = self.isEditing() ? '/api/admin/roles/' + self.editingId() : '/api/admin/roles';
        var method = self.isEditing() ? 'PUT' : 'POST';

        $.ajax({
            url: url,
            method: method,
            contentType: 'application/json',
            data: JSON.stringify(data)
        })
        .done(function(response) {
            toastr.success(response.message);
            self.roleModal.hide();
            self.loadRoles();
        })
        .fail(function(xhr) {
            var msg = xhr.responseJSON?.message || 'Bir hata olustu';
            toastr.error(msg);
        })
        .always(function() {
            self.isSaving(false);
        });
    };

    // Confirm delete role (show modal)
    self.confirmDeleteRole = function(role) {
        self.deleteRoleData({
            id: role.id,
            name: role.name,
            userCount: role.userCount || 0,
            capabilityId: role.capabilityId
        });
        self.targetRoleId(null);
        self.deleteRoleModal.show();
    };

    // Execute delete role
    self.executeDeleteRole = function() {
        var data = self.deleteRoleData();
        var url = '/api/admin/roles/' + data.id;

        // Kullanici varsa hedef rol ekle
        if (data.userCount > 0 && self.targetRoleId()) {
            url += '?targetRoleId=' + self.targetRoleId();
        }

        self.isDeleting(true);
        $.ajax({
            url: url,
            method: 'DELETE'
        })
        .done(function(response) {
            toastr.success(response.message);
            self.deleteRoleModal.hide();
            self.loadRoles();
        })
        .fail(function(xhr) {
            var msg = xhr.responseJSON?.message || 'Silinemedi';
            toastr.error(msg);
        })
        .always(function() {
            self.isDeleting(false);
        });
    };

    // Show permissions modal
    self.showPermissionsModal = function(role) {
        self.selectedRole(role);
        self.loadPermissions(role.id);
        self.permissionsModal.show();
    };

    // Load permissions - capability bazli gruplu
    self.loadPermissions = function(roleId) {
        $.get('/api/admin/roles/' + roleId + '/permissions/grouped')
            .done(function(data) {
                // Debug
                console.log('Gruplu moduller:', data.length, 'capability');

                var allModules = [];
                var groups = data.map(function(group) {
                    // Her capability icin modul listesini olustur
                    var modules = group.modules.map(function(p) {
                        var mod = {
                            moduleId: p.moduleId,
                            parentId: p.parentId == null ? null : p.parentId,
                            name: p.name,
                            icon: p.icon,
                            isSection: p.isSection,
                            children: [],
                            canView: ko.observable(p.canView),
                            canCreate: ko.observable(p.canCreate),
                            canEdit: ko.observable(p.canEdit),
                            canDelete: ko.observable(p.canDelete)
                        };
                        allModules.push(mod);
                        return mod;
                    });

                    // Bu capability icin tree olustur
                    var tree = buildPermissionTree(modules, null);

                    return {
                        capabilityId: group.capabilityId,
                        capabilityName: group.capabilityName,
                        capabilityIcon: group.capabilityIcon,
                        modules: tree,
                        isExpanded: ko.observable(true)
                    };
                });

                self.modulePermissions(allModules);
                self.capabilityGroups(groups);
            });
    };

    // Save permissions
    self.savePermissions = function() {
        var data = self.modulePermissions().map(function(p) {
            return {
                moduleId: p.moduleId,
                canView: p.canView(),
                canCreate: p.canCreate(),
                canEdit: p.canEdit(),
                canDelete: p.canDelete()
            };
        });

        // Debug: Kayit edilecek verileri goster
        console.log('Kaydedilecek izinler:', data.filter(function(d) { return d.canView; }));

        self.isSavingPermissions(true);
        $.ajax({
            url: '/api/admin/roles/' + self.selectedRole().id + '/permissions',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data)
        })
        .done(function(response) {
            toastr.success(response.message);
            self.permissionsModal.hide();
        })
        .fail(function(xhr) {
            var msg = xhr.responseJSON?.message || 'Izinler kaydedilemedi';
            toastr.error(msg);
        })
        .always(function() {
            self.isSavingPermissions(false);
        });
    };

    // Initialize
    self.init = function() {
        self.roleModal = new bootstrap.Modal(document.getElementById('roleModal'));
        self.permissionsModal = new bootstrap.Modal(document.getElementById('permissionsModal'));
        self.deleteRoleModal = new bootstrap.Modal(document.getElementById('deleteRoleModal'));
        self.loadCapabilities();
        self.loadRoles();
    };

    self.init();
}

// Apply bindings
$(function() {
    ko.applyBindings(new RolesViewModel(), document.getElementById('roles-app'));
});
