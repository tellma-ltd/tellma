import { GetByIdArguments } from './get-by-id-arguments';

export class GetChildrenArguments extends GetByIdArguments {
    i?: (string | number)[];
    filter?: string;
    roots?: boolean;
}
