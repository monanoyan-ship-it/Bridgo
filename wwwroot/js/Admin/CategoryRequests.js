// Admin Category Requests ViewModel
(function () {
    'use strict';

    function AdminCategoryRequestsViewModel() {
        var self = this;

        // Observables
        self.requests = ko.observableArray([]);
        self.statusFilter = ko.observable('');
        self.searchQuery = ko.observable('');
        self.isLoading = ko.observable(false);
        self.isSaving = ko.observable(false);
        self.pendingCount = ko.observable(0);

        // Selected request for modals
        self.selectedRequest = ko.observable(null);
        self.similarCategories = ko.observableArray([]);
        self.parentCategoryOptions = ko.observableArray([]);

        // Review form
        self.reviewForm = {
            statusId: ko.observable(''),
            reviewNote: ko.observable(''),
            parentCategoryId: ko.observable(null),
            approvedName: ko.observable('')
        };

        // Filtered requests
        self.filteredRequests = ko.computed(function () {
            var query = self.searchQuery().toLowerCase().trim();
            var requests = self.requests();

            if (!query) {
                return requests;
            }

            return requests.filter(function (r) {
                return r.requestedName.toLowerCase().indexOf(query) !== -1 ||
                    (r.vendorName && r.vendorName.toLowerCase().indexOf(query) !== -1) ||
                    (r.requestedByUserName && r.requestedByUserName.toLowerCase().indexOf(query) !== -1);
            });
        });

        // Load all requests
        self.loadRequests = function () {
            self.isLoading(true);
            var url = '/api/category-requests/admin/all';
            var status = self.statusFilter();
            if (status) {
                url += '?status=' + status;
            }

            fetch(url)
                .then(function (response) { return response.json(); })
                .then(function (data) {
                    var mapped = data.map(function (item) {
                        return self.mapRequest(item);
                    });
                    self.requests(mapped);
                })
                .catch(function (error) {
                    console.error('Error loading requests:', error);
                    toastr.error('Talepler yuklenirken hata olustu');
                })
                .finally(function () {
                    self.isLoading(false);
                });
        };

        // Load pending count
        self.loadPendingCount = function () {
            fetch('/api/category-requests/admin/pending-count')
                .then(function (response) { return response.json(); })
                .then(function (data) {
                    self.pendingCount(data.count || 0);
                })
                .catch(function (error) {
                    console.error('Error loading pending count:', error);
                });
        };

        // Map request from API
        self.mapRequest = function (item) {
            item.formattedDate = new Date(item.createdAt).toLocaleDateString('tr-TR');
            item.formattedReviewDate = item.reviewedAt ? new Date(item.reviewedAt).toLocaleDateString('tr-TR') : null;
            item.statusBadgeClass = item.statusCssClass || 'bg-secondary';
            return item;
        };

        // Load parent category options
        self.loadParentCategories = function () {
            fetch('/api/products/categories/select')
                .then(function (response) { return response.json(); })
                .then(function (data) {
                    self.parentCategoryOptions(data);
                })
                .catch(function (error) {
                    console.error('Error loading categories:', error);
                });
        };

        // Check similar categories
        self.checkSimilarCategories = function (name) {
            if (!name || name.length < 2) {
                self.similarCategories([]);
                return;
            }

            fetch('/api/category-requests/check-similar?name=' + encodeURIComponent(name))
                .then(function (response) { return response.json(); })
                .then(function (data) {
                    self.similarCategories(data);
                })
                .catch(function (error) {
                    console.error('Error checking similar:', error);
                });
        };

        // Show review modal
        self.showReviewModal = function (request) {
            self.selectedRequest(request);
            self.reviewForm.statusId('');
            self.reviewForm.reviewNote('');
            self.reviewForm.parentCategoryId(request.suggestedParentCategoryId);
            self.reviewForm.approvedName(request.requestedName);
            self.similarCategories([]);

            self.loadParentCategories();
            self.checkSimilarCategories(request.requestedName);

            var modal = new bootstrap.Modal(document.getElementById('reviewModal'));
            modal.show();
        };

        // Show detail modal
        self.showDetailModal = function (request) {
            self.selectedRequest(request);
            var modal = new bootstrap.Modal(document.getElementById('detailModal'));
            modal.show();
        };

        // Submit review
        self.submitReview = function () {
            var statusId = self.reviewForm.statusId();
            if (!statusId) {
                toastr.warning('Lutfen bir durum secin');
                return;
            }

            var request = self.selectedRequest();
            if (!request) return;

            var payload = {
                id: request.id,
                statusId: parseInt(statusId),
                reviewNote: self.reviewForm.reviewNote()
            };

            // If approving, include parent category and name
            if (statusId === '2') {
                payload.parentCategoryId = self.reviewForm.parentCategoryId();
                payload.approvedName = self.reviewForm.approvedName().trim() || request.requestedName;
            }

            self.isSaving(true);
            fetch('/api/category-requests/admin/review', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            })
                .then(function (response) {
                    if (!response.ok) {
                        return response.json().then(function (err) { throw new Error(err.message); });
                    }
                    return response.json();
                })
                .then(function (data) {
                    var messages = {
                        '2': 'Talep onaylandi ve kategori olusturuldu',
                        '3': 'Talep reddedildi',
                        '4': 'Talep mevcut kategori olarak isaretlendi'
                    };
                    toastr.success(messages[statusId] || 'Talep guncellendi');
                    bootstrap.Modal.getInstance(document.getElementById('reviewModal')).hide();
                    self.loadRequests();
                    self.loadPendingCount();
                })
                .catch(function (error) {
                    toastr.error(error.message || 'Islem sirasinda hata olustu');
                })
                .finally(function () {
                    self.isSaving(false);
                });
        };

        // Status filter change
        self.statusFilter.subscribe(function () {
            self.loadRequests();
        });

        // Initialize
        self.loadRequests();
        self.loadPendingCount();
    }

    // Initialize when DOM ready
    document.addEventListener('DOMContentLoaded', function () {
        var vm = new AdminCategoryRequestsViewModel();
        window.vm = vm;
        ko.applyBindings({ vm: vm }, document.querySelector('.container-fluid'));
    });
})();
