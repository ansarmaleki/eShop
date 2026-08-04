using System.Collections.Concurrent;

namespace eShop.WebhookClient.Services;

public class HooksRepository(ILogger<HooksRepository> logger)
{
    private readonly ConcurrentQueue<WebHookReceived> _data = new();
    private readonly ConcurrentDictionary<OnChangeSubscription, object?> _onChangeSubscriptions = new();

    public Task AddNew(WebHookReceived hook)
    {
        _data.Enqueue(hook);

        foreach (var subscription in _onChangeSubscriptions)
        {
            // The callback is fire-and-forget, so faults would otherwise go unobserved.
            _ = NotifySubscriberAsync(subscription.Key);
        }

        return Task.CompletedTask;
    }

    public Task<IEnumerable<WebHookReceived>> GetAll()
    {
        return Task.FromResult(_data.AsEnumerable());
    }

    private async Task NotifySubscriberAsync(OnChangeSubscription subscription)
    {
        try
        {
            await subscription.NotifyAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error notifying subscriber of a received webhook");
        }
    }

    public IDisposable Subscribe(Func<Task> callback)
    {
        var subscription = new OnChangeSubscription(callback, this);
        _onChangeSubscriptions.TryAdd(subscription, null);
        return subscription;
    }

    private class OnChangeSubscription(Func<Task> callback, HooksRepository owner) : IDisposable
    {
        public Task NotifyAsync() => callback();

        public void Dispose() => owner._onChangeSubscriptions.Remove(this, out _);
    }
}
