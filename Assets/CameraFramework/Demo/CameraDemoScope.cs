using CameraFramework.Integration;
using EventSystem;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace CameraFramework.Demo
{
    public sealed class CameraDemoScope : LifetimeScope
    {
        [SerializeField] private CameraFrameworkController controller;
        [SerializeField] private CameraDemoPanel panel;

        public void Configure(CameraFrameworkController cameraController, CameraDemoPanel demoPanel)
        {
            controller = cameraController;
            panel = demoPanel;
        }

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<EventBus>(Lifetime.Singleton).As<IEventBus>();
            builder.RegisterCameraFramework(controller);
            builder.RegisterComponent(panel);
        }
    }
}
