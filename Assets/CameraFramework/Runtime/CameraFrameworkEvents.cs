using UnityEngine;

namespace CameraFramework
{
    public readonly struct CameraShakeRequested
    {
        public readonly CameraEffectPreset Preset;
        public readonly float Intensity;
        public CameraShakeRequested(CameraEffectPreset preset, float intensity = 1f)
        {
            Preset = preset;
            Intensity = intensity;
        }
    }

    public readonly struct CameraFovRequested
    {
        public readonly float Offset;
        public CameraFovRequested(float offset) => Offset = offset;
    }

    public readonly struct CameraRecoilRequested
    {
        public readonly Vector3 Rotation;
        public CameraRecoilRequested(Vector3 rotation) => Rotation = rotation;
    }

    public readonly struct CameraSwitchRequested
    {
        public readonly CameraView View;
        public CameraSwitchRequested(CameraView view) => View = view;
    }
}
