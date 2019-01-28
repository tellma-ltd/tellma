import { Component, Input, OnInit } from '@angular/core';
import { Subject } from 'rxjs';
import { tap } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { Agent, AgentForSave, Agent_Gender } from '~/app/data/dto/agent';
import { addToWorkspace } from '~/app/data/util';
import { WorkspaceService } from '~/app/data/workspace.service';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { ActivatedRoute, ParamMap, Router } from '@angular/router';

@Component({
  selector: 'b-agents-details',
  templateUrl: './agents-details.component.html',
  styleUrls: ['./agents-details.component.scss']
})
export class AgentsDetailsComponent extends DetailsBaseComponent implements OnInit {

  private _genderChoices: { name: string, value: any }[];
  private notifyDestruct$ = new Subject<void>();
  private agentsApi = this.api.agentsApi(this.agentType, this.notifyDestruct$); // for intellisense
  private _agentType: 'individuals' | 'organizations';

  public birthDateTimeName: string;

  @Input()
  public get agentType(): 'individuals' | 'organizations' {
    return this._agentType;
  }

  public set agentType(t: 'individuals' | 'organizations') {
    if (this._agentType !== t) {
      this._agentType = t;
      this.agentsApi = this.api.agentsApi(this.agentType, this.notifyDestruct$);
      this.birthDateTimeName = `Agent_${t}_BirthDateTime`;
    }
  }

  create = () => {
    const result = new AgentForSave();
    result.IsRelated = false;
    return result;
  }

  constructor(private workspace: WorkspaceService, private api: ApiService, private route: ActivatedRoute, private router: Router) {
    super();
  }

  ngOnInit(): void {
    if (this.mode === 'screen') {
      this.route.paramMap.subscribe((params: ParamMap) => {
        // This triggers changes on the screen
        const agentType = params.get('agentType');

        if (['individuals', 'organizations'].indexOf(agentType) === -1) {
          this.router.navigate(['page-not-found']);
        }

        if (this.agentType !== agentType) {
          this.agentType = <'individuals' | 'organizations'>agentType;
        }
      });
    }
  }

  public get masterCrumb(): string {
    // TODO After implementing configuration
    const agentType = this.agentType;
    if (!!agentType) {
      return agentType.charAt(0).toUpperCase() + agentType.slice(1);
    }

    return agentType;
  }


  get isIndividual(): boolean {
    return this.agentType === 'individuals';
  }

  get genderChoices(): { name: string, value: any }[] {

    if (!this._genderChoices) {
      this._genderChoices = Object.keys(Agent_Gender)
        .map(key => ({ name: Agent_Gender[key], value: key }));
    }

    return this._genderChoices;
  }

  public genderLookup(value: string): string {
    if (!value) {
      return '';
    }

    return Agent_Gender[value];
  }


  public onActivate = (model: Agent): void => {
    if (!!model && !!model.Id) {
      this.agentsApi.activate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe(null, this.details.handleActionError);
    }
  }

  public onDeactivate = (model: Agent): void => {
    if (!!model && !!model.Id) {
      this.agentsApi.deactivate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe(null, this.details.handleActionError);
    }
  }

  public showActivate = (model: Agent) => !!model && !model.IsActive;
  public showDeactivate = (model: Agent) => !!model && model.IsActive;
}
