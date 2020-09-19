import React from 'react';
import { UserManager } from 'oidc-client';

export const SilentRenewCallback = () => {    
    
    const userManager = new UserManager({});
    userManager.signinSilentCallback();
    
    return null;
};