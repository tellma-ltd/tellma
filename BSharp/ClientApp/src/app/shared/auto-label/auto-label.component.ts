import { Component, OnInit, Input } from '@angular/core';
import { WorkspaceService } from '~/app/data/workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { propDescriptorImpl, dtoDescriptorImpl } from '~/app/data/dto/metadata';

@Component({
  selector: 'b-auto-label',
  templateUrl: './auto-label.component.html',
  styleUrls: ['./auto-label.component.scss']
})
export class AutoLabelComponent implements OnInit {

  // This component automatically displays the property label from its metadata

  @Input()
  baseCollection: string;

  @Input()
  path: string;

  @Input()
  subtype: string;

  _previousPath: string;
  _pathArray: string[];

  constructor(private ws: WorkspaceService, private translate: TranslateService) { }

  ngOnInit() {
  }

  get pathArray(): string[] {
    if (this.path !== this._previousPath || !this._pathArray) {
      this._previousPath = this.path;
      this._pathArray = (this.path || '').split('/').map(e => e.trim()).filter(e => !!e);
    }

    return this._pathArray;
  }

  get label(): string {
    try {
      const labels = [];
      propDescriptorImpl(this.pathArray, this.baseCollection, this.subtype, this.ws.current, this.translate, labels);
      return labels.join(' / ');
    } catch (ex) {
      console.error(ex.message);
      return `(${this.translate.instant('Error')})`;
    }
  }

}
