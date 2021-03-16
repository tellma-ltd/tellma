import { SelectExpandArguments } from './select-expand-arguments';

export interface GetArguments extends SelectExpandArguments {
  top?: number;
  skip?: number;
  orderby?: string;
  search?: string;
  filter?: string;
  countEntities?: boolean;
  silent?: boolean;
}
