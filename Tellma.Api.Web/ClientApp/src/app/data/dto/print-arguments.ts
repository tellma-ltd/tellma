import { ReportArguments } from '../workspace.service';

export interface PrintArguments {
    culture?: string;
    version?: string;
}

export interface PrintEntitiesArguments extends PrintArguments {
    filter?: string;
    orderby?: string;
    top?: number;
    skip?: number;
    i?: (string | number)[];
}

// tslint:disable-next-line:no-empty-interface
export interface PrintEntityByIdArguments extends PrintArguments {
    // id?: string | number;
}
