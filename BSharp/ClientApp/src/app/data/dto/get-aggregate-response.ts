// tslint:disable:variable-name
import { EntitiesResponse } from './get-response';
import { Entity } from '../entities/base/entity';

export class GetAggregateResponse extends EntitiesResponse<Entity> {
    Top: number;
}
