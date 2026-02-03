using System.Linq.Expressions;
using Bridgo.Models.Entities;

namespace Bridgo.Repositories;

/// <summary>
/// Generic Repository Interface
/// Controller'larda dogrudan DbContext KULLANILMAZ
/// </summary>
public interface IRepository<T> where T : BaseEntity
{
    // Query
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);

    // Query with includes
    IQueryable<T> Query();
    IQueryable<T> QueryNoTracking();

    // Commands
    Task<T> AddAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);
    void Update(T entity);
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);

    // Soft delete
    Task SoftDeleteAsync(int id);
}

/// <summary>
/// Unit of Work - Transaction yonetimi
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IRepository<Branch> Branches { get; }
    IRepository<Vendor> Vendors { get; }
    IRepository<Address> Addresses { get; }
    IRepository<VendorTeamMember> VendorTeamMembers { get; }
    IRepository<VendorCapabilityMapping> VendorCapabilityMappings { get; }
    IRepository<Language> Languages { get; }
    IRepository<CompanyRole> CompanyRoles { get; }
    IRepository<CompanyRoleUserMapping> CompanyRoleUserMappings { get; }
    IRepository<Product> Products { get; }
    IRepository<ProductCategory> ProductCategories { get; }
    IRepository<ProductImage> ProductImages { get; }
    IRepository<Country> Countries { get; }
    IRepository<State> States { get; }
    IRepository<Warehouse> Warehouses { get; }
    IRepository<ProductWarehouseStock> ProductWarehouseStocks { get; }
    IRepository<CategoryRequest> CategoryRequests { get; }
    IRepository<RefreshToken> RefreshTokens { get; }

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
}
