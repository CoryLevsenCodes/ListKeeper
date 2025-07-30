export interface User {
    id: number;
    email: string;
    password?: string;
    role?: string;
    username?: string;
    firstname?: string;
    lastname?: string;
    phone?: string;
    isMfaEnabled?: boolean;
    mfaSecretKey?: string;
    mfaBackupCodes?: string;
    mfaSetupDate?: Date;
    createdAt?: Date;
    createdBy?: string;
    updatedAt?: Date;
    updatedBy?: string;
    deletedAt?: Date;
    deletedBy?: string;
    token?: string;
}
