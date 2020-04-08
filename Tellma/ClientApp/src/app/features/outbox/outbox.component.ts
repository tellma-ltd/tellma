import { Component } from '@angular/core';
import { MasterBaseComponent } from '~/app/shared/master-base/master-base.component';
import { WorkspaceService } from '~/app/data/workspace.service';
import { metadata_Document } from '~/app/data/entities/document';
import { TranslateService } from '@ngx-translate/core';
import { SerialPropDescriptor } from '~/app/data/entities/base/metadata';
import { Router, ActivatedRoute } from '@angular/router';

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

    return propDesc.format(doc.SerialNumber);
  }

  public customChoiceHandler = (id: number | string, router: Router, route: ActivatedRoute, _: string) => {
    const outboxRecord = this.c[id];
    const docId = outboxRecord.DocumentId;
    const definitionId = this.workspace.currentTenant.Document[docId].DefinitionId;
    const extras = { state_key: 'from_outbox' };
    router.navigate(['../documents', definitionId, docId, extras], { relativeTo: route });
  }
}
