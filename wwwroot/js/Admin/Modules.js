// Admin Modules ViewModel
function ModulesViewModel() {
    var self = this;

    // Observables
    self.capabilities = ko.observableArray([]);
    self.modules = ko.observableArray([]);
    self.deletedModules = ko.observableArray([]);
    self.platformModules = ko.observableArray([]);
    self.selectedModuleIds = ko.observableArray([]); // Coklu secim icin
    self.isManualEntry = ko.observable(false); // Manuel giris mi?
    self.selectedCapabilityId = ko.observable(null);
    self.isPlatformSelected = ko.observable(false); // Platform (id=0) secili mi?
    self.selectedCapabilityName = ko.observable('');
    self.isLoading = ko.observable(false);
    self.isSaving = ko.observable(false);
    self.isEditing = ko.observable(false);
    self.editingId = ko.observable(null);
    self.parentModuleId = ko.observable(null);
    self.showDeleted = ko.observable(false);

    // Platform modulleri zaten tree olarak geliyor (API BuildModuleTree kullaniyor)
    self.platformModulesTree = ko.computed(function() {
        return self.platformModules();
    });

    // Platform secim alani gosterilsin mi?
    self.showPlatformSelect = ko.computed(function() {
        return !self.isEditing() && !self.isPlatformSelected();
    });

    // Modul checkbox degistiginde (parent-child iliskisi)
    self.onModuleCheckChange = function(module, isChecked) {
        if (isChecked) {
            // Section secildi ise tum alt modulleri de sec
            if (module.isMenuSection && module.children && module.children.length > 0) {
                self.addChildrenToSelection(module.children);
            }

            // Parent section'i da sec (menu icin gerekli)
            if (module.parentId) {
                var parent = self.findModuleById(module.parentId);
                if (parent && parent.isMenuSection) {
                    if (self.selectedModuleIds.indexOf(parent.id) === -1) {
                        self.selectedModuleIds.push(parent.id);
                    }
                    // Grandparent varsa onu da sec
                    if (parent.parentId) {
                        var grandParent = self.findModuleById(parent.parentId);
                        if (grandParent && grandParent.isMenuSection) {
                            if (self.selectedModuleIds.indexOf(grandParent.id) === -1) {
                                self.selectedModuleIds.push(grandParent.id);
                            }
                        }
                    }
                }
            }
        } else {
            // Section secimi kaldirildi - tum alt modulleri de kaldir
            if (module.isMenuSection && module.children && module.children.length > 0) {
                self.removeChildrenFromSelection(module.children);
            }
        }
    };

    // Alt modulleri secime ekle (recursive)
    self.addChildrenToSelection = function(children) {
        for (var i = 0; i < children.length; i++) {
            var child = children[i];
            if (self.selectedModuleIds.indexOf(child.id) === -1) {
                self.selectedModuleIds.push(child.id);
            }
            if (child.children && child.children.length > 0) {
                self.addChildrenToSelection(child.children);
            }
        }
    };

    // Alt modulleri secimden cikar (recursive)
    self.removeChildrenFromSelection = function(children) {
        for (var i = 0; i < children.length; i++) {
            var child = children[i];
            self.selectedModuleIds.remove(child.id);
            if (child.children && child.children.length > 0) {
                self.removeChildrenFromSelection(child.children);
            }
        }
    };

    // Modal title
    self.modalTitle = ko.computed(function() {
        if (self.isEditing()) return 'Modul Duzenle';
        if (self.parentModuleId()) return 'Alt Modul Ekle';
        return 'Yeni Modul';
    });

    // Form data
    self.formData = {
        name: ko.observable(''),
        displayName: ko.observable(''),
        displayNameResourceKey: ko.observable(''),
        description: ko.observable(''),
        icon: ko.observable('bi-box'),
        route: ko.observable(''),
        isMenuSection: ko.observable(false),
        displayOrder: ko.observable(1),
        isMenuItem: ko.observable(true),
        isActive: ko.observable(true)
    };

    // Icon listesi (populer Bootstrap Icons)
    self.availableIcons = ko.observableArray([
        // Genel
        { icon: 'bi-box', label: 'Kutu' },
        { icon: 'bi-grid', label: 'Grid' },
        { icon: 'bi-grid-3x3-gap', label: 'Grid Gap' },
        { icon: 'bi-speedometer2', label: 'Dashboard' },
        { icon: 'bi-house', label: 'Ev' },
        { icon: 'bi-gear', label: 'Ayarlar' },
        { icon: 'bi-sliders', label: 'Slider' },
        // Islemler
        { icon: 'bi-file-earmark-text', label: 'Dosya' },
        { icon: 'bi-file-earmark-richtext', label: 'Belge' },
        { icon: 'bi-file-earmark-check', label: 'Onay' },
        { icon: 'bi-folder', label: 'Klasor' },
        { icon: 'bi-bag', label: 'Siparis' },
        { icon: 'bi-cart', label: 'Sepet' },
        { icon: 'bi-shop', label: 'Magaza' },
        { icon: 'bi-shop-window', label: 'Vitrin' },
        { icon: 'bi-hammer', label: 'Acik Artirma' },
        { icon: 'bi-download', label: 'Indirme' },
        { icon: 'bi-upload', label: 'Yukleme' },
        // Kullanici/Hesap
        { icon: 'bi-person', label: 'Kisi' },
        { icon: 'bi-person-circle', label: 'Profil' },
        { icon: 'bi-people', label: 'Ekip' },
        { icon: 'bi-person-badge', label: 'Kimlik' },
        { icon: 'bi-building', label: 'Bina' },
        { icon: 'bi-building-check', label: 'Gumruk' },
        { icon: 'bi-geo-alt', label: 'Konum' },
        { icon: 'bi-credit-card', label: 'Odeme' },
        { icon: 'bi-bank', label: 'Banka' },
        { icon: 'bi-cash-stack', label: 'Para' },
        // Iletisim
        { icon: 'bi-chat-dots', label: 'Mesaj' },
        { icon: 'bi-chat-square-text', label: 'Yorum' },
        { icon: 'bi-bell', label: 'Bildirim' },
        { icon: 'bi-envelope', label: 'E-posta' },
        // Durum/Isaret
        { icon: 'bi-check-circle', label: 'Onay' },
        { icon: 'bi-patch-check', label: 'Dogrulama' },
        { icon: 'bi-shield-check', label: 'Guvenlik' },
        { icon: 'bi-shield-lock', label: 'Kilit' },
        { icon: 'bi-star', label: 'Yildiz' },
        { icon: 'bi-lightning', label: 'Islem' },
        // Lojistik
        { icon: 'bi-truck', label: 'Tasima' },
        { icon: 'bi-airplane', label: 'Ucak' },
        { icon: 'bi-globe', label: 'Dunya' },
        // Diger
        { icon: 'bi-card-checklist', label: 'Liste' },
        { icon: 'bi-calendar', label: 'Takvim' },
        { icon: 'bi-clock', label: 'Saat' },
        { icon: 'bi-search', label: 'Arama' },
        { icon: 'bi-filter', label: 'Filtre' },
        { icon: 'bi-share', label: 'Paylas' },
        { icon: 'bi-link-45deg', label: 'Link' },
        { icon: 'bi-qr-code', label: 'QR' },
        { icon: 'bi-bar-chart', label: 'Grafik' },
        { icon: 'bi-pie-chart', label: 'Pasta' },
        { icon: 'bi-graph-up', label: 'Artis' }
    ]);

    // Icon secilebilir mi goster
    self.showIconPicker = ko.observable(false);

    // Icon sec
    self.selectIcon = function(iconItem) {
        self.formData.icon(iconItem.icon);
        self.showIconPicker(false);
    };

    // Icon picker toggle
    self.toggleIconPicker = function() {
        self.showIconPicker(!self.showIconPicker());
    };

    // Section olan moduller (IsMenuSection = true) - flat list
    self.sectionModules = ko.computed(function() {
        return self.modules().filter(function(m) {
            return m.isMenuSection === true;
        });
    });

    // Section modullerini tree olarak getir (root level'dan basla)
    self.sectionModulesTree = ko.computed(function() {
        return self.modules().filter(function(m) {
            return m.isMenuSection === true;
        });
    });

    // Children icinden sadece section olanlari filtrele
    self.filterSections = function(children) {
        if (!children) return [];
        return children.filter(function(m) {
            return m.isMenuSection === true;
        });
    };

    // IsMenuSection gosterilsin mi? (Platform capability veya manuel giris)
    self.showIsMenuSection = ko.computed(function() {
        return self.isPlatformSelected() || self.isManualEntry();
    });

    // Modal
    self.modal = null;

    // Load capabilities
    self.loadCapabilities = function() {
        $.get('/api/admin/capabilities')
            .done(function(data) {
                // Platform butonu ekle (tum modulleri gosterir)
                var platformBtn = {
                    id: 0,
                    code: 'platform',
                    name: 'Platform',
                    icon: 'bi-grid-3x3-gap',
                    moduleCount: ko.observable(0)
                };

                // Modul sayisini observable yap
                data.forEach(function(cap) {
                    cap.moduleCount = ko.observable(cap.moduleCount || 0);
                });

                // Platform'u basa ekle
                data.unshift(platformBtn);
                self.capabilities(data);

                // URL'den capability sec veya Platform'u sec
                var urlParams = new URLSearchParams(window.location.search);
                var capId = urlParams.get('capability');
                if (capId) {
                    var cap = data.find(function(c) { return c.id == capId; });
                    if (cap) {
                        self.selectCapability(cap);
                    }
                } else {
                    // Varsayilan olarak Platform sec
                    self.selectCapability(platformBtn);
                }
            });
    };

    // Select capability
    self.selectCapability = function(capability) {
        self.selectedCapabilityId(capability.id);
        self.isPlatformSelected(capability.id === 0); // Platform id = 0
        self.selectedCapabilityName(capability.name);
        // Platform degilse cop kutusunu kapat
        if (capability.id !== 0) {
            self.showDeleted(false);
        }
        self.loadModules();
    };

    // Load modules
    self.loadModules = function() {
        self.isLoading(true);

        // Platform seciliyse tum modulleri getir
        var url = self.isPlatformSelected()
            ? '/api/admin/modules'
            : '/api/admin/capabilities/' + self.selectedCapabilityId() + '/modules';

        $.get(url)
            .done(function(data) {
                // Capability icin sadece isSelected olan modulleri goster
                if (!self.isPlatformSelected()) {
                    data = self.filterSelectedModules(data);
                }
                self.modules(data);
                self.updateCapabilityModuleCount(data);
            })
            .fail(function() {
                toastr.error('Moduller yuklenemedi');
            })
            .always(function() {
                self.isLoading(false);
            });
    };

    // Capability'nin modul sayisini guncelle (tree'deki tum modulleri say)
    self.updateCapabilityModuleCount = function(modules) {
        var count = self.countModulesRecursive(modules);
        var selectedId = self.selectedCapabilityId();

        // Observable'i guncelle
        var caps = self.capabilities();
        for (var i = 0; i < caps.length; i++) {
            if (caps[i].id === selectedId) {
                caps[i].moduleCount(count);
                break;
            }
        }
    };

    // Capability'ye platform modulleri ekle (coklu mapping olustur)
    self.addModulesToCapability = function(moduleIds) {
        if (!moduleIds || moduleIds.length === 0) {
            toastr.warning('Lutfen en az bir modul secin');
            return;
        }

        self.isSaving(true);

        $.ajax({
            url: '/api/admin/capabilities/' + self.selectedCapabilityId() + '/modules/add-multiple',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ moduleIds: moduleIds })
        })
        .done(function(response) {
            toastr.success(response.message || moduleIds.length + ' modul eklendi');
            self.modal.hide();
            self.loadModules();
        })
        .fail(function(xhr) {
            var msg = xhr.responseJSON?.message || 'Bir hata olustu';
            toastr.error(msg);
        })
        .always(function() {
            self.isSaving(false);
        });
    };

    // Platform modules icinde ID ile modul bul (recursive)
    self.findModuleById = function(id) {
        return self.findModuleInTree(self.platformModules(), id);
    };

    self.findModuleInTree = function(modules, id) {
        for (var i = 0; i < modules.length; i++) {
            if (modules[i].id === id) return modules[i];
            if (modules[i].children && modules[i].children.length > 0) {
                var found = self.findModuleInTree(modules[i].children, id);
                if (found) return found;
            }
        }
        return null;
    };

    // Sadece isSelected olan modulleri filtrele (recursive)
    self.filterSelectedModules = function(modules) {
        var result = [];
        for (var i = 0; i < modules.length; i++) {
            var m = modules[i];
            if (m.isSelected) {
                var copy = Object.assign({}, m);
                if (m.children && m.children.length > 0) {
                    copy.children = self.filterSelectedModules(m.children);
                }
                result.push(copy);
            }
        }
        return result;
    };

    // Tree'deki modulleri recursive say
    self.countModulesRecursive = function(modules) {
        var count = 0;
        for (var i = 0; i < modules.length; i++) {
            count++;
            if (modules[i].children && modules[i].children.length > 0) {
                count += self.countModulesRecursive(modules[i].children);
            }
        }
        return count;
    };

    // Reset form
    self.resetForm = function() {
        self.formData.name('');
        self.formData.displayName('');
        self.formData.displayNameResourceKey('');
        self.formData.description('');
        self.formData.icon('bi-box');
        self.formData.route('');
        self.formData.isMenuSection(false);
        self.formData.displayOrder(1);
        self.formData.isMenuItem(true);
        self.formData.isActive(true);
        self.isEditing(false);
        self.editingId(null);
        self.parentModuleId(null);
        self.selectedModuleIds([]);
        self.isManualEntry(false);
        self.showIconPicker(false);
    };

    // Show add modal
    self.showAddModal = function() {
        self.resetForm();
        self.modal.show();
    };

    // Show add sub modal
    self.showAddSubModal = function(parentModule) {
        self.resetForm();
        self.parentModuleId(parentModule.id);
        self.modal.show();
    };

    // Show edit modal
    self.showEditModal = function(module) {
        self.isEditing(true);
        self.editingId(module.id);
        self.parentModuleId(module.parentId);
        self.formData.name(module.name);
        self.formData.displayName(module.displayName || '');
        self.formData.displayNameResourceKey(module.displayNameResourceKey || '');
        self.formData.description(module.description || '');
        self.formData.icon(module.icon || 'bi-box');
        self.formData.route(module.route || '');
        self.formData.isMenuSection(module.isMenuSection || false);
        self.formData.displayOrder(module.displayOrder);
        self.formData.isMenuItem(module.isMenuItem);
        self.formData.isActive(module.isActive);
        self.showIconPicker(false);
        self.modal.show();
    };

    // Save module
    self.saveModule = function() {
        var isPlatformCapability = self.isPlatformSelected();
        var hasSelectedModules = self.selectedModuleIds().length > 0;
        var isManual = self.isManualEntry();

        // Capability'ye platform modullerini ekleme (coklu mapping olustur)
        if (!isPlatformCapability && !self.isEditing() && hasSelectedModules && !isManual) {
            self.addModulesToCapability(self.selectedModuleIds());
            return;
        }

        // Manuel giris veya Platform capability icin: yeni modul olustur
        // Platform'da capabilityIds bos, capability seciliyse o capability'nin ID'si
        var capIds = [];
        if (!isPlatformCapability && self.selectedCapabilityId()) {
            capIds.push(self.selectedCapabilityId());
        }

        var data = {
            capabilityIds: capIds,
            parentId: self.parentModuleId(),
            name: self.formData.name(),
            displayName: self.formData.displayName(),
            displayNameResourceKey: self.formData.displayNameResourceKey(),
            description: self.formData.description(),
            icon: self.formData.icon(),
            route: self.formData.route(),
            isMenuSection: self.formData.isMenuSection(),
            displayOrder: parseInt(self.formData.displayOrder()) || 1,
            isMenuItem: self.formData.isMenuItem(),
            isActive: self.formData.isActive()
        };

        // Validasyon: Ad zorunlu (manuel giris veya platform icin)
        if ((isManual || isPlatformCapability) && !data.name) {
            toastr.warning('Ad zorunludur');
            return;
        }

        // Capability + manuel giris: validasyon gerekli, modul secilmemis
        if (!isPlatformCapability && !self.isEditing() && !hasSelectedModules && !isManual) {
            toastr.warning('Lutfen en az bir modul secin veya manuel giris yapin');
            return;
        }

        self.isSaving(true);
        var url = self.isEditing() ? '/api/admin/modules/' + self.editingId() : '/api/admin/modules';
        var method = self.isEditing() ? 'PUT' : 'POST';

        $.ajax({
            url: url,
            method: method,
            contentType: 'application/json',
            data: JSON.stringify(data)
        })
        .done(function(response) {
            toastr.success(response.message);
            self.modal.hide();
            self.loadModules();
            // Platform modulleri listesini de guncelle (modal'daki checkbox listesi icin)
            self.loadPlatformModules();
        })
        .fail(function(xhr) {
            var msg = xhr.responseJSON?.message || 'Bir hata olustu';
            toastr.error(msg);
        })
        .always(function() {
            self.isSaving(false);
        });
    };

    // Delete module
    self.deleteModule = function(module) {
        var isPlatform = self.isPlatformSelected();
        var hasChildren = module.children && module.children.length > 0;

        // Platform: Gercek silme (soft delete)
        // Capability: Sadece mapping'i kaldir
        var title, message, confirmText;

        if (isPlatform) {
            // Platform'dan gercek silme
            title = hasChildren ? 'Section ve Alt Modulleri Sil' : 'Modul Sil';
            message = hasChildren
                ? '"' + module.name + '" ve altindaki ' + module.children.length + ' modulu silmek istediginize emin misiniz?'
                : '"' + module.name + '" modulunu silmek istediginize emin misiniz?';
            confirmText = hasChildren ? 'Tumunu Sil' : 'Sil';
        } else {
            // Capability'den cikarma (mapping sil)
            title = 'Modulu Cikar';
            message = '"' + module.name + '" modulunu bu yetenekten cikarmak istediginize emin misiniz? (Platform\'daki modul silinmez)';
            confirmText = 'Cikar';
        }

        showConfirm({
            title: title,
            message: message,
            confirmText: confirmText,
            confirmClass: 'btn-danger',
            onConfirm: function() {
                var url, method;

                if (isPlatform) {
                    // Platform: DELETE /api/admin/modules/{id}
                    url = '/api/admin/modules/' + module.id;
                    method = 'DELETE';
                } else {
                    // Capability: POST /api/admin/capabilities/{id}/modules/remove
                    url = '/api/admin/capabilities/' + self.selectedCapabilityId() + '/modules/remove';
                    method = 'POST';
                }

                $.ajax({
                    url: url,
                    method: method,
                    contentType: 'application/json',
                    data: isPlatform ? null : JSON.stringify({ moduleId: module.id })
                })
                .done(function(response) {
                    toastr.success(response.message);
                    self.loadModules();
                    if (isPlatform) {
                        self.loadPlatformModules();
                    }
                })
                .fail(function(xhr) {
                    var msg = xhr.responseJSON?.message || 'Islem basarisiz';
                    toastr.error(msg);
                });
            }
        });
    };

    // Load platform modules for suggestions
    self.loadPlatformModules = function() {
        $.get('/api/admin/modules')
            .done(function(data) {
                self.platformModules(data);
            });
    };

    // Load deleted modules
    self.loadDeletedModules = function() {
        self.isLoading(true);
        $.get('/api/admin/modules/deleted')
            .done(function(data) {
                self.deletedModules(data);
            })
            .fail(function() {
                toastr.error('Silinmis moduller yuklenemedi');
            })
            .always(function() {
                self.isLoading(false);
            });
    };

    // showDeleted toggle subscribe
    self.showDeleted.subscribe(function(show) {
        if (show) {
            self.loadDeletedModules();
        } else {
            self.loadModules();
        }
    });

    // Restore module
    self.restoreModule = function(module) {
        showConfirm({
            title: 'Modulu Geri Yukle',
            message: '"' + module.name + '" modulunu geri yuklemek istediginize emin misiniz?',
            confirmText: 'Geri Yukle',
            confirmClass: 'btn-success',
            onConfirm: function() {
                $.ajax({
                    url: '/api/admin/modules/' + module.id + '/restore',
                    method: 'POST'
                })
                .done(function(response) {
                    toastr.success(response.message);
                    self.loadDeletedModules();
                    // Platform modulleri de guncelle
                    if (self.isPlatformSelected()) {
                        self.loadPlatformModules();
                    }
                })
                .fail(function(xhr) {
                    var msg = xhr.responseJSON?.message || 'Geri yuklenemedi';
                    toastr.error(msg);
                });
            }
        });
    };

    // Hard delete module
    self.hardDeleteModule = function(module) {
        var hasChildren = module.children && module.children.length > 0;
        var message = hasChildren
            ? '"' + module.name + '" ve altindaki ' + module.children.length + ' modulu kalici olarak silmek istediginize emin misiniz? Bu islem geri alinamaz!'
            : '"' + module.name + '" modulunu kalici olarak silmek istediginize emin misiniz? Bu islem geri alinamaz!';

        showConfirm({
            title: 'Kalici Silme',
            message: message,
            confirmText: 'Kalici Sil',
            confirmClass: 'btn-danger',
            onConfirm: function() {
                $.ajax({
                    url: '/api/admin/modules/' + module.id + '/hard',
                    method: 'DELETE'
                })
                .done(function(response) {
                    toastr.success(response.message);
                    self.loadDeletedModules();
                })
                .fail(function(xhr) {
                    var msg = xhr.responseJSON?.message || 'Kalici silinemedi';
                    toastr.error(msg);
                });
            }
        });
    };

    // Initialize
    self.init = function() {
        self.modal = new bootstrap.Modal(document.getElementById('moduleModal'));
        self.loadCapabilities();
        self.loadPlatformModules();
    };

    self.init();
}

// Apply bindings
$(function() {
    ko.applyBindings(new ModulesViewModel(), document.getElementById('modules-app'));
});
