import { DtoForSaveKeyBase } from './dto-for-save-key-base';

export class MeasurementUnitForSave extends DtoForSaveKeyBase {
  Name: string;
  Name2: string;
  Code: string;
  UnitType: 'Pure' | 'Time' | 'Distance' | 'Count' | 'Mass' | 'Volume' | 'Money';
  UnitAmount: number;
  BaseAmount: number;
}

export class MeasurementUnit extends MeasurementUnitForSave {
  IsActive: boolean;
  CreatedAt: string;
  CreatedBy: string;
  ModifiedAt: string;
  ModifiedBy: string;
}

// Choice list (Also repeated in measurement units master template)
export const MeasurementUnit_UnitType = {
  'Pure': 'MU_Pure',
  'Time': 'MU_Time',
  'Distance': 'MU_Distance',
  'Count': 'MU_Count',
  'Mass': 'MU_Mass',
  'Volume': 'MU_Volume',
  'Money': 'MU_Money'
};
