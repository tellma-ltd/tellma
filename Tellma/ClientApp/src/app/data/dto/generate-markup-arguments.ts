export interface GenerateMarkupArguments {
    culture?: string;
}

export interface GenerateMarkupByFilterArguments extends GenerateMarkupArguments {
    filter?: string;
    orderby?: string;
    top?: number;
    skip?: number;
    i?: (string | number)[];
}

// tslint:disable-next-line:no-empty-interface
export interface GenerateMarkupByIdArguments extends GenerateMarkupArguments {
    // id?: string | number;
}
