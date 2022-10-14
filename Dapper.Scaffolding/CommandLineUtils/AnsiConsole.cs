using System;
using System.IO;

namespace Microsoft.Extensions.CommandLineUtils
{
  public class AnsiConsole
  {
    private int _boldRecursion;
    private bool _useConsoleColor;

    private AnsiConsole(TextWriter writer, bool useConsoleColor)
    {
      this.Writer = writer;
      this._useConsoleColor = useConsoleColor;
      if (!this._useConsoleColor)
        return;
      this.OriginalForegroundColor = Console.ForegroundColor;
    }

    public static AnsiConsole GetOutput(bool useConsoleColor) => new AnsiConsole(Console.Out, useConsoleColor);

    public static AnsiConsole GetError(bool useConsoleColor) => new AnsiConsole(Console.Error, useConsoleColor);

    public TextWriter Writer { get; }

    public ConsoleColor OriginalForegroundColor { get; }

    private void SetColor(ConsoleColor color) => Console.ForegroundColor = Console.ForegroundColor & ConsoleColor.DarkGray | color & ConsoleColor.Gray;

    private void SetBold(bool bold)
    {
      this._boldRecursion += bold ? 1 : -1;
      if (this._boldRecursion > 1 || this._boldRecursion == 1 && !bold)
        return;
      Console.ForegroundColor ^= ConsoleColor.DarkGray;
    }

    public void WriteLine(string message)
    {
      if (!this._useConsoleColor)
      {
        this.Writer.WriteLine(message);
      }
      else
      {
        int startIndex1 = 0;
        while (true)
        {
          int num = message.IndexOf("\u001B[", startIndex1);
          if (num != -1)
          {
            int startIndex2 = num + 2;
            int index = startIndex2;
            while (index != message.Length && message[index] >= ' ' && message[index] <= '?')
              ++index;
            this.Writer.Write(message.Substring(startIndex1, num - startIndex1));
            if (index != message.Length)
            {
              int result;
              if (message[index] == 'm' && int.TryParse(message.Substring(startIndex2, index - startIndex2), out result))
              {
                switch (result)
                {
                  case 1:
                    this.SetBold(true);
                    break;
                  case 22:
                    this.SetBold(false);
                    break;
                  case 30:
                    this.SetColor(ConsoleColor.Black);
                    break;
                  case 31:
                    this.SetColor(ConsoleColor.Red);
                    break;
                  case 32:
                    this.SetColor(ConsoleColor.Green);
                    break;
                  case 33:
                    this.SetColor(ConsoleColor.Yellow);
                    break;
                  case 34:
                    this.SetColor(ConsoleColor.Blue);
                    break;
                  case 35:
                    this.SetColor(ConsoleColor.Magenta);
                    break;
                  case 36:
                    this.SetColor(ConsoleColor.Cyan);
                    break;
                  case 37:
                    this.SetColor(ConsoleColor.Gray);
                    break;
                  case 39:
                    this.SetColor(this.OriginalForegroundColor);
                    break;
                }
              }
              startIndex1 = index + 1;
            }
            else
              goto label_23;
          }
          else
            break;
        }
        this.Writer.Write(message.Substring(startIndex1));
label_23:
        this.Writer.WriteLine();
      }
    }
  }
}
