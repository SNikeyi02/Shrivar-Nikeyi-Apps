# Banking App

A Java Swing-based desktop application that provides basic login validation, transaction recording, and unit testing using JUnit.

---

## Project Architecture

The project consists of three core source files under the `BankingApp` package:

| File Name | Class | Description |
| :--- | :--- | :--- |
| `BankingApp.java` | `BankingApp` | Contains the `main` method, authentication logic, user menu interface, and input handlers using `JOptionPane`. |
| `Transactions.java` | `Transactions` | Model class handling transaction objects, static counters, auto-generation of IDs, and string formatted output. |
| `TransactionsTest.java` | `TransactionsTest` | JUnit unit test suite for verifying total amount aggregation and ID format generation. |

---

## Validation & Logic Specifications

### 1. User Authentication Rules
To log in successfully, user inputs must meet the following criteria:

* **Account ID Requirements (`checkAccountID`):** Must contain both a `$` and an `*` symbol anywhere in the string.
* **Password Requirements (`checkPassword`):** Must contain at least **one digit** and at least **one whitespace/space** character.

---

### 2. ID Generation Formula (`generateID`)
Transaction IDs are auto-generated dynamically using the following structure:

```text
[First 3 Letters of Transaction Name] + "#" + [Sequential Transaction Number] + "*"
```

* **Example:** A transaction named `Rent` created as the 3rd overall transaction results in an ID of **`Ren#3*`**.

---

## Application Navigation Flow

```text
+------------------------------------+
|            App Start               |
+------------------------------------+
                  |
                  v
+------------------------------------+
|        Authentication Loop         |
|  - Account ID ($ & * required)     |
|  - Password (digit & space req)    |
+------------------------------------+
                  |
            [Valid Login]
                  v
+------------------------------------+
|             Main Menu              |
|  1: Make Transaction               |
|  2: Bank Report (Future)           |
|  3: Exit                           |
+------------------------------------+
```

---

## Unit Testing

The `TransactionsTest` class validates application integrity using JUnit assertions:

```java
// Asserts that accumulated transaction values match the expected subtotal
assertEquals(expectedtotalAmount, actualtotalAmount, 0);

// Asserts that generated IDs conform to expected substring and index rules
assertEquals(expectedID, actualID);
```

### Running Tests
1. Ensure JUnit 4 (or compatible version) JAR is added to your project's build path.
2. Run `TransactionsTest.java` directly through your IDE test runner.

---

ation**:
   ```bash
   java BankingApp.BankingApp
   ```
