import { DtoDescriptor } from './base/metadata';
import { TenantWorkspace } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { EntityWithKey } from './base/entity-with-key';
import { EntityForSave } from './base/entity-for-save';

export class IfrsConcept extends EntityForSave {
    IfrsType: 'Amendment' | 'Extension' | 'Regulatory';
    Label: string;
    Label2: string;
    Label3: string;
    Documentation: string;
    Documentation2: string;
    Documentation3: string;
    EffectiveDate: string;
    ExpiryDate: string;
    IsActive: boolean;
    CreatedAt: string;
    CreatedById: number | string;
    ModifiedAt: string;
    ModifiedById: number | string;
}

const _select = ['', '2', '3'].map(pf => 'Label' + pf);
export function metadata_IfrsConceptInner(ws: TenantWorkspace, trx: TranslateService, _subtype: string): DtoDescriptor {
    // Some global values affect the result, we check here if they have changed, otherwise we return the cached result
    return {
        select: _select,
        apiEndpoint: '',
        orderby: ws.isSecondaryLanguage ? [_select[1], _select[0]] : ws.isTernaryLanguage ? [_select[2], _select[0]] : [_select[0]],
        format: (item: EntityWithKey) => ws.getMultilingualValueImmediate(item, _select[0]),
        properties: {
            IfrsType: {
                control: 'choice',
                label: trx.instant('IfrsConcepts_IfrsType'),
                choices: ['Amendment', 'Extension', 'Regulatory'],
                format: (c: string) => {
                    switch (c) {
                        case 'Amendment': return trx.instant('IfrsConcept_Amendment');
                        case 'Extension': return trx.instant('IfrsConcept_Extension');
                        case 'Regulatory': return trx.instant('IfrsConcept_Regulatory');
                        default: return c;
                    }
                }
            },
            Label: { control: 'text', label: trx.instant('IfrsConcepts_Label') + ws.primaryPostfix },
            Label2: { control: 'text', label: trx.instant('IfrsConcepts_Label') + ws.secondaryPostfix },
            Label3: { control: 'text', label: trx.instant('IfrsConcepts_Label') + ws.ternaryPostfix },
            Id: { control: 'text', label: trx.instant('Code') },
            Documentation: { control: 'text', label: trx.instant('IfrsConcepts_Documentation') + ws.primaryPostfix },
            Documentation2: { control: 'text', label: trx.instant('IfrsConcepts_Documentation') + ws.secondaryPostfix },
            Documentation3: { control: 'text', label: trx.instant('IfrsConcepts_Documentation') + ws.ternaryPostfix },
            EffectiveDate: { control: 'date', label: trx.instant('IfrsConcepts_EffectiveDate') },
            ExpiryDate: { control: 'date', label: trx.instant('IfrsConcepts_ExpiryDate') },
            IsActive: { control: 'boolean', label: trx.instant('IsActive') },
            CreatedAt: { control: 'datetime', label: trx.instant('CreatedAt') },
            CreatedBy: { control: 'navigation', label: trx.instant('CreatedBy'), type: 'LocalUser', foreignKeyName: 'CreatedById' },
            ModifiedAt: { control: 'datetime', label: trx.instant('ModifiedAt') },
            ModifiedBy: { control: 'navigation', label: trx.instant('ModifiedBy'), type: 'LocalUser', foreignKeyName: 'ModifiedById' }
        }
    };
}
