using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;

namespace CameraFramework
{
    [Serializable]
    public struct CameraSlot
    {
        public CameraView View;
        public CinemachineCamera Camera;
    }

    [DisallowMultipleComponent]
    public sealed class CameraFrameworkController : MonoBehaviour, ICameraFramework
    {
        private const int ActivePriority = 100;
        private const int InactivePriority = 0;

        [SerializeField] private CinemachineBrain brain;
        [SerializeField] private CameraEffectProfile profile;
        [SerializeField] private CameraSlot[] cameras = Array.Empty<CameraSlot>();
        [SerializeField] private CameraView initialView;

        private CinemachineCamera _active;
        private CameraEffectsExtension _extension;
        private Tween _fovTween;
        private CinemachineCamera _fovCamera;
        private float _fovBaseline;
        private Transform _focusProxy;
        private CancellationTokenSource _focusCancellation;

        public void Configure(CinemachineBrain cameraBrain, CameraEffectProfile effectProfile,
            CameraSlot[] cameraSlots, CameraView startingView)
        {
            brain = cameraBrain;
            profile = effectProfile;
            cameras = cameraSlots;
            initialView = startingView;
            Initialize();
        }

        private void Awake() => Initialize();

        private void Initialize()
        {
            foreach (var slot in cameras)
            {
                if (slot.Camera == null) continue;
                slot.Camera.Priority = slot.View == initialView ? ActivePriority : InactivePriority;
                if (slot.View == initialView) Activate(slot.Camera);
            }
        }

        public void Shake(CameraEffectPreset preset, float intensity = 1f)
        {
            if (_extension == null || profile == null) return;
            _extension.Shake(profile.GetShake(preset), Mathf.Max(0f, intensity));
        }

        public void Recoil(Vector3 rotation, float returnDuration = .4f) =>
            _extension?.Recoil(rotation, returnDuration);

        public void PulseFov(float offset, float attack = .15f, float release = .35f)
        {
            if (_active == null) return;
            RestoreFov();
            var camera = _active;
            var original = camera.Lens.FieldOfView;
            _fovCamera = camera;
            _fovBaseline = original;
            var sequence = DOTween.Sequence().SetLink(gameObject, LinkBehaviour.KillOnDisable);
            sequence.Append(DOTween.To(() => camera.Lens.FieldOfView, value => SetFov(camera, value),
                Mathf.Clamp(original + offset, 1f, 179f), Mathf.Max(.01f, attack)).SetEase(Ease.OutQuad));
            sequence.Append(DOTween.To(() => camera.Lens.FieldOfView, value => SetFov(camera, value),
                original, Mathf.Max(.01f, release)).SetEase(Ease.InOutSine));
            sequence.OnComplete(() =>
            {
                if (_fovTween != sequence) return;
                _fovTween = null;
                _fovCamera = null;
            });
            _fovTween = sequence;
        }

        public async UniTask FocusAsync(Transform target, float moveDuration = .5f, float holdDuration = 1f,
            CancellationToken cancellationToken = default)
        {
            if (_active == null || target == null) return;
            CancelFocus();
            var source = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken, this.GetCancellationTokenOnDestroy());
            _focusCancellation = source;
            var token = source.Token;
            var camera = _active;
            var originalLookAt = camera.LookAt;
            var start = originalLookAt != null ? originalLookAt.position : camera.transform.position + camera.transform.forward * 10f;
            var proxy = new GameObject(nameof(CameraFrameworkController) + "FocusProxy").transform;
            _focusProxy = proxy;
            proxy.position = start;
            camera.LookAt = proxy;
            Tween focusTween = null;
            try
            {
                focusTween = proxy.DOMove(target.position, Mathf.Max(.01f, moveDuration)).SetEase(Ease.InOutSine);
                await UniTask.Delay(TimeSpan.FromSeconds(Mathf.Max(.01f, moveDuration) +
                    Mathf.Max(0f, holdDuration)), cancellationToken: token);
                if (originalLookAt != null)
                {
                    focusTween.Kill();
                    focusTween = proxy.DOMove(originalLookAt.position, Mathf.Max(.01f, moveDuration))
                        .SetEase(Ease.InOutSine);
                    await UniTask.Delay(TimeSpan.FromSeconds(Mathf.Max(.01f, moveDuration)), cancellationToken: token);
                }
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested) { }
            finally
            {
                if (camera != null && camera.LookAt == proxy) camera.LookAt = originalLookAt;
                focusTween?.Kill();
                if (proxy != null) Destroy(proxy.gameObject);
                if (_focusProxy == proxy) _focusProxy = null;
                if (_focusCancellation == source) _focusCancellation = null;
                source.Dispose();
            }
        }

        public async UniTask SwitchAsync(CameraView view, CancellationToken cancellationToken = default)
        {
            var destination = FindCamera(view);
            if (destination == null || destination == _active) return;
            CancelFocus();
            RestoreFov();
            foreach (var slot in cameras)
                if (slot.Camera != null) slot.Camera.Priority = slot.Camera == destination ? ActivePriority : InactivePriority;
            Activate(destination);
            var duration = brain != null ? brain.DefaultBlend.Time : 0f;
            if (duration > 0f)
                await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: cancellationToken);
        }

        public void StopEffects()
        {
            RestoreFov();
            _extension?.StopEffects();
            CancelFocus();
        }

        private CinemachineCamera FindCamera(CameraView view)
        {
            foreach (var slot in cameras)
                if (slot.View == view) return slot.Camera;
            return null;
        }

        private void Activate(CinemachineCamera camera)
        {
            _extension?.StopEffects();
            _active = camera;
            _extension = camera.GetComponent<CameraEffectsExtension>();
            if (_extension == null) _extension = camera.gameObject.AddComponent<CameraEffectsExtension>();
        }

        private static void SetFov(CinemachineCamera camera, float value)
        {
            var lens = camera.Lens;
            lens.FieldOfView = value;
            camera.Lens = lens;
        }

        private void CancelFocus()
        {
            _focusCancellation?.Cancel();
            _focusCancellation = null;
        }

        private void RestoreFov()
        {
            _fovTween?.Kill();
            _fovTween = null;
            if (_fovCamera != null) SetFov(_fovCamera, _fovBaseline);
            _fovCamera = null;
        }

        private void OnDestroy() => StopEffects();
    }
}
