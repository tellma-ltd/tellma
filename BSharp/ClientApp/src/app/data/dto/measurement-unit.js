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
var dto_for_save_key_base_1 = require("./dto-for-save-key-base");
var MeasurementUnitForSave = /** @class */ (function (_super) {
    __extends(MeasurementUnitForSave, _super);
    function MeasurementUnitForSave() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    return MeasurementUnitForSave;
}(dto_for_save_key_base_1.DtoForSaveKeyBase));
exports.MeasurementUnitForSave = MeasurementUnitForSave;
var MeasurementUnit = /** @class */ (function (_super) {
    __extends(MeasurementUnit, _super);
    function MeasurementUnit() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    return MeasurementUnit;
}(MeasurementUnitForSave));
exports.MeasurementUnit = MeasurementUnit;
// Choice list (Also repeated in measurement units master template)
exports.MeasurementUnit_UnitType = {
    'Pure': 'MU_Pure',
    'Time': 'MU_Time',
    'Distance': 'MU_Distance',
    'Count': 'MU_Count',
    'Mass': 'MU_Mass',
    'Volume': 'MU_Volume',
    'Money': 'MU_Money'
};
