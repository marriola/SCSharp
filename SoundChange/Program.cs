using SoundChange.Nodes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundChange
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("usage: SoundChange filename");
                return;
            }

            Console.OutputEncoding = Encoding.UTF8;

            var parser = new Parser(new StreamReader(args[0]));
            var nodes = new List<Node>();

            try
            {
                while (parser.HasNext)
                {
                    var node = parser.Next();
                    if (node == null)
                        continue;

                    nodes.Add(node);

                    Console.WriteLine(node.ToString());
                }
            }
            catch (ParseException ex)
            {
                Console.WriteLine($"Parse error {ex.Message}");
            }
            catch (SyntaxException ex)
            {
                Console.WriteLine($"Syntax error {ex.Message}");
            }
        }
    }
}
