using SoundChange.Lexer;
using SoundChange.Parser;
using SoundChange.Parser.Nodes;
using SoundChange.StateMachines;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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

            var parser = new Parser.Parser(new StreamReader(args[0]));
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
            catch (UnexpectedTokenException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (ParseException ex)
            {
                Console.WriteLine($"Parse error {ex.Message}");
            }
            catch (SyntaxException ex)
            {
                Console.WriteLine($"Syntax error {ex.Message}");
            }

            var features = nodes
                .Select(x => x as FeatureSetNode)
                .Where(x => x != null)
                .ToList();

            var categories = nodes
                .Select(x => x as CategoryNode)
                .Where(x => x != null)
                .ToList();

            nodes
                .Where(x => x is RuleNode)
                .ToList()
                .ForEach(rule => ((RuleNode)rule).FitUtterancesToKeys(features, categories));

            var rules = nodes
                .Where(x => x is RuleNode)
                .Select(x => new RuleMachine(x as RuleNode, features, categories))
                .ToList();

            var s = rules[1].ApplyTo("kraka");
        }
    }
}
