import { Pipe, PipeTransform } from '@angular/core';
import { isSpecified, formatAccounting } from '~/app/data/util';

@Pipe({
  name: 'accounting'
})
export class AccountingPipe implements PipeTransform {

  transform(value: number, digitsInfo?: string): unknown {
    if (!isSpecified(value)) {
      return null;
    }

    return formatAccounting(value, digitsInfo);
  }

}
