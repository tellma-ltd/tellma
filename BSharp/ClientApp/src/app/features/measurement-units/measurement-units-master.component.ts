import { Component, Input, OnDestroy, OnInit } from '@angular/core';
import { Observable, Subject } from 'rxjs';
import { tap } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { MeasurementUnit_UnitType } from '~/app/data/dto/measurement-unit';
import { addToWorkspace } from '~/app/data/util';
import { WorkspaceService } from '~/app/data/workspace.service';

@Component({
  selector: 'b-measurement-units-master',
  templateUrl: './measurement-units-master.component.html',
  styleUrls: ['./measurement-units-master.component.scss']
})
export class MeasurementUnitsMasterComponent implements OnInit, OnDestroy {

  @Input()
  public mode: 'screen' | 'popup' = 'screen';

  private notifyDestruct$ = new Subject<void>();
  private measurementUnitsApi = this.api.measurementUnitsApi(this.notifyDestruct$); // for intellisense

  constructor(private workspace: WorkspaceService, private api: ApiService) {

    this.measurementUnitsApi = this.api.measurementUnitsApi(this.notifyDestruct$);
  }

  onSelect(v: any) {
    console.log('select! ' + JSON.stringify(v));
  }

  ngOnInit() {
  }

  ngOnDestroy(): void {
    this.notifyDestruct$.next();
  }

  public get ws() {
    return this.workspace.current.MeasurementUnits;
  }

  public unitTypeLookup(value: string): string {
    return MeasurementUnit_UnitType[value];
  }

  public get unitTypes(): string[] {
    return Object.keys(MeasurementUnit_UnitType);
  }

  public onActivate = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.measurementUnitsApi.activate(ids, { ReturnEntities: true }).pipe(
      tap(res => addToWorkspace(res, this.workspace))
    );

    // The master template handles any errors
    return obs$;
  }

  public onDeactivate = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.measurementUnitsApi.deactivate(ids, { ReturnEntities: true }).pipe(
      tap(res => addToWorkspace(res, this.workspace))
    );

    // The master template handles any errors
    return obs$;
  }
}
