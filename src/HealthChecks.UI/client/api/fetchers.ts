import { Liveness, UIApiSettings, WebHook } from "../typings/models";
import uiSettings from "../config/UISettings";

export const getHealthChecks = async (): Promise<Liveness[]> => {
  const healthchecksData = await fetch(uiSettings.uiApiEndpoint);
  return healthchecksData.json();
};

export const getUIApiSettings = async (): Promise<UIApiSettings> => {
  const uiApiSettings = await fetch(uiSettings.uiSettingsEndpoint);
  return uiApiSettings.json();
};

export const getWebhooks = async (): Promise<WebHook[]> => {
  const webhooks = await fetch(uiSettings.webhookEndpoint);
  return webhooks.json();
};

export default {
  getHealthChecks,
  getUIApiSettings,
  getWebhooks,
};
