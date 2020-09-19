import { User } from 'oidc-client';
import React, { ComponentType, FunctionComponent, PropsWithChildren, useEffect } from 'react';
import { useUserStore } from '../../stores/userStore';

export const ProtectedRoute: FunctionComponent<PropsWithChildren<any>> = ({ children }) => {

    const [authService,
        setUser,
        user] = useUserStore(state => [state.authService!, state.setUser, state.user]);

    useEffect(() => {
        const login = async () => {
            const loggedIn = await authService.isAuthenticated();
            if (!loggedIn) {
                authService.signInRedirect();
            } else {
                const user = await authService.getUser();                
                setUser(user!);
            }
        }
        login();
    }, []);

    return (
        <>
           {user ? children: <span>Loading...</span>}
        </>
    )
}

export const useProtectedRoute = <T extends object>(WrappedComponent: ComponentType<T>) => (props: PropsWithChildren<T>) => (
    <ProtectedRoute>
        <WrappedComponent {...props} />
    </ProtectedRoute>
);
