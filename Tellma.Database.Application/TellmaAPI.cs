using System.Collections.Generic;
using System;
using Tellma.Api.Dto;
using Tellma.Client;
using Tellma.Model.Application;

// Parameters
int tenantId = 401;
int documentDefinitionId = 15;

// Create the Document
DocumentForSave document = new DocumentForSave
{
    // Document Properties
    PostingDate = DateTime.Today,
    PostingDateIsCommon = true,
    NotedAgentId = 1257,
    AgentIsCommon = true,
    CenterId = 8,

    // Lines
    Lines = new List<LineForSave>
    {
        // First Line
        new LineForSave
        {
            // Line Properties
            DefinitionId = 31,
            Text1 = "8:00 AM",
            Text2 = "3:00 PM",

            // Entries
            Entries = new List<EntryForSave>
            {
                // Entry Index = 0
                new EntryForSave
                {
                    AgentId = 1638,
                }
            }
        },
        // First Line
        new LineForSave
        {
            // Line Properties
            DefinitionId = 31,
            Text1 = "9:00 AM",
            Text2 = "2:30 PM",

            // Entries
            Entries = new List<EntryForSave>
            {
                // Entry Index = 0
                new EntryForSave
                {
                    AgentId = 1276,
                }
            }
        },

    }
};

// Create the client (See Pre-Requisites)
TellmaClient client = ...;

// Save the Document
await client
    .Application(tenantId)
    .Documents(documentDefinitionId)
    .Save(document);