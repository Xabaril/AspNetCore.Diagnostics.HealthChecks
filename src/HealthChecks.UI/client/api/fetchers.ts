import { Liveness, UIApiSettings, WebHook } from "../typings/models"
import uiSettings from '../config/UISettings';
import { AuthService } from "../auth/authService";
import { userStoreState } from "../stores/userStore";


export const getHealthChecks = async (): Promise<Liveness[]> => {
    const healthchecksData = await fetch(uiSettings.uiApiEndpoint, getAuthorizedHeaders());
    return healthchecksData.json();
}

export const getUIApiSettings = async (): Promise<UIApiSettings> => {

    const uiApiSettings = await fetch(uiSettings.uiSettingsEndpoint);
    return uiApiSettings.json();
}

export const getWebhooks = async (): Promise<WebHook[]> => {
    const webhooks = await fetch(uiSettings.webhookEndpoint, getAuthorizedHeaders());
    return webhooks.json();
}

const getAuthorizedHeaders = (): RequestInit | undefined => {

    const authService = userStoreState().authService;

    if (authService == null) return undefined;

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