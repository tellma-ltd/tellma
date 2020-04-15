import { ActionArguments } from './action-arguments';

export interface UpdateStateArguments extends ActionArguments {
    state: string;
}
