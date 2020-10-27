import { EntityForSave } from './base/entity-for-save';
import { EntryForReconciliation } from './entry-for-reconciliation';
import { ExternalEntry } from './external-entry';

// tslint:disable:max-line-length
export interface ReconciliationForSave<TEntry = ReconciliationEntryForSave, TExternalEntry = ReconciliationExternalEntryForSave> extends EntityForSave {
    Entries?: TEntry[];
    ExternalEntries?: TExternalEntry[];
}

export interface Reconciliation extends ReconciliationForSave<ReconciliationEntry, ReconciliationExternalEntry> {
    CreatedAt?: string;
    CreatedById?: number;
}

export interface ReconciliationEntryForSave extends EntityForSave {
    EntryId?: number;
}

export interface ReconciliationEntry extends ReconciliationEntryForSave {
    Entry?: EntryForReconciliation;
    CreatedAt?: string;
    CreatedById?: number;
}

export interface ReconciliationExternalEntryForSave extends EntityForSave {
    ExternalEntryId?: number;
    ExternalEntryIndex?: number;
}

export interface ReconciliationExternalEntry extends ReconciliationExternalEntryForSave {
    ExternalEntry?: ExternalEntry;
    CreatedAt?: string;
    CreatedById?: number;
}
