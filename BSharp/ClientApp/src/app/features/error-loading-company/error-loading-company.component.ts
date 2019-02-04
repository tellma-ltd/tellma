import { Component, OnInit } from '@angular/core';
import { WorkspaceService } from '~/app/data/workspace.service';
import { Location } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'b-error-loading-company',
  templateUrl: './error-loading-company.component.html',
  styleUrls: ['./error-loading-company.component.scss']
})
export class ErrorLoadingCompanyComponent implements OnInit {

  private _retryUrl: string;

  constructor(private workspace: WorkspaceService, private location: Location, private route: ActivatedRoute, private router: Router) {
    this.route.queryParamMap.subscribe(p => {
      this._retryUrl = p.get('retryUrl');
    });

  }

  ngOnInit() {
  }

  get errorMessage(): string {
    return this.workspace.ws.errorLoadingCompanyMessage;
  }

  public onGoBack() {
    this.location.back();
  }

  public onTryAgain() {
    this.router.navigateByUrl(this.retryUrl || '');
  }

  get showTryAgain(): boolean {
    return !!this.retryUrl;
  }

  get retryUrl(): string {
    return this._retryUrl;
  }
}
