using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;

internal static class Program
{
    private static readonly string OutputPath = "./output/";
    private static readonly string OutputFilename = "GeneratedRules.json";
    private static readonly char[] Vowels = ['a', 'e', 'i', 'o', 'u'];

    /// <summary>
    /// Regex \b (word boundary) doesn't work when the word begins or ends with an accented character, this works better
    /// </summary>
    private static readonly string RegexWordBoundaryClause = @"(?=^|\s|\b)";

    private static void Main(string[] args)
    {
        Console.WriteLine("Generating replacement rules...");

        List<JsonReplacementGroup> phase1Groups = [];
        List<JsonReplacementGroup> phase2Groups = [];

        //TODO: Handle alternate characters in rules e.g. fancy apostrope, letters with accents?

        foreach (var Rule in RuleList.Rules)
        {
            IEnumerable<ReplacementRule> Rules = [Rule];

            /*
            Original rule:
            [
                A or B -> C
                C or D -> A
                Aplural or Bplural -> Cplural
                Cplural or Dplural -> Aplural
            ]

            === Stage 1: Handle plurals

            Split each rule with plural handling into two rules, one for the singular inputs and one for the plural inputs
            [
                A or B -> C
                C or D -> A
            ],
            [
                Aplural or Bplural -> Cplural
                Cplural or Dplural -> Aplural
            ]

            === Stage 2: Handle bidirectional

            Split each bidirectional rule into two rules, one for forward and one for reverse

            [
                A or B -> C
            ],
            [
                C or D -> A
            ],
            [
                Aplural or Bplural -> Cplural
            ],
            [
                Cplural or Dplural -> Aplural
            ]

            Stage 3: Handle multi-input

            Split each rule with multiple inputs into one rule per input (we could handle this within a single regex, but that
            would make subsequent transformations more difficult. Might be possible if I can find a library that will construct
            a regex from an 'expression tree' type structure)

            [
                A -> C
            ],
            [
                B -> C
            ],
            [
                C -> A
            ],
            [
                D -> A
            ],
            [
                Aplural -> Cplural
            ],
            [
                Bplural -> Cplural
            ],
            [
                Cplural -> Aplural
            ],
            [
                Dplural -> Aplural
            ]

            Stage 4: Handle capitalisation

            Split each rule into multiple rules matching  and outputting different capitalisation cases

            Stage 5: Handle replace via UID

            Split each rule into two rules, one which transforms the input into a UID and another which transforms that UID into the original output.
            */

            Rules = HandlePlurals(Rules);
            Rules = HandleBidirectional(Rules);
            Rules = HandleMultiInput(Rules);

            IEnumerable<JsonSubstitution> substitutions = HandleCapitalisation(Rules);

            List<JsonSubstitution> phase1substitutions = [];
            List<JsonSubstitution> phase2substitutions = [];
            if(Rule.ReplaceViaUid)
            {
                // UID stage
                (phase1substitutions, phase2substitutions) = HandleReplaceViaUid(substitutions);
            }
            else
            {
                phase1substitutions = substitutions.ToList();
            }
            

            phase1Groups.Add(new JsonReplacementGroup
            {
                Name = $"{Rule.Inputs[0]} -> {Rule.Output}",
                Substitutions = phase1substitutions
            });

            if(phase2substitutions.Any())
            {
                phase2Groups.Add(new JsonReplacementGroup
                {
                    Name = $"{Rule.Inputs[0]} -> {Rule.Output}",
                    Substitutions = phase2substitutions
                });
            }
        }

        JsonReplacementRules JsonRules = new()
        {
            Groups = [.. phase1Groups, .. phase2Groups]
        };

        Directory.CreateDirectory(OutputPath);
        JsonSerializerOptions options = new(JsonSerializerOptions.Web)
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            }
        };
        using (FileStream stream = File.Open(Path.Join(OutputPath, OutputFilename), FileMode.Create))
        {
            JsonSerializer.Serialize(stream, JsonRules, options);
        }
        
        Console.WriteLine(JsonSerializer.Serialize(JsonRules, options));
    }

    /// <summary>
    /// Takes unidirectional rules and outputs one rule for each different input entry
    /// </summary>
    /// <returns></returns>
    private static IEnumerable<ReplacementRule> HandleMultiInput(IEnumerable<ReplacementRule> rules)
    {
        foreach(var rule in rules)
        {
            if(rule.Bidirectional || rule.InputsPlural.Any())
            {
                throw new Exception("Rules must be unidirectional");
            }

            foreach(var (i, input) in rule.Inputs.Index())
            {
                yield return rule with { Inputs = [input] };
            }
        }
    }

    private static IEnumerable<ReplacementRule> HandleBidirectional(IEnumerable<ReplacementRule> rules)
    {
        foreach(var rule in rules)
        {
            if(rule.InputsPlural.Any() || rule.ReverseInputsPlural.Any())
            {
                throw new Exception("Rules must not have plural handling");
            }

            yield return rule with 
            {
                ReverseInputs = [rule.ReverseInputs[0]],
                Bidirectional = false
            };

            if(rule.Bidirectional)
            {
                if(!rule.ReplaceViaUid)
                {
                    throw new Exception();
                }

                yield return rule with
                {
                    Inputs = rule.ReverseInputs,
                    ReverseInputs = [rule.Inputs[0]],

                    Bidirectional = false
                };
            }
        }
    }

    private static IEnumerable<ReplacementRule> HandlePlurals(IEnumerable<ReplacementRule> rules)
    {
        foreach(var rule in rules)
        {
            // Return the rule as-is to handle the singular
            yield return rule with
            {
                InputsPlural = [],
                ReverseInputsPlural = []
            };

            // Return a rule that handles only the plural
            if(rule.InputsPlural.Any() && rule.ReverseInputsPlural.Any())
            {
                yield return rule with
                {
                    Inputs = rule.InputsPlural,
                    ReverseInputs = rule.ReverseInputsPlural,
                    InputsPlural = [],
                    ReverseInputsPlural = []
                };
            }
        }
    }

    private static IEnumerable<JsonSubstitution> HandleCapitalisation(IEnumerable<ReplacementRule> rules)
    {
        foreach(var rule in rules)
        {
            if(rule.Inputs.Count > 1 || rule.ReverseInputs.Count > 1 || rule.Bidirectional)
            {
                throw new Exception("Rules must have be unidirectional with single input");
            }

            string input = rule.Inputs[0];

            if (rule.CapitalisationHandling == CapitalisationHandling.Preserve)
            {
                // All uppercase
                yield return new JsonSubstitution
                {
                    CaseSensitive = true,
                    Input = GetRegexForInput(rule, input.ToUpperInvariant()),
                    Output = rule.Output.ToUpperInvariant()
                };

                // First letter only uppercase
                yield return new JsonSubstitution
                {
                    CaseSensitive = true,
                    Input = GetRegexForInput(rule, char.ToUpperInvariant(input[0]) + string.Concat(input.Skip(1))),
                    Output = char.ToUpperInvariant(rule.Output[0]) + string.Concat(rule.Output.Skip(1)),
                };

                // Any other input case combination results in all lowercase output
                yield return new JsonSubstitution
                {
                    CaseSensitive = false,
                    Input = GetRegexForInput(rule, input.ToLowerInvariant()),
                    Output = rule.Output.ToLowerInvariant()
                };
            }
            else
            {
                yield return new JsonSubstitution
                {
                    CaseSensitive = rule.CapitalisationHandling == CapitalisationHandling.MatchExact,
                    Input = input,
                    Output = rule.Output
                };
            }
        }
    }

    private static (List<JsonSubstitution> phase1, List<JsonSubstitution> phase2) HandleReplaceViaUid(IEnumerable<JsonSubstitution> substitutions)
    {
        List<JsonSubstitution> phase1 = [];
        List<JsonSubstitution> phase2 = [];

        foreach(var substitution in substitutions)
        {
            string uid = substitution.GetHashCode().ToString();
            phase1.Add(substitution with
            {
                 Output = uid,
                 OutputType = SubstitutionOutputType.Text
            });
            phase2.Add(substitution with 
            {
                Input = uid
            });
        }

        return (phase1, phase2);
    }

    private static string GetRegexForInput(ReplacementRule Rule, string InputString)
    {
        if(Rule.WholeWords)
        {
            return $@"{RegexWordBoundaryClause}{InputString}{RegexWordBoundaryClause}";
        }
        else
        {
            return InputString;
        }
    }
}