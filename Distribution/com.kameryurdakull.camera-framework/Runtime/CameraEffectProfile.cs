using System;
using DG.Tweening;
using UnityEngine;

namespace CameraFramework
{
    public enum CameraEffectPreset { Subtle, Impact, Heavy }
    public enum CameraView { Overview, Showcase, Detail }

    [Serializable]
    public struct ShakeSettings
    {
        public Vector3 Position;
        public Vector3 Rotation;
        [Min(0.01f)] public float Frequency;
        [Min(0.01f)] public float Duration;
        public Ease Ease;
    }

    [CreateAssetMenu(menuName = "Camera Framework/Effect Profile")]
    public sealed class CameraEffectProfile : ScriptableObject
    {
        [SerializeField] private ShakeSettings subtle = new()
        {
            Position = new Vector3(.08f, .06f, .03f), Rotation = new Vector3(.5f, .5f, .3f),
            Frequency = 16f, Duration = .35f, Ease = Ease.OutCubic
        };
        [SerializeField] private ShakeSettings impact = new()
        {
            Position = new Vector3(.22f, .18f, .08f), Rotation = new Vector3(1.2f, 1f, .8f),
            Frequency = 20f, Duration = .65f, Ease = Ease.OutQuart
        };
        [SerializeField] private ShakeSettings heavy = new()
        {
            Position = new Vector3(.4f, .32f, .12f), Rotation = new Vector3(2.3f, 1.8f, 1.5f),
            Frequency = 23f, Duration = 1.1f, Ease = Ease.OutExpo
        };

        public ShakeSettings GetShake(CameraEffectPreset preset) => preset switch
        {
            CameraEffectPreset.Subtle => subtle,
            CameraEffectPreset.Impact => impact,
            CameraEffectPreset.Heavy => heavy,
            _ => impact
        };
    }
}
