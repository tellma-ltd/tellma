import { Component, OnInit, Input, ChangeDetectionStrategy, ChangeDetectorRef, OnChanges, SimpleChanges, OnDestroy } from '@angular/core';
import { WorkspaceService } from '~/app/data/workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { metadata, PropDescriptor } from '~/app/data/entities/base/metadata';
import { Subscription } from 'rxjs';

@Component({
  selector: 't-auto-label',
  templateUrl: './auto-label.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AutoLabelComponent implements OnInit, OnChanges, OnDestroy {

  // This component automatically displays the property label from its metadata

  @Input()
  collection: string;

  @Input()
  definitionId: number;

  @Input()
  path: string;

  @Input()
  useAlignment: boolean;

  _subscription: Subscription;
  _label: string;
  _alignment: 'left' | 'right' | 'center';
  _errorMessage: string;

  constructor(private workspace: WorkspaceService, private translate: TranslateService, private cdr: ChangeDetectorRef) { }

  ngOnInit() {
    this._subscription = this.workspace.stateChanged$.subscribe({
      next: () => {
        this.recompute();
        this.cdr.markForCheck();
      }
    });
  }

  ngOnChanges(_: SimpleChanges) {
    this.recompute();
  }

  ngOnDestroy() {
    if (!!this._subscription) {
      this._subscription.unsubscribe();
    }
  }

  private metadataFactory(collection: string) {
    const factory = metadata[collection]; // metadata factory for User
    if (!factory) {
      throw new Error(`The collection ${collection} does not exist`);
    }

    return factory;
  }

  private recompute() {

    // clear previous values
    this._label = null;
    this._errorMessage = null;

    try {
      if (!this.collection) {
        throw new Error(`The collection is not specified`);
      }

      const pathArray = (this.path || '').split('/').map(e => e.trim()).filter(e => !!e);

      if (pathArray.length === 0) {
        this._label = this.translate.instant('DisplayName');

      } else {

        const labelArray = [];

        let currentEntityDesc = this.metadataFactory(this.collection)(this.workspace, this.translate, this.definitionId);
        let currentCollection = this.collection;
        let currentDefinition = this.definitionId;
        let currentPropDesc: PropDescriptor;

        for (let i = 0; i < pathArray.length; i++) {
          const step = pathArray[i];

          currentPropDesc = currentEntityDesc.properties[step];

          if (!currentPropDesc) {
            throw new Error(`'${step}' does not exist on '${currentCollection || ''}', definition:'${currentDefinition || ''}'`);

          } else if (currentPropDesc.datatype === 'entity') {

            currentCollection = currentPropDesc.control;
            currentDefinition = currentPropDesc.definitionId;
            currentEntityDesc = this.metadataFactory(currentCollection)(this.workspace, this.translate, currentDefinition);

          } else if (i !== pathArray.length - 1) {
            throw new Error(
              `'${step}' is not a navigation property on '${currentCollection || ''}', definition:'${currentDefinition || ''}'`);
          }

          labelArray.push(currentPropDesc.label());
        }

        this._alignment = !!currentPropDesc ? currentPropDesc.alignment : null;
        this._label = labelArray.join(' / ');
      }
    } catch (ex) {

      this._label = `(${this.translate.instant('Error')})`;
      this._errorMessage = ex.message;
    }
  }


  // UI Bindings

  get label(): string {
    return this._label;
  }

  get alignment(): string {
    return this.useAlignment ? this._alignment : null;
  }

  get errorMessage(): string {
    return this._errorMessage || '';
  }

}
