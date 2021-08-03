import { Component, OnInit, OnDestroy, ViewChild, TemplateRef, Inject } from '@angular/core';
import { Subscription, Subject, Observable, of } from 'rxjs';
import { MyAdminUserForSave } from '~/app/data/dto/my-admin-user';
import { DetailsStatus, WorkspaceService, AdminWorkspace } from '~/app/data/workspace.service';
import { NavigationService } from '~/app/data/navigation.service';
import { TranslateService } from '@ngx-translate/core';
import { ProgressOverlayService } from '~/app/data/progress-overlay.service';
import { AuthService } from '~/app/data/auth.service';
import { StorageService } from '~/app/data/storage.service';
import { ApiService } from '~/app/data/api.service';
import { DOCUMENT } from '@angular/common';
import { NgbModal, NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { switchMap, tap, catchError } from 'rxjs/operators';
import { AdminSettingsForClient } from '~/app/data/dto/admin-settings-for-client';
import { appsettings } from '~/app/data/global-resolver.guard';
import { AdminUser } from '~/app/data/entities/admin-user';
import { GetByIdResponse } from '~/app/data/dto/get-by-id-response';
import { addSingleToWorkspace } from '~/app/data/util';
import { clearServerErrors, applyServerErrors } from '~/app/shared/details/details.component';
import { supportedCultures } from '~/app/data/supported-cultures';

@Component({
  selector: 't-admin-shell',
  templateUrl: './admin-shell.component.html',
  styles: []
})
export class AdminShellComponent implements OnInit, OnDestroy {

  // For the menu on small screens
  public isCollapsed = true;

  // My User stuff
  private _subscription: Subscription;
  private notifyDestruct$ = new Subject<void>();
  private notifyFetch$: Subject<void>;
  private adminUsersApi = this.apiService.adminUsersApi(this.notifyDestruct$); // for intellisense
  public myUser: MyAdminUserForSave;
  public myUserStatus: DetailsStatus;
  private _errorMessage: string;
  private _saveErrorMessage: string;
  private _activeLanguages: string[];

  @ViewChild('myAccountModal', { static: true })
  myAccountModal: TemplateRef<any>;

  constructor(
    public workspace: WorkspaceService, public nav: NavigationService,
    private translate: TranslateService, private progress: ProgressOverlayService,
    private auth: AuthService, private storage: StorageService, private apiService: ApiService,
    @Inject(DOCUMENT) private document: Document, private modalService: NgbModal) {

    this.notifyFetch$ = new Subject<any>();
    this.notifyFetch$.pipe(
      switchMap(() => this.doFetch())
    ).subscribe();
  }

  ngOnInit() {
    this.adminUsersApi = this.apiService.adminUsersApi(this.notifyDestruct$);
  }

  ngOnDestroy() {

    // cancel any backend operations
    this.notifyDestruct$.next();

    if (!!this._subscription) {
      this._subscription.unsubscribe();
    }
  }

  // UI Binding

  public get activeLanguages(): string[] {
    if  (!this._activeLanguages) {
      this._activeLanguages = Object.keys(supportedCultures);
    }
    return this._activeLanguages;
  }

  public languageName(id: string): string {
    return supportedCultures[id];
  }

  public onSetLanguage(lang: string) {
    this.translate.use(lang);
    this.storage.setItem('user_culture', lang);
  }

  get currentLanguage(): string {
    const cultureName = this.workspace.ws.culture || this.translate.currentLang || this.translate.defaultLang || 'en';
    return cultureName;
  }

  onToggleCollapse() {
    this.isCollapsed = !this.isCollapsed;
  }

  onCollapse() {
    this.isCollapsed = true;
  }

  get settings(): AdminSettingsForClient {
    return !!this.ws ? this.ws.settings : null;
  }

  get ws(): AdminWorkspace {
    return this.workspace.admin;
  }

  get userName(): string {
    return this.ws.userSettings.Name;
  }

  get isRtl(): boolean {
    return this.workspace.ws.isRtl;
  }

  public onMySystemAccount(): void {
    // TODO make these pages part of the SPA
    location.href = appsettings.identityAddress + '/identity/manage/';
  }

  public onSignOut(): void {
    this.progress.startAsyncOperation('sign_out', 'RedirectingToSignOut');
    this.auth.signOut();
  }

  public get flip() {
    // this is to flip the UI icons in RTL
    return this.isRtl ? 'horizontal' : null;
  }

  // All the my admin account stuff

  public onMyAdminAccount(): void {

    this.fetch();
    this.workspace.ignoreKeyDownEvents = true;
    this.modalService.open(this.myAccountModal, { windowClass: 't-myuser-modal' })
      .result.then(
        () => this.onMyAdminAccountClose(),
        () => this.onMyAdminAccountClose(),
      );
  }

  private onMyAdminAccountClose(): void {
    this.workspace.ignoreKeyDownEvents = false;
  }

  private fetch() {
    this.notifyFetch$.next(null);
  }

  private doFetch(): Observable<void> {
    // first show the rotator
    this.myUserStatus = DetailsStatus.loading;
    return this.adminUsersApi.getMyUser().pipe(
      tap((response: GetByIdResponse<AdminUser>) => {

        // add the server item to the workspace
        addSingleToWorkspace(response, this.workspace);

        // remoev the rotator
        this.myUserStatus = DetailsStatus.loaded;
        this.onEdit();
      }),
      catchError((friendlyError) => {
        this._errorMessage = friendlyError.error;
        this.myUserStatus = DetailsStatus.error;
        return of(null);
      })
    );
  }

  private onEdit() {
    const myId = this.ws.userSettings.UserId;
    const user = this.ws.get('AdminUser', myId) as AdminUser;

    this.myUser = {
      Name: user.Name
    };
  }

  public onSave(modal: NgbActiveModal) {

    // clear any errors displayed
    this._errorMessage = null;
    this._saveErrorMessage = null;
    clearServerErrors(this.myUser);

    // prepare the save observable
    this.adminUsersApi.saveMyUser(this.myUser).subscribe(
      (response: GetByIdResponse<AdminUser>) => {

        // update the workspace with the entity from the server
        addSingleToWorkspace(response, this.workspace);
        modal.close(true);
      },
      (friendlyError) => {

        // This handles 422 ModelState errors
        if (friendlyError.status === 422) {
          const unboundServerErrors = applyServerErrors(this.myUser, friendlyError.error);

          if (Object.keys(unboundServerErrors).length > 0) {
            // This shouldn't happen
            console.error(unboundServerErrors);
          }
        } else {
          this._saveErrorMessage = friendlyError.error;
        }

        return of(null);
      }
    );
  }

  public get errorMessage(): string {
    return this._errorMessage;
  }

  public get saveErrorMessage(): string {
    return this._saveErrorMessage;
  }

  public get isMyUserLoaded(): boolean {
    return this.myUserStatus === DetailsStatus.loaded;
  }

  public get isMyUserError(): boolean {
    return this.myUserStatus === DetailsStatus.error;
  }

  public get isMyUserLoading(): boolean {
    return this.myUserStatus === DetailsStatus.loading;
  }

  public get myEmail(): string {
    return (this.ws.get('AdminUser', this.ws.userSettings.UserId) as AdminUser).Email;
  }

  public get canSave(): boolean {
    return this.isMyUserLoaded;
  }
}
