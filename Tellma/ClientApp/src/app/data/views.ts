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

export type Action = 'Read' | 'Update' | 'Delete' | 'IsActive' |
    'ResendInvitationEmail' | 'ResetPassword' | 'State' | 'All';

function li(name: Action, criteria = true) {
    return { action: name, criteria };
}

// tslint:disable:object-literal-key-quotes
export const ACTIONS: { [action: string]: string } = {
    'Read': 'Permission_Read',
    'Update': 'Permission_Update',
    'Delete': 'Permission_Delete',
    'IsActive': 'Permission_IsActive',
    'ResendInvitationEmail': 'ResendInvitationEmail',
    'State': 'Permission_State',
    'ResetPassword': 'ResetPassword',
    'All': 'View_All',
};

export const ADMIN_VIEWS_BUILT_IN: { [view: string]: ViewInfo } = {
    'all': {
        name: 'View_All',
        actions: [
            li('Read', false)
        ]
    },
    'admin-users': {
        name: 'AdminUsers',
        read: true,
        update: true,
        delete: true,
        actions: [
            li('IsActive'),
            li('ResendInvitationEmail')
        ]
    },
    'identity-server-users': {
        name: 'IdentityServerUsers',
        read: true,
        actions: [
            li('ResetPassword'),
        ]
    }
};

// IMPORTANT: This mimmicks another C# structure on the server, it is important
// to keep them in sync
export const APPLICATION_VIEWS_BUILT_IN: { [view: string]: ViewInfo } = {
    'all': {
        name: 'View_All',
        actions: [
            li('Read', false)
        ]
    },
    'units': {
        name: 'Units',
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
            li('IsActive'),
            li('ResendInvitationEmail')
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
            li('IsActive')
        ]
    },
    'accounts': {
        name: 'Accounts',
        read: true,
        update: true,
        delete: true,
        actions: [
            li('IsActive')
        ]
    },
    'ifrs-concepts': {
        name: 'IfrsConcepts',
        actions: [
            li('Read', false)
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
        actions: []
    },
    'dashboard-definitions': {
        name: 'DashboardDefinitions',
        read: true,
        update: true,
        delete: true,
        actions: []
    },
    'centers': {
        name: 'Centers',
        read: true,
        update: true,
        delete: true,
        actions: [
            li('IsActive', false)
        ]
    },
    'entry-types': {
        name: 'EntryTypes',
        read: true,
        update: true,
        delete: true,
        actions: [
            li('IsActive', false)
        ]
    },
    'exchange-rates': {
        name: 'ExchangeRates',
        read: true,
        update: true,
        delete: true,
        actions: []
    },
    'details-entries': {
        name: 'DetailsEntries',
        read: true,
        actions: []
    },
    'markup-templates': {
        name: 'MarkupTemplates',
        read: true,
        update: true,
        delete: true,
        actions: []
    },
    'agents': {
        name: 'Agents',
        read: true,
        update: true,
        delete: true,
        actions: [
            li('IsActive', false)
        ]
    },
    'relation-definitions': {
        name: 'RelationDefinitions',
        read: true,
        update: true,
        delete: true,
        actions: [
            li('State')
        ]
    },
    'resource-definitions': {
        name: 'ResourceDefinitions',
        read: true,
        update: true,
        delete: true,
        actions: [
            li('State')
        ]
    },
    'lookup-definitions': {
        name: 'LookupDefinitions',
        read: true,
        update: true,
        delete: true,
        actions: [
            li('State')
        ]
    },
    'line-definitions': {
        name: 'LineDefinitions',
        read: true,
        update: true,
        delete: true,
        actions: [ ]
    },
    'document-definitions': {
        name: 'DocumentDefinitions',
        read: true,
        update: true,
        delete: true,
        actions: [
            li('State')
        ]
    },
    'general-settings': {
        name: 'GeneralSettings',
        actions: [
            li('Read', false),
            li('Update', false)
        ]
    },
    'financial-settings': {
        name: 'FinancialSettings',
        actions: [
            li('Read', false),
            li('Update', false)
        ]
    },
    'reconciliation': {
        name: 'BankReconciliation',
        actions: [
            li('Read', false),
            li('Update', false)
        ]
    },
    'emails': {
        name: 'Emails',
        read: true,
        actions: []
    },
    'sms-messages': {
        name: 'SmsMessages',
        read: true,
        actions: []
    }
};
