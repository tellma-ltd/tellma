import { EntitiesResponse } from './entities-response';
import { DetailsEntry } from '../entities/details-entry';

export interface StatementResponse extends EntitiesResponse<DetailsEntry> {
    Opening?: number;
    Closing?: number;
    Skip?: number;
    Top?: number;
    TotalCount?: number;
}
