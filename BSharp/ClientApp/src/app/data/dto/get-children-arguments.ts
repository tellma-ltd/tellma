import { GetByIdArguments } from './get-by-id-arguments';

export class GetChildrenArguments extends GetByIdArguments {
    ids?: (string | number)[];
    filter?: string;
}
