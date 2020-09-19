import { UserManagerSettings } from "oidc-client";
import { OidcOptions } from "../typings/models";

export const buildAuthConfig = (config: OidcOptions) : UserManagerSettings => {  
  const url = `${window.location.origin}${window.location.pathname}`;
  return  {
    authority: config.authority,
    client_id: config.clientId,
    response_type: config.responseType,
    redirect_uri: `${url}#/callback`,
    silent_redirect_uri: `${url}#/silent-renew-callback`,
    post_logout_redirect_uri: `${url}`,
    scope: config.scope,
    automaticSilentRenew: true,
    accessTokenExpiringNotificationTime: 30,
    loadUserInfo: true,
  }
}

