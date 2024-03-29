import { EntryForReconciliation } from '../entities/entry-for-reconciliation';
import { ExternalEntry, ExternalEntryForSave } from '../entities/external-entry';
import { ReconciliationForSave, Reconciliation } from '../entities/reconciliation';

export interface ReconciliationArgumentsBase {
    accountId: number;
    agentId: number;
}

export interface ReconciliationGetUnreconciledArguments extends ReconciliationArgumentsBase {
    asOfDate: string;
    entriesTop: number;
    entriesSkip: number;
    externalEntriesTop: number;
    externalEntriesSkip: number;
}

export interface ReconciliationGetReconciledArguments extends ReconciliationArgumentsBase {
    fromDate: string;
    toDate: string;
    fromAmount: number;
    toAmount: number;
    externalReferenceContains: string;
    top: number;
    skip: number;
}

export interface ReconciliationSavePayload {
    ExternalEntries: ExternalEntryForSave[];
    Reconciliations: ReconciliationForSave[];
    DeletedExternalEntryIds: number[];
    DeletedReconciliationIds: number[];
}
export interface ReconciliationGetUnreconciledResponse {
    ExternalEntries: ExternalEntry[];
    Entries: EntryForReconciliation[];
    EntriesBalance: number;
    UnreconciledEntriesBalance: number;
    UnreconciledExternalEntriesBalance: number;
    UnreconciledEntriesCount: number;
    UnreconciledExternalEntriesCount: number;
}

export interface ReconciliationGetReconciledResponse {
    Reconciliations: Reconciliation[];
    ReconciledCount: number;
}
