class OperatorInfo {
    constructor(public precedence: number, public associativity: 'left' | 'right') { }

    public get isLeftAssociative(): boolean {
        return this.associativity === 'left';
    }
}

const symbols = [
    // Comparison Operators
    '!=', '<>', '<=', '>=', '<', '>', '=',

    // Logical Operators
    '&&', '||', '!',

    // Brackets and comma
    '(', ')', ',',

    // Arithmetic Operators
    '+', '-', '*', '/', '%',

    // String Operators (for backward compatibility)
    'contains', 'startsw', 'endsw',

    // Tree Operators (for backward compatibility)
    'descof',

    // Logical Operators (for backward compatibility)
    'not', 'and', 'or',

    // Comparison Operators (for backward compatibility)
    'gt', 'ge', 'lt', 'le', 'eq', 'ne',

    // Directions
    'asc', 'desc',
];

const _operatorInfos: { [op: string]: OperatorInfo } = {
    // Arithmetic Opreators (take 2 numbers and return a number)
    ['*']: new OperatorInfo(2, 'left'),
    ['/']: new OperatorInfo(2, 'left'),
    ['%']: new OperatorInfo(2, 'left'),
    ['+']: new OperatorInfo(3, 'left'),
    ['-']: new OperatorInfo(3, 'left'),

    // Comparison Operators (take 2 objects and return a boolean)
    ['=']: new OperatorInfo(4, 'left'),
    ['!=']: new OperatorInfo(4, 'left'),
    ['<>']: new OperatorInfo(4, 'left'),
    ['<=']: new OperatorInfo(4, 'left'),
    ['<']: new OperatorInfo(4, 'left'),
    ['>']: new OperatorInfo(4, 'left'),
    ['>=']: new OperatorInfo(4, 'left'),
    ['eq']: new OperatorInfo(4, 'left'),
    ['ne']: new OperatorInfo(4, 'left'),
    ['le']: new OperatorInfo(4, 'left'),
    ['lt']: new OperatorInfo(4, 'left'),
    ['gt']: new OperatorInfo(4, 'left'),
    ['ge']: new OperatorInfo(4, 'left'),

    // Infix functions
    ['contains']: new OperatorInfo(4, 'left'),
    ['startsw']: new OperatorInfo(4, 'left'),
    ['endsw']: new OperatorInfo(4, 'left'),
    ['descof']: new OperatorInfo(4, 'left'),

    // Logical Operators
    ['!']: new OperatorInfo(5, 'left'),
    ['&&']: new OperatorInfo(6, 'left'),
    ['||']: new OperatorInfo(7, 'left'),
    ['not']: new OperatorInfo(5, 'left'),
    ['and']: new OperatorInfo(6, 'left'),
    ['or']: new OperatorInfo(7, 'left'),
};

function isAlphabeticOperator(op: string) {
    switch (op) {
        case 'eq':
        case 'ne':
        case 'le':
        case 'lt':
        case 'gt':
        case 'ge':
        case 'contains':
        case 'startsw':
        case 'endsw':
        case 'descof':
        case 'not':
        case 'and':
        case 'or':
        case 'asc':
        case 'desc':
            return true;
        default:
            return false;
    }
}

function tryGetOperatorInfo(op: string): OperatorInfo {
    return _operatorInfos[op.toLowerCase()];
}

function validUnaryOperator(op: string) {
    switch (op.toLowerCase()) {
        case '-':
        case '+':
        case '!':
        case 'not':
            return true;
        default:
            return false;
    }
}

function validBinaryOperator(op: string) {
    switch (op.toLowerCase()) {
        case '!':
        case 'not':
            return false;
        default:
            return true;
    }
}

function isDirectionKeyword(token: string): QueryexDirection {
    token = (token || '').toLowerCase();
    switch (token) {
        case 'asc':
        case 'desc':
            return token;
        default:
            return null;
    }
}

export class Queryex {

    public static parseSingle(
        expressionString: string,
        options?: {
            expectDirKeywords?: boolean,
            expectPathsOnly?: boolean,
            placeholderReplacement?: QueryexBase
        }) {

        const expArray = Queryex.parse(expressionString, options);
        if (expArray.length > 1) {
            throw new Error(`Expression cannot contain top level commas.`);
        } else if (expArray.length === 1) {
            return expArray[0];
        }
    }

    public static parse(
        expressionString: string,
        options?: {
            expectDirKeywords?: boolean,
            expectPathsOnly?: boolean,
            placeholderReplacement?: QueryexBase
        }): QueryexBase[] {

        if (!expressionString || !expressionString.trim()) {
            return [];
        }

        const opt = options || { expectDirKeywords: false, expectPathsOnly: false };

        const tokenStream = Queryex.tokenize(expressionString);
        return Queryex.parseTokenStream(tokenStream, expressionString,
            !!opt.expectDirKeywords,
            !!opt.expectPathsOnly,
            opt.placeholderReplacement);
    }

    private static tokenize(expressionString: string): string[] {
        const result: string[] = [];

        const expArray: string[] = expressionString.split('');
        let insideQuotes = false;
        let acc: string[] = [];
        let index = 0;

        function tryMatchSymbol(i: number): string {
            let matchingSymbol = symbols.find(symbol => (expArray.length - i) >= symbol.length &&
                Array.from(Array(symbol.length).keys()).every(j => symbol[j] === expArray[i + j].toLowerCase()));

            // TODO: all alphabetic operators
            if (isAlphabeticOperator(matchingSymbol)) {
                const prevIndex = i - 1;
                const precededProperly = prevIndex < 0 || !QueryexColumnAccess.properChar(expArray[prevIndex]);
                const nextIndex = i + matchingSymbol.length;
                const followedProperly = nextIndex >= expArray.length || !QueryexColumnAccess.properChar(expArray[nextIndex]);

                if (!precededProperly || !followedProperly) {
                    matchingSymbol = null;
                }
            }

            if (!!matchingSymbol) {
                matchingSymbol = expArray.slice(i, i + matchingSymbol.length).join('');
                return matchingSymbol;
            }
        }

        while (index < expArray.length) {
            const isSingleQuote = expArray[index] === `'`;
            if (isSingleQuote) {
                const followedBySingleQuote = (index + 1) < expArray.length && expArray[index + 1] === `'`;

                acc.push(expArray[index]);
                index++;

                if (!insideQuotes) {
                    insideQuotes = true;
                } else if (!followedBySingleQuote) {
                    insideQuotes = false;
                } else {
                    index++; // inside quotes and followed by a single quote
                }
            } else {
                let matchingSymbol: string;
                // tslint:disable:no-conditional-assignment
                if (!insideQuotes && !!(matchingSymbol = tryMatchSymbol(index))) {
                    if (acc.length > 0) {
                        const token = acc.join('').trim();
                        if (!!token) {
                            result.push(token);
                        }
                        acc = [];
                    }

                    result.push(matchingSymbol.trim());
                    index += matchingSymbol.length;
                } else {
                    acc.push(expArray[index]);
                    index++;
                }
            }
        }

        if (insideQuotes) {
            throw new Error(`Uneven number of single quotation marks in ${expressionString},
quotation marks in string literals should be escaped by specifying them twice.`);
        }

        if (acc.length > 0) {
            const token = acc.join('').trim();
            if (!!token) {
                result.push(token);
            }
        }

        return result;
    }

    private static parseTokenStream(
        tokens: string[],
        expressionString: string,
        expectDirKeywords: boolean,
        expectPathsOnly: boolean,
        placeholderReplacement: QueryexBase): QueryexBase[] {

        const result: QueryexBase[] = [];

        const ops: { op: string, usedAsUnaryOperator: boolean }[] = [];
        const brackets: BracketInfo[] = [];
        const output: QueryexBase[] = [];

        function popOperatorToOutput() {
            const { op, usedAsUnaryOperator } = ops.pop();
            let exp: QueryexBase;
            if (usedAsUnaryOperator) {
                if (!validUnaryOperator(op)) {
                    throw new Error(`Infix operator '${op}' is missing its first operand.`);
                } else if (output.length < 1) {
                    throw new Error(`Unary operator '${op}' is missing its operand.`);
                } else {
                    const inner = output.pop();
                    exp = new QueryexUnaryOperator(op, inner);
                }
            } else {
                if (!validBinaryOperator(op)) {
                    throw new Error(`Unary Operator '${op}' is used like an infix operator.`);
                } else if (output.length < 2) {
                    throw new Error(`Infix operator '${op}' is missing its second operand.`);
                } else {
                    const right = output.pop();
                    const left = output.pop();
                    exp = new QueryexBinaryOperator(op, left, right);
                }
            }

            output.push(exp);
        }

        function incrementArity() {
            if (brackets.length > 0) {
                const peek = brackets[brackets.length - 1];
                if (peek.isFunction && peek.arity === peek.arguments.length) {
                    peek.arity++;
                }
            }
        }

        function terminateFunctionArgument(bracketsInfo: BracketInfo) {
            if (bracketsInfo.arity > 0) {
                if (bracketsInfo.arguments.length !== bracketsInfo.arity - 1) {
                    throw new Error(`Blank function arguments are not allowed, pass null if that was your intention.`);
                }

                if (output.length === 0) {
                    throw new Error(`[Bug] Output stack is empty.`);
                }

                // Move the argument from the stack to the brackets info, this is in order to parse something like F(1, 2, +) correctly
                bracketsInfo.arguments.push(output.pop());
            }
        }

        let currentTokenIsPotentialFunction = false;
        let previousTokenIsPotentialFunction = false;
        let expressionTerminated = false;
        let previousToken: string;

        function currentTokenUsedLikeAPrefix(): boolean {
            return previousToken == null || previousToken === ',' || previousToken === '(' || !!tryGetOperatorInfo(previousToken);
        }

        function terminateCurrentExpression(): void {
            expressionTerminated = true;

            while (ops.length > 0) {
                if (ops[ops.length - 1].op !== '(') {
                    popOperatorToOutput();
                } else {
                    throw new Error(`Expression contains mismatched brackets: ${expressionString}.`);
                }
            }

            if (output.length === 0) {
                return;
            } else if (output.length > 1) {
                throw new Error(`Incorrectly formatted expression: ${expressionString}.`);
            }

            const res = output.pop();

            let dir: QueryexDirection;
            if (dir = isDirectionKeyword(previousToken)) {
                res.direction = dir;
            }

            result.push(res);
        }

        for (const currentToken of tokens) {
            // Shunting-yard implementation
            previousTokenIsPotentialFunction = currentTokenIsPotentialFunction;
            currentTokenIsPotentialFunction = false;
            expressionTerminated = false;

            let opInfo: OperatorInfo;
            if (opInfo = tryGetOperatorInfo(currentToken)) {
                incrementArity();

                const usedAsUnaryOperator = currentTokenUsedLikeAPrefix();
                if (!usedAsUnaryOperator) {
                    const keepPopping: () => boolean = () => {
                        if (ops.length === 0) {
                            return false;
                        }

                        const { op: opsPeek } = ops[ops.length - 1];
                        const opsPeekInfo = tryGetOperatorInfo(opsPeek);
                        const isOperator = !!opsPeekInfo;
                        const isFunction = opsPeek !== '(' && !isOperator;

                        return opsPeek !== '(' &&
                            (
                                isFunction ||
                                opsPeekInfo.precedence < opInfo.precedence ||
                                (opsPeekInfo.precedence === opInfo.precedence && opsPeekInfo.isLeftAssociative)
                            );
                    };

                    while (keepPopping()) {
                        popOperatorToOutput();
                    }
                }

                // Finally, push the token (and it's usage on the stack)
                currentTokenIsPotentialFunction = usedAsUnaryOperator && QueryexFunction.isValidFunctionName(currentToken);
                ops.push({ op: currentToken, usedAsUnaryOperator });
            } else if (currentToken === '(') {
                if (previousTokenIsPotentialFunction) {
                    if (!!tryGetOperatorInfo(previousToken)) {
                        // Nothing to do
                    } else {
                        output.pop();
                        ops.push({ op: previousToken, usedAsUnaryOperator: false });
                    }
                } else {
                    incrementArity();
                }

                brackets.push(new BracketInfo(previousTokenIsPotentialFunction));
                ops.push({ op: currentToken, usedAsUnaryOperator: false });
            } else if (currentToken === ')') {
                while (ops.length > 0 && ops[ops.length - 1].op !== '(') {
                    popOperatorToOutput();
                }

                if (ops.length > 0 && ops[ops.length - 1].op === '(') {
                    ops.pop(); // Left paren
                    const bracketsInfo = brackets.pop();
                    if (bracketsInfo.isFunction) {
                        terminateFunctionArgument(bracketsInfo);

                        const { op: functionName } = ops.pop();

                        const func = new QueryexFunction(functionName, bracketsInfo.arguments);
                        if (func.isAggregation && func.arguments.some(e => e.aggregations().length > 0)) {
                            throw new Error(`The expression ${func.toString()} contains an aggregation within an aggregation.`);
                        }

                        output.push(func);

                    } else if (previousToken === '(') {
                        throw new Error(`Invalid empty brackets ().`);
                    }
                } else {
                    throw new Error(`Expression contains mismatched brackets.`);
                }
            } else if (currentToken === ',') {
                if (brackets.length === 0) {
                    terminateCurrentExpression();
                } else if (brackets[brackets.length - 1].isFunction) {
                    while (ops.length > 0 && ops[ops.length - 1].op !== '(') {
                        popOperatorToOutput();
                    }

                    const bracketsInfo = brackets[brackets.length - 1];
                    terminateFunctionArgument(bracketsInfo);
                } else {
                    // tslint:disable-next-line:max-line-length
                    throw new Error(`Unexpected comma ',' character. Commas are only used to separate function arguments: Func(arg1, arg2, arg3).`);
                }
            } else if (!!isDirectionKeyword(currentToken)) {
                if (!expectDirKeywords) {
                    throw new Error(`Unexpected keyword '${currentToken}' in expression: ${expressionString}`);
                }
            } else {
                incrementArity();

                currentTokenIsPotentialFunction = currentTokenUsedLikeAPrefix() && QueryexFunction.isValidFunctionName(currentToken);

                let exp: QueryexBase;
                const tokenLower = currentToken.toLowerCase();
                switch (tokenLower) {
                    case '$':
                        if (!placeholderReplacement) {
                            throw new Error(`Unrecognized token: ${currentToken}`);
                        }
                        exp = placeholderReplacement.clone();
                        break;
                    case 'null':
                        exp = new QueryexNull();
                        break;
                    case 'true':
                    case 'false':
                        exp = new QueryexBit(tokenLower === 'true');
                        break;
                    case 'me':
                    case 'today':
                    case 'now':
                        exp = new QueryexFunction(tokenLower, []);
                        break;

                    default:

                        const quoteValue = QueryexQuote.isValidQuote(currentToken);
                        if (quoteValue !== undefined) {
                            exp = new QueryexQuote(quoteValue);
                            break;
                        }

                        const pair = QueryexNumber.isValidNumber(currentToken);
                        if (pair !== undefined) {
                            const { value, decimals } = pair;
                            exp = new QueryexNumber(value, decimals);
                            break;
                        }

                        const { pathArray, propName } = QueryexColumnAccess.isValidColumnAccess(currentToken, expectPathsOnly);
                        if (!!pathArray) {
                            exp = new QueryexColumnAccess(pathArray, propName);
                            break;
                        }

                        const paramName = QueryexParameter.isValidParameterName(currentToken);
                        if (!!paramName) {
                            exp = new QueryexParameter(paramName);
                            break;
                        }

                        throw new Error(`Unrecognized token: ${currentToken}`);
                }

                output.push(exp);
            }

            if (!!isDirectionKeyword(previousToken) && !expressionTerminated) {
                const keyword = previousToken.toLowerCase();
                // tslint:disable-next-line:max-line-length
                throw new Error(`Keyword '${keyword}' must come after the expression and outside any brackets like this: <exp1> ${keyword}, <exp2> ${keyword}.`);

            }

            previousToken = currentToken;
        }

        terminateCurrentExpression();
        return result;
    }
}

class BracketInfo {
    constructor(public isFunction: boolean) {
    }

    public arity = 0;
    public arguments: QueryexBase[] = [];
}

export abstract class QueryexBase {
    public direction: QueryexDirection;
    public get isAscending(): boolean { return this.direction === 'asc'; }
    public get isDescending(): boolean { return this.direction === 'desc'; }

    public unaggregatedColumnAccesses(): QueryexColumnAccess[] {
        const result: QueryexColumnAccess[] = [];
        this.unaggregatedColumnAccessesInner(result, false);
        return result;
    }

    public columnAccesses(): QueryexColumnAccess[] {
        const result: QueryexColumnAccess[] = [];
        this.columnAccessesInner(result);
        return result;
    }

    public aggregations(): QueryexFunction[] {
        const result: QueryexFunction[] = [];
        this.aggregationsInner(result);
        return result;
    }

    public parameters(): QueryexParameter[] {
        const result: QueryexParameter[] = [];
        this.parametersInner(result);
        return result;
    }

    public get isAggregation(): boolean {
        if (this instanceof QueryexFunction) {
            switch (this.nameLower) {
                case 'sum':
                case 'count':
                case 'avg':
                case 'max':
                case 'min':
                    return true;
                default:
                    return false;
            }
        }

        return false;
    }

    public abstract toString(): string;
    public abstract children(): QueryexBase[];
    public abstract unaggregatedColumnAccessesInner(result: QueryexColumnAccess[], aggregated: boolean): void;
    public abstract columnAccessesInner(result: QueryexColumnAccess[]): void;
    public abstract aggregationsInner(result: QueryexFunction[]): void;
    public abstract parametersInner(result: QueryexParameter[]): void;
    public abstract clone(): QueryexBase;
}

export class QueryexColumnAccess extends QueryexBase {
    hasSecondary?: boolean; // Column access of a bi-lingual property
    hasTernary?: boolean; // Column access of a tri-lingual prop

    constructor(public path: string[], public property: string) {
        super();
    }

    private static properFirstChar(token: string): boolean {
        return !!token && isLetter(token[0]);
    }

    public static properChar(c: string) {
        return isLetterOrDigit(c) || c === '_' || c === '.';
    }

    private static properChars(token: string): boolean {
        return !!token && token.split('').every(QueryexColumnAccess.properChar);
    }

    private static notReservedKeyword(token: string): boolean {
        switch (token.toLowerCase()) {
            case 'null':
            case 'true':
            case 'false':
            case 'asc':
            case 'desc':
                return false;
            default:
                return true;
        }
    }

    public static isValidColumnAccess(token: string, expectPathsOnly: boolean): { pathArray?: string[], propName?: string } {
        const match = this.properFirstChar(token) && this.properChars(token) && this.notReservedKeyword(token);
        if (match) {
            const steps = token
                .split('.')
                .map(e => e.trim())
                .filter(e => !!e);

            if (expectPathsOnly) {
                return { pathArray: steps };
            } else {
                const propName = steps.pop();
                return { pathArray: steps, propName };
            }
        }

        return {};
    }

    public toString(): string {
        const path = this.path.join('.') || '';
        const prop = this.property || '';
        const dot = !path || !prop ? '' : '.';

        return `${path}${dot}${prop}`;
    }

    public children(): QueryexBase[] {
        return [];
    }

    public unaggregatedColumnAccessesInner(result: QueryexColumnAccess[], aggregated: boolean) {
        if (!aggregated) {
            result.push(this);
        }
    }

    public columnAccessesInner(result: QueryexColumnAccess[]) {
        result.push(this);
    }

    public aggregationsInner(_: QueryexFunction[]) {
    }

    public parametersInner(_: QueryexParameter[]) {
    }

    public clone(): QueryexBase {
        const clone = new QueryexColumnAccess(this.path.slice(), this.property);
        clone.hasSecondary = this.hasSecondary;
        clone.hasTernary = this.hasTernary;

        return clone;
    }
}

export class QueryexFunction extends QueryexBase {
    arguments: QueryexBase[];
    nameLower: string;

    index?: number; // For aggregation functions
    sumIndex?: number; // For AVG
    countIndex?: number; // For AVG

    private static properFirstChar(token: string): boolean {
        return !!token && isLetter(token[0]);
    }

    private static properChars(token: string): boolean {
        return !!token && token.split('')
            .every(c => isLetterOrDigit(c) || c === '_');
    }

    private static notReservedKeyword(token: string): boolean {
        switch (token.toLowerCase()) {
            case 'null':
            case 'true':
            case 'false':
            case 'asc':
            case 'desc':
                return false;
            default:
                return true;
        }
    }

    public static isValidFunctionName(token: string): boolean {
        return this.properFirstChar(token) && this.properChars(token) && this.notReservedKeyword(token);
    }

    constructor(public name: string, args: QueryexBase[]) {
        super();
        this.arguments = args; // Typescript complained when I called constructor parameter "arguments"
        this.nameLower = name.toLowerCase();
    }

    public toString(): string {
        return `${this.name}(${this.arguments.map(a => DeBracket(a.toString())).join(', ')})`;
    }

    public children(): QueryexBase[] {
        return this.arguments;
    }

    public unaggregatedColumnAccessesInner(result: QueryexColumnAccess[], aggregated: boolean) {
        if (this.isAggregation) {
            aggregated = true;
        }

        for (const arg of this.arguments) {
            arg.unaggregatedColumnAccessesInner(result, aggregated);
        }
    }

    public columnAccessesInner(result: QueryexColumnAccess[]) {
        for (const arg of this.arguments) {
            arg.columnAccessesInner(result);
        }
    }

    public aggregationsInner(result: QueryexFunction[]) {
        if (this.isAggregation) {
            result.push(this);
        }

        for (const arg of this.arguments) {
            arg.aggregationsInner(result);
        }
    }

    public parametersInner(result: QueryexParameter[]) {
        for (const arg of this.arguments) {
            arg.parametersInner(result);
        }
    }

    public clone(): QueryexBase {
        return new QueryexFunction(this.name, this.arguments.map(e => e.clone()));
    }

    public setName(name: string) {
        this.name = name;
        this.nameLower = name.toLowerCase();
    }
}

export class QueryexBinaryOperator extends QueryexBase {
    constructor(public operator: string, public left: QueryexBase, public right: QueryexBase) {
        super();
    }

    public toString(): string {
        return `(${this.left.toString()} ${this.operator} ${this.right.toString()})`;
    }

    public children(): QueryexBase[] {
        return [this.left, this.right];
    }

    public unaggregatedColumnAccessesInner(result: QueryexColumnAccess[], aggregated: boolean) {
        this.left.unaggregatedColumnAccessesInner(result, aggregated);
        this.right.unaggregatedColumnAccessesInner(result, aggregated);
    }

    public columnAccessesInner(result: QueryexColumnAccess[]) {
        this.left.columnAccessesInner(result);
        this.right.columnAccessesInner(result);
    }

    public aggregationsInner(result: QueryexFunction[]) {
        this.left.aggregationsInner(result);
        this.right.aggregationsInner(result);
    }

    public parametersInner(result: QueryexParameter[]) {
        this.left.parametersInner(result);
        this.right.parametersInner(result);
    }

    public clone(): QueryexBase {
        return new QueryexBinaryOperator(this.operator, this.left.clone(), this.right.clone());
    }
}

export class QueryexUnaryOperator extends QueryexBase {
    constructor(public operator: string, public operand: QueryexBase) {
        super();
    }

    public toString(): string {
        return `(${this.operator} ${this.operand.toString()})`;
    }

    public children(): QueryexBase[] {
        return [this.operand];
    }

    public unaggregatedColumnAccessesInner(result: QueryexColumnAccess[], aggregated: boolean) {
        this.operand.unaggregatedColumnAccessesInner(result, aggregated);
    }

    public columnAccessesInner(result: QueryexColumnAccess[]) {
        this.operand.columnAccessesInner(result);
    }

    public aggregationsInner(result: QueryexFunction[]) {
        this.operand.aggregationsInner(result);
    }

    public parametersInner(result: QueryexParameter[]) {
        this.operand.parametersInner(result);
    }

    public clone(): QueryexBase {
        return new QueryexUnaryOperator(this.operator, this.operand.clone());
    }
}

export class QueryexQuote extends QueryexBase {
    constructor(public value: string) {
        super();
    }

    public static isValidQuote(token: string): string {
        if (token.length >= 2 && token.startsWith(`'`) && token.endsWith(`'`)) {
            return token.slice(1, -1);
        }
    }

    public toString(): string {
        return `'${this.value.replace(`'`, `''`)}'`;
    }

    public children(): QueryexBase[] {
        return [];
    }

    public unaggregatedColumnAccessesInner(_: QueryexColumnAccess[], __: boolean) {
    }

    public columnAccessesInner(_: QueryexColumnAccess[]) {
    }

    public aggregationsInner(_: QueryexFunction[]) {
    }

    public parametersInner(_: QueryexParameter[]) {
    }

    public clone(): QueryexBase {
        return new QueryexQuote(this.value);
    }
}

export class QueryexNumber extends QueryexBase {
    constructor(public value: number, public decimals: number) {
        super();
    }

    public static isValidNumber(token: string): { value: number, decimals: number } {
        if (isDigit(token[0]) && isDigit(token[token.length - 1]) && token.split('').every(c => isDigit(c) || c === '.')) {
            const value = parseFloat(token);

            const pieces = token.split('.');
            const decimals = pieces.length <= 1 ? 0 : pieces.pop().length;

            return { value, decimals };
        }
    }

    public toString(): string {
        return this.value.toString();
    }

    public children(): QueryexBase[] {
        return [];
    }

    public unaggregatedColumnAccessesInner(_: QueryexColumnAccess[], __: boolean) {
    }

    public columnAccessesInner(_: QueryexColumnAccess[]) {
    }

    public aggregationsInner(_: QueryexFunction[]) {
    }

    public parametersInner(_: QueryexParameter[]) {
    }

    public clone(): QueryexBase {
        return new QueryexNumber(this.value, this.decimals);
    }
}

export class QueryexNull extends QueryexBase {
    public toString(): string {
        return 'null';
    }

    public children(): QueryexBase[] {
        return [];
    }

    public unaggregatedColumnAccessesInner(_: QueryexColumnAccess[], __: boolean) {
    }

    public columnAccessesInner(_: QueryexColumnAccess[]) {
    }

    public aggregationsInner(_: QueryexFunction[]) {
    }

    public parametersInner(_: QueryexParameter[]) {
    }

    public clone(): QueryexBase {
        return new QueryexNull();
    }
}

export class QueryexBit extends QueryexBase {
    constructor(public value: boolean) {
        super();
    }

    public toString(): string {
        return (!!this.value).toString();
    }

    public children(): QueryexBase[] {
        return [];
    }

    public unaggregatedColumnAccessesInner(_: QueryexColumnAccess[], __: boolean) {
    }

    public columnAccessesInner(_: QueryexColumnAccess[]) {
    }

    public aggregationsInner(_: QueryexFunction[]) {
    }

    public parametersInner(_: QueryexParameter[]) {
    }

    public clone(): QueryexBase {
        return new QueryexBit(this.value);
    }
}

export class QueryexParameter extends QueryexBase {
    public keyLower: string;

    private static properFirstChar(token: string): boolean {
        return !!token && token[0] === '@';
    }

    private static properChars(token: string): boolean {
        return !!token && token.split('').slice(1)
            .every(c => isLetterOrDigit(c) || c === '_');
    }

    public static isValidParameterName(token: string): string {
        if (this.properFirstChar(token) && this.properChars(token)) {
            return token.slice(1);
        }
    }

    constructor(public key: string) {
        super();
        this.keyLower = key.toLowerCase();
    }

    public toString(): string {
        return `@${this.key}`;
    }

    public children(): QueryexBase[] {
        return [];
    }

    public unaggregatedColumnAccessesInner(_: QueryexColumnAccess[], __: boolean) {
    }

    public columnAccessesInner(_: QueryexColumnAccess[]) {
    }

    public aggregationsInner(_: QueryexFunction[]) {
    }

    public parametersInner(result: QueryexParameter[]) {
        result.push(this);
    }

    public clone(): QueryexBase {
        return new QueryexParameter(this.key);
    }
}

export type QueryexDirection = 'asc' | 'desc';

export function DeBracket(str: string) {
    if (!str || str.length < 2) {
        return str;
    }

    let level = 0;
    for (let i = 0; i < str.length - 1; i++) {
        if (str[i] === '(') {
            level++;
        }

        if (str[i] === ')') {
            level--;
        }

        if (level === 0) {
            // There are no enclosing brackets
            return str;
        }
    }

    if (str.endsWith(')')) {
        // Remove the brackets and call recursively
        str = DeBracket(str.slice(1, -1));
    }

    return str;
}

function isDigit(c: string): boolean {
    return c >= '0' && c <= '9';
}

function isLetter(c: string): boolean {
    return c.toLowerCase() !== c.toUpperCase(); // Only true for A-Z
}

function isLetterOrDigit(c: string): boolean {
    return isLetter(c) || isDigit(c);
}
