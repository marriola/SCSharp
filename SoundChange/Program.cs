using SoundChange.Lexer;
using SoundChange.Parser;
using SoundChange.Parser.Nodes;
using SoundChange.StateMachines;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Utility.CommandLine;

namespace SoundChange
{
    internal class Program
    {
        [Argument('r', "rules")]
        private static string RulesFile { get; set; }

        [Argument('l', "lexicon")]
        private static string LexiconFile { get; set; }

        [Argument('v', "verbose")]
        private static bool Verbose { get; set; }

        static void Main(string[] args)
        {
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;

            if (args.Length == 0)
            {
                Console.WriteLine("usage: SoundChange [rules] [lexicon]");
                return;
            }

            Arguments.Populate();
            var rules = ParseRules(RulesFile);
            var lexicon = ReadLexicon(LexiconFile);

            using (var writer = new StreamWriter(LexiconFile + ".out"))
            {
                foreach (var word in lexicon)
                {
                    var nextWord = TransformWord(word, rules, out List<string> transformations);
                    writer.WriteLine(nextWord);

                    if (Verbose)
                        transformations.ForEach(t => writer.WriteLine($"    {t}"));
                }
            }
        }

        private static string TransformWord(string inputWord, List<RuleMachine> rules, out List<string> transformations)
        {
            var result = inputWord;
            transformations = new List<string>();

            foreach (var rule in rules)
            {
                var oldWord = result;
                result = rule.ApplyTo(result, out List<string> ruleTransformations);

                if (ruleTransformations.Any())
                {
                    transformations.Add(rule.Rule.ToString());
                    transformations.Add($"    {oldWord} → {result}");
                }

                transformations.AddRange(ruleTransformations
                    .Select(t => $"        {t}"));
            }

            return result;
        }

        private static List<string> ReadLexicon(string path)
        {
            var lexicon = new List<string>();

            using (var reader = new StreamReader(path))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine().Trim();
                    if (line.Length > 0)
                        lexicon.Add(line);
                }
            }

            return lexicon;
        }

        private static List<RuleMachine> ParseRules(string path)
        {
            var parser = new Parser.Parser(new StreamReader(path));
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

            return nodes
                .Where(x => x is RuleNode)
                .Select(x => new RuleMachine(x as RuleNode, features, categories))
                .ToList();
        }

        private static RuleMachine SelectRule(List<RuleMachine> rules)
        {
            for (var i = 0; i < rules.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {rules[i].Rule.ToString()}");
            }

            while (true)
            {
                Console.Write("\nSelect a rule: ");
                var answer = Console.ReadLine();

                if (int.TryParse(answer, out int ruleNum) && ruleNum >= 1 && ruleNum <= rules.Count)
                {
                    return rules[ruleNum - 1];
                }
            }
        }
    }
}
