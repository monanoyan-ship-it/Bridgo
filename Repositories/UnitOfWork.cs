using Microsoft.EntityFrameworkCore.Storage;
using Bridgo.Data;
using Bridgo.Models.Entities;

namespace Bridgo.Repositories;

/// <summary>
/// Unit of Work Implementation
/// Transaction yonetimi ve repository'lerin merkezi erisimi
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    private IRepository<Branch>? _branches;
    private IRepository<Vendor>? _vendors;
    private IRepository<Address>? _addresses;
    private IRepository<VendorTeamMember>? _vendorTeamMembers;
    private IRepository<VendorCapabilityMapping>? _vendorCapabilityMappings;
    private IRepository<Language>? _languages;
    private IRepository<CompanyRole>? _companyRoles;
    private IRepository<CompanyRoleUserMapping>? _companyRoleUserMappings;
    private IRepository<Product>? _products;
    private IRepository<ProductCategory>? _productCategories;
    private IRepository<ProductImage>? _productImages;
    private IRepository<Country>? _countries;
    private IRepository<State>? _states;
    private IRepository<Warehouse>? _warehouses;
    private IRepository<ProductWarehouseStock>? _productWarehouseStocks;
    private IRepository<CategoryRequest>? _categoryRequests;
    private IRepository<RefreshToken>? _refreshTokens;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IRepository<Branch> Branches =>
        _branches ??= new Repository<Branch>(_context);

    public IRepository<Vendor> Vendors =>
        _vendors ??= new Repository<Vendor>(_context);

    public IRepository<Address> Addresses =>
        _addresses ??= new Repository<Address>(_context);

    public IRepository<VendorTeamMember> VendorTeamMembers =>
        _vendorTeamMembers ??= new Repository<VendorTeamMember>(_context);

    public IRepository<VendorCapabilityMapping> VendorCapabilityMappings =>
        _vendorCapabilityMappings ??= new Repository<VendorCapabilityMapping>(_context);

    public IRepository<Language> Languages =>
        _languages ??= new Repository<Language>(_context);

    public IRepository<CompanyRole> CompanyRoles =>
        _companyRoles ??= new Repository<CompanyRole>(_context);

    public IRepository<CompanyRoleUserMapping> CompanyRoleUserMappings =>
        _companyRoleUserMappings ??= new Repository<CompanyRoleUserMapping>(_context);

    public IRepository<Product> Products =>
        _products ??= new Repository<Product>(_context);

    public IRepository<ProductCategory> ProductCategories =>
        _productCategories ??= new Repository<ProductCategory>(_context);

    public IRepository<ProductImage> ProductImages =>
        _productImages ??= new Repository<ProductImage>(_context);

    public IRepository<Country> Countries =>
        _countries ??= new Repository<Country>(_context);

    public IRepository<State> States =>
        _states ??= new Repository<State>(_context);

    public IRepository<Warehouse> Warehouses =>
        _warehouses ??= new Repository<Warehouse>(_context);

    public IRepository<ProductWarehouseStock> ProductWarehouseStocks =>
        _productWarehouseStocks ??= new Repository<ProductWarehouseStock>(_context);

    public IRepository<CategoryRequest> CategoryRequests =>
        _categoryRequests ??= new Repository<CategoryRequest>(_context);

    public IRepository<RefreshToken> RefreshTokens =>
        _refreshTokens ??= new Repository<RefreshToken>(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
