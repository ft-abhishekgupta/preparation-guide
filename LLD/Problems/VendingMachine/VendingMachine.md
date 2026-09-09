# Vending Machine

## Requirements

## Class Design
class Product
    - id: string
    - name: string
    - price: decimal


enum MoneyType
    Coin
    Note


class Money
    - denomination: decimal
    - type: MoneyType


class Inventory
    - products: ConcurrentDictionary<string, Product>
    - quantities: ConcurrentDictionary<string, int>

    + AddProduct(product, quantity)
    + RemoveProduct(productId): bool
    + GetProduct(productId): Product
    + GetQuantity(productId): int
    + IsAvailable(productId): bool


class CashManager
    - cash: Dictionary<decimal, int>
    - lock: object

    + AddMoney(money)
    + CalculateChange(amount): List<Money>
    + RemoveChange(change)
    + GetTotalCash(): decimal
    + CollectAllMoney(): List<Money>


interface IVendingMachineState
    + SelectProduct(machine, productId)
    + InsertMoney(machine, money)
    + Dispense(machine)
    + Cancel(machine)


class IdleState : IVendingMachineState
class ProductSelectedState : IVendingMachineState
class AcceptingMoneyState : IVendingMachineState
class DispensingState : IVendingMachineState
class ReturningChangeState : IVendingMachineState


class VendingMachine
    - state: IVendingMachineState
    - inventory: Inventory
    - cashManager: CashManager

    - selectedProduct: Product?
    - insertedAmount: decimal

    - transactionLock: object

    + SelectProduct(productId)
    + InsertMoney(money)
    + Dispense()
    + Cancel()

    + Restock(productId, quantity)
    + CollectMoney(): List<Money>

    + SetState(state)

    + GetSelectedProduct(): Product?
    + GetInsertedAmount(): decimal
    + AddInsertedAmount(amount)
    + ResetTransaction()
```
```
  ┌──────────────┐
  │    Idle      │
  └──────┬───────┘
         │ select product
         ↓
  ┌──────────────┐
  │  Product     │
  │  Selected    │
  └──────┬───────┘
         │ insert money
         ↓
  ┌──────────────┐
  │  Accepting   │
  │    Money     │
  └──────┬───────┘
         │ sufficient money
         ↓
  ┌──────────────┐
  │  Dispensing  │
  └──────┬───────┘
         │
         ↓
  ┌──────────────┐
  │ Return       │
  │ Change       │
  └──────┬───────┘
         │
         ↓
       Idle
       
Out of stock       → Idle
Insufficient money → AcceptingMoney / Selected
Cancel             → ReturnChange → Idle
```