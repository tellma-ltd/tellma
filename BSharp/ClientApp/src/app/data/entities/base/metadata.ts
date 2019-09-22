import { metadata_MeasurementUnit } from '../measurement-unit';
import { TenantWorkspace } from '../../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { metadata_User as metadata_User } from '../user';
import { EntityWithKey } from './entity-with-key';
import { metadata_Role } from '../role';
import { metadata_ProductCategory } from '../product-category';
import { metadata_IfrsNote } from '../ifrs-note';
import { metadata_Agent } from '../agent';
import { metadata_View } from '../view';
import { metadata_ResourceLookup } from '../resource-lookup';
import { metadata_Currency } from '../currency';
import { metadata_Resource } from '../resource';
import {
    metadata_VoucherBooklet, metadata_ResourcePick, metadata_ResponsibilityCenter,
    metadata_IfrsAccountClassification, metadata_IfrsEntryClassification, metadata_Account
} from '../_temp';

export const metadata: { [collection: string]: (ws: TenantWorkspace, trx: TranslateService, subtype: string) => EntityDescriptor } = {
    MeasurementUnit: metadata_MeasurementUnit,
    User: metadata_User,
    Agent: metadata_Agent,
    Role: metadata_Role,
    ProductCategory: metadata_ProductCategory,
    IfrsNote: metadata_IfrsNote,
    View: metadata_View,
    ResourceLookup: metadata_ResourceLookup,
    Currency: metadata_Currency,
    Resource: metadata_Resource,

    // Temp
    VoucherBooklet: metadata_VoucherBooklet,
    ResourcePick: metadata_ResourcePick,
    ResponsibilityCenter: metadata_ResponsibilityCenter,
    IfrsAccountClassification: metadata_IfrsAccountClassification,
    IfrsEntryClassification: metadata_IfrsEntryClassification,
    Account: metadata_Account,
};

export interface EntityDescriptor {

    /**
     * The plural name of the entity (e.g. Agents)
     */
    titlePlural: string;

    /**
     * The singular name of the entity (e.g. Agent)
     */
    titleSingular: string;

    /**
     * The Entity properties that need to be selected from the server for the format function to succeed
     */
    select: string[];

    /**
     * When ordering by a nav property of this Entity type, this value specifies the OData 'orderby' value to use
     */
    orderby: string[];

    /**
     * The server endpoint from which to retrieve Entities of this type, after the 'https://www.bsharp.online/api/' part
     */
    apiEndpoint: string;

    /**
     * A function that returns a display string representing the entity
     */
    format: (item: EntityWithKey) => string;

    /**
     * When applicable: returns all the values that the type parameter can take
     */
    types?: () => string[];

    /**
     * The properties of the class
     */
    properties: { [prop: string]: PropDescriptor };
}

export interface PropDescriptorBase {

    /**
     * The label of this field, typically shown on table headers
     */
    label: string;

    /**
     * Whether the field value should be displayed as RTL
     */
    alignment?: 'left' | 'right' | 'center';
}

export interface TextPropDescriptor extends PropDescriptorBase {
    control: 'text';
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
}

export interface NumberPropDescriptor extends PropDescriptorBase {
    control: 'number';

    /**
     * Number of decimal places to display
     */
    minDecimalPlaces: number;
    maxDecimalPlaces: number;
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
     * Determines the subtype of the entities that reside in these properties (e.g. Inventory vs. Resource)
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
    | NumberPropDescriptor | DatePropDscriptor | DatetimePropDscriptor | NavigationPropDescriptor | StatePropDescriptor;

export function entityDescriptorImpl(
    pathArray: string[], baseCollection: string, baseDefinition: string,
    ws: TenantWorkspace, trx: TranslateService): EntityDescriptor {

    if (!baseCollection) {
        throw new Error(`The baseCollection is not specified, therefore cannot retrieve the Entity descriptor`);
    }

    let currentEntityDesc = metadata[baseCollection](ws, trx, baseDefinition);

    for (const step of pathArray) {
        const propDesc = currentEntityDesc.properties[step];

        if (!propDesc) {
            throw new Error(`Property ${step} does not exist`);

        } else if (propDesc.control !== 'navigation') {
            throw new Error(`Property ${step} is not a navigation property`);

        } else {

            const coll = propDesc.collection || propDesc.type;
            const definition = propDesc.definition;

            currentEntityDesc = metadata[coll](ws, trx, definition);
        }
    }

    return currentEntityDesc;
}
