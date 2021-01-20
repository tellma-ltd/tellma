// tslint:disable:max-line-length
import { TranslateService } from '@ngx-translate/core';
import { Collection, DataType, entityDescriptorImpl, getNavPropertyFromForeignKey, metadata, NavigationPropDescriptor, PropDescriptor } from './entities/base/metadata';
import { QueryexBase, QueryexBinaryOperator, QueryexBit, QueryexColumnAccess, QueryexFunction, QueryexNull, QueryexNumber, QueryexParameter, QueryexPlaceholder, QueryexQuote, QueryexUnaryOperator } from './queryex';
import { WorkspaceService } from './workspace.service';

// export interface FunctionInfo {
//     nativeDesc(trx: TranslateService): PropDescriptor;
//     tryDesc?(datatype: QxType, args: QueryexBase[]): PropDescriptor;
// }

// export const _functionInfos: { [name: string]: FunctionInfo } = {
//     month: {
//         nativeDesc: (trx: TranslateService) => {
//             return {
//                 datatype: 'integral',
//                 control: 'choice',
//                 label: () => trx.instant('DatePart_month'),
//                 choices: [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12],
//                 format: (c: number | string) => !c ? '' : trx.instant(`ShortMonth${c}`)
//             };
//         }
//     }
// };

type Calendar = 'GR' | 'ET' | 'UQ';
const calendarsArray: Calendar[] = ['GR', 'ET', 'UQ'];

function isPropDescriptor(target: DataType | PropDescriptor): target is PropDescriptor {
    return !!(target as PropDescriptor).datatype;
}

function precedence(datatype: DataType) {
    switch (datatype) {
        case 'boolean': return 1;
        case 'hierarchyid': return 2;
        case 'geography': return 4;
        case 'datetimeoffset': return 8;
        case 'datetime': return 16;
        case 'date': return 32;
        case 'integral': return 64;
        case 'decimal': return 65; // TODO: delete
        case 'bit': return 128;
        case 'string': return 256;
        case 'null': return 512;
        case 'entity': return 1024;
        default: throw new Error(`Precedence: Unknown datatype ${datatype}`);
    }
}

function merge(desc1: PropDescriptor, desc2: PropDescriptor, trx: TranslateService): PropDescriptor {
    const result = { ...desc1 }; // TODO: implement
    return result;
}

export class QueryexUtil {

    public static nativeDescriptor(
        expression: QueryexBase,
        coll: Collection,
        defId: number,
        wss: WorkspaceService,
        trx: TranslateService): PropDescriptor {

        function tryGetDesc(ex: QueryexBase, target: DataType | PropDescriptor): PropDescriptor {
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
            // TODO...

            // Default
            const nativeDesc = getNativeDesc(ex);
            if (nativeDesc.datatype === targetType) {
                return nativeDesc;
            } else if (nativeDesc.datatype === 'null') {
                if (targetType !== 'boolean') {
                    if (!!hintDesc) {
                        return hintDesc;
                    } else {
                        // A null can be implicitly cast to any one of these
                        switch (targetType) {
                            case 'string':
                                return { datatype: targetType, control: 'text', label: nativeDesc.label };
                            case 'integral':
                                return { datatype: targetType, control: 'number', label: nativeDesc.label, minDecimalPlaces: 0, maxDecimalPlaces: 0 };
                            case 'decimal':
                                return { datatype: targetType, control: 'number', label: nativeDesc.label, minDecimalPlaces: 0, maxDecimalPlaces: 4 };
                            case 'bit':
                                return { datatype: targetType, control: 'check', label: nativeDesc.label };
                            case 'date':
                                return { datatype: targetType, control: 'date', label: nativeDesc.label };
                            case 'datetime':
                            case 'datetimeoffset':
                                return { datatype: targetType, control: 'date', label: nativeDesc.label };
                            case 'geography':
                            case 'hierarchyid':
                                return { datatype: targetType, control: 'unsupported', label: nativeDesc.label };
                        }
                    }
                }
            } else if (nativeDesc.datatype === 'bit') {
                if (targetType === 'integral' || targetType === 'decimal') {
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
            } else if (nativeDesc.datatype === 'integral') {
                if (targetType === 'decimal') {
                    return {
                        datatype: targetType,
                        control: 'number',
                        minDecimalPlaces: 0,
                        maxDecimalPlaces: 0,
                        label: nativeDesc.label
                    };
                }
            }
        }

        function getNativeDesc(ex: QueryexBase): PropDescriptor {

            if (ex instanceof QueryexColumnAccess) {
                const entityDesc = entityDescriptorImpl(ex.path, coll, defId, wss, trx);

                // Special case for foreign keys
                {
                    const navPropDesc = getNavPropertyFromForeignKey(entityDesc, ex.property);
                    if (!!navPropDesc) {
                        const fkDesc = entityDesc.properties[ex.property];
                        // All nav props without FKs in the metadata have integral FKs
                        const datatype = !!fkDesc ? fkDesc.datatype as 'string' | 'integral' : 'integral';
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
                if (ex.property === 'Id' && (propDesc.datatype === 'integral' || propDesc.datatype === 'string')) {
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
                        if (ex.arguments.length < 1 || ex.arguments.length > 2) {
                            throw new Error(`No overload for function '${ex.name}' accepts ${ex.arguments.length} arguments.`);
                        }

                        const arg1 = ex.arguments[0];
                        if (ex.arguments.length >= 2) {
                            const arg2 = ex.arguments[1];
                            const arg2Desc = getNativeDesc(arg2);
                            if (arg2Desc.datatype !== 'boolean') {
                                throw new Error(`Function '${ex.name}': The second argument ${arg2} could not be interpreted as a boolean.`);
                            }
                        }

                        let resultDesc: PropDescriptor;
                        if (nameLower === 'count' || nameLower === 'min' || nameLower === 'max') {
                            resultDesc = getNativeDesc(arg1);
                            if (resultDesc.datatype === 'boolean' || resultDesc.datatype === 'entity') {
                                throw new Error(`Function '${ex.name}': The first argument ${arg1} cannot have type ${resultDesc.datatype}.`);
                            }

                            if (nameLower === 'count') {
                                resultDesc = {
                                    datatype: 'integral',
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
                            resultDesc = tryGetDesc(arg1, 'decimal');
                            resultDesc = { ...resultDesc };
                            if (!resultDesc) {
                                throw new Error(`Function '${ex.name}': The first argument ${arg1} could not be interpreted as a numeric.`);
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
                        const arg1Desc = tryGetDesc(arg1, 'date') || tryGetDesc(arg1, 'datetime') || tryGetDesc(arg1, 'datetimeoffset');
                        if (!!arg1Desc) {
                            let calendar: Calendar = 'GR'; // Gregorian
                            if (ex.arguments.length >= 2) {
                                const arg2 = ex.arguments[1];
                                if (arg2 instanceof QueryexQuote) {
                                    calendar = arg2.value.toUpperCase() as Calendar;
                                    if (calendarsArray.indexOf(calendar) < 0) {
                                        throw new Error(`Function '${ex.name}': The second argument ${ex.arguments[1]} must be one of the supported calendars: '${calendarsArray.join(`', '`)}'.`);
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
                                        datatype: 'integral',
                                        control: 'number',
                                        label,
                                        minDecimalPlaces: 0,
                                        maxDecimalPlaces: 0
                                    };
                                case 'quarter':
                                    return {
                                        datatype: 'integral',
                                        control: 'choice',
                                        label,
                                        choices: [1, 2, 3, 4],
                                        format: (c: number | string) => !c ? '' : trx.instant(`ShortQuarter${c}`)
                                    };
                                case 'month':
                                    if (calendar === 'GR') {
                                        return {
                                            datatype: 'integral',
                                            control: 'choice',
                                            label,
                                            choices: [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12],
                                            format: (c: number | string) => !c ? '' : trx.instant(`ShortMonth${c}`)
                                        };
                                    } else if (calendar === 'UQ') {
                                        return {
                                            datatype: 'integral',
                                            control: 'choice',
                                            label,
                                            choices: [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12],
                                            format: (c: number | string) => !c ? '' : trx.instant(`ShortMonthUq${c}`)
                                        };
                                    } else if (calendar === 'ET') {
                                        return {
                                            datatype: 'integral',
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
                                        datatype: 'integral',
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
                        const arg1Desc = tryGetDesc(arg1, 'integral');
                        if (!!arg1Desc) {
                            // Hopefully not a nullable expression
                        } else {
                            throw new Error(`Function '${ex.name}': The first argument ${arg1} could not be interpreted as a numeric.`);
                        }

                        // Arg #2 Date
                        const arg2 = ex.arguments[1];
                        const arg2Desc = tryGetDesc(arg2, 'date') || tryGetDesc(arg2, 'datetime') || tryGetDesc(arg2, 'datetimeoffset');
                        if (!arg2Desc) {
                            throw new Error(`Function '${ex.name}': The first argument ${arg2} could not be interpreted as a date, datetime or datetimeoffset.`);
                        }

                        // Arg #3 Calendar
                        let calendar: Calendar = 'GR'; // Gregorian
                        if (ex.arguments.length >= 3) {
                            const arg3 = ex.arguments[2];
                            if (arg3 instanceof QueryexQuote) {
                                calendar = arg3.value.toUpperCase() as Calendar;
                                if (calendarsArray.indexOf(calendar) < 0) {
                                    throw new Error(`Function '${ex.name}': The second argument ${ex.arguments[1]} must be one of the supported calendars: '${calendarsArray.join(`', '`)}'.`);
                                }
                            } else {
                                throw new Error(`Function '${ex.name}': The second argument must be a simple quote like this: 'UQ'.`);
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
                        const arg1Desc = tryGetDesc(arg1, 'boolean');
                        if (!!arg1Desc) {
                            return { ...arg1Desc };

                        } else {
                            throw new Error(`Function '${ex.name}': The first argument ${arg1} could not be interpreted as a boolean.`);
                        }
                    }

                    case 'if': {
                        const expectedArgCount = 3;
                        if (ex.arguments.length !== expectedArgCount) {
                            throw new Error(`Function '${ex.name}' accepts exactly ${expectedArgCount} argument(s).`);
                        }

                        const arg1 = ex.arguments[0];
                        const arg1Desc = tryGetDesc(arg1, 'boolean');
                        if (!arg1Desc) {
                            throw new Error(`Function '${ex.name}': The first argument ${arg1} could not be interpreted as a boolean.`);
                        }

                        const arg2 = ex.arguments[1];
                        let arg2Desc = getNativeDesc(arg2);
                        const arg2Type = arg2Desc.datatype;
                        if (arg2Type === 'boolean' || arg2Type === 'entity') {
                            throw new Error(`Function '${ex.name}': The second argument ${arg2} cannot have a type ${arg2Type}.`);
                        }

                        const arg3 = ex.arguments[2];
                        let arg3Desc = getNativeDesc(arg3);
                        const arg3Type = arg3Desc.datatype;
                        if (arg3Type === 'boolean' || arg3Type === 'entity') {
                            throw new Error(`Function '${ex.name}': The third argument ${arg3} cannot have a type ${arg3Type}.`);
                        }

                        if (precedence(arg2Type) > precedence(arg3Type)) {
                            arg2Desc = tryGetDesc(arg2, arg3Desc);
                        } else if (precedence(arg3Type) > precedence(arg2Type)) {
                            arg3Desc = tryGetDesc(arg3, arg2Desc);
                        }

                        if (!arg2Desc || !arg3Desc) {
                            throw new Error(`Function '${ex.name}' cannot be used on expressions ${arg2} and ${arg3} because they have incompatible data types.`);
                        }

                        const result = merge(arg2Desc, arg3Desc, trx);
                        result.label = () => trx.instant('Expression');
                        return result;
                    }

                    case 'isnull': {
                        const expectedArgCount = 2;
                        if (ex.arguments.length !== expectedArgCount) {
                            throw new Error(`Function '${ex.name}' accepts exactly ${expectedArgCount} argument(s).`);
                        }

                        const arg1 = ex.arguments[0];
                        let arg1Desc = getNativeDesc(arg1);
                        const arg1Type = arg1Desc.datatype;
                        if (arg1Type === 'boolean' || arg1Type === 'entity') {
                            throw new Error(`Function '${ex.name}': The first argument ${arg1} cannot have type ${arg1Type}.`);
                        }

                        const arg2 = ex.arguments[1];
                        let arg2Desc = getNativeDesc(arg2);
                        const arg2Type = arg2Desc.datatype;
                        if (arg2Type === 'boolean' || arg2Type === 'entity') {
                            throw new Error(`Function '${ex.name}': The second argument ${arg2} cannot have type ${arg2Type}.`);
                        }

                        if (precedence(arg1Type) > precedence(arg2Type)) {
                            arg1Desc = tryGetDesc(arg1, arg2Desc);
                        } else if (precedence(arg2Type) > precedence(arg1Type)) {
                            arg2Desc = tryGetDesc(arg2, arg1Desc);
                        }

                        if (!arg1Desc || !arg2Desc) {
                            throw new Error(`Function '${ex.name}' cannot be used on expressions ${arg1} and ${arg2} because they have incompatible data types.`);
                        }

                        const result = merge(arg1Desc, arg2Desc, trx);
                        result.label = () => trx.instant('Expression');
                        return result;
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
                            datatype: 'datetimeoffset',
                            control: 'datetime',
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
                        let leftDesc = tryGetDesc(ex.left, 'integral');
                        let rightDesc = tryGetDesc(ex.left, 'integral');

                        if (!leftDesc || !rightDesc) {
                            leftDesc = tryGetDesc(ex.left, 'string');
                            rightDesc = tryGetDesc(ex.left, 'string');

                            if (!leftDesc || !rightDesc) {
                                throw new Error(`Operator '${ex.operator}' cannot be used on expressions ${ex.left} and ${ex.right} because they have incompatible data types.`);
                            }
                        }

                        return merge(leftDesc, rightDesc, trx);
                    }

                    case '-':
                    case '*':
                    case '/':
                    case '%': {
                        const leftDesc = tryGetDesc(ex.left, 'integral');
                        if (!leftDesc) {
                            throw new Error(`Operator '${ex.operator}': Left operand ${ex.left} could not be interpreted as a numeric.`);
                        }

                        const rightDesc = tryGetDesc(ex.left, 'integral');
                        if (!rightDesc) {
                            throw new Error(`Operator '${ex.operator}': Right operand ${ex.right} could not be interpreted as a numeric.`);
                        }

                        return merge(leftDesc, rightDesc, trx);
                    }

                    case '&&':
                    case '||':
                    case 'and':
                    case 'or': {
                        const leftDesc = tryGetDesc(ex.left, 'boolean');
                        if (!leftDesc) {
                            throw new Error(`Operator '${ex.operator}': Left operand ${ex.left} could not be interpreted as a boolean.`);
                        }

                        const rightDesc = tryGetDesc(ex.left, 'boolean');
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
                        let leftDesc = getNativeDesc(left);
                        const leftType = leftDesc.datatype;
                        if (leftType === 'boolean' || leftType === 'entity') {
                            throw new Error(`Operator '${ex.operator}': The left operand ${left} cannot have type ${leftType}.`);
                        }

                        const right = ex.right;
                        let rightDesc = getNativeDesc(right);
                        const rightType = rightDesc.datatype;
                        if (rightType === 'boolean' || rightType === 'entity') {
                            throw new Error(`Operator '${ex.operator}': The right operand ${right} cannot have type ${rightType}.`);
                        }

                        if (precedence(leftType) > precedence(rightType)) {
                            leftDesc = tryGetDesc(left, rightDesc);
                        } else if (precedence(rightType) > precedence(leftType)) {
                            rightDesc = tryGetDesc(right, leftDesc);
                        }

                        if (!leftDesc || !rightDesc) {
                            throw new Error(`Operator '${ex.operator}' cannot be used on expressions ${left} and ${right} because they have incompatible data types.`);
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
                        let leftDesc = getNativeDesc(left);
                        const leftType = leftDesc.datatype;
                        if (leftType === 'boolean' || leftType === 'entity') {
                            throw new Error(`Operator '${ex.operator}': The left operand ${left} cannot have type ${leftType}.`);
                        }

                        const right = ex.right;
                        let rightDesc = getNativeDesc(right);
                        const rightType = rightDesc.datatype;
                        if (rightType === 'boolean' || rightType === 'entity') {
                            throw new Error(`Operator '${ex.operator}': The right operand ${right} cannot have type ${rightType}.`);
                        }

                        if (precedence(leftType) > precedence(rightType)) {
                            leftDesc = tryGetDesc(left, rightDesc);
                        } else if (precedence(rightType) > precedence(leftType)) {
                            rightDesc = tryGetDesc(right, leftDesc);
                        }

                        if (!leftDesc || !rightDesc) {
                            throw new Error(`Operator '${ex.operator}' cannot be used on expressions ${left} and ${right} because they have incompatible data types.`);
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
                        const leftDesc = tryGetDesc(ex.left, 'string');
                        if (!leftDesc) {
                            throw new Error(`Operator '${ex.operator}': Left operand ${ex.left} could not be interpreted as string.`);
                        }

                        const rightDesc = tryGetDesc(ex.right, 'string');
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
                        const desc = tryGetDesc(ex.operand, 'integral');
                        if (!desc) {
                            throw new Error(`Operator '${ex.operator}': Operand ${ex.operand} could not be interpreted as numeric.`);
                        }

                        return { ...desc, label: () => trx.instant('Expression') };
                    }

                    case '!':
                    case 'not': {
                        const desc = tryGetDesc(ex.operand, 'boolean');
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
                    datatype: 'integral',
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
            } else if (ex instanceof QueryexPlaceholder) {
                throw Error(`[Bug] Requesting descriptor before replacing placeholders.`);
            } else if (ex instanceof QueryexParameter) {
                // TODO
                return {
                    datatype: 'null',
                    control: 'null',
                    label: () => ''
                };
            } else {
                throw Error(`[Bug] ${ex} Has an unknown Queryex type.`);
            }
        }

        return getNativeDesc(expression);
    }
}
