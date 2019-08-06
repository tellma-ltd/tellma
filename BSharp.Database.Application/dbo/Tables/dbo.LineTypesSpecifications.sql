CREATE TABLE [dbo].[LineTypesSpecifications] (
	[LineTypeId]					NVARCHAR (50),
	[EntryNumber]					INT,

	-- IsEditableTill specifies the state up till which is is possible to edit the field
	-- State "Archived" is used to indicate lifetime editability 
	-- Must be added to all fields
	-- if Document edit mode is immutable, signatures cannot be deleted, and edits are not allowed once the state is reached
	-- If edit mode is is rigid, no edits are allowed after reaching the state unless the signatories delete their signatures
	-- If edit mode is flexible, edits are allowed after reaching the state, and all signatories before the edit are alerted

	[DirectionIsVisible]			BIT				NOT NULL DEFAULT 0,
	[DirectionIsEditableTill]		NVARCHAR (255)	NOT NULL DEFAULT N'Posted',
	[DirectionExpression]			NVARCHAR (255),
	[DirectionEntryNumber]			NVARCHAR (255),
	[Direction]						SMALLINT,

	[AccountIdIsVisible]			BIT				NOT NULL DEFAULT 0,
	[AccountIsEditableTill]			NVARCHAR (255)	NOT NULL DEFAULT N'Posted',
	[AccountIdIfrsFilter]			NVARCHAR (255),
	[AccountIdExpression]			NVARCHAR (255),
	[AccountIdEntryNumber]			INT,

	[IfrsNoteIdIsVisible]			BIT				NOT NULL DEFAULT 0,
	[IfrsNoteIdIsEditableTill]		NVARCHAR (255)	NOT NULL DEFAULT N'Posted',
	[IfrsNoteIdExpression]			NVARCHAR (255),
	[IfrsNoteIdEntryNumber]			INT,
	[IfrsNoteId]					NVARCHAR (255),

	[ResponsibilityCenterIdIsVisible]BIT			NOT NULL DEFAULT 0,
	[ResponsibilityCenterIdIsEditableTill]NVARCHAR (255)	NOT NULL DEFAULT N'Posted',
	[ResponsibilityCenterIdExpression]NVARCHAR (255),
	[ResponsibilityCenterIdEntryNumber]INT,

	[ResourceIdIsVisible]			BIT				NOT NULL DEFAULT 0,
	[ResourceIdIsEditableTill]		NVARCHAR (255)	NOT NULL DEFAULT N'Posted',
	[ResourceIdExpression]			NVARCHAR (255),
	[ResourceIdEntryNumber]			INT,
	[ResourceId]					INT,

	[InstanceIdIsVisible]			BIT				NOT NULL DEFAULT 0,
	[InstanceIdExpression]			NVARCHAR (255),
	[InstanceIdEntryNumber]			INT,
	
	[BatchCodeIsVisible]			BIT				NOT NULL DEFAULT 0,
	[BatchCodeExpression]			NVARCHAR (255),
	[BatchCodeEntryNumber]			INT,

	[DueDateIsVisible]				BIT				NOT NULL DEFAULT 0,
	[DueDateExpression]				NVARCHAR (255),
	[DueDateEntryNumber]			INT,

	[QuantityIsVisible]				BIT				NOT NULL DEFAULT 0,
	[QuantityExpression]			NVARCHAR (255),
	[QuantityEntryNumber]			INT,
	[Quantity]						VTYPE,

	[MoneyAmountIsVisible]			BIT				NOT NULL DEFAULT 0,
	[MoneyAmountExpression]			NVARCHAR (255),
	[MoneyAmountEntryNumber]		INT,

	[MassIsVisible]					BIT				NOT NULL DEFAULT 0,
	[MassExpression]				NVARCHAR (255),
	[MassEntryNumber]				INT,

	[VolumeIsVisible]				BIT				NOT NULL DEFAULT 0,
	[VolumeExpression]				NVARCHAR (255),
	[VolumeEntryNumber]				INT,

	[AreaIsVisible]					BIT				NOT NULL DEFAULT 0,
	[AreaExpression]				NVARCHAR (255),
	[AreaEntryNumber]				INT,

	[LengthIsVisible]				BIT				NOT NULL DEFAULT 0,
	[LengthExpression]				NVARCHAR (255),
	[LengthEntryNumber]				INT,

	[TimeIsVisible]					BIT				NOT NULL DEFAULT 0,
	[TimeExpression]				NVARCHAR (255),
	[TimeEntryNumber]				INT,

	[CountIsVisible]				BIT				NOT NULL DEFAULT 0,
	[CountExpression]				NVARCHAR (255),
	[CountEntryNumber]				INT,
	[Count]							INT,
	
	[ValueIsVisible]				BIT				NOT NULL DEFAULT 0,
	[ValueExpression]				NVARCHAR (255),
	[ValueEntryNumber]				INT,

	[MemoIsVisible]					BIT				NOT NULL DEFAULT 0,
	[MemoExpression]				NVARCHAR (255),
	[MemoEntryNumber]				INT,

	[ExternalReferenceIsVisible]	BIT				NOT NULL DEFAULT 0,
	[ExternalReferenceExpression]	NVARCHAR (255),
	[ExternalReferenceEntryNumber]	INT,

	[AdditionalReferenceIsVisible]	BIT				NOT NULL DEFAULT 0,
	[AdditionalReferenceExpression]	NVARCHAR (255),
	[AdditionalReferenceEntryNumber]INT,

	[RelatedResourceId]				INT, -- Good, Service, Labor, Machine usage

	[RelatedAgentIsVisible]			BIT				NOT NULL DEFAULT 0,
	[RelatedAgentExpression]		NVARCHAR (255),
	[RelatedAgentEntryNumber]		INT,
		
	[RelatedQuantity]				MONEY ,			-- used in Tax accounts, to store the quantiy of taxable item
	[RelatedMoneyAmount]			MONEY 				NOT NULL DEFAULT 0, -- e.g., amount subject to tax

    CONSTRAINT [PK_LineTypeSpecifications] PRIMARY KEY CLUSTERED ([LineTypeId], [EntryNumber]),
	CONSTRAINT [FK_LineTypeSpecifications_LineTypes] FOREIGN KEY ([LineTypeId]) REFERENCES [dbo].[LineTypes] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);