# 🎯 System Design Interview Guide

Seven files. Everything you need to navigate and answer any system design question.

> **The thesis:** interviews aren't won by knowing more components. They're won by making *defensible choices* — the simplest design that meets the requirements, plus a crisp account of what you traded away and where it breaks.

| File | What it's for | When to read |
|---|---|---|
| [⚡ 00-QuickRevision](00-QuickRevision.md) | The script, the numbers, the one-liners | The hour before the interview |
| [01-Framework](01-Framework.md) | How to spend the 45 minutes; what's actually scored | First, and again before every mock |
| [02-DecisionTrees](02-DecisionTrees.md) | "Which database / transport / cache / lock do I pick?" | Whenever you're unsure mid-design |
| [03-CoreConcepts](03-CoreConcepts.md) | The mechanics: networking, indexing, caching, sharding, CAP, numbers, specialized stores | To fill knowledge gaps |
| [04-Patterns](04-Patterns.md) | Requirement phrasing → known solution shape | To recognise problems fast |
| [05-Technologies](05-Technologies.md) | Redis, Kafka, Postgres, Dynamo, Cassandra, ES, Flink, ZK, edge | To justify the boxes you draw |
| [06-Problems](06-Problems.md) | 32 designs compressed to their key decisions | After attempting each on a whiteboard |

## The loop, in the room

**Requirement → pattern → concept → technology → a tradeoff you can defend.**

That's it. `01` tells you when to do each step, `04` turns the requirement into a shape, `03` and `05` give you the mechanics and the boxes, and `02` justifies every choice.

## Five rules that carry every interview

1. **Finish a simple system before you optimise anything.**
2. **Quantify the non-functional requirements** — they're the only justification for complexity.
3. **Do the math before adding infrastructure.** Postgres does 20k writes/s; 10M × 1 KB is 10 GB, not a sharding problem.
4. **Name the tradeoff, every time:** "I chose X over Y because the requirement is Z; the cost is W."
5. **Attack your own design** before the interviewer does. That's what senior sounds like.

---

*Condensed from the 72-document library in this repo. Each section links back to the full source docs (`../1-InAHurry/` … `../7-Problems/`) when you want the long version. Diagrams live in `images/`.*
