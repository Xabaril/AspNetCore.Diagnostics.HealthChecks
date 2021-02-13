
export interface UISettings {
    uiApiEndpoint: string;
    uiSettingsEndpoint: string;
    webhookEndpoint: string
    asideMenuOpened: boolean
}

let settings : UISettings = {
    uiApiEndpoint: window.uiEndpoint,
    uiSettingsEndpoint: window.uiSettingsEndpoint,
    webhookEndpoint: window.webhookEndpoint,
    asideMenuOpened: JSON.parse(window.asideMenuOpened)
};

export default settings;