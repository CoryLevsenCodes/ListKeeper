export interface User {
    id: number;
    email: string;
    password?: string;
    role?: string;
    username?: string;
    firstname?: string;
    lastname?: string;
    phone?: string;
    createdAt?: Date;
    createdBy?: string;
    updatedAt?: Date;
    updatedBy?: string;
    deletedAt?: Date;
    deletedBy?: string;
    token?: string;
}
