using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;

namespace CameraFramework
{
    [DisallowMultipleComponent]
    public sealed class CameraEffectsExtension : CinemachineExtension
    {
        private const float NoiseScale = 2f;
        private const float NoiseOffset = 1f;
        private const float PhaseStep = 31.416f;

        private ShakeSettings _shake;
        private Tween _shakeTween;
        private Tween _recoilTween;
        private float _weight;
        private float _phase;
        private float _intensity;
        private Vector3 _recoil;

        public void Shake(ShakeSettings settings, float intensity)
        {
            if (intensity <= 0f) return;
            _shake = settings;
            _intensity = intensity;
            _weight = 1f;
            _phase += PhaseStep;
            _shakeTween?.Kill();
            _shakeTween = DOTween.To(() => _weight, value => _weight = value, 0f,
                    Mathf.Max(.01f, settings.Duration))
                .SetEase(settings.Ease).SetLink(gameObject, LinkBehaviour.KillOnDisable);
        }

        public void Recoil(Vector3 rotation, float returnDuration)
        {
            _recoilTween?.Kill();
            _recoil += rotation;
            _recoilTween = DOTween.To(() => _recoil, value => _recoil = value, Vector3.zero,
                    Mathf.Max(.01f, returnDuration))
                .SetEase(Ease.OutCubic).SetLink(gameObject, LinkBehaviour.KillOnDisable);
        }

        public void StopEffects()
        {
            _shakeTween?.Kill();
            _recoilTween?.Kill();
            _weight = 0f;
            _recoil = Vector3.zero;
        }

        protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase virtualCamera,
            CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
        {
            if (stage != CinemachineCore.Stage.Finalize) return;

            if (_weight > 0f)
            {
                var time = Time.unscaledTime * _shake.Frequency;
                var position = Noise(time, _phase);
                var rotation = Noise(time, _phase + PhaseStep * 3f);
                var amplitude = _weight * _intensity;
                state.PositionCorrection += Vector3.Scale(position, _shake.Position) * amplitude;
                state.OrientationCorrection *= Quaternion.Euler(Vector3.Scale(rotation, _shake.Rotation) * amplitude);
            }

            if (_recoil != Vector3.zero)
                state.OrientationCorrection *= Quaternion.Euler(_recoil);
        }

        private static Vector3 Noise(float time, float phase) => new(
            Signed(Mathf.PerlinNoise(time + phase, phase)),
            Signed(Mathf.PerlinNoise(time + phase * 2f, phase * 3f)),
            Signed(Mathf.PerlinNoise(time + phase * 4f, phase * 5f)));

        private static float Signed(float value) => value * NoiseScale - NoiseOffset;

        private void OnDisable() => StopEffects();
    }
}
