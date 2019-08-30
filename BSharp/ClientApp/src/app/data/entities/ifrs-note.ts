import { SettingsForClient } from './settings';
import { DtoDescriptor } from './base/metadata';
import { TenantWorkspace } from '../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { IfrsConcept, metadata_IfrsConceptInner } from './ifrs-concept';

export class IfrsNote extends IfrsConcept {
  Node: string;
  Level: number;
  ParentNode: string;
  ChildCount: number;
  IsAggregate: boolean;
  ForDebit: boolean;
  ForCredit: boolean;
}

// Choice list
export const IfrsConcept_IfrsType = {
  'Amendment': 'IfrsConcept_Amendment',
  'Extension': 'IfrsConcept_Extension',
  'Regulatory': 'IfrsConcept_Regulatory'
};

let _currentLang: string;
let _settings: SettingsForClient;
let _cache: DtoDescriptor;

export function metadata_IfrsNote(ws: TenantWorkspace, trx: TranslateService, _subtype: string): DtoDescriptor {
  // Some global values affect the result, we check here if they have changed, otherwise we return the cached result
  if (trx.currentLang !== _currentLang || ws.settings !== _settings) {
    _currentLang = trx.currentLang;
    _settings = ws.settings;
    _cache = metadata_IfrsConceptInner(ws, trx, _subtype);
    _cache.apiEndpoint = 'ifrs-notes';
    _cache.properties['IsAggregate'] = { control: 'boolean', label: trx.instant('IfrsNotes_IsAggregate') };
    _cache.properties['ForDebit'] = { control: 'boolean', label: trx.instant('IfrsNotes_ForDebit') };
    _cache.properties['ForCredit'] = { control: 'boolean', label: trx.instant('IfrsNotes_ForCredit') };

    // tree stuff
    _cache.properties['ChildCount'] = {
      control: 'number', label: trx.instant('TreeChildCount'), minDecimalPlaces: 0,
      maxDecimalPlaces: 0, alignment: 'right'
    };
    _cache.properties['ActiveChildCount'] = {
      control: 'number', label: trx.instant('TreeActiveChildCount'),
      minDecimalPlaces: 0, maxDecimalPlaces: 0, alignment: 'right'
    };
    _cache.properties['Level'] = {
      control: 'number', label: trx.instant('TreeLevel'),
      minDecimalPlaces: 0, maxDecimalPlaces: 0, alignment: 'right'
    };
    _cache.properties['Parent'] = {
      control: 'navigation', label: trx.instant('TreeParent'),
      type: 'ProductCategory', foreignKeyName: 'ParentId'
    };
  }

  return _cache;
}

