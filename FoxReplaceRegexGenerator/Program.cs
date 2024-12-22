﻿using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;

internal static class Program
{
    private static readonly string OutputPath = "./output/";
    private static readonly string OutputFilename = "GeneratedRules.json";
    private static readonly char[] Vowels = ['a', 'e', 'i', 'o', 'u'];

    private static void Main(string[] args)
    {
        Console.WriteLine("Generating replacement rules...");

        var ReplacementRules = new List<ReplacementRule> {
            // Misc nouns
            new("cheese", "hardmilk"),
            new("milk", "cow juice", outputPlural: "cow juices"),
            new ReplacementRule("allegedly", "probably").NoPluralHandling(),
            new("new study", "recent rumour", "new studies") { Bidirectional = true },
            new("rebuild", "avenge") { Bidirectional = true },
            new("text", "scribble"),
            new("writer", "scribbler"),
            new("theory", "guesswork", inputPlural: "theories"),
            new ReplacementRule("research", "messing around").NoPluralHandling(),
            new ReplacementRule("creativity", "madness").NoPluralHandling(),
            new("idea", "notion"),
            new("language", "lingo"),
            new("evidence", "ancient legends"),
            new("hormones", "jelly beans"),
            new("people", "folk"),
            new("engineering", "bodging"),
            new ReplacementRule("authority", "in-chargeness").NoPluralHandling(),
            new("productivity", "profitability") { Bidirectional = true },
            new("wrongly", "rightly") { Bidirectional = true },
            new ReplacementRule("elongation", "enlengthening") { Bidirectional = true }.NoPluralHandling(),
            new("essential", "optional") { Bidirectional = true },
            new("mandatory", "recommended") { Bidirectional = true },
            new("necessary", "impossible") { Bidirectional = true },
            new("flood", "surprise bath"),
            new("resident", "dweller"),
            new("foot", "hand", "feet") { Bidirectional = true },
            new("bucket", "cup") { Bidirectional = true },
            new ReplacementRule("blood", "custard") { Bidirectional = true }.NoPluralHandling(),
            new("group", "cult"),
            new("representative", "deniable agent"),
            new("communication", "interference"),
            new("communicator", "meddler"),
            new("won't", "will") { InputsPlural = [], Bidirectional = true },
            new("consulted", "interrogated") { InputsPlural = [], Bidirectional = true },
            new("consultation", "interrogation") { Bidirectional = true },
            new("brain tumour", "headache") { Bidirectional = true },
            new("yes", "no"),
            new("should", "shouldn't") { Bidirectional = true},
            new("curious", "fearful") { Bidirectional = true},
            new("figuratively", "literally") { Bidirectional = true},
            new("principles", "quirks") { Bidirectional = true},

            // Question words
            new ReplacementRule("why", "how").NoPluralHandling(),
            new ReplacementRule("how", "when").NoPluralHandling(),
            new ReplacementRule("when", "where").NoPluralHandling(),
            new ReplacementRule("where", "what").NoPluralHandling(),
            new ReplacementRule("what", "who").NoPluralHandling(),
            new ReplacementRule("who", "why").NoPluralHandling(),

            new("measurement", "reckoning"),
            new("measure", "reckon"),

            // Materials
            new("concrete", "papier mache"),
            new("concrete", "papier-mâché") { Bidirectional = true },

            new("philosophy", "pondering"),
            new("philosopher", "ponderer"),
            new("philosophical", "ponderous"),

            new("woke", "awakened") { Bidirectional = true },
            new("blessed", "cursed") { Bidirectional = true },

            // Units/measures
            new("mile", "furlong") { Bidirectional = true },
            new("kilometre", "league") { Bidirectional = true },
            new("kilometer", "league") { Bidirectional = true },
            new("litre", "dram") { Bidirectional = true },
            new("liter", "dram") { Bidirectional = true },

            // Names
            new("charles", "charler"),
            new("ben", "benjermy"),
            new("benjamin", "benjermy"),
            new("larry", "larriott"),

            // Heresy
            new("the pope", "Satan") { CapitalisationHandling = CapitalisationHandling.Ignore },
            new("pope Francis", "Satan") { CapitalisationHandling = CapitalisationHandling.Ignore },
            new("pope", "Satan") { CapitalisationHandling = CapitalisationHandling.Ignore },
            new("bible", "1987 Sports Illustrated Swimsuit Issue") { CapitalisationHandling = CapitalisationHandling.Ignore },
            new("biblical", "literary"),


            // Animals
            new("drone", "dog") { Bidirectional = true },
            new("bird", "snake") { Bidirectional = true },


            new("self driving", "possessed"),
            new("self-driving", "possessed"),
            new("successfully", "suddenly") { Bidirectional = true },
            new("tension", "sexual tension"),
            new("optimistic", "delusional") { Bidirectional = true },
            new("CEO", "Head Honcho") { CapitalisationHandling = CapitalisationHandling.MatchExact },

            // Ships
            new("ship", "dinghy", outputPlural: "dinghies")  { Bidirectional = true },
            new("yacht", "galleon") { Bidirectional = true },
            new("boat", "oil tanker") { Bidirectional = true },

            // Durations
            new("second", "year") { Bidirectional = true },
            new("minute", "month") { Bidirectional = true },
            new("day", "week") { Bidirectional = true },

            // Grammatical
            new("may", "must") { Bidirectional = true },
        };

        List<JsonReplacementGroup> phase1Groups = [];
        List<JsonReplacementGroup> phase2Groups = [];

        //TODO: Handle alternate characters in rules e.g. fancy apostrope, letters with accents?

        foreach (var Rule in ReplacementRules)
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
                    ReverseInputs = rule.Inputs,

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
            return $@"\b{InputString}\b";
        }
        else
        {
            return InputString;
        }
    }
}