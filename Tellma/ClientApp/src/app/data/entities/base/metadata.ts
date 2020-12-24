import { metadata_Unit } from '../unit';
import { WorkspaceService } from '../../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { metadata_User as metadata_User } from '../user';
import { metadata_Role } from '../role';
import { metadata_Relation } from '../relation';
import { metadata_Lookup } from '../lookup';
import { metadata_Currency } from '../currency';
import { metadata_Resource } from '../resource';
import { metadata_VoucherBooklet } from '../_temp';
import { metadata_AccountClassification } from '../account-classification';
import { metadata_AccountType } from '../account-type';
import { metadata_Account } from '../account';
import { metadata_ReportDefinition } from '../report-definition';
import { metadata_Center } from '../center';
import { metadata_EntryType } from '../entry-type';
import { metadata_Document } from '../document';
import { metadata_SummaryEntry } from '../summary-entry';
import { metadata_DetailsEntry } from '../details-entry';
import { metadata_LineForQuery } from '../line';
import { metadata_AdminUser } from '../admin-user';
import { metadata_IdentityServerUser } from '../identity-server-user';
import { metadata_ExchangeRate } from '../exchange-rate';
import { metadata_InboxRecord } from '../inbox-record';
import { metadata_OutboxRecord } from '../outbox-record';
import { metadata_IfrsConcept } from '../ifrs-concept';
import { metadata_MarkupTemplate } from '../markup-template';
import { SelectorChoice } from '~/app/shared/selector/selector.component';
import { Entity } from './entity';
import { metadata_Agent } from '../agent';
import { metadata_RelationDefinition } from '../relation-definition';
import { metadata_ResourceDefinition } from '../resource-definition';
import { metadata_LookupDefinition } from '../lookup-definition';
import { Router } from '@angular/router';
import { metadata_Custody } from '../custody';
import { metadata_CustodyDefinition } from '../custody-definition';
import { metadata_LineDefinition } from '../line-definition';
import { metadata_DocumentDefinition } from '../document-definition';
import { metadata_Email } from '../email';
import { metadata_SmsMessage } from '../sms-message';

export const metadata: {
    [collection: string]: (
        ws: WorkspaceService,
        trx: TranslateService,
        definitionId?: number) => EntityDescriptor
} = {
    // Application
    Unit: metadata_Unit,
    User: metadata_User,
    Agent: metadata_Agent,
    Relation: metadata_Relation,
    Custody: metadata_Custody,
    Role: metadata_Role,
    Lookup: metadata_Lookup,
    Currency: metadata_Currency,
    Resource: metadata_Resource,
    AccountClassification: metadata_AccountClassification,
    IfrsConcept: metadata_IfrsConcept,
    AccountType: metadata_AccountType,
    Account: metadata_Account,
    ReportDefinition: metadata_ReportDefinition,
    Center: metadata_Center,
    EntryType: metadata_EntryType,
    Document: metadata_Document,
    LineForQuery: metadata_LineForQuery,
    ExchangeRate: metadata_ExchangeRate,
    DetailsEntry: metadata_DetailsEntry,
    SummaryEntry: metadata_SummaryEntry,
    MarkupTemplate: metadata_MarkupTemplate,
    InboxRecord: metadata_InboxRecord,
    OutboxRecord: metadata_OutboxRecord,
    RelationDefinition: metadata_RelationDefinition,
    CustodyDefinition: metadata_CustodyDefinition,
    ResourceDefinition: metadata_ResourceDefinition,
    LookupDefinition: metadata_LookupDefinition,
    LineDefinition: metadata_LineDefinition,
    DocumentDefinition: metadata_DocumentDefinition,
    EmailForQuery: metadata_Email,
    SmsMessageForQuery: metadata_SmsMessage,

    // Admin
    AdminUser: metadata_AdminUser,
    IdentityServerUser: metadata_IdentityServerUser,

    // Temp
    VoucherBooklet: metadata_VoucherBooklet,
};

/**
 * Array of all possible DataType values
 */
export const datatypesArray: DataType[] =
    ['string', 'integral', 'decimal', 'date', 'datetime', 'datetimeoffset', 'boolean', 'entity'];

/**
 * Array of all possible SimpleControl values
 */
export const simpleControlsArray: SimpleControl[] =
    ['text', 'serial', 'choice', 'number', 'percent', 'date', 'datetime', 'boolean'];

/**
 * Names of all datatypes
 */
export type DataType = 'string' | 'integral' | 'decimal' | 'date' | 'datetime' | 'datetimeoffset' | 'boolean' | 'entity';

/**
 * Combines simple and entity controls
 */
export type Control = SimpleControl | Collection;

/**
 * Names of simple (scalar) editors
 */
export type SimpleControl = 'text' | 'serial' | 'choice' | 'number' | 'percent' | 'date' | 'datetime' | 'boolean';

/**
 * Names of collections that support the standard tellma API (Get Fact)
 */
export type Collection =
    'Unit' |
    'User' |
    'Agent' |
    'Relation' |
    'Custody' |
    'Lookup' |
    'Currency' |
    'Resource' |
    'AccountClassification' |
    'IfrsConcept' |
    'AccountType' |
    'Account' |
    'ReportDefinition' |
    'Center' |
    'EntryType' |
    'Document' |
    'LineForQuery' |
    'ExchangeRate' |
    'DetailsEntry' |
    'SummaryEntry' |
    'MarkupTemplate' |
    'InboxRecord' |
    'OutboxRecord' |
    'RelationDefinition' |
    'CustodyDefinition' |
    'ResourceDefinition' |
    'LookupDefinition' |
    'LineDefinition' |
    'DocumentDefinition' |
    'EmailForQuery' |
    'SmsMessageForQuery' |
    'AdminUser' |
    'IdentityServerUser' |
    'VoucherBooklet';

let _collectionsSingular: SelectorChoice[];
let _collectionsPlural: SelectorChoice[];

export function collectionsWithEndpoint(ws: WorkspaceService, trx: TranslateService, singular = false): SelectorChoice[] {
    if (!_collectionsSingular) {
        const source = Object.keys(metadata)
            .filter(key => {
                const meta = metadata[key](ws, trx, null);
                return !!meta && !meta.isAdmin && !!meta.apiEndpoint;
            });

        _collectionsSingular = source
            .map(key => ({
                value: key,
                name: metadata[key](ws, trx, null).titleSingular
            }))
            .sort((a, b) => a.name() < b.name() ? - 1 : a.name() > b.name() ? 1 : 0);

        _collectionsPlural = source
            .map(key => ({
                value: key,
                name: metadata[key](ws, trx, null).titlePlural
            }))
            .sort((a, b) => a.name() < b.name() ? - 1 : a.name() > b.name() ? 1 : 0);
    }

    return singular ? _collectionsSingular : _collectionsPlural;
}

let _simpleControls: SelectorChoice[];

export function simpleControls(trx: TranslateService): SelectorChoice[] {
    if (!_simpleControls) {
        _simpleControls = simpleControlsArray
            .map(c => ({ value: c, name: () => trx.instant('Control_' + c) }));
    }

    return _simpleControls;
}

export interface EntityDescriptor {

    /**
     * The collection Id of this entity descriptor, (aka the table)
     */
    collection: string;

    /**
     * The definition Id of this entity descriptor
     */
    definitionId?: number;

    /**
     * Collections that have definitions list the definitions here
     */
    definitionIds?: number[];

    /**
     * The plural name of the entity (e.g. Relations).
     */
    titlePlural: () => string;

    /**
     * The singular name of the entity (e.g. Relation).
     */
    titleSingular: () => string;

    /**
     * The Entity properties that need to be selected from the server for the format function to succeed.
     */
    select: string[];

    /**
     * When ordering by a nav property of this Entity type, this value specifies the OData 'orderby' value to use.
     */
    orderby: () => string[];

    /**
     * The filter that filters away inactive/canceled/archived items by default
     */
    inactiveFilter: string;

    /**
     * The label on the button that eliminates 'inactiveFilter' from the query
     */
    includeInactveLabel?: () => string;

    /**
     * The server endpoint from which to retrieve Entities of this type, after the 'https://web.tellma.com/api/' part.
     */
    apiEndpoint?: string;

    /**
     * Any built-in parameters that are needed for the query
     */
    parameters?: ParameterDescriptor[];

    /**
     * The url of the screen that displays this type after the 'https://web.tellma.com/app/101/' part.
     */
    masterScreenUrl?: string;

    /**
     * The select required for navigateToDetails to succeed
     */
    navigateToDetailsSelect?: string[];

    /**
     * Navigates to the details screen representing the entity with a certain Id, this is a more powerful version of screenUrl
     */
    navigateToDetails?: (entity: Entity, router: Router, statekey?: string) => void;

    /**
     * A function that returns a display string representing the entity.
     */
    format: (item: Entity) => string;

    /**
     * When applicable: returns all the values that the type parameter can take.
     */
    types?: () => string[];

    /**
     * The properties of the class.
     */
    properties: { [propName: string]: PropDescriptor };

    /**
     * Used for caching the list of definitions
     */
    definitionIdsArray?: SelectorChoice[];

    /**
     * True for entities that belong to the admin workspace
     */
    isAdmin?: boolean;

    /**
     * True for definitioned screens that are archived
     */
    isArchived?: boolean;
}

export interface ParameterDescriptor {
    key: string;
    desc: PropDescriptor;
    isRequired?: boolean;
}

export interface PropDescriptorBase {

    /**
     * The label of this field, typically shown on table headers
     */
    label: () => string;
}

export interface PropVisualDescriptorBase {

    /**
     * Whether the field value should be displayed as RTL
     */
    alignment?: 'left' | 'right' | 'center';
}

export interface TextPropDescriptor extends TextPropVisualDescriptor, PropDescriptorBase {
    datatype: 'string';
}
export interface TextPropVisualDescriptor extends PropVisualDescriptorBase {
    control: 'text';
}

export interface SerialPropDescriptor extends SerialPropVisualDescriptor, PropDescriptorBase {
    datatype: 'integral';
}
export interface SerialPropVisualDescriptor extends PropVisualDescriptorBase {
    control: 'serial';

    prefix: string;
    codeWidth: number;
}


export interface ChoicePropDescriptor extends ChoicePropVisualDescriptor, PropDescriptorBase {
    datatype: 'integral' | 'string';
}
export interface ChoicePropVisualDescriptor extends PropVisualDescriptorBase {
    control: 'choice';

    /**
     * All the choices that can be selected
     */
    choices: (string | number)[];

    /**
     * A function that formats any choice into a display object
     */
    format: (choice: string | number) => string;
    color?: (choice: string | number) => string;

    /**
     * Useful for components to cache the list of { name, value } for the selector
     */
    selector?: { value: string | number; name: () => string }[];
}

export function getChoices(desc: ChoicePropVisualDescriptor): SelectorChoice[] {
    desc.selector = desc.selector || desc.choices.map(c => ({ value: c, name: () => desc.format(c) }));
    return desc.selector;
}

export interface NumberPropDescriptor extends NumberPropVisualDescriptor, PropDescriptorBase {
    datatype: 'integral' | 'decimal';
}
export interface NumberPropVisualDescriptor extends PropVisualDescriptorBase {
    control: 'number';

    /**
     * Number of decimal places to display
     */
    minDecimalPlaces: number;
    maxDecimalPlaces: number;
}

export interface PercentPropDescriptor extends PercentPropVisualDescriptor, PropDescriptorBase {
    datatype: 'decimal';
}
export interface PercentPropVisualDescriptor extends PropVisualDescriptorBase {
    control: 'percent';

    /**
     * Number of decimal places to display
     */
    minDecimalPlaces: number;
    maxDecimalPlaces: number;
}

export interface DatePropDescriptor extends DatePropVisualDescriptor, PropDescriptorBase {
    datatype: 'date' | 'datetime' | 'datetimeoffset';
}
export interface DatePropVisualDescriptor extends PropVisualDescriptorBase {
    control: 'date';
}

export interface DatetimePropDescriptor extends DatetimePropVisualDescriptor, PropDescriptorBase {
    datatype: 'datetime' | 'datetimeoffset';
}
export interface DatetimePropVisualDescriptor extends PropVisualDescriptorBase {
    control: 'datetime';
}

export interface BooleanPropDescriptor extends BooleanPropVisualDescriptor, PropDescriptorBase {
    datatype: 'boolean';
}
export interface BooleanPropVisualDescriptor extends PropVisualDescriptorBase {
    control: 'boolean';

    /**
     * Optional function for formatting the boolean values, defaults are 'Yes' and 'No'
     */
    format?: (b: boolean) => string;
}

export interface NavigationPropDescriptor extends NavigationPropVisualDescriptor, PropDescriptorBase {
    datatype: 'entity';
    /**
     * The name of the foreign key property
     */
    foreignKeyName: string;
}
export interface NavigationPropVisualDescriptor extends PropVisualDescriptorBase {
    control: Collection;

    /**
     * Determines the definitionId of the entities that reside in these properties (e.g. Inventory vs. Resource)
     */
    definitionId?: number;

    /**
     * For the details picker
     */
    filter?: string;
}

export declare type PropVisualDescriptor = TextPropVisualDescriptor | ChoicePropVisualDescriptor |
    BooleanPropVisualDescriptor | NumberPropVisualDescriptor | PercentPropVisualDescriptor |
    DatePropVisualDescriptor | DatetimePropVisualDescriptor | NavigationPropVisualDescriptor | SerialPropVisualDescriptor;

export declare type PropDescriptor = TextPropDescriptor | ChoicePropDescriptor |
    BooleanPropDescriptor | NumberPropDescriptor | PercentPropDescriptor |
    DatePropDescriptor | DatetimePropDescriptor | NavigationPropDescriptor | SerialPropDescriptor;

export function entityDescriptorImpl(
    pathArray: string[], baseCollection: string, baseDefinition: number,
    wss: WorkspaceService, trx: TranslateService): EntityDescriptor {

    if (!baseCollection) {
        throw new Error(`The baseCollection is not specified, therefore cannot retrieve the Entity descriptor`);
    }

    let currentEntityDesc = metadata[baseCollection](wss, trx, baseDefinition);

    for (const step of pathArray) {
        const propDesc = currentEntityDesc.properties[step];

        if (!propDesc) {
            throw new Error(`Property ${step} does not exist`);

        } else if (propDesc.datatype !== 'entity') {
            throw new Error(`Property ${step} is not a navigation property`);

        } else {

            const coll = propDesc.control;
            const definition = propDesc.definitionId;

            currentEntityDesc = metadata[coll](wss, trx, definition);
        }
    }

    return currentEntityDesc;
}

export function isText(propDesc: PropDescriptor): boolean {
    return !!propDesc && propDesc.datatype === 'string';
}

export function isNumeric(propDesc: PropDescriptor): boolean {
    return !!propDesc && (propDesc.datatype === 'integral' || propDesc.datatype === 'decimal');
}

export function hasControlOptions(control: Control) {
    if (!control) {
        return false;
    }

    switch (control) {
        case 'text':
        case 'date':
        case 'datetime':
        case 'boolean':
            // Those controls do not have additional options
            return false;
        default:
            return true;
    }
}
