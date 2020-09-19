import { User, UserManager } from 'oidc-client';
import create from 'zustand';
import  { AuthService } from '../auth/authService';

export type UserStore = {
    authService: AuthService | null,
    setAuthService: (authService: AuthService) => void,
    user: User | null,
    getAccessToken: () => string | undefined,    
    setUser: (user: User) => void;
}

export const useUserStore = create<UserStore>((set, get) => ({
    authService: null,
    setAuthService: (authService: AuthService) =>
        set(() => ({ authService })),
    getAccessToken: () => get().authService!.getAccessToken(),
    user: null,    
    setUser: (user: User) =>
        set(() => ({ user }))
}));