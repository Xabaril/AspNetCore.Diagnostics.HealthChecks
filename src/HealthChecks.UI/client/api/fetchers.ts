import { Liveness, UIApiSettings, WebHook } from "../typings/models"
import uiSettings from '../config/UISettings';
import { AuthService } from "../auth/authService";


export const getHealthChecks = async (authService: AuthService): Promise<Liveness[]> => {

    const healthchecksData = await fetch(uiSettings.uiApiEndpoint, getAuthorizedHeaders(authService));
    return healthchecksData.json();
}

export const getUIApiSettings = async (): Promise<UIApiSettings> => {

    const uiApiSettings = await fetch(uiSettings.uiSettingsEndpoint);
    return uiApiSettings.json();
}

export const getWebhooks = async (authService: AuthService | null): Promise<WebHook[]> => {
    const webhooks = await fetch(uiSettings.webhookEndpoint, getAuthorizedHeaders(authService));
    return webhooks.json();
}

const getAuthorizedHeaders = (authService: AuthService | null): RequestInit | undefined => {

    if (!authService) return undefined;

    const token = authService.getAccessToken();

    if (token) {
        console.log(`Using at: ${token}`);
        return {
            headers: {
                "Authorization": `Bearer ${token}`
            }
        }
    }
    
    return undefined;
}


export default {
    getHealthChecks,
    getUIApiSettings,
    getWebhooks
};