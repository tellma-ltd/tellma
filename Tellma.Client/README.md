# Tellma.Client

This is a .NET Standard library to simplify interaction with the Tellma web API.

## Pre-Requisites

To use this library you must obtain a client Id and a client secret (the client credentials) from your Tellma instance administrator. Keep those credentials safe as they allow your code to access the Tellma API as an authenticated user.

You can use the client credentials like this:

```cs
using Tellma.Client;

// Create the client
TellmaClient client = new TellmaClient(
    baseUrl: "https://web.tellma.com",
    authorityUrl: "https://web.tellma.com",
    clientId: "<YOUR_CLIENT_ID>",
    clientSecret: "<YOUR_CLIENT_SECRET>");
```

Note: `baseUrl` and `authorityUrl` are always https://web.tellma.com when integrating with the Tellma SAAS instance.

## Example Usage

Get the first 10 Agents of definition Id = 456 where name contains "John", ordered by `Code` in descending order:
```cs
using Tellma.Api.Dto;
using Tellma.Client;
using Tellma.Model.Application;

// Parameters
int tenantId = 123;
int agentDefinitionId = 456;

// Create the client
TellmaClient client = ...;

// Get Agents
EntitiesResult<Agent> result = await client
    .Application(tenantId)
    .Agents(agentDefinitionId)
    .GetEntities(new GetArguments
    {
        Top = 10,
        Filter = "Name contains 'John'",
        OrderBy = "Code desc"  
    });

IReadOnlyList<Agent> agents = result.Data;
```

Get the Resource of definition Id = 456, which has an Id = 789:

```cs
using Tellma.Api.Dto;
using Tellma.Client;
using Tellma.Model.Application;

// Parameters
int tenantId = 123;
int resourceDefinitionId = 456;
int resourceId = 789;

// Create the client
TellmaClient client = ...

// Get the Resource
EntityResult<Resource> result = await client
    .Application(tenantId)
    .Resources(resourceDefinitionId)
    .GetById(resourceId);

Resource resource = result.Entity;
```

Create a new Document of definition Id = 456, set some document header properties and add a single Line of definition Id = 789:
```cs
using Tellma.Api.Dto;
using Tellma.Client;
using Tellma.Model.Application;

// Parameters
int tenantId = 123;
int documentDefinitionId = 456;

// Create the Document
DocumentForSave document = new DocumentForSave
{
    // Document Properties
    PostingDate = DateTime.Today,
    PostingDateIsCommon = true,
    AgentId = 2,
    AgentIsCommon = true,
    Memo = "My Memo",
    MemoIsCommon = true,

    // Lines
    Lines = new List<LineForSave>
    {
        // First Line
        new LineForSave
        {
            // Line Properties
            DefinitionId = 789,
            Decimal1 = 1000.0m,

            // Entries
            Entries = new List<EntryForSave>
            {
                // Entry Index = 0
                new EntryForSave
                {
                    AgentId = 322,
                    NotedDate = DateTime.Today,
                    ExternalReference = "MYEXREF01"
                },

                // Entry Index = 1
                new EntryForSave
                {
                    MonetaryValue = 150.0m
                }
            }
        },
    }
};

// Create the client
TellmaClient client = ...;

// Save the Document
await client
    .Application(tenantId)
    .Documents(documentDefinitionId)
    .Save(document);
```

Edit the date of an existing Document of definition Id = 456, which has an Id = 789:
```cs
using Tellma.Api.Dto;
using Tellma.Client;
using Tellma.Model.Application;

// Parameters
int tenantId = 123;
int documentDefinitionId = 456;
int documentId = 789;

// Create the client
TellmaClient client = ...;

// Get the Document
DocumentForSave document = await client
    .Application(tenantId)
    .Documents(documentDefinitionId)
    .GetByIdForSave(documentId);

// Make your edits
document.PostingDate = new DateTime(2023, 1, 15);

// Save the Document
await client
    .Application(tenantId)
    .Documents(documentDefinitionId)
    .Save(document);
```

Delete a Lookup of definition Id = 456, which has an Id = 789:
```cs
using Tellma.Api.Dto;
using Tellma.Client;
using Tellma.Model.Application;

// Parameters
int tenantId = 123;
int lookupDefinitionId = 456;
int lookupId = 789;

// Create the client
TellmaClient client = ...;

// Delete the Lookup
await client
    .Application(tenantId)
    .Lookups(lookupDefinitionId)
    .DeleteById(lookupId);
```
