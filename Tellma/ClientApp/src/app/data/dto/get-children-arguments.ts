import { GetByIdArguments } from './get-by-id-arguments';

export interface GetChildrenArguments extends GetByIdArguments {
    i?: (string | number)[];
    filter?: string;
    roots?: boolean;
}
