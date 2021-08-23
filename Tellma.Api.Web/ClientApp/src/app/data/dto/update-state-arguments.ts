import { ActionArguments } from './action-arguments';
import { DefinitionState } from '../entities/base/definition-common';

export interface UpdateStateArguments extends ActionArguments {
    state: DefinitionState;
}
