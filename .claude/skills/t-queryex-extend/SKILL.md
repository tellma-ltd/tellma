---
name: t-queryex-extend
description: Step-by-step guide for adding new functions or operators to the Queryex expression language. Use this when asked to add a new Queryex function, extend the expression evaluator, or modify what expressions compile to in SQL.
user-invocable: true
allowed-tools: Read, Grep, Edit, Write, Bash
---

# Extending the Queryex Language

Queryex is Tellma's custom expression language used in report filters, measures, and computed columns. It compiles to SQL on the backend and is also evaluated client-side in TypeScript. Every change requires updates in **exactly two files**, kept in sync.

## The Two Files

| File | Role |
|------|------|
| `Tellma.Repository.Common/Queryex/QueryexFunction.cs` | C# — compiles expressions to SQL |
| `Tellma.Api.Web/ClientApp/src/app/data/queryex-util.ts` | TypeScript — type-checks and evaluates expressions client-side |

---

## Key Concepts

### QxType (C#)
Ordered enum of expression types. Higher precedence = smaller numeric value.
```
Boolean=1, HierarchyId=2, Geography=4, DateTimeOffset=8,
DateTime=16, Date=32, Numeric=64, Bit=128, String=256, Null=512
```
An expression with a higher-precedence type **cannot** be coerced into a lower-precedence type.

### QxNullity (C#)
Tracks whether an expression can be null. Use bitwise OR (`|`) to combine:
```
NotNull=1, Nullable=3, Null=7
```
- If any arg is `Null`, the whole expression is `Null`.
- If any arg is `Nullable`, the whole expression is `Nullable`.
- Use `resultNullity = arg1Nullity | arg2Nullity` for functions that null-propagate.

### DeBracket()
A C# extension method that strips wrapping parentheses from an already-compiled SQL fragment. Always call it on compiled argument SQL before embedding in a larger SQL string.

### TryCompile vs CompileNative (C#)
- `TryCompile(targetType, ctx, out sql, out nullity)` — tries to compile to a *specific* type; returns `false` if incompatible. Used by the type inference system.
- `CompileNative(ctx)` — compiles to the expression's *own* natural type. Most new functions belong here.
- Functions with a **fixed return type** only need a case in `CompileNative`. The existing `default: return base.TryCompile(...)` in `TryCompile` covers them automatically.
- Functions whose return type **depends on argument types** (like `IF`, `ISNULL`, `MIN`, `MAX`, `ADDDAYS`) need cases in **both** methods.

### DataType (TypeScript)
String union: `'boolean' | 'numeric' | 'string' | 'date' | 'datetime' | 'datetimeoffset' | 'bit' | 'entity'`

### tryDescImpl / nativeDescImpl (TypeScript)
- `nativeDescImpl(ex)` — returns the expression's natural `PropDescriptor` (includes `datatype`, `control`, `label`, etc.)
- `tryDescImpl(ex, target)` — tries to interpret `ex` as the given `DataType` or `PropDescriptor`; returns `null` on failure.
- `noLabel(trx)` — a label function returning the generic "Expression" label. Use for computed results that have no inherent name.

---

## Step 1 — Add the SQL compiler case in `QueryexFunction.cs`

Open `CompileNative()` (around line 154) and add a new `case` block **before** the `default:` at the end of the switch.

Follow the existing `abs` case as a pattern. Set `resultType`, `resultNullity`, and `resultSql`, then `break`.

### Template: fixed-type, single string arg → string result
```csharp
case "myfunc": // (str: string) => string
    {
        int expectedArgCount = 1;
        if (Arguments.Length != expectedArgCount)
            throw new QueryException($"Function '{Name}' accepts exactly {expectedArgCount} argument(s).");

        var arg1 = Arguments[0];
        if (!arg1.TryCompile(QxType.String, ctx, out string strSql, out QxNullity strNullity))
            throw new QueryException($"Function '{Name}': The first argument {arg1} could not be interpreted as a {QxType.String}.");

        resultType = QxType.String;
        resultNullity = strNullity;
        resultSql = $"MY_SQL_FUNC({strSql.DeBracket()})";
        break;
    }
```

### Template: fixed-type, string + numeric args → string result
```csharp
case "myfunc": // (str: string, n: numeric) => string
    {
        int expectedArgCount = 2;
        if (Arguments.Length != expectedArgCount)
            throw new QueryException($"Function '{Name}' accepts exactly {expectedArgCount} argument(s).");

        var arg1 = Arguments[0];
        if (!arg1.TryCompile(QxType.String, ctx, out string strSql, out QxNullity strNullity))
            throw new QueryException($"Function '{Name}': The first argument {arg1} could not be interpreted as a {QxType.String}.");

        var arg2 = Arguments[1];
        if (!arg2.TryCompile(QxType.Numeric, ctx, out string nSql, out QxNullity nNullity))
            throw new QueryException($"Function '{Name}': The second argument {arg2} could not be interpreted as a {QxType.Numeric}.");
        if (nNullity != QxNullity.NotNull)
            throw new QueryException($"Function '{Name}': The second argument {arg2} cannot be a nullable expression.");

        resultType = QxType.String;
        resultNullity = strNullity;
        resultSql = $"MY_SQL_FUNC({strSql.DeBracket()}, {nSql.DeBracket()})";
        break;
    }
```

### Template: optional argument (2 or 3 args)
```csharp
case "myfunc": // (str: string, start: numeric [, len: numeric]) => string
    {
        if (Arguments.Length < 2 || Arguments.Length > 3)
            throw new QueryException($"No overload for function '{Name}' accepts {Arguments.Length} arguments.");

        // ... compile required args ...

        string optionalSql;
        if (Arguments.Length == 3)
        {
            var arg3 = Arguments[2];
            if (!arg3.TryCompile(QxType.Numeric, ctx, out optionalSql, out QxNullity lenNullity))
                throw new QueryException($"Function '{Name}': The third argument {arg3} could not be interpreted as a {QxType.Numeric}.");
            if (lenNullity != QxNullity.NotNull)
                throw new QueryException($"Function '{Name}': The third argument {arg3} cannot be a nullable expression.");
        }
        else
        {
            optionalSql = "/* default fallback SQL */";
        }

        resultType = QxType.String;
        resultNullity = strNullity;
        resultSql = $"MY_SQL_FUNC({strSql.DeBracket()}, {startSql.DeBracket()}, {optionalSql})";
        break;
    }
```

### Template: null-propagating multi-arg (e.g. REPLACE)
```csharp
// resultNullity = union of all arg nullities
resultNullity = strNullity | oldNullity | newNullity;
```

### Template: date/time function
Date functions typically try multiple types in order:
```csharp
if (arg1.TryCompile(QxType.Date, ctx, out string dateSql, out QxNullity dateNullity) ||
    arg1.TryCompile(QxType.DateTime, ctx, out dateSql, out dateNullity) ||
    arg1.TryCompile(QxType.DateTimeOffset, ctx, out dateSql, out dateNullity))
{
    resultType = QxType.Numeric;
    resultNullity = dateNullity;
    resultSql = $"DATEPART(MY_PART, {dateSql.DeBracket()})";
    break;
}
else
{
    throw new QueryException($"...");
}
```

### Template: function whose return type varies with args (needs BOTH TryCompile AND CompileNative)
Add a case in `TryCompile` that attempts compilation to the requested `targetType` and returns true/false:
```csharp
// In TryCompile():
case "myfunc":
    {
        if (targetType == QxType.Date || targetType == QxType.Numeric)
        {
            // try to compile to targetType
            if (arg.TryCompile(targetType, ctx, out string argSql, out QxNullity argNullity))
            {
                resultSql = $"...{argSql}...";
                resultNullity = argNullity;
                return true;
            }
        }
        resultSql = null;
        resultNullity = default;
        return false;
    }
```

---

## Step 2 — Add the type descriptor case in `queryex-util.ts` (`nativeDescImpl`)

Find `nativeDescImpl` (search for `function nativeDescImpl`) and add a case **before** the `default:` throw inside its switch on `ex.name.toLowerCase()`.

### Template: string → numeric descriptor
```typescript
case 'myfunc': {
    const expectedArgCount = 1;
    if (ex.arguments.length !== expectedArgCount)
        throw new Error(`Function '${ex.name}' accepts exactly ${expectedArgCount} argument(s).`);

    if (!tryDescImpl(ex.arguments[0], 'string'))
        throw new Error(`Function '${ex.name}': The first argument ${ex.arguments[0]} could not be interpreted as a string.`);

    return {
        datatype: 'numeric',
        control: 'number',
        minDecimalPlaces: 0,
        maxDecimalPlaces: 0,
        isRightAligned: false,
        noSeparator: true,
        label: noLabel(trx)
    };
}
```

### Template: string → string descriptor
```typescript
case 'myfunc': {
    const expectedArgCount = 1;
    if (ex.arguments.length !== expectedArgCount)
        throw new Error(`Function '${ex.name}' accepts exactly ${expectedArgCount} argument(s).`);

    if (!tryDescImpl(ex.arguments[0], 'string'))
        throw new Error(`Function '${ex.name}': The first argument ${ex.arguments[0]} could not be interpreted as a string.`);

    return { datatype: 'string', control: 'text', label: noLabel(trx) };
}
```

### Template: optional 3rd argument
```typescript
case 'myfunc': {
    if (ex.arguments.length < 2 || ex.arguments.length > 3)
        throw new Error(`No overload for function '${ex.name}' accepts ${ex.arguments.length} arguments.`);

    if (!tryDescImpl(ex.arguments[0], 'string'))
        throw new Error(`...`);
    if (!tryDescImpl(ex.arguments[1], 'numeric'))
        throw new Error(`...`);
    if (ex.arguments.length === 3 && !tryDescImpl(ex.arguments[2], 'numeric'))
        throw new Error(`...`);

    return { datatype: 'string', control: 'text', label: noLabel(trx) };
}
```

### Common descriptor shapes

| Result type | Descriptor |
|-------------|------------|
| string | `{ datatype: 'string', control: 'text', label: noLabel(trx) }` |
| integer | `{ datatype: 'numeric', control: 'number', minDecimalPlaces: 0, maxDecimalPlaces: 0, isRightAligned: false, noSeparator: true, label: noLabel(trx) }` |
| decimal | `{ datatype: 'numeric', control: 'number', minDecimalPlaces: 0, maxDecimalPlaces: 4, isRightAligned: true, label: noLabel(trx) }` |
| date | `{ datatype: 'date', control: 'date', granularity: DateGranularity.days, label: noLabel(trx) }` |
| boolean | `{ datatype: 'boolean', control: 'check', label: noLabel(trx) }` |

---

## Step 3 — Add the client-side evaluator case in `queryex-util.ts` (`evaluateExp`)

Find the inner `evaluate` function inside `evaluateExp` (search for `return wss.currentTenant.userSettings.UserId` to find the `me` case nearby) and add cases **before** the `default:` throw.

### Template: null-propagating unary string function
```typescript
case 'myfunc': {
    const str = evaluate(ex.arguments[0]) as string;
    return str === null ? null : str.myJsMethod();
}
```

### Template: null-propagating binary
```typescript
case 'myfunc': {
    const str = evaluate(ex.arguments[0]) as string;
    const n = evaluate(ex.arguments[1]) as number;
    return str === null ? null : /* JS expression using str and n */;
}
```

### Template: optional argument
```typescript
case 'myfunc': {
    const str = evaluate(ex.arguments[0]) as string;
    if (str === null) { return null; }
    const start = evaluate(ex.arguments[1]) as number; // note: may be 1-indexed
    if (ex.arguments.length === 3) {
        const len = evaluate(ex.arguments[2]) as number;
        return str.slice(start - 1, start - 1 + len);
    } else {
        return str.slice(start - 1);
    }
}
```

### Template: all-args null-propagating
```typescript
case 'myfunc': {
    const a = evaluate(ex.arguments[0]) as string;
    const b = evaluate(ex.arguments[1]) as string;
    const c = evaluate(ex.arguments[2]) as string;
    return a === null || b === null || c === null ? null : /* expression */;
}
```

### JS equivalents for common SQL functions
| SQL | JS |
|-----|----|
| `LEN(s)` | `s.length` |
| `LEFT(s, n)` | `s.slice(0, n)` |
| `RIGHT(s, n)` | `n === 0 ? '' : s.slice(-n)` |
| `SUBSTRING(s, i, n)` (1-indexed) | `s.slice(i - 1, i - 1 + n)` |
| `TRIM(s)` | `s.trim()` |
| `UPPER(s)` | `s.toUpperCase()` |
| `LOWER(s)` | `s.toLowerCase()` |
| `REPLACE(s, old, new)` (all occurrences) | `s.split(old).join(new)` |
| `ABS(n)` | `Math.abs(n)` |
| `ROUND(n, d)` | Custom rounding logic |

---

## Step 4 — Verify

```bash
# Verify C# compiles (SQL project error about SSDT is pre-existing, ignore it)
dotnet build Tellma.Repository.Common/Tellma.Repository.Common.csproj

# Verify TypeScript compiles (protractor and @types/ws errors are pre-existing, ignore them)
cd Tellma.Api.Web/ClientApp && npx tsc --noEmit
```

---

## Checklist

- [ ] Case added in `CompileNative()` in `QueryexFunction.cs`
- [ ] If return type varies with args: case also added in `TryCompile()` in `QueryexFunction.cs`
- [ ] Case added in `nativeDescImpl` switch in `queryex-util.ts`
- [ ] Case added in the `evaluate` function inside `evaluateExp` in `queryex-util.ts`
- [ ] C# build passes (ignoring pre-existing SQL project error)
- [ ] TypeScript type-check passes (ignoring pre-existing protractor / `@types/ws` errors)
- [ ] Error messages match the pattern: `Function 'name': The Nth argument {arg} could not be interpreted as a {Type}.`
- [ ] Nullity is handled correctly: propagate from args that can be null; require `NotNull` for index/length args

---

## Examples Already Implemented

For concrete reference, see the string functions added in `QueryexFunction.cs` and `queryex-util.ts`:
- `LEN` — simplest pattern: unary, fixed string→numeric
- `LEFT` / `RIGHT` — shared case, two args, require non-null second arg
- `MID` — optional third argument, 1-indexed, maps to `SUBSTRING`
- `TRIM` / `UPPER` / `LOWER` — shared case, unary string→string
- `REPLACE` — three string args, nullity is union of all three

For numeric functions, see `ABS` (line ~490 in `QueryexFunction.cs`).
For date functions, see `YEAR` / `MONTH` / `DAY` and `ADDDAYS` / `ADDMONTHS` / `ADDYEARS`.
