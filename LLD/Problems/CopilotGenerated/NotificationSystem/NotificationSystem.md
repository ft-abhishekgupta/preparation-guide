# Notification System

A service that accepts events, fans them out to subscribers across multiple channels
(email, SMS, push), delivers them asynchronously through a queue, and retries failures
with backoff.

## Prompt

```
"Design a notification system. Services publish events like 'ORDER_SHIPPED' and the system
delivers messages to interested users over email, SMS and push.

Users control what they receive:

{
  "userId": "u1",
  "topic": "ORDER_SHIPPED",
  "channels": ["EMAIL", "PUSH"]
}

Delivery must be asynchronous and must survive transient channel failures."
```

## Questions

```
"Is publishing synchronous or fire-and-forget?"
> Fire-and-forget, the publisher must not block on delivery

"How do we know who to notify - explicit recipients or subscriptions?"
> Topic subscriptions, pub-sub

"Can a user receive the same event on multiple channels?"
> Yes, one message per subscribed channel

"What happens when a channel is down?"
> Retry with backoff, then dead letter

"How many retries?"
> Configurable, say 3 attempts with exponential backoff

"Do we need per-channel formatting?"
> Yes, SMS is short, email is rich

"Ordering guarantees?"
> Best effort, not required

"Distributed queue (Kafka/SQS) or in-memory?"
> In-memory for now, keep it pluggable
```

## Requirements

```
Requirements:
1. Publishers emit events: publish(topic, payload)
2. Users subscribe to a topic with a set of channels
3. One event fans out to N (user, channel) messages
4. Delivery is asynchronous - publish() returns immediately
5. Messages go into a queue consumed by worker threads
6. Each channel has its own sender (Email, SMS, Push)
7. Failed deliveries retry up to maxAttempts with exponential backoff
8. Permanently failed messages move to a dead letter queue
9. Message body is rendered per channel

Out of scope:
- Durable persistence of the queue
- Per-user rate limiting, quiet hours (discussed in extensibility)
- Delivery receipts / read tracking
- Distributed workers and exactly-once delivery
```

## Core Entities

```
NotificationService : Orchestrator / facade
SubscriptionRegistry: Who wants what, on which channels (pub-sub)
Message             : Unit of delivery (user + channel + rendered body)
DelayQueue          : Buffer between publish and delivery, honours retry delays
DeliveryWorker      : Consumer thread
Channel             : Strategy - Email / SMS / Push sender
RetryPolicy         : Strategy - how long to wait, when to give up
DeadLetterQueue     : Terminal store for permanent failures
```

**Why these patterns**

| Concern | Pattern | Reason |
|---|---|---|
| New channel (WhatsApp) | Strategy + registry | Plug in without touching workers |
| Fan-out | Pub-Sub / Observer | Publishers do not know recipients |
| Async delivery | Producer-Consumer | Decouples publish latency from delivery |
| Retry behaviour | Strategy | Fixed vs exponential vs jittered |

## Class Design

```
class NotificationService:
    - registry: SubscriptionRegistry
    - queue: DelayQueue
    - channels: Map<ChannelType, Channel>
    - workers: List<DeliveryWorker>

    + publish(topic, payload) -> void
    + subscribe(userId, topic, channels) -> void
    + unsubscribe(userId, topic) -> void
    + start() / shutdown() -> void

class SubscriptionRegistry:
    - subs: Map<topic, Map<userId, Subscription>>

    + subscribe(userId, topic, channels)
    + unsubscribe(userId, topic)
    + getSubscribers(topic) -> List<Subscription>

class Message:
    - id: string
    - userId: string
    - channel: ChannelType
    - body: string
    - attempt: int
    - nextAttemptAt: long

interface Channel:
    + type() -> ChannelType
    + send(message) -> void          // throws on failure

class EmailChannel / SmsChannel / PushChannel implements Channel

interface RetryPolicy:
    + shouldRetry(attempt) -> boolean
    + nextDelayMs(attempt) -> long

class ExponentialBackoffPolicy implements RetryPolicy:
    - maxAttempts: int
    - baseDelayMs: long

class DeliveryWorker:
    - queue, channels, retryPolicy, deadLetters
    + run() -> void
```

**Flow**

```
publish(topic, payload)
        |
        v
SubscriptionRegistry.getSubscribers(topic)      -> fan-out
        |
        v
render(topic, channel, payload)                 -> Message per (user, channel)
        |
        v
     DelayQueue  <------------- requeue with delay on failure
        |
        v
  DeliveryWorker -> Channel.send()
        |                    \
     success                  attempts exhausted
                                       |
                                       v
                              DeadLetterQueue
```

## Implementation

publish (fan-out)

```
publish(topic, payload)
    subscribers = registry.getSubscribers(topic)

    for sub in subscribers
        for channel in sub.channels
            body = render(topic, channel, payload)
            queue.enqueue(new Message(
                id: generateId(),
                userId: sub.userId,
                channel: channel,
                body: body,
                attempt: 0,
                nextAttemptAt: now()
            ))
    // returns immediately, nothing has been delivered yet
```

DeliveryWorker.run

```
run()
    while running
        message = queue.dequeue()          // blocks until a message is due
        if message == null: break          // null only on shutdown

        try
            channels[message.channel].send(message)
        catch error
            message.attempt += 1
            if retryPolicy.shouldRetry(message.attempt)
                message.nextAttemptAt = now() + retryPolicy.nextDelayMs(message.attempt)
                queue.enqueue(message)
            else
                deadLetters.add(message)
```

ExponentialBackoffPolicy

```
shouldRetry(attempt)
    return attempt < maxAttempts

nextDelayMs(attempt)
    return baseDelayMs * (2 ^ (attempt - 1))   // 1s, 2s, 4s ... optionally + jitter
```

Delay-aware queue (lock + condition variable)

```
enqueue(message)
    lock(monitor)
        insert message ordered by nextAttemptAt
        monitor.pulseAll()

dequeue()
    lock(monitor)
        while running
            if empty
                monitor.wait()
                continue

            head = peek()
            delay = head.nextAttemptAt - now()
            if delay <= 0
                return pop()

            monitor.wait(delay)      // wake when the head becomes due, or on new insert
        return null
```

Key detail: waiting with a timeout (not a spin) is what makes retries cheap - the worker
sleeps exactly until the earliest due message.

## Code

Message & enums

```cs
using System;

public enum ChannelType { Email, Sms, Push }

public class Message
{
    public string Id { get; }
    public string UserId { get; }
    public ChannelType Channel { get; }
    public string Body { get; }
    public int Attempt { get; set; }
    public long NextAttemptAt { get; set; }

    public Message(string id, string userId, ChannelType channel, string body)
    {
        Id = id;
        UserId = userId;
        Channel = channel;
        Body = body;
        Attempt = 0;
        NextAttemptAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}
```

SubscriptionRegistry

```cs
using System.Collections.Generic;
using System.Linq;

public class Subscription
{
    public string UserId { get; }
    public HashSet<ChannelType> Channels { get; }

    public Subscription(string userId, IEnumerable<ChannelType> channels)
    {
        UserId = userId;
        Channels = new HashSet<ChannelType>(channels);
    }
}

public class SubscriptionRegistry
{
    private readonly Dictionary<string, Dictionary<string, Subscription>> _subs = new();
    private readonly object _lock = new();

    public void Subscribe(string userId, string topic, IEnumerable<ChannelType> channels)
    {
        lock (_lock)
        {
            if (!_subs.TryGetValue(topic, out var byUser))
            {
                byUser = new Dictionary<string, Subscription>();
                _subs[topic] = byUser;
            }
            byUser[userId] = new Subscription(userId, channels);
        }
    }

    public void Unsubscribe(string userId, string topic)
    {
        lock (_lock)
        {
            if (_subs.TryGetValue(topic, out var byUser))
            {
                byUser.Remove(userId);
            }
        }
    }

    public List<Subscription> GetSubscribers(string topic)
    {
        lock (_lock)
        {
            return _subs.TryGetValue(topic, out var byUser)
                ? byUser.Values.ToList()
                : new List<Subscription>();
        }
    }
}
```

Channels

```cs
using System;

public interface IChannel
{
    ChannelType Type { get; }
    void Send(Message message);
}

public class EmailChannel : IChannel
{
    public ChannelType Type => ChannelType.Email;

    public void Send(Message message)
    {
        // smtpClient.Send(...) - throws on transient failure
        Console.WriteLine($"[EMAIL] to={message.UserId} body={message.Body}");
    }
}

public class SmsChannel : IChannel
{
    public ChannelType Type => ChannelType.Sms;

    public void Send(Message message)
        => Console.WriteLine($"[SMS] to={message.UserId} body={message.Body}");
}

public class PushChannel : IChannel
{
    public ChannelType Type => ChannelType.Push;

    public void Send(Message message)
        => Console.WriteLine($"[PUSH] to={message.UserId} body={message.Body}");
}
```

RetryPolicy

```cs
using System;

public interface IRetryPolicy
{
    bool ShouldRetry(int attempt);
    long NextDelayMs(int attempt);
}

public class ExponentialBackoffPolicy : IRetryPolicy
{
    private readonly int _maxAttempts;
    private readonly long _baseDelayMs;

    public ExponentialBackoffPolicy(int maxAttempts, long baseDelayMs)
    {
        _maxAttempts = maxAttempts;
        _baseDelayMs = baseDelayMs;
    }

    public bool ShouldRetry(int attempt) => attempt < _maxAttempts;

    public long NextDelayMs(int attempt) => _baseDelayMs * (long)Math.Pow(2, attempt - 1);
}
```

DelayQueue

```cs
using System;
using System.Collections.Generic;
using System.Threading;

public class DelayQueue
{
    private readonly List<Message> _items = new();   // kept sorted by NextAttemptAt
    private readonly object _lock = new();
    private bool _running = true;

    public void Enqueue(Message message)
    {
        lock (_lock)
        {
            var index = _items.FindIndex(m => m.NextAttemptAt > message.NextAttemptAt);
            if (index < 0)
            {
                _items.Add(message);
            }
            else
            {
                _items.Insert(index, message);
            }
            Monitor.PulseAll(_lock);
        }
    }

    public Message Dequeue()
    {
        lock (_lock)
        {
            while (_running)
            {
                if (_items.Count == 0)
                {
                    Monitor.Wait(_lock);
                    continue;
                }

                var head = _items[0];
                var delay = head.NextAttemptAt - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                if (delay <= 0)
                {
                    _items.RemoveAt(0);
                    return head;
                }

                Monitor.Wait(_lock, (int)Math.Min(delay, int.MaxValue));
            }
            return null;
        }
    }

    public void Shutdown()
    {
        lock (_lock)
        {
            _running = false;
            Monitor.PulseAll(_lock);
        }
    }
}
```

DeliveryWorker

```cs
using System;
using System.Collections.Generic;
using System.Threading;

public class DeliveryWorker
{
    private readonly DelayQueue _queue;
    private readonly Dictionary<ChannelType, IChannel> _channels;
    private readonly IRetryPolicy _retryPolicy;
    private readonly List<Message> _deadLetters;
    private Thread _thread;
    private volatile bool _running;

    public DeliveryWorker(DelayQueue queue, Dictionary<ChannelType, IChannel> channels,
                          IRetryPolicy retryPolicy, List<Message> deadLetters)
    {
        _queue = queue;
        _channels = channels;
        _retryPolicy = retryPolicy;
        _deadLetters = deadLetters;
    }

    public void Start()
    {
        _running = true;
        _thread = new Thread(Run) { IsBackground = true };
        _thread.Start();
    }

    public void Stop()
    {
        _running = false;
        _thread?.Join(1000);
    }

    private void Run()
    {
        while (_running)
        {
            var message = _queue.Dequeue();
            if (message == null)
            {
                break;   // queue returns null only after shutdown
            }

            try
            {
                _channels[message.Channel].Send(message);
            }
            catch (Exception ex)
            {
                message.Attempt++;
                if (_retryPolicy.ShouldRetry(message.Attempt))
                {
                    message.NextAttemptAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                                          + _retryPolicy.NextDelayMs(message.Attempt);
                    _queue.Enqueue(message);
                }
                else
                {
                    lock (_deadLetters)
                    {
                        _deadLetters.Add(message);
                    }
                    Console.WriteLine($"[DLQ] {message.Id} failed: {ex.Message}");
                }
            }
        }
    }
}
```

NotificationService

```cs
using System;
using System.Collections.Generic;

public class NotificationService
{
    private readonly SubscriptionRegistry _registry = new();
    private readonly DelayQueue _queue = new();
    private readonly List<Message> _deadLetters = new();
    private readonly List<DeliveryWorker> _workers = new();
    private readonly Dictionary<ChannelType, IChannel> _channels;
    private readonly Func<string, ChannelType, object, string> _render;

    public NotificationService(Dictionary<ChannelType, IChannel> channels,
                               IRetryPolicy retryPolicy,
                               int workerCount,
                               Func<string, ChannelType, object, string> render)
    {
        _channels = channels;
        _render = render;

        for (var i = 0; i < workerCount; i++)
        {
            _workers.Add(new DeliveryWorker(_queue, _channels, retryPolicy, _deadLetters));
        }
    }

    public void Start() => _workers.ForEach(w => w.Start());

    public void Subscribe(string userId, string topic, IEnumerable<ChannelType> channels)
        => _registry.Subscribe(userId, topic, channels);

    public void Unsubscribe(string userId, string topic) => _registry.Unsubscribe(userId, topic);

    public void Publish(string topic, object payload)
    {
        foreach (var sub in _registry.GetSubscribers(topic))
        {
            foreach (var channel in sub.Channels)
            {
                if (!_channels.ContainsKey(channel))
                {
                    continue;
                }

                var body = _render(topic, channel, payload);
                _queue.Enqueue(new Message(Guid.NewGuid().ToString(), sub.UserId, channel, body));
            }
        }
    }

    public IReadOnlyList<Message> DeadLetters => _deadLetters;

    public void Shutdown()
    {
        _queue.Shutdown();
        _workers.ForEach(w => w.Stop());
    }
}
```

Usage

```cs
var service = new NotificationService(
    channels: new Dictionary<ChannelType, IChannel>
    {
        [ChannelType.Email] = new EmailChannel(),
        [ChannelType.Sms]   = new SmsChannel(),
        [ChannelType.Push]  = new PushChannel()
    },
    retryPolicy: new ExponentialBackoffPolicy(maxAttempts: 3, baseDelayMs: 1000),
    workerCount: 4,
    render: (topic, channel, payload) => channel == ChannelType.Sms
        ? $"{topic}: {payload}"
        : $"<h1>{topic}</h1><p>{payload}</p>");

service.Start();
service.Subscribe("u1", "ORDER_SHIPPED", new[] { ChannelType.Email, ChannelType.Push });
service.Publish("ORDER_SHIPPED", new { OrderId = "o-42" });
```

## Extensibility

### "How would you add a WhatsApp channel?"

Implement `IChannel`, add the enum value, register it in the channel map and add a template.
Workers and the service are untouched.

### "How do you swap the in-memory queue for Kafka/SQS?"

Extract `IMessageQueue { Enqueue, Dequeue }` - everything already talks through it. A
`KafkaMessageQueue` implements retries with a delayed topic; SQS uses visibility timeouts
instead of the in-process delay heap.

### "A slow SMS provider blocks all deliveries - how do you fix that?"

Bulkhead: one queue plus a dedicated worker pool per channel, so SMS backpressure cannot
starve email. Add a circuit breaker per channel - after N consecutive failures, open the
circuit and fail fast straight into the retry path instead of burning worker time.

### "How do you avoid duplicate notifications?"

Idempotency key = hash(eventId, userId, channel). Keep a short-TTL "already delivered" set
and skip repeats. This matters because a retry can double-send when the provider actually
succeeded but the response was lost.

### "How would you support quiet hours or digest mode?"

Add a filter stage between fan-out and enqueue:

```
interface DeliveryFilter:
    shouldDeliver(userId, channel, message) -> boolean
```

Quiet hours returns false and reschedules for morning by setting `nextAttemptAt`. Digest
mode accumulates into a per-user bucket flushed by a timer job.

### "How would you prioritise OTP over marketing messages?"

Add a `priority` field and use two queues (or a priority-ordered queue). Workers drain HIGH
first; low-priority traffic never delays an OTP.

### "How do you monitor this?"

Counters per channel - published, delivered, retried, dead-lettered - plus a histogram of
enqueue-to-delivery latency. Alert on queue depth and DLQ growth rate.
