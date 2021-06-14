import { formatNumber } from '@angular/common';

export function accountingFormat(amount: number, digitsInfo: string): string {
    if (!!amount || amount === 0) {
        const result = formatNumber(Math.abs(amount), 'en-GB', digitsInfo);
        if (amount >= 0) {
            return ` ${result} `;
        } else {
            return `(${result})`;
        }
    } else {
        return '';
    }
}
