"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
exports.__esModule = true;
var http_1 = require("@angular/common/http");
var core_1 = require("@angular/core");
var rxjs_1 = require("rxjs");
var operators_1 = require("rxjs/operators");
var appconfig = {
    apiAddress: '',
    identityAddress: ''
};
var ApiService = /** @class */ (function () {
    // Will abstract away standard API calls for CRUD operations
    function ApiService(http, translate) {
        this.http = http;
        this.translate = translate;
        this.saveInProgress = false;
    }
    ApiService.prototype.measurementUnitsApi = function (cancellationToken$) {
        return {
            activate: this.activateFactory('measurement-units', cancellationToken$),
            deactivate: this.deactivateFactory('measurement-units', cancellationToken$)
        };
    };
    ApiService.prototype.crudFactory = function (endpoint, cancellationToken$) {
        var _this = this;
        return {
            get: function (args) {
                var paramsArray = _this.stringifyGetArguments(args);
                var params = paramsArray.join('&');
                var url = appconfig.apiAddress + ("api/" + endpoint + "?" + params);
                var obs$ = _this.http.get(url).pipe(operators_1.catchError(function (error) {
                    var friendlyError = _this.friendly(error);
                    return rxjs_1.throwError(friendlyError);
                }), operators_1.takeUntil(cancellationToken$));
                return obs$;
            },
            getById: function (id, args) {
                args = args || {};
                var paramsArray = [];
                if (!!args.expand) {
                    paramsArray.push("expand=" + encodeURIComponent(args.expand));
                }
                var params = paramsArray.join('&');
                var url = appconfig.apiAddress + ("api/" + endpoint + "/" + id + "?" + params);
                var obs$ = _this.http.get(url).pipe(operators_1.catchError(function (error) {
                    var friendlyError = _this.friendly(error);
                    return rxjs_1.throwError(friendlyError);
                }), operators_1.takeUntil(cancellationToken$));
                return obs$;
            },
            save: function (entities, args) {
                _this.saveInProgress = true;
                args = args || {};
                var paramsArray = [];
                if (!!args.expand) {
                    paramsArray.push("expand=" + encodeURIComponent(args.expand));
                }
                paramsArray.push("returnEntities=" + !!args.returnEntities);
                var params = paramsArray.join('&');
                var url = appconfig.apiAddress + ("api/" + endpoint + "?" + params);
                var obs$ = _this.http.post(url, entities, {
                    headers: new http_1.HttpHeaders({ 'Content-Type': 'application/json' })
                }).pipe(operators_1.tap(function () { return _this.saveInProgress = false; }), operators_1.catchError(function (error) {
                    _this.saveInProgress = false;
                    var friendlyError = _this.friendly(error);
                    return rxjs_1.throwError(friendlyError);
                }), operators_1.takeUntil(cancellationToken$), operators_1.finalize(function () { return _this.saveInProgress = false; }));
                return obs$;
            },
            "delete": function (ids) {
                _this.saveInProgress = true;
                var url = appconfig.apiAddress + ("api/" + endpoint);
                var obs$ = _this.http.request('DELETE', url, { body: ids }).pipe(operators_1.tap(function () { return _this.saveInProgress = false; }), operators_1.catchError(function (error) {
                    _this.saveInProgress = false;
                    var friendlyError = _this.friendly(error);
                    return rxjs_1.throwError(friendlyError);
                }), operators_1.takeUntil(cancellationToken$), operators_1.finalize(function () { return _this.saveInProgress = false; }));
                return obs$;
            },
            template: function (args) {
                args = args || {};
                var paramsArray = [];
                if (!!args.format) {
                    paramsArray.push("format=" + args.format);
                }
                var params = paramsArray.join('&');
                var url = appconfig.apiAddress + ("api/" + endpoint + "/template?" + params);
                var obs$ = _this.http.get(url, { responseType: 'blob' }).pipe(operators_1.catchError(function (error) {
                    var friendlyError = _this.friendly(error);
                    return rxjs_1.throwError(friendlyError);
                }), operators_1.takeUntil(cancellationToken$));
                return obs$;
            },
            "import": function (args, files) {
                args = args || {};
                var paramsArray = [];
                if (!!args.mode) {
                    paramsArray.push("mode=" + args.mode);
                }
                var formData = new FormData();
                for (var _i = 0, files_1 = files; _i < files_1.length; _i++) {
                    var file = files_1[_i];
                    formData.append(file.name, file);
                }
                _this.saveInProgress = true;
                var params = paramsArray.join('&');
                var url = appconfig.apiAddress + ("api/" + endpoint + "/import?" + params);
                var obs$ = _this.http.post(url, formData).pipe(operators_1.tap(function () { return _this.saveInProgress = false; }), operators_1.catchError(function (error) {
                    _this.saveInProgress = false;
                    var friendlyError = _this.friendly(error);
                    return rxjs_1.throwError(friendlyError);
                }), operators_1.takeUntil(cancellationToken$), operators_1.finalize(function () { return _this.saveInProgress = false; }));
                return obs$;
            },
            "export": function (args) {
                var paramsArray = _this.stringifyGetArguments(args);
                if (!!args.format) {
                    paramsArray.push("format=" + args.format);
                }
                var params = paramsArray.join('&');
                var url = appconfig.apiAddress + ("api/" + endpoint + "/export?" + params);
                var obs$ = _this.http.get(url, { responseType: 'blob' }).pipe(operators_1.catchError(function (error) {
                    var friendlyError = _this.friendly(error);
                    return rxjs_1.throwError(friendlyError);
                }), operators_1.takeUntil(cancellationToken$));
                return obs$;
            }
        };
    };
    ApiService.prototype.activateFactory = function (endpoint, cancellationToken$) {
        var _this = this;
        return function (ids, args) {
            args = args || {};
            var paramsArray = [];
            if (!!args.ReturnEntities) {
                paramsArray.push("returnEntities=" + args.ReturnEntities);
            }
            if (!!args.Expand) {
                paramsArray.push("expand=" + args.Expand);
            }
            var params = paramsArray.join('&');
            var url = appconfig.apiAddress + ("api/" + endpoint + "/activate?" + params);
            _this.saveInProgress = true;
            var obs$ = _this.http.put(url, ids, {
                headers: new http_1.HttpHeaders({ 'Content-Type': 'application/json' })
            }).pipe(operators_1.tap(function () { return _this.saveInProgress = false; }), operators_1.catchError(function (error) {
                _this.saveInProgress = false;
                var friendlyError = _this.friendly(error);
                return rxjs_1.throwError(friendlyError);
            }), operators_1.takeUntil(cancellationToken$), operators_1.finalize(function () { return _this.saveInProgress = false; }));
            return obs$;
        };
    };
    ApiService.prototype.deactivateFactory = function (endpoint, cancellationToken$) {
        var _this = this;
        return function (ids, args) {
            args = args || {};
            var paramsArray = [];
            if (!!args.ReturnEntities) {
                paramsArray.push("returnEntities=" + args.ReturnEntities);
            }
            if (!!args.Expand) {
                paramsArray.push("expand=" + args.Expand);
            }
            var params = paramsArray.join('&');
            var url = appconfig.apiAddress + ("api/" + endpoint + "/deactivate?" + params);
            _this.saveInProgress = true;
            var obs$ = _this.http.put(url, ids, {
                headers: new http_1.HttpHeaders({ 'Content-Type': 'application/json' })
            }).pipe(operators_1.tap(function () { return _this.saveInProgress = false; }), operators_1.catchError(function (error) {
                _this.saveInProgress = false;
                var friendlyError = _this.friendly(error);
                return rxjs_1.throwError(friendlyError);
            }), operators_1.takeUntil(cancellationToken$), operators_1.finalize(function () { return _this.saveInProgress = false; }));
            return obs$;
        };
    };
    ApiService.prototype.stringifyGetArguments = function (args) {
        args = args || {};
        var top = args.top || 50;
        var skip = args.skip || 0;
        var paramsArray = [
            "top=" + top,
            "skip=" + skip
        ];
        if (!!args.search) {
            paramsArray.push("search=" + encodeURIComponent(args.search));
        }
        if (!!args.orderBy) {
            paramsArray.push("orderBy=" + args.orderBy);
            paramsArray.push("desc=" + !!args.desc);
        }
        if (!!args.inactive) {
            paramsArray.push("inactive=" + args.inactive);
        }
        if (!!args.filter) {
            paramsArray.push("filter=" + encodeURIComponent(args.filter));
        }
        if (!!args.expand) {
            paramsArray.push("expand=" + encodeURIComponent(args.expand) + ")");
        }
        return paramsArray;
    };
    // Function to turn status codes into friendly localized human-readable errors
    ApiService.prototype.friendly = function (error) {
        var friendlyStructure = function (status, err) {
            return {
                status: status,
                error: err
            };
        };
        // Translates HttpClient's errors into human-friendly errors
        if (error instanceof http_1.HttpErrorResponse) {
            var res = error;
            switch (res.status) {
                case 0: // Offline
                    return friendlyStructure(res.status, this.translate.instant("Error_UnableToReachServer"));
                case 400: // Bad Request
                case 422: // Unprocessible entity
                    if (error.error instanceof Blob) {
                        // TODO: Need a better solution to handle blobs
                        return friendlyStructure(res.status, this.translate.instant("Error_UnkownClientError"));
                    }
                    else {
                        // These two status codes mean a friendly error is already coming from the server
                        return friendlyStructure(res.status, res.error);
                    }
                case 401: // Unauthorized
                    return friendlyStructure(res.status, this.translate.instant("Error_LoginSessionExpired"));
                case 403: // Forbidden
                    return friendlyStructure(res.status, this.translate.instant("Error_AccountDoesNotHaveSufficientPermissions"));
                case 404: // Not found
                    return friendlyStructure(res.status, this.translate.instant("Error_RecordNotFound"));
                case 500: // Internal Server Error
                    return friendlyStructure(res.status, this.translate.instant("Error_UnhandledServerError"));
                default: // Any other HTTP error
                    return friendlyStructure(res.status, this.translate.instant("Error_UnkownServerError"));
            }
        }
        else {
            console.error(error);
            return friendlyStructure(null, this.translate.instant("Error_UnkownClientError"));
        }
    };
    ApiService = __decorate([
        core_1.Injectable({
            providedIn: 'root'
        })
    ], ApiService);
    return ApiService;
}());
exports.ApiService = ApiService;
