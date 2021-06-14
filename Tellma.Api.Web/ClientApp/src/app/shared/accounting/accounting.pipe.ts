import { formatNumber } from '@angular/common';
import { Pipe, PipeTransform } from '@angular/core';
import { isSpecified } from '~/app/data/util';
import { accountingFormat } from './accounting-format';

@Pipe({
  name: 'accounting'
})
export class AccountingPipe implements PipeTransform {

  transform(value: number, digitsInfo?: string): unknown {
    if (!isSpecified(value)) {
      return null;
    }

    return accountingFormat(value, digitsInfo);
  }
}
