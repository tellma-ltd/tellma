import { UserCompany } from './user-company';

// tslint:disable:variable-name
export interface CompaniesForClient {
    IsAdmin: boolean;
    Companies: UserCompany[];
}
