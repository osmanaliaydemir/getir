using System.Threading.Channels;
using Microsoft.Extensions.Logging;

namespace Getir.Application.Common;

/// <summary>
/// Background task işlemleri için interface
/// </summary>
public interface IBackgroundTaskService
{
    /// <summary>
    /// Background task'ı kuyruğa ekler
    /// </summary>
    Task EnqueueTaskAsync<T>(T taskData, CancellationToken cancellationToken = default) where T : class;
    
    /// <summary>
    /// Background task'ı kuyruğa ekler (priority ile)
    /// </summary>
    Task EnqueueTaskAsync<T>(T taskData, TaskPriority priority, CancellationToken cancellationToken = default) where T : class;
    
    /// <summary>
    /// Background task'ı hemen çalıştırır
    /// </summary>
    Task ExecuteTaskAsync<T>(T taskData, CancellationToken cancellationToken = default) where T : class;
}

/// <summary>
/// Background task priority seviyeleri
/// </summary>
public enum TaskPriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Critical = 3
}

/// <summary>
/// Background task data wrapper
/// </summary>
public class BackgroundTaskData<T> where T : class
{
    /// <summary>
    /// Task data
    /// </summary>
    public T Data { get; set; } = default!;
    /// <summary>
    /// Task priority
    /// </summary>
    public TaskPriority Priority { get; set; } = TaskPriority.Normal;
    /// <summary>
    /// Task created at
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    /// <summary>
    /// Task correlation id
    /// </summary>
    public string? CorrelationId { get; set; }
}

/// <summary>
/// Background task servisi implementasyonu
/// </summary>
public class BackgroundTaskService : IBackgroundTaskService, IDisposable
{
    private readonly Channel<BackgroundTaskData<object>> _taskChannel;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BackgroundTaskService> _logger;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly Task _processingTask;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="serviceProvider">Service provider</param>
    /// <param name="logger">Logger</param>
    public BackgroundTaskService(IServiceProvider serviceProvider, ILogger<BackgroundTaskService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _cancellationTokenSource = new CancellationTokenSource();
        
        var options = new BoundedChannelOptions(1000)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = false,
            SingleWriter = false
        };
        
        _taskChannel = Channel.CreateBounded<BackgroundTaskData<object>>(options);
        
        // Background task processor'ı başlat
        _processingTask = Task.Run(ProcessTasksAsync);
    }

    /// <summary>
    /// Background task'ı kuyruğa ekler
    /// </summary>
    /// <typeparam name="T">Task data type</typeparam>
    /// <param name="taskData">Task data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task</returns>
    public async Task EnqueueTaskAsync<T>(T taskData, CancellationToken cancellationToken = default) where T : class
    {
        await EnqueueTaskAsync(taskData, TaskPriority.Normal, cancellationToken);
    }

    /// <summary>
    /// Background task'ı kuyruğa ekler (priority ile)
    /// </summary>
    /// <typeparam name="T">Task data type</typeparam>
    /// <param name="taskData">Task data</param>
    /// <param name="priority">Task priority</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task</returns>
    public async Task EnqueueTaskAsync<T>(T taskData, TaskPriority priority, CancellationToken cancellationToken = default) where T : class
    {
        var backgroundTask = new BackgroundTaskData<object>
        {
            Data = taskData,
            Priority = priority,
            CorrelationId = Guid.NewGuid().ToString()
        };

        await _taskChannel.Writer.WriteAsync(backgroundTask, cancellationToken);
        _logger.LogDebug("Background task enqueued: {Type} with priority {Priority}", typeof(T).Name, priority);
    }

    /// <summary>
    /// Background task'ı hemen çalıştırır
    /// </summary>
    /// <typeparam name="T">Task data type</typeparam>
    /// <param name="taskData">Task data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task</returns>
    public async Task ExecuteTaskAsync<T>(T taskData, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            _logger.LogDebug("Executing background task immediately: {Type}", typeof(T).Name);
            
            // Task'ı hemen çalıştır
            await ProcessTaskDataAsync(taskData, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing background task: {Type}", typeof(T).Name);
            throw;
        }
    }

    /// <summary>
    /// Background task'ları işler
    /// </summary>
    /// <returns>Task</returns>
    private async Task ProcessTasksAsync()
    {
        await foreach (var taskData in _taskChannel.Reader.ReadAllAsync(_cancellationTokenSource.Token))
        {
            try
            {
                _logger.LogDebug("Processing background task: {Type} with priority {Priority}", 
                    taskData.Data.GetType().Name, taskData.Priority);
                
                await ProcessTaskDataAsync(taskData.Data, _cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing background task: {Type}", taskData.Data.GetType().Name);
            }
        }
    }

    /// <summary>
    /// Background task'ı işler
    /// </summary>
    /// <typeparam name="T">Task data type</typeparam>
    /// <param name="taskData">Task data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task</returns>
    private async Task ProcessTaskDataAsync<T>(T taskData, CancellationToken cancellationToken) where T : class
    {
        // Bu method'da task type'ına göre uygun handler'ı bulup çalıştırmak gerekir
        // Şimdilik basit bir implementasyon
        
        _logger.LogInformation("Processing task data: {Type}", typeof(T).Name);
        
        // Task processing logic burada olacak
        await Task.Delay(100, cancellationToken); // Simulated work
    }

    /// <summary>
    /// Dispose
    /// </summary>
    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _taskChannel.Writer.Complete();
        _cancellationTokenSource.Dispose();
    }
}
