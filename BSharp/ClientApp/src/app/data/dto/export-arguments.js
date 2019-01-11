"use strict";
var __extends = (this && this.__extends) || (function () {
    var extendStatics = function (d, b) {
        extendStatics = Object.setPrototypeOf ||
            ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
            function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
        return extendStatics(d, b);
    };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
exports.__esModule = true;
var get_arguments_1 = require("./get-arguments");
var ExportArguments = /** @class */ (function (_super) {
    __extends(ExportArguments, _super);
    function ExportArguments() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    return ExportArguments;
}(get_arguments_1.GetArguments));
exports.ExportArguments = ExportArguments;
exports.ExportArguments_Format = {
    'xlsx': 'Excel',
    'csv': 'CSV'
};
