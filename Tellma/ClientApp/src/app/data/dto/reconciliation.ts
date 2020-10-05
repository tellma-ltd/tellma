import { ExternalEntry, ExternalEntryForSave } from '../entities/external-entry';
import { ReconciliationForSave, Reconciliation } from '../entities/reconciliation';

export interface ReconciliationArgumentsBase {
    AccountId: number;
    CustodyId: number;
    Top: number;
    Skip: number;
}


export interface ReconciliationLoadUnreconciledArguments extends ReconciliationArgumentsBase {
    AsOfDate: string;
}

export interface ReconciliationLoadReconciledArguments extends ReconciliationArgumentsBase {
    FromDate: string;
    ToDate: string;
    FromAmount: number;
    ToAmount: number;
    ExternalReferenceContains: string;
}

export interface ReconciliationSaveArguments extends ReconciliationArgumentsBase {
    ReturnReconciled: boolean;
    AsOfDate: string;
    FromDate: string;
    ToDate: string;
    FromAmount: number;
    ToAmount: number;
    ExternalReferenceContains: string;
}

export interface ReconciliationSavePayload {
    ExternalEntries: ExternalEntryForSave[];
    Reconciliations: ReconciliationForSave[];
    DeletedExternalEntryIds: number[];
    DeletedReconciliationIds: number[];
}

export interface ReconciliationLoadUnreconciledResponse {
    ExternalEntries: ExternalEntry[];
    Entries: number[];
}

export interface ReconciliationLoadReconciledResponse extends ReconciliationLoadUnreconciledResponse {
    Reconciliations: Reconciliation[];
}

export interface ReconciliationReportArguments extends ReconciliationArgumentsBase {
    AsOfDate: string;
}
