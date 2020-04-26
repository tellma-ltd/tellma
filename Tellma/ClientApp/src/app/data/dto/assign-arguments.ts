import { ActionArguments } from './action-arguments';

export interface AssignArguments extends ActionArguments {
    assigneeId: number;
    comment: string;
}
