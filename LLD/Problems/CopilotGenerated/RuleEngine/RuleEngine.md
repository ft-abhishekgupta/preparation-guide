# Rule Engine and Evaluator

A configurable engine that evaluates business rules against a set of facts and fires actions,
so business logic lives in configuration instead of `if/else` chains in code.

## Prompt

```
"Design a rule engine. Rules are supplied as configuration and evaluated against a set of
facts. When a rule's condition is true, its action fires.

{
  "name": "PremiumFreeShipping",
  "priority": 10,
  "condition": {
    "op": "AND",
    "children": [
      { "op": "EQUALS", "fact": "customer.tier", "value": "PREMIUM" },
      { "op": "GREATER_THAN", "fact": "order.total", "value": 500 }
    ]
  },
  "action": { "type": "APPLY_DISCOUNT", "params": { "percent": 100, "on": "shipping" } }
}

Conditions can nest arbitrarily. Build the engine that loads and evaluates these rules."
```

## Questions

```
"What do facts look like - flat map or nested objects?"
> Nested, addressed with dot notation like 'order.total'

"What operators do we need?"
> Comparison (=, !=, >, <, >=, <=), IN, CONTAINS, and AND/OR/NOT

"Do all matching rules fire, or only the first?"
> Configurable: FIRST_MATCH or ALL_MATCHES, in priority order

"Can a rule's action change facts and re-trigger evaluation?"
> No forward chaining, single pass

"What if a fact referenced by a rule is missing?"
> Condition evaluates to false, do not throw

"Are rules loaded at startup or hot-reloaded?"
> Startup, but keep reload possible

"Do we need an explanation of why a rule matched?"
> Yes, that is very useful for debugging
```

## Requirements

```
Requirements:
1. Rules are loaded from configuration (name, priority, condition tree, action)
2. Conditions compose: comparison leaves plus AND / OR / NOT nodes, any depth
3. Facts are a nested map addressed by dot paths ("order.items.count")
4. Evaluate rules in descending priority order
5. Two modes: FIRST_MATCH (stop after one) and ALL_MATCHES
6. Matching rules fire actions through a registered action handler
7. A missing fact makes the condition false rather than throwing
8. Result reports which rules matched and what they did

Out of scope:
- Forward chaining / RETE network
- Rule authoring UI and DSL parser
- Persistence and versioning of rules
- Distributed evaluation
```

## Core Entities

```
RuleEngine       : Orchestrator - evaluate(facts) -> EvaluationResult
Rule             : name + priority + condition + action
Condition        : Composite - Comparison leaves, logical nodes
FactContext      : Fact lookup by dot path
ConditionFactory : Builds the condition tree from config
ActionHandler    : Strategy - executes an action type
EvaluationResult : Which rules matched, outcomes, trace
```

**Why these patterns**

| Concern | Pattern | Reason |
|---|---|---|
| Nested conditions | Composite | AND/OR/NOT and leaves share one interface |
| Building from JSON | Factory | Config → object tree in one place |
| Operators | Strategy (map of comparators) | New operator = one map entry |
| Actions | Strategy + registry | Engine never knows what an action does |
| Explanation | Visitor-ish trace | Debuggability is the top ask for rule engines |

The whole design rests on one idea: **a condition is a tree, and every node answers the same
question** - `evaluate(facts) -> bool`. That is what allows unlimited nesting with no special
casing.

## Class Design

```
class RuleEngine:
    - rules: List<Rule>                 // sorted by priority desc
    - actions: Map<string, ActionHandler>
    - mode: EvaluationMode              // FIRST_MATCH | ALL_MATCHES

    + RuleEngine(ruleConfigs, actions, mode)
    + evaluate(facts) -> EvaluationResult
    + reload(ruleConfigs) -> void

class Rule:
    - name: string
    - priority: int
    - condition: Condition
    - action: ActionSpec

interface Condition:
    + evaluate(facts: FactContext) -> boolean
    + describe() -> string              // for the trace

class ComparisonCondition implements Condition:
    - factPath: string
    - op: Operator                      // EQUALS, GREATER_THAN, IN, CONTAINS ...
    - value: object

class AndCondition implements Condition:
    - children: List<Condition>
class OrCondition implements Condition:
    - children: List<Condition>
class NotCondition implements Condition:
    - child: Condition

class FactContext:
    - facts: Map<string, object>

    + get(path) -> object | MISSING     // "order.total" walks nested maps
    + has(path) -> boolean

class ConditionFactory:
    + create(configNode) -> Condition    // recursive

interface ActionHandler:
    + execute(params, facts) -> object

class EvaluationResult:
    - matched: List<MatchedRule>         // rule name + action output
    - trace: List<string>
```

**Condition tree for the sample rule**

```
                 AND
                /   \
     EQUALS(tier,   GREATER_THAN(
      "PREMIUM")      order.total, 500)
```

`AndCondition.evaluate` just asks its children - it does not care whether a child is a leaf
or another 10-level subtree.

## Implementation

RuleEngine.evaluate

```
evaluate(facts)
    context = new FactContext(facts)
    result = new EvaluationResult()

    for rule in rules                        // already sorted by priority desc
        matched = rule.condition.evaluate(context)
        result.trace.add(rule.name + " => " + matched)

        if !matched: continue

        handler = actions[rule.action.type]
        if handler == null
            result.trace.add("no handler for " + rule.action.type)
            continue

        output = handler.execute(rule.action.params, context)
        result.matched.add(new MatchedRule(rule.name, output))

        if mode == FIRST_MATCH
            break

    return result
```

Composite conditions - short circuiting

```
AndCondition.evaluate(facts)
    for child in children
        if !child.evaluate(facts): return false     // short circuit
    return true

OrCondition.evaluate(facts)
    for child in children
        if child.evaluate(facts): return true
    return false

NotCondition.evaluate(facts)
    return !child.evaluate(facts)
```

ComparisonCondition - the only place that touches data

```
evaluate(facts)
    actual = facts.get(factPath)
    if actual == MISSING: return false          // missing fact never matches

    switch op
        EQUALS:        return equals(actual, value)
        NOT_EQUALS:    return !equals(actual, value)
        GREATER_THAN:  return compare(actual, value) > 0
        LESS_THAN:     return compare(actual, value) < 0
        GREATER_EQUAL: return compare(actual, value) >= 0
        LESS_EQUAL:    return compare(actual, value) <= 0
        IN:            return (value as list).contains(actual)
        CONTAINS:      return (actual as list/string).contains(value)
```

FactContext - dot path resolution

```
get(path)
    parts = path.split(".")
    current = facts

    for part in parts
        if current is not a Map: return MISSING
        if !current.containsKey(part): return MISSING
        current = current[part]

    return current
```

ConditionFactory - recursive build

```
create(node)
    op = node["op"]

    switch op
        case "AND":
            return new AndCondition(node["children"].map(create))
        case "OR":
            return new OrCondition(node["children"].map(create))
        case "NOT":
            return new NotCondition(create(node["child"]))
        default:
            return new ComparisonCondition(
                node["fact"], parseOperator(op), node["value"])
```

Recursion mirrors the config shape exactly - roughly 15 lines handles arbitrary nesting.

## Code

FactContext

```cs
using System.Collections.Generic;

public class FactContext
{
    public static readonly object Missing = new();

    private readonly Dictionary<string, object> _facts;

    public FactContext(Dictionary<string, object> facts) => _facts = facts;

    public object Get(string path)
    {
        object current = _facts;

        foreach (var part in path.Split('.'))
        {
            if (current is not Dictionary<string, object> map || !map.TryGetValue(part, out current))
            {
                return Missing;
            }
        }

        return current;
    }

    public bool Has(string path) => Get(path) != Missing;
}
```

Conditions

```cs
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum Operator
{
    Equals, NotEquals, GreaterThan, LessThan, GreaterEqual, LessEqual, In, Contains
}

public interface ICondition
{
    bool Evaluate(FactContext facts);
    string Describe();
}

public class ComparisonCondition : ICondition
{
    private readonly string _factPath;
    private readonly Operator _op;
    private readonly object _value;

    public ComparisonCondition(string factPath, Operator op, object value)
    {
        _factPath = factPath;
        _op = op;
        _value = value;
    }

    public bool Evaluate(FactContext facts)
    {
        var actual = facts.Get(_factPath);
        if (actual == FactContext.Missing)
        {
            return false;
        }

        return _op switch
        {
            Operator.Equals       => Equals(actual, _value),
            Operator.NotEquals    => !Equals(actual, _value),
            Operator.GreaterThan  => Compare(actual, _value) > 0,
            Operator.LessThan     => Compare(actual, _value) < 0,
            Operator.GreaterEqual => Compare(actual, _value) >= 0,
            Operator.LessEqual    => Compare(actual, _value) <= 0,
            Operator.In           => _value is IEnumerable list && list.Cast<object>().Contains(actual),
            Operator.Contains     => ContainsValue(actual, _value),
            _ => false
        };
    }

    private static int Compare(object a, object b)
        => Convert.ToDouble(a).CompareTo(Convert.ToDouble(b));

    private static bool ContainsValue(object actual, object value)
        => actual switch
        {
            string s => s.Contains(Convert.ToString(value)),
            IEnumerable list => list.Cast<object>().Contains(value),
            _ => false
        };

    public string Describe() => $"{_factPath} {_op} {_value}";
}

public class AndCondition : ICondition
{
    private readonly List<ICondition> _children;

    public AndCondition(List<ICondition> children) => _children = children;

    public bool Evaluate(FactContext facts) => _children.All(c => c.Evaluate(facts));

    public string Describe() => "(" + string.Join(" AND ", _children.Select(c => c.Describe())) + ")";
}

public class OrCondition : ICondition
{
    private readonly List<ICondition> _children;

    public OrCondition(List<ICondition> children) => _children = children;

    public bool Evaluate(FactContext facts) => _children.Any(c => c.Evaluate(facts));

    public string Describe() => "(" + string.Join(" OR ", _children.Select(c => c.Describe())) + ")";
}

public class NotCondition : ICondition
{
    private readonly ICondition _child;

    public NotCondition(ICondition child) => _child = child;

    public bool Evaluate(FactContext facts) => !_child.Evaluate(facts);

    public string Describe() => $"NOT {_child.Describe()}";
}
```

ConditionFactory

```cs
using System;
using System.Collections.Generic;
using System.Linq;

public class ConditionFactory
{
    public ICondition Create(Dictionary<string, object> node)
    {
        var op = node["op"] as string;

        switch (op)
        {
            case "AND":
                return new AndCondition(Children(node));
            case "OR":
                return new OrCondition(Children(node));
            case "NOT":
                return new NotCondition(Create((Dictionary<string, object>)node["child"]));
            default:
                return new ComparisonCondition(
                    node["fact"] as string,
                    Enum.Parse<Operator>(ToPascal(op)),
                    node.GetValueOrDefault("value"));
        }
    }

    private List<ICondition> Children(Dictionary<string, object> node)
        => ((List<Dictionary<string, object>>)node["children"]).Select(Create).ToList();

    private static string ToPascal(string op)
        => string.Concat(op.Split('_').Select(p => char.ToUpper(p[0]) + p[1..].ToLower()));
}
```

Rule, Action & Result

```cs
using System.Collections.Generic;

public class ActionSpec
{
    public string Type { get; }
    public Dictionary<string, object> Params { get; }

    public ActionSpec(string type, Dictionary<string, object> parameters)
    {
        Type = type;
        Params = parameters;
    }
}

public class Rule
{
    public string Name { get; }
    public int Priority { get; }
    public ICondition Condition { get; }
    public ActionSpec Action { get; }

    public Rule(string name, int priority, ICondition condition, ActionSpec action)
    {
        Name = name;
        Priority = priority;
        Condition = condition;
        Action = action;
    }
}

public interface IActionHandler
{
    object Execute(Dictionary<string, object> parameters, FactContext facts);
}

public class MatchedRule
{
    public string RuleName { get; }
    public object Output { get; }

    public MatchedRule(string ruleName, object output)
    {
        RuleName = ruleName;
        Output = output;
    }
}

public class EvaluationResult
{
    public List<MatchedRule> Matched { get; } = new();
    public List<string> Trace { get; } = new();
}
```

RuleEngine

```cs
using System.Collections.Generic;
using System.Linq;

public enum EvaluationMode { FirstMatch, AllMatches }

public class RuleEngine
{
    private List<Rule> _rules;
    private readonly Dictionary<string, IActionHandler> _actions;
    private readonly EvaluationMode _mode;

    public RuleEngine(IEnumerable<Rule> rules,
                      Dictionary<string, IActionHandler> actions,
                      EvaluationMode mode)
    {
        _rules = rules.OrderByDescending(r => r.Priority).ToList();
        _actions = actions;
        _mode = mode;
    }

    public void Reload(IEnumerable<Rule> rules)
        => _rules = rules.OrderByDescending(r => r.Priority).ToList();

    public EvaluationResult Evaluate(Dictionary<string, object> facts)
    {
        var context = new FactContext(facts);
        var result = new EvaluationResult();

        foreach (var rule in _rules)
        {
            var matched = rule.Condition.Evaluate(context);
            result.Trace.Add($"{rule.Name} [{rule.Condition.Describe()}] => {matched}");

            if (!matched)
            {
                continue;
            }

            if (!_actions.TryGetValue(rule.Action.Type, out var handler))
            {
                result.Trace.Add($"  no handler registered for {rule.Action.Type}");
                continue;
            }

            var output = handler.Execute(rule.Action.Params, context);
            result.Matched.Add(new MatchedRule(rule.Name, output));

            if (_mode == EvaluationMode.FirstMatch)
            {
                break;
            }
        }

        return result;
    }
}
```

Usage

```cs
var condition = new AndCondition(new List<ICondition>
{
    new ComparisonCondition("customer.tier", Operator.Equals, "PREMIUM"),
    new ComparisonCondition("order.total", Operator.GreaterThan, 500)
});

var engine = new RuleEngine(
    rules: new[]
    {
        new Rule("PremiumFreeShipping", 10, condition,
                 new ActionSpec("APPLY_DISCOUNT", new() { ["percent"] = 100, ["on"] = "shipping" }))
    },
    actions: new Dictionary<string, IActionHandler> { ["APPLY_DISCOUNT"] = new DiscountHandler() },
    mode: EvaluationMode.AllMatches);

var facts = new Dictionary<string, object>
{
    ["customer"] = new Dictionary<string, object> { ["tier"] = "PREMIUM" },
    ["order"]    = new Dictionary<string, object> { ["total"] = 750 }
};

var result = engine.Evaluate(facts);   // matches PremiumFreeShipping
```

## Extensibility

### "How would you add a new operator like REGEX_MATCH?"

Add the enum value and one arm in the `switch`. Cleaner still: replace the switch with a
`Dictionary<Operator, Func<object, object, bool>>` so a new operator is a single registration
and `ComparisonCondition` becomes closed for modification.

### "How would you add a new action type?"

Implement `IActionHandler` and register it under a key. The engine only looks up a string, so
actions can be added by another team without touching engine code.

### "How do you support rules that depend on the outcome of other rules (forward chaining)?"

Loop evaluation: actions write derived facts back into the `FactContext`, then re-evaluate
until no new rule fires or a max-iteration cap is hit (guards infinite loops). RETE is the
optimised version - it caches partial condition matches across cycles rather than
re-evaluating everything.

### "10,000 rules and this evaluates every one - how do you speed it up?"

Index rules by their most selective fact: build `Map<factPath, List<Rule>>` at load time and
only evaluate rules whose indexed fact is present and in range. Also order AND-children by
cheapest/most-selective first so short circuiting kicks in early.

### "How do you validate rules before they go live?"

Validate at load: unknown operator, unknown action type, malformed tree, unreachable rule
(condition can never be true). Run new rules in shadow mode over recorded facts and diff the
outcomes before enabling them.

### "How do you make rules hot-reloadable?"

`Reload()` swaps the sorted list reference atomically. Because `_rules` is replaced rather
than mutated, an in-flight `Evaluate` keeps using its consistent snapshot - a `volatile`
field reference is enough, no locking needed.

### "How would you explain to a user why a rule did NOT match?"

Return a structured trace instead of strings: each condition node reports
`(description, result, actualValue)`. Walking the failed subtree shows exactly which leaf
was false and what fact value caused it.
