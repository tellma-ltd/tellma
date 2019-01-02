import { Component, OnInit, ViewChild, Input } from '@angular/core';
import { ICanDeactivate } from 'src/app/data/dirty-data.guard';
import { DetailsComponent } from 'src/app/shared/details/details.component';
import { Observable } from 'rxjs';
import { MeasurementUnit_UnitType, MeasurementUnitForSave } from 'src/app/data/dto/measurement-unit';

@Component({
  selector: 'b-measurement-units-details',
  templateUrl: './measurement-units-details.component.html',
  styleUrls: ['./measurement-units-details.component.css']
})
export class MeasurementUnitsDetailsComponent implements OnInit, ICanDeactivate {

  // TODO move to a base class
  @Input()
  public mode: 'screen' | 'popup' = 'screen';


  @ViewChild(DetailsComponent)
  details: DetailsComponent;

  createNew = () => {
    const result = new MeasurementUnitForSave();
    result.UnitAmount = 1;
    result.BaseAmount = 1;
    return result;
  }

  constructor() { }

  ngOnInit(): void {
  }


  // It might make sense to move these to a base class for
  // all details components, instead of repeating ourselves
  canDeactivate(): boolean | Observable<boolean> {
    return this.details.canDeactivate();
  }

  private _unitTypeChoices: { name: string, value: any }[];
  get unitTypeChoices(): { name: string, value: any }[] {

    if (!this._unitTypeChoices) {
      this._unitTypeChoices = Object.keys(MeasurementUnit_UnitType)
        .filter(e => e !== 'Money').map(
          key => ({ name: MeasurementUnit_UnitType[key], value: key }));
    }

    return this._unitTypeChoices;
  }

  public unitTypeLookup(value: string): string {
    if (!value) {
      return '';
    }

    return MeasurementUnit_UnitType[value];
  }
}
