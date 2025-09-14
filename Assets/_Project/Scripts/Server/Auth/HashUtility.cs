using System.Text;

public static class HashUtility
{
    public static string SHA512(string input)
    {
        using (var sha = System.Security.Cryptography.SHA512.Create())
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            byte[] hash = sha.ComputeHash(bytes);
            var sb = new StringBuilder();
            foreach (byte b in hash) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}