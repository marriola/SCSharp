using SoundChange.Lexer;
using SoundChange.Parser;
using SoundChange.Parser.Nodes;
using SoundChange.StateMachines.RuleMachine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Utility.CommandLine;

namespace SoundChange
{
    internal class Program
    {
        [Argument('r', "rules")]
        private static string RulesFile { get; set; }

        [Argument('l', "lexicon")]
        private static string LexiconFile { get; set; }

        [Argument('o', "output")]
        private static string OutputFile { get; set; }

        [Argument('v', "verbose")]
        private static bool Verbose { get; set; }

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine(@"usage: SoundChange [--verbose] --rules PATH --lexicon PATH [--output PATH]

Options:
    -v, --verbose       Outputs initial word and transformations applied to each word
    -r, --rules PATH    Specifies the path to the rules file
    -l, --lexicon PATH  Specifies the path to the lexicon file
    -o, --output PATH   Specifies the path to the output file");
                return;
            }

            Arguments.Populate();

            if (LexiconFile == null)
            {
                Console.WriteLine("Error: Lexicon file not specified.");
                return;
            }
            else if (RulesFile == null)
            {
                Console.WriteLine("Error: Rules file not specified.");
                return;
            }

            List<string> lexicon;
            List<RuleMachine> rules;

            try
            {
                lexicon = ReadLexicon(LexiconFile);
                rules = ParseRules(RulesFile);
            }
            catch (Exception e)
            {
                LogException(e);
                return;
            }

            try
            {
                var outputFile = OutputFile ?? Path.ChangeExtension(LexiconFile, ".out" + Path.GetExtension(LexiconFile));

                using (var writer = new StreamWriter(outputFile))
                {
                    var sw = new Stopwatch();
                    sw.Start();

                    foreach (var word in lexicon)
                    {
                        try
                        {
                            var nextWord = TransformWord(word, rules, out List<string> transformations);

                            if (Verbose)
                            {
                                writer.WriteLine($"{word} → {nextWord}");
                                transformations.ForEach(t => writer.WriteLine($"    {t}"));
                            }
                            else
                            {
                                writer.WriteLine(nextWord);
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Unexpected error transforming '{word}': {e.Message}");
                        }
                    }

                    sw.Stop();
                    Console.WriteLine($"Transformed {lexicon.Count} words in {sw.ElapsedMilliseconds} ms");
                }
            }
            catch (Exception e)
            {
                LogException(e);
            }
        }

        private static void LogException(Exception ex)
        {
            Console.WriteLine(ex.Message);
#if DEBUG
            Console.WriteLine(ex.StackTrace);
#endif
        }

        private static string TransformWord(string inputWord, List<RuleMachine> rules, out List<string> transformations)
        {
            var result = inputWord;
            transformations = new List<string>();

            foreach (var rule in rules)
            {
                var oldWord = result;
                result = rule.ApplyTo(result, out List<Transformation> ruleTransformations);

                if (ruleTransformations.Any())
                {
                    transformations.Add(rule.Rule.ToString());
                    transformations.Add($"    {oldWord} → {result}");
                }

                transformations.AddRange(ruleTransformations
                    //.Where(t => !(t is NullTransformation))
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
            var sw = new Stopwatch();
            sw.Start();

            var parser = new Parser.Parser(new StreamReader(path));
            var nodes = new List<Node>();

            while (parser.HasNext)
            {
                try
                {
                    var node = parser.Next();
                    if (node == null)
                        continue;

                    nodes.Add(node);
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
            }

            var features = nodes.OfType<FeatureSetNode>().ToList();
            var categories = nodes.OfType<CategoryNode>().ToList();
            categories.ForEach(c => c.ResolveIncludes(features, categories));

            var ruleNodes = nodes.OfType<RuleNode>().ToList();
            //ruleNodes.ForEach(x => x.FitUtterancesToKeys(features, categories));

            var rules = ruleNodes
                .Select(rule => new RuleMachine(rule, features, categories))
                .ToList();

            sw.Stop();
            Console.WriteLine($"Parsed {rules.Count} rules in {sw.ElapsedMilliseconds} ms");

            return rules;
        }
    }
}
