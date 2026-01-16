using CommonArchitecture.Core.Interfaces;
using CommonArchitecture.Infrastructure.Persistence;
using CommonArchitecture.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace CommonArchitecture.Infrastructure.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    private IProductRepository? _products;
    private IUserRepository? _users;
    private IRoleRepository? _roles;
    private IAuthRepository? _auth;
    private IMenuRepository? _menus;
    private IRoleMenuRepository? _roleMenus;
    private IRefreshTokenRepository? _refreshTokens;
    private ICategoryRepository? _categories;
    private IInventoryTransactionRepository? _inventoryTransactions;
    private IOrderRepository? _orders;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IProductRepository Products => _products ??= new ProductRepository(_context);
    public IUserRepository Users => _users ??= new UserRepository(_context);
    public IRoleRepository Roles => _roles ??= new RoleRepository(_context);
    public IAuthRepository Auth => _auth ??= new AuthRepository(_context);
    public IMenuRepository Menus => _menus ??= new MenuRepository(_context);
    public IRoleMenuRepository RoleMenus => _roleMenus ??= new RoleMenuRepository(_context);
    public IRefreshTokenRepository RefreshTokens => _refreshTokens ??= new RefreshTokenRepository(_context);
    public ICategoryRepository Categories => _categories ??= new CategoryRepository(_context);
    public IInventoryTransactionRepository InventoryTransactions => _inventoryTransactions ??= new InventoryTransactionRepository(_context);
    public IOrderRepository Orders => _orders ??= new OrderRepository(_context);

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null) return;
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null) return;
        await _transaction.CommitAsync(cancellationToken);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null) return;
        await _transaction.RollbackAsync(cancellationToken);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public void Dispose()
    {
        _context.Dispose();
        _transaction?.Dispose();
        GC.SuppressFinalize(this);
    }
}

