import { metadata_MeasurementUnit } from '../measurement-unit';
import { TenantWorkspace } from '../../workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { metadata_User as metadata_User } from '../user';
import { EntityWithKey } from './entity-with-key';
import { metadata_Role } from '../role';
import { metadata_ProductCategory } from '../product-category';
import { metadata_IfrsNote } from '../ifrs-note';

export const metadata: { [collection: string]: (ws: TenantWorkspace, trx: TranslateService, subtype: string) => DtoDescriptor } = {
    MeasurementUnit: metadata_MeasurementUnit,
    User: metadata_User,
    Role: metadata_Role,
    ProductCategory: metadata_ProductCategory,
    IfrsNote: metadata_IfrsNote
};

export interface DtoDescriptor {

    /**
     * The DTO properties that need to be selected from the server for the format function to succeed
     */
    select: string[];

    /**
     * When ordering by a nav property of this DTO type, this value specifies the OData 'orderby' value to use
     */
    orderby: string[];

    /**
     * The server endpoint from which to retrieve DTOs of this type, after the 'https://www.bsharp.online/api/' part
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
    subtype?: string;

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

export function dtoDescriptorImpl(pathArray: string[], baseCollection: string, baseSubtype: string,
    ws: TenantWorkspace, trx: TranslateService, ignoreLast = false, labels: string[] = null): DtoDescriptor {

    if (!baseCollection) {
        throw new Error(`The baseCollection is not specified, therefore cannot retrieve the DTO descriptor`);
    }

    const length = pathArray.length - (ignoreLast ? 1 : 0);
    let currentDtoDesc = metadata[baseCollection](ws, trx, baseSubtype);

    for (let i = 0; i < length; i++) {
        const step = pathArray[i];
        const propDesc = currentDtoDesc.properties[step];

        if (!propDesc) {
            throw new Error(`Property ${step} does not exist`);

        } else if (propDesc.control !== 'navigation') {
            throw new Error(`Property ${step} is not a navigation property`);

        } else {
            if (!!labels) {
                labels.push(propDesc.label);
            }

            const coll = propDesc.collection || propDesc.type;
            const subtype = propDesc.subtype;

            currentDtoDesc = metadata[coll](ws, trx, subtype);
        }
    }

    return currentDtoDesc;
}

export function propDescriptorImpl(pathArray: string[], baseCollection: string, baseSubtype: string,
    ws: TenantWorkspace, trx: TranslateService, labels: string[] = null): PropDescriptor {

    if (pathArray.length > 0) {
        const dtoDesc = dtoDescriptorImpl(pathArray, baseCollection, baseSubtype, ws, trx, true, labels);
        const lastStep = pathArray[pathArray.length - 1];
        const result = dtoDesc.properties[lastStep];
        if (!result) {
            throw new Error(`Property '${lastStep}' does not exist`);
        } else {
            if (!!labels) {
                labels.push(result.label);
            }

            return result;
        }
    } else {
        throw new Error(`The path is empty, therefore cannot retrieve the property descriptor`);
    }
}
