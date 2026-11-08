using System.Text.Json;

namespace ClinicManagement;

public sealed class JsonDataStore
{
    private readonly string _filePath = Path.Combine(AppContext.BaseDirectory, "clinic-data.json");
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public List<Patient> LoadPatients()
    {
        if (!File.Exists(_filePath))
        {
            return new List<Patient>();
        }

        try
        {
            string json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<Patient>>(json, _jsonOptions) ?? new List<Patient>();
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException("Plik danych pacjentów ma nieprawidłowy format.", exception);
        }
        catch (IOException exception)
        {
            throw new InvalidOperationException("Nie można odczytać pliku danych pacjentów.", exception);
        }
    }

    public void SavePatients(IEnumerable<Patient> patients)
    {
        try
        {
            string json = JsonSerializer.Serialize(patients, _jsonOptions);
            File.WriteAllText(_filePath, json);
        }
        catch (IOException exception)
        {
            throw new InvalidOperationException("Nie można zapisać danych pacjentów.", exception);
        }
    }
}
