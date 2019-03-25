using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using AudioBand.AudioSource;
using AudioBand.Logging;
using AudioBand.Settings;
using CSDeskBand;
using SimpleInjector;

namespace AudioBand
{
    /// <summary>
    /// The deskband
    /// </summary>
    [ComVisible(true)]
    [Guid("957D8782-5B07-4126-9B24-1E917BAAAD64")]
    [CSDeskBandRegistration(Name = "Audio Band", ShowDeskBand = true)]
    public class Deskband : CSDeskBandWin
    {
        private MainControl _mainControl;
        private Container _container;

        /// <summary>
        /// Initializes a new instance of the <see cref="Deskband"/> class.
        /// </summary>
        public Deskband()
        {
            AudioBandLogManager.Initialize();
            AppDomain.CurrentDomain.UnhandledException += (sender, args) => AudioBandLogManager.GetLogger("AudioBand").Error((Exception)args.ExceptionObject, "Unhandled Exception");
            ConfigureDependencies();
            _mainControl = _container.GetInstance<MainControl>();
        }

        /// <inheritdoc/>
        protected override Control Control => _mainControl;

        /// <inheritdoc/>
        protected override void DeskbandOnClosed()
        {
            base.DeskbandOnClosed();
            _mainControl.CloseAudioband();
            _mainControl.Hide();
            _mainControl = null;
        }

        private void ConfigureDependencies()
        {
            _container = new Container();
            _container.Register<IAudioSourceManager, AudioSourceManager>();
            _container.Register<IAppSettings, AppSettings>();
            _container.RegisterInstance(Options);
            _container.RegisterInstance(TaskbarInfo);
            _container.Verify();
        }
    }
}
