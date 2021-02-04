import { EntityDescriptor } from './base/metadata';
import { WorkspaceService } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { EntityWithKey } from './base/entity-with-key';
import { EntityForSave } from './base/entity-for-save';
import { SettingsForClient } from '../dto/settings-for-client';

export interface IfrsConcept extends EntityForSave {
    Label?: string;
    Label2?: string;
    Label3?: string;
    Code?: string;
    Documentation?: string;
    Documentation2?: string;
    Documentation3?: string;
}

const _label = ['', '2', '3'].map(pf => 'Label' + pf);
let _settings: SettingsForClient;
let _cache: EntityDescriptor;

export function metadata_IfrsConcept(wss: WorkspaceService, trx: TranslateService): EntityDescriptor {
    const ws = wss.currentTenant;
    // Some global values affect the result, we check here if they have changed, otherwise we return the cached result
    if (ws.settings !== _settings) {
        _settings = ws.settings;
        const entityDesc: EntityDescriptor = {

            collection: 'IfrsConcept',
            titleSingular: () => trx.instant('IfrsConcept'),
            titlePlural: () => trx.instant('IfrsConcepts'),
            select: _label,
            apiEndpoint: 'ifrs-concepts',
            masterScreenUrl: 'ifrs-concepts',
            orderby: () => ws.isSecondaryLanguage ? [_label[1], _label[0]] : ws.isTernaryLanguage ? [_label[2], _label[0]] : [_label[0]],
            inactiveFilter: null, // No inactive filter
            format: (item: EntityWithKey) => ws.getMultilingualValueImmediate(item, _label[0]),
            formatFromVals: (vals: any[]) => ws.localize(vals[0], vals[1], vals[2]),
            properties: {
                Id: { datatype: 'numeric', control: 'number', label: () => trx.instant('Id'), minDecimalPlaces: 0, maxDecimalPlaces: 0 },
                Label: { datatype: 'string', control: 'text', label: () => trx.instant('Label') + ws.primaryPostfix },
                Label2: { datatype: 'string', control: 'text', label: () => trx.instant('Label') + ws.secondaryPostfix },
                Label3: { datatype: 'string', control: 'text', label: () => trx.instant('Label') + ws.ternaryPostfix },
                Code: { datatype: 'string', control: 'text', label: () => trx.instant('Code') },
                Documentation: {
                    datatype: 'string', control: 'text', label: () => trx.instant('IfrsConcept_Documentation') + ws.primaryPostfix
                },
                Documentation2: {
                    datatype: 'string', control: 'text', label: () => trx.instant('IfrsConcept_Documentation') + ws.secondaryPostfix
                },
                Documentation3: {
                    datatype: 'string', control: 'text', label: () => trx.instant('IfrsConcept_Documentation') + ws.ternaryPostfix
                },
            }
        };

        if (!ws.settings.SecondaryLanguageId) {
            delete entityDesc.properties.Name2;
            delete entityDesc.properties.Documentation2;
        }

        if (!ws.settings.TernaryLanguageId) {
            delete entityDesc.properties.Name3;
            delete entityDesc.properties.Documentation3;
        }

        _cache = entityDesc;
    }

    return _cache;
}

