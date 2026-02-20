(function () {
    'use strict';

    function FeedViewModel() {
        var self = this;

        // State
        self.activeTab = ko.observable('feed');
        self.isLoading = ko.observable(false);
        self.isLoadingMore = ko.observable(false);
        self.posts = ko.observableArray([]);
        self.hasMore = ko.observable(false);
        self.nextCursor = ko.observable(null);
        self.products = ko.observableArray([]);
        self.sortMode = ko.observable('recent');

        // Compose
        self.newPost = {
            postTypeId: ko.observable('1'),
            title: ko.observable(''),
            content: ko.observable(''),
            productId: ko.observable(null)
        };

        // Comments
        self.expandedPostId = ko.observable(null);
        self.currentComments = ko.observableArray([]);
        self.commentsLoading = ko.observable(false);
        self.newCommentText = ko.observable('');

        // Delete
        self.postToDelete = ko.observable(null);

        // Hashtags
        self.trendingHashtags = ko.observableArray([]);
        self.activeHashtag = ko.observable(null);

        // Search
        self.searchQuery = ko.observable('');
        self.searchPostTypeId = ko.observable('');

        // Report
        self.reportPostId = ko.observable(null);
        self.reportReasonId = ko.observable('1');
        self.reportDescription = ko.observable('');

        // Post type names
        var postTypeNames = {
            1: 'Metin',
            2: 'Makale',
            3: 'Duyuru',
            4: 'Urun Tanitimi'
        };

        self.getPostTypeName = function (typeId) {
            return postTypeNames[typeId] || '';
        };

        // Content formatting - hashtags to links
        self.formatContent = function (content) {
            if (!content) return '';
            // Escape HTML first
            var div = document.createElement('div');
            div.textContent = content;
            var escaped = div.innerHTML;
            // Convert hashtags to clickable links
            return escaped.replace(/#(\w{2,50})/g, '<a href="#" class="text-primary text-decoration-none fw-semibold" onclick="window.__feedVM.filterByHashtag(\'$1\'); return false;">#$1</a>');
        };

        // Tab management
        self.setTab = function (tab) {
            if (self.activeTab() === tab) return;
            self.activeTab(tab);
            self.activeHashtag(null);
            self.posts([]);
            self.nextCursor(null);
            self.hasMore(false);
            self.expandedPostId(null);
            self.loadPosts();
        };

        // Sort mode
        self.setSortMode = function (mode) {
            if (self.sortMode() === mode) return;
            self.sortMode(mode);
            self.posts([]);
            self.nextCursor(null);
            self.hasMore(false);
            self.loadPosts();
        };

        // Time formatting
        self.formatTime = function (dateStr) {
            if (!dateStr) return '';
            var date = new Date(dateStr);
            var now = new Date();
            var diff = Math.floor((now - date) / 1000);

            if (diff < 60) return 'Az once';
            if (diff < 3600) return Math.floor(diff / 60) + ' dk';
            if (diff < 86400) return Math.floor(diff / 3600) + ' sa';
            if (diff < 604800) return Math.floor(diff / 86400) + ' gun';

            return date.toLocaleDateString('tr-TR', { day: 'numeric', month: 'short', year: 'numeric' });
        };

        // ============================================
        // HASHTAGS
        // ============================================

        self.loadTrendingHashtags = function () {
            $.get('/api/feed/hashtags/trending?count=10&hours=48')
                .done(function (data) {
                    self.trendingHashtags(data || []);
                });
        };

        self.filterByHashtag = function (tag) {
            tag = tag.replace(/^#/, '').toLowerCase();
            self.activeHashtag(tag);
            self.posts([]);
            self.nextCursor(null);
            self.hasMore(false);

            self.isLoading(true);
            var params = new URLSearchParams();
            params.append('pageSize', '20');
            if (self.nextCursor()) params.append('lastPostId', self.nextCursor());

            $.get('/api/feed/hashtags/' + encodeURIComponent(tag) + '?' + params.toString())
                .done(function (data) {
                    self.posts(data.posts || []);
                    self.hasMore(data.hasMore || false);
                    self.nextCursor(data.nextCursor || null);
                })
                .fail(function () {
                    toastr.error('Hashtag postlari yuklenemedi');
                })
                .always(function () {
                    self.isLoading(false);
                });
        };

        self.clearHashtagFilter = function () {
            self.activeHashtag(null);
            self.posts([]);
            self.nextCursor(null);
            self.hasMore(false);
            self.loadPosts();
        };

        // ============================================
        // SEARCH
        // ============================================

        self.searchPosts = function () {
            var query = self.searchQuery();
            var typeId = self.searchPostTypeId();

            if (!query && !typeId) return;

            self.activeHashtag(null);
            self.isLoading(true);

            var params = new URLSearchParams();
            if (query) params.append('query', query);
            if (typeId) params.append('postTypeId', typeId);

            $.get('/api/feed/search?' + params.toString())
                .done(function (data) {
                    self.posts(data.posts || []);
                    self.hasMore(data.hasMore || false);
                    self.nextCursor(data.nextCursor || null);
                })
                .fail(function () {
                    toastr.error('Arama basarisiz');
                })
                .always(function () {
                    self.isLoading(false);
                });
        };

        self.clearSearch = function () {
            self.searchQuery('');
            self.searchPostTypeId('');
            self.posts([]);
            self.nextCursor(null);
            self.hasMore(false);
            self.loadPosts();
        };

        // ============================================
        // REPORT
        // ============================================

        self.showReportModal = function (post) {
            self.reportPostId(post.id);
            self.reportReasonId('1');
            self.reportDescription('');
            var modal = new bootstrap.Modal(document.getElementById('reportModal'));
            modal.show();
        };

        self.submitReport = function () {
            var postId = self.reportPostId();
            if (!postId) return;

            $.ajax({
                url: '/api/feed/posts/' + postId + '/report',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({
                    reasonId: parseInt(self.reportReasonId()),
                    description: self.reportDescription() || null
                })
            })
                .done(function () {
                    toastr.success('Sikayet gonderildi');
                    bootstrap.Modal.getInstance(document.getElementById('reportModal')).hide();
                })
                .fail(function (xhr) {
                    var msg = xhr.responseJSON?.message || 'Hata olustu';
                    toastr.error(msg);
                });
        };

        // ============================================
        // LOAD POSTS
        // ============================================

        self.loadPosts = function () {
            if (self.activeHashtag()) {
                self.filterByHashtag(self.activeHashtag());
                return;
            }

            self.isLoading(true);
            var url;
            var tab = self.activeTab();

            if (tab === 'feed') {
                url = '/api/feed';
            } else if (tab === 'discover') {
                url = '/api/feed/discover';
            } else {
                url = '/api/feed/vendor/0';
            }

            var params = new URLSearchParams();
            if (self.nextCursor()) params.append('lastPostId', self.nextCursor());
            params.append('pageSize', '20');

            if (tab !== 'myPosts' && self.sortMode() === 'recommended') {
                params.append('sort', 'recommended');
            }

            if (tab === 'myPosts') {
                self.getCurrentVendorId(function (vendorId) {
                    url = '/api/feed/vendor/' + vendorId;
                    self.fetchPosts(url + '?' + params.toString(), false);
                });
            } else {
                self.fetchPosts(url + '?' + params.toString(), false);
            }
        };

        self.getCurrentVendorId = function (callback) {
            var vendorEl = document.querySelector('[data-vendor-id]');
            if (vendorEl) {
                callback(vendorEl.getAttribute('data-vendor-id'));
                return;
            }
            callback(0);
        };

        self.fetchPosts = function (url, append) {
            if (!append) self.isLoading(true);
            else self.isLoadingMore(true);

            $.get(url)
                .done(function (data) {
                    if (append) {
                        var existing = self.posts();
                        self.posts(existing.concat(data.posts || []));
                    } else {
                        self.posts(data.posts || []);
                    }
                    self.hasMore(data.hasMore || false);
                    self.nextCursor(data.nextCursor || null);
                })
                .fail(function () {
                    toastr.error('Paylasimlar yuklenemedi');
                })
                .always(function () {
                    self.isLoading(false);
                    self.isLoadingMore(false);
                });
        };

        self.loadMore = function () {
            if (!self.hasMore() || self.isLoadingMore()) return;

            // Hashtag mode
            if (self.activeHashtag()) {
                self.isLoadingMore(true);
                var params = new URLSearchParams();
                params.append('lastPostId', self.nextCursor());
                params.append('pageSize', '20');

                $.get('/api/feed/hashtags/' + encodeURIComponent(self.activeHashtag()) + '?' + params.toString())
                    .done(function (data) {
                        var existing = self.posts();
                        self.posts(existing.concat(data.posts || []));
                        self.hasMore(data.hasMore || false);
                        self.nextCursor(data.nextCursor || null);
                    })
                    .always(function () {
                        self.isLoadingMore(false);
                    });
                return;
            }

            var tab = self.activeTab();
            var url;
            var params = new URLSearchParams();
            params.append('lastPostId', self.nextCursor());
            params.append('pageSize', '20');

            if (tab !== 'myPosts' && self.sortMode() === 'recommended') {
                params.append('sort', 'recommended');
            }

            if (tab === 'feed') {
                url = '/api/feed';
            } else if (tab === 'discover') {
                url = '/api/feed/discover';
            } else {
                self.getCurrentVendorId(function (vendorId) {
                    url = '/api/feed/vendor/' + vendorId;
                    self.fetchPosts(url + '?' + params.toString(), true);
                });
                return;
            }

            self.fetchPosts(url + '?' + params.toString(), true);
        };

        // ============================================
        // COMPOSE
        // ============================================

        self.publishPost = function () {
            if (!self.newPost.content()) return;

            var data = {
                postTypeId: parseInt(self.newPost.postTypeId()),
                title: self.newPost.title() || null,
                content: self.newPost.content(),
                productId: self.newPost.productId() ? parseInt(self.newPost.productId()) : null,
                publishImmediately: true
            };

            $.ajax({
                url: '/api/feed/posts',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(data)
            })
                .done(function () {
                    toastr.success('Paylasim yapildi');
                    self.resetCompose();
                    self.posts([]);
                    self.nextCursor(null);
                    self.loadPosts();
                    self.loadTrendingHashtags();
                })
                .fail(function (xhr) {
                    var msg = xhr.responseJSON?.message || 'Hata olustu';
                    toastr.error(msg);
                });
        };

        self.saveDraft = function () {
            if (!self.newPost.content()) return;

            var data = {
                postTypeId: parseInt(self.newPost.postTypeId()),
                title: self.newPost.title() || null,
                content: self.newPost.content(),
                productId: self.newPost.productId() ? parseInt(self.newPost.productId()) : null,
                publishImmediately: false
            };

            $.ajax({
                url: '/api/feed/posts',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(data)
            })
                .done(function () {
                    toastr.success('Taslak kaydedildi');
                    self.resetCompose();
                    if (self.activeTab() === 'myPosts') {
                        self.posts([]);
                        self.nextCursor(null);
                        self.loadPosts();
                    }
                })
                .fail(function (xhr) {
                    var msg = xhr.responseJSON?.message || 'Hata olustu';
                    toastr.error(msg);
                });
        };

        self.resetCompose = function () {
            self.newPost.postTypeId('1');
            self.newPost.title('');
            self.newPost.content('');
            self.newPost.productId(null);
        };

        // Publish draft
        self.publishDraft = function (post) {
            $.ajax({
                url: '/api/feed/posts/' + post.id + '/publish',
                type: 'POST'
            })
                .done(function () {
                    toastr.success('Paylasim yayinlandi');
                    var posts = self.posts();
                    var index = posts.findIndex(function (p) { return p.id === post.id; });
                    if (index !== -1) {
                        posts[index].statusId = 2;
                        posts[index].publishedAt = new Date().toISOString();
                        self.posts(posts.slice());
                    }
                })
                .fail(function (xhr) {
                    var msg = xhr.responseJSON?.message || 'Hata olustu';
                    toastr.error(msg);
                });
        };

        // Delete post
        self.deletePost = function (post) {
            self.postToDelete(post);
            var modal = new bootstrap.Modal(document.getElementById('deleteConfirmModal'));
            modal.show();
        };

        self.confirmDelete = function () {
            var post = self.postToDelete();
            if (!post) return;

            $.ajax({
                url: '/api/feed/posts/' + post.id,
                type: 'DELETE'
            })
                .done(function () {
                    toastr.success('Paylasim silindi');
                    self.posts.remove(function (p) { return p.id === post.id; });
                    bootstrap.Modal.getInstance(document.getElementById('deleteConfirmModal')).hide();
                })
                .fail(function (xhr) {
                    var msg = xhr.responseJSON?.message || 'Hata olustu';
                    toastr.error(msg);
                });
        };

        // Like
        self.toggleLike = function (post) {
            $.ajax({
                url: '/api/feed/posts/' + post.id + '/like',
                type: 'POST'
            })
                .done(function () {
                    var posts = self.posts();
                    var index = posts.findIndex(function (p) { return p.id === post.id; });
                    if (index !== -1) {
                        if (posts[index].isLikedByCurrentUser) {
                            posts[index].isLikedByCurrentUser = false;
                            posts[index].likeCount = Math.max(0, posts[index].likeCount - 1);
                        } else {
                            posts[index].isLikedByCurrentUser = true;
                            posts[index].likeCount++;
                        }
                        self.posts(posts.slice());
                    }
                })
                .fail(function () {
                    toastr.error('Islem basarisiz');
                });
        };

        // Comments
        self.toggleComments = function (post) {
            if (self.expandedPostId() === post.id) {
                self.expandedPostId(null);
                self.currentComments([]);
                return;
            }

            self.expandedPostId(post.id);
            self.newCommentText('');
            self.loadComments(post.id);
        };

        self.loadComments = function (postId) {
            self.commentsLoading(true);
            $.get('/api/feed/posts/' + postId + '/comments')
                .done(function (data) {
                    self.currentComments(data || []);
                })
                .fail(function () {
                    toastr.error('Yorumlar yuklenemedi');
                })
                .always(function () {
                    self.commentsLoading(false);
                });
        };

        self.addComment = function (post) {
            var text = self.newCommentText();
            if (!text || !text.trim()) return;

            $.ajax({
                url: '/api/feed/posts/' + post.id + '/comments',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({ content: text.trim() })
            })
                .done(function () {
                    self.newCommentText('');
                    self.loadComments(post.id);
                    var posts = self.posts();
                    var index = posts.findIndex(function (p) { return p.id === post.id; });
                    if (index !== -1) {
                        posts[index].commentCount++;
                        self.posts(posts.slice());
                    }
                })
                .fail(function (xhr) {
                    var msg = xhr.responseJSON?.message || 'Hata olustu';
                    toastr.error(msg);
                });
        };

        self.deleteComment = function (comment) {
            $.ajax({
                url: '/api/feed/comments/' + comment.id,
                type: 'DELETE'
            })
                .done(function () {
                    toastr.success('Yorum silindi');
                    self.currentComments.remove(function (c) { return c.id === comment.id; });
                    var postId = self.expandedPostId();
                    var posts = self.posts();
                    var index = posts.findIndex(function (p) { return p.id === postId; });
                    if (index !== -1) {
                        posts[index].commentCount = Math.max(0, posts[index].commentCount - 1);
                        self.posts(posts.slice());
                    }
                })
                .fail(function () {
                    toastr.error('Yorum silinemedi');
                });
        };

        // Infinite scroll
        $(window).on('scroll', function () {
            if (self.isLoadingMore() || !self.hasMore()) return;
            var scrollBottom = $(window).scrollTop() + $(window).height();
            var docHeight = $(document).height();
            if (scrollBottom >= docHeight - 200) {
                self.loadMore();
            }
        });

        // Load products for compose (ProductShowcase)
        self.loadProducts = function () {
            $.get('/api/products')
                .done(function (data) {
                    self.products(data.items || data || []);
                })
                .fail(function () {
                    // Products not available - ok
                });
        };

        // Init
        self.loadPosts();
        self.loadProducts();
        self.loadTrendingHashtags();

        // Expose VM for hashtag click handlers in formatted content
        window.__feedVM = self;
    }

    $(function () {
        var app = document.getElementById('feed-app');
        if (app) {
            ko.applyBindings(new FeedViewModel(), app);
        }
    });
})();
