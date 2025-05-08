# Account Classifications Validation

## Overview
The `AccountClassifications_Validate__Activate` procedure validates account classifications before activation or deactivation. It specifically checks for active accounts with non-zero balances when deactivating classifications.

## Business Rules
- When deactivating a classification (@IsActive = 0):
  - Cannot deactivate if any accounts under the classification have non-zero balance
  - Validation only considers posted lines (State = 4)
  - Checks balance across all currencies
  - Returns up to @Top validation errors
- When activating (@IsActive = 1):
  - No validation is performed (assumes activation is always allowed)

## Parameters
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| @Ids | IndexedIdList | - | List of account classification IDs to validate |
| @IsActive | BIT | - | Flag indicating if the classification is being activated (1) or deactivated (0) |
| @Top | INT | 10 | Maximum number of validation errors to return |
| @IsError | BIT | - | Output parameter indicating if validation errors exist |

## Return Values
- Returns a ValidationErrorList containing:
  - Key: Index of the classification
  - ErrorName: Error message identifier (Error_TheAccountClassification0HasAccount1WithNonZeroBalance)
  - Argument0: Localized classification name
  - Argument1: Localized account name with non-zero balance

## Implementation Details
- Uses Common Table Expression (CTE) for efficient active accounts calculation
- Uses localization functions (fn_Localize) for displaying names in appropriate language
- Uses ValidationErrorList table type for consistent error handling
- Returns only posted lines (State = 4) for validation

## Usage Examples
```sql
-- Example 1: Validate before deactivating classifications
DECLARE @IsError BIT;
EXEC [bll].[AccountClassifications_Validate__Activate] 
    @Ids = <IndexedIdList>,
    @IsActive = 0,
    @Top = 10,
    @IsError = @IsError OUTPUT;

-- Example 2: Validate before activating classifications
DECLARE @IsError BIT;
EXEC [bll].[AccountClassifications_Validate__Activate] 
    @Ids = <IndexedIdList>,
    @IsActive = 1,
    @Top = 10,
    @IsError = @IsError OUTPUT;
```

## Error Handling
- Returns validation errors in ValidationErrorList format
- Sets @IsError to 1 if any validation errors exist
- Limits validation errors to @Top number of records
- Uses localized error messages for better user experience

## Notes
- Part of the account management validation system
- Uses standard error message format
- Integrates with localization system
- Optimized for performance with CTE usage
