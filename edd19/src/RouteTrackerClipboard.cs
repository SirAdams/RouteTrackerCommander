using System;
using System.Drawing;
using System.Windows.Forms;
namespace EDDiscovery.UserControls
{
    public partial class CommanderSurveyorPanel
    {
        private ExtendedControls.ExtButton copyRouteTargetButton;
        private string displayedRouteTarget;
        private void InitializeCopyRouteTargetButton()
        {
            copyRouteTargetButton = new ExtendedControls.ExtButton {
                Name = "copyRouteTargetButton", Image = EDDiscovery.Icons.Controls.Copy,
                Size = new Size(28, 28), Margin = new Padding(3, 1, 3, 1),
                FlatStyle = FlatStyle.Flat, Enabled = false,
                AccessibleName = "Copy current route target", UseVisualStyleBackColor = false
            };
            toolTip.SetToolTip(copyRouteTargetButton, "Copy current route target to clipboard");
            copyRouteTargetButton.Click += (sender, args) => CopyDisplayedRouteTarget(SetClipboardText);
            panelControls.Controls.Add(copyRouteTargetButton);
            panelControls.Controls.SetChildIndex(copyRouteTargetButton, panelControls.Controls.GetChildIndex(extButtonControlRoute) + 1);
        }
        private void SetDisplayedRouteTarget(string target)
        {
            displayedRouteTarget = target;
            copyRouteTargetButton.Enabled = currentRoute != null && !string.IsNullOrWhiteSpace(target);
        }
        internal bool CopyDisplayedRouteTarget(Action<string> copy)
        {
            if (commanderId < 0 || currentRoute == null || string.IsNullOrWhiteSpace(displayedRouteTarget)) return false;
            copy(displayedRouteTarget);
            return true;
        }
    }
}
