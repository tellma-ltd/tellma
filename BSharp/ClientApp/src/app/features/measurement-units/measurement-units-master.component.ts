import { Component, OnInit } from '@angular/core';
import { WorkspaceService } from 'src/app/data/workspace.service';
import { MeasurementUnitUnitType } from 'src/app/data/dto/measurement-unit';

@Component({
  selector: 'b-measurement-units-master',
  templateUrl: './measurement-units-master.component.html',
  styleUrls: ['./measurement-units-master.component.css']
})
export class MeasurementUnitsMasterComponent implements OnInit {

  constructor(private workspace: WorkspaceService) { }

  ngOnInit() {
  }

  public get ws() {
    return this.workspace.current.MeasurementUnits;
  }

  public UnitTypeLookup(value: string): string {
    return MeasurementUnitUnitType[value];
  }
}
