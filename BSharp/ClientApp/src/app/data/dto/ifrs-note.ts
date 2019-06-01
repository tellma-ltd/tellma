import { DtoForSaveKeyBase } from './dto-for-save-key-base';

export class IfrsConcept extends DtoForSaveKeyBase {
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
}

export class IfrsNote extends IfrsConcept {
  Node: string;
  Level: number;
  ParentNode: string;
  ChildCount: number;
  IsAggregate: boolean;
  ForDebit: boolean;
  ForCredit: boolean;
  CreatedAt: string;
  CreatedById: number | string;
  ModifiedAt: string;
  ModifiedById: number | string;
}

// Choice list
export const IfrsConcept_IfrsType = {
  'Amendment': 'IfrsConcept_Amendment',
  'Extension': 'IfrsConcept_Extension',
  'Regulatory': 'IfrsConcept_Regulatory'
};
