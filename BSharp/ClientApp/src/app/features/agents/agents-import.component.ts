import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, ParamMap, Router } from '@angular/router';

@Component({
  selector: 'b-agents-import',
  templateUrl: './agents-import.component.html'
})
export class AgentsImportComponent implements OnInit {

  public agentType: 'individuals' | 'organizations';

  constructor(private route: ActivatedRoute, private router: Router) { }

  ngOnInit() {
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

  public get masterCrumb(): string {
    // TODO After implementing configuration
    const agentType = this.agentType;
    if (!!agentType) {
      return agentType.charAt(0).toUpperCase() + agentType.slice(1);
    }

    return agentType;
  }
}
