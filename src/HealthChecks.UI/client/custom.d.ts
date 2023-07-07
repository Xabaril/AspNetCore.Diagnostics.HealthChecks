declare module "*.svg" {
  const content: any;
  export default content;
}

declare module "*.png" {
  const value: any;
  export = value;
}

interface Window {
  uiEndpoint: string;
  uiSettingsEndpoint: string;
  webhookEndpoint: string
  asideMenuOpened: string
}
