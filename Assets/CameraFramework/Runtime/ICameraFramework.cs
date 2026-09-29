using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CameraFramework
{
    public interface ICameraFramework
    {
        void Shake(CameraEffectPreset preset, float intensity = 1f);
        void Recoil(Vector3 rotation, float returnDuration = .4f);
        void PulseFov(float offset, float attack = .15f, float release = .35f);
        UniTask FocusAsync(Transform target, float moveDuration = .5f, float holdDuration = 1f,
            CancellationToken cancellationToken = default);
        UniTask SwitchAsync(CameraView view, CancellationToken cancellationToken = default);
        void StopEffects();
    }
}
