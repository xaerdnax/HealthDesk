using System.Drawing;
using System.Windows.Forms;

namespace ClinicManagement;

public sealed class MainForm : Form
{
    private readonly JsonDataStore _dataStore = new();
    private readonly BindingSource _patientsBindingSource = new();
    private readonly DataGridView _patientsGrid = new();
    private List<Patient> _patients = new();

    public MainForm()
    {
        Text = "System zarządzania przychodnią";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(900, 550);
        Size = new Size(1050, 650);

        LoadPatients();
        BuildLayout();
        RefreshPatientsGrid();
    }

    private void BuildLayout()
    {
        var rootLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(12),
        };
        rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 210));
        rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        var navigationPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(5),
        };
        navigationPanel.Controls.Add(new Label
        {
            AutoSize = true,
            Font = new Font(Font, FontStyle.Bold),
            Text = "PRZYCHODNIA",
            Margin = new Padding(3, 5, 3, 20),
        });
        navigationPanel.Controls.Add(CreateNavigationButton("Rejestracja pacjenta", OpenRegistrationForm));
        navigationPanel.Controls.Add(CreateNavigationButton("Usuń pacjenta", DeleteSelectedPatient));
        navigationPanel.Controls.Add(CreateNavigationButton("Wizyty", ShowNotImplementedMessage));
        navigationPanel.Controls.Add(CreateNavigationButton("Lekarze", ShowNotImplementedMessage));
        navigationPanel.Controls.Add(CreateNavigationButton("Odśwież dane", (sender, e) =>
        {
            LoadPatients();
            RefreshPatientsGrid();
        }));
        navigationPanel.Controls.Add(CreateNavigationButton("Zakończ", (sender, e) => Close()));

        var contentLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2,
            ColumnCount = 1,
        };
        contentLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));
        contentLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        contentLayout.Controls.Add(new Label
        {
            Dock = DockStyle.Fill,
            Font = new Font(Font, FontStyle.Bold),
            Text = "Panel główny - pacjenci",
            TextAlign = ContentAlignment.MiddleLeft,
        }, 0, 0);

        ConfigurePatientsGrid();
        contentLayout.Controls.Add(_patientsGrid, 0, 1);

        rootLayout.Controls.Add(navigationPanel, 0, 0);
        rootLayout.Controls.Add(contentLayout, 1, 0);
        Controls.Add(rootLayout);
    }

    private static Button CreateNavigationButton(string text, EventHandler clickHandler)
    {
        var button = new Button
        {
            AutoSize = false,
            Height = 42,
            Width = 180,
            Text = text,
            Margin = new Padding(0, 0, 0, 8),
        };
        button.Click += clickHandler;
        return button;
    }

    private void ConfigurePatientsGrid()
    {
        _patientsGrid.Dock = DockStyle.Fill;
        _patientsGrid.ReadOnly = true;
        _patientsGrid.AllowUserToAddRows = false;
        _patientsGrid.AllowUserToDeleteRows = false;
        _patientsGrid.AutoGenerateColumns = false;
        _patientsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _patientsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

        _patientsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(Patient.Id),
            HeaderText = "ID",
            FillWeight = 15,
        });
        _patientsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(Patient.FirstName),
            HeaderText = "Imię",
        });
        _patientsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(Patient.LastName),
            HeaderText = "Nazwisko",
        });
        _patientsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(Patient.Pesel),
            HeaderText = "PESEL",
        });
    }

    private void OpenRegistrationForm(object? sender, EventArgs e)
    {
        using var form = new PatientRegistrationForm();
        if (form.ShowDialog(this) != DialogResult.OK || form.RegisteredPatient is null)
        {
            return;
        }

        Patient patient = form.RegisteredPatient;

        if (_patients.Any(existingPatient => existingPatient.Pesel == patient.Pesel))
        {
            MessageBox.Show(
                "Pacjent o podanym numerze PESEL już istnieje w bazie.",
                "Duplikat PESEL",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        patient.Id = _patients.Count == 0 ? 1 : _patients.Max(item => item.Id) + 1;
        _patients.Add(patient);
        SavePatients();
        RefreshPatientsGrid();
    }

    private void DeleteSelectedPatient(object? sender, EventArgs e)
    {
        if (_patientsGrid.SelectedRows.Count == 0 ||
            _patientsGrid.SelectedRows[0].DataBoundItem is not Patient patient)
        {
            MessageBox.Show("Zaznacz pacjenta, którego chcesz usunąć.", "Brak zaznaczenia", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        DialogResult confirmation = MessageBox.Show(
            $"Czy na pewno usunąć pacjenta {patient.FullName}?",
            "Potwierdzenie usunięcia",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (confirmation != DialogResult.Yes)
        {
            return;
        }

        _patients.Remove(patient);
        SavePatients();
        RefreshPatientsGrid();
    }

    private void LoadPatients()
    {
        try
        {
            _patients = _dataStore.LoadPatients();
        }
        catch (InvalidOperationException exception)
        {
            MessageBox.Show(exception.Message, "Błąd danych", MessageBoxButtons.OK, MessageBoxIcon.Error);
            _patients = new List<Patient>();
        }
    }

    private void SavePatients()
    {
        try
        {
            _dataStore.SavePatients(_patients);
        }
        catch (InvalidOperationException exception)
        {
            MessageBox.Show(exception.Message, "Błąd zapisu", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void RefreshPatientsGrid()
    {
        _patientsBindingSource.DataSource = null;
        _patientsBindingSource.DataSource = _patients;
        _patientsGrid.DataSource = _patientsBindingSource;
    }

    private void ShowNotImplementedMessage(object? sender, EventArgs e)
    {
        MessageBox.Show("Ten moduł zostanie dodany w kolejnym kroku.", "Informacja", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
