using Getir.Application.Common;
using Getir.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace Getir.Application.Common;

/// <summary>
/// Transaction gerektiren servisler için base interface
/// </summary>
public interface ITransactionalService
{
    /// <summary>
    /// Transaction içinde çalıştırılacak işlemi execute eder
    /// </summary>
    /// <typeparam name="T">Dönüş tipi</typeparam>
    /// <param name="operation">Çalıştırılacak işlem</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem sonucu</returns>
    Task<Result<T>> ExecuteInTransactionAsync<T>(
        Func<Task<Result<T>>> operation,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Transaction içinde çalıştırılacak işlemi execute eder (void return)
    /// </summary>
    /// <param name="operation">Çalıştırılacak işlem</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem sonucu</returns>
    Task<Result> ExecuteInTransactionAsync(
        Func<Task<Result>> operation,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Transaction gerektiren servisler için base class
/// </summary>
public abstract class TransactionalService : ITransactionalService
{
    protected readonly IUnitOfWork _unitOfWork;
    protected readonly ILogger _logger;
    /// <summary>
    /// TransactionalService constructor
    /// </summary>
    /// <param name="unitOfWork">Unit of work instance</param>
    /// <param name="logger">Logger instance</param>
    protected TransactionalService(IUnitOfWork unitOfWork, ILogger logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Transaction içinde çalıştırılacak işlemi execute eder
    /// </summary>
    /// <typeparam name="T">Dönüş tipi</typeparam>
    /// <param name="operation">Çalıştırılacak işlem</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem sonucu</returns>
    public async Task<Result<T>> ExecuteInTransactionAsync<T>(
        Func<Task<Result<T>>> operation,
        CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        
        try
        {
            _logger.LogInformation("Transaction started for operation {OperationType}", typeof(T).Name);
            
            var result = await operation();
            
            if (result.Success)
            {
                await _unitOfWork.CommitAsync(cancellationToken);
                _logger.LogInformation("Transaction committed successfully for operation {OperationType}", typeof(T).Name);
            }
            else
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                _logger.LogWarning("Transaction rolled back due to operation failure: {Error}", result.Error);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Transaction rolled back due to exception in operation {OperationType}", typeof(T).Name);
            return Result.Fail<T>($"An error occurred: {ex.Message}", "TRANSACTION_ERROR");
        }
    }
    
    /// <summary>
    /// Transaction içinde çalıştırılacak işlemi execute eder (void return)
    /// </summary>
    /// <param name="operation">Çalıştırılacak işlem</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem sonucu</returns>
    public async Task<Result> ExecuteInTransactionAsync(
        Func<Task<Result>> operation,
        CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        
        try
        {
            _logger.LogInformation("Transaction started for operation");
            
            var result = await operation();
            
            if (result.Success)
            {
                await _unitOfWork.CommitAsync(cancellationToken);
                _logger.LogInformation("Transaction committed successfully");
            }
            else
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                _logger.LogWarning("Transaction rolled back due to operation failure: {Error}", result.Error);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Transaction rolled back due to exception");
            return Result.Fail($"An error occurred: {ex.Message}", "TRANSACTION_ERROR");
        }
    }
}
