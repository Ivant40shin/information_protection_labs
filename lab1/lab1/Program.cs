const string ABC = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ ."; // 54 символа
const int rows = 6;
const int cols = 9;


string text = File.ReadAllText( "input.txt");
string[] keys = File.ReadAllLines( "keys.txt");
string playfairKey = keys[0];
int caesarKey = int.Parse(keys[1]);

                   
Console.WriteLine("Шифр Плейфейра-----------------------------------------------------------------------");
Console.WriteLine("Исходный текст:\n'" + text + "'");
string pEncrypt = PlayfairEncrypt(text, playfairKey);
File.WriteAllText("playfair_encrypted.txt", pEncrypt);
Console.WriteLine("Шифрование Плейфера:\n'" + pEncrypt + "'");
string pDecrypt = PlayfairDecrypt(pEncrypt, playfairKey);
Console.WriteLine("Дешифрованный текст (шифр Плейфера):\n'" + pDecrypt + "'");
File.WriteAllText("playfair_decrypted.txt", pDecrypt);
                   
Console.WriteLine("Шифр Цезаря--------------------------------------------------------------------------");
Console.WriteLine("Исходный текст:\n'" + text + "'");
string cEncrypt = CaesarEncrypt(text, caesarKey);
File.WriteAllText("caesar_encrypted.txt", cEncrypt);
Console.WriteLine("Шифрование Цезаря:\n'" + cEncrypt + "'");
string cDecrypt = CaesarDecrypt(cEncrypt, caesarKey);
Console.WriteLine("Дешифрованный текст (шифр Цезаря):\n'" + cDecrypt + "'");
File.WriteAllText("caesar_decrypted.txt", cDecrypt);



/* удаление повторяющихся символов
 */
static string NormalizeKey(string key)
{
    if (string.IsNullOrEmpty(key)) return key;

    string result = new(key.Where(c => ABC.Contains(c))
        .Distinct()
        .ToArray());

    DebugWrite("Удаление повторяющихся символов в ключе.\nДо:\n" + key + "\nПосле:\n" + result + "\n");
    return result;
}

/* построение таблицы Плейфейра
 */
static char[,] BuildTable(string key)
{
    string normalKey = NormalizeKey(key);
    DebugWrite("Построение таблицы Плейфейра.\n");

    string otherABC = normalKey;
    foreach (char c in ABC)
    {
        if (!normalKey.Contains(c))
        {
            otherABC += c;
        }
    }

    char[,] table = new char[rows, cols];
    for (int i = 0; i < rows; i++)
    {
        for (int j = 0; j < cols; j++)
        {
            table[i, j] = otherABC[i * cols + j];
            DebugWrite(table[i, j] + " ");
        }
        DebugWrite("\n");
    }

    return table;
}

/* разбитие текста на пары
 * если последний символ, то добавляется пробел для создания пары
 * если идут 2 символа подряд, то в пару идет первый символ и добавляется пробел
 */
static List<string> PrepareText(string text)
{
    DebugWrite("Текст до разбития:\n'" + text + "'\n");
    string clean = new string(text.Where(c => ABC.Contains(c)).ToArray());
    if (clean.Length % 2 != 0) clean += " ";

    List<string> preparedText = new List<string>();
    for (int i = 0; i < clean.Length; i += 2)
    {
        if (i + 1 >= clean.Length)
            preparedText.Add(clean[i] + " ");
        else if (clean[i] == clean[i + 1])
        {
            preparedText.Add(clean[i] + " ");
            i--;
        }
        else
            preparedText.Add(clean.Substring(i, 2));
    }
    DebugWrite("Разбитый на пары текст:\n'" + string.Join("'", preparedText) + "'\n");
    return preparedText;
}

/* шифрование пары алгоритмом Плейфейра 
 * если символы в одной строке: при шифровании сдвигаются вправо
 * если символы в одном столбце: при шифровании сдвигаются вниз
 * если не лежат ни в одной строке/столбце - 1 правило Трисемуса:
 * Первой буквой биграммы шифртекста становится буква, расположен
 * ная в той же строке, что и первая буква исходной биграммы, и в
 * том же столбце, что и вторая буква открытого текста. 
 */
static string EncryptPair(string pair, char[,] table)
{
    (int rows1, int cols1) = FindPos(pair[0], table);
    (int rows2, int cols2) = FindPos(pair[1], table);
    DebugWrite("Шифруем пару: '" + pair + "'\n");

    if (rows1 == rows2)
    {
        cols1 = (cols1 + 1) % cols;
        cols2 = (cols2 + 1) % cols;
        DebugWrite($"'{pair[0]}' и '{pair[1]}' в одной строке -> сдвиг вправо: '{pair[0]}'->'{table[rows1, cols1]}', '{pair[1]}'->'{table[rows2, cols2]}'\n");
    }
    else if (cols1 == cols2)
    {
        rows1 = (rows1 + 1) % rows;
        rows2 = (rows2 + 1) % rows;
        DebugWrite($"'{pair[0]}' и '{pair[1]}' в одном столбце -> сдвиг вниз: '{pair[0]}'->'{table[rows1, cols1]}', '{pair[1]}'->'{table[rows2, cols2]}'\n");
    }
    else
    {
        (cols2, cols1) = (cols1, cols2);
        DebugWrite($"'{pair[0]}' и '{pair[1]}' не лежат ни в одной строке/столбце -> 1 правило Трисемуса: '{pair[0]}'->'{table[rows1, cols1]}', '{pair[1]}'->'{table[rows2, cols2]}'\n");
    }

    return $"{table[rows1, cols1]}{table[rows2, cols2]}";
}

/* дешифрование пары алгоритмом Плейфейра 
 * если символы в одной строке: при шифровании сдвигаются влево
 * если символы в одном столбце: при шифровании сдвигаются вверх
 * если не лежат ни в одной строке/столбце - 1 правило Трисемуса:
 * Первой буквой биграммы шифртекста становится буква, расположен
 * ная в той же строке, что и первая буква исходной биграммы, и в
 * том же столбце, что и вторая буква открытого текста. 
 */
static string DecryptPair(string pair, char[,] table)
{
    (int rows1, int cols1) = FindPos(pair[0], table);
    (int rows2, int cols2) = FindPos(pair[1], table);
    DebugWrite("Дешифруем пару: '" + pair + "'\n");

    if (rows1 == rows2)
    {
        cols1 = (cols1 - 1 + cols) % cols;
        cols2 = (cols2 - 1 + cols) % cols;
        DebugWrite($"'{pair[0]}' и '{pair[1]}' в одной строке -> сдвиг влево: '{pair[0]}'->'{table[rows1, cols1]}', '{pair[1]}'->'{table[rows2, cols2]}'\n");
    }
    else if (cols1 == cols2)
    {
        rows1 = (rows1 - 1 + rows) % rows;
        rows2 = (rows2 - 1 + rows) % rows;
        DebugWrite($"'{pair[0]}' и '{pair[1]}' в одном столбце -> сдвиг вверх: '{pair[0]}'->'{table[rows1, cols1]}', '{pair[1]}'->'{table[rows2, cols2]}'\n");
    }
    else
    {
        (cols2, cols1) = (cols1, cols2);
        DebugWrite($"'{pair[0]}' и '{pair[1]}' не лежат ни в одной строке/столбце -> 1 правило Трисемуса: '{pair[0]}'->'{table[rows1, cols1]}', '{pair[1]}'->'{table[rows2, cols2]}'\n");
    }

    return $"{table[rows1, cols1]}{table[rows2, cols2]}";
}

/* Возвращает индексы символа в таблице
 */
static (int, int) FindPos(char c, char[,] table)
{
    for (int i = 0; i < rows; i++)
        for (int j = 0; j < cols; j++)
            if (table[i, j] == c) return (i, j);
    return (-1, -1);
}

/* система шифрования Плейфейра
 */
static string PlayfairEncrypt(string text, string key)
{
    char[,] table = BuildTable(key);
    var prepared = PrepareText(text);

    List<string> result = new List<string>();
    DebugWrite("Шифруем пары: '" + string.Join("'", prepared) + "'\n");

    foreach (var pair in prepared)
        result.Add(EncryptPair(pair, table));

    DebugWrite("Зашифрованный текст: '" + string.Join("", result) + "'\n");

    return string.Join("", result);
}

static string PlayfairDecrypt(string text, string key)
{
    char[,] table = BuildTable(key);
    List<string> pairs = new List<string>();
    for (int i = 0; i < text.Length; i += 2)
    {
        pairs.Add(text.Substring(i, 2));
    }

    List<string> result = new List<string>();
    DebugWrite("Дешифруем текст: '" + text + "'\n");
    foreach (var pair in pairs)
        result.Add(DecryptPair(pair, table));
    DebugWrite("Дешифрованный текст: '" + string.Join("", result) + "'\n");

    return string.Join("", result);
}

/* система шифрования Цезаря
 * каждый символ текста сдвигается на key позиций вправо
 */
static string CaesarEncrypt(string text, int key)
{
#if DEBUG
    Console.WriteLine($"Ключ = {key}, алфавит:\n{ABC}\nКаждый символ сдвигается на {key} позиций вправо");

    foreach (char c in text)
    {
        int idx = ABC.IndexOf(c);
        int newIdx = (idx + key) % ABC.Length;
        char enc = ABC[newIdx];
        Console.WriteLine($"{c}({idx,-2})->{enc}({newIdx,-2}) // ({idx,-2} + {key}) % {ABC.Length} = {newIdx}");
    }
#endif

    return new string(text.Select(c =>
    {
        int idx = ABC.IndexOf(c);
        if (idx == -1) return c;
        return ABC[(idx + key) % ABC.Length];
    }).ToArray());
}

static string CaesarDecrypt(string text, int key)
{
#if DEBUG
    Console.WriteLine($"Ключ = {key}, алфавит:\n{ABC}\nКаждый символ сдвигается на {key} позиций влево");

    foreach (char c in text)
    {
        int idx = ABC.IndexOf(c);
        int newIdx = (idx - key + ABC.Length) % ABC.Length;
        char dec = ABC[newIdx];
        Console.WriteLine($"{c}({idx,-2})->{dec}({newIdx,-2}) // ({idx,-2} - {key} + {ABC.Length}) % {ABC.Length} = {newIdx}");

    }
#endif

    return new string(text.Select(c =>
    {
        int idx = ABC.IndexOf(c);
        if (idx == -1) return c;
        return ABC[(idx - key + ABC.Length) % ABC.Length];
    }).ToArray());
}

static void DebugWrite(string message)
{
#if DEBUG
    Console.Write(message);
#endif
}