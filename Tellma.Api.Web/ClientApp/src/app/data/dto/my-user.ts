export interface MyUserForSave {
    Name?: string;
    Name2?: string;
    Name3?: string;
    PreferredLanguage?: string;
    Image?: string;

    serverErrors?: { [key: string]: string[] };
}
