using System;
using Cysharp.Threading.Tasks;
using CameraFramework.Events;
using VContainer;
using VContainer.Unity;

namespace CameraFramework.Integration
{
    public sealed class CameraEventBridge : IStartable, IDisposable
    {
        private readonly IEventBus _eventBus;
        private readonly ICameraFramework _camera;

        [Inject]
        public CameraEventBridge(IEventBus eventBus, ICameraFramework camera)
        {
            _eventBus = eventBus;
            _camera = camera;
        }

        public void Start()
        {
            _eventBus.Subscribe<CameraShakeRequested>(OnShake);
            _eventBus.Subscribe<CameraFovRequested>(OnFov);
            _eventBus.Subscribe<CameraRecoilRequested>(OnRecoil);
            _eventBus.Subscribe<CameraSwitchRequested>(OnSwitch);
        }

        public void Dispose()
        {
            _eventBus.Unsubscribe<CameraShakeRequested>(OnShake);
            _eventBus.Unsubscribe<CameraFovRequested>(OnFov);
            _eventBus.Unsubscribe<CameraRecoilRequested>(OnRecoil);
            _eventBus.Unsubscribe<CameraSwitchRequested>(OnSwitch);
        }

        private void OnShake(CameraShakeRequested request) => _camera.Shake(request.Preset, request.Intensity);
        private void OnFov(CameraFovRequested request) => _camera.PulseFov(request.Offset);
        private void OnRecoil(CameraRecoilRequested request) => _camera.Recoil(request.Rotation);
        private void OnSwitch(CameraSwitchRequested request) => _camera.SwitchAsync(request.View).Forget();
    }
}
