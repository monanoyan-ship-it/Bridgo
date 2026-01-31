namespace Bridgo.DTOs.Warehouse;

/// <summary>
/// Depo listesi ve detay için DTO
/// </summary>
public class WarehouseDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int WarehouseTypeId { get; set; }
    public string? WarehouseTypeName { get; set; }
    public string? Description { get; set; }

    // Lokasyon
    public int? AddressId { get; set; }
    public string? AddressLine { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }

    // Kapasite
    public decimal? TotalCapacity { get; set; }
    public string? CapacityUnit { get; set; }

    // İletişim
    public int? ManagerUserId { get; set; }
    public string? ManagerName { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }

    // Çalışma saatleri
    public string? OperatingHours { get; set; }

    // Durum
    public int Priority { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }

    // İstatistikler
    public int ProductCount { get; set; }
    public int TotalStockQuantity { get; set; }

    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Depo oluşturma için DTO
/// </summary>
public class WarehouseCreateDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int WarehouseTypeId { get; set; } = 1;
    public string? Description { get; set; }

    public int? AddressId { get; set; }

    public decimal? TotalCapacity { get; set; }
    public string? CapacityUnit { get; set; }

    public int? ManagerUserId { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }

    public string? OperatingHours { get; set; }

    public int Priority { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Depo güncelleme için DTO
/// </summary>
public class WarehouseUpdateDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int WarehouseTypeId { get; set; }
    public string? Description { get; set; }

    public int? AddressId { get; set; }

    public decimal? TotalCapacity { get; set; }
    public string? CapacityUnit { get; set; }

    public int? ManagerUserId { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }

    public string? OperatingHours { get; set; }

    public int Priority { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Dropdown listesi için basit DTO
/// </summary>
public class WarehouseSelectDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}
