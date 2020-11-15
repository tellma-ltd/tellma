// tslint:disable:no-string-literal
import { Location, formatNumber } from '@angular/common';
import {
  Component, EventEmitter, Input, OnDestroy, OnInit, TemplateRef,
  ViewChild, Output, HostListener, DoCheck
} from '@angular/core';
import { ActivatedRoute, ParamMap, Router, Params, NavigationExtras } from '@angular/router';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { TranslateService } from '@ngx-translate/core';
import { catchError, switchMap, tap, skip, finalize } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { EntityForSave } from '~/app/data/entities/base/entity-for-save';
import { GetByIdResponse } from '~/app/data/dto/get-by-id-response';
import { EntitiesResponse } from '~/app/data/dto/entities-response';
import { addSingleToWorkspace, addToWorkspace, computeSelectForDetailsPicker, FriendlyError, printBlob } from '~/app/data/util';
import { DetailsStatus, MasterDetailsStore, WorkspaceService, MAXIMUM_COUNT } from '~/app/data/workspace.service';
import { ICanDeactivate } from '~/app/data/unsaved-changes.guard';
import { Subject, Observable, of, Subscription } from 'rxjs';
import { EntityDescriptor, metadata } from '~/app/data/entities/base/metadata';
import { environment } from '~/environments/environment';
import { GetByIdArguments } from '~/app/data/dto/get-by-id-arguments';
import { SaveArguments } from '~/app/data/dto/save-arguments';
import { SettingsForClient } from '~/app/data/dto/settings-for-client';
import { DefinitionsForClient } from '~/app/data/dto/definitions-for-client';
import { EntityWithKey } from '~/app/data/entities/base/entity-with-key';

export interface DropdownAction {
  template: TemplateRef<any>;
  action: (model: EntityForSave) => void;
  canAction?: (model: EntityForSave) => boolean;
  actionTooltip?: (model: EntityForSave) => string;
  showAction?: (model: EntityForSave) => boolean;
}

export type DocumentLayout = 'document' | 'full-screen';

@Component({
  selector: 't-details',
  templateUrl: './details.component.html'
})
export class DetailsComponent implements OnInit, OnDestroy, DoCheck, ICanDeactivate {

  @Input()
  expand: string;

  @Input()
  select: string;

  @Input()
  selectTemplate: string;

  @Input()
  additionalSelect: string; // Loaded after save in popup mode, should have been called popupSaveSelect

  @Input()
  extraParams: { [key: string]: any };

  @Input()
  masterCrumb: string;

  @Input()
  detailsCrumb: string;

  @Input()
  showCreateButton = true;

  @Input()
  showEditButton = true;

  @Input()
  showDeleteButton = true;

  @Input()
  documentHeaderTemplate: TemplateRef<any>;

  @Input()
  documentTemplate: TemplateRef<any>;

  @Input()
  sidebarTemplate: TemplateRef<any>;

  @Input()
  toolbarTemplate: TemplateRef<any>;

  @Input()
  savePreprocessing: (entity: EntityForSave) => void;

  @Input()
  actions: DropdownAction[] = [];

  @Input() // popup: only the title and the document are visible
  mode: 'popup' | 'screen' | 'preview' = 'screen';

  @Input() // determines the shape of the document
  layout: DocumentLayout = 'document';

  // (collection, definition, idString) represent the identity of an abstract "instance" of this screen
  // so when they change it will be as if a screen closed and another screen opened from the point
  // of view of the user, for performance reasons Angular does not destroy and recreate the component
  // if the same component is still used but its input has changed, so to simulate a screen change
  // we call destroy and init in the property setters of these two properties, if this is not the
  // first time they are being set, the same pattern is used in the master screen

  @Input()
  collection: string;

  @Input()
  definitionId: number;

  @Input()
  stateKey: string;

  @Input()
  idString: string;

  @Input()
  handleFreshExtras: (extras: { [key: string]: any }) => void;

  @Input()
  theme: 'light' | 'dark' = 'light';

  /**
   * Encodes any custom screen state in the url params
   */
  @Input()
  encodeCustomStateFunc: (params: Params) => void;

  @Output()
  saved = new EventEmitter<number | string>();

  @Output()
  cancel = new EventEmitter<void>();

  @ViewChild('errorModal', { static: true })
  errorModal: TemplateRef<any>;

  @ViewChild('successModal', { static: true })
  successModal: TemplateRef<any>;

  @ViewChild('unsavedChangesModal', { static: true })
  unsavedChangesModal: TemplateRef<any>;

  private _subscriptions: Subscription;
  private _editModel: EntityForSave;
  private notifyFetch$: Subject<void>;
  private notifyDestruct$ = new Subject<void>();
  private localState = new MasterDetailsStore();  // Used in popup mode
  private _errorMessage: string; // in the document area itself
  private _modalErrorMessage: string; // in the modal
  private _modalSuccessMessage: string; // in the modal
  private _unboundServerErrors: string[]; // in the modal
  private _serverErrors: { [path: string]: string[] }; // all server errors
  private _pristineModelJson: string;
  private crud = this.api.crudFactory(this.apiEndpoint, this.notifyDestruct$); // Just for intellisense

  // For ngDoCheck
  private _firstTime = true;
  private _collectionOld: string;
  private _definitionIdOld: number;
  private _stateKeyOld: string;
  private _idStringOld: string;

  // Moved below the fields to keep tslint happy
  @Input()
  createFunc: () => EntityForSave = () => ({})

  @Input()
  isInactive: (model: EntityForSave) => string = (model: EntityForSave) => !!model &&
    (model['IsActive'] == null || model['IsActive'] === false) ? 'Error_CannotModifyInactiveItemPleaseActivate' : null

  @Input()
  cloneFunc: (item: EntityForSave) => EntityForSave = (item: EntityForSave) => {
    if (!!item) {
      const clone = JSON.parse(JSON.stringify(item)) as EntityForSave;
      delete clone.Id;
      delete clone.EntityMetadata;
      delete clone.serverErrors;

      if (clone['ImageId']) {
        delete clone['ImageId'];
      }

      return clone;
    } else {
      // programmer mistake
      console.error('Cloning a non existing item');
      return null;
    }
  }

  @Input()
  registerPristineFunc: (pristineModel: EntityForSave) => void =
    (pristineModel: EntityForSave) => this._pristineModelJson = JSON.stringify(pristineModel)

  @Input()
  isDirtyFunc: (potentiallyDirtyModel: EntityForSave) => boolean = (potentiallyDirtyModel: EntityForSave) => {
    // By default we compare the JSON for dirty check
    // Some screens may wish to optimise this if JSON operations are expensive
    return this._pristineModelJson !== JSON.stringify(potentiallyDirtyModel);
  }

  @HostListener('window:beforeunload', ['$event'])
  beforeUnload($event: BeforeUnloadEvent) {
    // Prompts the user if they attempt to close the browser before saving changes
    if (environment.production && this.isDirty) {
      $event.returnValue = this.translate.instant('UnsavedChangesConfirmationMessage');
    }
  }

  constructor(
    private workspace: WorkspaceService, private api: ApiService, private location: Location,
    private router: Router, private route: ActivatedRoute, public modalService: NgbModal, private translate: TranslateService) {
    // the constructor contains initializations and wiring
    // that survives over the lifetime of the component itself
    // even if apiEndpoint or idString change

    // when the notifyFetch$ subject fires, cancel existing backend
    // call and dispatch a new backend call
    this.notifyFetch$ = new Subject<any>();
    this.notifyFetch$.pipe(
      switchMap(() => this.doFetch())
    ).subscribe();
  }

  ngOnInit() {

    // Rest all fields to their defaults
    this.localState = new MasterDetailsStore();
    this._errorMessage = null;
    this._modalErrorMessage = null;
    this._modalSuccessMessage = null;
    this._unboundServerErrors = [];
    this._serverErrors = null;
    this.registerPristineFunc(null);

    const handleFreshStateFromUrl = (params: ParamMap, firstTime = false) => {
      // the id parameter from the URI is only avaialble in screen mode
      // when it changes set idString which triggers a new refresh
      if (this.isScreenMode) {

        // When set to true, it means a parameter that defines a screen has changed
        let screenDefChange = false;

        // When set to true, it means the url is out of step with the state
        let triggerUrlStateChange = false;

        // Id
        if (params.has('id')) {
          const newId = params.get('id');
          if (this.idString !== newId) {
            this.idString = newId;
            screenDefChange = true;
          }
        }

        // definition Id
        const defIdParamName = 'definitionId';
        if (params.has(defIdParamName)) {
          const newDefId = +params.get(defIdParamName);
          if (!!newDefId && this.definitionId !== newDefId) {
            this.definitionId = newDefId;
            screenDefChange = true;
          }
        }

        // state key
        const stateKeyParamName = 'state_key';
        if (params.has(stateKeyParamName)) {
          const stateKey = params.get(stateKeyParamName);
          if (this.stateKey !== stateKey) {
            this.stateKey = stateKey;
            screenDefChange = true;
          }
        } else if (!!this.stateKey) { // Prevents infinite loop
          triggerUrlStateChange = true;
        }

        if (screenDefChange && !firstTime) {
          return false; // Don't bother with the rest of ngOnInit
        }

        if (triggerUrlStateChange || firstTime) {
          // The URL is out of step with the state => sync the two
          // This happens when we navigate to the screen again 2nd time
          // We must be careful here to avoid an infinite loop
          this.urlStateChange();
        }
      }

      return true; // The rest of ngOnInit can be executed
    };

    this._subscriptions = new Subscription();
    this._subscriptions.add(this.route.paramMap.pipe(skip(1)).subscribe(handleFreshStateFromUrl)); // future changes
    const carryOn = handleFreshStateFromUrl(this.route.snapshot.paramMap, true); // right now

    if (carryOn) {

      // Now that we have the definitionId
      this.crud = this.api.crudFactory(this.apiEndpoint, this.notifyDestruct$);

      // Fetch the data of the screen based on apiEndpoint and idString
      this.fetch();
    }
  }

  ngOnDestroy() {
    // cancel any backend operations
    this.notifyDestruct$.next();

    if (!!this._subscriptions) {
      this._subscriptions.unsubscribe();
    }
  }

  ngDoCheck() {
    // When navigating to another screen, Angular may reuse the same t-details component
    // without calling the onInit and onDestory lifecycle hooks, causing cross contamination
    // of state, here we call the lifecycle hooks manually if any of these properties changes
    // Note: we don't use ngOnChanges because that one is only called when the input changes
    // via binding, but sometimes the input is changed internally e.g. in response to a URL change

    let screenDefChanges = false;
    if (this._collectionOld !== this.collection) {
      this._collectionOld = this.collection;
      screenDefChanges = true;
    }
    if (this._definitionIdOld !== this.definitionId) {
      this._definitionIdOld = this.definitionId;
      screenDefChanges = true;
    }
    if (this._stateKeyOld !== this.stateKey) {
      this._stateKeyOld = this.stateKey;
      screenDefChanges = true;
    }
    if (this._idStringOld !== this.idString) {
      this._idStringOld = this.idString;
      screenDefChanges = true;
    }

    if (screenDefChanges) {
      if (this._firstTime) {
        this._firstTime = false;
      } else {
        this.ngOnDestroy();
        this.ngOnInit();
      }
    }
  }

  /**
   * This is invoked whenever a change occurs in the screen state that must be encoded in the URL
   */
  public urlStateChange(): void {
    if (this.isScreenMode) {
      const params: Params = {
      };

      if (!!this.encodeCustomStateFunc) {
        this.encodeCustomStateFunc(params);
      }

      // TODO: Add the built in stuff to params
      if (!!this.stateKey) {
        params.state_key = this.stateKey;
      }

      this.router.navigate(['.', params], { relativeTo: this.route, replaceUrl: true });
    }
  }

  get entityDescriptor(): EntityDescriptor {
    const coll = this.collection;
    return !!coll ? metadata[coll](this.workspace, this.translate, this.definitionId) : null;
  }

  get apiEndpoint(): string {
    const meta = this.entityDescriptor;
    return !!meta ? meta.apiEndpoint : null;
  }

  private getAndClearCloneId() {
    const cloneId = this.workspace.cloneId;
    delete this.workspace.cloneId;
    return cloneId;
  }

  private getAndClearIsEdit() {
    const isEdit = this.workspace.isEdit;
    delete this.workspace.isEdit;
    return isEdit;
  }

  private fetch() {
    this.notifyFetch$.next(null);
  }

  private doFetch(): Observable<void> {
    // clear the errors before refreshing
    this.clearErrors();

    // grab the configured state
    const s = this.state;

    // calculate some logical values
    const cloneId = this.getAndClearCloneId();
    const showInEditMode = this.getAndClearIsEdit();
    const isCloning = !!cloneId;
    const isNewNotClone = this.isNew && !isCloning;
    const isCloneOfAvailableItem = isCloning && !!this.workspace.current[this.collection][cloneId];

    // the block in the first IF statement returns immediately, it's either
    // a create new or a clone of an item that exists in the workspace, if
    // neither is true, then we have to fetch the record from the server,
    // then either display it or clone it
    if (isNewNotClone || isCloneOfAvailableItem) {

      // Create two entities, one for editing, and the other for dirty checking
      let editModel: EntityForSave;
      let pristineModal: EntityForSave;
      if (isCloneOfAvailableItem) {
        // IF it's a cloning operation, clone the item from workspace
        const item = this.workspace.current[this.collection][cloneId];
        editModel = this.cloneFunc(item);
        pristineModal = this.cloneFunc(item);
      } else {
        // IF it's create new, don't fetch anything, create an item in memory
        editModel = this.createFunc();
        pristineModal = this.createFunc();
      }

      this._editModel = editModel;

      // marks it as non-dirty until the user makes the first change
      this.registerPristineFunc(pristineModal);

      // Show edit form
      s.detailsStatus = DetailsStatus.edit;

      // return
      // IF it's a cloning operation, clone the item from workspace
      return of();

    } else {
      // IF it's the last viewed item also don't do anything
      if (!!s.detailsId && s.detailsId.toString() === this.idString && s.detailsStatus === DetailsStatus.loaded) {
        if (showInEditMode) {
          this.onEdit();
        }

        // the application caches the last record that was viewed by the user
        // if the new id is equal to the Id of the last record then just display
        // that last record. This is helpful when navigating to Id after a create new
        return of();
      } else {
        // ELSE fetch the record from server
        // first show the rotator
        s.detailsStatus = DetailsStatus.loading;

        // if we're cloning we need to fetch the clone id
        // otherwise we need to fetch the usual id input of the screen
        const id = isCloning ? cloneId : this.idString;

        // server call
        const args: GetByIdArguments = { expand: this.expand, select: this.select };
        return this.crud.getById(id, args, this.extraParams).pipe(
          tap((response: GetByIdResponse) => {

            // add the server item to the workspace
            s.detailsId = addSingleToWorkspace(response, this.workspace);
            s.extras = response.Extras;

            this.incrementRefreshCounter();

            if (!!this.handleFreshExtras) {
              this.handleFreshExtras(response.Extras);
            }

            if (isCloning) {
              // call the same method again but this time the cloned
              // item is immediately available in the workspace
              this.doFetch();

            } else {
              // display the item in readonly if it's a screen
              // or in edit if it's a popup
              s.detailsStatus = DetailsStatus.loaded;
              if (this.isPopupMode || showInEditMode) {
                this.onEdit();
              }
            }
          }),
          catchError((friendlyError) => {
            this._errorMessage = friendlyError.error;
            s.detailsStatus = DetailsStatus.error;
            return of(null);
          })
        );
      }
    }
  }

  private clearErrors(): void {
    this._errorMessage = null;
    this._modalErrorMessage = null;
    this._unboundServerErrors = [];
    this._serverErrors = null;
  }

  public get state(): MasterDetailsStore {
    // important to always reference the source, and not keep a local reference
    // on some occasions the source can be reset and using a local reference can cause bugs
    if (this.mode === 'popup') {

      // popups use a local store that vanishes when the popup is destroyed
      if (!this.localState) {
        this.localState = new MasterDetailsStore();
      }

      return this.localState;
    } else {

      // screen mode on the other hand use the global state
      return this.globalState;
    }
  }

  private get view(): string {
    return this.entityDescriptor.apiEndpoint;
  }

  private get globalState(): MasterDetailsStore {
    const mdState = this.workspace.current.mdState;
    const key = this.stateKey || this.apiEndpoint;
    if (!mdState[key]) {
      mdState[key] = new MasterDetailsStore();
    }

    return mdState[key];
  }

  private incrementRefreshCounter() {
    // Some components wish to track whether the entity was manually refreshed from the details screen
    const model = this.viewModel;
    if (!!model) {
      model.EntityMetadata = model.EntityMetadata || {};
      const meta = model.EntityMetadata;
      meta.$refresh = meta.$refresh || 0;
      meta.$refresh++;
    }
  }

  public canDeactivate(currentUrl?: string, nextUrl?: string): boolean | Observable<boolean> {
    // When the details screen changes its url state, the guard calls canDeactivate, and that
    // normally triggers a confirmation modal if the model is dirty, here we fix this
    let justUrlStateUpdate = false;
    if (!!currentUrl && !!nextUrl) {
      const currentPieces = currentUrl.split('/');
      const nextPieces = nextUrl.split('/');

      const currentIdPiece = currentPieces.pop().split(';')[0];
      const nextIdPiece = nextPieces.pop().split(';')[0];

      const currentPath = currentPieces.join('/');
      const nextPath = currentPieces.join('/');

      justUrlStateUpdate = currentPath === nextPath && currentIdPiece === nextIdPiece;
    }

    if (this.isDirty && !justUrlStateUpdate) {

      // IF there are unsaved changes, prompt the user asking if they would like them discarded
      const modal = this.modalService.open(this.unsavedChangesModal);

      // capture the user's decision in a subject:
      // first action when the user presses one of the two buttons
      // second func is when the user dismisses the modal with x or ESC or clicking the background
      const decision$ = new Subject<boolean>();
      modal.result.then(
        v => { decision$.next(v); decision$.complete(); },
        _ => { decision$.next(false); decision$.complete(); }
      );

      // return the subject that will eventually emit the user's decision
      return decision$;

    } else {

      // IF there are no unsaved changes, the navigation can happily proceed
      return true;
    }
  }

  public displayErrorModal(errorMessage: string) {
    // shows the error message in a dismissable modal
    this._modalErrorMessage = errorMessage;
    this.modalService.open(this.errorModal);
  }

  public displayModalMessage(message: string) {
    this._modalSuccessMessage = message;
    this.modalService.open(this.successModal);
  }

  get viewModel(): EntityForSave {
    // view data is always directly referencing the global workspace
    // this way, un update to a record in the global workspace automatically
    // updates all places where this record is displayed... nifty
    const s = this.state;
    return !!s.detailsId ? this.workspace.current[this.collection][s.detailsId] : null;
  }

  /**
   * Handles server errors for APIs that take an ID array as a parameter,
   * the top 10 errors are simply displayed to the user in a modal
   */
  public handleActionError = (friendlyError: any) => {
    if (this.workspace.current.unauthorized) {
      return;
    }

    const top = 10;
    let errorMessage: string = friendlyError.error;
    if (friendlyError.status === 422) {
      const validationErrors = friendlyError.error as { [key: string]: string[] };
      const keys = Object.keys(validationErrors);
      const tracker: { [error: string]: true } = {};
      for (const key of keys) {
        for (const error of validationErrors[key]) {
          tracker[error] = true; // To show distinct errors
        }
      }
      const errors = Object.keys(tracker);
      const newline = `
`;
      errorMessage = errors.slice(0, top || 10).map(e => ` - ${e}`).join(newline);
      if (errors.length > top) {
        errorMessage += '...'; // To show that's not all
      }
    } else {
      errorMessage = friendlyError.error as string;
    }

    this.displayErrorModal(errorMessage);
  }

  /**
   * Handles 422 Unprocessible Entity errors from a save operation, it distributes the
   * errors on the entity that was saved and all its related weak entities, by parsing
   * the paths and adding the messages in the serverErrors dictionary of the target entity
   */
  private apply422ErrorsToModel(errors: { [path: string]: string[] }) {
    this._unboundServerErrors = [];
    const serverErrors = applyServerErrors([this.activeModel], errors);
    const keys = Object.keys(serverErrors);
    keys.forEach(key => {
      serverErrors[key].forEach(error => {
        this._unboundServerErrors.push(error);
      });
    });
  }

  ////// UI Bindings

  get errorMessage() {
    return this._errorMessage;
  }

  get modalErrorMessage() {
    return this._modalErrorMessage;
  }

  get modalSuccessMessage() {
    return this._modalSuccessMessage;
  }

  get unboundServerErrors(): string[] {
    return this._unboundServerErrors;
  }

  get serverErrors(): { [path: string]: string[] } {
    return this._serverErrors;
  }

  get activeModel(): EntityForSave {
    return this.isEdit ? this._editModel : this.viewModel;
  }

  get extras(): { [key: string]: any } {
    return this.isNew ? {} : this.state.extras;
  }

  get showSpinner(): boolean {
    return this.state.detailsStatus === DetailsStatus.loading;
  }

  get showDocument(): boolean {
    return this.state.detailsStatus === DetailsStatus.loaded ||
      this.state.detailsStatus === DetailsStatus.edit;
  }

  get showSidebar(): boolean {
    return !!this.sidebarTemplate && this.showDocument;
  }

  get showRefresh(): boolean {
    return !this.isEdit;
  }

  get documentLayout(): boolean {
    return this.layout === 'document';
  }

  get fullScreenLayout(): boolean {
    return this.layout === 'full-screen';
  }

  get isNew() {
    return this.idString === 'new';
  }

  get isDirty(): boolean {
    return this.isEdit && this.isDirtyFunc(this._editModel);
  }

  get isEdit(): boolean {
    return this.state.detailsStatus === DetailsStatus.edit;
  }

  get isScreenMode() {
    return this.mode === 'screen';
  }

  get isPopupMode() {
    return this.mode === 'popup';
  }

  get isLight() {
    return this.theme === 'light';
  }

  get isDark() {
    return this.theme === 'dark';
  }

  get showViewToolbar(): boolean {
    return !this.showEditToolbar;
  }

  get showEditToolbar(): boolean {
    return this.state.detailsStatus === DetailsStatus.edit;
  }

  get showErrorMessage(): boolean {
    return this.state.detailsStatus === DetailsStatus.error;
  }

  get showCreate(): boolean {
    return this.showCreateButton;
  }

  get showEdit(): boolean {
    return this.showEditButton;
  }

  get showActions(): boolean {
    return (!!this.actions && this.actions.length > 0) || this.showDelete || this.showClone;
  }

  get showDelete(): boolean {
    return this.showDeleteButton;
  }

  get showClone(): boolean {
    return this.showCreate;
  }

  onRefresh(): void {
    const s = this.state;
    if (s.detailsStatus !== DetailsStatus.loading) {

      // clear the cached item and fetch again
      s.detailsId = null;
      this.fetch();
    }
  }

  onCreate(): void {
    if (!this.canCreate) {
      return;
    }

    this.router.navigate(['..', 'new'], { relativeTo: this.route });
  }

  get canCreatePermissions(): boolean {
    return this.workspace.current.canCreate(this.view);
  }

  private get notArchived(): boolean {
    return !this.entityDescriptor.isArchived;
  }

  get canCreate(): boolean {
    return this.canCreatePermissions && this.notArchived;
  }

  get createTooltip(): string {
    return !this.canCreatePermissions ? this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions') :
      !this.notArchived ? this.translate.instant('Error_DefinitionIsArchived') : '';
  }

  onClone(): void {
    if (!this.canClone) {
      return;
    }

    // this.router.navigate(['..', 'new', { cloneId: this.activeModel.Id }], { relativeTo: this.route });

    const ws = this.workspace;
    ws.cloneId = this.activeModel.Id.toString();
    this.router.navigate(['..', 'new'], { relativeTo: this.route })
      .then(success => {
        if (!success) {
          // delete ws.cloneId;
        }
      })
      .catch(_ => delete ws.cloneId);
  }

  get canClone(): boolean {
    return !!this.activeModel && !!this.activeModel && this.canCreate;
  }

  get cloneTooltip(): string {
    return this.createTooltip;
  }

  onEdit(): void {
    if (!this.canEdit) {
      return;
    }

    if (this.viewModel) {

      // register the current model for dirty checking
      this.registerPristineFunc(this.viewModel);

      // copy the model, and edit the copy (to allow cancelling changes easily)
      this._editModel = JSON.parse(JSON.stringify(this.viewModel));

      // show the edit view
      this.state.detailsStatus = DetailsStatus.edit;
    }
  }

  get canEditPermissions(): boolean {

    const createdById = this.activeModel ? this.activeModel['CreatedById'] : null;
    return this.workspace.current.canUpdate(this.view, createdById);
  }

  get canEdit(): boolean {
    return this.showEdit && this.showDocument && !this.isInactive(this.viewModel) && this.canEditPermissions;
  }

  get editTooltip(): string {
    const error = this.isInactive(this.viewModel);
    return this.canEditPermissions ? (!error ? '' : this.translate.instant(error))
      : this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions');
  }

  onSave(): void {
    // if it's new the user expects a save to happen even if there is no red asterisk
    if (!this.isDirty && !this.isNew) {
      if (this.isPopupMode) {
        // In popup mode, just notify the outside world that a save has happened
        this.saved.emit(this.viewModel.Id);

      } else {
        // since no changes, don't save to the database
        // just go back to view mode
        this.clearErrors();
        clearServerErrors(this._editModel);
        this._editModel = null;
        this.state.detailsStatus = DetailsStatus.loaded;
      }
    } else {

      // clear any errors displayed
      this.clearErrors();
      clearServerErrors(this._editModel);

      // we need the original value for when the save API call returns
      const isNew = this.isNew;

      if (this.savePreprocessing) {
        this.savePreprocessing(this._editModel);
      }

      // prepare the save observable
      const args: SaveArguments = {
        returnEntities: true
      };

      if (this.isPopupMode) {
        // When saving in popup mode, only rely on additionalSelect supplied from the details-picker
        args.select = computeSelectForDetailsPicker(this.entityDescriptor, this.additionalSelect);
      } else {
        args.expand = this.expand;
        args.select = this.select;
      }

      this.crud.save([this._editModel], args, this.extraParams).subscribe(
        (response: EntitiesResponse) => {

          // If we're updating, copy the old entity
          let oldEntity: EntityForSave;
          if (!isNew) {
            oldEntity = JSON.parse(JSON.stringify(this.viewModel));
          }

          // update the workspace with the entity from the server
          const s = this.state;
          s.detailsId = addToWorkspace(response, this.workspace)[0];
          s.extras = response.Extras;
          if (!!this.handleFreshExtras) {
            this.handleFreshExtras(response.Extras);
          }

          this.incrementRefreshCounter();

          // IF it's a new entity add it to the global state, (not the local one even if inside a popup)
          const entityWs = this.workspace.current[response.CollectionName];
          if (isNew) {
            this.globalState.insert([s.detailsId], entityWs);
          } else {
            this.globalState.update(oldEntity, entityWs);
          }

          if (this.isPopupMode) {
            // in popup mode, just notify the outside world that a save has happened
            this.onEdit(); // to replace the edit mode with the one from the server
            this.saved.emit(s.detailsId);

          } else {
            // in screen mode always close the edit view
            s.detailsStatus = DetailsStatus.loaded;

            // remove the local copy the user was editing
            this._editModel = null;

            // IF new and in screen mode, navigate to the Id just returned
            if (this.isNew) {
              this.router.navigate(['..', s.detailsId], { relativeTo: this.route, replaceUrl: true });
            }
          }
        },
        (friendlyError) => {
          if (this.workspace.current.unauthorized) {
            // The user will be redirected away from the tenant anyways
            // and the screen they're taken to will show an appropriate
            // error message, so no need to show a modal here
            return;
          }

          if (friendlyError.status === 422) {
            // This handles 422 ModelState errors
            this.apply422ErrorsToModel(friendlyError.error);
            this._serverErrors = friendlyError.error;
          } else {
            this.displayErrorModal(friendlyError.error);
          }
        }
      );
    }
  }

  onCancel(): void {

    if (this.mode === 'popup') {
      // in popup mode, just notify the outside world that a cancel has happened
      this.cancel.emit();
    } else {

      // in screen mode...
      if (this.isNew) {

        // navigate back to the last screen, this automatically
        // prompts the user for any unsaved changes
        this.location.back();

      } else {
        // prompt the user manually, since the Angular Router isn't involved
        const canCancel = this.canDeactivate();
        if (canCancel instanceof Observable) {
          canCancel.subscribe(can => {
            if (can) {
              this.doCancel();
            }
          });
        } else if (canCancel) {
          this.doCancel();
        }
      }
    }
  }

  doCancel(): void {

    // clear the edit model and error messages
    this._editModel = null;
    this.clearErrors();

    // ... and then close the edit form
    this.state.detailsStatus = DetailsStatus.loaded;
  }

  onDelete(): void {
    // Assuming the entity is not new
    const id = this.viewModel.Id;
    this.crud.deleteId(id).subscribe(
      () => this.onDeleteComplete([id]),
      (friendlyError) => this.handleActionError(friendlyError)
    );
  }

  onDeleteComplete = (ids: (string | number)[]) => {
    // remove from master and total of the global state
    this.globalState.delete(ids, this.workspace.current[this.globalState.collectionName]);

    // after a successful delete navigate back to the master
    this.router.navigate(['..'], { relativeTo: this.route, replaceUrl: true });
  }

  get canDeletePermissions(): boolean {
    const createdById = this.activeModel ? this.activeModel['CreatedById'] : null;
    return this.workspace.current.canDelete(this.view, createdById);
  }

  get canDelete(): boolean {
    return !!this.viewModel && this.canDeletePermissions;
  }

  get deleteTooltip(): string {
    return this.canDeletePermissions ? '' : this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions');
  }

  private navigateTo(id: string | number) {
    const entityDesc = this.entityDescriptor;
    if (!!id) {
      const entity = this.workspace.current[this.collection][id];
      const nextDefId = entity['DefinitionId'];

      // Navigate intelligently depending on whether the next entity has a definition Id
      // This allows navigating through a generic collection of entities of different definition Ids
      const navExtras: NavigationExtras = { relativeTo: this.route };
      if (!!entityDesc.definitionIds && !!nextDefId) {
        this.router.navigate(['../..', nextDefId, id], navExtras);
      } else {
        this.router.navigate(['..', id], navExtras);
      }
    }
  }

  onNext(): void {
    const nextId = this.getNextId();
    this.navigateTo(nextId);
  }

  get canNext(): boolean {
    return !!this.getNextId();
  }

  onPrevious(): void {
    const prevId = this.getPreviousId();
    this.navigateTo(prevId);
  }

  get canPrevious(): boolean {
    return !!this.getPreviousId();
  }

  private getNextId(): number | string {
    const s = this.state;
    const id = this.idString;

    if (!!id) {
      const index = s.masterIds.findIndex(e => e.toString() === id);
      if (index >= 0 && index < s.masterIds.length - 1) {
        const nextIndex = index + 1;
        const nextId = s.masterIds[nextIndex];
        return nextId;
      }
    }

    return null;
  }

  private getPreviousId(): number | string {
    const s = this.state;
    const id = this.idString;

    if (!!id) {
      const index = s.masterIds.findIndex(e => e.toString() === id);
      if (index > 0) {
        const prevIndex = index - 1;
        const prevId = s.masterIds[prevIndex];
        return prevId;
      }
    }

    return null;
  }

  get total(): number {
    return this.state.total;
  }

  get totalDisplay(): string {
    const total = this.total;
    if (total >= MAXIMUM_COUNT) {
      return formatNumber(MAXIMUM_COUNT - 1, 'en-GB') + '+';
    } else {
      return total.toString();
    }
  }

  get order(): number {
    const s = this.state;
    const id = this.idString;

    if (!!id) {
      const index = s.masterIds.findIndex(e => e.toString() === id);
      if (index !== -1) {
        return s.skip + index + 1;
      }
    }

    return null;
  }

  public get showNextAndPrevious(): boolean {
    return !!this.order;
  }

  onDocumentDblClick() {
    if (!this.isEdit && this.canEdit) {
      this.onEdit();
    }
  }

  public get flip() {
    // this is to flip the UI icons in RTL
    return this.workspace.ws.isRtl ? 'horizontal' : null;
  }

  public get actionsDropdownPlacement() {
    return this.workspace.ws.isRtl ? 'bottom-right' : 'bottom-left';
  }

  public canAction(action: DropdownAction): boolean {

    if (!!action.canAction) {
      return action.canAction(this.activeModel);
    } else {
      // true by default
      return true;
    }
  }

  public showAction(action: DropdownAction): boolean {

    if (!!action.showAction) {
      return action.showAction(this.activeModel);
    } else {
      // true by default
      return true;
    }
  }

  public onAction(action: DropdownAction) {
    action.action(this.activeModel);
  }

  public actionTooltip(action: DropdownAction): string {

    if (!!action.actionTooltip) {
      return action.actionTooltip(this.activeModel);
    } else {
      // don't show a tooltip by default
      return '';
    }
  }

  // Printing Stuff
  // tslint:disable:member-ordering

  public get showPrint(): boolean {
    return this.printingTemplates.length > 0;
  }

  private _printingTemplatesDefinitions: DefinitionsForClient;
  private _printingTemplatesSettings: SettingsForClient;
  private _printingTemplatesCollection: string;
  private _printingTemplatesDefinitionId: number;
  private _printingTemplatesResult: PrintingTemplate[];

  public get printingTemplates(): PrintingTemplate[] {
    if (!this.workspace.isApp) { // Printing is not supported in admin atm
      return [];
    }

    const ws = this.workspace.currentTenant;
    const collection = this.collection;
    const defId = this.definitionId;
    if (this._printingTemplatesDefinitions !== ws.definitions ||
      this._printingTemplatesSettings !== ws.settings ||
      this._printingTemplatesCollection !== collection ||
      this._printingTemplatesDefinitionId !== defId) {

      this._printingTemplatesDefinitions = ws.definitions;
      this._printingTemplatesSettings = ws.settings;
      this._printingTemplatesCollection = collection;
      this._printingTemplatesDefinitionId = defId;

      const result: PrintingTemplate[] = [];

      const settings = ws.settings;
      const def = ws.definitions;
      const templates = def.MarkupTemplates
        .filter(e => e.Collection === collection && e.DefinitionId === defId && e.Usage === 'QueryById');

      for (const template of templates) {
        const langCount = (template.SupportsPrimaryLanguage ? 1 : 0)
          + (template.SupportsSecondaryLanguage && !!settings.SecondaryLanguageId ? 1 : 0)
          + (template.SupportsTernaryLanguage && !!settings.TernaryLanguageId ? 1 : 0);

        if (template.SupportsPrimaryLanguage) {
          const postfix = langCount > 1 ? ` (${settings.PrimaryLanguageSymbol})` : ``;
          result.push({
            name: () => `${ws.getMultilingualValueImmediate(template, 'Name')}${postfix}`,
            templateId: template.MarkupTemplateId,
            culture: settings.PrimaryLanguageId
          });
        }

        if (template.SupportsSecondaryLanguage && !!settings.SecondaryLanguageId) {
          const postfix = langCount > 1 ? ` (${settings.SecondaryLanguageSymbol})` : ``;
          result.push({
            name: () => `${ws.getMultilingualValueImmediate(template, 'Name')}${postfix}`,
            templateId: template.MarkupTemplateId,
            culture: settings.SecondaryLanguageId
          });
        }

        if (template.SupportsTernaryLanguage && !!settings.TernaryLanguageId) {
          const postfix = langCount > 1 ? ` (${settings.TernaryLanguageSymbol})` : ``;
          result.push({
            name: () => `${ws.getMultilingualValueImmediate(template, 'Name')}${postfix}`,
            templateId: template.MarkupTemplateId,
            culture: settings.TernaryLanguageId
          });
        }
      }

      this._printingTemplatesResult = result;
    }

    return this._printingTemplatesResult;
  }

  private printingSubscription: Subscription;

  public onPrint(template: PrintingTemplate): void {
    const entity = this.activeModel;
    if (!entity || !entity.Id || !template) {
      return;
    }

    // Cancel any existing printing query
    if (!!this.printingSubscription) {
      this.printingSubscription.unsubscribe();
    }

    // New printing query
    this.printingSubscription = this.crud
      .printById(entity.Id, template.templateId, { culture: template.culture })
      .pipe(
        tap(blob => {
          this.printingSubscription = null;
          printBlob(blob);
        }),
        catchError(friendlyError => {
          this.printingSubscription = null;
          this.displayErrorModal(friendlyError.error);
          return of();
        }),
        finalize(() => {
          this.printingSubscription = null;
        })
      ).subscribe();
  }

  public get isPrinting(): boolean {
    return !!this.printingSubscription;
  }
}

export interface PrintingTemplate {
  name: () => string;
  templateId: number;
  culture: string;
}

export function applyServerErrors(
  entity: EntityForSave | EntityForSave[],
  errors: { [key: string]: string[] }): { [key: string]: string[] } {

  if (!entity) {
    return;
  }

  if (!errors) {
    return;
  }

  const paths = Object.keys(errors);
  const leftovers: { [key: string]: string[] } = {};
  for (const p of paths) {
    let path = p;

    // This path targets an entity as a whole, not just one of its properties, we
    // add them to a special indexer "_Self" in that entity's serverErrors collection
    if (p.trim().endsWith(']')) {
      path += '._Self';
    }

    const steps = path.split('.');
    let current = entity;

    for (let s = 0; s < steps.length - 1; s++) {
      const step = steps[s];
      if (step.endsWith(']')) {
        // handle array
        const parts = step.substring(0, step.length - 1).split('[');
        const arrayPart = parts[0];
        const indexPart = +parts[1];

        if (isNaN(indexPart)) {
          // ignore a malformed error path (a later step will add the errors to leftovers)
          console.error(`Badly formatted server error path '${p}'`);
          current = null;
          break;
        }

        if (!arrayPart) {
          // sometimes the entire entity is an array
          current = current[indexPart];
        } else {
          current = current[arrayPart][indexPart];
        }

      } else {
        // handle non-array
        current = current[step];
      }

      if (!current) {
        break;
      }
    }

    if (!!current) {
      const currentEntity = current as EntityForSave; // This is a lie to keep typescript happy, sometimes it's an array

      if (!currentEntity.serverErrors) {
        currentEntity.serverErrors = {};
      }

      const lastStep = steps[steps.length - 1];
      currentEntity.serverErrors[lastStep] = errors[p];

    } else {
      leftovers[path] = errors[path];
    }
  }

  return leftovers;
}

export function clearServerErrors(entity: EntityForSave | EntityForSave[]): void {
  if (!entity) {
    // nothing to clear
    return;
  }

  // if errors exist remove them (they can potentially exist even on arrays)
  if (!!(entity as EntityForSave).serverErrors) {
    delete (entity as EntityForSave).serverErrors;
  }

  // if the property is an array, recursively clear the errors from all the items
  if (Array.isArray(entity)) {
    // loop over the array items and clear them
    for (const item of entity) {
      clearServerErrors(item);
    }
  } else if (!!entity.Id || entity.Id === 0 || entity.Id === null) { // TODO: Review this
    // if the property is a DTO loop over the navigation properties and recursively clear their errors
    const props = Object.keys(entity);
    for (const prop of props) {
      const item = entity[prop];
      clearServerErrors(item);
    }
  }
}
