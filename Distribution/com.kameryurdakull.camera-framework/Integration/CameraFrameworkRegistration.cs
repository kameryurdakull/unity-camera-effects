using VContainer;
using VContainer.Unity;

namespace CameraFramework.Integration
{
    public static class CameraFrameworkRegistration
    {
        /// <summary>
        /// Registers the camera service. When event routing is enabled, the scope must also register IEventBus.
        /// </summary>
        public static void RegisterCameraFramework(this IContainerBuilder builder,
            CameraFrameworkController controller, bool routeEvents = true)
        {
            builder.RegisterComponent(controller).As<ICameraFramework>();
            if (routeEvents) builder.RegisterEntryPoint<CameraEventBridge>();
        }
    }
}
