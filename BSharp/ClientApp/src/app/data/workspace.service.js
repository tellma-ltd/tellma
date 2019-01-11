"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
exports.__esModule = true;
var core_1 = require("@angular/core");
var MasterStatus;
(function (MasterStatus) {
    // The master data is currently being fetched from the server
    MasterStatus[MasterStatus["loading"] = 1] = "loading";
    // The last fetch of data from the server completed successfully
    MasterStatus[MasterStatus["loaded"] = 2] = "loaded";
    // The last fetch of data from the server completed with an error
    MasterStatus[MasterStatus["error"] = 3] = "error";
})(MasterStatus = exports.MasterStatus || (exports.MasterStatus = {}));
var DetailsStatus;
(function (DetailsStatus) {
    // The details record is being fetched from the server
    DetailsStatus[DetailsStatus["loading"] = 1] = "loading";
    // The last fetch of the details record from the server completed successfully
    DetailsStatus[DetailsStatus["loaded"] = 2] = "loaded";
    // The last fetch of details record from the server resulted in an error
    DetailsStatus[DetailsStatus["error"] = 3] = "error";
    // The details record is set to be modified or is currently being modified
    DetailsStatus[DetailsStatus["edit"] = 4] = "edit";
})(DetailsStatus = exports.DetailsStatus || (exports.DetailsStatus = {}));
// Represents a collection of savable entities, indexed by their IDs
var EntityWorkspace = /** @class */ (function () {
    function EntityWorkspace() {
    }
    return EntityWorkspace;
}());
exports.EntityWorkspace = EntityWorkspace;
// This contains all the state that is specific to a particular tenant
var TenantWorkspace = /** @class */ (function () {
    function TenantWorkspace() {
        this.reset();
    }
    TenantWorkspace.prototype.reset = function () {
        this.mdState = {};
        this.MeasurementUnits = new EntityWorkspace();
    };
    return TenantWorkspace;
}());
exports.TenantWorkspace = TenantWorkspace;
// This contains the application state during a particular user session
var Workspace = /** @class */ (function () {
    function Workspace() {
        this.isRtl = false;
        this.tenants = {};
    }
    return Workspace;
}());
exports.Workspace = Workspace;
var MasterDetailsStore = /** @class */ (function () {
    function MasterDetailsStore() {
        this.top = 50; // +
        this.skip = 0;
        this.total = 0;
        this.inactive = false;
        this.filterState = {};
        this.masterIds = [];
    }
    MasterDetailsStore.prototype["delete"] = function (ids) {
        // removes a deleted item in memory and updates the stats
        this.total = Math.max(this.total - ids.length, 0);
        this.masterIds = this.masterIds.filter(function (e) { return ids.indexOf(e) === -1; });
    };
    MasterDetailsStore.prototype.insert = function (ids) {
        // adds a newly created item in memory and updates the stats
        this.total = this.total + ids.length;
        this.masterIds = ids.concat(this.masterIds);
    };
    return MasterDetailsStore;
}());
exports.MasterDetailsStore = MasterDetailsStore;
// The Workspace of the application stores ALL application wide in-memory state that survives
// navigation between screens(But does not survive a tab refresh) having all the state in one
// place is important for security, as it makes it easy to clear the state upon signing out
var WorkspaceService = /** @class */ (function () {
    function WorkspaceService() {
        this.reset();
    }
    Object.defineProperty(WorkspaceService.prototype, "current", {
        // Syntactic sugar for current tenant workspace
        get: function () {
            if (!this.ws.tenants[this.ws.tenantId]) {
                this.ws.tenants[this.ws.tenantId] = new TenantWorkspace();
            }
            return this.ws.tenants[this.ws.tenantId];
        },
        enumerable: true,
        configurable: true
    });
    // Wipes the application state clean, usually upon signing out
    WorkspaceService.prototype.reset = function () {
        this.ws = new Workspace();
    };
    WorkspaceService = __decorate([
        core_1.Injectable({
            providedIn: 'root'
        })
    ], WorkspaceService);
    return WorkspaceService;
}());
exports.WorkspaceService = WorkspaceService;
