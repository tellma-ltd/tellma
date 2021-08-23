import { EntitiesResponse } from './entities-response';
import { DetailsEntry } from '../entities/details-entry';

export interface StatementResponse extends EntitiesResponse<DetailsEntry> {
    Opening?: number;
    OpeningQuantity?: number;
    OpeningMonetaryValue?: number;
    Closing?: number;
    ClosingQuantity?: number;
    ClosingMonetaryValue?: number;
    Skip?: number;
    Top?: number;
    TotalCount?: number;
}
