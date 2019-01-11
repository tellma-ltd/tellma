"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
exports.__esModule = true;
var core_1 = require("@angular/core");
var RootComponent = /** @class */ (function () {
    function RootComponent(translate, workspace, api, storage) {
        var _this = this;
        this.translate = translate;
        this.workspace = workspace;
        this.api = api;
        this.storage = storage;
        this.QUERY_PARAM_NAME = 'ui-culture';
        // If the selected langauge is any of the below
        // the entire application is swapped to RTL layout
        this.rtlLanguages = [
            'ae',
            'ar',
            'arc',
            'bcc',
            'bqi',
            'ckb',
            'dv',
            'fa',
            'glk',
            'he',
            'ku',
            'mzn',
            'nqo',
            'pnb',
            'ps',
            'sd',
            'ug',
            'ur',
            'yi' /* 'ייִדיש', Yiddish */
        ];
        // Callback after the new app culture is loaded
        this.translate.onLangChange.subscribe(function (_) {
            // After ngx-translate successfully loads the language
            // we set it in the workspace so that all our components
            // reflect the change too
            var culture = _this.translate.currentLang;
            _this.setDocumentRTL(culture);
            if (!!document) {
                // TODO Load from configuration instead
                document.title = _this.translate.instant('AppName');
            }
            // TODO Set in local storage properly
            _this.storage.setItem('userCulture', culture);
        });
        // TODO load from app configuration
        // Fallback culture
        var defaultCulture = 'en';
        this.translate.setDefaultLang(defaultCulture);
        // TODO load from local storage properly
        var userCulture = this.storage.getItem('userCulture');
        if (!!userCulture) {
            this.translate.use(userCulture);
        }
    }
    RootComponent.prototype.setDocumentRTL = function (culture) {
        this.workspace.ws.culture = culture;
        var isRtl = this.rtlLanguages.some(function (e) { return culture.startsWith(e); });
        this.workspace.ws.isRtl = isRtl;
        if (isRtl && !!document) {
            document.body.classList.add('b-rtl');
        }
        else {
            document.body.classList.remove('b-rtl');
        }
    };
    Object.defineProperty(RootComponent.prototype, "showOverlay", {
        get: function () {
            // when there is a save in progress, block the user screen and prevent any navigation.
            return this.api.saveInProgress;
        },
        enumerable: true,
        configurable: true
    });
    RootComponent = __decorate([
        core_1.Component({
            selector: 'b-root',
            templateUrl: './root.component.html',
            styles: []
        })
    ], RootComponent);
    return RootComponent;
}());
exports.RootComponent = RootComponent;
