using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using UnityPlayer;

namespace vpad1
{
    class App : IFrameworkView, IFrameworkViewSource
    {
        private WinRTBridge.WinRTBridge m_Bridge;
        private AppCallbacks m_AppCallbacks;

        public App()
        {
            SetupOrientation();
            m_AppCallbacks = new AppCallbacks();
        }

        public virtual void Initialize(CoreApplicationView applicationView)
        {
            applicationView.Activated += ApplicationView_Activated;
            CoreApplication.Suspending += CoreApplication_Suspending;

            // Setup scripting bridge
            m_Bridge = new WinRTBridge.WinRTBridge();
            m_AppCallbacks.SetBridge(m_Bridge);

            m_AppCallbacks.SetCoreApplicationViewEvents(applicationView);
        }

        private void CoreApplication_Suspending(object sender, SuspendingEventArgs e)
        {
        }

        private void ApplicationView_Activated(CoreApplicationView sender, IActivatedEventArgs args)
        {
            CoreWindow.GetForCurrentThread().Activate();
        }

        public void SetWindow(CoreWindow coreWindow)
        {
            ApplicationView.GetForCurrentView().SuppressSystemOverlays = true;

            m_AppCallbacks.SetCoreWindowEvents(coreWindow);
            m_AppCallbacks.InitializeD3DWindow();
        }

        public void Load(string entryPoint)
        {
        }

        public void Run()
        {
            m_AppCallbacks.Run();
        }

        public void Uninitialize()
        {
        }

        [MTAThread]
        static void Main(string[] args)
        {
            var app = new App();
            CoreApplication.Run(app);
        }

        public IFrameworkView CreateView()
        {
            return this;
        }

        private void SetupOrientation()
        {
            Unity.UnityGenerated.SetupDisplay();
        }
    }
}
