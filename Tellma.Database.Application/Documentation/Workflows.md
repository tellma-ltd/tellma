# Workflows Table Documentation

## Overview
The `Workflows` table defines the workflow states and transitions for document lines in the system. It specifies which states are allowed for each line definition and how documents can progress through their lifecycle.

## Line Types
1. **Workflow Lines**
   - Lines whose definitions are associated with a workflow
   - Follow the workflow-defined states:
     - Draft (0)
     - Requested (1)
     - Approved (2)
     - Completed (3) or Posted (4)
     - Plus their negative equivalents

2. **Non-Workflow Lines**
   - Lines whose definitions are not associated with a workflow
   - Follow simplified state progression:
     - For Event/Regulatory lines:
       - Draft (0)
       - Posted (4) or Canceled (-4)
     - For other line types:
       - Draft (0)
       - Approved (2) or Rejected (-2)

## Document State Rules
- A document can be moved to state -1 (Canceled) if ALL its lines have negative states
- This applies to both workflow and non-workflow lines
- The document state reflects the collective state of all its lines

## Key Characteristics

1. **Document States**
   - 1: Closed
   - 0: Open
   - -1: Canceled

2. **Line States**
   - 0: Draft (Initial state)
   - 1: Requested
   - 2: Approved
   - 3: Completed (Events and Regulatory only)
   - 4: Posted (Events and Regulatory only)
   - -1: Void (Opposite of Requested)
   - -2: Rejected (Opposite of Approved)
   - -3: Failed (Opposite of Completed)
   - -4: Canceled (Opposite of Posted)

3. **State Progression**
   - Line States:
     - Draft (0) → Requested (1)
     - Requested (1) → Approved (2) or Posted (4)
     - Approved (2) → Completed (3) or Posted (4)
   - Document States:
     - Open (0) → Closed (1)
     - Open (0) → Canceled (-1)

4. **Workflow Rules**
   - For Event and Regulatory line types:
     - Final state MUST be Posted (4)
     - Can skip Approved (2) and go directly from Requested (1) to Posted (4)
   - For other line types:
     - Final state MUST be Approved (2)
     - Cannot reach Completed (3) or Posted (4) states
     - Must go through Approved (2) state before completion

## Purpose
The Workflows table defines the allowed workflow states for each line definition, including:

### Key Relationships
- Each workflow entry must have a LineDefinitionId
- Links to Users table through SavedById
- Supports system-versioning through ValidFrom/ValidTo columns

### Key Fields

1. **Core Fields**
```sql
[Id] INT,
[LineDefinitionId] INT,
[ToState] SMALLINT,
[SavedById] INT,
[ValidFrom] DATETIME2,
[ValidTo] DATETIME2
```
   - Id: Primary key
   - LineDefinitionId: References the line definition this workflow applies to
   - ToState: The target state that can be reached
   - SavedById: User who saved this workflow configuration

### Implementation Notes
1. **State Transitions**
   - Each line definition can have multiple workflow entries
   - Each entry defines a single allowed state transition
   - The combination of LineDefinitionId and ToState must be unique

2. **State Types**
   - Positive states (0-4): Forward progression states
   - Negative states (-1 to -4): Reverse/cancellation states
   - State 0 (Draft) is always the starting point

3. **Line Type Specifics**
   - Event and Regulatory line types:
     - Can reach states 0-4
     - Have full workflow progression
   - Other line types:
     - Limited to states 0-2
     - Cannot reach Completed/Posted states

4. **System Versioning**
   - Workflows are versioned using system-versioning
   - Changes to workflow states are tracked in WorkflowsHistory
   - Each change is timestamped with ValidFrom/ValidTo

## Best Practices
1. **State Progression**
   - Always start lines in Draft state (0)
   - For Workflow Lines:
     - Follow complete workflow-defined states
     - For Event/Regulatory:
       - End in Posted (4)
     - For other types:
       - End in Approved (2)
   - For Non-Workflow Lines:
     - For Event/Regulatory:
       - Draft (0) → Posted (4) or Canceled (-4)
     - For other types:
       - Draft (0) → Approved (2) or Rejected (-2)
   - Document state (-1) is determined by all lines having negative states

2. **Workflow Configuration**
   - Define all required state transitions using positive states only
   - Ensure final state matches line type:
     - Event/Regulatory: Posted (4)
     - Other: Approved (2)
   - Document any special workflow rules
   - Maintain proper state progression order

3. **State Management**
   - Only allow valid state transitions
   - For Event/Regulatory lines:
     - Allow skipping Approved (2) state
   - For other line types:
     - Prevent reaching Completed (3) or Posted (4)
   - Handle negative states automatically:
     - If user signs (Reject) at any signature point:
       - Requested → Void (-1)
       - Approved → Rejected (-2)
       - Completed → Failed (-3)
       - Posted → Canceled (-4)
   - Maintain audit trail of state changes
