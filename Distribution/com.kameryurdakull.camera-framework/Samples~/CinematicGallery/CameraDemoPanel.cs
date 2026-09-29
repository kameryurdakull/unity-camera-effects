using CameraFramework.Integration;
using Cysharp.Threading.Tasks;
using CameraFramework.Events;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace CameraFramework.Demo
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class CameraDemoPanel : MonoBehaviour
    {
        private const string SubtleButton = "shake-subtle";
        private const string ImpactButton = "shake-impact";
        private const string HeavyButton = "shake-heavy";
        private const string FovInButton = "fov-in";
        private const string FovOutButton = "fov-out";
        private const string RecoilButton = "recoil";
        private const string FocusButton = "focus";
        private const string OverviewButton = "view-overview";
        private const string ShowcaseButton = "view-showcase";
        private const string DetailButton = "view-detail";

        [SerializeField] private Transform focusTarget;
        private ICameraFramework _camera;
        private IEventBus _events;

        public void Configure(Transform target) => focusTarget = target;

        [Inject]
        public void Construct(ICameraFramework camera, IEventBus events)
        {
            _camera = camera;
            _events = events;
        }

        private void Start()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            Bind(root, SubtleButton, () => _events.Publish(new CameraShakeRequested(CameraEffectPreset.Subtle)));
            Bind(root, ImpactButton, () => _events.Publish(new CameraShakeRequested(CameraEffectPreset.Impact)));
            Bind(root, HeavyButton, () => _events.Publish(new CameraShakeRequested(CameraEffectPreset.Heavy)));
            Bind(root, FovInButton, () => _events.Publish(new CameraFovRequested(-15f)));
            Bind(root, FovOutButton, () => _events.Publish(new CameraFovRequested(18f)));
            Bind(root, RecoilButton, () => _events.Publish(new CameraRecoilRequested(new Vector3(-5f, 1f, 1.5f))));
            Bind(root, FocusButton, () => _camera.FocusAsync(focusTarget).Forget());
            Bind(root, OverviewButton, () => _events.Publish(new CameraSwitchRequested(CameraView.Overview)));
            Bind(root, ShowcaseButton, () => _events.Publish(new CameraSwitchRequested(CameraView.Showcase)));
            Bind(root, DetailButton, () => _events.Publish(new CameraSwitchRequested(CameraView.Detail)));
        }

        private static void Bind(VisualElement root, string name, System.Action action)
        {
            var button = root.Q<Button>(name);
            if (button != null) button.clicked += action;
        }
    }
}
