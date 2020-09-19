import React from 'react';
import { UserManager } from 'oidc-client';
export const AuthCallback = () => {

    const userManager = new UserManager({
        response_mode: 'query'
    });

    const redirectUrl = `${window.location.origin}${window.location.pathname}`;

    userManager.signinRedirectCallback().then(() =>  {
        window.location.replace(redirectUrl);
    });
    return (<div>Logging in...</div>)
};