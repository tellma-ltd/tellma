import { EntityForSave } from './base/entity-for-save';

export interface DocumentLineDefinitionEntryForSave extends EntityForSave {
    LineDefinitionId?: number;
    EntryIndex?: number;
    PostingDate?: string;
    PostingDateIsCommon?: boolean;
    Memo?: string;
    MemoIsCommon?: boolean;
    ParticipantId?: number;
    ParticipantIsCommon?: boolean;
    CurrencyId?: string;
    CurrencyIsCommon?: boolean;
    CustodyId?: number;
    CustodyIsCommon?: boolean;
    ResourceId?: number;
    ResourceIsCommon?: boolean;
    Quantity?: number;
    QuantityIsCommon?: boolean;
    UnitId?: number;
    UnitIsCommon?: boolean;
    CenterId?: number;
    CenterIsCommon?: boolean;
    Time1?: string;
    Time1IsCommon?: boolean;
    Time2?: string;
    Time2IsCommon?: boolean;
    ExternalReference?: string;
    ExternalReferenceIsCommon?: boolean;
    AdditionalReference?: string;
    AdditionalReferenceIsCommon?: boolean;
}

export interface DocumentLineDefinitionEntry extends DocumentLineDefinitionEntryForSave {
    DocumentId?: number;
    CreatedAt?: string;
    CreatedById?: number;
    ModifiedAt?: string;
    ModifiedById?: number;
}
