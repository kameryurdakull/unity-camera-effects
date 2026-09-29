# Cinematic Gallery

Open `CameraFrameworkDemo.unity` after importing this sample from Package Manager, then enter Play Mode.

The left panel contains:

| Section | Buttons | What to observe |
| --- | --- | --- |
| Camera angles | Overview, Showcase, Detail | Cinemachine blends between three cameras. |
| Impact & motion | Subtle Shake, Impact, Heavy Shake | Increasing positional and rotational noise. |
| Impact & motion | Recoil | A quick camera kick with a smooth return. |
| Impact & motion | Focus | The active camera turns toward the cyan side beacon, then returns. |
| Lens | Punch In, Pull Back | Temporary FOV changes. |

The gallery mesh is editable ProBuilder geometry. The demo uses URP materials, UI Toolkit, VContainer and the EventBus included in the package. The `Tools > Camera Framework > Create Demo Scene` menu creates another gallery scene at a unique path and leaves the imported scene intact.
