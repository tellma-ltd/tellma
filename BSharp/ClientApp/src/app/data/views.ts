export interface ViewInfo {
    name: string;
    read?: boolean;
    update?: boolean;
    delete?: boolean;
    actions: ActionInfo[];
}

export interface ActionInfo {
    action: Action;
    criteria: boolean;
}

export type Action = 'Read' | 'Update' | 'Delete' | 'IsActive' | 'IsDeprecated' | 'ResendInvitationEmail' | 'UpdateState' | 'All';

function li(name: Action, criteria = true) {
    return { action: name, criteria };
}

// tslint:disable:object-literal-key-quotes
export const ACTIONS: { [action: string]: string } = {
    'Read': 'Permission_Read',
    'Update': 'Permission_Update',
    'Delete': 'Permission_Delete',
    'IsActive': 'Permission_IsActive',
    'IsDeprecated': 'Permission_IsDeprecated',
    'ResendInvitationEmail': 'ResendInvitationEmail',
    'All': 'View_All',
};


// IMPORTANT: This mimmicks another C# structure on the server, it is important
// to keep them in sync
export const VIEWS_BUILT_IN: { [viewId: string]: ViewInfo } = {
    'all': {
        name: 'View_All',
        actions: [
            li('Read', false)
        ]
    },
    'measurement-units': {
        name: 'MeasurementUnits',
        read: true,
        update: true,
        delete: true,
        actions: [
            li('IsActive')
        ]
    },
    'roles': {
        name: 'Roles',
        read: true,
        update: true,
        delete: true,
        actions: [
            li('IsActive')
        ]
    },
    'users': {
        name: 'Users',
        read: true,
        update: true,
        delete: true,
        actions: [
            li('ResendInvitationEmail')
        ]
    },
    'ifrs-notes': {
        name: 'IfrsNotes',
        read: true,
        actions: [
            li('IsActive')
        ]
    },
    'currencies': {
        name: 'Currencies',
        read: true,
        update: true,
        delete: true,
        actions: [
            li('IsActive')
        ]
    },
    'account-classifications': {
        name: 'AccountClassifications',
        read: true,
        update: true,
        delete: true,
        actions: [
            li('IsDeprecated')
        ]
    },
    'accounts': {
        name: 'Accounts',
        read: true,
        update: true,
        delete: true,
        actions: [
            li('IsDeprecated')
        ]
    },
    'account-types': {
        name: 'AccountTypes',
        actions: [
            li('Read', false),
            li('IsActive', false)
        ]
    },
    'report-definitions': {
        name: 'ReportDefinitions',
        read: true,
        update: true,
        delete: true,
        actions: [
            li('UpdateState', false)
        ]
    },
    'responsibility-centers': {
        name: 'ResponsibilityCenters',
        read: true,
        update: true,
        delete: true,
        actions: [
            li('IsActive', false)
        ]
    },
    'resource-classifications': {
        name: 'ResourceClassifications',
        read: true,
        update: true,
        delete: true,
        actions: [
            li('IsActive', false)
        ]
    },

    'settings': {
        name: 'Settings',
        actions: [
            li('Read', false),
            li('Update', false)
        ]
    },
};
