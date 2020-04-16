import { SelectExpandArguments } from './select-expand-arguments';

export interface ActionArguments extends SelectExpandArguments {
    returnEntities?: boolean;
}
