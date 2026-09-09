# Resume

## About Yourself

```
I'm a backend software engineer and technical lead with more than six years of experience at Microsoft Xbox. I specialize in building scalable distributed systems with C#, .NET, and Azure, and I own services end to end, from architecture and design through production operations.

I've built backend platforms that serve more than 7 million players and thousands of game publishers, handling around 5,000 requests per second with 99.99% availability. My work has involved Cosmos DB, Redis, Azure Service Bus, and event-driven architectures. In my current role, I lead a team of four engineers and own the architecture and delivery of the Xbox News Feed system.

I've also worked on AI-powered content-processing systems and developer-productivity platforms, while contributing to frontend development when needed. I'm now looking for a backend role where I can solve challenging distributed-systems problems at scale, own products end to end, and continue growing as a technical leader.
```

## Career Path

```
I did my B.Tech in Computer Science from IET Lucknow and qualified GATE to pursue my master's at IIT Kanpur. After developing an interest in web and application development through college projects, I joined Microsoft Xbox. I began with React-based platforms for pricing, sales, and game publishing, then expanded into frontend architecture, Azure hosting, accessibility, and API operations.

I deliberately moved into backend and platform engineering through the Game Certification System, building services, infrastructure, and event-driven workflows while mentoring junior engineers. After my promotion to Software Engineer II, I took on larger distributed systems, cross-team initiatives, and multi-region reliability and security work.

Most recently, I led the replacement of the legacy Xbox Engagement Events and News Feed services with a new system built from the ground up. Overall, my career has progressed from feature delivery to end-to-end system ownership and cross-team technical leadership, while remaining hands-on.
```

## Why the switch

```
I've had strong opportunities at Microsoft Xbox, and I'm proud of the scale and impact of my work there. After more than six years, I was ready to explore new domains and challenges with broader technical ownership. Although I was affected by the recent layoffs, I had already begun considering my next step. I'm now looking for an environment where I can apply my experience in high-scale services, platform engineering, and technical leadership to create even greater impact.

>> End with something specific to company and role
```

## Why do you want to join this company / role

```
>> Connect response with company
>> Connect response with role
```

## Strength

```
Technical Ownership
- I take end-to-end ownership, from clarifying requirements and designing the architecture to implementation, production rollout, and operational support.

Problem Solving
- I break ambiguous problems into manageable pieces, evaluate alternatives, and use prototypes to validate ideas early. This approach has helped me align teams and drive decisions on complex cross-team projects.

Team Player
- I build strong working relationships, communicate risks and challenges early, and help teammates get unblocked. I focus on shared outcomes, knowledge sharing, and creating an environment where the team can succeed together.
```

## What motivates you

```
- Solving ambiguous, technically challenging problems through structured thinking.
- Continuously learning and expanding my technical capabilities.
- Working in a healthy, collaborative environment where people openly share ideas and support one another.
- Creating meaningful impact and receiving constructive feedback that helps me improve.
```

## Weakness / Area you are working on

```
I'm working on balancing depth with speed. I naturally seek an end-to-end understanding of systems and explore edge cases and failure scenarios. While this is valuable for production systems, it can slow lower-risk decisions. I now prioritize critical risks, make the simplest sound decision, and go deeper only when the scale or impact justifies it.
```

## Disagreement with Team / Manager

```
During planning for the News Feed replacement, the product team proposed dropping all existing posts at cutover to reduce migration time and cost. I understood the delivery pressure but disagreed because popular games had thousands of posts, and losing them could damage the experience for both players and publishers.

I raised the concern with engineering leadership and the product team, using concrete examples to explain the customer impact. I proposed validating the decision with key publishers and offered a dual-feed alternative: continue serving legacy posts while publishers onboarded and began publishing through the new platform.

When I disagree, I focus on the problem and trade-offs rather than on who is right. I seek to understand the other perspective, present evidence and practical alternatives, and then commit to the final decision so the team can move forward.
```

## Biggest Achievement

```
One of my biggest achievements was leading the development of Xbox's unified News Feed platform. I led the team and owned the system end to end, from architecture and implementation to migration and production rollout.

The platform replaced more than 20 legacy systems and unified content from hundreds of publishers for millions of players across Xbox storefronts, handling around 5,000 requests per second.

I also redesigned the data-access and caching strategy using Cosmos DB and Redis. This reduced p99 read latency from approximately 600 milliseconds to 150 milliseconds and lowered gateway load by 87%.

This achievement is particularly meaningful because it combined large-scale modernization, measurable performance gains, and hands-on technical leadership throughout the product lifecycle.
```

## Biggest Challenge

```
One of my biggest challenges was migrating a critical production service away from complex legacy dependencies without disrupting live traffic. The existing system had limited documentation and no clear owners, while also contributing significantly to latency and gateway load. I used AI-assisted analysis to accelerate my understanding of the system before designing its replacement.

I led a phased, zero-downtime migration. We redesigned the data model and partitioning strategy, introduced a new Cosmos DB path with Redis caching, and gradually shifted traffic using feature flags and configuration controls. Throughout the rollout, we monitored both paths closely to protect correctness and reliability.

The migration reduced p99 read latency from approximately 600 milliseconds to 150 milliseconds and lowered gateway load by 87%. My key learning was that, for critical distributed systems, a safe migration strategy is just as important as the target architecture.
```

## Failure Faced

```

```

## Complex Production Issue

```

```

## Where do you see yourself in 5 years

```
Over the next three to five years, I want to grow into a stronger senior or staff-level technical leader while remaining hands-on. I aim to own increasingly complex distributed systems, make sound architectural decisions, and influence outcomes across teams. I also want to help other engineers grow through mentoring and technical guidance, combining deep engineering work with broader organizational impact.
```

## Question for them

```
"What are the most challenging technical problems the team is currently working on?"

"How are architecture and technical decisions typically made within the team?"

"What would success look like for someone in this role in the first six months?"

"What are the biggest scalability or reliability challenges the team expects to tackle over the next year?"
```

## Projects

> Problem → Requirements → Architecture → Your contribution → Key design decisions → Tradeoffs → Scale → Failure handling → Performance → Monitoring → Biggest challenge → Result
