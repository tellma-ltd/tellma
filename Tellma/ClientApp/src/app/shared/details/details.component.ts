// tslint:disable:no-string-literal
import { Location } from '@angular/common';
import {
  Component, EventEmitter, Input, OnDestroy, OnInit, TemplateRef,
  ViewChild, Output, SimpleChanges, OnChanges, HostListener
} from '@angular/core';
import { ActivatedRoute, ParamMap, Router, Params } from '@angular/router';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { TranslateService } from '@ngx-translate/core';
import { catchError, switchMap, tap } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { EntityForSave } from '~/app/data/entities/base/entity-for-save';
import { GetByIdResponse } from '~/app/data/dto/get-by-id-response';
import { EntitiesResponse } from '~/app/data/dto/get-response';
import { addSingleToWorkspace, addToWorkspace, computeSelectForDetailsPicker } from '~/app/data/util';
import { DetailsStatus, MasterDetailsStore, WorkspaceService } from '~/app/data/workspace.service';
import { ICanDeactivate } from '~/app/data/unsaved-changes.guard';
import { Subject, Observable, of, Subscription } from 'rxjs';
import { EntityDescriptor, metadata } from '~/app/data/entities/base/metadata';
import { environment } from '~/environments/environment';

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
export class DetailsComponent implements OnInit, OnDestroy, OnChanges, ICanDeactivate {

  @Input()
  expand: string;

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
  savePreprocessing: (entity: EntityForSave) => void;

  @Input()
  actions: DropdownAction[] = [];

  @Input() // popup: only the title and the document are visible
  mode: 'popup' | 'screen' = 'screen';

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
  definition: string;

  @Input()
  idString: string;

  @Input()
  additionalSelect: string; // Loaded in popup mode

  @Input()
  extraParams: { [key: string]: any };

  @Input()
  handleFreshExtras: (extras: { [key: string]: any }) => void;

  @Input()
  theme: 'light' | 'dark' = 'light';

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

  private paramMapSubscription: Subscription;
  private _editModel: EntityForSave;
  private notifyFetch$: Subject<void>;
  private notifyDestruct$ = new Subject<void>();
  private localState = new MasterDetailsStore();  // Used in popup mode
  private _errorMessage: string; // in the document area itself
  private _modalErrorMessage: string; // in the modal
  private _modalSuccessMessage: string; // in the modal
  private _unboundServerErrors: string[]; // in the modal
  private _pristineModelJson: string;
  private crud = this.api.crudFactory(this.apiEndpoint, this.notifyDestruct$); // Just for intellisense

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
  doSomething($event: BeforeUnloadEvent) {
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

    // as if the screen is opened a new
    this.localState = new MasterDetailsStore();
    this._errorMessage = null;
    this._modalErrorMessage = null;
    this._modalSuccessMessage = null;
    this._unboundServerErrors = [];
    this.crud = this.api.crudFactory(this.apiEndpoint, this.notifyDestruct$);
    this.registerPristineFunc(null);

    this.paramMapSubscription = this.route.paramMap.subscribe((params: ParamMap) => {
      // the id parameter from the URI is only avaialble in screen mode
      // when it changes set idString which triggers a new refresh
      if (this.isScreenMode && params.has('id')) {
        // even though this might get set in a popup because the parent has an id param,
        // it gets wiped out afterwards when angular initializes the input properties
        const newId = params.get('id');
        if (this.idString !== newId) {
          const notFirstTime = !!this.idString;
          this.idString = newId;

          // Call this manually since Angular won't call ngOnChanges automatically
          if (notFirstTime) {
            this.newScreen();
          }
        }
      }
    });

    // Fetch the data of the screen based on apiEndpoint and idString
    this.fetch();
  }

  ngOnDestroy() {
    // cancel any backend operations
    this.notifyDestruct$.next();

    if (!!this.paramMapSubscription) {
      this.paramMapSubscription.unsubscribe();
    }
  }

  ngOnChanges(changes: SimpleChanges) {

    // the combination of these properties defines a whole new screen from the POV of the user
    // when either of these properties change it is equivalent to a screen closing and
    // and another screen opening even though Angular may reuse the same
    // component and never call ngOnDestroy and ngOnInit. So we call them
    // manually here if this is not the first time these properties are set
    // to simulate a screen closing and opening again
    const screenDefProperties = [changes.collection, changes.apiEndpoint, changes.idString];
    const screenDefChanges = screenDefProperties.some(prop => !!prop && !prop.isFirstChange());
    if (screenDefChanges) {
      this.newScreen();
    }
  }

  private newScreen(): void {
    // This method simulates navigating away from the screen and then navigating back
    this.ngOnDestroy();
    this.ngOnInit();
  }

  get entityDescriptor(): EntityDescriptor {
    const coll = this.collection;
    return !!coll ? metadata[coll](this.workspace, this.translate, this.definition) : null;
  }

  get apiEndpoint(): string {
    const meta = this.entityDescriptor;
    return !!meta ? meta.apiEndpoint : null;
  }

  private fetch() {
    this.notifyFetch$.next(null);
  }

  private get cloneId(): string {
    return this.route.snapshot.paramMap.get('cloneId');
  }

  private doFetch(): Observable<void> {
    // clear the errors before refreshing
    this.clearErrors();

    // grab the configured state
    const s = this.state;

    // calculate some logical values
    const cloneId = this.cloneId;
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
        return this.crud.getById(id, { expand: this.expand }, this.extraParams).pipe(
          tap((response: GetByIdResponse) => {

            // add the server item to the workspace
            this.state.detailsId = addSingleToWorkspace(response, this.workspace);
            this.state.extras = response.Extras;
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
              this.state.detailsStatus = DetailsStatus.loaded;
              if (this.isPopupMode) {
                this.onEdit();
              }
            }
          }),
          catchError((friendlyError) => {
            this._errorMessage = friendlyError.error;
            this.state.detailsStatus = DetailsStatus.error;
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
    if (!this.workspace.current.mdState[this.apiEndpoint]) {
      // if (this.mode === 'screen') {
      //   this.workspace.current.mdState = {}; // This forces any other master/details screen to refresh
      // }

      this.workspace.current.mdState[this.apiEndpoint] = new MasterDetailsStore();
    }

    return this.workspace.current.mdState[this.apiEndpoint];
  }

  public canDeactivate(): boolean | Observable<boolean> {
    if (this.isDirty) {

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

  public displayModalError(errorMessage: string) {
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

  public handleActionError = (friendlyError: any) => {

    // This handles 422 ModelState errors
    if (friendlyError.status === 422) {
      this._unboundServerErrors = [];
      const serverErrors = applyServerErrors([this.activeModel], friendlyError.error);
      const keys = Object.keys(serverErrors);
      keys.forEach(key => {
        serverErrors[key].forEach(error => {
          this._unboundServerErrors.push(error);
        });
      });

    } else {
      this.displayModalError(friendlyError.error);
    }
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

  get activeModel(): EntityForSave {
    return this.isEdit ? this._editModel : this.viewModel;
  }

  get extras(): { [key: string]: any } {
    return this.state.extras;
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

  get canCreate(): boolean {
    return this.canCreatePermissions;
  }

  get createTooltip(): string {
    return this.canCreatePermissions ? '' : this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions');
  }

  onClone(): void {
    if (!this.canClone) {
      return;
    }

    const params: Params = {
      cloneId: this.activeModel.Id.toString()
    };

    this.router.navigate(['..', 'new', params], { relativeTo: this.route });
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
        this.saved.emit(this._editModel.Id);

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
      const select = this.isPopupMode ? computeSelectForDetailsPicker(this.entityDescriptor, this.additionalSelect) : null;
      this.crud.save([this._editModel], { select, expand: this.expand, returnEntities: true }, this.extraParams).subscribe(
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
        (friendlyError) => this.handleActionError(friendlyError)
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
      () => {
        // remove from master and total of the global state
        this.globalState.delete([id], this.workspace.current[this.globalState.collectionName]);

        // after a successful delete navigate back to the master
        this.router.navigate(['..'], { relativeTo: this.route });
      },
      (friendlyError) => this.handleActionError(friendlyError)
    );
  }

  get canDeletePermissions(): boolean {
    return this.canEditPermissions;
  }

  get canDelete(): boolean {
    return !!this.viewModel && this.canDeletePermissions;
  }

  get deleteTooltip(): string {
    return this.canDeletePermissions ? '' : this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions');
  }

  onNext(): void {
    this.router.navigate(['..', this.getNextId()], { relativeTo: this.route });
  }

  get canNext(): boolean {

    return !!this.getNextId();
  }

  onPrevious(): void {
    this.router.navigate(['..', this.getPreviousId()], { relativeTo: this.route });
  }

  get canPrevious(): boolean {
    return !!this.getPreviousId();
  }

  private getNextId(): number | string {
    const s = this.state;
    const id = this.idString;

    if (!!id) {
      const index = s.masterIds.findIndex(e => e.toString() === id);
      if (index !== -1 && index !== s.masterIds.length - 1) {
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
  for (const path of paths) {
    const steps = path.split('.');

    let current = entity;
    for (let s = 0; s < steps.length - 1; s++) {
      const step = steps[s];
      if (!current) {
        // Do nothing

      } else if (step.endsWith(']')) {
        // handle array
        const parts = step.substring(0, step.length - 1).split('[');
        const arrayPart = parts[0];
        const indexPart = +parts[1];

        if (isNaN(indexPart)) {
          // ignore a malformed error path
          leftovers[path] = errors[path];
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
    }

    if (!!current) {
      const currentEntity = current as EntityForSave;
      // we have to use the property indexer here otherwise typescript will complain
      // in reality we are setting serverErrors even if the target is an array, javascript
      // allows that but typescript doesn't
      if (!currentEntity.serverErrors) {
        currentEntity.serverErrors = {};
      }

      const lastStep = steps[steps.length - 1];
      currentEntity.serverErrors[lastStep] = errors[path];

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
  } else if (!!entity.Id || entity.Id === null) {
    // if the property is a DTO loop over the navigation properties and recursively clear their errors
    const props = Object.keys(entity);
    for (const prop of props) {
      const item = entity[prop];
      clearServerErrors(item);
    }
  }
}
