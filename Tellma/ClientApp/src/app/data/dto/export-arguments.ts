// tslint:disable:variable-name
import { GetArguments } from './get-arguments';

export class ExportArguments extends GetArguments {
  format?: 'csv' | 'xlsx';
}

export const ExportArguments_Format = {
  xlsx: 'Excel',
  csv: 'CSV'
};
