import React, { FunctionComponent, useState } from 'react';
import { Route, Redirect, NavLink } from 'react-router-dom';
import { LivenessPage } from './pages/LivenessPage';
import { WebhooksPage } from './pages/WebhooksPage';
import { AsideMenu } from './components/AsideMenu';
import WhiteGearIcon from '../assets/svg/white-gear.svg';
import WhiteHeartIcon from '../assets/svg/heart-check.svg';
import { UISettings } from './config/UISettings';
import { isError, useQuery } from 'react-query';
import fetchers from './api/fetchers';
import { AlertPanel } from './components/AlertPanel';

interface AppProps {
  uiSettings: UISettings;
}

interface AppState {
  menuOpen: boolean;
}

const App: FunctionComponent<AppProps> = ({ uiSettings }) => {

  const [asidemenuOpened, setAsideMenu] = useState<boolean>(uiSettings.asideMenuOpened);
  const { data: apiSettings, isError } = useQuery("uiApiSettings", fetchers.getUIApiSettings, { retry: 1 });

  const toggleMenu = () => {
    setAsideMenu(!asidemenuOpened);
  };


  if (isError) {
    return <AlertPanel message="Error retrivieng UI api settings from endpoint" />    
  }

  if (apiSettings == undefined) return null;

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
        <Route
          exact
          path="/"
          render={() => <Redirect to="/healthchecks" />}
        />
        <Route
          path="/healthchecks"
          render={() => <LivenessPage apiSettings={apiSettings} />}
        />
        <Route
          path="/webhooks"
          render={() => (
            <WebhooksPage />
          )}
        />
      </section>
    </main>
  );
}

export { App };