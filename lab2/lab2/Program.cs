/*
 * 23.	Зашифровать и расшифровать сообщение, содержащее символы
 * кодовой таблицы ASCII с помощью шифра Вернама. Ключом 
 * является набор случайных символов общей длиной не менее 10. 
 * 
 * 0 XOR 0 = 0
 * 0 XOR 1 = 1
 * 1 XOR 0 = 1
 * 1 XOR 1 = 0 
 */
string text = File.ReadAllText("input.txt");
string key = File.ReadAllText("keys.txt");


Console.WriteLine("Шифр Вернама-----------------------------------------------------------------------");
Console.WriteLine("Исходный текст:\n'" + text + "'");
Console.WriteLine("Ключ:\n'" + key + "'");
string encrypt = VernamСipher(text, key);
File.WriteAllText("encrypted.txt", encrypt);
Console.WriteLine("Шифрование Вернама:\n'" + encrypt + "'");
string decrypt = VernamСipher(encrypt, key);
Console.WriteLine("Дешифрованный текст (шифр Вернама):\n'" + decrypt + "'");
File.WriteAllText("decrypted.txt", decrypt);

static string VernamСipher(string text, string key)
{
    
    if (key.Length < 10)
    {
        throw new Exception("Не соблюдается условие задания: длина ключа < 10");
    }
    if (key.Length < text.Length)
    {
        string blockKey = key;
        DebugWrite($"Длина ключа = {key.Length}, меньше длины текста = {text.Length}\nДублируем ключ до тех пор, пока его длина не будет >= {text.Length}\n");
        while (key.Length < text.Length)
        {
            key += blockKey;
        }
        DebugWrite($"Расширенный ключ: '{key}'\n");
    }

    string result = string.Empty;
    for (int i = 0; i < text.Length; i++)
    {
        byte textByte = (byte)text[i];
        byte keyByte = (byte)key[i];
        byte resultByte = (byte)(textByte ^ keyByte);

        string textBinary = Convert.ToString(textByte, 2).PadLeft(8, '0');
        string keyBinary = Convert.ToString(keyByte, 2).PadLeft(8, '0');
        string resultBinary = Convert.ToString(resultByte, 2).PadLeft(8, '0');
        DebugWrite($"Шифруем символ '{text[i]}'({textByte})\tпо символу '{key[i]}'({keyByte}):\t'{textBinary}' XOR '{keyBinary}' = '{resultBinary}' -> '{(char)resultByte}'({resultByte})\n");

        result += (char)resultByte;
    }
    return result;
}

static void DebugWrite(string message)
{
#if DEBUG
    Console.Write(message);
#endif
}