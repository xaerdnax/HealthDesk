using System.Drawing;
using System.Windows.Forms;

namespace ClinicManagement;

public sealed class PatientRegistrationForm : Form
{
    private readonly TextBox _firstNameTextBox = new();
    private readonly TextBox _lastNameTextBox = new();
    private readonly TextBox _peselTextBox = new();
    private readonly Button _saveButton = new();

    public Patient? RegisteredPatient { get; private set; }

    public PatientRegistrationForm()
    {
        Text = "Rejestracja pacjenta";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(430, 250);

        BuildLayout();
    }

    private void BuildLayout()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(18),
            ColumnCount = 2,
            RowCount = 5,
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 115));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        AddField(layout, "Imię:", _firstNameTextBox, 0);
        AddField(layout, "Nazwisko:", _lastNameTextBox, 1);
        AddField(layout, "PESEL:", _peselTextBox, 2);

        var infoLabel = new Label
        {
            AutoSize = true,
            ForeColor = Color.DimGray,
            Text = "PESEL musi zawierać 11 cyfr i poprawną cyfrę kontrolną.",
            Margin = new Padding(3, 0, 3, 10),
        };
        layout.Controls.Add(infoLabel, 1, 3);

        _saveButton.Text = "Zapisz pacjenta";
        _saveButton.AutoSize = true;
        _saveButton.Anchor = AnchorStyles.Right;
        _saveButton.Click += SaveButton_Click;
        layout.Controls.Add(_saveButton, 1, 4);

        AcceptButton = _saveButton;
        Controls.Add(layout);
    }

    private static void AddField(TableLayoutPanel layout, string labelText, TextBox textBox, int row)
    {
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        layout.Controls.Add(new Label
        {
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Text = labelText,
            Margin = new Padding(3, 7, 3, 3),
        }, 0, row);
        textBox.Dock = DockStyle.Fill;
        layout.Controls.Add(textBox, 1, row);
    }

    private static bool IsValidName(string value)
    {
        return value.All(character => char.IsLetter(character) || character == ' ' || character == '-');
    }

    private void SaveButton_Click(object? sender, EventArgs e)
    {
        string firstName = _firstNameTextBox.Text.Trim();
        string lastName = _lastNameTextBox.Text.Trim();
        string pesel = _peselTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
        {
            MessageBox.Show("Imię i nazwisko są wymagane.", "Błąd walidacji", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!IsValidName(firstName) || !IsValidName(lastName))
        {
            MessageBox.Show(
                "Imię i nazwisko mogą zawierać tylko litery, spacje lub myślnik. Nie mogą zawierać cyfr.",
                "Błąd walidacji",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        if (!PeselValidator.IsValid(pesel))
        {
            MessageBox.Show("Podaj prawidłowy numer PESEL.", "Błąd walidacji", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _peselTextBox.Focus();
            return;
        }

        RegisteredPatient = new Patient
        {
            FirstName = firstName,
            LastName = lastName,
            Pesel = pesel,
        };
        DialogResult = DialogResult.OK;
        Close();
    }
}
