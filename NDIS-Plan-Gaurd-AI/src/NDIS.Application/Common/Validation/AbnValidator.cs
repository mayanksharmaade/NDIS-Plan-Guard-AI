namespace NDIS.Application.Common.Validation;

public static class AbnValidator
{
    private static readonly int[] Weights =
    [
        10, 1, 3, 5, 7, 9, 11, 13, 15, 17, 19
    ];

    public static string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return new string(value.Where(char.IsDigit).ToArray());
    }

    public static bool IsValid(string value)
    {
        var abn = Normalize(value);

        if (abn.Length != 11)
        {
            return false;
        }

        var digits = abn
            .Select(x => x - '0')
            .ToArray();

        digits[0]--;

        var sum = 0;

        for (var i = 0; i < digits.Length; i++)
        {
            sum += digits[i] * Weights[i];
        }

        return sum % 89 == 0;
    }
}
