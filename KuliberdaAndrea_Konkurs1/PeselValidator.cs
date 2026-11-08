namespace ClinicManagement;

public static class PeselValidator
{
    public static bool IsValid(string? pesel)
    {
        if (string.IsNullOrWhiteSpace(pesel) || pesel.Length != 11 || !pesel.All(char.IsDigit))
        {
            return false;
        }

        int[] weights = { 1, 3, 7, 9, 1, 3, 7, 9, 1, 3 };
        int sum = 0;

        for (int i = 0; i < weights.Length; i++)
        {
            sum += (pesel[i] - '0') * weights[i];
        }

        int controlDigit = (10 - sum % 10) % 10;
        return controlDigit == pesel[10] - '0';
    }
}
