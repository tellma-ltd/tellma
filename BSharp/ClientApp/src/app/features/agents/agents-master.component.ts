import { Component, OnInit, Input } from '@angular/core';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { addToWorkspace } from '~/app/data/util';
import { WorkspaceService } from '~/app/data/workspace.service';
import { MasterBaseComponent } from '~/app/shared/master-base/master-base.component';
import { metadata_Agent } from '~/app/data/entities/agent';
import { TranslateService } from '@ngx-translate/core';
import { ChoicePropDescriptor } from '~/app/data/entities/base/metadata';

@Component({
  selector: 'b-agents-master',
  templateUrl: './agents-master.component.html'
})
export class AgentsMasterComponent extends MasterBaseComponent implements OnInit {


  private agentsApi = this.api.agentsApi(this.notifyDestruct$); // for intellisense

  public tableColumnPaths: string[];
  public filterDefinition: any;
  public expand = '';

  @Input()
  filterDefault: string;

  constructor(private workspace: WorkspaceService, private api: ApiService, private translate: TranslateService) {
    super();
  }

  ngOnInit(): void {
  }

  public get c() {
    return this.workspace.current.Agent;
  }

  public get ws() {
    return this.workspace.current;
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

  public agentTypeLookup(value: string): string {
    const descriptor = <ChoicePropDescriptor> metadata_Agent(this.ws, this.translate, null).properties.AgentType;
    return descriptor.format(value);
  }
}
