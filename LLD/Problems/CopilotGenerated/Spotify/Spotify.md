# Spotify (Music Streaming)

A music streaming service: browse a catalog, build playlists, and play a queue with shuffle
and repeat modes, notifying listeners as playback changes.

## Prompt

```
"Design a music streaming service like Spotify. Users search a catalog, create playlists,
and play songs. The player supports play/pause/next/previous, shuffle and repeat.

play({ "userId": "u1", "playlistId": "p1" })

Focus on the playback engine - the queue, shuffle/repeat behaviour, and how state changes
propagate."
```

## Questions

```
"Playback of real audio, or just state management?"
> State management, streaming bytes is out of scope

"What play modes do we need?"
> Shuffle on/off, and repeat OFF / REPEAT_ONE / REPEAT_ALL

"Can a user queue a song ahead of the current playlist?"
> Yes, 'play next' - a priority queue in front

"Does shuffle reshuffle every loop?"
> Yes, generate a new order each pass

"What can a user search by?"
> Song title, artist, album

"Do we track listening history / recently played?"
> Yes, simple recent list

"Multiple devices playing at once?"
> One active session per user for now

"Free vs premium restrictions?"
> Discuss it in extensibility
```

## Requirements

```
Requirements:
1. Catalog of songs with title, artist, album, duration
2. Search by title / artist / album, case-insensitive substring
3. Users create playlists: add, remove, reorder songs
4. Player operations: play, pause, resume, next, previous, seek, stop
5. Play a playlist, an album, or a single song
6. Shuffle on/off - a new random order each pass, current song stays playing
7. Repeat modes: OFF (stop at end), REPEAT_ONE, REPEAT_ALL
8. "Play next" inserts a song ahead of the normal queue
9. Playback state changes notify subscribers (UI, scrobbler, history)
10. Track recently played songs per user

Out of scope:
- Audio decoding / streaming / buffering
- Recommendations and radio
- Offline downloads, DRM
- Social features, collaborative playlists
```

## Core Entities

```
MusicService    : Facade - search, playlists, playback entry points
Song / Album / Artist : Catalog value objects
Playlist        : Ordered, mutable list of songs
PlayQueue       : What plays next - upNext + main list + play order
Player          : Playback state machine (PLAYING/PAUSED/STOPPED)
PlayStrategy    : Strategy - Sequential vs Shuffle ordering
RepeatMode      : OFF | REPEAT_ONE | REPEAT_ALL
PlaybackListener: Observer - UI, history, analytics
SearchService   : Indexed catalog lookup
```

**Why these patterns**

| Concern | Pattern | Reason |
|---|---|---|
| Shuffle vs sequential | Strategy | Ordering swaps at runtime without touching Player |
| Playback state | State machine | play/pause/next legality depends on state |
| UI + history + analytics updates | Observer | Player stays unaware of consumers |
| Search | Inverted index | Avoids scanning the whole catalog per keystroke |

The key modelling insight: **the queue owns ordering, the player owns state**. Keeping them
separate is what makes shuffle a 10-line class instead of conditionals scattered everywhere.

## Class Design

```
class MusicService:
    - catalog: Catalog
    - search: SearchService
    - playlists: Map<string, Playlist>
    - players: Map<userId, Player>

    + search(query) -> List<Song>
    + createPlaylist(userId, name) -> Playlist
    + playPlaylist(userId, playlistId) -> void
    + getPlayer(userId) -> Player

class Song:
    - id, title, artistId, albumId: string
    - durationSec: int

class Playlist:
    - id, name, ownerId: string
    - songs: List<Song>

    + add(song) / remove(songId) / move(from, to)

class PlayQueue:
    - songs: List<Song>          // the source list
    - order: List<int>           // indices, produced by the strategy
    - position: int              // index into order
    - upNext: Deque<Song>        // 'play next' overrides

    + load(songs, strategy) -> void
    + current() -> Song | null
    + next(repeatMode) -> Song | null
    + previous() -> Song | null
    + addNext(song) -> void
    + setStrategy(strategy) -> void   // keeps the current song playing

class Player:
    - queue: PlayQueue
    - state: PlaybackState        // PLAYING, PAUSED, STOPPED
    - positionSec: int
    - repeatMode: RepeatMode
    - shuffle: boolean
    - listeners: List<PlaybackListener>

    + play(songs) / pause() / resume() / stop()
    + next() / previous() / seek(sec)
    + setShuffle(on) / setRepeat(mode)

interface PlayStrategy:
    + buildOrder(size, startIndex) -> List<int>

class SequentialStrategy implements PlayStrategy
class ShuffleStrategy implements PlayStrategy

enum RepeatMode: OFF, REPEAT_ONE, REPEAT_ALL
enum PlaybackState: PLAYING, PAUSED, STOPPED

interface PlaybackListener:
    + onSongChanged(song) -> void
    + onStateChanged(state) -> void
```

**Queue model**

```
  upNext:  [ songX ]                <- 'play next', consumed first, then discarded
  songs:   [ A, B, C, D, E ]        <- the loaded playlist, never reordered
  order:   [ 2, 0, 4, 1, 3 ]        <- shuffle output (indices into songs)
  position:      ^ = 1              <- currently playing songs[order[1]] = A

  next() -> upNext non-empty ? pop it
                             : advance position through `order`
```

Shuffling an index list rather than the song list means toggling shuffle off restores the
original order for free, and the playlist itself is never mutated by playback.

## Implementation

Player.play / next

```
play(songs)
    queue.load(songs, currentStrategy())
    state = PLAYING
    positionSec = 0
    notifySongChanged(queue.current())
    notifyStateChanged(PLAYING)

next()
    song = queue.next(repeatMode)

    if song == null
        stop()                        // end of queue, repeat OFF
        return

    positionSec = 0
    state = PLAYING
    notifySongChanged(song)

previous()
    if positionSec > 3               // standard behaviour: restart before skipping back
        seek(0)
        return

    song = queue.previous()
    if song != null
        positionSec = 0
        notifySongChanged(song)
```

PlayQueue.next - where repeat modes live

```
next(repeatMode)
    if !upNext.isEmpty()
        return upNext.pop()           // manual queue wins over everything

    if repeatMode == REPEAT_ONE
        return current()              // same song, position untouched

    if position + 1 < order.size()
        position += 1
        return current()

    // reached the end
    if repeatMode == REPEAT_ALL
        order = strategy.buildOrder(songs.size(), startIndex: null)  // reshuffle each pass
        position = 0
        return current()

    return null                       // repeat OFF -> stop

current()
    if order.isEmpty(): return null
    return songs[order[position]]

previous()
    if position == 0: return current()
    position -= 1
    return current()
```

Toggling shuffle without interrupting the song

```
setStrategy(newStrategy)
    playingIndex = order[position]                  // remember the actual song

    order = newStrategy.buildOrder(songs.size(), startIndex: playingIndex)
    position = order.indexOf(playingIndex)          // keep playing the same song
    strategy = newStrategy
```

This is the subtle requirement most candidates miss: switching shuffle must not restart or
skip the current track.

Strategies

```
SequentialStrategy.buildOrder(size, startIndex)
    return [0, 1, 2, ..., size-1]

ShuffleStrategy.buildOrder(size, startIndex)
    indices = [0 .. size-1]
    fisherYatesShuffle(indices)

    if startIndex != null
        swap(indices, 0, indices.indexOf(startIndex))   // current song goes first
    return indices
```

Search index

```
buildIndex(songs)
    for song in songs
        for token in tokenize(song.title + " " + artistName + " " + albumName)
            index[token.toLower()].add(song.id)

search(query)
    tokens = tokenize(query.toLower())
    result = null

    for token in tokens
        matches = union of index[k] for every k starting with token   // prefix match
        result = (result == null) ? matches : intersect(result, matches)

    return result.map(id -> catalog[id])
```

## Code

Catalog entities

```cs
public class Song
{
    public string Id { get; }
    public string Title { get; }
    public string Artist { get; }
    public string Album { get; }
    public int DurationSec { get; }

    public Song(string id, string title, string artist, string album, int durationSec)
    {
        Id = id;
        Title = title;
        Artist = artist;
        Album = album;
        DurationSec = durationSec;
    }
}
```

Playlist

```cs
using System.Collections.Generic;

public class Playlist
{
    private readonly List<Song> _songs = new();

    public string Id { get; }
    public string Name { get; }
    public string OwnerId { get; }

    public Playlist(string id, string name, string ownerId)
    {
        Id = id;
        Name = name;
        OwnerId = ownerId;
    }

    public void Add(Song song) => _songs.Add(song);

    public void Remove(string songId) => _songs.RemoveAll(s => s.Id == songId);

    public void Move(int from, int to)
    {
        if (from < 0 || from >= _songs.Count || to < 0 || to >= _songs.Count)
        {
            return;
        }

        var song = _songs[from];
        _songs.RemoveAt(from);
        _songs.Insert(to, song);
    }

    public IReadOnlyList<Song> Songs => _songs;
}
```

PlayStrategy

```cs
using System;
using System.Collections.Generic;
using System.Linq;

public interface IPlayStrategy
{
    List<int> BuildOrder(int size, int? startIndex);
}

public class SequentialStrategy : IPlayStrategy
{
    public List<int> BuildOrder(int size, int? startIndex) => Enumerable.Range(0, size).ToList();
}

public class ShuffleStrategy : IPlayStrategy
{
    private readonly Random _random = new();

    public List<int> BuildOrder(int size, int? startIndex)
    {
        var indices = Enumerable.Range(0, size).ToList();

        for (var i = size - 1; i > 0; i--)      // Fisher-Yates
        {
            var j = _random.Next(i + 1);
            (indices[i], indices[j]) = (indices[j], indices[i]);
        }

        if (startIndex.HasValue)
        {
            var pos = indices.IndexOf(startIndex.Value);
            (indices[0], indices[pos]) = (indices[pos], indices[0]);
        }

        return indices;
    }
}
```

PlayQueue

```cs
using System.Collections.Generic;
using System.Linq;

public enum RepeatMode { Off, RepeatOne, RepeatAll }

public class PlayQueue
{
    private List<Song> _songs = new();
    private List<int> _order = new();
    private readonly LinkedList<Song> _upNext = new();
    private IPlayStrategy _strategy = new SequentialStrategy();
    private int _position;

    public void Load(IEnumerable<Song> songs, IPlayStrategy strategy)
    {
        _songs = songs.ToList();
        _strategy = strategy;
        _order = _strategy.BuildOrder(_songs.Count, null);
        _position = 0;
        _upNext.Clear();
    }

    public Song Current()
        => _order.Count == 0 ? null : _songs[_order[_position]];

    public Song Next(RepeatMode repeatMode)
    {
        if (_upNext.Count > 0)
        {
            var song = _upNext.First.Value;
            _upNext.RemoveFirst();
            return song;
        }

        if (_order.Count == 0)
        {
            return null;
        }

        if (repeatMode == RepeatMode.RepeatOne)
        {
            return Current();
        }

        if (_position + 1 < _order.Count)
        {
            _position++;
            return Current();
        }

        if (repeatMode == RepeatMode.RepeatAll)
        {
            _order = _strategy.BuildOrder(_songs.Count, null);   // reshuffle each pass
            _position = 0;
            return Current();
        }

        return null;
    }

    public Song Previous()
    {
        if (_order.Count == 0)
        {
            return null;
        }

        if (_position > 0)
        {
            _position--;
        }

        return Current();
    }

    public void AddNext(Song song) => _upNext.AddFirst(song);

    public void SetStrategy(IPlayStrategy strategy)
    {
        if (_order.Count == 0)
        {
            _strategy = strategy;
            return;
        }

        var playing = _order[_position];
        _order = strategy.BuildOrder(_songs.Count, playing);
        _position = _order.IndexOf(playing);   // keep the current song playing
        _strategy = strategy;
    }
}
```

Player

```cs
using System.Collections.Generic;

public enum PlaybackState { Playing, Paused, Stopped }

public interface IPlaybackListener
{
    void OnSongChanged(Song song);
    void OnStateChanged(PlaybackState state);
}

public class Player
{
    private readonly PlayQueue _queue = new();
    private readonly List<IPlaybackListener> _listeners = new();

    public PlaybackState State { get; private set; } = PlaybackState.Stopped;
    public int PositionSec { get; private set; }
    public RepeatMode Repeat { get; private set; } = RepeatMode.Off;
    public bool Shuffle { get; private set; }

    public void Subscribe(IPlaybackListener listener) => _listeners.Add(listener);

    public void Play(IEnumerable<Song> songs)
    {
        _queue.Load(songs, CurrentStrategy());
        PositionSec = 0;
        SetState(PlaybackState.Playing);
        NotifySong(_queue.Current());
    }

    public void Pause()
    {
        if (State == PlaybackState.Playing)
        {
            SetState(PlaybackState.Paused);
        }
    }

    public void Resume()
    {
        if (State == PlaybackState.Paused)
        {
            SetState(PlaybackState.Playing);
        }
    }

    public void Stop()
    {
        PositionSec = 0;
        SetState(PlaybackState.Stopped);
    }

    public void Next()
    {
        var song = _queue.Next(Repeat);

        if (song == null)
        {
            Stop();
            return;
        }

        PositionSec = 0;
        SetState(PlaybackState.Playing);
        NotifySong(song);
    }

    public void Previous()
    {
        if (PositionSec > 3)      // restart the track before skipping back
        {
            Seek(0);
            return;
        }

        var song = _queue.Previous();
        if (song != null)
        {
            PositionSec = 0;
            NotifySong(song);
        }
    }

    public void Seek(int seconds) => PositionSec = seconds;

    public void PlayNext(Song song) => _queue.AddNext(song);

    public void SetRepeat(RepeatMode mode) => Repeat = mode;

    public void SetShuffle(bool on)
    {
        Shuffle = on;
        _queue.SetStrategy(CurrentStrategy());
    }

    public Song CurrentSong() => _queue.Current();

    private IPlayStrategy CurrentStrategy()
        => Shuffle ? new ShuffleStrategy() : new SequentialStrategy();

    private void SetState(PlaybackState state)
    {
        State = state;
        _listeners.ForEach(l => l.OnStateChanged(state));
    }

    private void NotifySong(Song song) => _listeners.ForEach(l => l.OnSongChanged(song));
}
```

SearchService

```cs
using System.Collections.Generic;
using System.Linq;

public class SearchService
{
    private readonly Dictionary<string, HashSet<string>> _index = new();
    private readonly Dictionary<string, Song> _songs = new();

    public void Add(Song song)
    {
        _songs[song.Id] = song;

        foreach (var token in Tokenize($"{song.Title} {song.Artist} {song.Album}"))
        {
            if (!_index.TryGetValue(token, out var ids))
            {
                ids = new HashSet<string>();
                _index[token] = ids;
            }
            ids.Add(song.Id);
        }
    }

    public List<Song> Search(string query)
    {
        HashSet<string> result = null;

        foreach (var token in Tokenize(query))
        {
            var matches = new HashSet<string>(
                _index.Where(kv => kv.Key.StartsWith(token)).SelectMany(kv => kv.Value));

            if (result == null)
            {
                result = matches;
            }
            else
            {
                result.IntersectWith(matches);
            }
        }

        return result?.Select(id => _songs[id]).ToList() ?? new List<Song>();
    }

    private static IEnumerable<string> Tokenize(string text)
        => text.ToLowerInvariant().Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
}
```

History listener (Observer in action)

```cs
using System.Collections.Generic;
using System.Linq;

public class RecentlyPlayedListener : IPlaybackListener
{
    private readonly LinkedList<Song> _recent = new();
    private readonly int _capacity;

    public RecentlyPlayedListener(int capacity) => _capacity = capacity;

    public void OnSongChanged(Song song)
    {
        _recent.Remove(_recent.FirstOrDefault(s => s.Id == song.Id));
        _recent.AddFirst(song);

        if (_recent.Count > _capacity)
        {
            _recent.RemoveLast();
        }
    }

    public void OnStateChanged(PlaybackState state) { }

    public IReadOnlyList<Song> Recent => _recent.ToList();
}
```

Usage

```cs
var player = new Player();
player.Subscribe(new RecentlyPlayedListener(capacity: 50));

player.Play(playlist.Songs);
player.SetShuffle(true);          // reorders upcoming songs, current song keeps playing
player.SetRepeat(RepeatMode.RepeatAll);
player.PlayNext(someSong);        // jumps the queue
player.Next();                    // plays someSong
```

## Extensibility

### "How would you add a 'radio' mode that never ends?"

New `IPlayStrategy` that appends recommended songs when the order is nearly exhausted, or a
`PlayQueue` that pulls from an `ISongSource` instead of a fixed list. The player is unaware
the queue is infinite.

### "Free tier can only shuffle and gets ads - how do you model that?"

Decorator around `Player` or a policy object consulted before each operation:

```
class FreeTierPlayer decorates Player:
    setShuffle(false) -> throw / ignore
    next() -> if skipCount > 6 in this hour: reject
              if songsPlayed % 4 == 0: playAd()
```

Keeps entitlement rules out of the core playback logic.

### "Two devices for the same user - how do you keep them in sync?"

Move playback state to a server-side session per user, with devices as clients that send
commands and receive state events. Exactly one device holds the "active" role; a transfer
command moves it. The existing `PlaybackListener` interface becomes the push channel.

### "The search index rebuilds fully - how would you scale it?"

For a real catalog, delegate to a proper search engine (Elasticsearch) behind the same
`SearchService` interface. In-process, keep the inverted index but use a trie for prefix
matching rather than scanning every key, and pre-rank by popularity so top results appear
first.

### "How do you track plays for royalties?"

Another `PlaybackListener` that emits a play event once a song passes a threshold (30 seconds
is the industry convention). Because it is an observer, adding it does not touch `Player`.

### "How would you support collaborative playlists?"

Playlist mutations become ordered operations (add/remove/move) applied with a version number,
so concurrent edits merge deterministically instead of last-write-wins clobbering. Broadcast
each applied operation to collaborators.

### "How do you keep queue operations fast for a 10,000-song playlist?"

All the hot operations - `Current`, `Next`, `Previous`, `AddNext` - are already O(1). Only
`SetStrategy` is O(n) from the reshuffle plus `IndexOf`, and it happens on a user toggle, not
per track. Keeping an index-to-order-position map would make even that O(1) if needed.
