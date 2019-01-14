import { isAndroid } from 'tns-core-modules/platform';

// TODO move to a config file
const isProduction = true;

export class AppConfig {
    apiAddress = isProduction ? 'https://bsharp11staging.azurewebsites.net/' :
     (isAndroid ? 'https://10.0.2.2:44339/' : 'https://localhost:44339/');
    identityAddress = '';
}

export const appconfig = new AppConfig();
