using System.Security.Cryptography;
using System.Text;

namespace Coka.Social.Listening.Infra.Helpers;

/// <summary>
/// CLI tool: dotnet run -- encrypt "your-connection-string"
/// Used to generate encrypted values for appsettings.json
/// </summary>
public static class EncryptionCli
{
    public static void Run(string[] args)
    {
        if (args.Length < 2 || args[0] != "encrypt")
        {
            Console.WriteLine("Usage: dotnet run -- encrypt \"<connection-string>\"");
            return;
        }

        Console.WriteLine(EncryptionHelper.Encrypt(args[1]));
    }
}
