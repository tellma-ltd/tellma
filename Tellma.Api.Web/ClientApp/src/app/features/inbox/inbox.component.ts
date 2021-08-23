import { Component } from '@angular/core';
import { MasterBaseComponent } from '~/app/shared/master-base/master-base.component';
import { WorkspaceService } from '~/app/data/workspace.service';
import { formatSerial, metadata_Document } from '~/app/data/entities/document';
import { TranslateService } from '@ngx-translate/core';
import { SerialPropDescriptor } from '~/app/data/entities/base/metadata';

@Component({
  selector: 't-inbox',
  templateUrl: './inbox.component.html',
  styles: []
})
export class InboxComponent extends MasterBaseComponent {

  constructor(private workspace: WorkspaceService, private translate: TranslateService) {
    super();
  }

  public get c() {
    return this.ws.InboxRecord;
  }

  public get ws() {
    return this.workspace.currentTenant;
  }

  public formatSerial(id: number) {
    const doc = this.ws.Document[id];
    const defId = doc.DefinitionId;
    const meta = metadata_Document(this.workspace, this.translate, defId);
    const propDesc = meta.properties.SerialNumber as SerialPropDescriptor;
    const value = doc.SerialNumber;

    return formatSerial(value, propDesc.prefix, propDesc.codeWidth);
  }

  public canRead = () => true;
}
