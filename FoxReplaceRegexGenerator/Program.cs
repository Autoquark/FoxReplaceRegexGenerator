using System.Text.Json;
using System.Text.Json.Serialization;

internal static class Program
{
    private static readonly string OutputPath = "./output/";
    private static readonly string OutputFilename = "GeneratedRules.json";

    private static void Main(string[] args)
    {
        Console.WriteLine("Generating replacement rules...");

        var ReplacementRules = new List<ReplacementRule> {
            new("cheese", "hardmilk"),

            // Durations
            // second, minute, hour, day, week, month, year
            new("second", "year"),
            new("minute", "month"),
        };

        List<JsonReplacementGroup> phase1Groups = [];
        List<JsonReplacementGroup> phase2Groups = [];

        foreach (var Rule in ReplacementRules)
        {
            // Plural handling
            IEnumerable<ReplacementRule> Rules = [Rule];

            Rules = HandlePlurals(Rules);

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
                Name = $"{Rule.Input} -> {Rule.Output}",
                Substitutions = phase1substitutions
            });
            
            if(phase2substitutions.Any())
            {
                phase2Groups.Add(new JsonReplacementGroup
                {
                    Name = $"{Rule.Input} -> {Rule.Output}",
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

    private static IEnumerable<ReplacementRule> HandlePlurals(IEnumerable<ReplacementRule> rules)
    {
        foreach(var rule in rules)
        {
            // Return the rule as-is to handle the singular
            yield return rule;

            if(!string.IsNullOrWhiteSpace(rule.InputPlural) && !string.IsNullOrWhiteSpace(rule.OutputPlural))
            {
                yield return rule with
                {
                     Input = rule.InputPlural,
                     Output = rule.OutputPlural
                };
            }
        }
    }

    private static IEnumerable<JsonSubstitution> HandleCapitalisation(IEnumerable<ReplacementRule> rules)
    {
        foreach(var rule in rules)
        {
            if (rule.CapitalisationHandling == CapitalisationHandling.Preserve)
            {
                // All uppercase
                yield return new JsonSubstitution
                {
                    CaseSensitive = true,
                    Input = GetRegexForInput(rule, rule.Input.ToUpperInvariant()),
                    Output = rule.Output.ToUpperInvariant()
                };

                // First letter only uppercase
                yield return new JsonSubstitution
                {
                    CaseSensitive = true,
                    Input = GetRegexForInput(rule, char.ToUpperInvariant(rule.Input[0]) + string.Concat(rule.Input.Skip(1))),
                    Output = char.ToUpperInvariant(rule.Output[0]) + string.Concat(rule.Output.Skip(1)),
                };

                // Any other input case combination results in all lowercase output
                yield return new JsonSubstitution
                {
                    CaseSensitive = false,
                    Input = GetRegexForInput(rule, rule.Input.ToLowerInvariant()),
                    Output = rule.Output.ToLowerInvariant()
                };
            }
            else
            {
                yield return new JsonSubstitution
                {
                    CaseSensitive = rule.CapitalisationHandling == CapitalisationHandling.MatchExact,
                    Input = rule.Input,
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