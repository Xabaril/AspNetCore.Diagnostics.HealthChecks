import React, { FunctionComponent, useEffect, useState } from 'react';
import { NavLink } from 'react-router-dom';
import { AsideMenu } from './components/AsideMenu';
import WhiteGearIcon from '../assets/svg/white-gear.svg';
import WhiteHeartIcon from '../assets/svg/heart-check.svg';
import { UISettings } from './config/UISettings';
import Routes from './routes/Routes';
import { ProtectedRoute } from './auth/components/ProtectedRoute';
import { useUserStore } from "./stores/userStore";
import fetchers from './api/fetchers';
import { AlertPanel } from './components/AlertPanel';
import { useQuery } from 'react-query';
import { AuthService } from './auth/authService';
import { buildAuthConfig } from './auth/authConfig';

interface AppProps {
  uiSettings: UISettings;
}

const App: FunctionComponent<AppProps> = ({ uiSettings }) => {

  const [asidemenuOpened, setAsideMenu] = useState<boolean>(uiSettings.asideMenuOpened);
  const { data: apiSettings, isError } = useQuery("uiApiSettings", fetchers.getUIApiSettings, { retry: 1 });
  const [authService, setAuthService] = useUserStore(state => [state.authService, state.setAuthService])
  const [oidcEnabled, setOidcEnabled] = useState<boolean | null>(null);

  useEffect(() => {
    if (apiSettings) {
      if (apiSettings.oidcOptions) {
        setAuthService(new AuthService(buildAuthConfig(apiSettings.oidcOptions)));
        setOidcEnabled(true);
      }
      else {
        setOidcEnabled(false);
      }
    }

  }, [apiSettings]);

  const toggleMenu = () => {
    setAsideMenu(!asidemenuOpened);
  };

  if (isError) {
    return <AlertPanel message="Error retrivieng UI api settings from endpoint" />
  }

  if (apiSettings == undefined || oidcEnabled == null) return null;

  return (
    <main id="outer-container">
      <AsideMenu
        isOpen={asidemenuOpened}
        onClick={() => toggleMenu()}>
        <NavLink
          to="/healthchecks"
          className="hc-aside-menu__item"
          activeClassName="hc-aside-menu__item--active"
        >
          <img alt="icon heart check" className="hc-menu-icon" src={WhiteHeartIcon} />
          <span>Health Checks</span>
        </NavLink>
        <NavLink
          to="/webhooks"
          className="hc-aside-menu__item"
          activeClassName="hc-aside-menu__item--active"
        >
          <img alt="icon gear" className="hc-menu-icon" src={WhiteGearIcon} />
          <span>Webhooks</span>
        </NavLink>
      </AsideMenu>
      <section className="hc-section-router">
        {oidcEnabled && authService ?
          <ProtectedRoute>
            <Routes apiSettings={apiSettings} />
          </ProtectedRoute>
          :
          <Routes apiSettings={apiSettings} />
        }

      </section>
    </main>
  );
}

export { App };