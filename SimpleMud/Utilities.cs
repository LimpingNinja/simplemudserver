using System.Text;
namespace SimpleMud;

public class Utilities
{
    private static Dictionary<char, byte[]> _colorCodes = new Dictionary<char, byte[]>
    {
        { 'y', new byte[] { 27, 91, 51, 51, 109 } }, // "\033[33m"
        { 'b', new byte[] { 27, 91, 51, 52, 109 } }, // "\033[34m"
        { 'g', new byte[] { 27, 91, 51, 50, 109 } }, // "\033[32m"
        { 'r', new byte[] { 27, 91, 51, 49, 109 } }, // "\033[31m"
        { 'c', new byte[] { 27, 91, 51, 54, 109 } }, // "\033[36m"
        { 'p', new byte[] { 27, 91, 51, 53, 109 } }, // "\033[35m"
        { 'd', new byte[] { 27, 91, 51, 48, 109 } }, // "\033[30m"
        { 'w', new byte[] { 27, 91, 51, 55, 109 } }, // "\033[37m"
        { 'Y', new byte[] { 27, 91, 49, 59, 51, 51, 109 } }, // "\033[1;33m"
        { 'B', new byte[] { 27, 91, 49, 59, 51, 52, 109 } }, // "\033[1;34m"
        { 'G', new byte[] { 27, 91, 49, 59, 51, 50, 109 } }, // "\033[1;32m"
        { 'R', new byte[] { 27, 91, 49, 59, 51, 49, 109 } }, // "\033[1;31m"
        { 'C', new byte[] { 27, 91, 49, 59, 51, 54, 109 } }, // "\033[1;36m"
        { 'P', new byte[] { 27, 91, 49, 59, 51, 53, 109 } }, // "\033[1;35m"
        { 'D', new byte[] { 27, 91, 49, 59, 51, 48, 109 } }, // "\033[1;30m"
        { 'W', new byte[] { 27, 91, 49, 59, 51, 55, 109 } }, // "\033[1;37m"
        { 'n', new byte[] { 27, 91, 48, 109 } }  // "\033[0m"
    };
    
    public static byte[] ConvertToAnsi(string input)
    {
        var output = new MemoryStream();
        int i = 0;
        while (i < input.Length)
        {
            if (input[i] == '#')
            {
                i++;
                if (i < input.Length && _colorCodes.TryGetValue(input[i], out var colorCode))
                {
                    output.Write(colorCode, 0, colorCode.Length);
                    i++;
                    continue;
                }
            }

            int start = i;
            while (i < input.Length && input[i] != '#')
            {
                i++;
            }

            var bytes = Encoding.UTF8.GetBytes(input, start, i - start);
            output.Write(bytes, 0, bytes.Length);
        }

        return output.ToArray();
    }
}