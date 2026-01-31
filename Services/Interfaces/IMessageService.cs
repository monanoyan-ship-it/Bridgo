using Bridgo.DTOs.Message;

namespace Bridgo.Services.Interfaces;

public interface IMessageService
{
    // Thread operations
    Task<MessageThreadSearchResultDto> GetThreadsAsync(int vendorId, MessageThreadFilterDto filter);
    Task<MessageThreadDetailDto?> GetThreadDetailAsync(int vendorId, int threadId);
    Task<ServiceResult<MessageThreadDetailDto>> CreateThreadAsync(int vendorId, int userId, CreateThreadDto dto);
    Task<ServiceResult> CloseThreadAsync(int vendorId, int threadId);
    Task<ServiceResult> ReopenThreadAsync(int vendorId, int threadId);

    // Message operations
    Task<ServiceResult<MessageListDto>> SendMessageAsync(int vendorId, int userId, SendMessageDto dto);
    Task<ServiceResult> MarkAsReadAsync(int vendorId, MarkAsReadDto dto);
    Task<ServiceResult> DeleteMessageAsync(int vendorId, int messageId);

    // Stats
    Task<MessageStatsDto> GetStatsAsync(int vendorId);
    Task<int> GetUnreadCountAsync(int vendorId);

    // Reference lookups
    Task<MessageThreadDetailDto?> GetThreadByReferenceAsync(int vendorId, int referenceType, int referenceId);
}
