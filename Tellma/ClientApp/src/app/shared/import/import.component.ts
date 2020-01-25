import { Component, OnInit, Input, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { TemplateArguments_format } from '~/app/data/dto/template-arguments';
import { ApiService } from '~/app/data/api.service';
import { Subject } from 'rxjs';
import { downloadBlob } from '~/app/data/util';
import { TranslateService } from '@ngx-translate/core';
import { ImportArguments_Mode } from '~/app/data/dto/import-arguments';
import { WorkspaceService } from '~/app/data/workspace.service';
import { ImportResult } from '~/app/data/dto/import-result';
import { SelectorChoice } from '../selector/selector.component';

@Component({
  selector: 't-import',
  templateUrl: './import.component.html'
})
export class ImportComponent implements OnInit, OnDestroy {


  @Input()
  apiEndpoint: string;

  @Input()
  title: string;

  @Input()
  templateFileName: string;

  public format: 'xlsx' | 'csv' = 'xlsx';
  public mode: 'Insert' | 'Update' | 'Merge' = 'Insert';
  public showSpinner: boolean;
  public downloadErrorMessage: string;
  public importErrorMessage: string;
  public importValidationErrors: string[] = [];
  public importResult: ImportResult;

  private _modeChoices: SelectorChoice[];
  private _formatChoices: SelectorChoice[];
  private notifyDestruct$ = new Subject<void>();
  private crud = this.api.crudFactory(this.apiEndpoint, this.notifyDestruct$); // Only for intellisense

  constructor(
    private api: ApiService, private workspace: WorkspaceService, private router: Router,
    private route: ActivatedRoute, private translate: TranslateService) { }

  ngOnInit() {
    this.crud = this.api.crudFactory(this.apiEndpoint, this.notifyDestruct$);
  }

  ngOnDestroy(): void {
    this.notifyDestruct$.next();
  }

  onDownloadTemplate() {
    this.showSpinner = true;
    const format = this.format;
    this.downloadErrorMessage = null;
    this.crud.template({ format }).subscribe(
      (blob: Blob) => {
        this.showSpinner = false;
        const fileName = `${this.templateFileName || this.translate.instant('Template')} ${new Date().toDateString()}.${format}`;
        downloadBlob(blob, fileName);
      },
      (friendlyError: any) => {
        this.showSpinner = false;
        this.downloadErrorMessage = friendlyError.error;
      }
    );
  }

  onImport(input: any) {
    const files = input.files;

    if (files.length === 0) {
      return;
    }

    // Clear any displayed errors
    this.importErrorMessage = null;
    this.importValidationErrors = [];
    this.importResult = null;

    this.crud.import({ mode: this.mode }, files).subscribe(
      (importResult: ImportResult) => {
        this.showSpinner = false;

         // this forces a refresh when the user navigates to the master screen
        this.workspace.current.mdState[this.apiEndpoint] = null;

        // TODO Show the result to the user
        this.importResult = importResult;
      },
      (friendlyError: any) => {
        this.showSpinner = false;
        if (friendlyError.status === 422) {
          // 422 represents a response of validation errors
          // The payload is an object where each property is a list of error messages
          // the code below unpacks that into a flat list of errors to show the user
          const keys = Object.keys(friendlyError.error);
          let errorList = [];

          for (const key of keys) {
            const list = friendlyError.error[key];
            errorList = errorList.concat(list);
          }

          this.importValidationErrors = errorList;
          this.importErrorMessage = this.translate.instant('ImportedFileDidNotPassValidation');

        } else {

          this.importErrorMessage = friendlyError.error;
        }
      }
    );
  }

  onCancel() {
    this.router.navigate(['..'], { relativeTo: this.route });
  }

  get showDownloadErrorMessage() {
    return !!this.downloadErrorMessage;
  }

  get showImportErrorMessage() {
    return !!this.importErrorMessage;
  }

  get showSuccessMessage() {
    return !!this.importResult;
  }

  get formatChoices(): SelectorChoice[] {

    if (!this._formatChoices) {
      this._formatChoices = Object.keys(TemplateArguments_format)
        .map(key => ({ name: () => this.translate.instant(TemplateArguments_format[key]), value: key }));
    }

    return this._formatChoices;
  }

  get modeChoices(): SelectorChoice[] {

    if (!this._modeChoices) {
      this._modeChoices = Object.keys(ImportArguments_Mode)
        .map(key => ({ name: () => this.translate.instant(ImportArguments_Mode[key]), value: key }));
    }

    return this._modeChoices;
  }

}
