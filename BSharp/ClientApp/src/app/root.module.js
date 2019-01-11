"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
exports.__esModule = true;
var core_1 = require("@angular/core");
var platform_browser_1 = require("@angular/platform-browser");
var root_routing_module_1 = require("./root-routing.module");
var root_component_1 = require("./root.component");
var companies_component_1 = require("./features/companies/companies.component");
var page_not_found_component_1 = require("./features/page-not-found/page-not-found.component");
var angular_fontawesome_1 = require("@fortawesome/angular-fontawesome");
var http_1 = require("@angular/common/http");
var core_2 = require("@ngx-translate/core");
var api_translate_loader_1 = require("./data/api-translate-loader");
var RootModule = /** @class */ (function () {
    function RootModule() {
    }
    RootModule = __decorate([
        core_1.NgModule({
            declarations: [
                root_component_1.RootComponent,
                companies_component_1.CompaniesComponent,
                page_not_found_component_1.PageNotFoundComponent
            ],
            imports: [
                platform_browser_1.BrowserModule,
                angular_fontawesome_1.FontAwesomeModule,
                root_routing_module_1.RootRoutingModule,
                http_1.HttpClientModule,
                core_2.TranslateModule.forRoot({
                    loader: {
                        provide: core_2.TranslateLoader,
                        useFactory: api_translate_loader_1.ApiTranslateLoaderFactory,
                        deps: [http_1.HttpClient]
                    }
                })
            ],
            providers: [
            // {
            //   provide: HTTP_INTERCEPTORS,
            //   useClass: RootHttpInterceptor,
            //   deps: [WorkspaceService],
            //   multi: true
            // }
            ],
            bootstrap: [root_component_1.RootComponent]
        })
    ], RootModule);
    return RootModule;
}());
exports.RootModule = RootModule;
