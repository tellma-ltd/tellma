export interface PrintingPreviewTemplate {
    Context?: string;
    Collection?: string;
    DefinitionId?: number;
    DownloadName?: string;
    Body?: string;
    Parameters?: PrintingPreviewParameter[];
}

export interface PrintingPreviewParameter {
    Key?: string;
    Control?: string;
}
