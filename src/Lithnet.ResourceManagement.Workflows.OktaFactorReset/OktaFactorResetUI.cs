using System.Web.UI.WebControls;
using System.Workflow.ComponentModel;
using Microsoft.IdentityManagement.WebUI.Controls;
using Microsoft.ResourceManagement.Workflow.Activities;

namespace Lithnet.ResourceManagement.Workflows
{
    public class OktaFactorResetUI : ActivitySettingsPart
    {
        private const string AllowedFactorTypesID = "controlAllowedFactorTypes";
        private const string TenantUrlID = "controlTenantUrl";
        private const string OktaIdAttributeNameID = "controlOktaIdAttributeName";

        /// <inheritdoc />
        /// <summary>
        ///  Creates a Table that contains the controls used by the activity UI
        ///  in the Workflow Designer of the FIM portal. Adds that Table to the
        ///  collection of Controls that defines each activity that can be selected
        ///  in the Workflow Designer of the FIM Portal. Calls the base class of 
        ///  ActivitySettingsPart to render the controls in the UI.
        /// </summary>
        protected override void CreateChildControls()
        {
            Table controlLayoutTable = new Table
            {
                Width = Unit.Percentage(100.0),
                BorderWidth = 0,
                CellPadding = 2
            };

            controlLayoutTable.Rows.Add(this.AddTableRowTextBox("Tenant URL", OktaFactorResetUI.TenantUrlID, 400, 0, false, false, null));
            controlLayoutTable.Rows.Add(this.AddTableRowDescription("Specify the full URL of the Okta tenant (Ensure the URL does not contain -admin after the tenant name)"));
            controlLayoutTable.Rows.Add(new TableRow());

            controlLayoutTable.Rows.Add(this.AddTableRowTextBox("Okta ID attribute name", OktaFactorResetUI.OktaIdAttributeNameID, 400, 0, false, false, null));
            controlLayoutTable.Rows.Add(this.AddTableRowDescription("Specify the system name of the attribute that contains the user's Okta ID"));
            controlLayoutTable.Rows.Add(new TableRow());

            controlLayoutTable.Rows.Add(this.AddTableRowTextBox("Factors to reset", OktaFactorResetUI.AllowedFactorTypesID, 400, 0, true, false, null));
            controlLayoutTable.Rows.Add(this.AddTableRowDescription("Enter the types of providers that this activity can reset. Use '*' to allow the activity to reset all factor types. Otherwise, use the format {provider}/{type} and separate each entry on a new line. For example, OKTA/push or YUBICO/token:hardware"));
            controlLayoutTable.Rows.Add(new TableRow());

            this.Controls.Add(controlLayoutTable);
            base.CreateChildControls();
        }

        private TableRow AddTableRowTextBox(string labelText, string controlID, int width, int maxLength, bool multiLine, bool password, string defaultValue)
        {
            TableCell cell;
            TableRow row = new TableRow();

            Label label = new Label();
            label.Text = labelText;
            label.CssClass = this.LabelCssClass;

            cell = new TableCell();
            cell.Controls.Add(label);
            row.Cells.Add(cell);

            TextBox text = new TextBox();
            text.ID = controlID;
            text.CssClass = this.TextBoxCssClass;
            text.Text = defaultValue;

            if (maxLength > 0)
            {
                text.MaxLength = maxLength;
            }

            text.Width = width;

            if (multiLine)
            {
                text.TextMode = TextBoxMode.MultiLine;
                text.Rows = 6;
                text.Wrap = true;
            }

            if (password)
            {
                text.TextMode = TextBoxMode.Password;
            }

            cell = new TableCell();
            cell.Controls.Add(text);
            row.Cells.Add(cell);
            return row;
        }

        private TableRow AddTableRowDescription(string labelText)
        {
            TableCell cell;
            TableRow row = new TableRow();
            
            Label label = new Label();
            label.Text = labelText;
            label.CssClass = this.LabelCssClass;

            cell = new TableCell();
            row.Cells.Add(cell);

            cell = new TableCell();
            cell.Controls.Add(label);
            row.Cells.Add(cell);

            return row;
        }

        private string GetText(string textBoxID)
        {
            TextBox textBox = (TextBox)this.FindControl(textBoxID);
            return textBox.Text ?? string.Empty;
        }

        private void SetText(string textBoxID, string text)
        {
            TextBox textBox = (TextBox)this.FindControl(textBoxID);
            if (textBox != null)
            {
                textBox.Text = text;
            }
        }

        private void SetControlReadOnlyOption(string textBoxID, bool readOnly)
        {
            TextBox control = this.FindControl(textBoxID) as TextBox;

            if (control != null)
            {
                control.ReadOnly = readOnly;
            }
        }

        /// <summary>
        /// Called when a user clicks the Save button in the Workflow Designer. 
        /// Returns an instance of the RequestLoggingActivity class that 
        /// has its properties set to the values entered into the text box controls
        /// used in the UI of the activity. 
        /// </summary>
        public override Activity GenerateActivityOnWorkflow(SequentialWorkflow workflow)
        {
            if (!this.ValidateInputs())
            {
                return null;
            }

            return new OktaFactorReset
            {
                TenantUrl = this.GetText(OktaFactorResetUI.TenantUrlID),
                AllowedFactorTypes = this.GetText(OktaFactorResetUI.AllowedFactorTypesID),
                OktaIdAttributeName = this.GetText(OktaFactorResetUI.OktaIdAttributeNameID)
            };
        }

        /// <summary>
        /// Called by FIM when the UI for the activity must be reloaded.
        /// It passes us an instance of our workflow activity so that we can
        /// extract the values of the properties to display in the UI.
        /// </summary>
        public override void LoadActivitySettings(Activity activity)
        {
            var resetActivity = activity as OktaFactorReset;

            if (resetActivity != null)
            {
                this.SetText(OktaFactorResetUI.TenantUrlID, resetActivity.TenantUrl);
                this.SetText(OktaFactorResetUI.AllowedFactorTypesID, resetActivity.AllowedFactorTypes);
                this.SetText(OktaFactorResetUI.OktaIdAttributeNameID, resetActivity.OktaIdAttributeName);
            }
        }

        /// <summary>
        /// Saves the activity settings.
        /// </summary>
        public override ActivitySettingsPartData PersistSettings()
        {
            ActivitySettingsPartData data = new ActivitySettingsPartData();
            data[OktaFactorReset.AllowedFactorTypesPropertyName] = this.GetText(OktaFactorResetUI.AllowedFactorTypesID);
            data[OktaFactorReset.TenantUrlPropertyName] = this.GetText(OktaFactorResetUI.TenantUrlID);
            data[OktaFactorReset.OktaIdAttributeNamePropertyName] = this.GetText(OktaFactorResetUI.OktaIdAttributeNameID);
            return data;
        }

        /// <summary>
        ///  Restores the activity settings in the UI
        /// </summary>
        public override void RestoreSettings(ActivitySettingsPartData data)
        {
            if (data == null)
            {
                return;
            }

            this.SetText(OktaFactorResetUI.AllowedFactorTypesID, (string)data[OktaFactorReset.AllowedFactorTypesPropertyName]);
            this.SetText(OktaFactorResetUI.TenantUrlID, (string)data[OktaFactorReset.TenantUrlPropertyName]);
            this.SetText(OktaFactorResetUI.OktaIdAttributeNameID, (string)data[OktaFactorReset.OktaIdAttributeNamePropertyName]);
        }

        /// <summary>
        ///  Switches the activity between read only and read/write mode
        /// </summary>
        public override void SwitchMode(ActivitySettingsPartMode mode)
        {
            bool readOnly = mode == ActivitySettingsPartMode.View;
            this.SetControlReadOnlyOption(OktaFactorResetUI.AllowedFactorTypesID, readOnly);
            this.SetControlReadOnlyOption(OktaFactorResetUI.TenantUrlID, readOnly);
            this.SetControlReadOnlyOption(OktaFactorResetUI.OktaIdAttributeNameID, readOnly);
        }

        public override string Title
        {
            get
            {
                return "Reset Okta factors activity";
            }
        }

        /// <summary>
        ///  In general, this method should be used to validate information entered
        ///  by the user when the activity is added to a workflow in the Workflow
        ///  Designer.
        ///  We could add code to verify that the log file path already exists on
        ///  the server that is hosting the FIM Portal and check that the activity
        ///  has permission to write to that location. However, the code
        ///  would only check if the log file path exists when the
        ///  activity is added to a workflow in the workflow designer. This class
        ///  will not be used when the activity is actually run.
        ///  For this activity we will just return true.
        /// </summary>
        public override bool ValidateInputs()
        {
            return true;
        }
    }
}
