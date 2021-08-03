// tslint:disable:variable-name
import { EntityWithKey } from '../entities/base/entity-with-key';
import { Entity } from '../entities/base/entity';
import { EntitiesResponse } from './entities-response';

export interface GetResponse<TEntity extends Entity = EntityWithKey> extends EntitiesResponse<TEntity> {
  Skip: number;
  Top: number;
  OrderBy: string;
  Desc: boolean;
  TotalCount: number;
}
