import { User } from "oidc-client";
import React, { createContext, FunctionComponent } from "react";

export const UserContext = createContext<User | null>(null);

export interface IUserProviderProps {
    user: User
}

export const UserProvider : FunctionComponent<IUserProviderProps> = ({user, children}) => {
    return <UserContext.Provider value={user}>
        {children}
    </UserContext.Provider>
};