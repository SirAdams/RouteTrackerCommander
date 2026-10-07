using System;
using System.Drawing;
using System.Windows.Forms;
using RouteTrackerCommander;
namespace EDDiscovery.UserControls
{
    public partial class CommanderSurveyorPanel
    {
        private ExtendedControls.ExtButton updateButton;
        private Timer updateTimer;
        private bool updateCheckRunning, updatesClosed;
        private ReleaseUpdate availableUpdate;
        private const string UpdateSetting = "RouteTrackerCommander_UpdateChecksEnabled";
        private void InitializeUpdateButton()
        {
            updateButton = new ExtendedControls.ExtButton { Name = "updateButton", Image = EDDiscovery.Icons.Controls.Refresh,
                Size = new Size(28, 28), Margin = new Padding(3, 1, 3, 1), FlatStyle = FlatStyle.Flat,
                AccessibleName = "Check for DLC updates", UseVisualStyleBackColor = false };
            toolTip.SetToolTip(updateButton, "Check for DLC updates (right-click for options)");
            updateButton.Click += (sender, args) => { if (availableUpdate != null) ShowAvailableUpdate(); else CheckForDlcUpdates(true); };
            var menu = new ContextMenuStrip(); components.Add(menu);
            var automatic = new ToolStripMenuItem("Automatically check for updates") { CheckOnClick = true };
            automatic.Click += (sender, args) => EliteDangerousCore.DB.UserDatabase.Instance.PutSetting(UpdateSetting, automatic.Checked);
            menu.Items.Add(automatic); menu.Items.Add("Check now", null, (sender, args) => CheckForDlcUpdates(true));
            menu.Opening += (sender, args) => automatic.Checked = EliteDangerousCore.DB.UserDatabase.Instance.GetSetting(UpdateSetting, true);
            updateButton.ContextMenuStrip = menu;
            panelControls.Controls.Add(updateButton);
            panelControls.Controls.SetChildIndex(updateButton, panelControls.Controls.GetChildIndex(copyRouteTargetButton) + 1);
            updateTimer = new Timer(components) { Interval = 12 * 60 * 60 * 1000 };
            updateTimer.Tick += (sender, args) => CheckAutomaticUpdates();
            Disposed += (sender, args) => StopUpdateChecks();
        }
        private void StartUpdateChecks() { updateTimer.Start(); CheckAutomaticUpdates(); }
        private void CheckAutomaticUpdates()
        {
            if (EliteDangerousCore.DB.UserDatabase.Instance.GetSetting(UpdateSetting, true)) CheckForDlcUpdates(false);
        }
        private void StopUpdateChecks() { updatesClosed = true; updateTimer?.Stop(); }
        private async void CheckForDlcUpdates(bool manual)
        {
            if (updateCheckRunning || updatesClosed || IsDisposed) return;
            updateCheckRunning = true;
            toolTip.SetToolTip(updateButton, "Checking for DLC updates...");
            try
            {
                var json = await ReleaseChecker.GetReleases(manual);
                if (updatesClosed || IsDisposed) return;
                availableUpdate = ReleaseChecker.SelectRelease(json, typeof(RouteTrackerCommanderEDDClass).Assembly.GetName().Version, HostAssetSuffix);
                updateButton.Text = availableUpdate == null ? "" : "!";
                toolTip.SetToolTip(updateButton, availableUpdate == null ? "No newer DLC package found — click to check again" : "DLC " + availableUpdate.Version.ToString(3) + " available — click for download");
                if (manual) { if (availableUpdate != null) ShowAvailableUpdate(); else MessageBox.Show(this, "No newer package was found for this EDDiscovery version.", "Route Tracker Commander", MessageBoxButtons.OK, MessageBoxIcon.Information); }
            }
            catch (Exception)
            {
                if (updatesClosed || IsDisposed) return;
                toolTip.SetToolTip(updateButton, "Update check failed — click to retry");
                if (manual) MessageBox.Show(this, "Could not check GitHub for updates. Check your connection or try again later.", "Route Tracker Commander", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            finally { updateCheckRunning = false; }
        }
        private void ShowAvailableUpdate()
        {
            if (availableUpdate == null) return;
            string message = "DLC " + availableUpdate.Version.ToString(3) + (availableUpdate.Preview ? " (preview)" : "") + " is available.\n\nChoose the " + HostAssetSuffix + " ZIP. Close EDDiscovery before replacing the DLL.\n\nOpen the release page?";
            if (MessageBox.Show(this, message, "Route Tracker Commander", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(availableUpdate.ReleaseUrl) { UseShellExecute = true });
        }
        private const string HostAssetSuffix = "EDD19.1.11";
    }
}
