# ST10435835-PROG5121-POE

# HeisenBugs Task Management System

A Java Swing-based application designed for secure user authentication, task creation, tracking, and report generation using `JOptionPane` and array data structures.

---

## Project Architecture

The project is structured under the `st10435835` package and contains four primary source files:

| File Name | Class | Description |
| :--- | :--- | :--- |
| `ST10435835.java` | `ST10435835` | Contains the `main` method that coordinates the workflow between `Login` and `Task` modules. |
| `Login.java` | `Login` | Handles user registration, credentials validation (regex, length checks), and user login flows. |
| `Task.java` | `Task` | Manages task entries, array stores (max 20), task status tracking, ID generation, and reports. |
| `LoginTest.java` | `LoginTest` | JUnit test suite for testing username format, password complexity, and user authentication logic. |
| `TaskTest.java` | `TaskTest` | JUnit test suite for testing task validation, unique ID creation, duration calculations, and array manipulation. |

---

## 🛠️ Validation Rules & Formatting Specifications

### 1. User Registration & Login Requirements

* **Username Specifications (`checkUsername`):**
  * Maximum length of **5 characters**.
  * Must contain an underscore (`_`).

* **Password Specifications (`checkPasswordComplexity`):**
  * Length between **8 and 20 characters**.
  * Must contain at least **one uppercase letter**, **one lowercase letter**, **one digit**, and **one special character**.

---

### 2. Task Description & Unique ID Rules

* **Task Description (`checkTaskDescription`):** Must be strictly **less than 50 characters**.
* **Task ID Generation (`createTaskID`):**
  Auto-generated in upper case using the format:
  ```text
  [FIRST 2 LETTERS OF TASK NAME] : [TASK NUMBER] : [LAST 3 LETTERS OF DEV NAME]

Example: Task "Login Feature", Task Number 0, Developer "Robyn" results in LO:0:BYN.











