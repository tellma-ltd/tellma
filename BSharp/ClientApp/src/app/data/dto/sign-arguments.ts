import { ActionArguments } from '../action-arguments';

export interface SignArguments extends ActionArguments {
    toState: number;
    reasonId?: number;
    reasonDetails?: string;
    onBehalfOfUserId?: number;
    roleId?: number;
    signedAt?: string;
}
