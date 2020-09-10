import { Liveness, UIApiSettings, WebHook } from "../typings/models"
import uiSettings from '../config/UISettings';

export const getHealthChecks = async () : Promise<Liveness[]> => {
    const healthchecksData = await (await fetch(uiSettings.uiApiEndpoint)).json()
    return healthchecksData;
}

export const getUIApiSettings = async() : Promise<UIApiSettings> => {
    const uiApiSettings = await (await fetch(uiSettings.uiSettingsEndpoint)).json()    
    return uiApiSettings;
}

export const getWebhooks = async () : Promise<WebHook[]> => {
    const webhooks = await (await fetch(uiSettings.webhookEndpoint)).json()
    return webhooks;
}

export default {
    getHealthChecks,
    getUIApiSettings,
    getWebhooks
};