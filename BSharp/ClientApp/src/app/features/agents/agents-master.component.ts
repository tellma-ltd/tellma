import { Component, Input, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { addToWorkspace } from '~/app/data/util';
import { WorkspaceService } from '~/app/data/workspace.service';
import { MasterBaseComponent } from '~/app/shared/master-base/master-base.component';
import { Agent_Gender } from '~/app/data/dto/agent';
import { ParamMap, ActivatedRoute } from '@angular/router';

@Component({
  selector: 'b-agents-master',
  templateUrl: './agents-master.component.html',
  styleUrls: ['./agents-master.component.scss']
})
export class AgentsMasterComponent extends MasterBaseComponent implements OnInit {

  private agentsApi = this.api.agentsApi(this.agentType, this.notifyDestruct$); // for intellisense
  private _agentType: 'Individual' | 'Organization';

  @Input()
  public get agentType(): 'Individual' | 'Organization' {
    return this._agentType;
  }

  public set agentType(t: 'Individual' | 'Organization') {
    if (this._agentType !== t) {
      this._agentType = t;
      this.agentsApi = this.api.agentsApi(this.agentType, this.notifyDestruct$);
      this.birthDateTimeName = `Agent_${t}_BirthDateTime`;

      if (t === 'Individual') {
        this.tableColumnPaths = [
          'Name', 'Name2', 'Title', 'Title2',
          'Code', 'Address', 'BirthDateTime',
          'IsRelated', 'TaxIdentificationNumber',
          'Gender', 'IsActive'
        ];
      }

      if (t === 'Organization') {
        this.tableColumnPaths = [
          'Name', 'Name2', 'Code', 'Address', 'BirthDateTime',
          'IsRelated', 'TaxIdentificationNumber', 'IsActive'
        ];
      }
    }
  }

  public tableColumnPaths: string[];
  public birthDateTimeName: string;
  public filterDefinition: any;

  constructor(private workspace: WorkspaceService, private api: ApiService, private route: ActivatedRoute) {
    super();
  }

  ngOnInit(): void {
    if (this.mode === 'screen') {
      this.route.paramMap.subscribe((params: ParamMap) => {
        // This triggers changes on the screen
        const t = params.get('agentType');
        let agentType: 'Individual' | 'Organization';

        if (t === 'individuals') {
          agentType = 'Individual';
        }
        if (t === 'organizations') {
          agentType = 'Organization';
        }

        if (this.agentType !== agentType) {
            this.agentType = agentType;
        }
      });
    }
  }

  public get ws() {
    return this.workspace.current.Custodies;
  }

  public genderLookup(value: string): string {
    return Agent_Gender[value];
  }

  public onActivate = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.agentsApi.activate(ids, { ReturnEntities: true }).pipe(
      tap(res => addToWorkspace(res, this.workspace))
    );

    // The master template handles any errors
    return obs$;
  }

  public onDeactivate = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.agentsApi.deactivate(ids, { ReturnEntities: true }).pipe(
      tap(res => addToWorkspace(res, this.workspace))
    );

    // The master template handles any errors
    return obs$;
  }
}
