# Camera Framework

Cinemachine 3 camera gestures behind one `ICameraFramework` service. The core runtime assembly depends on Cinemachine, UniTask and DOTween. EventBus and VContainer wiring live in the separate Integration assembly, so the core can be reused with another composition root.

## Quick start

1. Add a `CinemachineBrain` to the render camera and create one or more `CinemachineCamera` objects.
2. Create an **Effect Profile** with **Assets > Create > Camera Framework > Effect Profile**.
3. Add `CameraFrameworkController` to a scene object. Assign the Brain, Profile and camera slots in the Inspector, including an initial view.
4. In a VContainer `LifetimeScope`, call `builder.RegisterCameraFramework(controller)` after registering your project's `IEventBus`. Pass `routeEvents: false` if direct service calls are enough.
5. Inject `ICameraFramework` into gameplay systems.

```csharp
private readonly ICameraFramework _camera;

[Inject]
public WeaponCameraFeedback(ICameraFramework camera) => _camera = camera;

public void OnFire()
{
    _camera.Recoil(new Vector3(-4f, 0f, 0f));
    _camera.PulseFov(-8f);
    _camera.Shake(CameraEffectPreset.Subtle);
}
```

The package also accepts `CameraShakeRequested`, `CameraFovRequested`, `CameraRecoilRequested` and `CameraSwitchRequested` through the existing EventBus. `FocusAsync` temporarily moves the active camera's LookAt proxy to a `Transform`; the camera needs an Aim component, such as `CinemachineRotationComposer`, for the focus movement to appear.

## Demo

Open `Demo/CameraFrameworkDemo.unity` and enter Play Mode. The UI Toolkit panel tests three camera angles, three shake strengths, recoil, focus and FOV pulses. Geometry is made with editable ProBuilder meshes. **Tools > Camera Framework > Create Demo Scene** creates another full scene at a unique path, preserving existing demos.

The integration uses the EventBus assembly already in this project. When moving the package to another project, include an equivalent `IEventBus` implementation or omit the Integration and Demo assemblies and register the core service directly.
