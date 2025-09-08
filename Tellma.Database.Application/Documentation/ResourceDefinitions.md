# ResourceDefinitions Table Documentation

## Overview
The ResourceDefinitions table defines various types of resources in the Tellma system. Resources are tangible or intangible items that can be tracked and managed within the system.

## Resource Types
The ResourceDefinitions table supports various types of resources. Here are some examples with their typical configurations:

### Property, Plant and Equipment
- **Office Equipment**
- **Machinery**
- **Investment Property**
- **Intangible Assets Other Than Goodwill**
- **Biological Assets**

### Inventory Items
- **Merchandise**
- **Production Supplies**
- **Raw Materials**
- **Finished Goods**

### Other Categories
- **Other**
- **Non Current Non Financial Assets**
- **Miscellaneous (e.g., Tasks)**
- **Employee Leave type**
- **Employee Loan type**

## Example Configurations

### Office Equipment
```sql
ResourceDefinitionType = 'PropertyPlantAndEquipment'
CurrencyVisibility = 'Required'
LocationVisibility = 'Required'
ImageVisibility = 'Optional'
DescriptionVisibility = 'Optional'
```

### Machinery
```sql
ResourceDefinitionType = 'PropertyPlantAndEquipment'
CurrencyVisibility = 'Required'
UnitCardinality = 'Single'
ReorderLevelVisibility = 'Required'
EconomicOrderQuantityVisibility = 'Required'
```

### Investment Property
```sql
ResourceDefinitionType = 'InvestmentProperty'
CurrencyVisibility = 'Required'
LocationVisibility = 'Required'
FromDateVisibility = 'Required'
ToDateVisibility = 'Required'
```

### Employee Leave Type
```sql
ResourceDefinitionType = 'Miscellaneous'
DescriptionVisibility = 'Required'
```

### Employee Loan Type
```sql
ResourceDefinitionType = 'Miscellaneous'
CurrencyVisibility = 'Required'
```

## Best Practices

1. **Field Usage**
   - Always set CurrencyVisibility to Required for financial resources
   - Use appropriate UnitCardinality for inventory items
   - Set ReorderLevel and EconomicOrderQuantity for inventory management
   - Use LocationVisibility for physical assets

2. **Resource Type Selection**
   - Use PropertyPlantAndEquipment for fixed assets
   - Use InvestmentProperty for investment properties
   - Use IntangibleAssetsOtherThanGoodwill for non-physical assets
   - Use Miscellaneous for non-financial resources

3. **Inventory Management**
   - Set appropriate UnitCardinality
   - Configure ReorderLevel and EconomicOrderQuantity
   - Use UnitMassVisibility for raw materials
   - Set proper UnitMassUnitId for mass tracking

4. **Employee Resources**
   - Use proper descriptions for leave types
   - Set currency for loan types
   - Maintain employee-related records
   - Track employee-specific resources

## Table Structure

### Basic Information
```sql
CREATE TABLE [dbo].[ResourceDefinitions] (
    [Id] INT CONSTRAINT [PK_ResourceDefinitions] PRIMARY KEY IDENTITY,
    [Code] NVARCHAR(255) NOT NULL CONSTRAINT [UQ_ResourceDefinitions] UNIQUE,
    [TitleSingular] NVARCHAR(100),
    [TitleSingular2] NVARCHAR(100),
    [TitleSingular3] NVARCHAR(100),
    [TitlePlural] NVARCHAR(100),
    [TitlePlural2] NVARCHAR(100),
    [TitlePlural3] NVARCHAR(100),
    [ResourceDefinitionType] NVARCHAR(255) NOT NULL
)
```

## Table Structure

### Basic Information
```sql
CREATE TABLE [dbo].[ResourceDefinitions] (
    [Id] INT CONSTRAINT [PK_ResourceDefinitions] PRIMARY KEY IDENTITY,
    [Code] NVARCHAR(255) NOT NULL CONSTRAINT [UQ_ResourceDefinitions] UNIQUE,
    [TitleSingular] NVARCHAR(100),
    [TitleSingular2] NVARCHAR(100),
    [TitleSingular3] NVARCHAR(100),
    [TitlePlural] NVARCHAR(100),
    [TitlePlural2] NVARCHAR(100),
    [TitlePlural3] NVARCHAR(100),
    [ResourceDefinitionType] NVARCHAR(255) NOT NULL
)
```

### Visibility Settings
```sql
[CurrencyVisibility] NVARCHAR(50) NOT NULL DEFAULT N'Required',
[CenterVisibility] NVARCHAR(50) NOT NULL DEFAULT N'None',
[ImageVisibility] NVARCHAR(50) NOT NULL DEFAULT N'None',
[DescriptionVisibility] NVARCHAR(50) NOT NULL DEFAULT N'None',
[LocationVisibility] NVARCHAR(50) NOT NULL DEFAULT N'None',
[FromDateVisibility] NVARCHAR(50) NOT NULL DEFAULT N'None',
[ToDateVisibility] NVARCHAR(50) NOT NULL DEFAULT N'None',
```

### Custom Fields
```sql
-- Date Fields
[Date1Visibility] NVARCHAR(50) NULL DEFAULT N'None',
[Date2Visibility] NVARCHAR(50) NULL DEFAULT N'None',
[Date3Visibility] NVARCHAR(50) NULL DEFAULT N'None',
[Date4Visibility] NVARCHAR(50) NULL DEFAULT N'None',

-- Decimal Fields
[Decimal1Visibility] NVARCHAR(50) NOT NULL DEFAULT N'None',
[Decimal2Visibility] NVARCHAR(50) NOT NULL DEFAULT N'None',
[Decimal3Visibility] NVARCHAR(50) NOT NULL DEFAULT N'None',
[Decimal4Visibility] NVARCHAR(50) NOT NULL DEFAULT N'None',

-- Integer Fields
[Int1Visibility] NVARCHAR(50) NOT NULL DEFAULT N'None',
[Int2Visibility] NVARCHAR(50) NOT NULL DEFAULT N'None',

-- Lookup Fields
[Lookup1Visibility] NVARCHAR(50) NOT NULL DEFAULT N'None',
[Lookup2Visibility] NVARCHAR(50) NOT NULL DEFAULT N'None',
[Lookup3Visibility] NVARCHAR(50) NOT NULL DEFAULT N'None',
[Lookup4Visibility] NVARCHAR(50) NOT NULL DEFAULT N'None',

-- Text Fields
[Text1Visibility] NVARCHAR(50) NOT NULL DEFAULT N'None',
[Text2Visibility] NVARCHAR(50) NOT NULL DEFAULT N'None'
```

### Resource-Specific Fields
```sql
-- Resource Properties
[IdentifierVisibility] NVARCHAR(50) NOT NULL DEFAULT N'None',
[VatRateVisibility] NVARCHAR(50) DEFAULT N'None',
[DefaultVatRate] DECIMAL(9,4) CONSTRAINT [ResourceDefinitions__DefaultVatRate] CHECK ([DefaultVatRate] BETWEEN 0 AND 1),

-- Inventory Properties
[ReorderLevelVisibility] NVARCHAR(50) NOT NULL DEFAULT N'None',
[EconomicOrderQuantityVisibility] NVARCHAR(50) NOT NULL DEFAULT N'None',
[UnitCardinality] NVARCHAR(50) NOT NULL DEFAULT N'Single',
[DefaultUnitId] INT,
[UnitMassVisibility] NVARCHAR(50) DEFAULT N'None',
[DefaultUnitMassUnitId] INT,

-- Financial Properties
[MonetaryValueVisibility] NVARCHAR(50) NOT NULL DEFAULT N'None',
```

### Relationships
```sql
-- Agent Relationships
[Agent1Visibility] NVARCHAR(50) NOT NULL DEFAULT N'None',
[Agent1DefinitionId] INT,
[Agent2Visibility] NVARCHAR(50) NOT NULL DEFAULT N'None',
[Agent2DefinitionId] INT,

-- Resource Relationships
[Resource1Visibility] NVARCHAR(50) NOT NULL DEFAULT N'None',
[Resource1DefinitionId] INT,
[Resource2Visibility] NVARCHAR(50) NOT NULL DEFAULT N'None',
[Resource2DefinitionId] INT,
```

### Scripts and Attachments
```sql
[PreprocessScript] NVARCHAR(MAX),
[ValidateScript] NVARCHAR(MAX),
[HasAttachments] BIT NOT NULL DEFAULT 0,
[AttachmentsCategoryDefinitionId] INT,
```

## Field Usage Examples

### Property, Plant and Equipment
```sql
-- Required Fields:
CurrencyVisibility = 'Required'
UnitCardinality = 'Single'
ReorderLevelVisibility = 'Required'
EconomicOrderQuantityVisibility = 'Required'

-- Optional Fields:
ImageVisibility = 'Optional'
DescriptionVisibility = 'Optional'
```

### Investment Property
```sql
-- Required Fields:
CurrencyVisibility = 'Required'
LocationVisibility = 'Required'
FromDateVisibility = 'Required'
ToDateVisibility = 'Required'

-- Optional Fields:
ImageVisibility = 'Optional'
DescriptionVisibility = 'Optional'
```

### Inventory Item
```sql
-- Required Fields:
CurrencyVisibility = 'Required'
UnitCardinality = 'Multiple'
DefaultUnitId = [UnitId]
UnitMassVisibility = 'Required'
ReorderLevelVisibility = 'Required'
EconomicOrderQuantityVisibility = 'Required'

-- Optional Fields:
ImageVisibility = 'Optional'
DescriptionVisibility = 'Optional'
```

## Special Notes

### Visibility States
- None: Field hidden from UI
- Optional: Field visible but not required
- Required: Field visible and must be filled

### Resource Relationships
1. **Agent Relationships**
   - Agent1: Primary relationship (e.g., owner)
   - Agent2: Secondary relationship (e.g., manager)

2. **Resource Relationships**
   - Resource1: Primary grouping
   - Resource2: Secondary grouping

### Unit Management
- UnitCardinality: Controls unit handling
  - None: No units
  - Single: Single unit
  - Multiple: Multiple units
- DefaultUnitId: Base unit for measurements
- UnitMassVisibility: Controls mass unit visibility
- DefaultUnitMassUnitId: Base mass unit

### Inventory Management
- ReorderLevel: Minimum stock level
- EconomicOrderQuantity: Optimal order quantity
- UnitCardinality: Controls unit handling
- DefaultUnitId: Base unit for measurements

### Financial Properties
- Currency: Required for all resources
- VatRate: Optional, defaults to 0-1
- MonetaryValue: Controls monetary value visibility

## Best Practices

1. Field Usage
   - Use Currency for all resources
   - Use VatRate for taxable resources
   - Use UnitCardinality for inventory items
   - Use ReorderLevel and EconomicOrderQuantity for inventory management

2. Relationships
   - Use Agent1 for primary relationships
   - Use Agent2 for secondary relationships
   - Use Resource1 for primary grouping
   - Use Resource2 for secondary grouping

3. Visibility Settings
   - Use Required for mandatory fields
   - Use Optional for supplementary information
   - Use None for non-applicable fields

4. Unit Management
   - Set appropriate UnitCardinality
   - Define DefaultUnitId for inventory items
   - Configure UnitMassVisibility for mass tracking

5. Inventory Management
   - Set appropriate ReorderLevel
   - Calculate EconomicOrderQuantity
   - Use UnitCardinality for multiple units

6. Scripts
   - Use PreprocessScript for automation
   - Use ValidateScript for business rules
   - Document script logic for maintainability

## System Features

1. **Multi-language Support**
   - All title fields support multiple languages
   - Labels for custom fields support multiple languages
   - Consider regional requirements

2. **System Versioning**
   - All changes are tracked
   - History available in ResourceDefinitionsHistory
   - Maintain versioning for audit purposes

3. **State Management**
   - Hidden: Not visible in UI
   - Visible: Normal state
   - Archived: Historical records
   - Testing: Development/test state

4. **MainMenu Fields**
   - Control menu display
   - Icon: Visual representation
   - Section: Menu organization
   - SortKey: Menu ordering
