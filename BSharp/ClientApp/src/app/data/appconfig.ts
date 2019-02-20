import { environment } from '~/environments/environment';

export class AppConfig {
    apiAddress = environment.production ? '' : 'https://localhost:44339/';
    identityAddress = environment.production ? window.location.origin : 'https://localhost:44339';

    // EVERYTHING below is optional/can be loaded from the discovery
    // document but when set here it can lead to lightning-fast startup
    // since the discovery document doesn't have to be loaded
    identityConfig = environment.production ? {
        jwks: {
            keys: [
                {
                    kty: 'RSA',
                    use: 'sig',
                    kid: '1207053BFBAA39A5C0E02FF3504ED32B19593B4A',
                    x5t: 'EgcFO_uqOaXA4C_zUE7TKxlZO0o',
                    e: 'AQAB',
                    n: '2Q1VVUalK5OiDkBrWq_neO61uyVtMTKWy9eG9hOFeiWJPkNPsPgEozaDfzK6qfTkc' +
                        'Kj4Aeayeu3af6EbR18oZgei7bCbVqskr_W5WBFz9Au0mO8iLETsM6lgSA4FxC1daHNmf' +
                        'Qwm60prY31c03uY-94erL1xwVhacNVgbQ_OUHdi0sLqOKnbIAX8JW0lwmudONjFmSpKa' +
                        'Cuw5G5_GNJYqWFUzju3D7PW38qHpuYPbSWxULOEts2MxEdVx1wRn64S_PXX04oPkxhVe' +
                        '4cvMJsECBp9o7akrE7EhRaB_OIqAgcBA4YZCgAzuzZzpf4TH9cQDgLAga7OOTRjt7031' +
                        'OS4veAyuZMTU_GBlmViCS4l_T87xkGOt7A6PLbMG7AAsuwD9wh2-SuIBjsmETiNcpZun' +
                        'QiSoXvsztc5l4VyjpLqI1pw82AfxDU_Mc1mc-Wy0vqA8-AOSGT_4vGLAdZdvYqKri3pK' +
                        'KqyIjjKQULbwZxE1_XFFPtQ21NFNJlgMX_AYjuO5cqD28Ms26TvzLxzUfSpxYHb9aeLb' +
                        '9Ps4c_eQgnbzk9MIaEyJC8TPJXvSML5LCzfccc9LM7x4jYYsKGqDCijL-KKSb7ReDxLM' +
                        '0KBc_jw3cYmRQsxEiRgfVtp-KAseRDyDjJ6T6zdENuYmfaPQat5cwplBY5l9X3aIl8mbfPDmZk',
                    x5c: [
                        'MIIFBDCCAuygAwIBAgIJAKtxDqYNPZxkMA0GCSqGSIb3DQEBCwUAMBcxFTATBgNVB' +
                        'AMMDGJhbmFuLWl0LmNvbTAeFw0xOTAyMjAwMTAyNDJaFw0yOTAyMTcwMTAyNDJaMB' +
                        'cxFTATBgNVBAMMDGJhbmFuLWl0LmNvbTCCAiIwDQYJKoZIhvcNAQEBBQADggIPADC' +
                        'CAgoCggIBANkNVVVGpSuTog5Aa1qv53jutbslbTEylsvXhvYThXoliT5DT7D4BKM2' +
                        'g38yuqn05HCo+AHmsnrt2n+hG0dfKGYHou2wm1arJK/1uVgRc/QLtJjvIixE7DOpY' +
                        'EgOBcQtXWhzZn0MJutKa2N9XNN7mPveHqy9ccFYWnDVYG0PzlB3YtLC6jip2yAF/C' +
                        'VtJcJrnTjYxZkqSmgrsORufxjSWKlhVM47tw+z1t/Kh6bmD20lsVCzhLbNjMRHVcd' +
                        'cEZ+uEvz119OKD5MYVXuHLzCbBAgafaO2pKxOxIUWgfziKgIHAQOGGQoAM7s2c6X+' +
                        'x/XEA4CwIGuzjk0Y7e9N9TkuL3gMrmTE1PxgZZlYgkuJf0/O8ZBjrewOjy2zBuwAL' +
                        'LsA/cIdvkriAY7JhE4jXKWbp0IkqF77M7XOZeFco6S6iNacPNgH8Q1PzHNZnPlstL' +
                        '6gPPgDkhk/+LxiwHWXb2Kiq4t6SiqsiI4ykFC28GcRNf1xRT7UNtTRTSZYDF/wGI7' +
                        'juXKg9vDLNuk78y8c1H0qcWB2/Wni2/T7OHP3kIJ285PTCGhMiQvEzyV70jC+Sws3' +
                        '3HHPSzO8eI2GLChqgwooy/iikm+0Xg8SzNCgXP48N3GJkULMRIkYH1bafigLHkQ8g' +
                        '4yek+s3RDbmJn2j0GreXMKZQWOZfV92iJfJm3zw5mZAgMBAAGjUzBRMB0GA1UdDgQ' +
                        'WBBT4k1cyMMkJqQLIJTP3BjT0Rqn8mDAfBgNVHSMEGDAWgBT4k1cyMMkJqQLIJTP3' +
                        'BjT0Rqn8mDAPBgNVHRMBAf8EBTADAQH/MA0GCSqGSIb3DQEBCwUAA4ICAQCgxcm1M' +
                        'yYpMVOjWEDQygvh1Zx2yGZC3rHb0y2DI51sOXmhIBvo0/tpqRaIVchvU0al8f07bM' +
                        '4BXbCD9jy4Lvf4j1HIOt90tpmyupJYhPWeivwpAEnpnEFkrrpzZDRiQH3CG53RXb4' +
                        'Vo6LcGfeCbtgtW2U4JjNTJXuon2uLSuUzLLWh9WFSt09jjbkB4/3DRqf/jX3XrlS2' +
                        'ATZpIijZ22QOK+tOzkHh1YW9HgLPTAJ0lYQMQmWQ0mExQN4tOKsTP79Clx1X1kLJK' +
                        'K61J3m/0BCVECWi8nMefAcrxltOpYKAnxw2haZX82OgWakFMo2teYOl88XTVqHDUg' +
                        'byVIG+TSh/hEiHVx8nPoAFUBKWXb0zlpPQGhHIQ1r+3qOlpmW3Hcx6joyIFrjmOEG' +
                        '7F/kqOUu4K1HMSYZiZW7BgOpcImjK9I/gd2spp1EvWSYjpw86618v7FpioJKXSQ/v' +
                        'p50xwBgLt2M9g2N2JRN+/zaISakULUWxBRYcr94xIOoq6a+jcICsLY5VWl6k+Efal' +
                        'LFiyM9dgi+MaKaD16+ZU9K5lqbMpowYJFgSQnpK5UFqV+4KH8IeGYTT/YP1kFyuz/' +
                        '+VCxu66gb+iWdNGmmoe0jNasr9l4H9oAJUnqrq5w1sOR4l9mp2fesUjDQigtpnysw' +
                        'b/O0GSMplarLQqzthKMGh8uyf9Q=='
                    ],
                    alg: 'RS256'
                }
            ]
        },

        loginUrl: this.identityAddress + '/connect/authorize',
        sessionCheckIFrameUrl: this.identityAddress + '/connect/checksession',
        logoutUrl: this.identityAddress + '/connect/endsession',

        // Periodicaly refresh the tokens silently every X milliseconds
        tokenRefreshPeriodInSeconds: 60 * 60
    } : {
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
