using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace ProgrammingSheet5ConvParser;

public class Program2
{
    static string[] lines;

    private static Dictionary<string, InfoWrapper> content;
    
    public static void run()
    {
        lines = File.ReadAllLines("/Users/windfury/RiderProjects/ProgrammingSheet5ConvParser/ProgrammingSheet5ConvParser/code.txt");

        // Add some defaults
        content = new Dictionary<string, InfoWrapper>();
        content.Add("Alice", new InfoWrapper("Alice", "NAME"));
        content.Add("Bob", new InfoWrapper("Alice", "NAME"));
        content.Add("Ezio", new InfoWrapper("Alice", "NAME"));
        content.Add("Desmond", new InfoWrapper("Alice", "NAME"));
        
        
        // First, read all pattern for input:

        foreach (string line in lines)
        {
            if (!line.Trim().StartsWith("history")) continue;
            if (line.Trim().StartsWith("history = "))
            {
                continue;
            }

            if (!line.Trim().StartsWith("history.add(")) continue;

            var split = line.Split('"');
            string content1 = split[1];

            string[] contentSplit = content1.Contains(" ") ? content1.Split(' ') : new[] { content1 };
            for (int i = 0; i < contentSplit.Length; ++i)
            {
                string word = contentSplit[i], type, formatType = "%s";

                if (content.ContainsKey(word))
                {
                    content[word].Occurences++;
                    continue;
                }

                bool isNumber;
                try
                {
                    int.Parse(word);
                    isNumber = true;
                }
                catch (FormatException)
                {
                    isNumber = false;
                }

                if (i == 0) type = "COMMAND";
                else if (isNumber)
                {
                    type = "NUMBER";
                    formatType = "%d";
                }
                else type = "VARIABLE";

                content.Add(word, new InfoWrapper(type, formatType));
            }
        }

        // Now, Write all of it and replace
        
        
        foreach (string line in lines)
        {
            if (!line.Trim().StartsWith("history")) continue;
            if (line.Trim().StartsWith("history = "))
            {
                
                
                continue;
            }
            if (!line.Trim().StartsWith("history.add(")) continue;

            var split = line.Split('"');
            string content1 = split[1], content2 = split[3];
            
            StringBuilder builder = new StringBuilder();
            builder.Append("        history.add(\"");

            // Content 1:
            
            string[] contentSplit = content1.Contains(" ") ? content1.Split(' ') : new [] { content1 };
            StringBuilder formatBuilder = new StringBuilder("\"");
            for (int i = 0; i < contentSplit.Length; ++i)
            {
                string word = contentSplit[i];
                if (!content.ContainsKey(word))
                {
                    builder.Append(word);
                    if (i < contentSplit.Length - 1) builder.Append(' ');
                    continue;
                }

                InfoWrapper wrapper = content[word];
                builder.Append(wrapper.FormatType);
                if (i < contentSplit.Length - 1) builder.Append(' ');
                // Format
                if (!formatBuilder.ToString().EndsWith('"')) formatBuilder.Append(", ");
                else formatBuilder.Append(".formatted(");
                formatBuilder.Append(wrapper.SpecialName);
            }
            
            // Middle:
            
            builder.Append(formatBuilder).Append(", \"");
            
            // Content 2:
            
            // Some trivial cases
            if (Regex.IsMatch(content2, "OK, ([A-Za-z]+)'s turn"))
            {
                string name = content2.Replace("OK, ", "").Replace("'s turn", "");
                builder.Remove(builder.ToString().Length - 1, 1);
                builder.Append($"getTurnString({content[name].SpecialName})");
            }
            else if (content2 == "OK, all players are now ready to evaluate!")
            {
                builder.Remove(builder.ToString().Length - 1, 1);
                builder.Append("EVALUATION_READY_STRING");
            }
            else
            {

                string tempFormat = content2 + "";

                if (content2.Contains(';'))
                {
                    contentSplit = content2.Split(';');
                    formatBuilder.Clear();

                    String? next = content2, changedWord = null;
                    while ((next = NextContent(changedWord, out changedWord)) != null)
                    {
                        // Processing next unit
                    }

                    string? NextContent(string? word, out string? changedWord)
                    {
                        if (word == null || word.Length == 0) { changedWord = null; return null; }
                        int nextIndexSemi = word.IndexOf(';');
                        int nextIndexEmpty = word.IndexOf(' ');

                        if (nextIndexSemi == -1)
                        {
                            if (nextIndexEmpty == -1)
                            {
                                changedWord = "";
                                return word;
                            }

                            changedWord = word.Substring(nextIndexEmpty + 1);
                            return word.Substring(0, nextIndexEmpty);
                        }

                        int index = Math.Min(nextIndexEmpty, nextIndexSemi);
                        
                        changedWord = word.Substring(index + 1);
                        return word.Substring(0, index);
                    }
                    
                    for (int i = 0; i < contentSplit.Length; ++i)
                    {
                        string word = contentSplit[i], lineBreak = "";
                        
                        
                        word.IndexOf(';');
                        //if (word.StartsWith("%n"))
                        
                        if (!content.ContainsKey(word))
                        {
                            builder.Append(word);
                            if (i < contentSplit.Length - 1) builder.Append(';');
                            continue;
                        }
                        
                        
                    }
                }
                
                contentSplit = content2.Contains(" ") ? content2.Split(' ') : new [] { content2 };
                formatBuilder.Clear();
                for (int i = 0; i < contentSplit.Length; ++i)
                {
                    string word = contentSplit[i];
                
                    if (!content.ContainsKey(word))
                    {
                        builder.Append(word);
                        if (i < contentSplit.Length - 1) builder.Append(' ');
                        continue;
                    }
                }
            }

            builder.Append(");");
            Console.WriteLine(builder.ToString());
        }
        
        
        // Print all variables
        Console.WriteLine("---------");
        Console.WriteLine("protected static final String EVALUATION_READY_STRING = \"OK, all players are now ready to evaluate!\";");
        foreach (var wrapper in content.Values)
        {
            Console.WriteLine($"protected static final String {wrapper.SpecialName} = \"{wrapper.Variable}\"");
        }
    }

    internal class InfoWrapper
    {
        internal int Occurences = 1;
        internal string PrefixType = "";
        internal string FormatType = "%s";
        internal string SpecialName = "";
        internal string Variable = "";

        public InfoWrapper(string variable, string prefixType, string formatType = "%s")
        {
            Variable = variable;
            PrefixType = prefixType;
            FormatType = formatType;

            SpecialName = prefixType.ToUpper() + "_" + variable.ToUpper().Replace(";", "_").Replace(" ", "_");
        }
    }
}