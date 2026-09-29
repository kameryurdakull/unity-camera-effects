using System;
using Cysharp.Threading.Tasks;

namespace CameraFramework.Events
{
    public interface IEventBus
    {
        void Subscribe<T>(Action<T> callback);
        void Unsubscribe<T>(Action<T> callback);
        UniTask PublishAsync<T>(T eventData);
        void Publish<T>(T eventMessage);
    }
}
