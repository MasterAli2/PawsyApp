using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using PawsyApp.KittyColors;

namespace PawsyApp.Utils;

internal class WriteLog
{
    internal static Task Cutely(object msg, (object ContextName, object ContextValue)[] context)
    {
        StringBuilder sb = new(msg.ToString());
        sb.AppendLine();
        var dump = false;

        foreach (var (ContextName, ContextValue) in context)
        {
            if (ContextName is null)
            {
                sb.AppendLine(KittyColor.WrapInColor("ContextName is null", ColorCode.Red));
                dump = true;
                continue;
            }
            if (ContextValue is null)
            {
                sb.AppendLine(KittyColor.WrapInColor("ContextValue is null", ColorCode.Red));
                dump = true;
                continue;
            }


            sb.Append("  ");
            sb.Append(KittyColor.WrapInColor(ContextName.ToString(), ColorCode.Cyan));
            sb.Append(": ");
            sb.AppendLine(ContextValue.ToString());
        }

        if (dump)
        {
            using StreamWriter writer = new("pawsy-errors.log", true);
            writer.WriteLine(sb);
        }

        sb.AppendLine();
        return Normal(sb);
    }

    internal static Task LineNormal(object msg) => Normal(msg.ToString() + "\n");

    internal static Task Normal(object msg)
    {
        Console.Write($"[{KittyColor.WrapInColor("Pawsy!", ColorCode.Magenta)}]  {msg}");
        return Task.CompletedTask;
    }
}
