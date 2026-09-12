# Local Delivery Service

Gopuff delivers goods typically found in a convenience store via rapid delivery and 500+ micro distribution centers (DCs).

## Requirements

![alt text](image.png)

## Core Entities

```
Item
Inventory
Distribution Center
Order
OrderItem
```

## API

![alt text](image-1.png)

## HLD

### Customers should be able to query availability of items

![alt text](image-2.png)

### Customers should be able to order items

GOOD - 2 Data Stores with Distributed Lock

- Can cause deadlock or inconsistencies

BETTER - Single DB Transaction

![alt text](image-3.png)

## Deep Dive

### Make availability lookups incorporate traffic and drive time

BAD - Simple SQL Distance : Not realtime
BAD - Travel Time estimation across all DCs
GREAT - Travel Time estimation across nearby DCs

![alt text](image-4.png)

### Make availability lookups fast and scalable

GREAT - Redis for inventory

![alt text](image-5.png)

GREAT - DB Replication and Partitioning

![alt text](image-6.png)
