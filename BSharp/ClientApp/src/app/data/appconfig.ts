import { environment } from '~/environments/environment';

export class AppConfig {
    apiAddress = environment.production ? '' : 'https://localhost:44339/';
    identityAddress = '';
}


export const appconfig = new AppConfig();
