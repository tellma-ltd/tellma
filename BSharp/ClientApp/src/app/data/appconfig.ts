import { environment } from '~/environments/environment';

export class AppConfig {
    apiAddress = environment.production ? '' : 'https://localhost:44339/';
    identityAddress = environment.production ? '' : 'https://localhost:44339';

    // EVERYTHING below is optional/can be loaded from the discovery
    // document but when set here it can lead to lightning-fast startup
    // since the discovery document doesn't have to be loaded
    identityConfig = environment.production ? {} : {
        jwks: {
            keys: [
                {
                    kty: 'RSA',
                    use: 'sig',
                    kid: '2b8f9fe7747e07c0679d633c88d372c1',
                    e: 'AQAB',
                    alg: 'RS256',
                    n: 'rGVpLbPuUqscSDYG6X0oVfnBnH4oUugnHFMxg8s2xqMnjDZ32luEC6' +
                        '7n9nwukknDEq4HBYAfyiGfa8oi0MSsCH1Etj7otaKuqStxU7rf-y-9yKz7' +
                        'RIDCNJ6IWkXMmNIs79CdWAtqtX6RXK0mgG48nmZmbNml7as-CvvKtTS' +
                        'wPDrlwrTtTYff8UIgKpA__zmP52UNAPZKmiXHeiZqM3W75NUzS2qrpRpoBcm' +
                        '1HZH5OiHPI8upOed8IogauiLXh-kY5eTc6b5qg2nBwphkVKZ3I5lJkrs' +
                        'GQkNkvH6pLQmw6O9FgbswM2fHaLKMhLOhPlAgDAVpfYnTF2OKFuswa3WUQQ'
                }
            ]
        },

        loginUrl: this.identityAddress + '/connect/authorize',
        sessionCheckIFrameUrl: this.identityAddress + '/connect/checksession',
        logoutUrl: this.identityAddress + '/connect/endsession',

        // Periodicaly refresh the tokens silently every X milliseconds
        tokenRefreshPeriodInSeconds: 60 * 60
    };
}


export const appconfig = new AppConfig();
