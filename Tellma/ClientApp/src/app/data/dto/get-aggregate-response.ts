// tslint:disable:variable-name
import { EntitiesResponse } from './entities-response';
import { Entity } from '../entities/base/entity';

export interface GetAggregateResponse extends EntitiesResponse<Entity> {
    Top: number;
}
