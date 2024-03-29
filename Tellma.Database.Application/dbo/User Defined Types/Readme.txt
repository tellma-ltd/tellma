Protocol for adding columns in tables:
Resources
Agents
Documents
Lines
Entries

1) Start with the minimum columns that are needed
2) If the new column is used in more than 20% of the potential B# customers, hard code it (especially if used in business logic or to build ready reports)
3) If it is used in less than 20% but need to be indexed, displayed in detailed reports, or used for ordering => dynamic
4) Otherwise, add it to THE Json column in the table. [JsonFields] It will only show in the details grid of the entity.

For properties that are 1-many (such as medical history)
1) If used by more than 20%, or in business logic, we define a special table
2) Otherwise, we store in more sophisticated Json column (definition should accommodate multiple rows)

Hard coded
----------
ResourceDefinitionId: currencies
AccountTypeCode: Cash
DocumentDefinitionId : journal-vouchers
LineDefinitionId : manual-lines

AgentDefinitionCode: TaxDepartment
AgentCode: VAT, EIT, ...

Hard Rules
----------
Functional Currency (Resource and Currency) must stay active.