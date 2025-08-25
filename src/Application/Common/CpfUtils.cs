namespace SalesApp.Application.Common;

public static class CpfUtils
{
  public static bool IsValid(string? cpf)
  {
    if (string.IsNullOrWhiteSpace(cpf)) return false;
    cpf = new string(cpf.Where(char.IsDigit).ToArray());
    if (cpf.Length != 11) return false;
    if (cpf.Distinct().Count() == 1) return false;

    int Calc(string s, int len)
    {
      int sum = 0;
      for (int i = 0; i < len; i++) sum += (len + 1 - i) * (s[i] - '0');
      int r = sum % 11;
      return r < 2 ? 0 : 11 - r;
    }

    var d1 = Calc(cpf, 9);
    var d2 = Calc(cpf, 10);
    return cpf[9] - '0' == d1 && cpf[10] - '0' == d2;
  }
}
