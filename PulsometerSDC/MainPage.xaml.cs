using System;
using Xamarin.Forms.Xaml;
using Xamarin.Forms;

using Tizen.Wearable.CircularUI.Forms;
using Tizen.Security;
using Tizen.Sensor;

namespace PulsometerSDC
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : CirclePage
    {
        private HeartRateMonitor _monitor;
        private bool _measuring = false;

        public MainPage()
        {
            InitializeComponent();
            CheckPrivileges();
        }

        private void CheckPrivileges()
        {
            // check permission status (allow, deny, ask) to determine action which has to be taken
            string privilege = "http://tizen.org/privilege/healthinfo";
            CheckResult result = PrivacyPrivilegeManager.CheckPermission(privilege);

            if (result == CheckResult.Allow)
            {
                OnPrivilegesGranted();
            }
            else if (result == CheckResult.Deny)
            {
                OnPrivilegesDenied();
            }
            else // the user must be asked about granting the privilege
            {
                PrivacyPrivilegeManager.GetResponseContext(privilege).TryGetTarget(out var context);

                if (context != null)
                {
                    context.ResponseFetched += (sender, e) =>
                    {
                        if (e.cause == CallCause.Answer && e.result == RequestResult.AllowForever)
                        {
                            OnPrivilegesGranted();
                        }
                        else
                        {
                            OnPrivilegesDenied();
                        }
                    };
                }

                PrivacyPrivilegeManager.RequestPermission(privilege);
            }
        }

        private void OnPrivilegesGranted()
        {
            // create an instance of the monitor
            _monitor = new HeartRateMonitor();
            // specify frequency of the sensor data event by setting the interval value (in milliseconds)
            _monitor.Interval = 1000;

            // stop the measurement when the application goes background
            MessagingCenter.Subscribe<App>(this, "sleep", (sender) => { if (_measuring) { StopMeasurement(); } });
        }

        private void OnPrivilegesDenied()
        {
            // close the application
            Tizen.Applications.Application.Current.Exit();
        }

        private void OnMonitorDataUpdated(object sender, HeartRateMonitorDataUpdatedEventArgs e)
        {
            // update displayed value
            hrValue.Text = e.HeartRate > 0 ? e.HeartRate.ToString() : "0";
        }

        private void StartMeasurement()
        {
            _monitor.DataUpdated += OnMonitorDataUpdated;
            _monitor.Start();
            _measuring = true;

            // update the view
            actionButton.Text = "STOP";
            measuringIndicator.IsVisible = true;
        }

        private void StopMeasurement()
        {
            _monitor.DataUpdated -= OnMonitorDataUpdated;
            _monitor.Stop();
            _measuring = false;

            // update the view
            actionButton.Text = "MEASURE";
            measuringIndicator.IsVisible = false;
        }

        private void OnActionButtonClicked(object sender, EventArgs e)
        {
            if (_measuring)
            {
                StopMeasurement();
            }
            else
            {
                StartMeasurement();
            }
        }
    }
}