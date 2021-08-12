import { ActionArguments } from './action-arguments';

export interface UpdateAssignmentArguments extends ActionArguments {
    id: string | number;
    comment: string;
}


