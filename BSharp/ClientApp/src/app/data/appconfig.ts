import { environment } from '~/environments/environment';

export class AppConfig {
    apiAddress = environment.production ? '' : 'https://localhost:44339/';
    identityAddress = environment.production ? '' : 'https://localhost:44339/';
}


export const appconfig = new AppConfig();
