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
                .ForEach(r => (r as RuleNode).FitUtterancesToKeys(features, categories));

            var rules = nodes
                .Where(x => x is RuleNode)
                .Select(x => (x as RuleNode, new RuleMachine(x as RuleNode, features, categories)))
                .ToList();

            while (true)
            {
                var rule = SelectRule(rules);

                Console.Write("Enter a word to transform: ");
                var inputWord = Console.ReadLine();

                Console.WriteLine(rule.ApplyTo(inputWord));
                Console.WriteLine();
            }
        }

        private static RuleMachine SelectRule(List<(RuleNode rule, RuleMachine machine)> rules)
        {
            for (var i = 0; i < rules.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {rules[i].rule.ToString()}");
            }

            while (true)
            {
                Console.Write("\nSelect a rule: ");
                var answer = Console.ReadLine();

                if (int.TryParse(answer, out int ruleNum) && ruleNum >= 1 && ruleNum <= rules.Count)
                {
                    return rules[ruleNum - 1].machine;
                }
            }
        }
    }
}
