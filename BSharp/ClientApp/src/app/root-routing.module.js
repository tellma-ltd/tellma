"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
exports.__esModule = true;
var core_1 = require("@angular/core");
var router_1 = require("@angular/router");
var page_not_found_component_1 = require("./features/page-not-found/page-not-found.component");
var companies_component_1 = require("./features/companies/companies.component");
var routes = [
    { path: 'companies', component: companies_component_1.CompaniesComponent },
    // Lazy loaded modules,
    {
        path: 'app',
        loadChildren: './features/application.module#ApplicationModule',
        data: { preload: true }
    },
    {
        path: 'admin',
        loadChildren: './features/admin.module#AdminModule',
        data: { preload: false }
    },
    {
        path: 'identity',
        loadChildren: './features/identity.module#IdentityModule',
        data: { preload: true }
    },
    {
        path: 'welcome',
        loadChildren: './features/landing.module#LandingModule',
        data: { preload: false }
    },
    { path: '', redirectTo: 'companies', pathMatch: 'full' },
    { path: '**', component: page_not_found_component_1.PageNotFoundComponent }
];
var RootRoutingModule = /** @class */ (function () {
    function RootRoutingModule() {
    }
    RootRoutingModule = __decorate([
        core_1.NgModule({
            imports: [router_1.RouterModule.forRoot(routes, { preloadingStrategy: router_1.PreloadAllModules })],
            exports: [router_1.RouterModule]
        })
    ], RootRoutingModule);
    return RootRoutingModule;
}());
exports.RootRoutingModule = RootRoutingModule;
