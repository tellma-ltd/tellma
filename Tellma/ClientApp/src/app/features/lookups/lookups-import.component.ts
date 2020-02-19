import { Component, Input, OnInit } from '@angular/core';
import { WorkspaceService } from '~/app/data/workspace.service';
import { Router, ActivatedRoute, ParamMap } from '@angular/router';

@Component({
  selector: 't-lookups-import',
  templateUrl: './lookups-import.component.html',
  styles: []
})
export class LookupsImportComponent implements OnInit {

  @Input()
  public definitionId: string;

  constructor(private workspace: WorkspaceService, private router: Router, private route: ActivatedRoute) {
  }

  ngOnInit() {
    this.route.paramMap.subscribe((params: ParamMap) => {
      // This triggers changes on the screen

      const definitionId = params.get('definitionId');

      if (!definitionId || !this.ws.definitions.Lookups[definitionId]) {
        this.router.navigate(['page-not-found'], { relativeTo: this.route.parent, replaceUrl: true });
      }

      if (this.definitionId !== definitionId) {
        this.definitionId = definitionId;
      }
    });
  }

  public get ws() {
    return this.workspace.currentTenant;
  }

  public get masterCrumb(): string {
    const definitionId = this.definitionId;
    const definition = this.ws.definitions.Lookups[definitionId];
    if (!definition) {
      this.router.navigate(['page-not-found'], { relativeTo: this.route.parent, replaceUrl: true });
    }

    return this.ws.getMultilingualValueImmediate(definition, 'TitlePlural');
  }
}
