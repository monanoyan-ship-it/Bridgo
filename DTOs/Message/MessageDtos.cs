namespace Bridgo.DTOs.Message;

// ============================================
// LIST DTOs
// ============================================

public class MessageThreadListDto
{
    public int Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public int ThreadType { get; set; }
    public string ThreadTypeName { get; set; } = string.Empty;
    public int ParticipantVendorId { get; set; }
    public string ParticipantVendorName { get; set; } = string.Empty;
    public string? LastMessage { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public int UnreadCount { get; set; }
    public bool IsClosed { get; set; }
    public int? ReferenceType { get; set; }
    public int? ReferenceId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class MessageListDto
{
    public int Id { get; set; }
    public int ThreadId { get; set; }
    public int SenderVendorId { get; set; }
    public string SenderVendorName { get; set; } = string.Empty;
    public int SenderUserId { get; set; }
    public string SenderUserName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<MessageAttachmentDto> Attachments { get; set; } = new();
    public bool IsMine { get; set; }
}

public class MessageAttachmentDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
}

// ============================================
// DETAIL DTOs
// ============================================

public class MessageThreadDetailDto
{
    public int Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public int ThreadType { get; set; }
    public string ThreadTypeName { get; set; } = string.Empty;
    public int InitiatorVendorId { get; set; }
    public string InitiatorVendorName { get; set; } = string.Empty;
    public int RecipientVendorId { get; set; }
    public string RecipientVendorName { get; set; } = string.Empty;
    public int? ReferenceType { get; set; }
    public int? ReferenceId { get; set; }
    public bool IsClosed { get; set; }
    public DateTime? ClosedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<MessageListDto> Messages { get; set; } = new();
}

// ============================================
// CREATE/UPDATE DTOs
// ============================================

public class CreateThreadDto
{
    public int RecipientVendorId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int ThreadType { get; set; } = 1; // General
    public int? ReferenceType { get; set; }
    public int? ReferenceId { get; set; }
}

public class SendMessageDto
{
    public int ThreadId { get; set; }
    public string Content { get; set; } = string.Empty;
}

public class MarkAsReadDto
{
    public int ThreadId { get; set; }
    public int? MessageId { get; set; } // null ise tum mesajlar okundu
}

// ============================================
// FILTER DTOs
// ============================================

public class MessageThreadFilterDto
{
    public string? Search { get; set; }
    public int? ThreadType { get; set; }
    public bool? Unread { get; set; }
    public bool? IsClosed { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

// ============================================
// SEARCH RESULT DTOs
// ============================================

public class MessageThreadSearchResultDto
{
    public List<MessageThreadListDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

// ============================================
// STATS DTOs
// ============================================

public class MessageStatsDto
{
    public int TotalThreads { get; set; }
    public int UnreadThreads { get; set; }
    public int TotalMessages { get; set; }
    public int UnreadMessages { get; set; }
}
