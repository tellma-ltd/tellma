// tslint:disable:variable-name
import { GetArguments } from './get-arguments';

export interface ExportArguments extends GetArguments {
  format?: 'csv' | 'xlsx';
}

export const ExportArguments_Format = {
  xlsx: 'Excel',
  csv: 'CSV'
};
