import { EntityForSave } from './base/entity-for-save';

export interface DocumentLineDefinitionEntryForSave extends EntityForSave {
    LineDefinitionId?: number;
    EntryIndex?: number;
    PostingDate?: string;
    PostingDateIsCommon?: boolean;
    Memo?: string;
    MemoIsCommon?: boolean;
    CurrencyId?: string;
    CurrencyIsCommon?: boolean;
    CenterId?: number;
    CenterIsCommon?: boolean;

    CustodianId?: number;
    CustodianIsCommon?: boolean;
    RelationId?: number;
    RelationIsCommon?: boolean;
    ResourceId?: number;
    ResourceIsCommon?: boolean;
    NotedRelationId?: number;
    NotedRelationIsCommon?: boolean;

    Quantity?: number;
    QuantityIsCommon?: boolean;
    UnitId?: number;
    UnitIsCommon?: boolean;
    Time1?: string;
    Time1IsCommon?: boolean;
    Duration?: number;
    DurationIsCommon?: boolean;
    DurationUnitId?: number;
    DurationUnitIsCommon?: boolean;
    Time2?: string;
    Time2IsCommon?: boolean;
    ExternalReference?: string;
    ExternalReferenceIsCommon?: boolean;
    ReferenceSourceId?: number;
    ReferenceSourceIsCommon?: boolean;
    InternalReference?: string;
    InternalReferenceIsCommon?: boolean;
}

export interface DocumentLineDefinitionEntry extends DocumentLineDefinitionEntryForSave {
    DocumentId?: number;
    CreatedAt?: string;
    CreatedById?: number;
    ModifiedAt?: string;
    ModifiedById?: number;
}
