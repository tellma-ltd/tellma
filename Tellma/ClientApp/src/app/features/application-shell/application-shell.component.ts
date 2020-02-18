import { Component, OnInit, OnDestroy, Inject, TemplateRef, ViewChild } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { WorkspaceService, TenantWorkspace, DetailsStatus } from '~/app/data/workspace.service';
import { SettingsForClient } from '~/app/data/dto/settings-for-client';
import { AuthService } from '~/app/data/auth.service';
import { appsettings } from '~/app/data/global-resolver.guard';
import { ProgressOverlayService } from '~/app/data/progress-overlay.service';
import { NavigationService } from '~/app/data/navigation.service';
import { Subscription, Subject, Observable, of } from 'rxjs';
import { StorageService } from '~/app/data/storage.service';
import { DOCUMENT } from '@angular/common';
import { NgbModal, NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { MyUserForSave } from '~/app/data/dto/my-user';
import { User } from '~/app/data/entities/user';
import { ApiService } from '~/app/data/api.service';
import { switchMap, tap, catchError } from 'rxjs/operators';
import { GetByIdResponse } from '~/app/data/dto/get-by-id-response';
import { addSingleToWorkspace } from '~/app/data/util';
import { clearServerErrors, applyServerErrors } from '~/app/shared/details/details.component';

@Component({
  selector: 't-application-shell',
  templateUrl: './application-shell.component.html'
})
export class ApplicationShellComponent implements OnInit, OnDestroy {

  // For the menu on small screens
  public isCollapsed = true;

  // My User stuff
  private _subscription: Subscription;
  private notifyDestruct$ = new Subject<void>();
  private notifyFetch$: Subject<void>;
  private usersApi = this.apiService.usersApi(this.notifyDestruct$); // for intellisense
  public myUser: MyUserForSave;
  public myUserStatus: DetailsStatus;
  private _errorMessage: string;
  private _saveErrorMessage: string;

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
    this.usersApi = this.apiService.usersApi(this.notifyDestruct$);

    ////// These ensures that once in a company, the language is
    ////// restricted to one of the company's languages
    this.restrictToCompanyLanguages();
    this._subscription = this.workspace.stateChanged$
      .subscribe(() => this.restrictToCompanyLanguages()); // In case settings changed
  }

  ngOnDestroy() {

    // cancel any backend operations
    this.notifyDestruct$.next();

    if (!!this._subscription) {
      this._subscription.unsubscribe();
    }

    ////// when we exit a company, reset the language how it was
    // IMPORTANT: also in root.component.ts, keep in sync
    const defaultCulture = this.document.documentElement.lang || 'en';
    const userCulture = this.storage.getItem('user_culture') || defaultCulture;
    this.translate.use(userCulture);
  }

  private restrictToCompanyLanguages() {
    const primary = this.settings.PrimaryLanguageId;
    const secondary = this.settings.SecondaryLanguageId;
    const ternary = this.settings.TernaryLanguageId;
    const current = this.translate.currentLang;

    if (current !== primary && current !== secondary && current !== ternary) {
      const preferred = this.ws.userSettings.PreferredLanguage;
      if (!!preferred && (preferred === primary || preferred === secondary || preferred === ternary)) {
        this.translate.use(preferred);
      } else {
        // The user's preferred language is not one of the company languages
        this.translate.use(primary);
      }
    }
  }

  private onSetLanguage(lang: string) {
    this.translate.use(lang);
    this.storage.setItem('user_culture', lang);

    // TODO: Set preferred langauge
  }

  // UI Binding

  onToggleCollapse() {
    this.isCollapsed = !this.isCollapsed;
  }

  onCollapse() {
    this.isCollapsed = true;
  }

  onPrimary() {
    this.onSetLanguage(this.settings.PrimaryLanguageId);
  }

  onSecondary() {
    this.onSetLanguage(this.settings.SecondaryLanguageId);
  }

  onTernary() {
    this.onSetLanguage(this.settings.TernaryLanguageId);
  }

  get settings(): SettingsForClient {
    return !!this.ws ? this.ws.settings : null;
  }

  get ws(): TenantWorkspace {
    return this.workspace.currentTenant;
  }

  get userName(): string {
    return this.ws.getMultilingualValueImmediate(this.ws.userSettings, 'Name');
  }
  get companyName(): string {
    return this.ws.getMultilingualValueImmediate(this.ws.settings, 'ShortCompanyName');
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

  // All the my account stuff

  public onMyCompanyAccount(): void {

    this.fetch();
    this.workspace.ignoreKeyDownEvents = true;
    this.modalService.open(this.myAccountModal, { windowClass: 't-myuser-modal' })
      .result.then(
        () => this.onMyCompanyAccountClose(),
        () => this.onMyCompanyAccountClose(),
      );
  }

  private fetch() {
    this.notifyFetch$.next(null);
  }

  private doFetch(): Observable<void> {
    // first show the rotator
    this.myUserStatus = DetailsStatus.loading;
    return this.usersApi.getMyUser().pipe(
      tap((response: GetByIdResponse<User>) => {

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
    const user = this.ws.get('User', myId) as User;

    this.myUser = {
      Name: user.Name,
      Name2: user.Name2,
      Name3: user.Name3,
      PreferredLanguage: user.PreferredLanguage
    };
  }

  public onSave(modal: NgbActiveModal) {

    // clear any errors displayed
    this._errorMessage = null;
    this._saveErrorMessage = null;
    clearServerErrors(this.myUser);

    // prepare the save observable
    this.usersApi.saveMyUser(this.myUser).subscribe(
      (response: GetByIdResponse<User>) => {

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

  private onMyCompanyAccountClose(): void {
    this.workspace.ignoreKeyDownEvents = false;
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
    return (this.ws.get('User', this.ws.userSettings.UserId) as User).Email;
  }

  public get myImageId(): string {
    return (this.ws.get('User', this.ws.userSettings.UserId) as User).ImageId;
  }

  public get canSave(): boolean {
    return this.isMyUserLoaded;
  }
}
