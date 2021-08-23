import { SelectExpandArguments } from './select-expand-arguments';

// tslint:disable-next-line:no-empty-interface
export interface GetByIdsArguments extends SelectExpandArguments {
    i: (string | number)[];
}
