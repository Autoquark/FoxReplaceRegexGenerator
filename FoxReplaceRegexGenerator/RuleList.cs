static class RuleList
{
    public static List<ReplacementRule> TestRules { get; } = [new(["concrete"], ["papier mache", "papier-mâché", "papier mâché"]) { Bidirectional = true }];

    public static List<ReplacementRule> Rules { get; } = 
    [
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
        new ReplacementRule("robust", "shoddy") { Bidirectional = true}.NoPluralHandling(),
        new ReplacementRule(["verifiable", "verified"], "legit") { Bidirectional = true}.NoPluralHandling(),

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
        new(["concrete"], ["papier mache", "papier-mâché"]) { Bidirectional = true },

        new("philosophy", "pondering"),
        new("philosopher", "ponderer"),
        new("philosophical", "ponderous"),

        new("woke", "awakened") { Bidirectional = true },
        new("blessed", "cursed") { Bidirectional = true },

        // Units/measures
        new("mile", "furlong") { Bidirectional = true },
        new(["kilometre", "kilometer"], "league") { Bidirectional = true },
        new(["litre", "liter"], "dram") { Bidirectional = true },

        // Names
        new("charles", "charler"),
        new("ben", "benjermy"),
        new("benjamin", "benjermy"),
        new("larry", "larriott"),

        // Heresy
        new(["the pope", "pope Francis", "pope"], "Satan") { CapitalisationHandling = CapitalisationHandling.Ignore },
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
    ];
}