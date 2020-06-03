import { metadata_Unit } from '../unit';
import { WorkspaceService } from '../../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { metadata_User as metadata_User } from '../user';
import { metadata_Role } from '../role';
import { metadata_Agent } from '../agent';
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
import { metadata_Line } from '../line';
import { metadata_AdminUser } from '../admin-user';
import { metadata_IdentityServerUser } from '../identity-server-user';
import { metadata_ExchangeRate } from '../exchange-rate';
import { metadata_InboxRecord } from '../inbox-record';
import { metadata_OutboxRecord } from '../outbox-record';
import { metadata_IfrsConcept } from '../ifrs-concept';
import { metadata_MarkupTemplate } from '../markup-template';
import { SelectorChoice } from '~/app/shared/selector/selector.component';
import { Entity } from './entity';

export const metadata: {
    [collection: string]: (
        ws: WorkspaceService,
        trx: TranslateService,
        definitionId?: string) => EntityDescriptor
} = {
    // Application
    Unit: metadata_Unit,
    User: metadata_User,
    Agent: metadata_Agent,
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
    Line: metadata_Line,
    ExchangeRate: metadata_ExchangeRate,
    DetailsEntry: metadata_DetailsEntry,
    SummaryEntry: metadata_SummaryEntry,
    MarkupTemplate: metadata_MarkupTemplate,
    InboxRecord: metadata_InboxRecord,
    OutboxRecord: metadata_OutboxRecord,

    // Admin
    AdminUser: metadata_AdminUser,
    IdentityServerUser: metadata_IdentityServerUser,

    // Temp
    VoucherBooklet: metadata_VoucherBooklet,
};

let _collections: SelectorChoice[];

export function collectionsWithEndpoint(ws: WorkspaceService, trx: TranslateService): SelectorChoice[] {
    if (!_collections) {
        _collections = Object.keys(metadata)
            .filter(key => {
                const meta = metadata[key](ws, trx, null);
                return !!meta && !meta.isAdmin && !!meta.apiEndpoint;
            })
            .map(key => ({
                value: key,
                name: metadata[key](ws, trx, null).titlePlural
            }));
    }

    return _collections;
}

export interface EntityDescriptor {

    /**
     * The collection Id of this entity descriptor, (aka the table)
     */
    collection: string;

    /**
     * The definition Id of this entity descriptor
     */
    definitionId?: string;

    /**
     * Collections that have definitions list the definitions here
     */
    definitionIds?: string[];

    /**
     * The plural name of the entity (e.g. Agents).
     */
    titlePlural: () => string;

    /**
     * The singular name of the entity (e.g. Agent).
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
    screenUrl?: string;

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

    /**
     * Whether the field value should be displayed as RTL
     */
    alignment?: 'left' | 'right' | 'center';
}

export interface TextPropDescriptor extends PropDescriptorBase {
    control: 'text';
}

export interface SerialPropDescriptor extends PropDescriptorBase {
    control: 'serial';

    format: (serial: number) => string;
}

export interface ChoicePropDescriptor extends PropDescriptorBase {
    control: 'choice';

    /**
     * All the choices that can be selected
     */
    choices: (string | number)[];

    /**
     * A function that formats any choice into a display object
     */
    format: (choice: string | number) => string;

    /**
     * Useful for components to cache the list of { name, value } for the selector
     */
    selector?: { value: string | number; name: () => string }[];
}

export interface StatePropDescriptor extends PropDescriptorBase {
    control: 'state';

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

export function getChoices(desc: StatePropDescriptor | ChoicePropDescriptor): SelectorChoice[] {
    desc.selector = desc.selector || desc.choices.map(c => ({ value: c, name: () => desc.format(c) }));
    return desc.selector;
}

export interface NumberPropDescriptor extends PropDescriptorBase {
    control: 'number';

    /**
     * Number of decimal places to display
     */
    minDecimalPlaces: number;
    maxDecimalPlaces: number;
    // formatAsCurrency?: boolean;
}

export interface DatePropDscriptor extends PropDescriptorBase {
    control: 'date';
}

export interface DatetimePropDscriptor extends PropDescriptorBase {
    control: 'datetime';
}

export interface BooleanPropDescriptor extends PropDescriptorBase {
    control: 'boolean';

    /**
     * Optional function for formatting the boolean values, defaults are 'Yes' and 'No'
     */
    format?: (b: boolean) => string;
}

export interface NavigationPropDescriptor extends PropDescriptorBase {
    control: 'navigation';

    /**
     * Determines the definitionId of the entities that reside in these properties (e.g. Inventory vs. Resource)
     */
    definition?: string;

    /**
     * The name of the foreign key property
     */
    foreignKeyName: string;

    /**
     * Determines the type of this property
     */
    type: string; // e.g. Agent

    /**
     * Determines the name of the collection holding the entities represented by this property
     */
    collection?: string; // e.g. Custody
}

export declare type PropDescriptor = TextPropDescriptor | ChoicePropDescriptor | BooleanPropDescriptor
    | NumberPropDescriptor | DatePropDscriptor | DatetimePropDscriptor | NavigationPropDescriptor
    | StatePropDescriptor | SerialPropDescriptor;

export function entityDescriptorImpl(
    pathArray: string[], baseCollection: string, baseDefinition: string,
    wss: WorkspaceService, trx: TranslateService): EntityDescriptor {

    if (!baseCollection) {
        throw new Error(`The baseCollection is not specified, therefore cannot retrieve the Entity descriptor`);
    }

    let currentEntityDesc = metadata[baseCollection](wss, trx, baseDefinition);

    for (const step of pathArray) {
        const propDesc = currentEntityDesc.properties[step];

        if (!propDesc) {
            throw new Error(`Property ${step} does not exist`);

        } else if (propDesc.control !== 'navigation') {
            throw new Error(`Property ${step} is not a navigation property`);

        } else {

            const coll = propDesc.collection || propDesc.type;
            const definition = propDesc.definition;

            currentEntityDesc = metadata[coll](wss, trx, definition);
        }
    }

    return currentEntityDesc;
}

export function isText(propDesc: PropDescriptor): boolean {
    return !!propDesc && (propDesc.control === 'text' ||
        ((propDesc.control === 'state' || propDesc.control === 'choice') && (typeof propDesc.choices[0]) === 'string'));
}

export function isNumeric(propDesc: PropDescriptor): boolean {
    return !!propDesc && propDesc.control === 'number';
}
