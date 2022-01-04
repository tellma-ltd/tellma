// tslint:disable:member-ordering
import { Component } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { MessagePreview } from '~/app/data/dto/message-command-preview';
import { ChoicePropDescriptor } from '~/app/data/entities/base/metadata';
import { metadata_Message, MessageState, MessageForQuery } from '~/app/data/entities/message';
import { WorkspaceService } from '~/app/data/workspace.service';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';


@Component({
  selector: 't-messages-details',
  templateUrl: './messages-details.component.html',
  styles: []
})
export class MessagesDetailsComponent extends DetailsBaseComponent {

  public expand = '';
  private _stateDesc: ChoicePropDescriptor;

  constructor(private workspace: WorkspaceService, private translate: TranslateService) {
    super();
  }

  public get ws() {
    return this.workspace.currentTenant;
  }

  public stateDisplay(state: MessageState) {
    if (!this._stateDesc) {
      const meta = metadata_Message(null, this.translate);
      this._stateDesc = meta.properties.State as ChoicePropDescriptor;
    }

    return this._stateDesc.format(state);
  }

  public stateColor(state: MessageState) {
    if (!this._stateDesc) {
      const meta = metadata_Message(null, this.translate);
      this._stateDesc = meta.properties.State as ChoicePropDescriptor;
    }

    return this._stateDesc.color(state);
  }

  private _truncateStr: string;
  private _truncateResult: string;

  public truncate(str: string): string {
    str = str || '';
    const max = 50;
    if (this._truncateStr !== str) {
      this._truncateStr = str;
      this._truncateResult = str.substring(0, max) + (str.length > max ? '...' : '');
    }

    return this._truncateResult;
  }
}
