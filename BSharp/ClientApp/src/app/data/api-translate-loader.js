"use strict";
exports.__esModule = true;
var rxjs_1 = require("rxjs");
var appconfig_1 = require("./appconfig");
var operators_1 = require("rxjs/operators");
// A custom loader for ngx-translate that loads the translation from the API
var ApiTranslateLoader = /** @class */ (function () {
    function ApiTranslateLoader(http) {
        this.http = http;
    }
    ApiTranslateLoader.prototype.getTranslation = function (lang) {
        var baseAddress = appconfig_1.appconfig.apiAddress;
        var url = baseAddress + ("api/translations/client-translations/" + lang);
        console.log('XXXXXXXXXXXXXX ' + url);
        // TODO use local storage to to instantly load the app
        return this.http.get(url)
            .pipe(operators_1.catchError(function (err) {
            console.log('XXXXXXXXXXXXXX ' + err);
            return rxjs_1.throwError(err);
        }));
    };
    return ApiTranslateLoader;
}());
exports.ApiTranslateLoader = ApiTranslateLoader;
function ApiTranslateLoaderFactory(http) {
    return new ApiTranslateLoader(http);
}
exports.ApiTranslateLoaderFactory = ApiTranslateLoaderFactory;
