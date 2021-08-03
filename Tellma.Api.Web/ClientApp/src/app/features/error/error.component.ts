import { Component, OnInit } from '@angular/core';
import { WorkspaceService } from '~/app/data/workspace.service';
import { ActivatedRoute, Router } from '@angular/router';
import { Location } from '@angular/common';
import { StorageService } from '~/app/data/storage.service';

@Component({
  selector: 't-error',
  templateUrl: './error.component.html'
})
export class ErrorComponent implements OnInit {

  private _retryUrl: string;
  private _title: string;
  private _generalErrorMessage: string;
  private _detailedErrorMessage: string;

  constructor(
    private workspace: WorkspaceService, private location: Location,
    private route: ActivatedRoute, private router: Router, private storage: StorageService) {
    this.route.queryParamMap.subscribe(p => {
      // retry URL
      this._retryUrl = p.get('retryUrl');

    });

    this.route.paramMap.subscribe(p => {

      // title & general error
      const error = p.get('error');
      switch (error) {
        case 'unauthorized':
          this._title = 'Unauthorized';
          this._generalErrorMessage = 'Error_UnauthorizedForCompany';
          break;

        case 'loading-company-settings':
          this._title = 'Error_LoadingCompanySettings';
          break;

        case 'admin-unauthorized':
          this._title = 'Unauthorized';
          this._generalErrorMessage = 'Error_UnauthorizedForAdmin';
          break;

        case 'loading-admin-settings':
          this._title = 'Error_LoadingAdminSettings';
          break;

        case 'loading-global-settings':
          this._title = 'Error_LoadingGlobalSettings';
          break;

        case 'page-not-found':
          this._title = 'Error_PageNotFound';
          this._generalErrorMessage = `Error_PageNotFoundMessage`;
          break;

        default:
          this._title = 'Error';
          this._generalErrorMessage = 'Error_UnkownClientError';
          break;
      }

      // detailed error
      this._detailedErrorMessage = this.workspace.ws.errorMessage;
      // this._detailedErrorMessage = 'Unable to reach the server please, check the connection of your device.';

      this.workspace.ws.errorMessage = null;
    });

  }

  ngOnInit() {

  }

  // appears in large-font red header
  get title(): string {
    return this._title;
  }

  // appears in normal black font under the title
  get generalErrorMessage(): string {
    return this._generalErrorMessage;
  }

  // appears inside a danger alert with an error icon
  get detailedErrorMessage(): string {
    return this._detailedErrorMessage;
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

  public onBack() {
    this.storage.removeItem('last_visited_url_v2');
    this.location.back();
  }

  public onHome() {
    this.storage.removeItem('last_visited_url_v2');
    this.router.navigateByUrl('/root/welcome');
  }

  get flip() {
    return this.workspace.ws.isRtl ? 'horizontal' : null;
  }
}
