using MongoDB.Driver;

namespace MongoSharpen.Builders;

public interface ITransaction : IDisposable
{
    Task CommitAsync(CancellationToken cancellation = default);
}

internal sealed class Transaction : ITransaction
{
    private readonly DbContext _context;

    public Transaction(DbContext context, ClientSessionOptions? options = null)
    {
        _context = context;
        _context.Session = _context.Client.StartSession(options);

        var inTransaction = _context.Session.IsInTransaction;
        if (!inTransaction) _context.Session.StartTransaction();
    }

    public void Dispose()
    {
        _context.Session?.Dispose();
        _context.Session = null;
    }

    public async Task CommitAsync(CancellationToken cancellation = default)
    {
        if (_context.Session == null || _context.Session.IsInTransaction == false)
            throw new InvalidOperationException("No transaction started");

        await _context.Session.CommitTransactionAsync(cancellation);

        // to be able to start a new transaction with the same db context
        Dispose();
    }
}