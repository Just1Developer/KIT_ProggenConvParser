// See https://aka.ms/new-console-template for more information

using System.Text;

string[] lines = File.ReadAllLines("/Users/windfury/RiderProjects/ProgrammingSheet5ConvParser/ProgrammingSheet5ConvParser/content_test.txt");

Conversation current = new Conversation();
foreach (var line in lines)
{
    if (line.StartsWith("More information about"))
    {
        Console.WriteLine(current.ToString());
        current = new Conversation();
        continue;
    }
    
    if (line.Trim().Length == 0) continue;
    
    current.Add(line);
}

internal class Conversation
{
    private List<string> lines = new ();
    public Stage Stage = Stage.Title;
    
    internal void Add(string s)
    {
        if (s.StartsWith("Your program unexpectedly")) return;
        lines.Add(s);
    }

    internal int size()
    {
        return lines.Count;
    }

    public override string ToString()
    {
        StringBuilder builder = new StringBuilder();
        string testFall = $"        // {lines[0].Replace(" fehlgeschlagen", "")}";
        StringBuilder args = new StringBuilder();
        string[] argsString = lines[lines.Count - 1].Substring(1, lines[lines.Count - 1].Length - 2).Split(", ");
        args.Append("        history = new InteractionHistory(");
        foreach (string arg in argsString)
        {
            args.Append('"').Append(arg).Append('"').Append(", ");
        }
        args.Remove(args.ToString().Length - 2, 2);    // Remove last 2 chars
        args.Append(");");
        
        builder.AppendLine(testFall).AppendLine(args.ToString())
            .AppendLine("        history.add(Main.LAUNCH_CHECK, Main.NONE);");

        string expectedOutput;

        if (lines[1].StartsWith("Expected:"))
        {
            expectedOutput = lines[1].Split(" (System.out) that is \"")[1].Split('"')[0];
        }
        else if (lines[1].StartsWith("Timeout"))
        {
            expectedOutput = "        history.addQuitCommand();";
        }
        else expectedOutput = "// Generation Error";

        StringBuilder outputBuilder = new StringBuilder();
        bool outputPrinted = false;
        
        for (int i = 3; i < lines.Count - 2; ++i)
        {
            string l = lines[i];
            string lPure = l.Substring(7, l.Length - 9);    // Remove also \n at the end
            if (l.StartsWith("-->"))
            {
                outputPrinted = false;
                if (outputBuilder.Length != 0)
                {
                    ExportBuilder();
                }
                outputBuilder.Append($"        history.add(\"{lPure}\", \"");
            }
            else if (l.StartsWith("<"))
            {
                if (outputPrinted) outputBuilder.Append("%n");
                else outputPrinted = true;
                outputBuilder.Append(lPure);
            }
        }

        if (!outputPrinted)
        {
            if (outputBuilder.ToString().Contains(".add(\"quit\""))
                builder.AppendLine(expectedOutput);
            else
            {
                outputBuilder.Append(expectedOutput);
                ExportBuilder();
            }
        }

        void ExportBuilder()
        {
            string outp;
            if (outputBuilder.ToString().Contains("Error, "))
            {
                outp = outputBuilder.ToString().Replace("Error, ", "%s");
                outp += '"';
                outp += ".formatted(Main.ERROR_PREFIX));";
            }
            else outp = outputBuilder.Append("\");").ToString();
            builder.AppendLine(outp);
            outputBuilder.Clear();
        }
        
        /*
        foreach (var line in lines)
        {
            Console.WriteLine(line);
        }
        */

        return builder.ToString();
    }
}

internal enum Stage
{
    Title, InputReq, Conv, Args, MoreInfo
}