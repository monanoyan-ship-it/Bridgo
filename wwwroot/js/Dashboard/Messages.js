// Dashboard/Messages.js - Mesajlasma Sistemi
(function () {
    'use strict';

    function MessagesViewModel() {
        var self = this;

        // State
        self.isLoading = ko.observable(false);
        self.isLoadingThread = ko.observable(false);
        self.isSending = ko.observable(false);
        self.isCreating = ko.observable(false);
        self.threads = ko.observableArray([]);
        self.selectedThread = ko.observable(null);
        self.threadDetail = ko.observable(null);
        self.stats = ko.observable({});
        self.vendors = ko.observableArray([]);

        // Filter
        self.filter = {
            search: ko.observable('')
        };

        // New message
        self.newMessage = ko.observable('');

        // New thread form
        self.newThread = {
            recipientVendorId: ko.observable(''),
            subject: ko.observable(''),
            message: ko.observable('')
        };

        // Modal
        self.newThreadModal = null;

        // Computed
        self.canSend = ko.computed(function () {
            return self.newMessage().trim().length > 0 && self.selectedThread();
        });

        self.canCreateThread = ko.computed(function () {
            return self.newThread.recipientVendorId() &&
                self.newThread.subject().trim() &&
                self.newThread.message().trim();
        });

        // Filter subscription
        var filterTimeout;
        self.filter.search.subscribe(function () {
            clearTimeout(filterTimeout);
            filterTimeout = setTimeout(function () {
                self.loadThreads();
            }, 300);
        });

        // Load threads
        self.loadThreads = function () {
            self.isLoading(true);

            var params = { pageSize: 50 };
            if (self.filter.search()) params.search = self.filter.search();

            $.ajax({
                url: '/api/messages/threads',
                data: params,
                success: function (result) {
                    self.threads(result.items || []);
                },
                error: function () {
                    toastr.error('Mesajlar yuklenirken hata olustu.');
                },
                complete: function () {
                    self.isLoading(false);
                }
            });
        };

        // Load stats
        self.loadStats = function () {
            $.ajax({
                url: '/api/messages/stats',
                success: function (result) {
                    self.stats(result || {});
                }
            });
        };

        // Load vendors (for new message)
        self.loadVendors = function () {
            $.ajax({
                url: '/api/suppliers/list',
                success: function (result) {
                    self.vendors(result || []);
                }
            });
        };

        // Select thread
        self.selectThread = function (thread) {
            self.selectedThread(thread);
            self.loadThreadDetail(thread.id);
        };

        // Load thread detail
        self.loadThreadDetail = function (threadId) {
            self.isLoadingThread(true);

            $.ajax({
                url: '/api/messages/threads/' + threadId,
                success: function (result) {
                    self.threadDetail(result);
                    self.markAsRead(threadId);
                    setTimeout(self.scrollToBottom, 100);
                },
                error: function () {
                    toastr.error('Konu yuklenirken hata olustu.');
                },
                complete: function () {
                    self.isLoadingThread(false);
                }
            });
        };

        // Mark as read
        self.markAsRead = function (threadId) {
            $.ajax({
                url: '/api/messages/read',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({ threadId: threadId }),
                success: function () {
                    // Update thread list
                    var thread = self.threads().find(function (t) { return t.id === threadId; });
                    if (thread) {
                        thread.unreadCount = 0;
                        self.threads.valueHasMutated();
                    }
                    self.loadStats();
                }
            });
        };

        // Send message
        self.sendMessage = function () {
            if (!self.canSend()) return;

            self.isSending(true);

            $.ajax({
                url: '/api/messages/send',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({
                    threadId: self.selectedThread().id,
                    content: self.newMessage()
                }),
                success: function (result) {
                    // Add message to thread
                    var messages = self.threadDetail()?.messages || [];
                    messages.push(result);
                    self.threadDetail().messages = messages;
                    self.threadDetail.valueHasMutated();

                    // Clear input
                    self.newMessage('');
                    self.scrollToBottom();

                    // Update thread list
                    self.loadThreads();
                },
                error: function (xhr) {
                    var msg = xhr.responseJSON?.message || 'Mesaj gonderilemedi.';
                    toastr.error(msg);
                },
                complete: function () {
                    self.isSending(false);
                }
            });
        };

        // Show new thread modal
        self.showNewThread = function () {
            self.newThread.recipientVendorId('');
            self.newThread.subject('');
            self.newThread.message('');
            self.newThreadModal.show();
        };

        // Create thread
        self.createThread = function () {
            if (!self.canCreateThread()) return;

            self.isCreating(true);

            $.ajax({
                url: '/api/messages/threads',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({
                    recipientVendorId: parseInt(self.newThread.recipientVendorId()),
                    subject: self.newThread.subject(),
                    message: self.newThread.message()
                }),
                success: function (result) {
                    self.newThreadModal.hide();
                    toastr.success('Mesaj gonderildi.');
                    self.loadThreads();
                    self.loadStats();
                    // Select new thread
                    self.selectedThread({ id: result.id });
                    self.loadThreadDetail(result.id);
                },
                error: function (xhr) {
                    var msg = xhr.responseJSON?.message || 'Mesaj gonderilemedi.';
                    toastr.error(msg);
                },
                complete: function () {
                    self.isCreating(false);
                }
            });
        };

        // Close thread
        self.closeThread = function () {
            if (!self.selectedThread()) return;

            $.ajax({
                url: '/api/messages/threads/' + self.selectedThread().id + '/close',
                method: 'POST',
                success: function () {
                    toastr.success('Konu kapatildi.');
                    self.loadThreadDetail(self.selectedThread().id);
                    self.loadThreads();
                },
                error: function (xhr) {
                    var msg = xhr.responseJSON?.message || 'Konu kapatilamadi.';
                    toastr.error(msg);
                }
            });
        };

        // Reopen thread
        self.reopenThread = function () {
            if (!self.selectedThread()) return;

            $.ajax({
                url: '/api/messages/threads/' + self.selectedThread().id + '/reopen',
                method: 'POST',
                success: function () {
                    toastr.success('Konu yeniden acildi.');
                    self.loadThreadDetail(self.selectedThread().id);
                    self.loadThreads();
                },
                error: function (xhr) {
                    var msg = xhr.responseJSON?.message || 'Konu acilamadi.';
                    toastr.error(msg);
                }
            });
        };

        // Get participant name
        self.getParticipantName = function () {
            var detail = self.threadDetail();
            if (!detail) return '';
            // Current vendor is not shown, show the other one
            // We need to determine which one is current user's vendor
            // For now, show both
            return detail.initiatorVendorName + ' - ' + detail.recipientVendorName;
        };

        // Scroll to bottom
        self.scrollToBottom = function () {
            var container = document.getElementById('messagesContainer');
            if (container) {
                container.scrollTop = container.scrollHeight;
            }
        };

        // Helper functions
        self.formatDate = function (dateStr) {
            if (!dateStr) return '';
            var date = new Date(dateStr);
            var now = new Date();
            var isToday = date.toDateString() === now.toDateString();
            if (isToday) {
                return date.toLocaleTimeString('tr-TR', { hour: '2-digit', minute: '2-digit' });
            }
            return date.toLocaleDateString('tr-TR');
        };

        self.formatDateTime = function (dateStr) {
            if (!dateStr) return '';
            return new Date(dateStr).toLocaleString('tr-TR');
        };

        // Make helpers global
        window.formatDate = self.formatDate;
        window.formatDateTime = self.formatDateTime;

        // Init
        self.init = function () {
            var newThreadEl = document.getElementById('newThreadModal');
            if (newThreadEl) self.newThreadModal = new bootstrap.Modal(newThreadEl);

            self.loadStats();
            self.loadThreads();
            self.loadVendors();
        };

        self.init();
    }

    // Apply bindings
    $(function () {
        ko.applyBindings(new MessagesViewModel(), document.getElementById('messages-app'));
    });
})();
