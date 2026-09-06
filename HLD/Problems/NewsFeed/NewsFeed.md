# Newsfeed

Facebook is a social network which pioneered the News Feed, a product which shows recent stories from users in your social graph.

## Requirements

![alt text](image.png)

## Core Entities

- User
- Follow
- Post

## API

```
POST /posts
{
    "content": { }
}
// -> 200 OK
{
    "postId": // ...
}

PUT /users/[id]/follow
{ }
// -> 200 OK

GET /feed?pageSize={size}&cursor={timestamp?}
{
    items: Post[],
    nextCursor: string
}
```

## HLD

### Users should be able to create posts

![alt text](image-1.png)

### Users should be able to friend/follow people

![alt text](image-2.png)

- GSI : Globally Secondary Index ~ Auto maintained by DynamoDB

### Users should be able to view a feed of posts from people they follow

![alt text](image-3.png)

### Users should be able to page through their feed

- Cursor based pagination over timestamp

## Deep Dive

### How do we handle users who are following a large number of users?

- Fanout on Write
- On post creation, service precompute the feed and store it in the database

![alt text](image-4.png)

### How do we handle users with a large number of followers?

BAD - Query the DB

GOOD - Async Workers to generate the feed

- Large overhead on workers
- ![alt text](image-5.png)

GREAT - Async workers with Hybrid Feed

- For celebrity account dont update the precomputed feed, as will require massive write
- Use hybrid feed model, precomputed feed + celebrity posts
  ![alt text](image-6.png)

### How can we handle uneven reads of Posts?

GOOD - Post Cache with Large Keyspace

- Cause hotspot in cache

GREAT - Redundant Post Cache

- Sharded and Replicated Cache Instances
- LRU Eviction Policy
- ![alt text](image-7.png)

## Final Design

![alt text](image-8.png)
