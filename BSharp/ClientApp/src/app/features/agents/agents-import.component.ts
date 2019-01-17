import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, ParamMap } from '@angular/router';

@Component({
  selector: 'b-agents-import',
  templateUrl: './agents-import.component.html',
  styleUrls: ['./agents-import.component.scss']
})
export class AgentsImportComponent implements OnInit {

  public agentType: 'Individual' | 'Organization';

  constructor(private route: ActivatedRoute) { }

  ngOnInit() {
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
