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

export type Action = 'Read' | 'Update' | 'Delete' | 'IsActive' | 'ResendInvitationEmail' | 'All';

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
    'All': 'View_All',
  };

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
    'agents': {
        name: 'Agents',
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

    'settings': {
        name: 'Settings',
        actions: [
            li('Read', false),
            li('Update', false)
        ]
    },
};
