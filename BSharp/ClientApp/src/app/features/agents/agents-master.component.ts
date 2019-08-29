import { Component, Input, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { addToWorkspace } from '~/app/data/util';
import { WorkspaceService } from '~/app/data/workspace.service';
import { MasterBaseComponent } from '~/app/shared/master-base/master-base.component';
import { ParamMap, ActivatedRoute, Router } from '@angular/router';
import { EntityWithKey } from '~/app/data/entities/base/entity-with-key';

@Component({
  selector: 'b-agents-master',
  templateUrl: './agents-master.component.html',
  styleUrls: ['./agents-master.component.scss']
})
export class AgentsMasterComponent extends MasterBaseComponent implements OnInit {

  @Input()
  public set agentType(t: 'individuals' | 'organizations' | 'all') {
    if (this._agentType !== t) {
      this._agentType = t;
      this.agentsApi = this.api.agentsApi(this.agentType, this.notifyDestruct$);
      this.birthDateTimeName = `Agent_${t}_BirthDateTime`;

      if (t === 'individuals') {
        this.tableColumnPaths = [
          'Name', 'Name2', 'Title', 'Title2',
          'Code', 'Address', 'BirthDateTime',
          'IsRelated', 'TaxIdentificationNumber',
          'Gender', 'IsActive'
        ];
      }

      if (t === 'organizations') {
        this.tableColumnPaths = [
          'Name', 'Name2', 'Code', 'Address', 'BirthDateTime',
          'IsRelated', 'TaxIdentificationNumber', 'IsActive'
        ];
      }

      if (t === 'all') {
        this.tableColumnPaths = [
          'Name', 'Name2', 'Code', 'Address',
          'IsRelated', 'TaxIdentificationNumber', 'IsActive'
        ];
      }
    }
  }

  public get agentType(): 'individuals' | 'organizations' | 'all' {
    return this._agentType;
  }

  private agentsApi = this.api.agentsApi(this.agentType, this.notifyDestruct$); // for intellisense
  private _agentType: 'individuals' | 'organizations' | 'all';

  public tableColumnPaths: string[];
  public birthDateTimeName: string;
  public filterDefinition: any;
  public expand = '';

  constructor(private workspace: WorkspaceService, private api: ApiService, private route: ActivatedRoute, private router: Router) {
    super();

    this.route.paramMap.subscribe((params: ParamMap) => {
      // This triggers changes on the screen
      if (params.has('agentType')) {
        const agentType = params.get('agentType');

        if (['individuals', 'organizations', 'all'].indexOf(agentType) === -1
        || agentType === 'all' && this.mode === 'screen') {
          this.router.navigate(['page-not-found']);
        }

        if (this.agentType !== agentType) {
          this.agentType = <'individuals' | 'organizations' | 'all'>agentType;
        }
      }
    });
  }

  ngOnInit(): void {
  }

  public get c() {
    return this.workspace.current.Agent;
  }

  public get ws() {
    return this.workspace.current;
  }

  // public genderLookup(value: string): string {
  //   return Agent_Gender[value];
  // }

  public get masterCrumb(): string {
    // TODO After implementing configuration
    const agentType = this.agentType;
    if (!!agentType) {
      return agentType === 'all' ? 'Agents' : agentType.charAt(0).toUpperCase() + agentType.slice(1);
    }

    return agentType;
  }

  public onActivate = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.agentsApi.activate(ids, { returnEntities: true }).pipe(
      tap(res => addToWorkspace(res, this.workspace))
    );

    // The master template handles any errors
    return obs$;
  }

  public onDeactivate = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.agentsApi.deactivate(ids, { returnEntities: true }).pipe(
      tap(res => addToWorkspace(res, this.workspace))
    );

    // The master template handles any errors
    return obs$;
  }
}
