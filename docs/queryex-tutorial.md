# Queryex Language Tutorial

Queryex ("Query Expression") is Tellma's expression language for filtering, sorting, and selecting data through the API. It compiles to SQL Server T-SQL and is used in query string parameters like `filter`, `orderby`, `select`, and others.

This guide teaches the language from first principles with progressive examples.

---

## Table of Contents

1. [Where Queryex Is Used](#1-where-queryex-is-used)
2. [Literals and Values](#2-literals-and-values)
3. [Accessing Entity Properties](#3-accessing-entity-properties)
4. [Comparison Operators](#4-comparison-operators)
5. [Logical Operators](#5-logical-operators)
6. [Arithmetic Operators](#6-arithmetic-operators)
7. [String Operators](#7-string-operators)
8. [Built-in Functions](#8-built-in-functions)
9. [Aggregation Functions](#9-aggregation-functions)
10. [Null Handling](#10-null-handling)
11. [Operator Precedence](#11-operator-precedence)
12. [Query Parameter Reference](#12-query-parameter-reference)
13. [Common Patterns and Examples](#13-common-patterns-and-examples)
14. [Error Reference](#14-error-reference)

---

## 1. Where Queryex Is Used

Queryex expressions appear in API query string parameters. Different parameters accept different kinds of expressions:

| Parameter | What It Does | Expression Kind |
|---|---|---|
| `filter` | Filters rows (like SQL WHERE) | Boolean expression |
| `orderby` | Sorts results (like SQL ORDER BY) | Expression + `asc`/`desc` |
| `select` | Chooses which columns to return | Any non-boolean expression |
| `expand` | Loads related navigation properties | Property paths only |
| `aggregateselect` | Aggregates values (like SQL GROUP BY + SELECT) | Aggregation expressions |
| `aggregateorderby` | Sorts aggregated results | Aggregation expression + `asc`/`desc` |
| `having` | Filters aggregated results (like SQL HAVING) | Boolean aggregation expression |

**Example API call:**
```
GET /api/documents?filter=Value > 1000 and PostingDate >= '2024-01-01'&orderby=PostingDate desc
```

---

## 2. Literals and Values

These are the raw values you can write directly in an expression.

### Strings

Strings use **single quotes**:

```
'Hello'
'New York'
'Account-101'
```

To include a single quote inside a string, double it:

```
'It''s a nice day'        -- represents: It's a nice day
'O''Brien'                 -- represents: O'Brien
```

### Numbers

Write numbers directly, with or without a decimal point:

```
100
3.14
0.5
1000000
```

### Booleans

```
true
false
```

Case-insensitive: `True`, `TRUE`, `true` are all equivalent.

### Null

```
null
```

Represents the absence of a value.

### Date and Time Constants

These are written like function calls and return dynamic values at query time:

```
today()     -- today's date (no time component)
now()       -- current date and time with timezone offset
me()        -- the current user's ID (a number)
```

---

## 3. Accessing Entity Properties

To reference a field on an entity, write its name directly:

```
Name
Code
Value
IsActive
```

### Navigating to Related Entities

Use dot notation to access fields on related (navigation) entities:

```
Agent.Name              -- the Name of the related Agent
Account.Center.Code     -- the Code of the Center of the related Account
Resource.Lookup1.Name   -- deep navigation
```

Each segment must be a valid navigation property. Tellma resolves the correct SQL JOIN automatically.

### Rules for Property Names

- Must start with a letter
- Can contain letters, digits, and underscores
- Use `.` to navigate between related entities
- Cannot be reserved words: `null`, `true`, `false`, `asc`, `desc`

---

## 4. Comparison Operators

Comparison operators compare two values and return a boolean result. They are used primarily in `filter` expressions.

### Equality

```
Name = 'Expenses'
Code = 'ACC-101'
Value = 0
IsActive = true
```

`=` is **null-safe**: `null = null` evaluates to `true`.

### Inequality

Both forms are equivalent:

```
Code != 'ACC-101'
Code <> 'ACC-101'
```

### Relational Comparisons

```
Value > 1000
Amount <= 500
PostingDate >= '2024-01-01'
PostingDate < '2025-01-01'
```

### Alphabetic Aliases

These are equivalent alternatives, useful when the expression is embedded in a URL or context where `<` and `>` are awkward:

| Symbol | Alias |
|--------|-------|
| `=`  | `eq` |
| `!=` | `ne` |
| `<`  | `lt` |
| `<=` | `le` |
| `>`  | `gt` |
| `>=` | `ge` |

```
Value gt 1000
Amount le 500
```

### Comparing Dates

Dates can be compared directly using string literals in ISO 8601 format. Queryex converts them to the correct SQL type:

```
PostingDate = '2024-06-15'
CreatedAt >= '2024-01-01'
UpdatedAt < '2025-01-01T00:00:00'
```

### Null Checks

Use `= null` and `!= null` (not SQL `IS NULL`):

```
Agent = null                -- Agent is not set
CancellationDate != null    -- has been cancelled
```

---

## 5. Logical Operators

Combine boolean sub-expressions into more complex conditions.

### AND

All of the following are equivalent:

```
IsActive = true and Value > 100
IsActive = true && Value > 100
```

The result is `true` only when **both** sides are `true`.

### OR

All of the following are equivalent:

```
Type = 'Purchase' or Type = 'Sale'
Type = 'Purchase' || Type = 'Sale'
```

The result is `true` when **at least one** side is `true`.

### NOT

All of the following are equivalent:

```
not IsActive
!IsActive
```

Negates a boolean value.

### Combining Conditions

You can chain logical operators freely:

```
IsActive = true and Value > 0 and Agent != null
Type = 'Purchase' or Type = 'AdvancePayment' or Type = 'Refund'
```

Use parentheses to control grouping when mixing `and` and `or`:

```
(Type = 'Purchase' or Type = 'Sale') and PostingDate >= '2024-01-01'
```

Without parentheses, `and` binds more tightly than `or`, so:

```
A or B and C    -- means: A or (B and C)
```

---

## 6. Arithmetic Operators

Perform calculations on numeric values.

### Basic Operations

```
Value + 10
Value - DiscountAmount
Quantity * UnitPrice
Total / Count
Total % 12          -- remainder (modulo)
```

String concatenation also uses `+`:

```
FirstName + ' ' + LastName
```

### Unary Plus and Minus

```
-Value
+Amount
```

### Using Arithmetic in Filter

```
(Value * Direction) > 1000      -- signed value exceeds threshold
(Revenue - Cost) >= 0           -- profitable transactions
```

### Using Arithmetic in Select

```
Value * Direction               -- computed signed amount
Quantity * UnitPrice            -- line total
```

### Using Arithmetic in OrderBy

```
(Value * Direction) desc        -- sort by signed value, largest first
```

---

## 7. String Operators

These operators test whether a string contains, starts with, or ends with a substring. They return boolean values.

### `contains`

True if the string contains the given substring (case-insensitive):

```
Name contains 'Smith'
Code contains 'ACC'
Description contains 'invoice'
```

### `startsw`

True if the string starts with the given prefix:

```
Code startsw 'ACC'
Name startsw 'Al-'
```

### `endsw`

True if the string ends with the given suffix:

```
Code endsw '-001'
Name endsw 'Ltd'
```

### Combining String Operators

```
Name contains 'John' or Name contains 'Jane'
Code startsw 'ACC' and Code endsw '-01'
```

---

## 8. Built-in Functions

Functions are called with parentheses: `functionName(arg1, arg2, ...)`.

### Conditional Functions

#### `if(condition, valueIfTrue, valueIfFalse)`

Returns one of two values based on a condition:

```
if(IsActive, 'Active', 'Inactive')
if(Value > 0, Value, 0)
if(Agent != null, Agent.Name, 'No Agent')
```

The true and false branches must produce the same type.

#### `isnull(value, fallback)`

Returns the fallback when the value is null:

```
isnull(Agent.Name, 'Unknown')
isnull(DiscountAmount, 0)
```

### Date Functions

#### Extracting Date Parts

```
year(PostingDate)               -- 2024
month(PostingDate)              -- 6 (June)
day(PostingDate)                -- 15
quarter(PostingDate)            -- 2 (April–June)
weekday(PostingDate)            -- 1 (Sunday) through 7 (Saturday)
hour(CreatedAt)                 -- 0–23
minute(CreatedAt)               -- 0–59
second(CreatedAt)               -- 0–59
```

#### Calendar Support

The date-part functions accept an optional calendar argument:

```
year(PostingDate, 'gregorian')  -- default
year(PostingDate, 'umalqura')   -- Islamic (Umm al-Qura) calendar
year(PostingDate, 'ethiopian')  -- Ethiopian calendar
```

This is useful for ERP systems operating under non-Gregorian fiscal calendars.

#### Date Arithmetic

```
adddays(30, PostingDate)        -- 30 days after PostingDate
addmonths(-1, today())          -- one month ago
addyears(1, ContractStartDate)  -- one year after contract start
```

Note: the number comes **first**, then the date.

#### Date Boundaries

```
startofmonth(PostingDate)       -- first day of PostingDate's month
startofyear(PostingDate)        -- first day of PostingDate's year
startofmonth(today())           -- first day of current month
startofyear(today(), 'umalqura') -- first day of current Hijri year
```

#### Date Conversion

```
date(CreatedAt)     -- strip the time component, return DATE
```

#### Date Differences

```
diffdays(today(), ContractEndDate)      -- days until contract expires
diffhours(CreatedAt, UpdatedAt)         -- hours to process
diffminutes(StartTime, EndTime)         -- duration in minutes
diffseconds(StartTime, EndTime)         -- duration in seconds
```

### String Functions

```
len(Name)                       -- number of characters
upper(Name)                     -- 'john smith' → 'JOHN SMITH'
lower(Name)                     -- 'JOHN SMITH' → 'john smith'
trim(Name)                      -- remove leading and trailing spaces
left(Code, 3)                   -- first 3 characters
right(Code, 3)                  -- last 3 characters
mid(Code, 2, 4)                 -- 4 characters starting at position 2 (1-indexed)
mid(Code, 2)                    -- everything from position 2 to end
replace(Name, 'Ltd', 'LLC')     -- substitute all occurrences
```

### Numeric Functions

```
abs(Value * Direction)          -- absolute value
abs(-50)                        -- 50
```

---

## 9. Aggregation Functions

Aggregation functions summarize many rows into a single value. They are only valid in `aggregateselect`, `aggregateorderby`, and `having` parameters — not in `filter` or `select`.

### Basic Aggregations

```
sum(Value)          -- total of all values
count(Id)           -- count of non-null values
avg(Value)          -- average value
min(PostingDate)    -- earliest date
max(PostingDate)    -- latest date
```

### Conditional Aggregations

Each aggregation function accepts an optional second boolean argument that acts as a row filter:

```
sum(Value, Direction = 1)           -- sum only where Direction = 1
count(Id, IsActive = true)          -- count only active rows
avg(Value, Value > 0)               -- average of positive values only
```

### Example: Aggregation Query

```
aggregateselect=Agent.Name,sum(Value),count(Id)
&aggregateorderby=sum(Value) desc
&having=sum(Value) > 10000
```

This groups by `Agent.Name`, returns the total value and count per agent, sorted by total value, showing only agents whose total exceeds 10,000.

---

## 10. Null Handling

Null values require careful handling. Queryex is designed to be explicit and safe.

### Null Propagation in Arithmetic

Any arithmetic involving `null` produces `null`:

```
Value + null = null
null * 100  = null
```

Use `isnull` to provide a default:

```
isnull(Value, 0) + isnull(Discount, 0)
```

### Null-Safe Equality

Unlike SQL, `=` in Queryex is null-safe:

```
Agent = null        -- true if Agent has no value (equivalent to SQL IS NULL)
Code != null        -- true if Code has a value (equivalent to SQL IS NOT NULL)
null = null         -- true (unlike SQL where NULL = NULL is unknown)
```

### Null in Comparisons

For relational operators (`<`, `>`, `<=`, `>=`) and string operators (`contains`, `startsw`, `endsw`), if either operand is `null`, the result is `false` — never `null`.

### Null and Logical Operators

Boolean expressions passed to `and`, `or`, and `not` must not be `null`. Navigation paths can introduce nullability; use `isnull` or null checks to guard them.

```
-- Guard a navigation property before using it
Agent != null and Agent.Name contains 'Smith'
```

---

## 11. Operator Precedence

When multiple operators appear without parentheses, higher-precedence operators bind first. From highest to lowest:

| Precedence | Operators |
|---|---|
| 1 (highest) | Function calls, literals, `()` grouping |
| 2 | `*`, `/`, `%` |
| 3 | `+`, `-` (binary and unary) |
| 4 | `=`, `!=`, `<>`, `<`, `>`, `<=`, `>=`, `eq`, `ne`, `lt`, `le`, `gt`, `ge`, `contains`, `startsw`, `endsw`, `descof` |
| 5 | `!`, `not` |
| 6 | `&&`, `and` |
| 7 (lowest) | `\|\|`, `or` |

### Practical Consequences

```
A + B * C           -- means A + (B * C)
A = B and C = D     -- means (A = B) and (C = D)
A or B and C        -- means A or (B and C)
not A and B         -- means (not A) and B
```

**When in doubt, use parentheses:**

```
(A or B) and C      -- explicit grouping
```

---

## 12. Query Parameter Reference

### `filter`

Accepts a single boolean expression. Rows where the expression is `false` or `null` are excluded.

```
filter=Value > 1000
filter=IsActive = true and Code startsw 'ACC'
filter=(Type = 'Purchase' or Type = 'Sale') and PostingDate >= today()
```

### `orderby`

A comma-separated list of expressions, each optionally followed by `asc` or `desc`. The default direction when omitted is ascending.

```
orderby=Name
orderby=Name asc
orderby=Value desc, Name asc
orderby=(Value * Direction) desc
```

### `select`

A comma-separated list of expressions whose values are returned. Cannot contain aggregation functions.

```
select=Id, Name, Code
select=Id, Name, Value * Direction
select=Id, if(IsActive, 'Active', 'Inactive'), Agent.Name
```

### `expand`

A comma-separated list of navigation property **paths** (not expressions). Used to load related entities eagerly.

```
expand=Agent
expand=Account, Resource
expand=Agent, Account.Center
```

No operators or functions are allowed here — only dot-separated property paths.

### `aggregateselect`

Like `select`, but aggregation functions are allowed (and usually expected). Non-aggregated expressions become the GROUP BY key.

```
aggregateselect=Agent.Name, sum(Value), count(Id)
aggregateselect=year(PostingDate), month(PostingDate), sum(Value)
```

### `aggregateorderby`

Like `orderby` but for aggregated results. Supports `asc`/`desc`.

```
aggregateorderby=sum(Value) desc
aggregateorderby=Agent.Name asc, sum(Value) desc
```

### `having`

A boolean expression over aggregations, applied after grouping. Only allowed after `aggregateselect`.

```
having=sum(Value) > 10000
having=count(Id) >= 5 and avg(Value) < 500
```

---

## 13. Common Patterns and Examples

### Date Range Filter

```
PostingDate >= '2024-01-01' and PostingDate < '2025-01-01'
```

### This Month's Records

```
year(PostingDate) = year(today()) and month(PostingDate) = month(today())
```

Or using date boundaries:

```
PostingDate >= startofmonth(today()) and PostingDate < addmonths(1, startofmonth(today()))
```

### Records Created by the Current User

```
CreatedById = me()
```

### Active Records Only

```
filter=IsActive = true
```

Or equivalently (since `IsActive` is already boolean-typed):

```
filter=IsActive = true
```

### Text Search

```
Name contains 'smith' or Code contains 'smith'
```

### Signed Amount (Debit/Credit Direction)

Many financial documents store `Value` as a positive number and `Direction` as `+1` or `-1`. To get the signed amount:

```
select=Value * Direction
filter=(Value * Direction) > 0
orderby=(Value * Direction) desc
```

### Age in Days

```
select=diffdays(CreatedAt, today())
filter=diffdays(CreatedAt, today()) > 30
```

### Hierarchical Filter (Descendants)

The `descof` operator filters to rows whose hierarchy-typed column is a descendant of a given node. The right-hand side is a path string:

```
Concept descof '1'
Center.Node descof '1-2'
```

### Combine Null Guard with Navigation

```
Agent != null and Agent.Lookup1.Code = 'CATEGORY-A'
```

### Conditional Label in Select

```
select=Id, Name, if(Value * Direction > 0, 'Credit', 'Debit')
```

### Aggregation: Revenue by Agent This Year

```
aggregateselect=Agent.Name, sum(Value, Direction = 1)
&having=sum(Value, Direction = 1) > 0
&aggregateorderby=sum(Value, Direction = 1) desc
&filter=year(PostingDate) = year(today())
```

### Aggregation: Monthly Summary

```
aggregateselect=year(PostingDate), month(PostingDate), sum(Value), count(Id)
&aggregateorderby=year(PostingDate) asc, month(PostingDate) asc
```

---

## 14. Error Reference

When an expression is invalid, the API returns an error message. Here are the common ones and their causes:

| Error Message | Cause | Fix |
|---|---|---|
| `Unrecognized token` | A character or word is not valid syntax | Check spelling and allowed characters |
| `Mismatched brackets` | Parentheses are unbalanced | Count opening and closing `()` |
| `Unknown function 'xyz'` | Function name does not exist | Check the function name spelling |
| `No overload accepts N arguments` | Wrong number of arguments passed to a function | Check the function signature |
| `Cannot be a Boolean expression` | A boolean value was used where a non-boolean is expected (e.g., in arithmetic) | Remove the boolean operand from arithmetic |
| `Could not be interpreted as Numeric` | A non-numeric expression was used in arithmetic | Ensure both sides of `+`, `-`, `*`, `/` are numeric |
| `Cannot contain aggregation` | An aggregation function (`sum`, `count`, etc.) appears in `filter` or `select` | Move it to `aggregateselect` or `having` |
| `Expected paths only` | A non-path expression was used in `expand` | Use only dot-separated property paths in `expand` |
| `Left operand of 'descof' must be a column access` | `descof` was used on a computed expression | Use a direct property path on the left |
| `Type mismatch` | Two operands or branches have incompatible types | Ensure both sides of comparisons or `if` branches have matching types |

---

## Quick Reference Card

```
-- Literals
'string'    42    3.14    true    false    null    today()    now()    me()

-- Comparisons
=  !=  <  >  <=  >=     (also: eq ne lt gt le ge)

-- Logic
and &&     or ||     not !

-- Arithmetic
+  -  *  /  %

-- String tests
Name contains 'x'    Code startsw 'A'    Code endsw 'Z'

-- Navigation
Agent.Name    Account.Center.Code    Resource.Lookup1.Name

-- Functions (selection)
if(cond, a, b)    isnull(val, default)    abs(n)

-- Functions (strings)
upper(s)  lower(s)  trim(s)  len(s)  left(s,n)  right(s,n)  mid(s,i,n)  replace(s,old,new)

-- Functions (dates)
year(d)  month(d)  day(d)  quarter(d)  weekday(d)  hour(dt)  minute(dt)  second(dt)
date(dt)    startofmonth(d)    startofyear(d)
adddays(n,d)  addmonths(n,d)  addyears(n,d)
diffdays(d1,d2)  diffhours(dt1,dt2)  diffminutes(dt1,dt2)  diffseconds(dt1,dt2)

-- Aggregations (aggregateselect / having only)
sum(v)    count(v)    avg(v)    min(v)    max(v)
sum(v, condition)   -- conditional aggregation

-- Sort direction (orderby / aggregateorderby only)
Name asc    Value desc
```
