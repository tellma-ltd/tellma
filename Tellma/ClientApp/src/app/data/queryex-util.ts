// tslint:disable:max-line-length
import { TranslateService } from '@ngx-translate/core';
import { Collection, DataType, entityDescriptorImpl, getNavPropertyFromForeignKey, metadata, NavigationPropDescriptor, PropDescriptor, PropVisualDescriptor } from './entities/base/metadata';
import { QueryexBase, QueryexBinaryOperator, QueryexBit, QueryexColumnAccess, QueryexFunction, QueryexNull, QueryexNumber, QueryexParameter, QueryexQuote, QueryexUnaryOperator } from './queryex';
import { WorkspaceService } from './workspace.service';

type Calendar = 'GR' | 'ET' | 'UQ';
const calendarsArray: Calendar[] = ['GR', 'ET', 'UQ'];

// Type Guards
function isPropDescriptor(target: DataType | PropDescriptor | PropVisualDescriptor): target is PropDescriptor {
    return !!target && !!(target as PropDescriptor).datatype;
}

function isNavPropDescriptor(desc: PropDescriptor): desc is NavigationPropDescriptor {
    return desc.hasOwnProperty('foreignKeyName');
}

function precedence(datatype: DataType) {
    switch (datatype) {
        case 'boolean': return 1;
        case 'hierarchyid': return 2;
        case 'geography': return 4;
        case 'datetimeoffset': return 8;
        case 'datetime': return 16;
        case 'date': return 32;
        case 'numeric': return 64;
        case 'numeric': return 65; // TODO: delete
        case 'bit': return 128;
        case 'string': return 256;
        case 'null': return 512;
        case 'entity': return 1024;
        default: throw new Error(`Precedence: Unknown datatype ${datatype}`);
    }
}

function mergeArithmeticNumericDescriptors(d1: PropDescriptor, d2: PropDescriptor, label: () => string): PropDescriptor {
    // This is for arithmetic operations (+ /)
    if (d1.datatype === 'numeric' && d2.datatype === 'numeric') {
        if (d1.control === 'percent' && d2.control === 'percent') {
            return {
                datatype: 'numeric',
                control: 'percent',
                minDecimalPlaces: Math.min(d1.minDecimalPlaces, d2.minDecimalPlaces),
                maxDecimalPlaces: Math.max(d1.maxDecimalPlaces, d2.maxDecimalPlaces),
                label
            };
        } else {
            const leftMinDecimals = d1.control === 'number' ? d1.minDecimalPlaces :
                d1.control === 'percent' ? d1.minDecimalPlaces - 2 : 0;
            const rightMinDecimals = d2.control === 'number' ? d2.minDecimalPlaces :
                d2.control === 'percent' ? d2.minDecimalPlaces - 2 : 0;
            const leftMaxDecimals = d1.control === 'number' ? d1.maxDecimalPlaces :
                d1.control === 'percent' ? d1.maxDecimalPlaces - 2 : 0;
            const rightMaxDecimals = d2.control === 'number' ? d2.maxDecimalPlaces :
                d2.control === 'percent' ? d2.maxDecimalPlaces - 2 : 0;

            return {
                datatype: 'numeric',
                control: 'number',
                minDecimalPlaces: Math.min(leftMinDecimals, rightMinDecimals),
                maxDecimalPlaces: Math.max(leftMaxDecimals, rightMaxDecimals),
                label
            };
        }
    } else {
        throw new Error(`[Bug] Merging non numeric datatypes ${d1} and ${d2}.`);
    }
}

function mergeFallbackNumericDescriptors(d1: PropDescriptor, d2: PropDescriptor, label: () => string): PropDescriptor {
    // This is for fallback operations like If and IsNull
    if (d1.datatype === 'numeric' && d2.datatype === 'numeric') {

        // If they're both entities with the same control and defId, return that with the filter conjunction
        if (isNavPropDescriptor(d1) && isNavPropDescriptor(d2)) {
            if (d1.definitionId === d2.definitionId && d1.control === d2.control) {
                let filter = d1.filter;
                if (!filter) {
                    filter = d2.filter;
                } else if (!!d2.filter) {
                    filter = `(${filter}) and (${d2.filter})`;
                }

                return {
                    datatype: 'numeric',
                    control: d1.control,
                    definitionId: d1.definitionId,
                    filter,
                    label,
                    foreignKeyName: d1.foreignKeyName
                };
            }

        } else if (d1.control === 'choice' && d2.control === 'choice') {
            // If they're both choices, merge the choices
            // Efficiently calculate the union of the two choice arrays
            const tracker = {};
            for (const v of d1.choices) {
                tracker[v] = true;
            }
            for (const v of d2.choices) {
                tracker[v] = true;
            }

            const choices = Object.keys(tracker).map(c => +c);

            // Prepare the combined format function
            const format1 = d1.format;
            const format2 = d2.format;
            const format = (v: string) => format1(v) || format2(v);

            const color1 = d1.color;
            const color2 = d2.color;
            const color = (v: string) => (color1 ? color1(v) : null) || (color2 ? color2(v) : null);

            return {
                datatype: 'numeric',
                control: 'choice',
                format,
                color,
                choices,
                label
            };
        } else if (d1.control === 'serial' && d2.control === 'serial') {
            if (d1.prefix === d2.prefix && d1.codeWidth === d2.codeWidth) {
                return {
                    datatype: 'numeric',
                    control: 'serial',
                    prefix: d1.prefix,
                    codeWidth: d1.codeWidth,
                    label
                };
            }
        }

        // Last resort, merge as you would for arithmetic
        return mergeArithmeticNumericDescriptors(d1, d2, label);

    } else {
        throw new Error(`[Bug] Merging non numeric datatypes ${d1} and ${d2}.`);
    }
}

function mergeFallbackStringDescriptors(d1: PropDescriptor, d2: PropDescriptor, label: () => string): PropDescriptor {
    // This is for fallback operations like If and IsNull
    if (d1.datatype === 'string' && d2.datatype === 'string') {

        // If they're both entities with the same control and defId, return that with the filter conjunction
        if (isNavPropDescriptor(d1) && isNavPropDescriptor(d2)) {
            if (d1.definitionId === d2.definitionId && d1.control === d2.control) {
                let filter = d1.filter;
                if (!filter) {
                    filter = d2.filter;
                } else if (!!d2.filter) {
                    filter = `(${filter}) and (${d2.filter})`;
                }

                return {
                    datatype: 'string',
                    control: d1.control,
                    definitionId: d1.definitionId,
                    filter,
                    label,
                    foreignKeyName: d1.foreignKeyName
                };
            }

        } else if (d1.control === 'choice' && d2.control === 'choice') {
            // If they're both choices, merge the choices
            // Efficiently calculate the union of the two choice arrays
            const tracker = {};
            for (const v of d1.choices) {
                tracker[v] = true;
            }
            for (const v of d2.choices) {
                tracker[v] = true;
            }

            const choices = Object.keys(tracker);

            // Prepare the combined format function
            const format1 = d1.format;
            const format2 = d2.format;
            const format = (v: string) => format1(v) || format2(v);

            const color1 = d1.color;
            const color2 = d2.color;
            const color = (v: string) => (color1 ? color1(v) : null) || (color2 ? color2(v) : null);

            return {
                datatype: 'string',
                control: 'choice',
                format,
                color,
                choices,
                label
            };
        }

        // Last resort, return a plain old text editor
        return {
            datatype: 'string',
            control: 'text',
            label
        };
    }
}

function getLowestPrecedenceDescFromVisual(desc: PropVisualDescriptor, label: () => string, wss: WorkspaceService, trx: TranslateService): PropDescriptor {
    switch (desc.control) {
        case 'unsupported':
            return { datatype: 'boolean', ...desc, label };
        case 'datetime':
            return { datatype: 'datetime', ...desc, label };
        case 'date':
            return { datatype: 'date', ...desc, label };
        case 'number':
            return { datatype: 'numeric', ...desc, label };
        case 'check':
            return { datatype: 'bit', ...desc, label };
        case 'text':
            return { datatype: 'string', ...desc, label };
        case 'serial':
            return { datatype: 'numeric', ...desc, label };
        case 'choice':
            return { datatype: 'string', ...desc, label };
        case 'percent':
            return { datatype: 'numeric', ...desc, label };
        case 'null':
            return { datatype: 'null', ...desc, label };
        default:
            const entityDesc = metadata[desc.control](wss, trx, desc.definitionId);
            const idDesc = entityDesc.properties.Id.datatype;
            if (idDesc === 'numeric') {
                return { datatype: 'numeric', ...desc, foreignKeyName: null, label };
            } else if (idDesc === 'string') {
                return { datatype: 'string', ...desc, foreignKeyName: null, label };
            } else {
                throw new Error(`[Bug] type ${entityDesc.titleSingular()} has an Id that is neither string nor numeric`);
            }
    }
}

/**
 * Returns a PropDescriptor from a PropVisualDescriptor and a DataType, if they are compatible, else returns undefined
 */
export function tryGetDescFromVisual(desc: PropVisualDescriptor, datatype: DataType, label: () => string, wss: WorkspaceService, trx: TranslateService): PropDescriptor {
    switch (desc.control) {
        case 'unsupported':
            switch (datatype) {
                case 'boolean':
                case 'hierarchyid':
                case 'geography':
                    return { datatype, ...desc, label };
            }
            break;
        case 'datetime':
            switch (datatype) {
                case 'datetimeoffset':
                case 'datetime':
                    return { datatype, ...desc, label };
            }
            break;
        case 'date':
            switch (datatype) {
                case 'datetimeoffset':
                case 'datetime':
                case 'date':
                    return { datatype, ...desc, label };
            }
            break;
        case 'number':
            if (datatype === 'numeric') {
                return { datatype, ...desc, label };
            }
            break;
        case 'check':
            if (datatype === 'bit') {
                return { datatype, ...desc, label };
            }
            break;
        case 'text':
            if (datatype === 'string') {
                return { datatype, ...desc, label };
            }
            break;
        case 'serial':
            if (datatype === 'numeric') {
                return { datatype, ...desc, label };
            }
            break;
        case 'choice':
            if (datatype === 'string') {
                return { datatype, ...desc, label };
            } else if (datatype === 'numeric' && desc.choices.every(c => !isNaN(+c))) {
                return { datatype, ...desc, label };
            }
            break;
        case 'percent':
            if (datatype === 'numeric') {
                return { datatype, ...desc, label };
            }
            break;
        case 'null':
            if (datatype === 'null') {
                return { datatype, ...desc, label };
            }
            break;
        default:
            const idDesc = metadata[desc.control](wss, trx, desc.definitionId).properties.Id.datatype;
            if (datatype === 'numeric' && idDesc === 'numeric') {
                return { datatype, ...desc, foreignKeyName: null, label };
            }
            if (datatype === 'string' && idDesc === 'string') {
                return { datatype, ...desc, foreignKeyName: null, label };
            }
    }
}

function tryGetDescFromDatatype(targetType: DataType, label: () => string): PropDescriptor {
    // A null can be implicitly cast to any one of these
    switch (targetType) {
        case 'string':
            return { datatype: targetType, control: 'text', label };
        case 'numeric':
            return { datatype: targetType, control: 'number', label, minDecimalPlaces: 0, maxDecimalPlaces: 4 };
        case 'bit':
            return { datatype: targetType, control: 'check', label };
        case 'date':
            return { datatype: targetType, control: 'date', label };
        case 'datetime':
        case 'datetimeoffset':
            return { datatype: targetType, control: 'date', label };
        case 'geography':
        case 'hierarchyid':
            return { datatype: targetType, control: 'unsupported', label };
    }
}

function mergeDescriptors(d1: PropDescriptor, d2: PropDescriptor, label: () => string): PropDescriptor {

    if (d1.datatype !== d2.datatype) {
        throw new Error(`[Bug] Merging two different datatypes ${d1.datatype} and ${d2.datatype}.`);
    }

    switch (d1.datatype) {
        case 'string': {
            return mergeFallbackStringDescriptors(d1, d2, label);
        }
        case 'numeric':
            return mergeFallbackNumericDescriptors(d1, d2, label);

        case 'date':
            return {
                datatype: 'date',
                control: 'date',
                label
            };

        case 'datetime':
            return {
                datatype: 'datetime',
                control: 'datetime',
                label
            };

        case 'datetimeoffset':
            return {
                datatype: 'datetimeoffset',
                control: 'datetime',
                label
            };

        case 'bit':
            return {
                datatype: 'bit',
                control: 'check',
                label
            };

        case 'geography':
        case 'hierarchyid':
        case 'boolean':
            return {
                datatype: d1.datatype,
                control: 'unsupported',
                label
            };

        case 'null':
            return {
                datatype: 'null',
                control: 'null',
                label
            };
    }

    throw new Error(`[Bug] Merging unhandled datatype ${d1.datatype}.`);
}

export interface ExpressionInfo {
    exp?: QueryexBase;
    desc?: PropDescriptor;
}

export class QueryexUtil {

    public static differentOverrides(overrides: { [key: string]: PropVisualDescriptor }, maxDescs: { [key: string]: PropDescriptor }): boolean {
        for (const key of Object.keys(maxDescs)) {
            const maxDesc = maxDescs[key];
            const override = overrides[key];
            if (!isPropDescriptor(override) || maxDesc.datatype !== override.datatype) {
                return true;
            }
        }

        return false;
    }

    public static parameterMaxDescs(
        expressions: QueryexBase[],
        defaultExpressions: { [key: string]: ExpressionInfo }): { [key: string]: PropDescriptor } {

        // (1) Group all parameters by key
        const lowerKeys: { [key: string]: string } = {};
        const parameters: { [key: string]: QueryexParameter[] } = {};
        for (const exp of expressions) {
            for (const p of exp.parameters()) {
                // This makes the keys case-insensitive while keeping the original keys casing
                if (!lowerKeys[p.keyLower]) {
                    lowerKeys[p.keyLower] = p.key;
                }
                const key = lowerKeys[p.keyLower];

                if (!!parameters[key]) {
                    parameters[key].push(p);
                } else {
                    parameters[key] = [p];
                }
            }
        }

        // Results
        const result: { [key: string]: PropDescriptor } = {};

        for (const key of Object.keys(parameters)) {
            const paramsOfKey = parameters[key];
            const defaultExp = defaultExpressions[key];

            let descMax: PropDescriptor;
            if (paramsOfKey.length > 1 || !!defaultExp) {
                // A parameter key has a default expression or is used more than once, must check that the datatypes are consistent
                for (const p of paramsOfKey) {
                    if (!descMax) {
                        descMax = p.desc;
                    } else if (!!p.desc) {
                        if (precedence(descMax.datatype) > precedence(p.desc.datatype)) {
                            descMax = p.desc;
                            // outSecondCheck.required = true; // Multiple usages of same param but with different native datatypes
                        } else if (p.desc.datatype === descMax.datatype) {
                            descMax = mergeDescriptors(p.desc, descMax, p.desc.label);
                        }
                    }
                }

                if (!!descMax && !!defaultExp && !!defaultExp.desc && precedence(descMax.datatype) > precedence(defaultExp.desc.datatype)) {
                    descMax = defaultExp.desc;
                    // outSecondCheck.required = true; // The default expression uses a different datatype
                }

            } else {
                descMax = paramsOfKey[0].desc;
            }

            result[key] = descMax; // Could be 'null'
        }

        return result;
    }

    public static nativeDesc(
        exp: QueryexBase,
        overrides: { [key: string]: PropVisualDescriptor },
        coll: Collection,
        defId: number,
        wss: WorkspaceService,
        trx: TranslateService) {

        return QueryexUtil.tryDesc(exp, overrides, null, coll, defId, wss, trx);
    }

    public static tryBooleanDesc(
        exp: QueryexBase,
        overrides: { [key: string]: PropVisualDescriptor },
        coll: Collection,
        defId: number,
        wss: WorkspaceService,
        trx: TranslateService) {

        return QueryexUtil.tryDesc(exp, overrides, 'boolean', coll, defId, wss, trx);
    }

    public static tryDesc(
        expression: QueryexBase,
        overrides: { [key: string]: PropVisualDescriptor },
        t: DataType,
        coll: Collection,
        defId: number,
        wss: WorkspaceService,
        trx: TranslateService): PropDescriptor {

        // We defined everything as inner functions so we don't have to pass 5 parameters with every one of the many recursive calls

        function tryDescImpl(ex: QueryexBase, target: DataType | PropDescriptor): PropDescriptor {
            // Unpack the parameter
            let hintDesc: PropDescriptor;
            let targetType: DataType;
            if (isPropDescriptor(target)) {
                hintDesc = target;
                targetType = target.datatype;
            } else {
                hintDesc = null;
                targetType = target;
            }

            // First check the cases where the requested output can influence the requested input
            if (ex instanceof QueryexFunction) {
                const nameLower = ex.name.toLowerCase();
                switch (nameLower) {
                    case 'min':
                    case 'max': {
                        const arg1 = aggregationParameters(ex);
                        const arg1Desc = tryDescImpl(arg1, target);
                        if (!!arg1Desc) {
                            const resultDesc = { ...arg1Desc };

                            const originalLabel = resultDesc.label;
                            const aggregationLabel = () => trx.instant('DefaultAggregationMeasure', {
                                0: trx.instant('Aggregation_' + nameLower),
                                1: originalLabel()
                            });

                            resultDesc.label = aggregationLabel;
                            return resultDesc;
                        } else {
                            return undefined;
                        }
                    }

                    case 'if': {
                        const { arg2, arg3 } = ifParameters(ex);

                        const arg2Desc = tryDescImpl(arg2, target);
                        const arg3Desc = tryDescImpl(arg3, target);

                        if (!!arg2Desc && !!arg3Desc) {
                            return mergeDescriptors(arg2Desc, arg3Desc, () => trx.instant('Expression'));
                        } else {
                            return undefined;
                        }
                    }

                    case 'isnull': {
                        const { arg1, arg2 } = isNullParameters(ex);

                        const arg1Desc = tryDescImpl(arg1, target);
                        const arg2Desc = tryDescImpl(arg2, target);

                        if (!!arg1Desc && !!arg2Desc) {
                            return mergeDescriptors(arg1Desc, arg2Desc, () => trx.instant('Expression'));
                        } else {
                            return undefined;
                        }
                    }
                }
            } else if (ex instanceof QueryexBinaryOperator) {
                // We can omit this one
            } else if (ex instanceof QueryexQuote) {
                switch (targetType) {
                    case 'date':
                    case 'datetime':
                    case 'datetimeoffset': {
                        if (!isNaN(Date.parse(ex.value))) {
                            if (!!hintDesc) {
                                return hintDesc;
                            } else {
                                const label = () => trx.instant('Expression');
                                if (targetType === 'date') {
                                    const control = 'date';
                                    return { datatype: targetType, control, label };

                                } else {
                                    const control = 'datetime';
                                    return { datatype: targetType, control, label };
                                }
                            }
                        } else {
                            return undefined;
                        }
                    }
                }
            }

            // else if (ex instanceof QueryexParameter) {
            //     if (targetType === 'boolean') {
            //         ex.desc = {
            //             datatype: 'bit',
            //             control: 'check',
            //             label: () => ''
            //         };
            //         return {
            //             datatype: 'boolean',
            //             control: 'unsupported',
            //             label: () => ''
            //         };
            //     }
            // }

            // else if (ex instanceof QueryexParameter) {
            //     if (targetType !== 'boolean') {
            //         const overrideDesc = overrides[ex.keyLower];
            //         // The returned value must have a datatype compatible with overrideDesc
            //         if (isPropDescriptor(overrideDesc)) {
            //             if (overrideDesc.datatype === targetType) {
            //                 ex.desc = { ...overrideDesc };
            //             } else {
            //                 ex.desc = null;
            //             }
            //         } else {
            //             const label = !!hintDesc ? hintDesc.label : () => ex.key;
            //             ex.desc = tryGetDescFromVisual(overrideDesc, targetType, label, wss, trx);
            //         }

            //         return ex.desc;
            //     }
            // }

            // Default
            const nativeDesc = nativeDescImpl(ex);
            if (nativeDesc.datatype === targetType) {
                return nativeDesc;
            } else if (nativeDesc.datatype === 'null') {
                let desc: PropDescriptor;
                if (targetType !== 'boolean') {
                    if (!!hintDesc) {
                        desc = { ...hintDesc };
                    } else {
                        desc = tryGetDescFromDatatype(targetType, nativeDesc.label);
                    }
                }

                // If this a parameter that has not been overridden, remember the result
                if (ex instanceof QueryexParameter && !!desc) {
                    ex.desc = desc;
                }

                return desc;
            } else if (nativeDesc.datatype === 'bit') {
                if (targetType === 'numeric') {
                    return {
                        datatype: targetType,
                        control: 'number',
                        minDecimalPlaces: 0,
                        maxDecimalPlaces: 0,
                        label: nativeDesc.label
                    };
                } else if (targetType === 'boolean') {
                    return {
                        datatype: targetType,
                        control: 'unsupported',
                        label: nativeDesc.label
                    };
                }
            }
        }

        function nativeDescImpl(ex: QueryexBase): PropDescriptor {

            if (ex instanceof QueryexColumnAccess) {
                const entityDesc = entityDescriptorImpl(ex.path, coll, defId, wss, trx);

                // Special case for foreign keys
                {
                    const navPropDesc = getNavPropertyFromForeignKey(entityDesc, ex.property);
                    if (!!navPropDesc) {
                        const fkDesc = entityDesc.properties[ex.property];
                        // All nav props without FKs in the metadata have integral FKs
                        const datatype = !!fkDesc ? fkDesc.datatype as 'string' | 'numeric' : 'numeric';
                        const result = {
                            ...navPropDesc,
                        };

                        result.datatype = datatype;
                        return result;
                    }
                }

                const propDesc = entityDesc.properties[ex.property];
                if (!propDesc) {
                    throw new Error(`Property '${ex.property}' does not exist on type ${entityDesc.titleSingular()}.`);
                }

                if (propDesc.datatype === 'entity') {
                    // Should we do anything?
                }

                // Special case for Ids
                if (ex.property === 'Id' && (propDesc.datatype === 'numeric' || propDesc.datatype === 'string')) {
                    if (ex.path.length === 0) {
                        const label = metadata[coll](wss, trx, defId).titleSingular;
                        const result = {
                            datatype: propDesc.datatype,
                            control: coll,
                            definitionId: defId,
                            foreignKeyName: null,
                            label
                        };

                        return result;
                    } else {
                        const navEntityDesc = entityDescriptorImpl(ex.path.slice(0, -1), coll, defId, wss, trx);
                        const navPropDesc = navEntityDesc.properties[ex.path[ex.path.length - 1]] as NavigationPropDescriptor;
                        const result = {
                            ...navPropDesc,
                        };

                        result.datatype = propDesc.datatype;
                        return result;
                    }
                }

                // Finally return as is
                return propDesc;

            } else if (ex instanceof QueryexFunction) {
                const nameLower = ex.name.toLowerCase();
                switch (nameLower) {
                    case 'count':
                    case 'sum':
                    case 'avg':
                    case 'min':
                    case 'max': {
                        const arg1 = aggregationParameters(ex);

                        let resultDesc: PropDescriptor;
                        if (nameLower === 'count' || nameLower === 'min' || nameLower === 'max') {
                            resultDesc = nativeDescImpl(arg1);
                            if (resultDesc.datatype === 'boolean' || resultDesc.datatype === 'entity') {
                                throw new Error(`Function '${ex.name}': The first argument ${arg1} cannot have type ${resultDesc.datatype}.`);
                            }

                            if (nameLower === 'count') {
                                resultDesc = {
                                    datatype: 'numeric',
                                    control: 'number',
                                    maxDecimalPlaces: 0,
                                    minDecimalPlaces: 0,
                                    alignment: 'right',
                                    label: resultDesc.label
                                };
                            } else {
                                resultDesc = { ...resultDesc };
                            }
                        } else { // sum and avg
                            resultDesc = tryDescImpl(arg1, 'numeric');
                            if (!resultDesc) {
                                throw new Error(`Function '${ex.name}': The first argument ${arg1} could not be interpreted as a numeric.`);
                            } else {
                                resultDesc = { ...resultDesc };
                            }
                        }

                        const originalLabel = resultDesc.label;
                        const aggregationLabel = () => trx.instant('DefaultAggregationMeasure', {
                            0: trx.instant('Aggregation_' + nameLower),
                            1: originalLabel()
                        });

                        resultDesc.label = aggregationLabel;
                        return resultDesc;
                    }

                    case 'year':
                    case 'quarter':
                    case 'month':
                    case 'day':
                    case 'weekday': {
                        if (ex.arguments.length < 1 || ex.arguments.length > 2) {
                            throw new Error(`No overload for function '${ex.name}' accepts ${ex.arguments.length} arguments.`);
                        }

                        const datePart = nameLower;
                        const arg1 = ex.arguments[0];
                        const arg1Desc = tryDescImpl(arg1, 'date') || tryDescImpl(arg1, 'datetime') || tryDescImpl(arg1, 'datetimeoffset');
                        if (!!arg1Desc) {
                            let calendar: Calendar = 'GR'; // Gregorian
                            if (ex.arguments.length >= 2) {
                                const arg2 = ex.arguments[1];
                                if (arg2 instanceof QueryexQuote) {
                                    calendar = arg2.value.toUpperCase() as Calendar;
                                    if (calendarsArray.indexOf(calendar) < 0) {
                                        throw new Error(`Function '${ex.name}': The second argument ${arg2} must be one of the supported calendars: '${calendarsArray.join(`', '`)}'.`);
                                    }
                                } else {
                                    throw new Error(`Function '${ex.name}': The second argument must be a simple quote like this: 'UQ'.`);
                                }
                            }

                            const label = () => `${arg1Desc.label()} (${trx.instant('DatePart_' + datePart)})`;
                            switch (datePart) {
                                case 'day':
                                case 'year':
                                    return {
                                        datatype: 'numeric',
                                        control: 'number',
                                        label,
                                        minDecimalPlaces: 0,
                                        maxDecimalPlaces: 0
                                    };
                                case 'quarter':
                                    return {
                                        datatype: 'numeric',
                                        control: 'choice',
                                        label,
                                        choices: [1, 2, 3, 4],
                                        format: (c: number | string) => !c ? '' : trx.instant(`ShortQuarter${c}`)
                                    };
                                case 'month':
                                    if (calendar === 'GR') {
                                        return {
                                            datatype: 'numeric',
                                            control: 'choice',
                                            label,
                                            choices: [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12],
                                            format: (c: number | string) => !c ? '' : trx.instant(`ShortMonth${c}`)
                                        };
                                    } else if (calendar === 'UQ') {
                                        return {
                                            datatype: 'numeric',
                                            control: 'choice',
                                            label,
                                            choices: [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12],
                                            format: (c: number | string) => !c ? '' : trx.instant(`ShortMonthUq${c}`)
                                        };
                                    } else if (calendar === 'ET') {
                                        return {
                                            datatype: 'numeric',
                                            control: 'choice',
                                            label,
                                            choices: [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13],
                                            format: (c: number | string) => !c ? '' : trx.instant(`ShortMonthEt${c}`)
                                        };
                                    } else {
                                        // Should not reach here
                                        const msg = `Unknown calendar ${calendar}`;
                                        console.error(msg);
                                        throw new Error(msg);
                                    }
                                case 'weekday':
                                    return {
                                        datatype: 'numeric',
                                        control: 'choice',
                                        label,
                                        choices: [2 /* Mon */, 3, 4, 5, 6, 7, 1 /* Sun */],
                                        // SQL Server numbers the days differently from ngb-datepicker
                                        format: (c: number) => !c ? '' : trx.instant(`ShortDay${(c - 1) === 0 ? 7 : c - 1}`)
                                    };
                                default:
                                    throw new Error('Never'); // To keep compiler happy
                            }

                        } else {
                            throw new Error(`Function '${ex.name}': The first argument ${arg1} could not be interpreted as a date, datetime or datetimeoffset.`);
                        }
                    }

                    case 'adddays':
                    case 'addmonths':
                    case 'addyears': {
                        if (ex.arguments.length < 2 || ex.arguments.length > 3) {
                            throw new Error(`No overload for function '${ex.name}' accepts ${ex.arguments.length} arguments.`);
                        }

                        // Arg #1 Number
                        const arg1 = ex.arguments[0];
                        const arg1Desc = tryDescImpl(arg1, 'numeric');
                        if (!!arg1Desc) {
                            // Hopefully not a nullable expression
                        } else {
                            throw new Error(`Function '${ex.name}': The first argument ${arg1} could not be interpreted as a numeric.`);
                        }

                        // Arg #2 Date
                        const arg2 = ex.arguments[1];
                        const arg2Desc = tryDescImpl(arg2, 'date') || tryDescImpl(arg2, 'datetime') || tryDescImpl(arg2, 'datetimeoffset');
                        if (!arg2Desc) {
                            throw new Error(`Function '${ex.name}': The second argument ${arg2} could not be interpreted as a date, datetime or datetimeoffset.`);
                        }

                        // Arg #3 Calendar
                        let calendar: Calendar = 'GR'; // Gregorian
                        if (ex.arguments.length >= 3) {
                            const arg3 = ex.arguments[2];
                            if (arg3 instanceof QueryexQuote) {
                                calendar = arg3.value.toUpperCase() as Calendar;
                                if (calendarsArray.indexOf(calendar) < 0) {
                                    throw new Error(`Function '${ex.name}': The third argument ${arg3} must be one of the supported calendars: '${calendarsArray.join(`', '`)}'.`);
                                }
                            } else {
                                throw new Error(`Function '${ex.name}': The third argument must be a simple quote like this: 'UQ'.`);
                            }
                        }

                        return { ...arg2Desc };
                    }

                    case 'not': {
                        const expectedArgCount = 1;
                        if (ex.arguments.length !== expectedArgCount) {
                            throw new Error(`Function '${ex.name}' accepts exactly ${expectedArgCount} argument(s).`);
                        }

                        const arg1 = ex.arguments[0];
                        const arg1Desc = tryDescImpl(arg1, 'boolean');
                        if (!!arg1Desc) {
                            return { ...arg1Desc };

                        } else {
                            throw new Error(`Function '${ex.name}': The first argument ${arg1} could not be interpreted as a boolean.`);
                        }
                    }

                    case 'if': {
                        const { arg2, arg3 } = ifParameters(ex);

                        let arg2Desc = nativeDescImpl(arg2);
                        const arg2Type = arg2Desc.datatype;
                        if (arg2Type === 'boolean' || arg2Type === 'entity') {
                            throw new Error(`Function '${ex.name}': The second argument ${arg2} cannot have a type ${arg2Type}.`);
                        }

                        let arg3Desc = nativeDescImpl(arg3);
                        const arg3Type = arg3Desc.datatype;
                        if (arg3Type === 'boolean' || arg3Type === 'entity') {
                            throw new Error(`Function '${ex.name}': The third argument ${arg3} cannot have a type ${arg3Type}.`);
                        }

                        if (precedence(arg2Type) > precedence(arg3Type)) {
                            arg2Desc = tryDescImpl(arg2, arg3Desc);
                        } else if (precedence(arg3Type) > precedence(arg2Type)) {
                            arg3Desc = tryDescImpl(arg3, arg2Desc);
                        }

                        if (!arg2Desc || !arg3Desc) {
                            throw new Error(`Function '${ex.name}' cannot be used on expressions ${arg2} (${arg2Type}) and ${arg3} (${arg3Type}) because they have incompatible data types.`);
                        }

                        return mergeDescriptors(arg2Desc, arg3Desc, () => trx.instant('Expression'));
                    }

                    case 'isnull': {
                        const { arg1, arg2 } = isNullParameters(ex);

                        let arg1Desc = nativeDescImpl(arg1);
                        const arg1Type = arg1Desc.datatype;
                        if (arg1Type === 'boolean' || arg1Type === 'entity') {
                            throw new Error(`Function '${ex.name}': The first argument ${arg1} cannot have type ${arg1Type}.`);
                        }

                        let arg2Desc = nativeDescImpl(arg2);
                        const arg2Type = arg2Desc.datatype;
                        if (arg2Type === 'boolean' || arg2Type === 'entity') {
                            throw new Error(`Function '${ex.name}': The second argument ${arg2} cannot have type ${arg2Type}.`);
                        }

                        if (precedence(arg1Type) > precedence(arg2Type)) {
                            arg1Desc = tryDescImpl(arg1, arg2Desc);
                        } else if (precedence(arg2Type) > precedence(arg1Type)) {
                            arg2Desc = tryDescImpl(arg2, arg1Desc);
                        }

                        if (!arg1Desc || !arg2Desc) {
                            throw new Error(`Function '${ex.name}' cannot be used on expressions ${arg1} (${arg1Type}) and ${arg2} (${arg2Type}) because they have incompatible data types.`);
                        }

                        return mergeDescriptors(arg1Desc, arg2Desc, () => trx.instant('Expression'));
                    }

                    case 'today': {
                        if (ex.arguments.length > 0) {
                            throw new Error(`Function '${ex.name}' does not accept any arguments.`);
                        }

                        return {
                            datatype: 'date',
                            control: 'date',
                            label: () => trx.instant('Today')
                        };
                    }

                    case 'now': {
                        if (ex.arguments.length > 0) {
                            throw new Error(`Function '${ex.name}' does not accept any arguments.`);
                        }

                        return {
                            datatype: 'datetimeoffset',
                            control: 'datetime',
                            label: () => trx.instant('Now')
                        };
                    }

                    case 'me': {
                        if (ex.arguments.length > 0) {
                            throw new Error(`Function '${ex.name}' does not accept any arguments.`);
                        }

                        return {
                            datatype: 'numeric',
                            control: 'number',
                            minDecimalPlaces: 0,
                            maxDecimalPlaces: 0,
                            label: () => trx.instant('CurrentUser')
                        };
                    }

                    default:
                        {
                            throw new Error(`Unknown function '${ex.name}'.`);
                        }
                }
            } else if (ex instanceof QueryexBinaryOperator) {
                const opLower = ex.operator.toLowerCase();
                switch (opLower) {
                    case '+': {
                        let leftDesc = tryDescImpl(ex.left, 'numeric');
                        let rightDesc = tryDescImpl(ex.right, 'numeric');

                        const label = () => trx.instant('Expression');

                        if (!leftDesc || !rightDesc) {
                            leftDesc = tryDescImpl(ex.left, 'string');
                            rightDesc = tryDescImpl(ex.right, 'string');

                            if (!leftDesc || !rightDesc) {
                                const leftType = nativeDescImpl(ex.left).datatype;
                                const rightType = nativeDescImpl(ex.right).datatype;
                                throw new Error(`Operator '${ex.operator}' cannot be used on expressions ${ex.left} (${leftType}) and ${ex.right} (${rightType}) because they have incompatible data types.`);
                            }

                            return {
                                datatype: 'string',
                                control: 'text',
                                label
                            };
                        }

                        return mergeArithmeticNumericDescriptors(leftDesc, rightDesc, label);
                    }

                    case '-':
                    case '*':
                    case '/':
                    case '%': {
                        const leftDesc = tryDescImpl(ex.left, 'numeric');
                        if (!leftDesc) {
                            throw new Error(`Operator '${ex.operator}': Left operand ${ex.left} could not be interpreted as a numeric.`);
                        }

                        const rightDesc = tryDescImpl(ex.right, 'numeric');
                        if (!rightDesc) {
                            throw new Error(`Operator '${ex.operator}': Right operand ${ex.right} could not be interpreted as a numeric.`);
                        }

                        const label = () => trx.instant('Expression');
                        return mergeArithmeticNumericDescriptors(leftDesc, rightDesc, label);
                    }

                    case '&&':
                    case '||':
                    case 'and':
                    case 'or': {
                        const leftDesc = tryDescImpl(ex.left, 'boolean');
                        if (!leftDesc) {
                            throw new Error(`Operator '${ex.operator}': Left operand ${ex.left} could not be interpreted as a boolean.`);
                        }

                        const rightDesc = tryDescImpl(ex.right, 'boolean');
                        if (!rightDesc) {
                            throw new Error(`Operator '${ex.operator}': Right operand ${ex.right} could not be interpreted as a boolean.`);
                        }

                        return {
                            datatype: 'boolean',
                            control: 'unsupported',
                            label: () => trx.instant('Expression')
                        };
                    }
                    case '<>':
                    case '>':
                    case '>=':
                    case '<':
                    case '<=':
                    case '=':
                    case '!=':
                    case 'eq':
                    case 'ne':
                    case 'gt':
                    case 'ge':
                    case 'lt':
                    case 'le': {
                        const left = ex.left;
                        let leftDesc = nativeDescImpl(left);
                        const leftType = leftDesc.datatype;
                        if (leftType === 'boolean' || leftType === 'entity') {
                            throw new Error(`Operator '${ex.operator}': The left operand ${left} cannot have type ${leftType}.`);
                        }

                        const right = ex.right;
                        let rightDesc = nativeDescImpl(right);
                        const rightType = rightDesc.datatype;
                        if (rightType === 'boolean' || rightType === 'entity') {
                            throw new Error(`Operator '${ex.operator}': The right operand ${right} cannot have type ${rightType}.`);
                        }

                        if (precedence(leftType) > precedence(rightType)) {
                            leftDesc = tryDescImpl(left, rightDesc);
                        } else if (precedence(rightType) > precedence(leftType)) {
                            rightDesc = tryDescImpl(right, leftDesc);
                        }

                        if (!leftDesc || !rightDesc) {
                            throw new Error(`Operator '${ex.operator}' cannot be used on expressions ${left} (${leftType}) and ${right} (${rightType}) because they have incompatible data types.`);
                        }

                        return {
                            datatype: 'boolean',
                            control: 'unsupported',
                            label: () => trx.instant('Expression')
                        };
                    }

                    case 'descof': {
                        if (!(ex.left instanceof QueryexColumnAccess)) {
                            throw new Error(`Operator '${ex.operator}': The left operand ${ex.left} must be a column access like AccountType.Concept.`);
                        }

                        const ca = ex.right.columnAccesses()[0];
                        if (ca === ex.right) {
                            throw new Error(`Operator '${ex.operator}': The right operand cannot be a column access expression like ${ca}.`);
                        } else if (!!ca) {
                            throw new Error(`Operator '${ex.operator}': The right operand cannot contain a column access expression like ${ca}.`);
                        }

                        const left = ex.left;
                        let leftDesc = nativeDescImpl(left);
                        const leftType = leftDesc.datatype;
                        if (leftType === 'boolean' || leftType === 'entity') {
                            throw new Error(`Operator '${ex.operator}': The left operand ${left} cannot have type ${leftType}.`);
                        }

                        const right = ex.right;
                        let rightDesc = nativeDescImpl(right);
                        const rightType = rightDesc.datatype;
                        if (rightType === 'boolean' || rightType === 'entity') {
                            throw new Error(`Operator '${ex.operator}': The right operand ${right} cannot have type ${rightType}.`);
                        }

                        if (precedence(leftType) > precedence(rightType)) {
                            leftDesc = tryDescImpl(left, rightDesc);
                        } else if (precedence(rightType) > precedence(leftType)) {
                            rightDesc = tryDescImpl(right, leftDesc);
                        }

                        if (!leftDesc || !rightDesc) {
                            throw new Error(`Operator '${ex.operator}' cannot be used on expressions ${left} (${leftType}) and ${right} (${rightType}) because they have incompatible data types.`);
                        }

                        return {
                            datatype: 'boolean',
                            control: 'unsupported',
                            label: () => trx.instant('Expression')
                        };
                    }

                    case 'contains':
                    case 'startsw':
                    case 'endsw': {
                        const leftDesc = tryDescImpl(ex.left, 'string');
                        if (!leftDesc) {
                            throw new Error(`Operator '${ex.operator}': Left operand ${ex.left} could not be interpreted as string.`);
                        }

                        const rightDesc = tryDescImpl(ex.right, 'string');
                        if (!rightDesc) {
                            throw new Error(`Operator '${ex.operator}': Right operand ${ex.right} could not be interpreted as string.`);
                        }

                        return {
                            datatype: 'boolean',
                            control: 'unsupported',
                            label: () => trx.instant('Expression')
                        };
                    }
                }
            } else if (ex instanceof QueryexUnaryOperator) {
                const opLower = ex.operator.toLowerCase();
                switch (opLower) {
                    case '+':
                    case '-': {
                        const desc = tryDescImpl(ex.operand, 'numeric');
                        if (!desc) {
                            throw new Error(`Operator '${ex.operator}': Operand ${ex.operand} could not be interpreted as numeric.`);
                        }

                        if (opLower === '+') {
                            return { ...desc }; // +ve sign doesn't change anything
                        } else if (desc.control === 'number' || desc.control === 'percent') {
                            // -ve sign: If the inside is a plain number or a percent, preserve it
                            return {
                                datatype: 'numeric',
                                control: desc.control,
                                maxDecimalPlaces: desc.maxDecimalPlaces,
                                minDecimalPlaces: desc.minDecimalPlaces,
                                label: () => trx.instant('Expression')
                            };
                        } else { // serial, choice, entity
                            // Serial, choice, entity: turn it into plain number
                            return {
                                datatype: 'numeric',
                                control: 'number',
                                maxDecimalPlaces: 0,
                                minDecimalPlaces: 0,
                                label: () => trx.instant('Expression')
                            };
                        }
                    }

                    case '!':
                    case 'not': {
                        const desc = tryDescImpl(ex.operand, 'boolean');
                        if (!desc) {
                            throw new Error(`Operator '${ex.operator}': Operand ${ex.operand} could not be interpreted as boolean.`);
                        }

                        return {
                            datatype: 'boolean',
                            control: 'unsupported',
                            label: () => trx.instant('Expression')
                        };
                    }
                }
            } else if (ex instanceof QueryexQuote) {
                return {
                    datatype: 'string',
                    control: 'text',
                    label: () => ex.toString()
                };
            } else if (ex instanceof QueryexNumber) {
                return {
                    datatype: 'numeric',
                    control: 'number',
                    minDecimalPlaces: 0,
                    maxDecimalPlaces: 6,
                    label: () => ex.toString()
                };
            } else if (ex instanceof QueryexNull) {
                return {
                    datatype: 'null',
                    control: 'null',
                    label: () => ''
                };
            } else if (ex instanceof QueryexBit) {
                return {
                    datatype: 'bit',
                    control: 'check',
                    label: () => ex.value ? trx.instant('Yes') : trx.instant('No')
                };
            } else if (ex instanceof QueryexParameter) {
                const overrideDesc = overrides[ex.keyLower];
                if (isPropDescriptor(overrideDesc)) {
                    ex.desc = { ...overrideDesc };
                } else if (!!overrideDesc) {
                    const label = () => ex.key;
                    ex.desc = getLowestPrecedenceDescFromVisual(overrideDesc, label, wss, trx);
                } else {
                    const label = () => ex.key;
                    ex.desc = {
                        datatype: 'null',
                        control: 'null',
                        label
                    };
                }

                return ex.desc;
            } else {
                throw Error(`[Bug] ${ex} Has an unknown Queryex type.`);
            }
        }

        function aggregationParameters(ex: QueryexFunction): QueryexBase {
            if (ex.arguments.length < 1 || ex.arguments.length > 2) {
                throw new Error(`No overload for function '${ex.name}' accepts ${ex.arguments.length} arguments.`);
            }

            // Argument #1
            const arg1 = ex.arguments[0];

            // Argument #2
            if (ex.arguments.length >= 2) {
                const arg2 = ex.arguments[1];
                const arg2Desc = tryDescImpl(arg2, 'boolean');
                if (!arg2Desc) {
                    throw new Error(`Function '${ex.name}': The second argument ${arg2} could not be interpreted as a boolean.`);
                }
            }

            return arg1;
        }

        function ifParameters(ex: QueryexFunction): { arg2: QueryexBase, arg3: QueryexBase } {
            const expectedArgCount = 3;
            if (ex.arguments.length !== expectedArgCount) {
                throw new Error(`Function '${ex.name}' accepts exactly ${expectedArgCount} argument(s).`);
            }

            const arg1 = ex.arguments[0];
            const arg1Desc = tryDescImpl(arg1, 'boolean');
            if (!arg1Desc) {
                throw new Error(`Function '${ex.name}': The first argument ${arg1} could not be interpreted as a boolean.`);
            }

            const arg2 = ex.arguments[1];
            const arg3 = ex.arguments[2];

            return { arg2, arg3 };
        }

        function isNullParameters(ex: QueryexFunction): { arg1: QueryexBase, arg2: QueryexBase } {
            const expectedArgCount = 2;
            if (ex.arguments.length !== expectedArgCount) {
                throw new Error(`Function '${ex.name}' accepts exactly ${expectedArgCount} argument(s).`);
            }

            const arg1 = ex.arguments[0];
            const arg2 = ex.arguments[1];

            return { arg1, arg2 };
        }

        if (!!t) {
            return tryDescImpl(expression, t);
        } else {
            return nativeDescImpl(expression);
        }
    }
}
