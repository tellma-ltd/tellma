import { Component } from '@angular/core';
import { MasterBaseComponent } from '~/app/shared/master-base/master-base.component';
import { WorkspaceService } from '~/app/data/workspace.service';
import { formatSerial, metadata_Document } from '~/app/data/entities/document';
import { TranslateService } from '@ngx-translate/core';
import { SerialPropDescriptor } from '~/app/data/entities/base/metadata';

@Component({
  selector: 't-outbox',
  templateUrl: './outbox.component.html',
  styles: []
})
export class OutboxComponent extends MasterBaseComponent {

  constructor(private workspace: WorkspaceService, private translate: TranslateService) {
    super();
  }

  public get c() {
    return this.ws.OutboxRecord;
  }

  public get ws() {
    return this.workspace.currentTenant;
  }

  public formatSerial(id: number) {
    const outboxRecord = this.c[id];
    const doc = this.ws.Document[outboxRecord.DocumentId];
    const defId = doc.DefinitionId;
    const meta = metadata_Document(this.workspace, this.translate, defId);
    const propDesc = meta.properties.SerialNumber as SerialPropDescriptor;
    const value = doc.SerialNumber;

    return formatSerial(value, propDesc.prefix, propDesc.codeWidth);
  }
}
