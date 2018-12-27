import { Component, OnInit, Input } from '@angular/core';
import { WorkspaceService } from 'src/app/data/workspace.service';
import { MeasurementUnit_UnitType } from 'src/app/data/dto/measurement-unit';
import { Observable, of } from 'rxjs';

@Component({
  selector: 'b-measurement-units-master',
  templateUrl: './measurement-units-master.component.html',
  styleUrls: ['./measurement-units-master.component.css']
})
export class MeasurementUnitsMasterComponent implements OnInit {

  @Input()
  mode: 'screen' | 'popup' = 'screen';

  onSelect(v: any) {
    console.log('select! ' + JSON.stringify(v));
  }

  constructor(private workspace: WorkspaceService) { }

  ngOnInit() {
  }

  public get ws() {
    return this.workspace.current.MeasurementUnits;
  }

  public UnitTypeLookup(value: string): string {
    return MeasurementUnit_UnitType[value];
  }

  public onActivate(ids: (number | string)[]): Observable<void> {
    console.log('onActivate');
    return of();
  }

  public onDeactivate(ids: (number | string)[]): Observable<void> {
    console.log('onDeactivate');
    return of();
  }
}
