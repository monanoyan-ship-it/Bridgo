function ProductReviewsViewModel() {
    var self = this;

    // State
    self.isLoadingMy = ko.observable(false);
    self.isLoadingIncoming = ko.observable(false);
    self.isSaving = ko.observable(false);
    self.isEditing = ko.observable(false);
    self.modalError = ko.observable('');

    // Data
    self.myReviews = ko.observableArray([]);
    self.incomingReviews = ko.observableArray([]);
    self.stats = ko.observable(null);

    // My Reviews Filters
    self.mySearchQuery = ko.observable('');
    self.myFilterRating = ko.observable('');

    // Incoming Reviews Filters
    self.incomingSearchQuery = ko.observable('');
    self.incomingFilterRating = ko.observable('');
    self.incomingFilterResponse = ko.observable('');

    // Edit form
    self.editingId = ko.observable(null);
    self.form = {
        overallRating: ko.observable(0),
        qualityRating: ko.observable(null),
        valueRating: ko.observable(null),
        title: ko.observable(''),
        comment: ko.observable(''),
        pros: ko.observable(''),
        cons: ko.observable(''),
        wouldRecommend: ko.observable(true)
    };

    // Delete
    self.deleteReviewId = ko.observable(null);
    self.deleteReviewName = ko.observable('');

    // Respond
    self.respondingReview = ko.observable(null);
    self.respondText = ko.observable('');

    // ===== Filtered lists =====

    self.filteredMyReviews = ko.computed(function () {
        var result = self.myReviews();
        var query = self.mySearchQuery().toLowerCase().trim();
        var rating = self.myFilterRating();

        if (query) {
            result = result.filter(function (r) {
                return (r.productName && r.productName.toLowerCase().indexOf(query) > -1) ||
                       (r.title && r.title.toLowerCase().indexOf(query) > -1) ||
                       (r.comment && r.comment.toLowerCase().indexOf(query) > -1);
            });
        }

        if (rating) {
            result = result.filter(function (r) {
                return r.overallRating == rating;
            });
        }

        return result;
    });

    self.filteredIncomingReviews = ko.computed(function () {
        var result = self.incomingReviews();
        var query = self.incomingSearchQuery().toLowerCase().trim();
        var rating = self.incomingFilterRating();
        var responseFilter = self.incomingFilterResponse();

        if (query) {
            result = result.filter(function (r) {
                return (r.productName && r.productName.toLowerCase().indexOf(query) > -1) ||
                       (r.buyerCompanyName && r.buyerCompanyName.toLowerCase().indexOf(query) > -1) ||
                       (r.title && r.title.toLowerCase().indexOf(query) > -1) ||
                       (r.comment && r.comment.toLowerCase().indexOf(query) > -1);
            });
        }

        if (rating) {
            result = result.filter(function (r) {
                return r.overallRating == rating;
            });
        }

        if (responseFilter === 'responded') {
            result = result.filter(function (r) { return r.sellerResponse; });
        } else if (responseFilter === 'pending') {
            result = result.filter(function (r) { return !r.sellerResponse; });
        }

        return result;
    });

    // ===== Star helpers =====

    self.getStars = function (rating) {
        var arr = [];
        for (var i = 0; i < rating; i++) arr.push(i);
        return arr;
    };

    self.getEmptyStars = function (rating) {
        var arr = [];
        for (var i = rating; i < 5; i++) arr.push(i);
        return arr;
    };

    // ===== Load data =====

    self.loadMyReviews = function () {
        self.isLoadingMy(true);
        fetch('/api/product-reviews/my')
            .then(function (r) { return r.json(); })
            .then(function (data) {
                self.myReviews(data);
            })
            .catch(function (err) {
                console.error('My reviews load error:', err);
                toastr.error(T('Common.Error.Load', 'Veriler yuklenirken hata olustu'));
            })
            .finally(function () {
                self.isLoadingMy(false);
            });
    };

    self.loadIncomingReviews = function () {
        self.isLoadingIncoming(true);
        fetch('/api/product-reviews/incoming')
            .then(function (r) { return r.json(); })
            .then(function (data) {
                self.incomingReviews(data);
            })
            .catch(function (err) {
                console.error('Incoming reviews load error:', err);
                toastr.error(T('Common.Error.Load', 'Veriler yuklenirken hata olustu'));
            })
            .finally(function () {
                self.isLoadingIncoming(false);
            });
    };

    self.loadStats = function () {
        fetch('/api/product-reviews/stats')
            .then(function (r) { return r.json(); })
            .then(function (data) {
                self.stats(data);
            })
            .catch(function (err) {
                console.error('Stats load error:', err);
            });
    };

    // ===== Edit modal =====

    self.resetForm = function () {
        self.editingId(null);
        self.isEditing(false);
        self.modalError('');
        self.form.overallRating(0);
        self.form.qualityRating(null);
        self.form.valueRating(null);
        self.form.title('');
        self.form.comment('');
        self.form.pros('');
        self.form.cons('');
        self.form.wouldRecommend(true);
    };

    self.openEditModal = function (review) {
        self.resetForm();
        self.editingId(review.id);
        self.isEditing(true);
        self.form.overallRating(review.overallRating);
        self.form.qualityRating(review.qualityRating);
        self.form.valueRating(review.valueRating);
        self.form.title(review.title || '');
        self.form.comment(review.comment || '');
        self.form.pros(review.pros || '');
        self.form.cons(review.cons || '');
        self.form.wouldRecommend(review.wouldRecommend);

        var modal = new bootstrap.Modal(document.getElementById('reviewModal'));
        modal.show();
    };

    self.saveReview = function () {
        if (self.form.overallRating() < 1) {
            self.modalError(T('ProductReview.Error.RatingRequired', 'Lutfen bir puan verin'));
            return;
        }

        self.modalError('');
        self.isSaving(true);

        var url = '/api/product-reviews/' + self.editingId();
        var method = 'PUT';

        var data = {
            overallRating: self.form.overallRating(),
            qualityRating: self.form.qualityRating(),
            valueRating: self.form.valueRating(),
            title: self.form.title(),
            comment: self.form.comment(),
            pros: self.form.pros(),
            cons: self.form.cons(),
            wouldRecommend: self.form.wouldRecommend()
        };

        fetch(url, {
            method: method,
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        })
        .then(function (r) {
            if (r.ok) {
                bootstrap.Modal.getInstance(document.getElementById('reviewModal')).hide();
                self.loadMyReviews();
                toastr.success(T('ProductReview.Success.Updated', 'Degerlendirme guncellendi'));
            } else {
                return r.json().then(function (err) {
                    self.modalError(err.message || T('Common.Error.Save', 'Kayit sirasinda hata olustu'));
                });
            }
        })
        .catch(function (err) {
            console.error('Save error:', err);
            self.modalError(T('Common.Error.Save', 'Kayit sirasinda hata olustu'));
        })
        .finally(function () {
            self.isSaving(false);
        });
    };

    // ===== Delete =====

    self.confirmDelete = function (review) {
        self.deleteReviewId(review.id);
        self.deleteReviewName(review.productName + (review.title ? ' - ' + review.title : ''));
        var modal = new bootstrap.Modal(document.getElementById('deleteReviewModal'));
        modal.show();
    };

    self.deleteReview = function () {
        fetch('/api/product-reviews/' + self.deleteReviewId(), { method: 'DELETE' })
            .then(function (r) {
                if (r.ok) {
                    bootstrap.Modal.getInstance(document.getElementById('deleteReviewModal')).hide();
                    self.loadMyReviews();
                    toastr.success(T('ProductReview.Success.Deleted', 'Degerlendirme silindi'));
                } else {
                    return r.json().then(function (err) {
                        toastr.error(err.message || T('Common.Error.Delete', 'Silme sirasinda hata olustu'));
                    });
                }
            })
            .catch(function (err) {
                console.error('Delete error:', err);
                toastr.error(T('Common.Error.Delete', 'Silme sirasinda hata olustu'));
            });
    };

    // ===== Respond modal (Seller) =====

    self.openRespondModal = function (review) {
        self.respondingReview(review);
        self.respondText('');
        var modal = new bootstrap.Modal(document.getElementById('respondModal'));
        modal.show();
    };

    self.submitResponse = function () {
        if (!self.respondText().trim()) {
            toastr.warning(T('ProductReview.Error.ResponseRequired', 'Yanit metni zorunludur'));
            return;
        }

        self.isSaving(true);

        fetch('/api/product-reviews/' + self.respondingReview().id + '/respond', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ response: self.respondText().trim() })
        })
        .then(function (r) {
            if (r.ok) {
                bootstrap.Modal.getInstance(document.getElementById('respondModal')).hide();
                self.loadIncomingReviews();
                self.loadStats();
                toastr.success(T('ProductReview.Success.Responded', 'Yanit gonderildi'));
            } else {
                return r.json().then(function (err) {
                    toastr.error(err.message || T('Common.Error.Save', 'Kayit sirasinda hata olustu'));
                });
            }
        })
        .catch(function (err) {
            console.error('Respond error:', err);
            toastr.error(T('Common.Error.Save', 'Kayit sirasinda hata olustu'));
        })
        .finally(function () {
            self.isSaving(false);
        });
    };

    // ===== Initialize =====
    self.loadMyReviews();
    self.loadIncomingReviews();
    self.loadStats();
}

$(document).ready(function () {
    ko.applyBindings(new ProductReviewsViewModel(), document.getElementById('product-reviews-app'));
});
