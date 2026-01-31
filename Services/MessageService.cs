using Microsoft.EntityFrameworkCore;
using Bridgo.Data;
using Bridgo.DTOs.Message;
using Bridgo.Models.Entities;
using Bridgo.Services.Interfaces;

namespace Bridgo.Services;

public class MessageService : IMessageService
{
    private readonly ApplicationDbContext _context;

    public MessageService(ApplicationDbContext context)
    {
        _context = context;
    }

    // ============================================
    // THREAD OPERATIONS
    // ============================================

    public async Task<MessageThreadSearchResultDto> GetThreadsAsync(int vendorId, MessageThreadFilterDto filter)
    {
        var query = _context.MessageThreads
            .Include(t => t.InitiatorVendor)
            .Include(t => t.RecipientVendor)
            .Include(t => t.Messages)
            .Where(t => t.InitiatorVendorId == vendorId || t.RecipientVendorId == vendorId)
            .AsQueryable();

        // Filters
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.ToLower();
            query = query.Where(t => t.Subject.ToLower().Contains(search));
        }

        if (filter.ThreadType.HasValue)
            query = query.Where(t => t.ThreadType == filter.ThreadType.Value);

        if (filter.IsClosed.HasValue)
            query = query.Where(t => t.IsClosed == filter.IsClosed.Value);

        // Order by last message
        query = query.OrderByDescending(t => t.LastMessageAt ?? t.CreatedAt);

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);

        var threads = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var items = threads.Select(t => {
            var typeItem = MessageThreadTypes.GetById(t.ThreadType);
            var lastMessage = t.Messages?.OrderByDescending(m => m.CreatedAt).FirstOrDefault();
            return new MessageThreadListDto
            {
                Id = t.Id,
                Subject = t.Subject,
                ThreadType = t.ThreadType,
                ThreadTypeName = typeItem?.SystemName ?? "",
                ParticipantVendorId = t.InitiatorVendorId == vendorId ? t.RecipientVendorId : t.InitiatorVendorId,
                ParticipantVendorName = t.InitiatorVendorId == vendorId
                    ? (t.RecipientVendor?.CompanyName ?? "")
                    : (t.InitiatorVendor?.CompanyName ?? ""),
                LastMessage = lastMessage != null
                    ? lastMessage.Content.Substring(0, Math.Min(100, lastMessage.Content.Length))
                    : null,
                LastMessageAt = t.LastMessageAt,
                UnreadCount = t.Messages?.Count(m => !m.IsRead && m.SenderVendorId != vendorId) ?? 0,
                IsClosed = t.IsClosed,
                ReferenceType = t.ReferenceType,
                ReferenceId = t.ReferenceId,
                CreatedAt = t.CreatedAt
            };
        }).ToList();

        // Filter unread after mapping if needed
        if (filter.Unread == true)
        {
            items = items.Where(i => i.UnreadCount > 0).ToList();
        }

        return new MessageThreadSearchResultDto
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalPages = totalPages
        };
    }

    public async Task<MessageThreadDetailDto?> GetThreadDetailAsync(int vendorId, int threadId)
    {
        var thread = await _context.MessageThreads
            .Include(t => t.InitiatorVendor)
            .Include(t => t.RecipientVendor)
            .Include(t => t.Messages!.Where(m =>
                !(m.IsDeletedBySender && m.SenderVendorId == vendorId) &&
                !(m.IsDeletedByRecipient && m.SenderVendorId != vendorId)))
                .ThenInclude(m => m.Attachments)
            .Include(t => t.Messages!)
                .ThenInclude(m => m.SenderVendor)
            .Include(t => t.Messages!)
                .ThenInclude(m => m.SenderUser)
            .FirstOrDefaultAsync(t => t.Id == threadId &&
                (t.InitiatorVendorId == vendorId || t.RecipientVendorId == vendorId));

        if (thread == null) return null;

        return new MessageThreadDetailDto
        {
            Id = thread.Id,
            Subject = thread.Subject,
            ThreadType = thread.ThreadType,
            ThreadTypeName = MessageThreadTypes.GetById(thread.ThreadType)?.SystemName ?? "",
            InitiatorVendorId = thread.InitiatorVendorId,
            InitiatorVendorName = thread.InitiatorVendor?.CompanyName ?? "",
            RecipientVendorId = thread.RecipientVendorId,
            RecipientVendorName = thread.RecipientVendor?.CompanyName ?? "",
            ReferenceType = thread.ReferenceType,
            ReferenceId = thread.ReferenceId,
            IsClosed = thread.IsClosed,
            ClosedAt = thread.ClosedAt,
            CreatedAt = thread.CreatedAt,
            Messages = thread.Messages?.OrderBy(m => m.CreatedAt).Select(m => new MessageListDto
            {
                Id = m.Id,
                ThreadId = m.ThreadId,
                SenderVendorId = m.SenderVendorId,
                SenderVendorName = m.SenderVendor?.CompanyName ?? "",
                SenderUserId = m.SenderUserId,
                SenderUserName = m.SenderUser != null ? $"{m.SenderUser.FirstName} {m.SenderUser.LastName}" : "",
                Content = m.Content,
                IsRead = m.IsRead,
                ReadAt = m.ReadAt,
                CreatedAt = m.CreatedAt,
                IsMine = m.SenderVendorId == vendorId,
                Attachments = m.Attachments?.Select(a => new MessageAttachmentDto
                {
                    Id = a.Id,
                    FileName = a.FileName,
                    FilePath = a.FilePath,
                    ContentType = a.ContentType,
                    FileSize = a.FileSize
                }).ToList() ?? new List<MessageAttachmentDto>()
            }).ToList() ?? new List<MessageListDto>()
        };
    }

    public async Task<ServiceResult<MessageThreadDetailDto>> CreateThreadAsync(int vendorId, int userId, CreateThreadDto dto)
    {
        // Validate recipient
        var recipientVendor = await _context.Vendors.FindAsync(dto.RecipientVendorId);
        if (recipientVendor == null)
            return ServiceResult<MessageThreadDetailDto>.Fail("Alici firma bulunamadi.");

        if (dto.RecipientVendorId == vendorId)
            return ServiceResult<MessageThreadDetailDto>.Fail("Kendinize mesaj gonderemezsiniz.");

        // Check for existing thread with same reference
        if (dto.ReferenceType.HasValue && dto.ReferenceId.HasValue)
        {
            var existingThread = await _context.MessageThreads
                .FirstOrDefaultAsync(t =>
                    t.ReferenceType == dto.ReferenceType &&
                    t.ReferenceId == dto.ReferenceId &&
                    ((t.InitiatorVendorId == vendorId && t.RecipientVendorId == dto.RecipientVendorId) ||
                     (t.InitiatorVendorId == dto.RecipientVendorId && t.RecipientVendorId == vendorId)));

            if (existingThread != null)
            {
                // Add message to existing thread
                var newMessage = new Message
                {
                    ThreadId = existingThread.Id,
                    SenderVendorId = vendorId,
                    SenderUserId = userId,
                    Content = dto.Message
                };
                _context.Messages.Add(newMessage);
                existingThread.LastMessageAt = DateTime.UtcNow;
                if (existingThread.IsClosed)
                {
                    existingThread.IsClosed = false;
                    existingThread.ClosedAt = null;
                }
                await _context.SaveChangesAsync();

                var result = await GetThreadDetailAsync(vendorId, existingThread.Id);
                return ServiceResult<MessageThreadDetailDto>.Ok(result!, "Mesaj mevcut konuya eklendi.");
            }
        }

        // Create new thread
        var thread = new MessageThread
        {
            Subject = dto.Subject,
            InitiatorVendorId = vendorId,
            InitiatorUserId = userId,
            RecipientVendorId = dto.RecipientVendorId,
            ThreadType = dto.ThreadType,
            ReferenceType = dto.ReferenceType,
            ReferenceId = dto.ReferenceId,
            LastMessageAt = DateTime.UtcNow
        };
        _context.MessageThreads.Add(thread);
        await _context.SaveChangesAsync();

        // Add first message
        var message = new Message
        {
            ThreadId = thread.Id,
            SenderVendorId = vendorId,
            SenderUserId = userId,
            Content = dto.Message
        };
        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        var threadDetail = await GetThreadDetailAsync(vendorId, thread.Id);
        return ServiceResult<MessageThreadDetailDto>.Ok(threadDetail!, "Mesaj konusu olusturuldu.");
    }

    public async Task<ServiceResult> CloseThreadAsync(int vendorId, int threadId)
    {
        var thread = await _context.MessageThreads
            .FirstOrDefaultAsync(t => t.Id == threadId &&
                (t.InitiatorVendorId == vendorId || t.RecipientVendorId == vendorId));

        if (thread == null)
            return ServiceResult.Fail("Konu bulunamadi.");

        if (thread.IsClosed)
            return ServiceResult.Fail("Konu zaten kapali.");

        thread.IsClosed = true;
        thread.ClosedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ServiceResult.Ok("Konu kapatildi.");
    }

    public async Task<ServiceResult> ReopenThreadAsync(int vendorId, int threadId)
    {
        var thread = await _context.MessageThreads
            .FirstOrDefaultAsync(t => t.Id == threadId &&
                (t.InitiatorVendorId == vendorId || t.RecipientVendorId == vendorId));

        if (thread == null)
            return ServiceResult.Fail("Konu bulunamadi.");

        if (!thread.IsClosed)
            return ServiceResult.Fail("Konu zaten acik.");

        thread.IsClosed = false;
        thread.ClosedAt = null;
        await _context.SaveChangesAsync();

        return ServiceResult.Ok("Konu yeniden acildi.");
    }

    // ============================================
    // MESSAGE OPERATIONS
    // ============================================

    public async Task<ServiceResult<MessageListDto>> SendMessageAsync(int vendorId, int userId, SendMessageDto dto)
    {
        var thread = await _context.MessageThreads
            .FirstOrDefaultAsync(t => t.Id == dto.ThreadId &&
                (t.InitiatorVendorId == vendorId || t.RecipientVendorId == vendorId));

        if (thread == null)
            return ServiceResult<MessageListDto>.Fail("Konu bulunamadi.");

        if (thread.IsClosed)
            return ServiceResult<MessageListDto>.Fail("Kapali konuya mesaj gonderilemez.");

        var message = new Message
        {
            ThreadId = dto.ThreadId,
            SenderVendorId = vendorId,
            SenderUserId = userId,
            Content = dto.Content
        };
        _context.Messages.Add(message);

        thread.LastMessageAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Load full message data
        var sender = await _context.Users.FindAsync(userId);
        var senderVendor = await _context.Vendors.FindAsync(vendorId);

        var result = new MessageListDto
        {
            Id = message.Id,
            ThreadId = message.ThreadId,
            SenderVendorId = message.SenderVendorId,
            SenderVendorName = senderVendor?.CompanyName ?? "",
            SenderUserId = message.SenderUserId,
            SenderUserName = sender != null ? $"{sender.FirstName} {sender.LastName}" : "",
            Content = message.Content,
            IsRead = false,
            CreatedAt = message.CreatedAt,
            IsMine = true,
            Attachments = new List<MessageAttachmentDto>()
        };

        return ServiceResult<MessageListDto>.Ok(result, "Mesaj gonderildi.");
    }

    public async Task<ServiceResult> MarkAsReadAsync(int vendorId, MarkAsReadDto dto)
    {
        var thread = await _context.MessageThreads
            .FirstOrDefaultAsync(t => t.Id == dto.ThreadId &&
                (t.InitiatorVendorId == vendorId || t.RecipientVendorId == vendorId));

        if (thread == null)
            return ServiceResult.Fail("Konu bulunamadi.");

        // Mark messages as read (only messages from other vendor)
        var query = _context.Messages
            .Where(m => m.ThreadId == dto.ThreadId && m.SenderVendorId != vendorId && !m.IsRead);

        if (dto.MessageId.HasValue)
            query = query.Where(m => m.Id == dto.MessageId.Value);

        var messages = await query.ToListAsync();
        foreach (var message in messages)
        {
            message.IsRead = true;
            message.ReadAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return ServiceResult.Ok($"{messages.Count} mesaj okundu olarak isaretlendi.");
    }

    public async Task<ServiceResult> DeleteMessageAsync(int vendorId, int messageId)
    {
        var message = await _context.Messages
            .Include(m => m.Thread)
            .FirstOrDefaultAsync(m => m.Id == messageId);

        if (message == null)
            return ServiceResult.Fail("Mesaj bulunamadi.");

        // Check if user has access to this thread
        if (message.Thread == null ||
            (message.Thread.InitiatorVendorId != vendorId && message.Thread.RecipientVendorId != vendorId))
            return ServiceResult.Fail("Bu mesaja erisim yetkiniz yok.");

        // Soft delete for the vendor
        if (message.SenderVendorId == vendorId)
            message.IsDeletedBySender = true;
        else
            message.IsDeletedByRecipient = true;

        await _context.SaveChangesAsync();

        return ServiceResult.Ok("Mesaj silindi.");
    }

    // ============================================
    // STATS
    // ============================================

    public async Task<MessageStatsDto> GetStatsAsync(int vendorId)
    {
        var threads = await _context.MessageThreads
            .Include(t => t.Messages)
            .Where(t => t.InitiatorVendorId == vendorId || t.RecipientVendorId == vendorId)
            .ToListAsync();

        var unreadMessages = threads.Sum(t =>
            t.Messages?.Count(m => !m.IsRead && m.SenderVendorId != vendorId) ?? 0);

        var unreadThreads = threads.Count(t =>
            t.Messages != null && t.Messages.Any(m => !m.IsRead && m.SenderVendorId != vendorId));

        return new MessageStatsDto
        {
            TotalThreads = threads.Count,
            UnreadThreads = unreadThreads,
            TotalMessages = threads.Sum(t => t.Messages?.Count ?? 0),
            UnreadMessages = unreadMessages
        };
    }

    public async Task<int> GetUnreadCountAsync(int vendorId)
    {
        return await _context.Messages
            .Include(m => m.Thread)
            .Where(m => m.Thread != null &&
                (m.Thread.InitiatorVendorId == vendorId || m.Thread.RecipientVendorId == vendorId) &&
                m.SenderVendorId != vendorId &&
                !m.IsRead)
            .CountAsync();
    }

    // ============================================
    // REFERENCE LOOKUPS
    // ============================================

    public async Task<MessageThreadDetailDto?> GetThreadByReferenceAsync(int vendorId, int referenceType, int referenceId)
    {
        var thread = await _context.MessageThreads
            .FirstOrDefaultAsync(t =>
                t.ReferenceType == referenceType &&
                t.ReferenceId == referenceId &&
                (t.InitiatorVendorId == vendorId || t.RecipientVendorId == vendorId));

        if (thread == null) return null;

        return await GetThreadDetailAsync(vendorId, thread.Id);
    }
}
