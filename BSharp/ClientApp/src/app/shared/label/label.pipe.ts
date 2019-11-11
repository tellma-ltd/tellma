import { Pipe, PipeTransform } from '@angular/core';
import { PropDescriptor } from '~/app/data/entities/base/metadata';

@Pipe({
  name: 'label',
  pure: false
})
export class LabelPipe implements PipeTransform {

  transform(value: any, ...args: any[]): any {
    const prop = value as PropDescriptor;
    return !!prop && !!prop.label ? prop.label() : '';
  }

}
