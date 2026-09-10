# Task / Job Scheduler (Thread Safe)

An in-process scheduler that runs one-off and recurring jobs at a given time, on a fixed
pool of worker threads, safely under concurrent submission and cancellation.

## Prompt

```
"Design an in-memory job scheduler. Callers submit jobs to run once after a delay, at a
fixed rate, or via a cron-like schedule. A fixed pool of worker threads executes them.

schedule({
  "jobId": "reportJob",
  "type": "FIXED_RATE",
  "intervalMs": 60000,
  "task": () => generateReport()
})

The scheduler must be thread safe: many threads submit and cancel jobs concurrently while
workers execute them."
```

## Questions

```
"Single process or distributed across machines?"
> Single process, in-memory

"What schedule types do we support?"
> One-time (delay), fixed rate, fixed delay

"How many jobs run in parallel?"
> Configurable worker pool size

"Can the same job overlap with itself if it runs long?"
> No, skip or defer the next run

"Can jobs be cancelled mid-flight?"
> Cancel prevents future runs, current run finishes

"What if a job throws?"
> Log it, do not kill the worker, keep the schedule alive

"Do we need priorities?"
> Not now, earliest due time wins

"Persistence across restarts?"
> Out of scope
```

## Requirements

```
Requirements:
1. schedule(job) supports:
   - ONE_TIME    : run once after delayMs
   - FIXED_RATE  : run every intervalMs measured from scheduled start
   - FIXED_DELAY : run intervalMs after the previous run finishes
2. A fixed pool of N worker threads executes due jobs
3. Jobs execute at or after their scheduled time (no early execution)
4. cancel(jobId) stops all future runs; an in-flight run is allowed to finish
5. Thread safe: concurrent schedule / cancel / execute
6. A job that throws must not kill its worker or its schedule
7. The same job never runs concurrently with itself
8. shutdown() stops accepting jobs and drains workers cleanly

Out of scope:
- Persistence and crash recovery
- Distributed coordination / leader election
- Cron expression parsing (pluggable trigger instead)
- Job dependencies (DAG)
```

## Core Entities

```
JobScheduler   : Orchestrator, public API
Job            : Definition (id, task, trigger, state)
Trigger        : Strategy - computes the next run time
JobQueue       : Thread-safe min-heap ordered by nextRunTime
WorkerThread   : Pulls due jobs and executes them
JobStatus      : SCHEDULED, RUNNING, CANCELLED, COMPLETED
```

**Why these patterns**

| Concern | Pattern | Reason |
|---|---|---|
| Schedule types | Strategy (`Trigger`) | Cron/backoff later without touching workers |
| Dispatch | Producer-Consumer | Fixed pool absorbs bursts |
| Waking at the right time | Monitor + timed wait | No busy spin, no missed jobs |

## Class Design

```
class JobScheduler:
    - queue: JobQueue
    - jobs: ConcurrentMap<string, Job>
    - workers: List<WorkerThread>
    - running: volatile boolean

    + JobScheduler(poolSize)
    + scheduleOnce(jobId, task, delayMs) -> void
    + scheduleAtFixedRate(jobId, task, initialDelayMs, intervalMs) -> void
    + scheduleWithFixedDelay(jobId, task, initialDelayMs, intervalMs) -> void
    + cancel(jobId) -> boolean
    + shutdown() -> void

class Job:
    - id: string
    - task: Runnable
    - trigger: Trigger
    - nextRunTime: long
    - status: JobStatus        // guarded by the job's own lock
    - cancelled: boolean

    + execute() -> void
    + cancel() -> void

interface Trigger:
    + nextRunTime(scheduledTime, actualFinishTime) -> long | null
    // null => no further runs

class OneTimeTrigger implements Trigger
class FixedRateTrigger implements Trigger:
    - intervalMs: long
class FixedDelayTrigger implements Trigger:
    - intervalMs: long

class JobQueue:                 // the only shared mutable structure
    - heap: MinHeap<Job> by nextRunTime
    - lock: Monitor

    + add(job) -> void
    + takeDue() -> Job          // blocks until a job is due
    + remove(jobId) -> void
    + shutdown() -> void
```

**Concurrency model**

```
                 schedule()  cancel()          (many caller threads)
                      |         |
                      v         v
              +-------------------------+
              |   JobQueue (1 lock)     |   heap ordered by nextRunTime
              |   Monitor.Wait(timeout) |
              +-------------------------+
                 |        |        |
              worker1  worker2  worker3        (N worker threads)
                 |
            job.execute()  -> reschedule via trigger -> queue.add()
```

Three rules keep this safe:

1. **One lock owns the queue.** All mutation of the heap happens inside it.
2. **Timed wait, not spin.** A worker waits exactly until the head job is due; `Pulse` on
   insert re-evaluates when an earlier job arrives.
3. **Job state is guarded per job.** A job is removed from the queue before running, so it
   cannot be picked up twice - self-overlap is impossible by construction.

## Implementation

JobQueue.takeDue - the heart of the scheduler

```
takeDue()
    lock(monitor)
        while running
            if heap.isEmpty()
                monitor.wait()                 // nothing to do, sleep until an insert
                continue

            head = heap.peek()
            delay = head.nextRunTime - now()

            if delay <= 0
                heap.pop()
                if head.cancelled: continue    // drop cancelled jobs lazily
                return head

            monitor.wait(delay)                // wake when due OR when an earlier job arrives
        return null
```

add / cancel

```
add(job)
    lock(monitor)
        heap.push(job)
        monitor.pulseAll()      // an earlier job may now be at the head

cancel(jobId)
    job = jobs.get(jobId)
    if job == null: return false
    job.cancelled = true        // volatile write; queue entry is dropped when popped
    jobs.remove(jobId)
    return true
```

Worker loop

```
run()
    while running
        job = queue.takeDue()
        if job == null: break

        try
            job.status = RUNNING
            job.task.run()
        catch error
            log("job {job.id} failed", error)     // never let it escape
        finally
            finished = now()
            next = job.trigger.nextRunTime(job.nextRunTime, finished)

            if next != null && !job.cancelled
                job.nextRunTime = next
                job.status = SCHEDULED
                queue.add(job)                    // reschedule only after the run completes
            else
                job.status = COMPLETED
                jobs.remove(job.id)
```

Because a recurring job is re-added only in `finally`, it is never in the queue while it is
running - that is what prevents self-overlap.

Triggers

```
OneTimeTrigger.nextRunTime(scheduled, finished)
    return null

FixedRateTrigger.nextRunTime(scheduled, finished)
    next = scheduled + intervalMs
    if next <= now()
        next = now()          // run drifted long; skip missed slots, do not pile up
    return next

FixedDelayTrigger.nextRunTime(scheduled, finished)
    return finished + intervalMs
```

Fixed rate anchors on the scheduled time (steady cadence); fixed delay anchors on the finish
time (steady gap). The "skip missed slots" clamp avoids a burst of catch-up runs after a slow
execution.

## Code

Trigger

```cs
using System;

public interface ITrigger
{
    long? NextRunTime(long scheduledTime, long finishedTime);
}

public class OneTimeTrigger : ITrigger
{
    public long? NextRunTime(long scheduledTime, long finishedTime) => null;
}

public class FixedRateTrigger : ITrigger
{
    private readonly long _intervalMs;

    public FixedRateTrigger(long intervalMs) => _intervalMs = intervalMs;

    public long? NextRunTime(long scheduledTime, long finishedTime)
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var next = scheduledTime + _intervalMs;
        return next <= now ? now : next;   // do not queue up missed slots
    }
}

public class FixedDelayTrigger : ITrigger
{
    private readonly long _intervalMs;

    public FixedDelayTrigger(long intervalMs) => _intervalMs = intervalMs;

    public long? NextRunTime(long scheduledTime, long finishedTime) => finishedTime + _intervalMs;
}
```

Job

```cs
using System;

public enum JobStatus { Scheduled, Running, Cancelled, Completed }

public class Job
{
    public string Id { get; }
    public Action Task { get; }
    public ITrigger Trigger { get; }
    public long NextRunTime { get; set; }
    public JobStatus Status { get; set; } = JobStatus.Scheduled;

    private volatile bool _cancelled;
    public bool IsCancelled => _cancelled;

    public Job(string id, Action task, ITrigger trigger, long firstRunTime)
    {
        Id = id;
        Task = task;
        Trigger = trigger;
        NextRunTime = firstRunTime;
    }

    public void Cancel()
    {
        _cancelled = true;
        Status = JobStatus.Cancelled;
    }
}
```

JobQueue

```cs
using System;
using System.Collections.Generic;
using System.Threading;

public class JobQueue
{
    private readonly List<Job> _heap = new();   // kept sorted by NextRunTime for clarity
    private readonly object _lock = new();
    private bool _running = true;

    public void Add(Job job)
    {
        lock (_lock)
        {
            if (!_running)
            {
                return;   // dropped after shutdown; callers are rejected in JobScheduler
            }

            var index = _heap.FindIndex(j => j.NextRunTime > job.NextRunTime);
            if (index < 0)
            {
                _heap.Add(job);
            }
            else
            {
                _heap.Insert(index, job);
            }

            Monitor.PulseAll(_lock);   // head may have changed - re-evaluate waits
        }
    }

    public Job TakeDue()
    {
        lock (_lock)
        {
            while (_running)
            {
                if (_heap.Count == 0)
                {
                    Monitor.Wait(_lock);
                    continue;
                }

                var head = _heap[0];
                var delay = head.NextRunTime - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                if (delay <= 0)
                {
                    _heap.RemoveAt(0);
                    if (head.IsCancelled)
                    {
                        continue;      // lazily discard cancelled jobs
                    }
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
            Monitor.PulseAll(_lock);   // release every waiting worker
        }
    }
}
```

JobScheduler

```cs
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

public class JobScheduler
{
    private readonly JobQueue _queue = new();
    private readonly ConcurrentDictionary<string, Job> _jobs = new();
    private readonly List<Thread> _workers = new();
    private volatile bool _running = true;

    public JobScheduler(int poolSize)
    {
        for (var i = 0; i < poolSize; i++)
        {
            var thread = new Thread(WorkerLoop) { IsBackground = true, Name = $"worker-{i}" };
            _workers.Add(thread);
            thread.Start();
        }
    }

    public void ScheduleOnce(string jobId, Action task, long delayMs)
        => Schedule(jobId, task, new OneTimeTrigger(), delayMs);

    public void ScheduleAtFixedRate(string jobId, Action task, long initialDelayMs, long intervalMs)
        => Schedule(jobId, task, new FixedRateTrigger(intervalMs), initialDelayMs);

    public void ScheduleWithFixedDelay(string jobId, Action task, long initialDelayMs, long intervalMs)
        => Schedule(jobId, task, new FixedDelayTrigger(intervalMs), initialDelayMs);

    private void Schedule(string jobId, Action task, ITrigger trigger, long delayMs)
    {
        if (!_running)
        {
            throw new InvalidOperationException("Scheduler is shut down");
        }

        var firstRun = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + delayMs;
        var job = new Job(jobId, task, trigger, firstRun);

        if (!_jobs.TryAdd(jobId, job))
        {
            throw new InvalidOperationException($"Job {jobId} already scheduled");
        }

        _queue.Add(job);
    }

    public bool Cancel(string jobId)
    {
        if (!_jobs.TryRemove(jobId, out var job))
        {
            return false;
        }

        job.Cancel();   // future runs suppressed; an in-flight run finishes normally
        return true;
    }

    private void WorkerLoop()
    {
        while (_running)
        {
            var job = _queue.TakeDue();
            if (job == null)
            {
                break;
            }

            var scheduledTime = job.NextRunTime;

            try
            {
                job.Status = JobStatus.Running;
                job.Task();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[job:{job.Id}] failed: {ex.Message}");
            }
            finally
            {
                Reschedule(job, scheduledTime);
            }
        }
    }

    private void Reschedule(Job job, long scheduledTime)
    {
        var finished = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var next = job.Trigger.NextRunTime(scheduledTime, finished);

        if (next.HasValue && !job.IsCancelled && _running)
        {
            job.NextRunTime = next.Value;
            job.Status = JobStatus.Scheduled;
            _queue.Add(job);
            return;
        }

        job.Status = JobStatus.Completed;
        _jobs.TryRemove(job.Id, out _);
    }

    public void Shutdown()
    {
        _running = false;
        _queue.Shutdown();
        foreach (var worker in _workers)
        {
            worker.Join(2000);
        }
    }
}
```

Usage

```cs
var scheduler = new JobScheduler(poolSize: 4);

scheduler.ScheduleOnce("welcome-email", () => SendEmail(), delayMs: 5000);
scheduler.ScheduleAtFixedRate("metrics", () => FlushMetrics(), initialDelayMs: 0, intervalMs: 10000);
scheduler.ScheduleWithFixedDelay("cleanup", () => PurgeTemp(), initialDelayMs: 1000, intervalMs: 60000);

scheduler.Cancel("metrics");
scheduler.Shutdown();
```

## Thread Safety Summary

| Shared state | Protection |
|---|---|
| Heap of pending jobs | Single `lock` inside `JobQueue` |
| Worker wakeups | `Monitor.Wait(timeout)` + `PulseAll` on every insert / shutdown |
| Job registry | `ConcurrentDictionary` |
| Cancellation flag | `volatile bool`, checked on pop and before reschedule |
| Self-overlap | Job is out of the queue while running; re-added only in `finally` |
| Worker survival | Every task body wrapped in try/catch |

Deliberately avoided: a lock per job plus the queue lock (nested locks invite deadlock).
The queue lock is held only for heap operations, never while a task runs.

## Extensibility

### "How would you add cron support?"

New `ITrigger`:

```
class CronTrigger implements Trigger:
    - expression: string
    nextRunTime(scheduled, finished) -> parse expression, return next matching instant
```

Nothing else changes - that is the payoff of the Strategy boundary.

### "The sorted list insert is O(n) - does that matter?"

For a few thousand jobs, no. Swap the `List` for a binary min-heap (O(log n) insert/pop) or a
`PriorityQueue`; the lock protocol stays identical. At very large scale use hashed timing
wheels, which give O(1) insert for coarse-grained ticks.

### "How do you cancel a long-running job mid-execution?"

Cooperative cancellation only - you cannot safely kill a thread. Pass a `CancellationToken`
into the task and have it poll:

```
scheduleOnce(id, token => { while (!token.IsCancellationRequested) { ... } }, delay)
```

`Cancel` then signals the token in addition to suppressing future runs.

### "One slow job starves the pool - how do you contain it?"

Either bound execution with a timeout wrapper and log an overrun, or split into pools by job
class (fast/slow) so long ETL jobs cannot occupy every worker. A single-threaded scheduler
thread that hands work to an executor is the usual production shape.

### "How would you make this survive restarts?"

Persist job definitions plus `nextRunTime`; on boot, reload and re-queue. Anything whose time
passed while down either fires immediately or is skipped based on a misfire policy
(`FIRE_NOW` vs `SKIP`).

### "How does this become distributed?"

Move the queue to a shared store (DB table or Redis sorted set keyed by nextRunTime). Each
node atomically claims a due job (`UPDATE ... WHERE status='SCHEDULED'` / `ZPOPMIN`) so only
one node runs it, and holds a lease with a heartbeat so a crashed node's job is reclaimed.
