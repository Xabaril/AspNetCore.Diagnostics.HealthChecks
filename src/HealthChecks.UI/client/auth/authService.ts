import { UserManager, Log, User, UserManagerSettings } from "oidc-client";

export class AuthService {
  private userManager: UserManager;
  private accessToken: string;
  constructor(private userSettings: UserManagerSettings) {
    Log.logger = console;
    Log.level = Log.DEBUG;
    this.userManager = new UserManager(userSettings);
    this.bindEvents();
  }
  public signInRedirect() {
    this.userManager.signinRedirect();
  }

  public signInRedirectCallback(url: string) {
    this.userManager.signinRedirectCallback(url);
  }

  public getAccessToken() {
    return this.accessToken;
  }

  public async isAuthenticated(): Promise<boolean> {
    const user = await this.userManager.getUser();
    return !!user && !user.expired;
  }

  public async getUser(): Promise<User | null> {
    let user = await this.userManager.getUser();
    if (!user) {
      console.log("Executing signin silent...");
      user = await this.signinSilent();
    }
    if (user) this.accessToken = user.access_token;
    return user;
  }

  public async signinSilent(): Promise<User> {
    return this.userManager.signinSilent();
  }

  public logOut() {
    this.userManager.signoutRedirect();
  }

  private bindEvents() {
    this.userManager.events.addAccessTokenExpired((e) =>
      console.log("The access token expired")
    );
    this.userManager.events.addSilentRenewError((e) =>
      console.log("There was an error in the silent renew process", e)
    );
    this.userManager.events.addUserLoaded((u) => {
      this.accessToken = u?.access_token;
    });
  }
}
