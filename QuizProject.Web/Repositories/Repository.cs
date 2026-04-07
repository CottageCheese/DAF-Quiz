using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using QuizProject.Web.Data;

namespace QuizProject.Web.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly ApplicationDbContext _db;
    private readonly DbSet<T> _set;

    public Repository(ApplicationDbContext db)
    {
        _db = db;
        _set = db.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id) => await _set.FindAsync(id);

    public async Task<IEnumerable<T>> GetAllAsync() => await _set.ToListAsync();

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate) =>
        await _set.Where(predicate).ToListAsync();

    public IQueryable<T> Query() => _set;

    public async Task AddAsync(T entity) => await _set.AddAsync(entity);

    public async Task AddRangeAsync(IEnumerable<T> entities) => await _set.AddRangeAsync(entities);

    public void Update(T entity) => _set.Update(entity);

    public void Remove(T entity) => _set.Remove(entity);

    public Task<int> SaveChangesAsync() => _db.SaveChangesAsync();
}
